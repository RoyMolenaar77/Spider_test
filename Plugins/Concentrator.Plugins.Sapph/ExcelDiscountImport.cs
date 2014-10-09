using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Sapph
{
  public class ExcelDiscountImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Excel Discount Import"; }
    }
    public bool AddProductsToContentProductGroup { get; set; }

    protected override void Process()
    {
      System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

      var discountExcelLocation = GetConfiguration().AppSettings.Settings["DiscountImport.ExcelLocation"];
      var addToProductGroupMappingID = GetConfiguration().AppSettings.Settings["DiscountImport.AddToProductGroupMappingID"];
      var addToMasterGroupMappingID = GetConfiguration().AppSettings.Settings["DiscountImport.AddToMasterGroupMappingID"];
      var connectorIDs = GetConfiguration().AppSettings.Settings["DiscountImport.ConnectorIDs"].Value.SplitToInts(',');

      if (discountExcelLocation == null || string.IsNullOrEmpty(discountExcelLocation.Value))
      {
        log.AuditError("The discount excel location was not specified");
        return;
      }

      if (addToProductGroupMappingID != null && !string.IsNullOrEmpty(addToProductGroupMappingID.Value))
        AddProductsToContentProductGroup = true;

      var repo = new Sapph.Repositories.ExcelSalesPricesRepository(discountExcelLocation.Value);

      var discountList = repo.GetDiscountList().Distinct().ToList();

      if (discountList == null)
        return;

      var totalRows = discountList.Count;
      var currentRow = 0;

      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 120;

        db.BeginTransaction();

        try
        {
          var processed = new List<string>();
          foreach (var discountProduct in discountList)
          {
            if(currentRow % 50 == 0)
              log.AuditInfo(string.Format("Progress {0}/{1}", currentRow, totalRows));

            currentRow++;
            if (processed.Contains(discountProduct.VendorItemNumber))
              continue;

            var updatePriceQuery = string.Format(@"UPDATE VendorPrice SET SpecialPrice = VP.Price - ( VP.Price * {0} )
                        FROM Product P
                        INNER JOIN VendorAssortment VA ON P.ProductID = VA.ProductID
                        INNER JOIN VendorPrice VP ON VA.VendorAssortmentID = VP.VendorAssortmentID
                        WHERE P.IsConfigurable = 0 AND P.VendorItemNumber LIKE '{1}%'", discountProduct.DiscountPercentage.ToString(), discountProduct.VendorItemNumber);

            db.Execute(updatePriceQuery);

            if (AddProductsToContentProductGroup)
            {
              var deleteQuery = @"DELETE CPG 
                                  FROM ContentProductGroup CPG
                                  INNER JOIN Product P ON CPG.ProductID = P.ProductID
                                  WHERE P.VendorItemNumber LIKE '{0}%' AND CPG.MasterGroupMappingID = {1} AND CPG.ConnectorID = {2}";

              var baseQuery = @"INSERT INTO ContentProductGroup 
                           SELECT {0}, P.ProductID, {1}, 1, GETDATE(), NULL, NULL, 1, 0, 1, {2}
                           FROM Product P
                           INNER JOIN Content C ON P.ProductID = C.ProductID
                           WHERE C.ConnectorID = {0} AND P.VendorItemNumber LIKE '{3}%'";

              foreach (var connectorID in connectorIDs)
              {
                var deleteExistingContentProductGroupsQuery = string.Format(deleteQuery,
                  discountProduct.VendorItemNumber,
                  addToMasterGroupMappingID.Value,
                  connectorID);

                db.Execute(deleteExistingContentProductGroupsQuery);

                var insertIntoContentProductGroupQuery = string.Format(baseQuery,
                  connectorID,
                  addToProductGroupMappingID.Value,
                  addToMasterGroupMappingID.Value,
                  discountProduct.VendorItemNumber);

                db.Execute(insertIntoContentProductGroupQuery);
              }
            }
            processed.Add(discountProduct.VendorItemNumber);
          }
          db.CompleteTransaction();
        }
        catch (Exception e)
        {
          db.AbortTransaction();
          log.AuditError("An error occured update the special prices", e);
        }
        log.AuditInfo("Succesfully updated special prices with discounts.");
      }
    }
  }
}