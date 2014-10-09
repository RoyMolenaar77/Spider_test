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
  public class ProductMappingBulk : IceCatHTTPLoader<ConcentratorDataContext>
  {
    private const string _localFileName = "product_mapping.xml";

    #region SQL strings

    public const string ProductMappingTableName = "[DataStaging].[ICECat_ProductMapping]";
    private string _productMappingCreate = String.Format(@"CREATE TABLE {0} (                                        
                                        [Supplier_ID] int NOT NULL,                                        
                                        [MatchProductID] nvarchar(100) NULL,
                                        [Prod_ID] nvarchar(100) NULL,
                                        [ProductID] int NULL,
                                        [CorrespondingProductID] int NULL,
                                      ) ON [PRIMARY]", ProductMappingTableName);

    private string _productMappingMergeStatement = String.Format(@"MERGE [dbo].[VendorProductMatch] as PM
                                                          USING {0} as IPM
                                                            ON (PM.VendorID =  {1} AND IPM.ProductID = PM.ProductID
AND (IPM.CorrespondingProductID = PM.VendorProductID OR IPM.MatchProductID = PM.VendorItemNumber))
                                                          WHEN NOT MATCHED BY TARGET and IPM.ProductID is not null
                                                            THEN 
                                                              INSERT (ProductID,VendorProductID, VendorItemNumber,VendorID, MatchPercentage) 
                                                              VALUES(IPM.ProductID,IPM.CorrespondingProductID, IPM.MatchProductID, {1}, 25);",
                                                              ProductMappingTableName, DataImport.VendorID);

    private string _productMappingMergeStatement2 = String.Format(@"MERGE [dbo].[VendorProductMatch] as PM
                                                          USING {0} as IPM
                                                            ON (PM.VendorID =  {1} AND IPM.CorrespondingProductID = PM.ProductID
AND (IPM.ProductID = PM.VendorProductID OR IPM.Prod_ID = PM.VendorItemNumber))
                                                          WHEN NOT MATCHED BY TARGET and IPM.CorrespondingProductID is not null
                                                            THEN 
                                                              INSERT (ProductID,VendorProductID, VendorItemNumber,VendorID, MatchPercentage) 
                                                              VALUES(IPM.CorrespondingProductID,IPM.ProductID, IPM.Prod_ID, {1}, 25);",
                                                              ProductMappingTableName, DataImport.VendorID);

    #endregion
    string url = "/export/freexml.int/product_mapping.xml";

    public ProductMappingBulk(string baseUri, string localCache, NetworkCredential credentials, bool openIceCat)
      : base(baseUri, credentials)
    {
      LocalCacheLocation = Path.Combine(localCache, _localFileName);

      if (!openIceCat)
        url = "/export/level4/INT/product_mapping.xml";
    }

    public string LocalCacheLocation { get; private set; }

    protected override IDataReader CreateReader()
    {
      var xmlReader = new XmlTextReader(new FileStream(LocalCacheLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 3000, FileOptions.SequentialScan));
      var reader = new ProductMappingFileReader();
      reader.Load(xmlReader);

      return reader;
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      context.ExecuteStoreCommand(_productMappingCreate);
      File.Delete(LocalCacheLocation);
      DownloadFile(url, LocalCacheLocation);
      //BasicUnzip.Unzip(LocalCacheLocation + ".gz", LocalCacheLocation);

      using (var xmlReader = new XmlTextReader(new FileStream(LocalCacheLocation, FileMode.Open, FileAccess.Read, FileShare.Read, 3000, FileOptions.SequentialScan)))
      {
        var reader = new ProductMappingFileReader();
        reader.Load(xmlReader);
        BulkLoad(ProductMappingTableName, 100, reader);
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      string update = (String.Format(@"MERGE {0} as [PI]
                                              USING (
                                                  SELECT P.VendorItemNumber, P.ProductID, BV.BrandID, BV.VendorBrandCode
                                                  FROM Product P
                                                    INNER JOIN BrandVendor BV ON (BV.VendorID = {1} AND BV.BrandID = P.BrandID AND BV.BrandID != -1)
                                                 ) as CP
                                              ON (CP.VendorItemNumber = [PI].Prod_ID AND [PI].Supplier_ID = CP.VendorBrandCode)
WHEN MATCHED
                                              THEN UPDATE SET 
                                                        [PI].productid = cp.productid
                                                    ;
                                            ", ProductMappingTableName, DataImport.VendorID));
      context.ExecuteStoreCommand(update);

      string update2 = (String.Format(@"MERGE {0} as [PI]
                                              USING (
                                                  SELECT P.VendorItemNumber, P.ProductID, BV.BrandID, BV.VendorBrandCode
                                                  FROM Product P
                                                    INNER JOIN BrandVendor BV ON (BV.VendorID = {1} AND BV.BrandID = P.BrandID AND BV.BrandID != -1)
                                                 ) as CP
                                              ON (CP.VendorItemNumber = [PI].MatchProductID AND [PI].Supplier_ID = CP.VendorBrandCode)
                                              WHEN MATCHED
	                                              THEN UPDATE SET 
                                                        [PI].CorrespondingProductID = (SELECT P.ProductID FROM Product P
                                                    INNER JOIN BrandVendor BV ON (BV.VendorID = {1} AND BV.BrandID = P.BrandID AND BV.BrandID != -1)
                                                    WHERE P.VendorItemNumber = [PI].MatchProductID and BV.VendorBrandCode = [PI].Supplier_ID)
                                                    ;
                                            ", ProductMappingTableName, DataImport.VendorID));
      context.ExecuteStoreCommand(update2);

      context.ExecuteStoreCommand(String.Format(@"DELETE FROM {0} where productid is null and correspondingproductid is null", ProductMappingTableName));

      context.ExecuteStoreCommand(_productMappingMergeStatement);
      context.ExecuteStoreCommand(_productMappingMergeStatement2);
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", ProductMappingTableName));
      //File.Delete(LocalCacheLocation);
    }

    private class ProductMappingFileReader : XmlDataReader
    {
      private const string Tag = "ProductMapping";
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
        row[0] = (int)element.Attribute("supplier_id");
        row[1] = element.Attribute("m_prod_id").Value;
        row[2] = element.Attribute("prod_id").Value;
      }
    }
  }

}
