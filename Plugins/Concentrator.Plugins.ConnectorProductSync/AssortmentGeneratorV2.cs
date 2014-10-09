using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Vendors.Base;
using Concentrator.Plugins.ConnectorProductSync.Models;

using PetaPoco;
using Concentrator.Objects.Assortment;


namespace Concentrator.Plugins.ConnectorProductSync
{
  public class AssortmentGeneratorV2 : ConcentratorPlugin
  {
    ///<summary>
    ///If this setting is supplied, the process will check in the MasterGroupMappingProductVendor table for the specific vendor/product/mastergroupmapping combo
    /// This is used whenever we want to support different product/mastergroupmapping combinations per vendor.
    /// E.G: A product which is in the SALE category for Vendor Country NL but not for Vendor Country BE
    /// </summary>
    private const string MATCH_MASTERGROUPMAPPINGPRODUCTS_PER_VENDOR_SETTING_KEY = "MatchMasterGroupMappingProductVendor";

    public override string Name
    {
      get { return "Assortment generator v2"; }
    }

    public bool InsertProductsToAllGroups = false;

    protected override void Process()
    {
      using (var database = new Database(Connection, Database.MsSqlClientProvider))
      {
        database.CommandTimeout = 30 * 60;

        var connectors = database
          .Query<Connector>(@"SELECT * FROM [dbo].[Connector] WHERE [IsActive] = 1 AND (([ConnectorType] & 2 = 2) or ([ConnectorType] & 4 = 4))")
          .ToArray();

        foreach (var connector in connectors)
        {
#if DEBUG
          if (connector.ConnectorID != 6) continue;
#endif

          log.Info("Starting generate assortment for " + connector.Name);

          InsertProductsToAllGroups = (database
              .FirstOrDefault<String>(String.Format(@"SELECT [Value] FROM [dbo].[ConnectorSetting] WHERE [ConnectorID] = {0} AND [SettingKey] = 'InsertProductsToAllGroups'"
              , connector.ConnectorID)) ?? Boolean.FalseString)
            .ParseToBool()
            .GetValueOrDefault();

          try
          {
            ProcessContent(connector.ConnectorID, database);
          }
          catch (Exception e)
          {
            log.AuditError("Generate assortment failed for " + connector.Name, e, ProcessType);
          }

          log.Info("Finished generate assortment for " + connector.Name);
        }
      }
    }

