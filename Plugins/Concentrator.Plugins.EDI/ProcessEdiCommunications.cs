using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI;
using System.Configuration;
using Quartz;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.EDI.Mapping;
using System.Data;
using Concentrator.Objects.DataAccess.Repository;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Linq.Expressions;

namespace Concentrator.Plugins.EDI
{
  public class ProcessEdiCommunications : ConcentratorEDIPlugin
  {
    public override string Name
    {
      get { return "Start EDI communication service"; }
    }

    private int ediVendorID;

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        #region Communication
        var comms = (from o in unit.Scope.Repository<EdiCommunication>().GetAll()
                     where !o.LastRun.HasValue || (o.NextRun.HasValue && DateTime.Now >= o.NextRun.Value)
                     select o).ToList();

        foreach (var comm in comms)
        {
          try
          {
            ediVendorID = comm.EdiFieldMappings.FirstOrDefault().EdiVendorID;
            string connection = comm.Connection;

            if (ConfigurationManager.ConnectionStrings[comm.Connection] != null)
              connection = ConfigurationManager.ConnectionStrings[comm.Connection].ConnectionString;

            EDICommunicationLayer eCom = new EDICommunicationLayer((EdiConnectionType)comm.EdiConnectionType, connection);
            var query = SetQueryParams(comm.Query, comm);
            var data = eCom.GetVendorData(query);

            var ediMapping = unit.Scope.Repository<EdiFieldMapping>().GetAll(x => x.EdiCommunicationID == comm.EdiCommunicationID).ToList();
            var matchFields = (from m in ediMapping
                               where m.MatchField
                               group m by m.TableName into tableGroup
                               select tableGroup).ToDictionary(x => x.Key, x => x);
         
            foreach (DataRow row in data.Tables[0].AsEnumerable())
            {
              string tableName = matchFields.FirstOrDefault().Key;
              Type tableType = Type.GetType(tableName);

              var repoMethod = typeof(IScope).GetMethod("Repository").MakeGenericMethod(tableType);

              var repo = repoMethod.Invoke(unit.Scope, null);

              var table = repo.GetType().GetMethod("GetAllAsQueryable").Invoke(repo, new object[1] { null });

              IQueryable result = null;

              var set = (table as IQueryable);
              var mField = matchFields[tableName].FirstOrDefault();

              foreach (var matchField in matchFields[tableName])
              {
                if (!String.IsNullOrEmpty(matchField.VendorFieldName))
                {
                  var value = row[matchField.VendorFieldName];
                  var propertyType = tableType.GetProperty(matchField.FieldName);

                  LambdaExpression expression = null;
                  if (propertyType.PropertyType == typeof(String))
                  {
                    expression = System.Linq.Dynamic.DynamicExpression.ParseLambda(set.ElementType, typeof(bool), string.Format(string.Format(string.Format("({0}) == \"{1}\"", matchField.FieldName, value))));
                  }
                  else
                  {
                    expression = System.Linq.Dynamic.DynamicExpression.ParseLambda(set.ElementType, typeof(bool), string.Format(string.Format("({0}) == \"{1}\"", matchField.FieldName, Convert.ChangeType(value, (TypeCode)matchField.VendorFieldType))));
                  }

                  if (result == null)
                  {
                    result = set.Provider.CreateQuery
                        (
                          Expression.Call(typeof(Queryable),
                                          "Where",
                                          new Type[] { set.ElementType },
                                          set.Expression,
                                          Expression.Quote(expression)));
                  }
                  else
                  {
                    result = result.Provider.CreateQuery
                        (
                          Expression.Call(typeof(Queryable),
                                          "Where",
                                          new Type[] { set.ElementType },
                                          set.Expression,
                                          Expression.Quote(expression)));
                  }
                }

                var count = ((IQueryable<EdiOrderResponse>)result.AsQueryable()).Count();

                if (count == 1)
                {
                  EdiOrderResponse resp = ((IQueryable<EdiOrderResponse>)result.AsQueryable()).FirstOrDefault();

                  var newRecord = GenerateResponse(unit.Scope, resp, data.Tables[0].AsEnumerable().Where(x => Convert.ChangeType(x.Field<object>(mField.VendorFieldName), (TypeCode)mField.VendorFieldType) == row[mField.VendorFieldName]), ediMapping);

                  newRecord.EdiOrderResponseLines.ForEach((x, idx) =>
                  {
                    x.EdiOrderLine.SetStatus(EdiOrderStatus.ReceiveShipmentNotificaiton, unit);
                    x.EdiOrderLine.SetStatus(EdiOrderStatus.WaitForInvoiceNotification, unit);
                  });
                  unit.Save();
                }
              }
            }

            comm.LastRun = DateTime.Now;
            CronExpression exp = new CronExpression(comm.Schedule);
            comm.NextRun = exp.GetTimeAfter(DateTime.UtcNow);
            unit.Save();
          }
          catch (Exception ex)
          {
            comm.Remark = string.Format("Communication rule failed: {0}", ex.Message);
            log.AuditError(string.Format("Communication rule failed (rule: {0})", comm.EdiCommunicationID), ex, "Communication EDI orders");
          }
        }
        #endregion

