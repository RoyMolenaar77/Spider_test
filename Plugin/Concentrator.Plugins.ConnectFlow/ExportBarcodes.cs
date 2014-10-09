using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using Concentrator.Objects.ConcentratorService;
using SAPiDocConnector.Helpers;
using SAPiDocConnector;
using Concentrator.Objects;
using Concentrator.Objects.Models.Connectors;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Globalization;
using System.IO;
using System.Data.SqlClient;
using Concentrator.Objects.Models.Vendors;
using System.Configuration;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.ConnectFlow
{
  [ConnectorSystem(4)]
  public class ExportBarcodes : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "ConnectFlow barcode Export"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.ShopAssortment)))
      {
        using (var unit = GetUnitOfWork())
        {
          var funcRepo = ((IFunctionScope)unit.Scope).Repository();
          var records = funcRepo.GetAssortmentContentView(connector.ConnectorID).Where(x => x.BrandID > 0);

          var connectorBarcodes = (from pb in unit.Scope.Repository<ProductBarcodeView>().GetAll(x => x.ConnectorID == connector.ConnectorID)
                                   join ass in records on pb.ProductID equals ass.ProductID
                                   select new
                                   {
                                     pb.Barcode,
                                     JdeNumber = ass.CustomItemNumber
                                   }).ToList();

          Dictionary<int, List<string>> barcodeList = new Dictionary<int, List<string>>();

          connectorBarcodes.ForEach(x =>
          {
            int jdeNumber = int.Parse(x.JdeNumber);

            if (barcodeList.ContainsKey(jdeNumber))
              barcodeList[jdeNumber].Add(x.Barcode);
            else
              barcodeList.Add(jdeNumber, new List<string>() { x.Barcode });
          });


          log.Debug("Start import/update barcodes in Aggregator for " + connector.Name);
          #region barcode
          var barcodes = new Dictionary<int, List<string>>();

          using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
          {
            connection.Open();

            using (SqlCommand command = new SqlCommand(@"select 
p.JdeProductId,
b.Code
from Barcodes b
inner join Products p on b.ProductId = p.Id", connection))
            {
              using (SqlDataReader r = command.ExecuteReader())
              {
                while (r.Read())
                {
                  try
                  {
                    int prodid = (int)r["JdeProductId"];
                    string barcode = (string)r["Code"];

                    if (barcodes.ContainsKey(prodid))
                      barcodes[prodid].Add(barcode);
                    else
                      barcodes.Add(prodid, new List<string>() { barcode });
                  }
                  catch (Exception ex)
                  {
                    log.Error("Error insert barcode", ex);
                  }

                }
              }
            }

            try
            {
              barcodeList.Keys.ForEach((prod, x) =>
              {
                int JdeNumber = prod;
                var unusedBarcodes = new List<string>();

                if (barcodes.ContainsKey(JdeNumber))
                  unusedBarcodes = barcodes[JdeNumber];

 barcodeList[prod].ForEach((bar, idx) =>
                  {
                    try
                    {
                      string barcode = bar.Trim();

                      if (unusedBarcodes.Contains(barcode))
                      {
                        unusedBarcodes.Remove(barcode);
                      }
                      else
                      {
                        try
                        {
                          string query = string.Format(@"insert into barcodes (productid,code)
select id, '{0}' from products where jdeproductid = {1} and isstatic != 1 and '{0}' not in (select code from barcodes)", barcode, JdeNumber);
                          //unusedBarcodes.Remove(barcode);

                          using (SqlCommand command = new SqlCommand(query, connection))
                          {
                            command.ExecuteNonQuery();
                          }
                        }
                        catch (Exception ex)
                        {
                          log.Error("Error insert barcode", ex);
                        }
                      }
                    }
                    catch (Exception ex)
                    {
                      log.Error("error", ex);
                    }
                  });

                unusedBarcodes.ForEach(b =>
                {
                  string deleteBarcodeQuery = string.Format(@"DELETE b
FROM barcodes b
INNER JOIN products p on b.productid = p.id
where p.jdeproductid = {0} and code = '{1}' and isstatic != 1", JdeNumber, b);

                  using (SqlCommand command = new SqlCommand(deleteBarcodeQuery, connection))
                  {
                    command.ExecuteNonQuery();
                  }
                });

              });
            }
            catch (Exception ex)
            {
              log.Error("Error export barcodes to aggregator", ex);
            }
          }
          #endregion
          log.Debug("Finish import/update products in Aggregator for " + connector.Name);


        }
      }

    }
  }
}