    private void ProcessContent(Int32 connectorID, Database database)
    {
      var connectorSettings = database
        .Query<ConnectorSetting>("SELECT [SettingKey], [Value] FROM [dbo].[ConnectorSetting] WHERE [ConnectorID] = @0", connectorID)
        .ToDictionary(connectorSetting => connectorSetting.SettingKey, connectorSetting => connectorSetting.Value);

      var excludeProducts = connectorSettings
        .GetValueOrDefault("ExcludeProducts", Boolean.FalseString)
        .ParseToBool()
        .GetValueOrDefault();

      /// If this setting is supplied, the process will check in the MasterGroupMappingProductVendor table for the specific vendor/product/mastergroupmapping combo
      /// This is used whenever we want to support different product/mastergroupmapping combinations per vendor.
      /// E.G: A product which is in the SALE category for Vendor Country NL but not for Vendor Country BE
      var checkProductMappingPerVendor = connectorSettings
        .GetValueOrDefault(MATCH_MASTERGROUPMAPPINGPRODUCTS_PER_VENDOR_SETTING_KEY, Boolean.FalseString)
        .ParseToBool()
        .GetValueOrDefault();

      var products = GetProductsByConnector(connectorID, database, excludeProducts);
      var tableName = string.Format("Temp_Content_Product_{0}", connectorID);

      SyncContent(connectorID, database, products, tableName);

      var masterGroupMappings = database
        .Query<MasterGroupMapping>(@"
SELECT DISTINCT [MasterGroupMappingID], [ParentMasterGroupMappingID], [ProductGroupID], [Score], [FlattenHierarchy], [FilterByParentGroup]
FROM [dbo].[MasterGroupMapping] AS MGM
LEFT JOIN [dbo].[Connector] AS C ON MGM.[ConnectorID] = C.[ParentConnectorID]
WHERE MGM.[ConnectorID] = @0 OR C.[ConnectorID] = @0", connectorID)
        .ToArray();

      foreach (var masterGroupMapping in masterGroupMappings)
      {
        var settingValues = database.Query<MasterGroupMappingSettingValue, MasterGroupMappingSetting>(string.Format(@"SELECT * 
                   FROM MasterGroupMappingSettingValue MGMSV 
                   INNER JOIN MasterGroupMappingSetting MGMS ON MGMSV.MasterGroupMappingSettingID = MGMS.MasterGroupMappingSettingID
                   WHERE MGMSV.MasterGroupMappingID = {0}", masterGroupMapping.MasterGroupMappingID));
        
        masterGroupMapping.MasterGroupMappingSettingValues = settingValues.ToList();
      }

      // If a product is matched, include the matched product in the categories of the original product
      // TODO : refactor
      var baseMasterGroupMappingProduct = database
        .Query<dynamic>(@";
with
original as 
(
  select distinct mgm.MasterGroupMappingID, tempContent.ProductID
  from MasterGroupMapping mgm
  inner join mastergroupmappingproduct mpg on mpg.mastergroupmappingid = mgm.SourceMasterGroupMappingID
  inner join Content tempContent on tempContent.productid = mpg.productid and @0 = tempcontent.connectorid
  left join connector c on mgm.ConnectorID = c.ParentConnectorID
  inner join ConnectorPublicationRule cpr on cpr.connectorpublicationruleid = tempContent.ConnectorPublicationRuleID
  left join MasterGroupMappingProductVendor mpv on mpv.productid = tempContent.productid and (mgm.sourcemastergroupmappingid is null or mpv.mastergroupmappingid = mgm.SourceMasterGroupMappingID)and mpv.vendorid = cpr.vendorid
  where (mgm.connectorid = @0 or c.ConnectorID = @0) and mpg.IsProductMapped = 1 and (@1 = 0 or (@1 =1 and mpv.vendorid is not null))
),
productTranslation as 
(
  select pm.productmatchid, pm.productid, pm2.productid as ShouldBeProductID, c2.connectorid
  from productmatch pm
  left join content c on pm.productid = c.productid  and c.connectorid = @0
  join productmatch pm2 on pm2.ProductMatchID = pm.ProductMatchID   and pm2.productid != pm.productid
  join content c2 on pm2.productid = c2.productid 
  where c2.connectorid = @0 and pm.isMatched = 1 and pm.MatchStatus = 2 and pm2.isMatched = 1 and pm2.MatchStatus = 2
)
select distinct mgm.MasterGroupMappingID,  pt.ShouldbeProductID as ProductID
from 
MasterGroupMapping mgm
inner join mastergroupmappingproduct mpg on mpg.mastergroupmappingid = mgm.SourceMasterGroupMappingID
inner join productTranslation pt on pt.connectorid = mgm.connectorid and pt.productid = mpg.productid
left join connector c on mgm.ConnectorID = c.ParentConnectorID
where (mgm.connectorid = @0 or c.ConnectorID = @0) and mpg.IsProductMapped = 1
union  
select * from original"
        , connectorID
        , Convert.ToInt32(checkProductMappingPerVendor))
        .ToList();

      var productsPerMasterGroupMapping = (
        from p in baseMasterGroupMappingProduct
        group p by (int)p.MasterGroupMappingID into groups
        select new
        {
          MasterGroupMappingID = (int)groups.Key,
          ProductIDs = (from p in groups select (int)p.ProductID).ToList()
        }).ToDictionary(c => c.MasterGroupMappingID, c => c.ProductIDs);


      var productsMasterGroupMapping = (
        from p in baseMasterGroupMappingProduct
        group p by (int)p.ProductID into groups
        select new
        {
          ProductID = (int)groups.Key,
          MasterGroupMappingIDs = (from p in groups select (int)p.MasterGroupMappingID).ToList()
        }).ToDictionary(c => c.ProductID, c => c.MasterGroupMappingIDs);

      var tree = BuildTree(masterGroupMappings);

      AddProductsToTree(tree, productsPerMasterGroupMapping, productsMasterGroupMapping);

      var toSync = tree.FlattenTreeToList(connectorID, 1);

      SyncNewContentProductGroups(toSync, database, connectorID, tableName);

      database.Execute(string.Format("DROP TABLE {0}", tableName));
    }

    private int getProductGroupMappingID(Database db, int connectorID)
    {
      string getProductGroupQuery = "select top 1 * from ProductGroup";

      ProductGroup productGroup = db.Query<ProductGroup>(getProductGroupQuery).FirstOrDefault();

      if (productGroup == null)
      {
        var createProductGroupQuery = @"insert into ProductGroup
                                          values(0, 0, NULL)";

        db.Execute(createProductGroupQuery);

        productGroup = db.Query<ProductGroup>(getProductGroupQuery).FirstOrDefault();
      }

      string getMasterGroupmappingQuery = @"select top 1 * from MasterGroupMapping";

      MasterGroupMapping masterGroupMapping = db.Query<MasterGroupMapping>(getMasterGroupmappingQuery).FirstOrDefault();

      if (masterGroupMapping == null)
      {
        log.AuditError("Cannot create an assortment without a master group mapping", ProcessType);

        throw new Exception();

      }


      string getProductGroupMappingQuery = "select top 1 * from ProductGroupMapping";

      ProductGroupMapping productGroupMapping = db.Query<ProductGroupMapping>(getProductGroupMappingQuery).FirstOrDefault();

      if (productGroupMapping == null)
      {
        var createProductGroupQuery = string.Format(@"insert into ProductGroupMapping( ConnectorID, ProductGroupID, FlattenHierarchy, FilterByParentGroup, MasterGroupMappingID)
                                                      values({0}, {1}, 1, 1, {2} )", connectorID, productGroup.ProductGroupID, masterGroupMapping.MasterGroupMappingID);

        db.Execute(createProductGroupQuery);

        productGroupMapping = db.Query<ProductGroupMapping>(getProductGroupMappingQuery).FirstOrDefault();
      }


      return productGroupMapping.ProductGroupMappingID;

    }

    private void SyncNewContentProductGroups(List<ContentProductGroupModel> newCpg, Database db, int connectorID, string contentTableName)
    {
      string tableName = string.Format("Temp_Content_Product_Group_{0}", connectorID);
      try
      {
        int productGroupmappingID = getProductGroupMappingID(db, connectorID);

        db.Execute(string.Format(@"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = '{0}'))
BEGIN
    drop table {0}
END
", tableName));


        var q = string.Format(@"CREATE TABLE {0}(
                                ProductID int not null,
                                ConnectorID int not null,                                
                                MasterGroupMappingID int not null,                                
                                CreatedBy int not null)", tableName);
        db.Execute(q); //create temp
        using (var connection = new SqlConnection(Connection))
        {
          connection.Open();
          using (SqlBulkCopy copyBulk = new SqlBulkCopy(connection))
          {
            copyBulk.BatchSize = 100000;
            copyBulk.BulkCopyTimeout = 180;
            copyBulk.DestinationTableName = tableName;
            copyBulk.NotifyAfter = 100000;
            copyBulk.SqlRowsCopied += (s, e) => log.DebugFormat("{0} Records inserted ", e.RowsCopied);

            using (var collection = new GenericCollectionReader<ContentProductGroupModel>(newCpg))
            {
              copyBulk.WriteToServer(collection);
            }
          }
        }

        db.Execute(string.Format(@"MERGE ContentProductGroup trg using 
                  (
                  select t.* from {0} t 
                  inner join content c on c.productid= t.productid and t.connectorid = c.connectorid

                  ) src
                                   on src.connectorid = trg.connectorid and src.productid = trg.productid and src.MasterGroupMappingID = trg.MasterGroupMappingID
                                    when not matched by target
                                         then insert (productid, connectorid, MasterGroupMappingID, createdby, [exists], productgroupmappingid)
                                         values (src.productid, src.connectorid, src.MasterGroupMappingID, src.createdby, 1, {2})
                                    when not matched by source and trg.connectorid = {1} and trg.iscustom = 0
                                          then delete
                ;", tableName, connectorID, productGroupmappingID));

        db.Execute(string.Format(@"merge mastergroupmappingproduct trg
                                    using (
	                                    select t.mastergroupmappingid, t.productid, tc.ConnectorPublicationRuleID
	                                    from {0} t 
	                                    inner join {1} tc on t.productid = tc.productid and t.connectorid = tc.connectorid

                                    ) src
                                    on src.mastergroupmappingid = trg.mastergroupmappingid and src.productid = trg.productid
                                    when not matched by target
                                    then
	                                    insert (Mastergroupmappingid, productid, isapproved, iscustom, isproductmapped, connectorpublicationruleid)
	                                    values (src.Mastergroupmappingid, src.productid, 0, 0, 1, src.connectorpublicationruleid)
                                    when not matched by source and iscustom = 0 and trg.mastergroupmappingid in (select mastergroupmappingid from mastergroupmapping where connectorid = {2})
	                                    then delete;", tableName, contentTableName, connectorID));

        db.Execute(string.Format("Drop table {0}", tableName));
      }
      catch (Exception e)
      {
        log.Debug("Synchronization of content product groups failed", e);
        throw e;
      }
    }

    private void AddProductsToTree(AssortmentTree tree, Dictionary<int, List<int>> productsPerProductGroupMapping, Dictionary<int, List<int>> productMapping)
    {
      foreach (var mappingGroup in productsPerProductGroupMapping)
      {
        tree.AddProducts(mappingGroup.Value, mappingGroup.Key, productMapping, InsertProductsToAllGroups);
      }
    }

    private AssortmentTree BuildTree(IEnumerable<MasterGroupMapping> mappings)
    {
      var tree = new AssortmentTree();

      foreach (var mapping in mappings.Where(c => c.ParentMasterGroupMappingID == null).ToList())
      {
        var retainProducts = false;
        var retainProductsSetting = mapping.MasterGroupMappingSettingValues.FirstOrDefault(x => x.MasterGroupMappingSetting.Name == "Retain Products");
        if (retainProductsSetting != null)
          bool.TryParse(retainProductsSetting.Value, out retainProducts);

        tree.AddToTree(mapping.MasterGroupMappingID, flattenHierarchy: mapping.FlattenHierarchy, filterByParent: mapping.FilterByParentGroup, retainProducts: retainProducts);

        foreach (var childMapping in mappings.Where(c => c.ParentMasterGroupMappingID == mapping.MasterGroupMappingID))
        {
          AddChild(tree, childMapping, mappings);
        }
      }

      return tree;
    }

    private void AddChild(AssortmentTree tree, MasterGroupMapping current, IEnumerable<MasterGroupMapping> mappings)
    {
      var retainProducts = false;
      var retainProductsSetting = current.MasterGroupMappingSettingValues.FirstOrDefault(x => x.MasterGroupMappingSetting.Name == "Retain Products");
      if (retainProductsSetting != null)
        bool.TryParse(retainProductsSetting.Value, out retainProducts);

      tree.AddToTree(current.MasterGroupMappingID, current.ParentMasterGroupMappingID, flattenHierarchy: current.FlattenHierarchy, filterByParent: current.FilterByParentGroup, retainProducts: retainProducts);

      foreach (var childMapping in mappings.Where(c => c.ParentMasterGroupMappingID == current.MasterGroupMappingID))
      {
        AddChild(tree, childMapping, mappings);
      }
    }

    private void SyncContent(int connectorID, Database db, List<VendorProductInfoWithWehkampInformation> products, string tableName)
    {
      var productsConverted = (from p in products
                               select p as VendorProductInfo).ToList();

      db.Execute(string.Format(@"IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = '{0}'))
                BEGIN
                    drop table {0}
                END
                ", tableName));


      var q = string.Format(@"CREATE TABLE {0}(
                                ProductID int not null,
                                ConnectorID int not null,                                
                                ConnectorPublicationRuleID int not null,
                                ShortDescription nvarchar(2000) null,          
                                LongDescription  nvarchar(max) null,         
                                LineType nvarchar(50) null,                  
                                LedgerClass nvarchar(50) null,               
                                ProductDesk nvarchar(50) null,              
                                ExtendedCatalog nvarchar(20) null,
                                ProductMatchID int null,
                                PublicationRuleIndex int not null
                                
                                )", tableName);
      db.Execute(q); //create temp




      using (var connection = new SqlConnection(Connection))
      {
        connection.Open();
        using (SqlBulkCopy copyBulk = new SqlBulkCopy(connection))
        {
          copyBulk.BatchSize = 10000;
          copyBulk.BulkCopyTimeout = 600;
          copyBulk.DestinationTableName = tableName;
          copyBulk.NotifyAfter = 10000;
          copyBulk.SqlRowsCopied += (s, e) => log.DebugFormat("{0} Records inserted ", e.RowsCopied);

          using (var collection = new GenericCollectionReader<VendorProductInfo>(products))
          {
            copyBulk.WriteToServer(collection);
          }
        }
      }

      db.Execute(string.Format(@"CREATE NONCLUSTERED INDEX CIN ON {0} (ProductID, ConnectorID)", tableName));

      RemoveProductMatchesFromTempTable(tableName, db);


      db.Execute(string.Format(@"merge content trg
                  using {0} source
                  on trg.productid = source.productid and trg.connectorid = source.connectorid

                  when not matched by target
                  then 
	                  insert (ProductID, ConnectorID, ShortDescription, LongDescription, LineType, LedgerClass,ProductDesk, ExtendedCatalog, CreatedBy, ConnectorPublicationRuleID)
	                  values (source.ProductID, source.ConnectorID, source.ShortDescription, source.LongDescription, source.LineType, source.LedgerClass,source.ProductDesk,1,1, source.connectorpublicationruleid)

                  when matched
	                  then update 
		                  set
		                  trg.shortdescription = source.shortdescription,
		                  trg.LongDescription = source.longdescription,
		                  trg.linetype = source.linetype,
		                  trg.ledgerclass = source.ledgerclass,
		                  trg.ProductDesk = source.ProductDesk,
		                  trg.extendedCatalog = 1,
                      trg.ConnectorPublicationRuleID = source.connectorpublicationruleid
                  when not matched by source and trg.connectorid = {1}
                  then delete;
                  ", tableName, connectorID));
    }

    private void RemoveProductMatchesFromTempTable(string tableName, Database db)
    {
      db.Execute(string.Format(@"          
            with 
            matchesRanked as 
            (	
	            select *, 
	            ROW_NUMBER() over (partition by productmatchid order by publicationruleindex desc) as ranknumber
	             from {0} where productmatchid != -1	  
	
            ),
            matches as
            (
	            select ProductID, ConnectorID, Connectorpublicationruleid, shortdescription, longdescription, linetype, ledgerclass, productdesk, extendedcatalog, productmatchid, publicationruleindex 
              from matchesRanked where ranknumber=  1
	            
              union all
	            
              select * 
              from {0} where productmatchid = -1
            )
            delete from {0} where productid not in (
            select productid from matches)
      ", tableName));
    }

    private List<VendorProductInfoWithWehkampInformation> GetProductsByConnector(int connectorID, Database database, bool excludeProducts)
    {
      log.DebugFormat("");
      log.DebugFormat("-----> Start Getting Product By Connector Publication Rules");
      List<VendorProductInfoWithWehkampInformation> products = new List<VendorProductInfoWithWehkampInformation>();
      List<string> vendorItemNumbersToExclude = new List<string>();
      Dictionary<int, string> vendorItemNumbers = new Dictionary<int, string>();

      if (excludeProducts)
      {
        vendorItemNumbersToExclude = database.Fetch<string>("SELECT Value FROM ExcludeProduct WHERE ConnectorID = @0", connectorID).ToList();
        vendorItemNumbers = database.Fetch<Product>("SELECT ProductID, VendorItemNumber FROM Product").ToDictionary(x => x.ProductID, y => y.VendorItemNumber);
      }

      var connectorRules = database
        .Query<ConnectorPublicationRule>(@"SELECT * FROM [dbo].[ConnectorPublicationRule] WHERE [IsActive] = 1 AND [ConnectorID] = @0", connectorID)
        .Where(x => !x.FromDate.HasValue || x.FromDate < DateTime.UtcNow)
        .Where(x => !x.ToDate.HasValue || x.ToDate > DateTime.UtcNow)
        .OrderBy(x => x.PublicationIndex);

      foreach (var connectorRule in connectorRules)
      {
        var productsByConnectorPublicationRule = GetProductsByConnectorPublicationRule(connectorRule, database);

        if (productsByConnectorPublicationRule.Count > 0)
        {
          switch (connectorRule.PublicationType)
          {
            case (int)ConnectorPublicationRuleType.Include:
              log.DebugFormat("Connector Publication Rule ID: {0}({2}), Count Products: {1} - Include",
                connectorRule.ConnectorPublicationRuleID,
                productsByConnectorPublicationRule.Count,
                connectorRule.PublicationIndex);

              var productsToRemove =
                (
                  from p in products
                  join cprp in productsByConnectorPublicationRule on p.ProductID equals cprp.ProductID
                  select p
                ).ToList();

              if (productsToRemove.Count > 0)
              {
                productsToRemove.ForEach(vendorProduct =>
                {
                  products.RemoveAll(x => x.ProductID == vendorProduct.ProductID);
                });
              }

              products.AddRange(productsByConnectorPublicationRule);

              break;
            case (int)ConnectorPublicationRuleType.Exclude:
              log.DebugFormat("Connector Publication Rule ID: {0}({2}), Count Products: {1} - Exclude",
                connectorRule.ConnectorPublicationRuleID,
                productsByConnectorPublicationRule.Count,
                connectorRule.PublicationIndex);

              productsByConnectorPublicationRule.ForEach(vendorProduct =>
            {
              products.RemoveAll(x => x.ProductID == vendorProduct.ProductID);
            });
              break;
          }
        }
      }

      //exclude
      List<VendorProductInfoWithWehkampInformation> productIDsToRemove = new List<VendorProductInfoWithWehkampInformation>();
      foreach (var va in products)
      {
        if (excludeProducts &&
                          (string.IsNullOrEmpty(va.SentToWehkamp) || va.SentToWehkamp.ToLowerInvariant() == "false") &&
                          (string.IsNullOrEmpty(va.SentToWehkampAsDummy) || va.SentToWehkampAsDummy.ToLowerInvariant() == "false"))
        {
          string vendorItemNumber = vendorItemNumbers[va.ProductID];

          if (ExcludeProduct(vendorItemNumber, vendorItemNumbersToExclude))
          {
            productIDsToRemove.Add(va);
          }
        }
      }
      foreach (var toRemove in productIDsToRemove)
        products.Remove(toRemove);

      return products;
    }

    private bool ExcludeProduct(string vendorItemNumber, List<string> excludedVendorItemNumbers)
    {

      foreach (var excludedVendorItemNumber in excludedVendorItemNumbers)
      {
        if (vendorItemNumber.ToLower().StartsWith(excludedVendorItemNumber.ToLower()))
          return true;
      }
      return false;
    }

    private List<VendorProductInfoWithWehkampInformation> GetProductsByConnectorPublicationRule(ConnectorPublicationRule connectorpublicationRule, Database database)
    {
      string additionalFilters = String.Empty;
      string masterGroupMappingFilter = String.Empty;
      string productAttributeValueFilter = String.Empty;
      string onlyApprovedProductsFilter = string.Empty;

      //Reminder: IsActive als connectorsetting of optie in de connectorpublicationrule opnemen!
      //Is o.a. voor Jumbo voor als die overgaat op MasterGroupMapping, dit staat al in de oude GenerateAssortment plugin
      string baseQuery = @"
        SELECT VA.VendorID, VA.VendorAssortmentID, min(VP.ConcentratorStatusID) as ConcentratorStatusID, P.BrandID, VA.ProductID, ISNULL(SUM(VS.QuantityOnHand),0) AS QuantityOnHand, max(vp.costprice) as Price, {2} as ConnectorPublicationRuleID, {4} as ConnectorID, 
               Va.ShortDescription, VA.LongDescription, va.LineType, va.LedgerClass, va.productdesk, cast(va.extendedcatalog as nvarchar(200)) as extendedcatalog, isnull(PM.ProductMatchID, -1) as ProductMatchID, {5} as PublicationRuleIndex,
               pavSentToWehkamp.Value as 'SentToWehkamp', pavSentAsDummy.Value as 'SentToWehkampAsDummy'
         FROM VendorAssortment VA
          LEFT OUTER JOIN  ProductAttributeValue pavSentToWehkamp  ON va.ProductID = pavSentToWehkamp.ProductID AND pavSentToWehkamp.AttributeID = (SELECT AttributeID FROM  ProductAttributeMetaData pamd  WHERE pamd.AttributeCode = 'SentToWehkamp') 
          LEFT OUTER JOIN  ProductAttributeValue pavSentAsDummy  ON va.ProductID = pavSentAsDummy.ProductID AND pavSentAsDummy.AttributeID = (SELECT AttributeID FROM  ProductAttributeMetaData pamd  WHERE pamd.AttributeCode = 'SentToWehkampAsDummy')
	        INNER JOIN Product P ON (VA.ProductID = P.ProductID)
          {3}
          {6}
	        LEFT JOIN VendorStock VS ON (VA.VendorID = VS.VendorID AND VA.ProductID = VS.ProductID)
	        LEFT JOIN VendorPrice VP ON (VA.VendorAssortmentID = VP.VendorAssortmentID)
          {7}
          left join ProductMatch PM on PM.ProductID = VA.ProductID and pm.isMatched = 1 and pm.MatchStatus = 2
	        WHERE va.IsActive = 1 AND ((p.isconfigurable = 0 and VP.Vendorassortmentid is not null) or (p.isconfigurable = 1)) AND  VA.VendorID = {0} {1}
        GROUP BY VA.VendorID, VA.VendorAssortmentID, P.BrandID, VA.ProductID,Va.ShortDescription, VA.LongDescription, va.LineType, va.LedgerClass, va.productdesk, va.extendedcatalog, PM.ProductMatchID,pavSentToWehkamp.Value,pavSentAsDummy.Value";

      if (connectorpublicationRule.BrandID.HasValue)
      {
        additionalFilters += String.Format(" AND P.BrandID = {0}", connectorpublicationRule.BrandID);
      }

      if (connectorpublicationRule.MasterGroupMappingID.HasValue && connectorpublicationRule.MasterGroupMappingID.Value > 0)
      {
        masterGroupMappingFilter = String.Format("INNER JOIN dbo.MasterGroupMappingProduct mgmp ON p.ProductID = mgmp.ProductID and mgmp.IsProductMapped = 1");
        additionalFilters += String.Format(" AND mgmp.MasterGroupMappingID = {0}", connectorpublicationRule.MasterGroupMappingID);
      }

      if (connectorpublicationRule.ProductID.HasValue)
      {
        additionalFilters += String.Format(" AND va.productid = {0}", connectorpublicationRule.ProductID);
      }

      if (connectorpublicationRule.PublishOnlyStock.HasValue && connectorpublicationRule.PublishOnlyStock.Value)
      {
        additionalFilters += " AND VS.QuantityOnHand > 0";
      }

      if (connectorpublicationRule.StatusID.HasValue)
      {
        additionalFilters += String.Format(" AND VP.ConcentratorStatusID = {0}", connectorpublicationRule.StatusID.Value);
      }

      if (connectorpublicationRule.FromPrice.HasValue)
      {
        additionalFilters += String.Format(" AND vp.Price > {0}", connectorpublicationRule.FromPrice.Value.ToString(CultureInfo.InvariantCulture));
      }

      if (connectorpublicationRule.ToPrice.HasValue)
      {
        additionalFilters += String.Format(" AND vp.Price < {0}", connectorpublicationRule.ToPrice.Value.ToString(CultureInfo.InvariantCulture));
      }
      if (connectorpublicationRule.AttributeID.HasValue ||
          !string.IsNullOrEmpty(connectorpublicationRule.AttributeValue))
      {
        productAttributeValueFilter = String.Format("INNER JOIN dbo.ProductAttributeValue PAV ON VA.ProductID = PAV.ProductID");
        if (connectorpublicationRule.AttributeID.HasValue)
        {
          additionalFilters += String.Format(" AND PAV.AttributeID = {0}", connectorpublicationRule.AttributeID);
        }
        if (!string.IsNullOrEmpty(connectorpublicationRule.AttributeValue))
        {
          additionalFilters += String.Format(" AND PAV.Value = '{0}'", connectorpublicationRule.AttributeValue);
        }
      }

      if (connectorpublicationRule.OnlyApprovedProducts)
      {
        onlyApprovedProductsFilter = @"
				  INNER JOIN dbo.MasterGroupMappingProduct mp ON P.ProductID = mp.ProductID AND mp.IsApproved = 1 AND mp.IsProductMapped = 1
				  INNER JOIN dbo.MasterGroupMapping m ON mp.MasterGroupMappingID = m.MasterGroupMappingID AND m.ConnectorID IS NULL
        ";
      }

      List<VendorProductInfoWithWehkampInformation> products = database.Query<VendorProductInfoWithWehkampInformation>(String.Format(baseQuery,
                                                                                    connectorpublicationRule.VendorID,
                                                                                    additionalFilters,
                                                                                    connectorpublicationRule.ConnectorPublicationRuleID,
                                                                                    masterGroupMappingFilter,
                                                                                    connectorpublicationRule.ConnectorID,
                                                                                    connectorpublicationRule.PublicationIndex,
                                                                                    productAttributeValueFilter,
                                                                                    onlyApprovedProductsFilter)).ToList();

      return products;
    }
  }
}