        try
        {
          ediProcessor.GetCustomOrderResponses(unit, Config);
          unit.Save();
        }
        catch (Exception ex)
        {
          log.AuditCritical("CustomOrderresponse failed processing", ex);
        }
      }
    }

    private EdiOrderResponse GenerateResponse(IScope scope, EdiOrderResponse response, EnumerableRowCollection rows, List<EdiFieldMapping> mapping)
    {
      var grouped = (from m in mapping
                     group m by m.TableName into tableGroup
                     select tableGroup).ToDictionary(x => x.Key, x => x);

      EdiOrderResponse newResponse = new EdiOrderResponse();
      newResponse.EdiOrderID = response.EdiOrderID;
      newResponse.ReceiveDate = DateTime.Now;
      newResponse.ResponseType = (int)Enum.Parse(typeof(OrderResponseTypes), mapping.FirstOrDefault().EdiCommunication.ResponseType);
      Type t = Type.GetType("Concentrator.Objects.Models.EDI.Response.EdiOrderResponse, Concentrator.Objects");
      var ediOrderResponseFields = grouped["Concentrator.Objects.Models.EDI.Response.EdiOrderResponse, Concentrator.Objects"];

      foreach (var m in ediOrderResponseFields)
      {
        if (!string.IsNullOrEmpty(m.VendorFieldName))
        {
          foreach (DataRow row in rows)
          {
            var value = row[m.VendorFieldName];
            t.GetProperty(m.FieldName).SetValue(newResponse, Convert.ChangeType(value, (TypeCode)m.VendorFieldType), null);

            if (newResponse.GetType() == newResponse.GetType())
              break;
          }
        }
        else
        {
          object value = null;
          {

            if (m.VendorDefaultValue == "[Document]")
            {
              List<string> strList = new List<string>();
              foreach (DataRow r in rows)
              {
                strList.Add(string.Join(",", r.ItemArray.ToArray()));
              }
              value = string.Join(",", strList);
            }
            else if (m.VendorDefaultValue == "[VendorID]")
            {
              value = ediVendorID;
            }

            t.GetProperty(m.FieldName).SetValue(newResponse, Convert.ChangeType(value, (TypeCode)m.VendorFieldType), null);
          }
        }
      }
      scope.Repository<EdiOrderResponse>().Add(newResponse);

      foreach (DataRow row in rows)
      {
        EdiOrderResponseLine newResponseLine = new EdiOrderResponseLine();
        newResponseLine.EdiOrderResponse = newResponse;
        var set = response.EdiOrderResponseLines.AsQueryable();
        IQueryable result = null;
        var assambly = "Concentrator.Objects.Models.EDI.Response.EdiOrderResponseLine, Concentrator.Objects";
        t = Type.GetType(assambly);

        foreach (var matchField in grouped[assambly].Where(x => x.MatchField))
        {
          var value = row[matchField.VendorFieldName];
          var propertyType = newResponseLine.GetType().GetProperty(matchField.FieldName);

          LambdaExpression expression = null;
          if (propertyType.PropertyType == typeof(String))
          {
            expression = System.Linq.Dynamic.DynamicExpression.ParseLambda(set.ElementType, typeof(bool), string.Format(string.Format(string.Format("({0}) == \"{1}\"", matchField.FieldName, value))));
          }
          else
          {
            expression = System.Linq.Dynamic.DynamicExpression.ParseLambda(set.ElementType, typeof(bool), string.Format(string.Format("({0}) == \"{1}\"", matchField.FieldName, Convert.ChangeType(value, (TypeCode)matchField.VendorFieldType))));
          }

          if (result == null)
          {
            result = set.Provider.CreateQuery
                (
                  Expression.Call(typeof(Queryable),
                                  "Where",
                                  new Type[] { set.ElementType },
                                  set.Expression,
                                  Expression.Quote(expression)));
          }
          else
          {
            result = result.Provider.CreateQuery
                (
                  Expression.Call(typeof(Queryable),
                                  "Where",
                                  new Type[] { set.ElementType },
                                  set.Expression,
                                  Expression.Quote(expression)));
          }
        }

        var ediOrderline = ((IQueryable<EdiOrderResponseLine>)result.AsQueryable()).FirstOrDefault();

        if (ediOrderline != null)
        {
          newResponseLine.EdiOrderLine = ediOrderline.EdiOrderLine;
        }

        foreach (var m in grouped[assambly])
        {
          if (!string.IsNullOrEmpty(m.VendorFieldName))
          {
            var value = row[m.VendorFieldName];
            t.GetProperty(m.FieldName).SetValue(newResponseLine, Convert.ChangeType(value, (TypeCode)m.VendorFieldType), null);
          }
          else
          {
            object value = null;
            {

              if (m.VendorDefaultValue == "[Document]")
              {
                List<string> strList = new List<string>();
                foreach (DataRow r in rows)
                {
                  strList.Add(string.Join(",", r.ItemArray.Cast<string>().ToArray()));
                }
                value = string.Join(",", strList);
              }
              else if (m.VendorDefaultValue == "[VendorID]")
              {
                value = ediVendorID;
              }

              t.GetProperty(m.FieldName).SetValue(newResponseLine, Convert.ChangeType(value, (TypeCode)m.VendorFieldType), null);
            }
          }
        }


        scope.Repository<EdiOrderResponseLine>().Add(newResponseLine);
      }

      return newResponse;
    }

    public override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

    private string SetQueryParams(string query, EdiCommunication ediCom)
    {
      Dictionary<string, string> queryParams = new Dictionary<string, string>();
      queryParams.Add("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
      if (ediCom.LastRun.HasValue)
        queryParams.Add("@LastRun", ediCom.LastRun.Value.ToString("yyyy-MM-dd"));
      else
        queryParams.Add("@LastRun", DateTime.Now.ToString("yyyy-MM-dd"));

      foreach (var par in queryParams.Keys)
      {
        query = query.Replace(par, "'" + queryParams[par] + "'");
      }

      return query;
    }

  }
}
