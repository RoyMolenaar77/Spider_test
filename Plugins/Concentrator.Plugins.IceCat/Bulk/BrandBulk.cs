using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Xml;
using System.IO;
using System.Data;
using System.Net;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.IceCat.Bulk
{
  public class BrandBulk : IceCatHTTPLoader<ConcentratorDataContext>
  {
    private const string _localFileName = "BrandIndex.xml";

    #region SQL strings

    public const string BrandIndexTableName = "[DataStaging].[ICECat_BrandIndex]";
    private string _brandIndexCreate = String.Format(@"CREATE TABLE {0} (
                                        [IceCatBrandID] int NOT NULL,
                                        [Name] nvarchar(100) NULL,
                                        [Logo] nvarchar(255) NULL
                                      ) ON [PRIMARY]", BrandIndexTableName);

    private string _brandMergeStatement = String.Format(@"MERGE [dbo].[BrandVendor] as BV
                                                          USING {0} as IB
                                                          ON (BV.VendorID = {1} AND IB.IceCatBrandID = BV.VendorBrandCode)                                                                                 
                                                          WHEN NOT MATCHED BY TARGET
                                                          THEN 
                                                            INSERT (BrandID, VendorID, VendorBrandCode, Name) 
                                                            VALUES(ISNULL((SELECT top 1 BrandID FROM Brand B WHERE B.Name = [IB].Name), -1), {1}, [IB].IceCatBrandID, [IB].Name)
                                                          WHEN MATCHED
                                                          THEN UPDATE SET
                                                            [BV].[Name] = [IB].[Name];",
                                                              BrandIndexTableName, DataImport.VendorID);

    private string _brandMergeImages = String.Format(@"MERGE [dbo].[BRANDMEDIA] as BM
                                                        USING (select ib.*,b.brandid from {0} IB
                                                                Inner join Brand B on IB.IceCatBrandID = B.BrandID) as IB
                                                          ON (BM.VendorID = {1} AND IB.Brandid = BM.brandid and ib.logo = BM.MediaUrl) 
                                                        WHEN NOT MATCHED BY TARGET
                                                          THEN 
                                                            INSERT (BrandID, sequence, typeid, mediaurl) 
                                                            VALUES(IB.brandID,0,1,[IB].[Logo])
                                                        WHEN MATCHED
                                                          THEN UPDATE SET                                                              
                                                            [BM].[MediaUrl] = [IB].[Logo];
                                                      ", BrandIndexTableName, DataImport.VendorID);

    #endregion

    string url = "/export/freexml.int/refs/SuppliersList.xml.gz";

    public BrandBulk(string baseUri, string localCache, NetworkCredential credentials, bool openIceCat)
      : base(baseUri, credentials)
    {
      LocalCacheLocation = Path.Combine(localCache, _localFileName);

      if (!openIceCat)
        url = "/export/level4/refs/SuppliersList.xml.gz";
    }

    public string LocalCacheLocation { get; private set; }

    protected override IDataReader CreateReader()
    {
      var xmlReader = new XmlTextReader(new FileStream(LocalCacheLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 3000, FileOptions.SequentialScan));
      var reader = new BrandIndexFileReader();
      reader.Load(xmlReader);

      return reader;
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      context.ExecuteStoreCommand(_brandIndexCreate);
      DownloadFile(url, LocalCacheLocation + ".gz");
      BasicUnzip.Unzip(LocalCacheLocation + ".gz", LocalCacheLocation);

      using (var xmlReader = new XmlTextReader(new FileStream(LocalCacheLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 3000, FileOptions.SequentialScan)))
      {
        var reader = new BrandIndexFileReader();
        reader.Load(xmlReader);
        BulkLoad(BrandIndexTableName, 100, reader);
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(_brandMergeStatement);
      context.ExecuteStoreCommand(_brandMergeImages);
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", BrandIndexTableName));
      File.Delete(LocalCacheLocation);
    }

    private class BrandIndexFileReader : XmlDataReader
    {
      private const string Tag = "Supplier";
      private const int _fieldCount = 3;

      public override bool IsElement(XmlReader reader)
      {
        return reader.Name == Tag;
      }

      public override int FieldCount
      {
        get { return _fieldCount; }
      }

      protected override void FillRow(System.Xml.Linq.XElement element, object[] row)
      {
        row[0] = (int)element.Attribute("ID");
        row[1] = element.Attribute("Name").Value;
        row[2] = element.Attribute("LogoPic") != null ? element.Attribute("LogoPic").Value : null;
      }
    }
  }

}
