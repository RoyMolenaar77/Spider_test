using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.Enums;
using Concentrator.Plugins.Wehkamp.Helpers;

namespace Concentrator.Plugins.Wehkamp
{
  public class ProductInformationExport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Product Information Export"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      log.InfoFormat("Start processing Product Information");

      var vendorIDsToProcess = VendorSettingsHelper.GetVendorIDsToExportToWehkamp(log);

      foreach (var vendorID in vendorIDsToProcess)
      {
        _monitoring.Notify(Name, vendorID);
        log.InfoFormat("Start processing Product Information for VendorID {0}", vendorID);

        var productIDsInFile = new List<int>();

        //Get all products that we need to export to Wehkamp
        var products = GetProductInformationData(vendorID);
        if (products == null || products.Count == 0)
        {
          log.InfoFormat("No 'export to wehkamp' products found for VendorID {0}", vendorID);
          continue;
        }

        //Add all products to the artikelInformatie file
        var artInfo = new artikelInformatie();

        log.InfoFormat("Start processing {0} products for VendorID {1}", products.Count, vendorID);
        var start = DateTime.Now;
        var processedCount = 0;
        foreach (var product in products)
        {
          artikelInformatieArtikel artikel;

          try
          {
            artikel = CreateNewArtikelInformatieArtikel(product);
            productIDsInFile.Add(product.ProductID);
          }
          catch (Exception e)
          {
            log.AuditError(string.Format("Can't create Wehkamp article for product: id='{0}' - vendoritemnumber='{1}'. Product isn't exported to Wehkamp", product.ProductID, product.VendorItemNumber), e);
            continue;
          }

          
          var sizes = GetProductSizes(vendorID, product.ProductID);
          foreach (var size in sizes)
          {
            artikel.maatlijst.Add(CreateNewArtikelInformatieMaatGegevens(size));
          }

          artInfo.artikel.Add(artikel);

          processedCount++;
          if (DateTime.Now > start.AddSeconds(30))
          {
            log.InfoFormat("Processed {0} products for VendorID {1}", processedCount, vendorID);
            start = DateTime.Now;
          }
        }



        //Create productattributes (artikelEigenschap) for all products in artikelInformatie file
        var artikelEigenschapData = CreateProductAttributes(products);

        //Save data to disk
        log.InfoFormat("Start saving Product Information");
        var productInformationSaved = SaveProductInformation(vendorID, artInfo);

        log.InfoFormat("Start saving Product Attributes");
        var productAttributesSaved = SaveProductAttributes(vendorID, artikelEigenschapData);

        if (productInformationSaved && productAttributesSaved)
        {
          //Set all products as SentToWehkamp = true
          log.InfoFormat("Start setting products as SentToWehkamp");
          SetProductsAsSentToWehkamp(productIDsInFile);

          //Remove SentAsDummy attribute if sent before as a dummy product
          log.InfoFormat("Start cleaning SentAsDummy attribute");
          RemoveSentAsDummy(productIDsInFile);

          //Process products which needs a price update file
          ProcessResendPriceUpdateToWehkampProducts(products, vendorID);
        }
        
        log.InfoFormat("Finished processing Product Information for VendorID {0}", vendorID);
      } //end foreach (var vendorID in vendorIDsToProcess)

