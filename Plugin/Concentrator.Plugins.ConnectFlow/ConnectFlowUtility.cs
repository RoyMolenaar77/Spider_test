using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace Concentrator.Plugins.ConnectFlow
{
  public class ConnectFlowUtility
  {
    internal static string GetAtricleNumber(string concentratorProductID, Dictionary<string, string> productTranslation, bool ifEmptyReturnConcentratorID)
    {
      if (productTranslation.ContainsKey(concentratorProductID))
        return productTranslation[concentratorProductID];
      else if (ifEmptyReturnConcentratorID)
        return concentratorProductID;
      else
        return string.Empty;
    }

    internal static Dictionary<string, string> GetCrossProducts(System.Xml.Linq.XDocument products)
    {
      Dictionary<string, string> crossDic = new Dictionary<string, string>();
      var concentratorProudcts = (from p in products.Root.Elements("Product")
                                  group p by p.Attribute("CustomProductID").Value into product
                                  select new
                                  {
                                    ConcentratorProductID = product.FirstOrDefault().Attribute("ProductID").Value,
                                    JdeItemNumber = product.Key
                                  }).ToDictionary(x => x.JdeItemNumber, x => x.ConcentratorProductID);

      List<ConnectFlowAdditionalInformation> adiList = new List<ConnectFlowAdditionalInformation>();

      using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Xtract"].ConnectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(@"SELECT 
IVITM as JDE,
IVCITM as SAP
from f4104
where IVXRT = 'DC'", connection))
        {
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              string jdeItem = reader["JDE"].ToString();

              if (concentratorProudcts.ContainsKey(jdeItem))
              {
                var item = new ConnectFlowAdditionalInformation()
                {
                  ConcentratorProductID = concentratorProudcts[jdeItem],
                  JdeNumber = jdeItem,
                  SapNumber = reader["SAP"].ToString()
                };
                adiList.Add(item);
              }
            }
          }
        }
      }

      return (from a in adiList
              group a by a.ConcentratorProductID into cross
              select cross).ToDictionary(x => x.Key, x => x.FirstOrDefault().SapNumber);
      
      return crossDic;
    }

    internal static Dictionary<string, int> GetInsurances(System.Xml.Linq.XDocument products)
    {
      Dictionary<string, int> crossDic = new Dictionary<string, int>();

      List<ConnectFlowAdditionalInformation> adiList = new List<ConnectFlowAdditionalInformation>();

      var concentratorProudcts = (from p in products.Root.Elements("Product")
                                  group p by p.Attribute("CustomProductID").Value into product
                                  select new
                                  {
                                    ConcentratorProductID = product.FirstOrDefault().Attribute("ProductID").Value,
                                    JdeItemNumber = product.Key
                                  }).ToDictionary(x => x.JdeItemNumber, x => x.ConcentratorProductID);



      using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Xtract"].ConnectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(@"select 
imitm,
imprp8
from f4101
where imprp8 != ''", connection))
        {
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {

              string jdeItem = reader["imitm"].ToString();

              if (concentratorProudcts.ContainsKey(jdeItem))
              {
                int ins = 0;
                int.TryParse(reader["imprp8"].ToString(), out ins);

                var item = new ConnectFlowAdditionalInformation()
                {
                  ConcentratorProductID = concentratorProudcts[jdeItem],
                  JdeNumber = jdeItem,
                  Risk = ins
                };
                adiList.Add(item);
              }
            }
          }
        }
      }

      return (from a in adiList
              group a by a.ConcentratorProductID into cross
              select cross).ToDictionary(x => x.Key, x => x.FirstOrDefault().Risk);
    }
  }

  public class ConnectFlowAdditionalInformation
  {
    public string SapNumber { get; set; }
    public string JdeNumber { get; set; }
    public string ConcentratorProductID { get; set; }
    public int Risk { get; set; }
  }
}
