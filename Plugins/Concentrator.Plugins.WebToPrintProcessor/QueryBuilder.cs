using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Objects.WebToPrint;
using Concentrator.Objects.WebToPrint.Components;
using Concentrator.Objects;
using System.Data.SqlClient;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.WebToPrint;
using Concentrator.Objects.Environments;

namespace Concentrator.Plugins.WebToPrintProcessor
{
  public class QueryBuilder
  {
    /// <summary>
    /// Contains a dictionary of webtoprintbinding ID's and compositecomponents that contain that binding
    /// </summary>
    private Dictionary<int, List<CompositeComponent>> Bindings;

    public QueryBuilder()
    {
      Bindings = new Dictionary<int, List<CompositeComponent>>();
    }

    public void AddComponent(int bindingID, CompositeComponent component)
    {
      if (!Bindings.ContainsKey(bindingID))
      {
        Bindings.Add(bindingID, new List<CompositeComponent>());
      }
      Bindings[bindingID].Add(component);
    }

    /// <summary>
    /// Updates all the objects currently registered with the querybuilder
    /// </summary>
    public void Execute(IUnitOfWork work)
    {

      using (SqlConnection conn = new SqlConnection(Environments.Current.Connection))
      {
        conn.Open();
        foreach (int bindingID in Bindings.Keys)
        {
          if (bindingID > 0)
          {
            // fetch the binding
            WebToPrintBinding wtpb = work.Scope.Repository<WebToPrintBinding>().GetSingle(c => c.BindingID == bindingID);
            if (wtpb == null)
            {
              throw new Exception("Unknown binding attached to component");
            }
            string sql = wtpb.Query;
            SqlCommand comm = new SqlCommand(sql, conn);

            Bindings[bindingID].ForEach(c =>
            {
              foreach (WebToPrintBindingField wtpbf in wtpb.WebToPrintBindingFields.Where(f => f.Type % 2 == 0))
              {
                comm.Parameters.Add(new SqlParameter("@" + wtpbf.FieldID.ToString(), c.Inputs[wtpbf.Name]));
              }
              SqlDataReader reader = comm.ExecuteReader();
              if (reader.Read())
              {
                Dictionary<string, string> bindingValues = new Dictionary<string, string>();
                foreach (var field in wtpb.WebToPrintBindingFields)
                {
                  if (field.Type % 2 == 1)
                  {
                    bindingValues.Add(field.Name, reader.GetSqlValue(reader.GetOrdinal(field.FieldID.ToString())).ToString());
                  }
                  if ((field.Options & (int)BindingFieldOptions.IndexValue) == (int)BindingFieldOptions.IndexValue)
                  {
                      // is databinding source, set all components bound to this output to be used in the indexbuilder
                      foreach (var comp in c.Children)
                      {
                          if (comp.DataBindingSource == field.Name)
                              comp.IsIndexComponent = true;
                      }
                  }
                }
                c.SetData(bindingValues);
              }
              try
              {
                reader.Close();
              }
              catch (Exception e)
              {
                
              }
              comm.Parameters.Clear();
            });
          }

        }
        conn.Close();
      }
    }
  }
}