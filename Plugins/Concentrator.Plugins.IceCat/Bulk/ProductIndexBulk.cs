using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Concentrator.Objects;
using System.Net;
using System.Data;
using System.Xml;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.IceCat.Bulk
{
  public class ProductIndexBulk : IceCatHTTPLoader<ConcentratorDataContext>
  {
    private const string _localFileName = "ProductListINT.xml";

    #region Query vars

    public const string ProductIndexTableName = "[DataStaging].[ICECAT_ProductIndex]";
    private string _productIndexCreate = String.Format(@"CREATE TABLE {0} (
	                                            [path] [nvarchar](150) NOT NULL,
	                                            [Prod_ID] [nvarchar](250) NULL,
	                                            [Model_Name] [nvarchar](2500) NULL,
	                                            [Product_ID] [int] NOT NULL,
	                                            [Updated] [nvarchar](50) NULL,
	                                            [Cat_ID] [int] NULL,
	                                            [Quality] [nvarchar](50) NULL,
                                              [ImageURL] nvarchar(250) NOT NULL,
	                                            [Supplier_id] [int] NULL,
                                              [ConcentratorID] INT NULL,
                                              [ConcentratorBrandID] INT NULL
                                            ) ON [PRIMARY];", ProductIndexTableName);

    #endregion

    string url = "/export/freexml.int/INT/files.index.xml";
    log4net.ILog _log;

    public ProductIndexBulk(string baseUri, string localCache, NetworkCredential credentials, bool openIceCat, log4net.ILog log)
      : base(baseUri, credentials)
    {
      _log = log;
      LocalCacheLocation = Path.Combine(localCache, _localFileName);

      if (!openIceCat)
      {
        url = "/export/level4/INT/files.index.xml";
      }

      _log.InfoFormat("Used Icecat URL: {0}", url);
    }

    public string LocalCacheLocation { get; private set; }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      context.ExecuteStoreCommand(_productIndexCreate);
      DownloadFile(url, LocalCacheLocation);
      BulkLoad(ProductIndexTableName);
    }

    public override void Sync(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(@"CREATE NONCLUSTERED INDEX IX_ConcentratorID
ON [DataStaging].[ICECAT_ProductIndex] ([ConcentratorID])");
            
      context.ExecuteStoreCommand(String.Format(@"
                                             MERGE {0} as [PI]
                                              USING (
   SELECT P.VendorItemNumber, P.ProductID, BV.BrandID, BV.VendorBrandCode
   FROM Product P
   INNER JOIN BrandVendor BV ON (BV.VendorID ={1} AND BV.BrandID = P.BrandID AND BV.BrandID != -1)
                                                 ) as CP
                                              ON (CP.VendorItemNumber = [PI].Prod_ID AND [PI].Supplier_ID = CP.VendorBrandCode)
                                              WHEN MATCHED
	                                              THEN UPDATE SET 
                                                        [PI].ConcentratorID = CP.ProductID,
                                                        [PI].ConcentratorBrandID = CP.BrandID;
                                            ", ProductIndexTableName, DataImport.VendorID));

      context.ExecuteStoreCommand(String.Format(@" MERGE {0} as [PI]
                                              USING (
   SELECT DISTINCT PM.VendorItemNumber, P.ProductID, BV.BrandID, BV.VendorBrandCode
      FROM Product P
        INNER JOIN BrandVendor BV ON (BV.VendorID =38 AND BV.BrandID = P.BrandID AND BV.BrandID != -1)
        inner JOIN VendorProductMatch PM ON (PM.ProductID = P.ProductID and pm.vendoritemnumber is not null )
       where pm.VendorItemNumber not in (
       select T1.VendorItemNumber
from VendorProductMatch T1, VendorProductMatch T2
where T1.vendoritemnumber = T2.vendoritemnumber
and T1.productid > T2.productid
)and p.productid not in (
select concentratorid from  [DataStaging].[ICECAT_ProductIndex] where concentratorid is not null
)
) as CP
ON (CP.VendorItemNumber = [PI].Prod_ID AND [PI].Supplier_ID = CP.VendorBrandCode)
                                                   WHEN NOT MATCHED BY SOURCE and [PI].ConcentratorID is null
	                                              THEN DELETE
                                                    WHEN MATCHED
	                                              THEN UPDATE SET 
                                                        [PI].ConcentratorID = CP.ProductID,
                                                        [PI].ConcentratorBrandID = CP.BrandID; ", ProductIndexTableName, DataImport.VendorID));


      context.ExecuteStoreCommand(@"delete T1
from DataStaging.ICECAT_ProductIndex T1, DataStaging.ICECAT_ProductIndex T2
where T1.concentratorid = T2.concentratorid
and T1.product_id != T2.product_id");

      //      context.ExecuteCommand(String.Format(@"DELETE IP
      //                                             FROM {0} IP
      //                                               LEFT JOIN BrandVendor B ON (B.VendorBrandCode = IP.Supplier_ID AND B.VendorID = {1})
      //                                               LEFT JOIN Product P ON (P.VendorItemNumber = IP.Prod_ID AND P.BrandID = B.BrandID)
      //                                             WHERE (B.BrandID IS NULL OR B.BrandID = -1) OR P.ProductID IS NULL", ProductIndexTableName, StagingImport.VendorID));


      //      context.ExecuteCommand(String.Format(@"
      //                                          UPDATE IP
      //                                          SET IP.ConcentratorID = CP.ProductID
      //                                          FROM {0} IP
      //                                            LEFT JOIN BrandVendor B ON (B.VendorBrandCode = IP.Supplier_ID AND B.VendorID = {1})
      //                                            LEFT JOIN Product P ON (P.VendorItemNumber = IP.Prod_ID AND P.BrandID = B.BrandID)
      //                                          WHERE (B.BrandID IS NULL OR B.BrandID = -1) OR P.ProductID IS NULL
      //                                          ", ProductIndexTableName, StagingImport.VendorID));


    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", ProductIndexTableName));
      File.Delete(LocalCacheLocation);
    }

    protected override IDataReader CreateReader()
    {
      var xmlReader = new XmlTextReader(new FileStream(LocalCacheLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 3000, FileOptions.SequentialScan));
      var reader = new ProductIndexFileReader();
      reader.Load(xmlReader);

      return reader;
    }

    private class ProductIndexFileReader : XmlDataReader
    {
      private const string Tag = "file";
      private const int _fieldCount = 9;

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
        row[0] = element.Attribute("path").Value;
        row[1] = element.Attribute("Prod_ID").Value;
        row[2] = element.Attribute("Model_Name").Value;
        row[3] = element.Attribute("Product_ID").Value;
        row[4] = element.Attribute("Updated").Value;
        row[5] = element.Attribute("Catid").Value;
        row[6] = element.Attribute("Quality").Value;
        row[7] = element.Attribute("HighPic").Value;
        row[8] = element.Attribute("Supplier_id").Value;
      }
    }

  }
}
