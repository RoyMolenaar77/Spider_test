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
  public class ShipmentNotificationExport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Shipment Notification Export"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      log.InfoFormat("Start processing Shipment Notification");

      var vendorIDsToProcess = VendorSettingsHelper.GetVendorIDsToExportToWehkamp(log);

      foreach (var vendorID in vendorIDsToProcess)
      {
        _monitoring.Notify(Name, vendorID);
        log.InfoFormat("Start processing Shipment Notification for VendorID {0}", vendorID);

        var dummyProductIDsInFile = new List<int>();

        //Get all products in shipment notification that we need to export to Wehkamp
        var products = GetShipmentNotificationProductInformationData(vendorID);
        if (products == null || products.Count == 0)
          continue;

        //Add all products to the artikelInformatie file that aren't sent to Wehkamp before
        var artInfo = new artikelInformatie();

        //Add all products to the aankomst file
        var shipmentInfo = new aankomst();

        foreach (var product in products)
        {
          shipmentInfo.aankomsten.Add(CreateNewShipmentItem(product, vendorID));

          //Create dummy product if parentproduct isn't sent to Wehkamp (as dummy or regular product)
          if ((string.IsNullOrEmpty(product.ParentSentToWehkamp) || product.ParentSentToWehkamp.ToLower(CultureInfo.InvariantCulture) == "false") && (string.IsNullOrEmpty(product.ParentSentToWehkampAsDummy) || product.ParentSentToWehkampAsDummy.ToLower(CultureInfo.InvariantCulture) == "false") && !dummyProductIDsInFile.Contains(product.ParentProductID))
          {
            var artikel = CreateNewDummyArtikelInformatieArtikel(product);

            var sizes = GetDummyProductSizes(vendorID, product.ParentProductID);
            foreach (var size in sizes)
            {
              artikel.maatlijst.Add(CreateNewDummyArtikelInformatieMaatGegevens(size));
            }

            artInfo.artikel.Add(artikel);

            if (!dummyProductIDsInFile.Contains(product.ParentProductID))
              dummyProductIDsInFile.Add(product.ParentProductID);

            if (!dummyProductIDsInFile.Contains(product.ProductID))
              dummyProductIDsInFile.Add(product.ProductID);
          }
        }

        var orderIDs = products.Select(p => p.OrderID).Distinct().ToList();
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
          db.CommandTimeout = 60;

          try
          {
            var updateOrdersSql = string.Format("UPDATE [Order] SET IsDispatched = 1, DispatchToVendorDate = getdate() WHERE OrderID IN ({0})", string.Join(",", orderIDs.ToArray()));
            db.Execute(updateOrdersSql);

            foreach (var orderID in orderIDs)
            {
              var id = orderID;
              var item = from p in products
                         where p.OrderID == id
                         select p;

              var orderLineIDs = item.Select(p => p.OrderLineID).Distinct().ToList();
              var updateOrderLinesSql = string.Format("UPDATE [OrderLine] SET IsDispatched = 1, DispatchedToVendorID = {1} WHERE OrderLineID IN ({0})", string.Join(",", orderLineIDs.ToArray()), vendorID);
              db.Execute(updateOrderLinesSql);
            }

            //Save data to disk
            if (artInfo.artikel.Count != 0)
            {
              SaveProductInformation(vendorID, artInfo, db);
              //Set all dummy products as SentToAsDummyWehkamp = true
              SetDummyProductsAsSentAsDummyToWehkamp(dummyProductIDsInFile, db);
            }

            SaveShipmentInformation(vendorID, shipmentInfo, db);

            db.CompleteTransaction();
          }
          catch (Exception e)
          {
            db.AbortTransaction();
            log.AuditError("Something went wrong while updating the order dispatched status or saving the files in the wehkamp drives", e);
          }
        }

        log.InfoFormat("Finished processing Shipment Notification for VendorID {0}", vendorID);
      } //end foreach (var vendorID in vendorIDsToProcess)

      log.InfoFormat("Finished processing Shipment Notification");
      _monitoring.Notify(Name, 1);
    }


    private static aankomstAankomsten CreateNewShipmentItem(ProductInformationShipmentNotification product, int vendorID)
    {
      return new aankomstAankomsten
      {
        artikelNummer = product.Artikelnummer,
        kleurNummer = product.Kleurnummer,
        maat = product.WehkampMagazijnmaat,
        aantalOpgegeven = product.NumberToReceive,
        verwachteAankomstDatum = DateTime.Now.AddDays(VendorSettingsHelper.GetDaysToAddForShipmentDate(vendorID)),
        ggb = product.PackingSlipNumber,
        goederenSoort = "V"
      };
    }

    private List<ProductInformationShipmentNotification> GetShipmentNotificationProductInformationData(int vendorID)
    {
      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          var products = db.Fetch<ProductInformationShipmentNotification>(string.Format(QueryHelper.GetShipmentNotificationProductInformationQuery(), vendorID));
          return products;
        }
      }
      catch (Exception ex)
      {
        log.Error("Error while retrieving shipment notification products to export to Wehkamp.", ex);
        return null;
      }

    }
    private List<ProductInformationSize> GetDummyProductSizes(int vendorID, int productID)
    {
      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          var sizes = db.Fetch<ProductInformationSize>(string.Format(QueryHelper.GetProductInformationSizesQuery(), vendorID, productID));
          return sizes;
        }
      }
      catch (Exception ex)
      {
        log.Error("Error while retrieving productsizes to export to Wehkamp.", ex);
        return null;
      }
    }

    private static artikelInformatieArtikel CreateNewDummyArtikelInformatieArtikel(ProductInformation product)
    {
      return new artikelInformatieArtikel
      {
        artikelNummer = product.Artikelnummer,
        kleurNummer = product.Kleurnummer,
        startDatum = product.Startdatum,
        korteOmschrijving = "Dummy",
        langeOmschrijving = "Dummy",
        kleurOmschrijving = "Dummy",
        kwaliteitOmschrijving = "Dummy",
        materiaalOmschrijving = "Dummy",
        beeldSoortNummer = "4",
        artikelGroep = product.Artikelgroep,
        maatlijst = new List<artikelInformatieArtikelMaatGegevens>()
      };
    }
    private static artikelInformatieArtikelMaatGegevens CreateNewDummyArtikelInformatieMaatGegevens(ProductInformationSize size)
    {
      return new artikelInformatieArtikelMaatGegevens
      {
        maat = size.WehkampMagazijnmaat,
        presentatieMaat = size.Maat,
        wehkampMaat = size.WehkampMagazijnmaat,
        wehkampPresentatieMaat = size.WehkampPresentatiemaat,
        verkoopPrijs = size.SpecialPrijs != null ? size.SpecialPrijs.Value : size.Prijs,
        scratchPrijs = size.Prijs,
        scratchPrijsSpecified = true, //--> Altijd de scratchprijs meegeven; ook indien gelijk aan verkoopprijs (verzoek CC/AT)
        EANNummer = size.EANCode
      };
    }


    private void SaveShipmentInformation(int vendorID, aankomst shipment, PetaPoco.Database db = null)
    {
      var alliantieName = VendorSettingsHelper.GetAlliantieName(vendorID);
      var retailPartnerCode = VendorSettingsHelper.GetRetailPartnerCode(vendorID);
      var sequenceNumberShipment = CommunicatorHelper.GetSequenceNumber(vendorID);

      shipment.header.berichtDatumTijd = DateTime.Now;
      shipment.header.berichtNaam = "aankomst";
      shipment.header.retailPartnerCode = retailPartnerCode;
      shipment.header.bestandsNaam = string.Format("{0}{1}aankomst.xml", sequenceNumberShipment, alliantieName);

      var messageIDShipment = MessageHelper.InsertMessage(MessageHelper.WehkampMessageType.ShipmentNotification, shipment.header.bestandsNaam, vendorID, db);

      try
      {
        shipment.SaveToFile(Path.Combine(ConfigurationHelper.ShipmentNotificationRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), shipment.header.bestandsNaam));
        MessageHelper.UpdateMessageStatus(messageIDShipment, WehkampMessageStatus.Success, db);
      }
      catch (Exception ex)
      {
        log.Fatal("Error while saving aankomst file", ex);
        MessageHelper.UpdateMessageStatus(messageIDShipment, WehkampMessageStatus.Error);
      }
    }
    private void SaveProductInformation(int vendorID, artikelInformatie artInfo, PetaPoco.Database db = null)
    {
      var alliantieName = VendorSettingsHelper.GetAlliantieName(vendorID);
      var retailPartnerCode = VendorSettingsHelper.GetRetailPartnerCode(vendorID);
      var sequenceNumberArtikelInformatie = CommunicatorHelper.GetSequenceNumber(vendorID);

      artInfo.header.berichtDatumTijd = DateTime.Now;
      artInfo.header.berichtNaam = "artikelInformatie";
      artInfo.header.retailPartnerCode = retailPartnerCode;
      artInfo.header.bestandsNaam = string.Format("{0}{1}artikelInformatie.xml", sequenceNumberArtikelInformatie, alliantieName);

      var messageIDArtInfo = MessageHelper.InsertMessage(MessageHelper.WehkampMessageType.ProductInformation, artInfo.header.bestandsNaam, vendorID, db);

      try
      {
        artInfo.SaveToFile(string.Format(Path.Combine(ConfigurationHelper.ProductInformationRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), artInfo.header.bestandsNaam)));
        MessageHelper.UpdateMessageStatus(messageIDArtInfo, WehkampMessageStatus.Success, db);
      }
      catch (Exception ex)
      {
        log.Fatal("Error while saving artikelInfo file", ex);
        MessageHelper.UpdateMessageStatus(messageIDArtInfo, WehkampMessageStatus.Error);
      }
    }


    private static void SetProcessedOrdersAsDispatched(List<int> orderIDs)
    {
      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 5 * 60;

        var sql = string.Format("UPDATE [Order] SET IsDispatched = 1, DispatchToVendorDate = getdate() WHERE OrderID IN ({0})", string.Join(",", orderIDs.ToArray()));
        db.Execute(sql);
      }
    }
    private static void SetProcessedOrderLinesAsDispatched(List<int> orderLineIDs, int vendorID)
    {
      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 5 * 60;

        var sql = string.Format("UPDATE [OrderLine] SET IsDispatched = 1, DispatchedToVendorID = {1} WHERE OrderLineID IN ({0})", string.Join(",", orderLineIDs.ToArray()), vendorID);
        db.Execute(sql);
      }
    }


    private static void SetDummyProductsAsSentAsDummyToWehkamp(List<int> productIDs, PetaPoco.Database db = null)
    {
      var sentToWehkampAsDummyAttributeID = VendorSettingsHelper.GetSentToWehkampAsDummyAttributeID();

      var disposeDatabase = (db == null);
      if (db == null)
      {
        db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient");
        db.CommandTimeout = 5 * 60;
      }

      foreach (var productID in productIDs)
      {
        var actualProductID = db.ExecuteScalar<int>(@"select isnull(ParentProductID, productID) as ProductID from Product where productid = @0", productID);

        var sql = string.Format("IF EXISTS (SELECT * FROM [ProductAttributeValue] WHERE AttributeID = {0} AND ProductID = {1}) UPDATE [ProductAttributeValue] SET [Value] = 'true' WHERE AttributeID = {0} AND ProductID = {1} ELSE INSERT INTO [ProductAttributeValue] (AttributeID, ProductID, [Value], CreatedBy, CreationTime) VALUES ({0}, {1}, 'true', 1, getdate())", sentToWehkampAsDummyAttributeID, actualProductID);
        db.Execute(sql);
      }

      if (disposeDatabase)
        db.Dispose();
    }
  }


  internal class ProductInformationShipmentNotification : ProductInformation
  {
    public string NumberToReceive { get; set; }
    public string ReceivedDate { get; set; }
    public string PackingSlipNumber { get; set; }
    public string Maat { get; set; }
    public string Maatcode { get; set; }
    public string EANCode { get; set; }
    public string WehkampPresentatiemaat { get; set; }
    public string WehkampMagazijnmaat { get; set; }
    public decimal Prijs { get; set; }
    public decimal? SpecialPrijs { get; set; }
    public string LastUpdated { get; set; }
    public int ParentProductID { get; set; }

    public string ParentReadyForWehkamp { get; set; }
    public string ParentSentToWehkamp { get; set; }
    public string ParentSentToWehkampAsDummy { get; set; }

    public int OrderID { get; set; }
    public int OrderLineID { get; set; }

  }
}