      log.InfoFormat("Finished processing Product Information");
      _monitoring.Notify(Name, 1);
    }

    private void ProcessResendPriceUpdateToWehkampProducts(IEnumerable<ProductInformation> products, int vendorID)
    {
      var productIDs = products.Where(p => p.ResendPriceUpdateToWehkamp != null && p.ResendPriceUpdateToWehkamp.ToLowerInvariant() == "true").Select(product => product.ProductID).ToList();

      if (productIDs.Count == 0)
        return;

      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          var sql = string.Format("UPDATE VendorPrice SET LastUpdated = GETDATE() WHERE VendorAssortmentID IN (SELECT VendorAssortmentID FROM VendorAssortment va INNER JOIN Product p ON p.ProductID = va.ProductID WHERE p.ParentProductID IN ({0}) AND va.VendorID = {1})", string.Join(",", productIDs.ToArray()), vendorID);
          db.Execute(sql);

          sql = string.Format("DELETE FROM ProductAttributeValue WHERE AttributeID = {0} AND ProductID IN ({1})", VendorSettingsHelper.GetResendPriceUpdateToWehkampAttributeID(), string.Join(",", productIDs.ToArray()));
          db.Execute(sql);
        }
      }
      catch (Exception ex)
      {
        log.AuditError(String.Format("ProcessResendPriceUpdateToWehkampProducts. ProductID's : {0}", string.Join(",", productIDs.ToArray())), ex);
      }
    }


    private static artikelInformatieArtikel CreateNewArtikelInformatieArtikel(ProductInformation product)
    {
      var artInfo = new artikelInformatieArtikel
      {
        artikelNummer = product.Artikelnummer,
        kleurNummer = product.Kleurnummer,
        startDatum = product.Startdatum,
        korteOmschrijving = product.Korteomschrijving.Length > 16 ? product.Korteomschrijving.Substring(0, 16) : product.Korteomschrijving,
        langeOmschrijving = product.Langeomschrijving,
        kleurOmschrijving = product.Kleuromschrijving.Length > 100 ? product.Kleuromschrijving.Substring(0, 100) : product.Kleuromschrijving,
        kwaliteitOmschrijving = product.Kwaliteitomschrijving.Length > 100 ? product.Kwaliteitomschrijving.Substring(0, 100) : product.Kwaliteitomschrijving,
        materiaalOmschrijving = product.MateriaalomschrijvingCoolcat.Length > 100 ? product.MateriaalomschrijvingCoolcat.Substring(0, 100) : product.MateriaalomschrijvingCoolcat,
        beeldSoortNummer = "4",
        artikelGroep = product.Artikelgroep,
        maatlijst = new List<artikelInformatieArtikelMaatGegevens>()
      };

      if (artInfo.langeOmschrijving.IndexOf("\r", StringComparison.Ordinal) > 0)
      {
        artInfo.langeOmschrijving = artInfo.langeOmschrijving.Substring(0, artInfo.langeOmschrijving.IndexOf("\r", StringComparison.Ordinal));
      }
      if (artInfo.langeOmschrijving.IndexOf("\n", StringComparison.Ordinal) > 0)
      {
        artInfo.langeOmschrijving = artInfo.langeOmschrijving.Substring(0, artInfo.langeOmschrijving.IndexOf("\n", StringComparison.Ordinal));
      }
      artInfo.langeOmschrijving = artInfo.langeOmschrijving.Length > 3000 ? artInfo.langeOmschrijving.Substring(0, 3000) : artInfo.langeOmschrijving;

      return artInfo;

    }
    private static artikelInformatieArtikelMaatGegevens CreateNewArtikelInformatieMaatGegevens(ProductInformationSize size)
    {
      return new artikelInformatieArtikelMaatGegevens
      {
        maat = size.WehkampMagazijnmaat,
        presentatieMaat = size.Maat,
        wehkampMaat = size.WehkampMagazijnmaat,
        wehkampPresentatieMaat = size.WehkampPresentatiemaat,
        verkoopPrijs = size.SpecialPrijs != null ? size.SpecialPrijs.Value : size.Prijs,
        scratchPrijs = size.Prijs,
        scratchPrijsSpecified = true, // --> Altijd de scratchprijs meegeven; ook indien gelijk aan verkoopprijs (verzoek CC/AT)
        EANNummer = size.EANCode
      };
    }

    private static artikelEigenschap CreateProductAttributes(List<ProductInformation> products)
    {
      var eigenschap = new artikelEigenschap();

      foreach (var information in products)
      {
        var artEigenschap = new artikelEigenschapArtikel
        {
          artikelNummer = information.Artikelnummer,
          kleurNummer = information.Kleurnummer,
          eigenschaplijst = new List<artikelEigenschapArtikelEigenschapGegevens>()
        };

        artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
        {
          eigenschap = "Geslacht",
          eigenschapWaarde = VendorItemHelper.GetWehkampGender(information.VendorItemNumber, information.VendorID)
        });

        artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
        {
          eigenschap = "Kleur",
          eigenschapWaarde = information.WehkampKleuromschrijving
        });

        artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
        {
          eigenschap = "Materiaal",
          eigenschapWaarde = information.MateriaalomschrijvingWehkamp
        });

        artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
        {
          eigenschap = "Merk",
          eigenschapWaarde = VendorSettingsHelper.GetMerkName(information.VendorID)
        });

        var wehkampSleeveLength = VendorItemHelper.GetWehkampSleeveLength(information.VendorItemNumber, information.VendorID);
        if (!string.IsNullOrEmpty(wehkampSleeveLength))
        {
          artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
          {
            eigenschap = "Mouwlengte",
            eigenschapWaarde = wehkampSleeveLength
          });
        }

        if (information.VendorID == 25)
        {
          if (!string.IsNullOrEmpty(information.Dessin))
          {
            artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
            {
              eigenschap = "Dessin",
              eigenschapWaarde = information.Dessin
            });
          }

          if (!string.IsNullOrEmpty(information.Kraagvorm))
          {
            artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
            {
              eigenschap = "Kraagvorm",
              eigenschapWaarde = information.Kraagvorm
            });
          }

          if (!string.IsNullOrEmpty(information.Pijpwijdte))
          {
            artEigenschap.eigenschaplijst.Add(new artikelEigenschapArtikelEigenschapGegevens
            {
              eigenschap = "Pijpwijdte",
              eigenschapWaarde = information.Pijpwijdte
            });
          }
        }


        eigenschap.artikel.Add(artEigenschap);
      }
      return eigenschap;
    }
 


    private List<ProductInformation> GetProductInformationData(int vendorID)
    {
      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          var products = db.Fetch<ProductInformation>(string.Format(QueryHelper.GetProductInformationQuery(), vendorID, VendorSettingsHelper.ExportOnlyProductsOlderThanXXXMinutes(vendorID)));
          return products;
        }
      }
      catch (Exception ex)
      {
        log.Error("Error while retrieving products to export to Wehkamp.", ex);
        return null;
      }

    }
    private List<ProductInformationSize> GetProductSizes(int vendorID, int productID)
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

    private bool SaveProductInformation(int vendorID, artikelInformatie artInfo)
    {
      var alliantieName = VendorSettingsHelper.GetAlliantieName(vendorID);
      var retailPartnerCode = VendorSettingsHelper.GetRetailPartnerCode(vendorID);
      var sequenceNumberArtikelInformatie = CommunicatorHelper.GetSequenceNumber(vendorID);

      artInfo.header.berichtDatumTijd = DateTime.Now;
      artInfo.header.berichtNaam = "artikelInformatie";
      artInfo.header.retailPartnerCode = retailPartnerCode;
      artInfo.header.bestandsNaam = string.Format("{0}{1}artikelInformatie.xml", sequenceNumberArtikelInformatie, alliantieName);

      var messageIDArtInfo = MessageHelper.InsertMessage(MessageHelper.WehkampMessageType.ProductInformation, artInfo.header.bestandsNaam, vendorID);

      try
      {
        artInfo.SaveToFile(Path.Combine(ConfigurationHelper.ProductInformationRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), artInfo.header.bestandsNaam));
        MessageHelper.UpdateMessageStatus(messageIDArtInfo, WehkampMessageStatus.Success);
      }
      catch (Exception ex)
      {
        log.Fatal("Error while saving artikelInfo file", ex);
        MessageHelper.UpdateMessageStatus(messageIDArtInfo, WehkampMessageStatus.Error);

        return false;
      }

      return true;
    }
    private bool SaveProductAttributes(int vendorID, artikelEigenschap artikelEigenschapData)
    {
      var alliantieName = VendorSettingsHelper.GetAlliantieName(vendorID);
      var retailPartnerCode = VendorSettingsHelper.GetRetailPartnerCode(vendorID);
      var sequenceNumberArtikelEigenschap = CommunicatorHelper.GetSequenceNumber(vendorID);

      artikelEigenschapData.header.berichtDatumTijd = DateTime.Now;
      artikelEigenschapData.header.berichtNaam = "artikelEigenschap";
      artikelEigenschapData.header.retailPartnerCode = retailPartnerCode;
      artikelEigenschapData.header.bestandsNaam = string.Format("{0}{1}artikelEigenschap.xml", sequenceNumberArtikelEigenschap, alliantieName);

      var messageIDArtAttr = MessageHelper.InsertMessage(MessageHelper.WehkampMessageType.ProductAttribute, artikelEigenschapData.header.bestandsNaam, vendorID);

      try
      {
        artikelEigenschapData.SaveToFile(string.Format(Path.Combine(ConfigurationHelper.ProductAttributesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), artikelEigenschapData.header.bestandsNaam)));
        MessageHelper.UpdateMessageStatus(messageIDArtAttr, WehkampMessageStatus.Success);
      }
      catch (Exception ex)
      {
        log.Fatal("Error while saving artikelEigenschap file", ex);
        MessageHelper.UpdateMessageStatus(messageIDArtAttr, WehkampMessageStatus.Error);

        return false;
      }

      return true;
    }



    private static void SetProductsAsSentToWehkamp(List<int> productIDs)
    {
      var sentToWehkampAttributeID = VendorSettingsHelper.GetSentToWehkampAttributeID();

      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 30;

        foreach (var productID in productIDs)
        {
          var sql = string.Format("IF EXISTS (SELECT * FROM [ProductAttributeValue] WHERE AttributeID = {0} AND ProductID = {1}) UPDATE [ProductAttributeValue] SET [Value] = 'true' WHERE AttributeID = {0} AND ProductID = {1} ELSE INSERT INTO [ProductAttributeValue] (AttributeID, ProductID, [Value], CreatedBy, CreationTime) VALUES ({0}, {1}, 'true', 1, getdate())", sentToWehkampAttributeID, productID);
          db.Execute(sql);
        }
      }
    }
    private static void RemoveSentAsDummy(List<int> productIDs)
    {
      var sentToWehkampAsDummyAttributeID = VendorSettingsHelper.GetSentToWehkampAsDummyAttributeID();

      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 30;
        var sql = string.Format("DELETE FROM ProductAttributeValue WHERE AttributeID = {0} AND ProductID IN ({1})", sentToWehkampAsDummyAttributeID, string.Join(",", productIDs.ToArray()));
        db.Execute(sql);

        var sqlParents = string.Format("DELETE FROM ProductAttributeValue WHERE AttributeID = {0} AND ProductID IN (SELECT ParentProductID FROM Product WHERE ProductID IN ({1}))", sentToWehkampAsDummyAttributeID, string.Join(",", productIDs.ToArray()));
        db.Execute(sqlParents);
      }
    }


  }


  internal class ProductInformation
  {
    public string Artikelnummer { get; set; }
    public string Kleurnummer { get; set; }
    public string FormattedKleurnummer { get; set; }
    public DateTime Startdatum { get; set; }
    public string Korteomschrijving { get; set; }
    public string Langeomschrijving { get; set; }
    public string Kleuromschrijving { get; set; }
    public string WehkampKleuromschrijving { get; set; }
    public string Kwaliteitomschrijving { get; set; }
    public string MateriaalomschrijvingCoolcat { get; set; }
    public string MateriaalomschrijvingWehkamp { get; set; }
    public string Artikelgroep { get; set; }
    public int SourceVendorID { get; set; }
    public int ProductID { get; set; }
    public string VendorItemNumber { get; set; }
    public int ConnectorID { get; set; }
    public int VendorID { get; set; }
    public string ReadyForWehkamp { get; set; }
    public string SentToWehkamp { get; set; }
    public string SentToWehkampAsDummy { get; set; }
    public string ResendPriceUpdateToWehkamp { get; set; }
    public string Dessin { get; set; }
    public string Kraagvorm { get; set; }
    public string Pijpwijdte { get; set; }
  }

  internal class ProductInformationSize
  {
    public string Artikelnummer { get; set; }
    public string Kleurnummer { get; set; }
    public string FormattedKleurnummer { get; set; }
    public string VendorItemNumber { get; set; }
    public int ProductID { get; set; }
    public string Maat { get; set; }
    public string Maatcode { get; set; }
    public string EANCode { get; set; }
    public string WehkampPresentatiemaat { get; set; }
    public string WehkampMagazijnmaat { get; set; }
    public decimal Prijs { get; set; }
    public decimal? SpecialPrijs { get; set; }
    public string LastUpdated { get; set; }
    public string SentToWehkamp { get; set; }
    public string SentToWehkampAsDummy { get; set; }
    public string ResendProductInformationToWehkamp { get; set; }
  }
}
