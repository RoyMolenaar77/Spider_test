using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.Helpers;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp
{
  public class ProductRelationImport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Product Relation Import"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      var vendorIDsToProcess = VendorSettingsHelper.GetVendorIDsToExportToWehkamp(log);

      foreach (var vendorID in vendorIDsToProcess)
      {
        _monitoring.Notify(Name, vendorID);

        var productIDs = new List<int>();
        var id = vendorID;
        var messages = MessageHelper.GetMessagesByStatusAndType(Enums.WehkampMessageStatus.Created, MessageHelper.WehkampMessageType.ProductRelation).Where(m => m.VendorID == id);

        foreach (var message in messages)
        {
          log.Info(string.Format("{0} - Loading file: {1}", Name, message.Filename));

          MessageHelper.UpdateMessageStatus(message.MessageID, Enums.WehkampMessageStatus.InProgress);

          artikelRelatie relatieData;
          var loaded = artikelRelatie.LoadFromFile(Path.Combine(message.Path, message.Filename), out relatieData);

          if (!loaded)
          {
            log.AuditError(string.Format("Error while loading file {0}", message.Filename));
            MessageHelper.Error(message);
            continue;
          }

          try
          {
            using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
            {
              var attributeID = db.FirstOrDefault<int>(string.Format("SELECT AttributeID FROM ProductAttributeMetaData WHERE AttributeCode = '{0}'", "WehkampProductNumber"));

              if (attributeID == 0)
              {
                throw new ConfigurationErrorsException("Attribute \"WehkampProductNumber\" is missing from the ProductAttributeMetadata table");
              }

              foreach (var rel in relatieData.relatie)
              {
                var productID = ProductHelper.GetProductIDByWehkampData(rel.artikelNummer, rel.kleurNummer, message.VendorID);

                if (productID == 0)
                {
                  log.Error(string.Format("No product could be found with VendorItemNumber containing {0} and {1}", rel.artikelNummer, rel.kleurNummer));
                  continue;
                }

                productIDs.Add(productID);


                var attributevalueid = db.FirstOrDefault<int>(string.Format("SELECT AttributeValueID FROM ProductAttributeValue WHERE AttributeID = '{0}' AND ProductID = '{1}'", attributeID, productID));
                if (attributevalueid == 0)
                {
                  db.Execute(string.Format("INSERT INTO ProductAttributeValue (AttributeID, ProductID, Value, CreatedBy, CreationTime) VALUES ('{0}', '{1}', '{2}', '{3}', getdate())",
                    attributeID, productID, rel.wehkampArtikelNummer, 1));
                }
                else
                {
                  db.Execute(string.Format("UPDATE ProductAttributeValue SET Value = '{1}', LastModifiedBy = '{2}', LastModificationTime = getdate() WHERE AttributeValueID = '{0}'",
                    attributevalueid, rel.wehkampArtikelNummer, 1));
                }
              }
            }
          }
          catch (Exception ex)
          {
            log.Fatal(string.Format("Error while processing file {0}", message.Filename), ex);

            MessageHelper.Error(message);
            continue;
          }

          MessageHelper.Archive(message);
        }

        //Process Images for all found products
        var pme = new ProductMediaExport();
        pme.ProcessProductImages(productIDs, vendorID);
      }

      _monitoring.Notify(Name, 1);
    }
  }
}
