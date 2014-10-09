using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Parse;
using System.IO;
using System.Data;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ZipUtil;
using Concentrator.Objects.Models.Products;
using Ionic.Zip;
using System.Globalization;
using Concentrator.Objects.Sftp;

namespace Concentrator.Plugins.Ingram
{
  public class ProductImport : VendorBase
  {
    protected override int VendorID
    {
      get
      {
        return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value);
      }
    }

    protected override int DefaultVendorID
    {
      get
      {
        return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value);
      }
    }

    protected override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

    protected override void SyncProducts()
    {
      Process();
    }
    public override string Name
    {
      get { return "Ingram Product Import"; }
    }

    public string connectionString;

    private string[] AttributeMapping = new[] 
    { 
        #region Attributes
        "ISBN10",
        "IngramDepartmentCode",
        "EditionNumber",
        "EditionDescription",
        "AbridgedEditionFlag",
        "LargePrintFlag",
        "SeriesID",
        "SeriesNumber",
        "Contributor1",
        "Contributor1Role",
        "Contributor2",
        "Contributor2Role",
        "Contributor3",
        "Contributor3Role",
        "PublisherImprint",
        "BISACBindingType",
        "BISACMediaType",
        "BISACChildrensBookType",
        "StrippableIndicator",
        "IngramSubject",
        "BISACSubjectCode1",
        "BISACSubject1Description",
        "BISACSubjectCode2",
        "BISACSubject2Description",
        "BISACSubjectCode3",
        "BISACSubject3Description",
        "AudienceAgeMinimum",
        "AudienceAgeMaximum",
        "AudienceGradeMinimum",
        "AudienceGradeMaximum",
        "LibraryofCongressCardNumber",
        "DeweyDecimalClassificationNo",
        "LibraryofCongressSubjectHeading1",
        "LCSubjectHeading2",
        "Pages",
        "Playingtime",
        "Numberofunits",
        "Weight",
        "Length",
        "Width",
        "Height",
        "DumpDisplayFlag",
        "MonthlyAddChangeFlag",
        "WeeklyAddChangeFlag",
        "OnDemandPrintFlag",
        "LexileReadingLevel",
        "PublisherCode",
        "SpringArborDivisionFlag",
        "LeadingArticleFlag",
        "IllustrationFlag",
        "ContentLanguage",
        "SpringArborProductType",
        "SpringArborSubjectCategory",
        "AccessoryCode",
        "FileSize",
        "ImprintID",
        "DirectShipEligibleIndicator",
        "ContributorID1",
        "ContributorID2",
        "ContributorID3"
          
        #endregion
    };

    static IEnumerable<string> ReadFrom(string file)
    {
      string line;
      using (var reader = File.OpenText(file))
      {
        while ((line = reader.ReadLine()) != null)
        {
          yield return line;
        }
      }
    }

    public void Process()
    {
      var config = GetConfiguration();

      //Local directory files
      var LocalDir = config.AppSettings.Settings["LocalImportDir"].Value;

      if (!Directory.Exists(LocalDir))
        Directory.CreateDirectory(LocalDir);
#if DEBUG
      var activeTitlesFile = Path.Combine(LocalDir, "ttlingv2.zip");
      var activeTitlesUpdateFile = Path.Combine(LocalDir, "ttladdch.zip");
      var annotationsFile = Path.Combine(LocalDir, "antingrm-13.zip");
      var seriesFile = Path.Combine(LocalDir, "SERIES.txt");
      var categoriesFile = Path.Combine(LocalDir, "catg2.txt");
      var familiesFile = Path.Combine(LocalDir, "family-13.zip");
      var stockFile = Path.Combine(LocalDir, "stockv2@ingram.zip");
#else

      var Username = config.AppSettings.Settings["FtpUsername"].Value;
      var Password = config.AppSettings.Settings["FtpPassword"].Value;
      var FtpUrl = config.AppSettings.Settings["FtpUrl"].Value;


      FtpManager ftp = new FtpManager(FtpUrl, null, Username, Password, false, true, log);

      var categoriesFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["Categories"].Value);
      var annotationsFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["Annotations"].Value);
      var activeTitlesFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["Titles"].Value);
      var activeTitlesUpdateFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["TitlesUpdate"].Value);
      var seriesFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["Series"].Value);
      var familiesFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["Familys"].Value);
      var stockFile = ftp.DownloadToDisk(LocalDir, config.AppSettings.Settings["Stock"].Value);


