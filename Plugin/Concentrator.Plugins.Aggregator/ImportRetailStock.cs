using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Data.SqlClient;
using System.Configuration;
using Concentrator.Objects.Models.Vendors;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Bulk;

namespace Concentrator.Plugins.Aggregator
{
  public class ImportRetailStock : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Aggregator Retail Stock import"; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      using (var unit = GetUnitOfWork())
      {
        Vendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.AggregatorRetailStock) && x.IsActive).ForEach((vendor, idx) =>
        {
          try
          {
            var retailDic = (from v in unit.Scope.Repository<VendorMapping>().GetAll(x => x.VendorID == vendor.VendorID)
                             select new
                             {
                               BackendVendorCode = v.Vendor1.BackendVendorCode,
                               VendorID = v.MapVendorID
                             }).ToDictionary(x => x.BackendVendorCode, x => x.VendorID);

            Dictionary<int, List<VendorStock>> vendorStockList = new Dictionary<int, List<VendorStock>>();

            //var translationDic = GetCrossProducts(vendor.VendorID, unit);

            List<Concentrator.Objects.Vendors.Bulk.VendorStockBulk.VendorImportStock> stockList = new List<Objects.Vendors.Bulk.VendorStockBulk.VendorImportStock>();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
            {
              connection.Open();

              using (SqlCommand command = new SqlCommand(string.Format("select * from {0}", config.AppSettings.Settings["SqlViewName"].Value), connection))
              {
                command.CommandTimeout = new TimeSpan(0, 15, 0).Seconds;
                using (SqlDataReader r = command.ExecuteReader())
                {
                  while (r.Read())
                  {
                    try
                    {
                      string sapNr = (string)r["ProductId"];
                      int tempID = !r.IsDBNull(r.GetOrdinal("ConcentratorProductId")) ? (int)r["ConcentratorProductId"] : -1;

                      string backendShopNumber = ((int)r["BackendRelationID"]).ToString();

                      if (tempID > 0 && retailDic.ContainsKey(backendShopNumber)) //|| translationDic.ContainsKey(sapNr)) )
                      {
                        int concentratorProductID = tempID; //> 0 ? tempID : translationDic[sapNr];

                        int retailVendorID = retailDic[backendShopNumber];
                        int qty = (int)double.Parse(r["QuantityOnHand"].ToString());

                        var vs = new Concentrator.Objects.Vendors.Bulk.VendorStockBulk.VendorImportStock()
                        {
                          VendorID = retailVendorID,
                          VendorStockType = "Assortment",
                          ProductID = concentratorProductID,
                          QuantityOnHand = qty > 0 ? qty : 0,
                          CustomItemNumber = null,
                          defaultVendorID = retailVendorID,
                          PromisedDeliveryDate = null,
                          QuantityToReceive = 0,
                          StockStatus = null,
                          UnitCost = null,
                          VendorBrandCode = null,
                          VendorItemNumber = null,
                          VendorStatus = null
                        };
                        stockList.Add(vs);
                      }
                    }
                    catch (Exception ex)
                    {
                      log.AuditError(ex);
                    }
                  }
                }
              }
            }

            using (var vendorStockBulk = new VendorStockBulk(stockList, vendor.VendorID, vendor.ParentVendorID, "Aggregator"))
            {
              vendorStockBulk.Init(unit.Context);
              vendorStockBulk.Sync(unit.Context);
            }

            log.Info("Finish import Aggregator stock for " + vendor.VendorID);
          }
          catch (Exception ex)
          {
            log.AuditError(ex);
          }

        });
      }
    }

    private Dictionary<string, int> GetCrossProducts(int vendorID, IUnitOfWork unit)
    {
      Dictionary<string, int> crossDic = new Dictionary<string, int>();

      using (JDECrossReference.JdeAssortmentSoapClient client = new JDECrossReference.JdeAssortmentSoapClient())
      {
        var concentratorProducts = (from p in unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == vendorID && x.IsActive)
                                    select new
                                    {
                                      ConcentratorProductID = p.ProductID,
                                      JdeItemNumber = p.CustomItemNumber
                                    }).ToList();

        var crossReference = client.GetCrossItemNumbers();

        concentratorProducts.ForEach(x =>
        {
          var item = (from DataRow r in crossReference.Tables[0].Rows
                      where r.Field<double>("JDE").ToString() == x.JdeItemNumber
                      select r.Field<string>("SAP")).FirstOrDefault();

          if (item != null)
          {
            if (!crossDic.ContainsKey(item))
              crossDic.Add(item.Trim(), x.ConcentratorProductID);
          }
          //else
          //{
          //  if (!crossDic.ContainsKey(x.ConcentratorProductID.ToString()))
          //    crossDic.Add(x.ConcentratorProductID.ToString(), x.ConcentratorProductID);
          //}
        });
      }

      return crossDic;
    }
  }
}
