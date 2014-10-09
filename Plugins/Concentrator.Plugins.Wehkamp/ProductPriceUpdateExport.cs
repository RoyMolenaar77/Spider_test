using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.Enums;
using Concentrator.Plugins.Wehkamp.Helpers;

namespace Concentrator.Plugins.Wehkamp
{
  public class ProductPriceUpdateExport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Product Price Update Export"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      log.InfoFormat("Start processing Product Price Update");

      var vendorIDsToProcess = VendorSettingsHelper.GetVendorIDsToExportToWehkamp(log);

      foreach (var vendorID in vendorIDsToProcess)
      {
        _monitoring.Notify(Name, vendorID);
        log.InfoFormat("Start processing Product Price Update for VendorID {0}", vendorID);

        var runDateTime = DateTime.Now;

        //Get all price updates that we need to export to Wehkamp
        var products = GetProductsWithChangedPricesData(vendorID);
        if (products == null || products.Count == 0)
        {
          log.InfoFormat("There are no price updates for VendorID {0}", vendorID);
          //Update last processed datetime for vendor
          VendorSettingsHelper.SetLastPriceExportDateTime(vendorID, runDateTime);
          continue;
        }

        log.InfoFormat("Processing {0} Product Price Updates records for VendorID {1}", products.Count, vendorID);

        //Add all products to the artikelInformatie file
        var priceChange = new prijsAanpassing { aanpassing = new List<prijsAanpassingAanpassing>() };

        foreach (var product in products)
        {
          priceChange.aanpassing.Add(CreateNewPrijsAanpassingItem(product));
        }

        //Save data to disk
        SavePriceChanges(vendorID, priceChange);

        //Process products which needs a price update file
        ProcessResendProductInformationToWehkampProducts(products);

        //Update last processed datetime for vendor
        VendorSettingsHelper.SetLastPriceExportDateTime(vendorID, runDateTime);

        log.InfoFormat("Finished processing Product Price Update for VendorID {0}", vendorID);
      } //end foreach (var vendorID in vendorIDsToProcess)

      log.InfoFormat("Finished processing Product Price Update");
      _monitoring.Notify(Name, 1);
    }

    private void SavePriceChanges(int vendorID, prijsAanpassing priceChange)
    {
      var alliantieName = VendorSettingsHelper.GetAlliantieName(vendorID);
      var retailPartnerCode = VendorSettingsHelper.GetRetailPartnerCode(vendorID);
      var sequenceNumber = CommunicatorHelper.GetSequenceNumber(vendorID);

      priceChange.header.berichtDatumTijd = DateTime.Now;
      priceChange.header.berichtNaam = "prijsAanpassing";
      priceChange.header.retailPartnerCode = retailPartnerCode;
      priceChange.header.bestandsNaam = string.Format("{0}{1}prijsAanpassing.xml", sequenceNumber, alliantieName);

      var messageIDPriceChange = MessageHelper.InsertMessage(MessageHelper.WehkampMessageType.ProductPriceUpdate, priceChange.header.bestandsNaam, vendorID);

      try
      {
        priceChange.SaveToFile(string.Format(Path.Combine(ConfigurationHelper.ProductPricesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), priceChange.header.bestandsNaam)));
        MessageHelper.UpdateMessageStatus(messageIDPriceChange, WehkampMessageStatus.Success);
      }
      catch (Exception ex)
      {
        log.Fatal("Error while saving Price Change file", ex);
        MessageHelper.UpdateMessageStatus(messageIDPriceChange, WehkampMessageStatus.Error);
      }
    }

    private void ProcessResendProductInformationToWehkampProducts(IEnumerable<ProductInformationSize> products)
    {
      var productIDs = products.Where(p => p.ResendProductInformationToWehkamp != null && p.ResendProductInformationToWehkamp.ToLowerInvariant() == "true").Select(product => product.ProductID).ToList();

      if (productIDs.Count == 0)
        return;

      var sql = string.Empty;
      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          sql = string.Format("UPDATE ProductAttributeValue SET Value = 'false' WHERE AttributeID = (SELECT AttributeID FROM ProductAttributeMetaData pamd WHERE pamd.AttributeCode = 'SentToWehkamp') AND ProductID IN (SELECT DISTINCT ParentProductID FROM Product WHERE ProductID IN ({0}))", string.Join(",", productIDs.ToArray()));
          db.Execute(sql);

          sql = string.Format("DELETE FROM ProductAttributeValue WHERE AttributeID = {0} AND ProductID IN (SELECT DISTINCT ParentProductID FROM Product WHERE ProductID IN ({1}))", VendorSettingsHelper.GetResendProductInformationToWehkampAttributeID(), string.Join(",", productIDs.ToArray()));
          db.Execute(sql);
        }
      }
      catch (Exception ex)
      {
        log.AuditError(String.Format("ProcessResendPriceUpdateToWehkampProducts. ProductID's : {0}; SQL used: {1}", string.Join(",", productIDs.ToArray()), sql), ex);
      }
    }

    private static prijsAanpassingAanpassing CreateNewPrijsAanpassingItem(ProductInformationSize product)
    {
      return new prijsAanpassingAanpassing
      {
        artikelNummer = product.Artikelnummer,
        kleurNummer = product.Kleurnummer,
        maat = product.WehkampMagazijnmaat,
        verkoopPrijs = product.SpecialPrijs != null ? product.SpecialPrijs.Value : product.Prijs,
        beginDatum = DateTime.Now,
        beginDatumSpecified = true
      };
    }

    private List<ProductInformationSize> GetProductsWithChangedPricesData(int vendorID)
    {
      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          var products = db.Fetch<ProductInformationSize>(string.Format(QueryHelper.GetProductsWithPriceChangesQuery(), vendorID, VendorSettingsHelper.GetLastPriceExportDateTime(vendorID).ToUniversalTime().ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture)));
          return products;
        }
      }
      catch (Exception ex)
      {
        log.Error("Error while retrieving products to export to Wehkamp.", ex);
        return null;
      }
    }
  }
}