#endif
      var VendorID = Int32.Parse(config.AppSettings.Settings["VendorID"].Value);
      var DefaultVendorID = Int32.Parse(config.AppSettings.Settings["VendorID"].Value);

      // var ftp.OpenFile(

      using (var unit = GetUnitOfWork())
      {
        #region Setup

        List<ProductAttributeMetaData> attributes;
        SetupAttributes(unit, AttributeMapping, out attributes, null);

        var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == DefaultVendorID).ToList();
        var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

        #endregion

        try
        {
#if !DEBUG
          FileStream activeFiles_Stream = File.Open(activeTitlesFile, FileMode.Open, FileAccess.Read, FileShare.None);
          var assortmentFile = Unzip(activeFiles_Stream, LocalDir, "ttlingv2.txt");
          FileStream annotations_Stream = File.Open(annotationsFile, FileMode.Open, FileAccess.Read, FileShare.None);
          var annotations = Unzip(annotations_Stream, LocalDir, "ANTINGRM.TXT");
          FileStream fam_Stream = File.Open(familiesFile, FileMode.Open, FileAccess.Read, FileShare.None);
          var families = Unzip(fam_Stream, LocalDir, "FAMILY-13.DAT");
          FileStream stock_Stream = File.Open(stockFile, FileMode.Open, FileAccess.Read, FileShare.None);
          var stocks = Unzip(stock_Stream, LocalDir, "stockv2@ingram.dat");
#else
 var assortmentFile = @"G:\Ingram\Import\ttlingv2.txt";
 var annotations = @"G:\Ingram\Import\ANTINGRM.TXT";
 var families = @"G:\Ingram\Import\FAMILY-13.DAT";
 var stocks = @"G:\Ingram\Import\stockv2@ingram.dat";
#endif

          #region Assortment
          var assortment = (from a in ReadFrom(assortmentFile)
                            where a.Length > 400
                            select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
                            {
                              #region BrandVendor
                              BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                {
                  new VendorAssortmentBulk.VendorImportBrand()
                  {
                      VendorID = DefaultVendorID,
                      VendorBrandCode = a.Substring(225, 40).Trim(), //UITGEVER_ID
                      ParentBrandCode = null,
                      Name = a.Substring(225, 40).Trim() //row["Publisher Imprint"].ToString().Trim() //UITGEVER_NM
                  }
                },
                              #endregion

                              #region GeneralProductInfo
                              VendorProduct = new VendorAssortmentBulk.VendorProduct
                              {
                                VendorItemNumber = a.Substring(442, 17).Trim(), //EAN
                                CustomItemNumber = a.Substring(442, 17).Trim(), //EAN
                                ShortDescription = a.Substring(161, 30).Trim(), //ShortTitle
                                LongDescription = "",
                                LineType = null,
                                LedgerClass = null,
                                ProductDesk = null,
                                ExtendedCatalog = null,
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                VendorBrandCode = a.Substring(225, 40).Trim(), //Contributor 1
                                Barcode = a.Substring(442, 17).Trim(), //EAN
                                VendorProductGroupCode1 = a.Substring(212, 9).Trim(),//Series ID 
                                VendorProductGroupCodeName1 = string.Empty,//a.Substring(221, 4).Trim(),//series number
                                VendorProductGroupCode2 = a.Substring(459, 4).Trim(), // Ingram subject
                                VendorProductGroupCodeName2 = "" //GEEN NAAM                        
                              },
                              #endregion

                              #region RelatedProducts

                              RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),//getRelatedProducts(row, families, VendorID),

                              #endregion

                              #region Attributes
                              VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),
                              #endregion

                              #region Images
                              VendorImportImages = new List<VendorAssortmentBulk.VendorImportImage>(),
                              #endregion

                              #region Prices
                              VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = a.Substring(442, 17).Trim(), //EAN
                                Price =  "0", //row.Price.ToString().Trim(), //ADVIESPRIJS
                                CostPrice = "0", //row.Price.ToString().Trim(), //NETTOPRIJS
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = ""//STADIUM_LEVENSCYCLUS_KD
                            }
                        },
                              #endregion

                              #region Stock
                              VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = a.Substring(442, 17).Trim(), //EAN
                                QuantityOnHand = 0,
                                StockType = "Assortment",
                                StockStatus = ""//GetProductStatusName(row.Price["Publisher Status Code"].ToString().Trim())//STADIUM_LEVENSCYCLUS_KD
                            }
                        },
                              #endregion
                            }).Take(100000);

          using (VendorAssortmentBulk bulkExport = new VendorAssortmentBulk(assortment, VendorID, DefaultVendorID))
          {
            bulkExport.IncludeBrandMapping = true;
            bulkExport.Init(unit.Context);
            bulkExport.Sync(unit.Context);
          }
          #endregion

          #region RawBulk

          var Description = (from r in ReadFrom(annotations)
                             select new tempDescriptionsModel
                             {
                               EAN = r.Substring(0, 12).Trim(),
                               Text = r.Substring(15).Trim()
                             });


          var Category = (from r in ReadFrom(categoriesFile)
                          select new tempCategoriesModel
                          {
                            IngramSubjectCode = r.Substring(1, 4).Trim(),
                            IngramSubjectDescription = r.Substring(8, 59).Trim()
                          });


          var RelatedProduct = (from r in ReadFrom(families)
                                select new tempRelatedProductsModel
                                {
                                  EAN = r.Substring(0, 12).Trim(),
                                  FamilyID = r.Substring(13, 7).Trim(),
                                  ItemID = r.Substring(20, 3).Trim()
                                });

          var Series = (from r in ReadFrom(seriesFile)
                        select new tempSeriesModel
                        {
                          SeriesID = r.Substring(1, 9).Trim(),
                          Text = r.Substring(13).Replace('"', ' ')
                        });



          var StockandPrice = (from r in ReadFrom(stocks)
                               select new tempStockandPriceModel
                               {
                                 EAN = r.Substring(1, 13),
                                 Price = ((Int32.Parse(r.Substring(150, 7)) * 0.75) / 100).ToString("0.00", CultureInfo.InvariantCulture),
                                 CostPrice = ((Int32.Parse(r.Substring(157, 7)) * 0.75) / 100).ToString("0.00", CultureInfo.InvariantCulture),
                                 QuantityOnHand = (Int32.Parse(r.Substring(38, 7).Trim()) +
                                                  Int32.Parse(r.Substring(45, 7).Trim()) +
                                                  Int32.Parse(r.Substring(52, 7).Trim()) +
                                                  Int32.Parse(r.Substring(59, 7).Trim())).ToString(),
                                 CommercialStatus = GetProductStatusName(r.Substring(178, 2).ToString().Trim())
                               });


          var rawDate = new RawBulkModel()
          {
            Categories = Category,
            Descriptions = Description,
            RelatedProducts = RelatedProduct,
            Series = Series,
            StockandPrice = StockandPrice
          };

          using (RawBulk rawbulk = new RawBulk(rawDate, VendorID))
          {
            
            rawbulk.Init(unit.Context);
            rawbulk.Sync(unit.Context);
          }

          #endregion

        }
        catch (Exception ex)
        {

        }

      }
    }

    private string Download(string LocalDir, FtpManager.RemoteFile remoteFile)
    {
      log.AuditInfo("Downloading file: " + remoteFile.FileName);

      var savePath = Path.Combine(LocalDir, remoteFile.FileName);
      try
      {
        FileStream file = File.Create(savePath);
        remoteFile.Data.CopyTo(file);
        file.Close();
        log.AuditInfo("Done downloading file: " + remoteFile.FileName);
      }
      catch (Exception e)
      {
        log.AuditError(e.Message, e);
      }
      return savePath;
    }

    private string Unzip(Stream stream, string saveDirectory = "", string fileName = "")
    {

//#if DEBUG
//      return Path.Combine(saveDirectory, fileName);
//      //FileStream fileeStream = new FileStream(Path.Combine(saveDirectory, fileName), FileMode.Open, FileAccess.Read, FileShare.None);
//      //return fileeStream;
//#endif

      var zipProc = new ZipProcessor(stream);

      var zippie = zipProc.File;
      zippie.ExtractProgress += new EventHandler<Ionic.Zip.ExtractProgressEventArgs>(zippie_ExtractProgress);

      if (saveDirectory != "")
      {
        // zippie.sa
        zippie[fileName].Extract(saveDirectory, ExtractExistingFileAction.OverwriteSilently);

        FileStream fileStream = new FileStream(Path.Combine(saveDirectory, fileName), FileMode.Open, FileAccess.Read, FileShare.None);
        //return fileStream;
      }

      //foreach (var file in zipProc)
      //{
      //  return file.Data;
      //}
      return Path.Combine(saveDirectory, fileName);
    }

    void zippie_ExtractProgress(object sender, Ionic.Zip.ExtractProgressEventArgs e)
    {
      log.Info(string.Format("Saved {0} from {1}", e.BytesTransferred, e.TotalBytesToTransfer));
    }

    private string GetProductTypeName(string code)
    {
      switch (code)
      {
        case "K": return "Video"; ;
        case "M": return "Gifts,Cards, other non-book sideline items, & Church Supplies";
        case "N": return "Fixtures";
        case "P": return "Mass Market Paperback";
        case "Q": return "Quality or Trade Paperback";
        case "R": return "Cloth or Hardcover";
        case "S": return "Computer Software or Multimedia";
        case "T": return "Calendars, Maps, blank books, and other book-like sideline items";
        case "W": return "Audio Cassette";
        default: return null;
      }
    }

    private string GetProductStatusName(string code)
    {
      switch (code)
      {
        case "AB": { return "Canceled"; }
        case "CS": { return "Availability Uncertain"; }
        case "EX": { return "No longer stocked by Ingram"; }
        case "IP": { return "In print and available"; }
        case "NY": { return "Not yet published"; }
        case "OI": { return "Out of stock indefinitely"; }
        case "OP": { return "Out of print"; }
        case "PP": { return "Postponed indefinitely"; }
        case "RF": { return "Referred to another supplier"; }
        case "RM": { return "Remaindered"; }
        case "TP": { return "Temporarily out of stock because publisher cannot supply"; }
        case "WS": { return "Withdrawn from Sale"; }
        default: return null;
      }
    }
  }
}
