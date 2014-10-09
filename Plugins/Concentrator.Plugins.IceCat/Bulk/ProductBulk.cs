using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Data.SqlClient;
using Concentrator.Objects.Environments;
using log4net;
using System.Threading;
using Concentrator.Objects.DataAccess.EntityFramework;

namespace Concentrator.Plugins.IceCat.Bulk
{
  public class ProductBulk : IceCatHTTPLoader<ConcentratorDataContext>
  {
    #region Queries

    public const string ProductSpecTable = "[DataStaging].[ICECAT_ProductSpecs]";
    private string _productSpecTableCreate = String.Format(@"CREATE TABLE {0} (
                                                                [ProdID] nvarchar(100) NOT NULL,
                                                                [SupplierID] int NOT NULL,
                                                                [LangID] INT NOT NULL,
                                                                [Value] NVarChar(3000) NOT NULL,
                                                                [ICAttributeID] INT NOT NULL,
                                                                [CatFeatureID] INT NOT NULL,
                                                                [CatFeatureGroupID] INT NOT NULL,
                                                                [ConcentratorProductID] INT NULL,
                                                                [ConcentratorAttributeID] INT NULL
                                                              ) ON [Primary]", ProductSpecTable);

    public const string ProductDescriptionTable = "[DataStaging].[ICECAT_ProductDescriptions]";
    private string _productDescTableCreate = String.Format(@"CREATE TABLE {0} (
                                                                    [LongDesc] NVarChar(2500) NOT NULL,
                                                                    [ShortDesc] NVarChar(1000) NOT NULL,
                                                                    [PDFUrl] NVarChar(500) NOT NULL,
                                                                    [Warranty] NVarChar(2500) NOT NULL,
                                                                    [Quality] NVarChar(300) NOT NULL,
                                                                    [LangID] INT NOT NULL,
                                                                    [ConcentratorProductID] INT NULL,
                                                                    [URL] NVarChar(500) NULL,
                                                                    [PDFSize] INT NULL
                                                                  ) ON [Primary]", ProductDescriptionTable);

    public const string BarcodeTable = "[DataStaging].[ICECAT_ProductBarcodes]";
    public string _barcodeTableCreate = String.Format(@"CREATE TABLE {0} (
                                                           [Barcode] NVARCHAR(50) NOT NULL,
                                                           [ProductID] INT NOT NULL
                                                        ) ON [Primary]
                                                       ", BarcodeTable);

    public const string RelatedTable = "[DataStaging].[ICECAT_RelatedProducts]";
    public string _relatedTableCreate = String.Format(@"CREATE TABLE {0} (
                                                          LeftProdID INT NOT NULL,
                                                          RightICProdID NVarChar(250) NOT NULL,
                                                          SupplierID int NOT NULL,
                                                          Reversed BIT NOT NULL,
                                                          Preferred BIT NOT NULL
                                                        ) ON [Primary]
                                                       ", RelatedTable);

    public const string ImageTable = "[DataStaging].[ICECAT_ExtraImages]";
    public string _imageTableCreate = String.Format(@"CREATE TABLE {0} (
                                                        ProductID INT NOT NULL,
                                                        Sequence INT NOT NULL,
                                                        [URL] NVarChar(250) NOT NULL
                                                      ) ON [Primary]", ImageTable);

    #endregion

    public ProductBulk(string baseUri, NetworkCredential credentials)
      : base(baseUri, credentials)
    {

    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      var query = String.Format(@"SELECT Prod_ID as ProdID, [path] as Path, BV.BrandID, ConcentratorID
                                  FROM {0}
                                    LEFT JOIN BrandVendor BV ON BV.VendorBrandCode = CAST(Supplier_id as nvarchar(10))",
                                ProductIndexBulk.ProductIndexTableName);

      var toProcess = context.ExecuteStoreQuery<ProductProcessResult>(query).Count();
      var _productsToProcess = context.ExecuteStoreQuery<ProductProcessResult>(query);

      int processed = 0;
      int progressReportIdx = toProcess / 20;


      context.ExecuteStoreCommand(_productSpecTableCreate);
      context.ExecuteStoreCommand(_productDescTableCreate);
      context.ExecuteStoreCommand(_barcodeTableCreate);
      context.ExecuteStoreCommand(_relatedTableCreate);
      context.ExecuteStoreCommand(_imageTableCreate);

      var options = new ParallelOptions
      {
        MaxDegreeOfParallelism = 8
      };

      Log.InfoFormat("About to download {0} individual product files", toProcess);

      try
      {
        // Processes all products
        Parallel.ForEach(_productsToProcess, options, (p, ls, idx) =>
        {
          XElement root;
          using (var file = new MemoryStream())
          {
            try
            {
              //var req = (HttpWebRequest)WebRequest.Create(new Uri(base.BaseURI, p.Path));
              //req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
              //req.Credentials = base.Credentials;

              //var t = req.DownloadDataAsync();
              ////.ContinueWith<byte[]>(data =>
              ////{
              ////  return data.Result;
              ////});
              //t.Start(Task.Factory.Scheduler);
              //t.Wait();
              //if (t.IsCompleted && !t.IsFaulted)
              //{
              //  file.Write(t.Result, 0, t.Result.Length);
              //}
              //else
              //  return;
              DownloadFile(p.Path, file);

              file.Position = 0;

              using (XmlTextReader reader = new XmlTextReader(file))
              {
                root = XDocument.Load(reader).Element("ICECAT-interface").Element("Product");
              }
            }
            catch (Exception ex)
            {
              Log.Error(String.Format("Failed to download {0} from: {1}", p.ProdID, p.Path), ex);
              return;
            }
          }

          #region Parse xml

          var baseSpec = new
          {
            HighPic = root.Attribute("HighPic").Value,
            ICECatID = (int)root.Attribute("ID"),
            Name = root.Attribute("Name").Value,
            ManufacturerID = root.Attribute("Prod_id").Value,
            Quality = root.Attribute("Quality").Value,
            ReleaseDate = root.Attribute("ReleaseDate").Value,
            ThumbPic = root.Attribute("ThumbPic").Value,
            ICECatCategory = (int)root.Element("Category").Attribute("ID"),
            SupplierID = (int)root.Element("Supplier").Attribute("ID")
          };

          var descriptions = from d in root.Elements("ProductDescription")
                             let l = d.Attribute("langid")
                             where l != null
                             let lang = (int)l
                             where DataImport.Languages.ContainsKey(lang)
                             select new
                             {
                               IceCatID = (int)d.Attribute("ID"),
                               LongDesc = d.Attribute("LongDesc").Value,
                               ManualPDFUrl = d.Attribute("ManualPDFURL").Value,
                               PDFSize = (int)d.Attribute("ManualPDFSize"),
                               ShortDesc = d.Attribute("ShortDesc").Value,
                               WarrantyInfo = d.Attribute("WarrantyInfo").Value,
                               ICECatLang = lang,
                               URL = d.Attribute("URL").Value
                             };

          var barcodes = from b in root.Elements("EANCode")
                         let eanNode = b.Attribute("EAN")
                         where eanNode != null
                         select eanNode.Value.Trim();

          var images = from i in root.Element("ProductGallery").Elements("ProductPicture")
                       select i.Attribute("Pic").Value;

          var specs = from f in root.Elements("ProductFeature")
                      where !f.IsEmpty
                      let signs = f.Try(e => e.Element("Feature").Element("Measure").Element("Signs"), null)
                      select new
                      {
                        CatFeatureID = (int)f.Attribute("CategoryFeature_ID"),
                        CatFeatureGroupID = (int)f.Attribute("CategoryFeatureGroup_ID"),
                        SpecID = (int)f.Element("Feature").Attribute("ID"),
                        ValID = (long)f.Attribute("ID"),
                        RawValue = f.Attribute("Value").Value,
                        PresentationValue = f.Attribute("Presentation_Value").Value,
                        Signs = signs.Try(s => from si in s.Elements("Sign")
                                               let langID = (int)si.Attribute("langid")
                                               where IceCat.DataImport.Languages.ContainsKey(langID)
                                               select new
                                               {
                                                 LangID = langID,
                                                 Value = si.Value
                                               }, null)
                      };

          var related = from r in root.Elements("ProductRelated")
                        where !r.IsEmpty
                        let prod = r.Element("Product")
                        where !prod.IsEmpty
                        let supplier = prod.Element("Supplier")
                        where supplier != null
                        select new
                        {
                          ProdID = prod.Attribute("Prod_id").Value,
                          SupplierID = (int)supplier.Attribute("ID"),
                          Preferred = (bool)r.Attribute("Preferred"),
                          Reversed = (bool)r.Attribute("Reversed")
                        };

          #endregion Parse xml

          using (var ctx = new ConcentratorDataContext())
          {
            using (var connection = new SqlConnection(Environments.Current.Connection))
            {
              connection.Open();

              var com = connection.CreateCommand();

              // Insert spec values in staging table
              com.CommandText = "INSERT INTO " + ProductSpecTable + " ([ProdID], [SupplierID], [LangID], [Value], [ICAttributeID], [CatFeatureID], [CatFeatureGroupID], [ConcentratorProductID]) VALUES(@ProdID, @SupplierID, @LangID, @Value, @AttributeID, @CatFeatureID, @CatFeatureGroupID, @ConProductID);";
              foreach (var spec in specs)
              {
                foreach (var lang in DataImport.Languages)
                {
                  com.Parameters.AddWithValue("ProdID", baseSpec.ManufacturerID);
                  com.Parameters.AddWithValue("SupplierID", baseSpec.SupplierID);
                  com.Parameters.AddWithValue("LangID", lang.Value);
                  com.Parameters.AddWithValue("Value", spec.RawValue.Cap(3000));
                  com.Parameters.AddWithValue("AttributeID", spec.SpecID);
                  com.Parameters.AddWithValue("ConProductID", p.ConcentratorID);
                  com.Parameters.AddWithValue("CatFeatureID", spec.CatFeatureID);
                  com.Parameters.AddWithValue("CatFeatureGroupID", spec.CatFeatureGroupID);

                  com.ExecuteNonQuery();
                  com.Parameters.Clear();
                }
              }

              com = connection.CreateCommand();
              // Insert descriptions in staging table
              com.CommandText = "INSERT INTO " + ProductDescriptionTable + " ([LongDesc], [ShortDesc], [PDFUrl], [Warranty], [Quality], [LangID], [ConcentratorProductID], PDFSize, [URL]) VALUES(@LongDesc, @ShortDesc, @PDFUrl, @Warranty, @Quality, @LangID, @ConcentratorProductID, @PDFSize, @URL);";
              foreach (var desc in descriptions)
              {
                com.Parameters.AddWithValue("LongDesc", desc.LongDesc.Cap(2500));
                com.Parameters.AddWithValue("ShortDesc", desc.ShortDesc.Cap(1000));
                com.Parameters.AddWithValue("PDFUrl", desc.ManualPDFUrl);
                com.Parameters.AddWithValue("Warranty", desc.WarrantyInfo.Cap(2500));
                com.Parameters.AddWithValue("LangID", desc.ICECatLang);
                com.Parameters.AddWithValue("Quality", baseSpec.Quality);
                com.Parameters.AddWithValue("ConcentratorProductID", p.ConcentratorID);
                com.Parameters.AddWithValue("PDFSize", desc.PDFSize);
                com.Parameters.AddWithValue("URL", desc.URL);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
              }

              com = connection.CreateCommand();
              // Insert barcodes in staging table
              com.CommandText = "INSERT INTO " + BarcodeTable + " (Barcode, ProductID) VALUES(@Barcode, @ProductID)";
              foreach (var barcode in barcodes)
              {
                com.Parameters.AddWithValue("Barcode", barcode);
                com.Parameters.AddWithValue("ProductID", p.ConcentratorID);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
              }

              // Insert related products in staging table
              com.CommandText = "INSERT INTO " + RelatedTable + " (LeftProdID, RightICProdID, SupplierID, Reversed, Preferred) VALUES(@LeftProdID, @RightICProdID, @SupplierID, @Reversed, @Preferred)";
              foreach (var rel in related)
              {
                com.Parameters.AddWithValue("LeftProdID", p.ConcentratorID);
                com.Parameters.AddWithValue("RightICProdID", rel.ProdID);
                com.Parameters.AddWithValue("SupplierID", rel.SupplierID);
                com.Parameters.AddWithValue("Reversed", rel.Reversed);
                com.Parameters.AddWithValue("Preferred", rel.Preferred);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
              }

              // Insert images in staging table
              com.CommandText = "INSERT INTO " + ImageTable + " (ProductID, Sequence, URL) VALUES(@ProductID, @Seq, @Url)";
              var imgs = images.ToList();
              for (int i = 0; i < imgs.Count; i++)
              {
                var img = imgs[i];
                com.Parameters.AddWithValue("ProductID", p.ConcentratorID);
                com.Parameters.AddWithValue("Seq", i + 1);
                com.Parameters.AddWithValue("Url", img);

                com.ExecuteNonQuery();
                com.Parameters.Clear();
              }
            }
          }

          Interlocked.Increment(ref processed);

          if (processed % (progressReportIdx <= 0 ? 1 : progressReportIdx) == 0)
            Log.InfoFormat("{0} Files downloaded and imported", processed);

        });
      }
      catch (AggregateException ex)
      {
        foreach (var inEx in ex.InnerExceptions)
        {
          Log.Fatal("Exception from ProductBulk loop: ", ex);
        }
        throw ex;
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format(@"UPDATE IP
                                             SET IP.ConcentratorAttributeID = IA.ConcentratorID
                                             FROM {1} IP
                                               INNER JOIN {2} IA ON IA.CatFeatureID = IP.CatFeatureID
                                             ", DataImport.VendorID, ProductSpecTable, AttributeMetaDataBulk.AttributeMetaTable));


      var specs = Task.Factory.StartNew(() =>
      {
        using (var connection = new SqlConnection(Environments.Current.Connection))
        {
          var command = connection.CreateCommand();
          command.CommandTimeout = 3600;

          // Sync specs
          command.CommandText = String.Format(@" MERGE 
	                                              ProductAttributeValue A
                                              USING 
	                                              {0} S
                                              ON 
	                                              (A.ProductID = S.ConcentratorProductID AND A.AttributeID = S.ConcentratorAttributeID AND A.LanguageID =S.[LangID])
                                              WHEN NOT MATCHED BY TARGET and S.ConcentratorAttributeID is not null
	                                              THEN 
		                                              INSERT (AttributeID, ProductID, [Value], LanguageID, CreatedBy, CreationTime) 
		                                              VALUES(S.ConcentratorAttributeID, S.ConcentratorProductID, S.[Value], S.[LangID], 1, GETDATE())
                                              WHEN MATCHED
	                                              THEN
		                                              UPDATE SET 
                                                    A.[Value] = S.[Value],
                                                    A.LastModifiedBy = 1,
                                                    A.LastModificationTime = GETDATE();", ProductSpecTable);
          connection.Open();
          command.ExecuteNonQuery();
        }
      });

      var barcode = Task.Factory.StartNew(() =>
      {
        using (var connection = new SqlConnection(Environments.Current.Connection))
        {
          var command = connection.CreateCommand();
          command.CommandTimeout = 3600;

          // Sync barcodes
          command.CommandText = String.Format(@"MERGE 
	                                             ProductBarcode B
                                             USING 
	                                             {0} I
                                             ON 
	                                             (B.Barcode = I.Barcode AND B.ProductID = I.ProductID)
                                             WHEN NOT MATCHED BY TARGET
	                                             THEN INSERT (Barcode, ProductID, VendorID, BarcodeType) VALUES(I.Barcode, I.ProductID,{1},0);", BarcodeTable, DataImport.VendorID);
          connection.Open();
          command.ExecuteNonQuery();
        }
      });

      var related = Task.Factory.StartNew(() =>
      {

        using (var connection = new SqlConnection(Environments.Current.Connection))
        {
          var command = connection.CreateCommand();
          command.CommandTimeout = 3600;

          // Sync related
          command.CommandText = String.Format(@"
                                            MERGE 
                                              RelatedProduct P
                                            USING 
                                              (
                                                SELECT RP.LeftProdID as ProductID, IDX.ConcentratorID AS RelatedProduct, RP.Preferred, RP.Reversed
                                                FROM {2} RP
	                                                INNER JOIN {0} IDX 
		                                                ON (RP.RightICProdID = IDX.Prod_ID AND RP.SupplierID = IDX.Supplier_id)
                                                 ) as I
                                            ON 
                                              (P.ProductID = I.ProductID AND P.RelatedProductID = I.RelatedProduct AND VendorID = {1})
                                            WHEN NOT MATCHED BY TARGET
                                              THEN
                                                INSERT (ProductID, RelatedProductID, Preferred, Reversed, CreationTime, CreatedBy, VendorID,relatedproducttypeid)
                                                VALUES(I.ProductID, I.RelatedProduct, I.Preferred, I.Reversed, GETDATE(), 1, {1}, 1)
                                            WHEN MATCHED
                                              THEN 
                                                UPDATE SET 
	                                                P.Preferred = I.Preferred,
	                                                P.Reversed = I.Reversed,
                                                  P.LastModifiedBy = 1,
                                                  P.LastModificationTime = GETDATE(),
                                                  P.relatedproducttypeid = 1
                                            WHEN NOT MATCHED BY SOURCE AND P.VendorID = {1}
                                              THEN DELETE;", ProductIndexBulk.ProductIndexTableName, DataImport.VendorID, RelatedTable);
          connection.Open();
          command.ExecuteNonQuery();
        }
      });

      var descriptions = Task.Factory.StartNew(() =>
      {

        using (var connection = new SqlConnection(Environments.Current.Connection))
        {
          var command = connection.CreateCommand();
          command.CommandTimeout = 3600;

          // Sync descriptions
          command.CommandText = String.Format(@"MERGE 
	                                              ProductDescription P
                                              USING
	                                              {0} I
                                              ON
	                                              (I.[LangID] = P.LanguageID AND I.ConcentratorProductID = P.ProductID AND P.VendorID = {1})
                                              WHEN NOT MATCHED BY TARGET
	                                              THEN
		                                              INSERT (ProductID, LanguageID, VendorID, ShortContentDescription, LongContentDescription, PDFUrl, PDFSize, URL, WarrantyInfo, Quality, CreatedBy, CreationTime)
		                                              VALUES (I.ConcentratorProductID, I.[LangID], {1}, I.ShortDesc, I.LongDesc, I.PDFUrl, I.PDFSize, I.URL, I.Warranty, I.Quality, 1, GETDATE())
                                              WHEN MATCHED 
	                                              THEN
		                                              UPDATE SET 
			                                              P.ShortContentDescription = I.ShortDesc,
			                                              P.LongContentDescription = I.LongDesc,
			                                              P.PDFUrl = I.PDFUrl,
			                                              P.PDFSize = I.PDFSize,
			                                              P.URL = I.URL,
			                                              P.WarrantyInfo = I.Warranty,
			                                              P.Quality = I.Quality,
			                                              P.LastModifiedBy = 1,
			                                              P.LastModificationTime = GETDATE()
                                              WHEN NOT MATCHED BY SOURCE and P.VendorID = {1}
	                                              THEN DELETE;", ProductDescriptionTable, DataImport.VendorID);

          connection.Open();
          command.ExecuteNonQuery();
        }
      });

      var images = Task.Factory.StartNew(() =>
      {
        using (var connection = new SqlConnection(Environments.Current.Connection))
        {
          var command = connection.CreateCommand();
          command.CommandTimeout = 3600;
          connection.Open();

          // Remove images
          //command.CommandText = String.Format("DELETE ProductImage WHERE VendorID = {0}", StagingImport.VendorID);
          //command.ExecuteNonQuery();

          // Insert main images from product index
          command.CommandText = String.Format(@"INSERT INTO {1} (ProductID, Sequence, [URL])
                                             SELECT ConcentratorID, 0, ImageURL
                                             FROM {0}
                                             WHERE ImageUrl != ''", ProductIndexBulk.ProductIndexTableName, ImageTable);
          command.ExecuteNonQuery();

          // Insert other images from staging table
          //          command.CommandText = String.Format(@"INSERT INTO {2} (ProductID, Sequence, VendorID, ImageUrl)
          //                                             SELECT ProductID, Sequence, {0}, URL
          //                                             FROM {1}", StagingImport.VendorID, ImageTable, ImageTable);
          //          command.ExecuteNonQuery();

          command.CommandText = string.Format(@"MERGE 
                                                  ProductMedia P
                                                USING
                                                  {0} I
                                                ON
                                                  (I.Sequence = P.Sequence AND I.ProductID = P.ProductID AND P.VendorID = {1} AND TypeID = 1)
                                                WHEN NOT MATCHED BY TARGET
                                                  THEN
                                                      INSERT (ProductID, Sequence, VendorID, MediaUrl, TypeID)
                                                      VALUES (I.ProductID, I.Sequence, {1}, I.[URL], 1)
                                                WHEN MATCHED and I.[URL] != P.MediaUrl
                                                  THEN
                                                      UPDATE SET 
                                                          P.MediaUrl = I.[URL],
                                                          P.MediaPath = null
                                                WHEN NOT MATCHED BY SOURCE and P.VendorID = {1}
                                                THEN DELETE;", ImageTable, DataImport.VendorID);

          command.ExecuteNonQuery();
        }
      });

      try
      {
        Task.WaitAll(specs, barcode, related, descriptions, images);
      }
      catch (AggregateException ex)
      {
        foreach (var inEx in ex.InnerExceptions)
        {
          Log.Fatal("Exception from ProductBulk sync: ", ex);
        }
        throw ex;
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", ProductSpecTable));
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", ProductDescriptionTable));
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", BarcodeTable));
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", RelatedTable));
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", ImageTable));
    }

    protected class ProductProcessResult
    {
      public string ProdID { get; set; }
      public int ConcentratorID { get; set; }
      public string Path { get; set; }
      public int BrandID { get; set; }
    }
  }
}
