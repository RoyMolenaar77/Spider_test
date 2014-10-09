using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using System.Data.Linq;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using PetaPoco;
using System.Data.SqlClient;
using System.Data;
using Concentrator.Objects.Vendors.Base;
using Concentrator.Objects.Assortment;

namespace Concentrator.Plugins.AssortmentGenerator
{
  public class GenerateAssortment : ConcentratorPlugin
  {


    public override string Name
    {
      get { return "Assortment Generator Plugin"; }
    }

    public class VaGroupInfo
    {
      public int VendorAssortmentID { get; set; }
      public int ProductGroupID { get; set; }
    }

    protected override void Process()
    {
      
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();

          var connectors = (from c in this.Connectors
                            where
                              (((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment)
                               || ((ConnectorType)c.ConnectorType).Has(ConnectorType.ShopAssortment))
#if !DEBUG
 && c.IsActive
#endif
                            select c).ToList();

          foreach (Connector conn in connectors)
          {
            bool excludeProducts = conn.ConnectorSettings.GetValueByKey<bool>("ExcludeProducts", false);

            using (var pDb = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
            {
              pDb.CommandTimeout = 15 * 60;

              var productMatches = pDb.Query<ProductMatch>("SELECT * FROM ProductMatch WHERE isMatched = 1").ToList();

              List<string> vendorItemNumbersToExclude = new List<string>();
              Dictionary<int, string> vendorItemNumbers = new Dictionary<int, string>();

              Dictionary<int, Content> contentList = new Dictionary<int, Content>();

              log.DebugFormat("Start Generating Assortment for {0}", conn.Name);

              log.DebugFormat("Connector {0}({1}) has {2} rules for assortment generation", conn.Name, conn.ConnectorID, conn.ContentProducts.Count);

              bool assortmentLoaded = false;

              var publicationRules = pDb.Fetch<ConnectorPublication>("SELECT * FROM ConnectorPublication WHERE ConnectorID = @0", conn.ConnectorID);

              if (excludeProducts)
              {
                vendorItemNumbersToExclude = pDb.Fetch<string>("Select value from ExcludeProduct where ConnectorID = @0", conn.ConnectorID).ToList();
                vendorItemNumbers = pDb.Fetch<Product>("select productID, VendorItemNumber from product").ToDictionary(x => x.ProductID, y => y.VendorItemNumber);
              }


              foreach (ContentProduct rule in conn.ContentProducts.OrderBy(x => x.ProductContentIndex))
              {
                var connectorPublicationRules = publicationRules.Where(x => x.VendorID == rule.VendorID && (!x.FromDate.HasValue || (x.FromDate.HasValue && x.FromDate <= DateTime.Now)) && (!x.ToDate.HasValue || (x.ToDate.HasValue && x.ToDate >= DateTime.Now))).ToList();

                if (!assortmentLoaded)
                  assortmentLoaded = rule.IsAssortment;

                #region content logic

                log.DebugFormat("Processing Rule {0} for connectorid {1}", rule.ProductContentID, conn.ConnectorID);

                if (rule.ProductID.HasValue)
                {
                  #region Single Product

                  if (assortmentLoaded && !rule.IsAssortment)
                  {
                    // exclude product
                    if (rule.ProductID.HasValue)
                      contentList.Remove(rule.ProductID.Value);
                  }
                  else
                  {
                    var vass = _assortmentRepo.GetSingle(a => a.VendorID == rule.VendorID
                                      && a.ProductID == rule.ProductID.Value
                                      && a.IsActive);

                    if (vass != null)
                    {
                      var groups = (from va in _assortmentRepo.GetAllAsQueryable()
                                    from pa in va.ProductGroupVendors
                                    select new
                                    {
                                      va.VendorAssortmentID,
                                      pa.ProductGroupVendorID
                                    }).ToList();

                      var vendoradd = (from va in _assortmentRepo.GetAllAsQueryable()
                                       where va.VendorAssortmentID == vass.VendorAssortmentID
                                       select new VendorAdds
                                       {
                                         VendorAssortmentID = va.VendorAssortmentID,
                                         ConcentratorStatusID = va.VendorPrices.FirstOrDefault().ConcentratorStatusID,
                                         QuantityOnHand = unit.Scope.Repository<VendorStock>().GetAllAsQueryable(c => c.ProductID == va.ProductID && c.VendorID == va.VendorID).Sum(c => c.QuantityOnHand),
                                         ProductID = va.ProductID,
                                         BrandID = va.Product.BrandID
                                       }).ToList();

                      Content content = null;
                      if (!contentList.ContainsKey(vass.ProductID))
                      {

                        if (vass.ProductGroupVendors == null) vass.ProductGroupVendors = new List<ProductGroupVendor>();
                        content = new Content
                                        {
                                          ConnectorID = conn.ConnectorID,
                                          ProductID = rule.ProductID.Value,
                                          ShortDescription = vass.ShortDescription,
                                          LongDescription = vass.LongDescription,
                                          ProductContentID = rule.ProductContentID,
                                          ProductGroupVendors = vass.ProductGroupVendors.Select(c => c.ProductGroupID).ToList(),
                                          Product = unit.Scope.Repository<Product>().GetSingle(c => c.ProductID == rule.ProductID.Value)
                                        };

                        contentList.Add(rule.ProductID.Value, content);

                        CheckConnectorPublication(content, connectorPublicationRules, vendoradd.FirstOrDefault(x => x.VendorAssortmentID == vass.VendorAssortmentID), unit, contentList);
                      }

                    }
                    else
                      log.DebugFormat(
                        "Could not add product {0} to content, does not exists in vender assortment (VendorID {1})",
                        rule.ProductID.Value, rule.VendorID);
                  }

                  #endregion
                }
                else
                {

                  #region Non Product
                  //include scenario

                  string additionalFilters = String.Empty;


                  if (rule.BrandID.HasValue)
                  {
                    additionalFilters += String.Format(" AND P.BrandID = {0}", rule.BrandID);
                  }

                  //if (rule.ProductGroupID.HasValue)
                  //{
                  //  vendorAssortment = vendorAssortment.Where(c => c.ProductGroupVendors.Any(l => l.ProductGroupID == rule.ProductGroupID.Value && l.VendorID == rule.VendorID));
                  //}


                  string query = String.Format(@"SELECT  VA.VendorAssortmentID, VP.ConcentratorStatusID, P.BrandID, VA.ProductID, ISNULL(SUM(VS.QuantityOnHand),0) AS QuantityOnHand
 FROM VendorAssortment VA
	INNER JOIN Product P ON (VA.ProductID = P.ProductID)
	INNER JOIN VendorStock VS ON (VA.VendorID = VS.VendorID AND VA.ProductID = VS.ProductID)
	INNER JOIN VendorPrice VP ON (VA.VendorAssortmentID = VP.VendorAssortmentID)
	WHERE VA.VendorID = {0} {1}
GROUP BY VA.VendorAssortmentID, VP.ConcentratorStatusID, P.BrandID, VA.ProductID
", rule.VendorID, additionalFilters);


                  var vendoradd = pDb.Query<VendorAdds>(query).ToDictionary(x => x.VendorAssortmentID, y => y);

                  // base collection
                  //var vendorAssortment = pDb.Fetch<VendorAssortment>("SELECT * FROM VendorAssortment WHERE VendorID = @0 AND IsActive = 1", rule.VendorID);
                  var vendorAssortmentQuery = string.Format("SELECT va.*, pavSentToWehkamp.Value as 'SentToWehkamp', pavSentAsDummy.Value as 'SentToWehkampAsDummy' FROM VendorAssortment va LEFT OUTER JOIN  ProductAttributeValue pavSentToWehkamp  ON va.ProductID = pavSentToWehkamp.ProductID AND pavSentToWehkamp.AttributeID = (SELECT AttributeID FROM  ProductAttributeMetaData pamd  WHERE pamd.AttributeCode = 'SentToWehkamp') LEFT OUTER JOIN  ProductAttributeValue pavSentAsDummy  ON va.ProductID = pavSentAsDummy.ProductID AND pavSentAsDummy.AttributeID = (SELECT AttributeID FROM  ProductAttributeMetaData pamd  WHERE pamd.AttributeCode = 'SentToWehkampAsDummy') WHERE va.VendorID = {0} AND  va.IsActive = 1", rule.VendorID);
                  var vendorAssortment = pDb.Fetch<VendorAssortmentWehkampSent>(vendorAssortmentQuery);


                  var productGroupVendors = pDb.Fetch<ProductGroupVendor>("SELECT * FROM ProductGroupVendor WHERE VendorID = @0 OR VendorID = @1", rule.VendorID, rule.Vendor.ParentVendorID ?? rule.VendorID);


                  string groupsQuery = string.Format(@"SELECT DISTINCT VA.VendorAssortmentID, 	PGV.ProductGroupID
                                                     FROM VendorAssortment VA
                                                     INNER JOIN VendorProductGroupAssortment VPGA ON (VPGA.VendorAssortmentID = VA.VendorAssortmentID)
                                                     INNER JOIN  ProductGroupVendor PGV ON (PGV.ProductGroupVendorID = VPGA.ProductGroupVendorID)
                                                     WHERE VA.VendorID = {0} AND VA.IsActive=1", rule.VendorID);



                  var groupSource = pDb.Fetch<VaGroupInfo>(groupsQuery);
                  var groups = (from g in groupSource
                                group g by g.VendorAssortmentID into grouped
                                select grouped).ToDictionary(x => x.Key, y => (from pg in y select pg.ProductGroupID).ToList());
                  groupSource = null;


                  var productRepo = pDb.Fetch<Product>("SELECT * FROM Product").ToDictionary(x => x.ProductID, y => y);


                  List<ProductAttributeValue> values = pDb.Query<ProductAttributeValue>(string.Format(@"select pav.* from productattributevalue pav
                                                                                          inner join connectorpublication cp on pav.attributeid = cp.attributeid
                                                                                          where cp.connectorid = {0}", rule.ConnectorID)).ToList();

                  foreach (var va in vendorAssortment)
                  {
                    if (!groups.ContainsKey(va.VendorAssortmentID))
                      continue;

                    if (assortmentLoaded && !rule.IsAssortment)
                    {
                      contentList.Remove(va.ProductID);
                    }
                    else
                    {
                      if (excludeProducts && 
                        (string.IsNullOrEmpty(va.SentToWehkamp) || va.SentToWehkamp.ToLowerInvariant() == "false") && 
                        (string.IsNullOrEmpty(va.SentToWehkampAsDummy) || va.SentToWehkampAsDummy.ToLowerInvariant() == "false"))
                      {
                        string vendorItemNumber = vendorItemNumbers[va.ProductID];

                        if (ExcludeProduct(vendorItemNumber, vendorItemNumbersToExclude))
                          continue;
                      }


                      Content content = null;
                      if (!contentList.ContainsKey(va.ProductID))
                      {

                        content = new Content
                                    {
                                      ConnectorID = conn.ConnectorID,
                                      ProductID = va.ProductID,
                                      ShortDescription = va.ShortDescription,
                                      LongDescription = va.LongDescription,
                                      LineType = va.LineType,
                                      ProductContentID = rule.ProductContentID,
                                      ProductGroupVendors = groups[va.VendorAssortmentID],
                                      Product = productRepo[va.ProductID]
                                    };


                        //content.Product.RelatedProductsSource = relatedProducts.Where(c => c.ProductID == content.ProductID).ToList();
                        content.Product.ProductAttributeValues = values.Where(c => c.ProductID == content.ProductID).ToList();

                        contentList.Add(va.ProductID, content);
                      }

                      VendorAdds vad = null;
                      vendoradd.TryGetValue(va.VendorAssortmentID, out vad);

                      CheckConnectorPublication(content, connectorPublicationRules, vad, unit, contentList);


                    }
                  }
                  #endregion
                }


                log.DebugFormat("Finished Processing Rule {0} for Connector {1}", rule.ProductContentID, conn.Name);

                #endregion

              }

              ///put logic here

              try
              {
                var vendorsettings = pDb.Query<ContentProduct>("SELECT * FROM ContentProduct WHERE ConnectorID = @0", conn.ConnectorID)
                  .Select(v => new
                {
                  v.ProductContentIndex,
                  v.ProductContentID

                }).Distinct().ToDictionary(x => x.ProductContentID, x => x.ProductContentIndex);


                var copyContentList = (from c in contentList.Values
                                       join pm in productMatches on c.ProductID equals pm.ProductID
                                       select c).ToList();

                foreach (var c in copyContentList)
                {
                  var match = productMatches.Where(x => x.ProductID == c.ProductID).FirstOrDefault();
                  if (match != null)
                  {
                    var matches = productMatches.Where(x => x.ProductMatchID == match.ProductMatchID && x.ProductID != c.ProductID).ToList();

                    if (matches.Count > 0)
                    {
                      foreach (var product in matches)
                      {
                        if (!c.ProductContentID.HasValue) continue;

                        int contentVendorIndex = vendorsettings[c.ProductContentID.Value];

                        Content matchContent = null;
                        if (contentList.TryGetValue(product.ProductID, out matchContent))
                        {
                          if (!matchContent.ProductContentID.HasValue) continue;

                          int matchContentIndex = vendorsettings[matchContent.ProductContentID.Value];

                          if (contentVendorIndex > matchContentIndex)
                            contentList.Remove(c.ProductID);
                          else
                            contentList.Remove(matchContent.ProductID);
                        }
                      }
                    }
                  }
                }
              }
              catch (Exception ex)
              {
                log.AuditError("Cleaning Matches failed for {0}", conn.Name);
              }

              var curContent = pDb.Fetch<Content>("SELECT * FROM Content WHERE ConnectorID = @0", conn.ConnectorID).ToDictionary(x => x.ProductID, y => y);

              try
              {
                log.DebugFormat("Cleaning up assortment for {0}", conn.Name);

                var delcontent = (from c in curContent
                                  where !contentList.ContainsKey(c.Key)
                                  select c.Value).ToList();

                foreach (var rec in delcontent)
                  pDb.Delete("Content", "ProductID, ConnectorID", rec);

                log.DebugFormat("Finished cleaning up for {0}", conn.Name);
              }
              catch (Exception ex)
              {
                log.FatalFormat("Assorment cleanup failed for {0} error {1}", conn.ConnectorID, ex.StackTrace);
              }

              #region Existing Content

              //OBSOLETE:

              //log.Debug("Processing Existing Content");

              //foreach (var curCon in contentList.Values)
              //{
              //  Content exCon = null;
              //  curContent.TryGetValue(curCon.ProductID, out exCon);

              //  if (exCon != null)
              //  {
              //    exCon.ExtendedCatalog = curCon.ExtendedCatalog;
              //    exCon.LedgerClass = curCon.LedgerClass;
              //    exCon.LineType = curCon.LineType;
              //    exCon.LongDescription = curCon.LongDescription;
              //    exCon.ProductDesk = curCon.ProductDesk;
              //    exCon.ShortDescription = curCon.ShortDescription;
              //    exCon.ProductContentID = curCon.ProductContentID;

              //    pDb.Update("Content", "ProductID, ConnectorID", exCon, new List<String>() { "ShortDescription", "LongDescription", "LineType", "LedgerClass", "ProductDesk", "ExtendedCatalog", "ProductContentID" });
              //  }
              //}
              ////unit.Save();
              //log.Debug("Finished processing existing content");

              #endregion

              var newContent = (from c in contentList
                                where !curContent.ContainsKey(c.Key)
                                select c.Value).ToList();


              log.DebugFormat("Inserting {0} rows for connector {1}", newContent.Count(), conn.ConnectorID);
              foreach (var c in newContent)
              {
                pDb.Insert("Content", "ProductID, ConnectorID", false, new { ProductID = c.ProductID, ConnectorID = c.ConnectorID, c.ProductContentID, c.ShortDescription, c.LongDescription, c.ExtendedCatalog, c.LedgerClass, c.LineType, c.ProductDesk, CreatedBy = c.CreatedBy });
              }

              #region contentproductgroups
              try
              {

                var contentProductGroups = pDb.Fetch<ContentProductGroup>("SELECT * FROM ContentProductGroup WHERE IsCustom = 0 AND ConnectorID = @0", conn.ConnectorID);

                //List<ProductGroupMapping> productGroupMappings = pDb.Fetch<ProductGroupMapping>("SELECT * FROM ProductGroupMapping WHERE Depth = 0 AND (ConnectorID = @0 OR ConnectorID = @1)",
                //    conn.ConnectorID, conn.ParentConnectorID ?? conn.ConnectorID);

                List<ProductGroupMapping> productGroupMappings = pDb.Fetch<ProductGroupMapping>("SELECT * FROM ProductGroupMapping WHERE (ConnectorID = @0 OR ConnectorID = @1)",
                conn.ConnectorID, conn.ParentConnectorID ?? conn.ConnectorID);

                Dictionary<int, List<int>> MappingProductGroup = (
                    from c in productGroupMappings
                    group c by c.ProductGroupID into gr
                    select new { gr.Key, Value = gr.Select(c => c.ProductGroupMappingID) })
                  .ToDictionary(c => c.Key, c => c.Value.ToList());

                //Start assortment tree processes
                var tree = BuildTree(productGroupMappings);

                var products = GetContentsPerMapping(contentList.Values.ToList(), productGroupMappings, MappingProductGroup);

                //add products to tree
                AddProductsToTree(tree, products, contentList.Values.ToList(), MappingProductGroup);
                //end assortment tree processes

                var toSync = tree.FlattenTreeToList(conn.ConnectorID, 1);
                SyncNewContentProductGroups(toSync, pDb, conn.ConnectorID);

                log.DebugFormat("Finish Remove unused contentproductgroups");
              }
              catch (Exception ex)
              {
                log.Fatal("Error processing contentproductgroups for connectorID" + conn.ConnectorID.ToString(), ex);
              }
              #endregion

              log.DebugFormat("Finish insert assortment for connector {0}: {1}", conn.ConnectorID, conn.Name);

              if (conn.ConnectorSettings.GetValueByKey<bool>("SyncContentProductGroupsWithParentConnector", false))
                SyncCustomContentProducts(conn, pDb);
            }

          }
        }
        catch (Exception ex)
        {
          log.Fatal("Processs assortment error", ex);
        }
      }
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

    private Dictionary<int, List<int>> GetContentsPerMapping(List<Content> products, List<ProductGroupMapping> mappings, Dictionary<int, List<int>> MappingProductGroup)
    {
      Dictionary<int, List<int>> ProductsPerMapping = (from c in mappings
                                                       select c.ProductGroupMappingID).ToDictionary(c => c, c => new List<int>());


      foreach (var product in products)
      {
        foreach (var group in product.ProductGroupVendors)
        {
          if (MappingProductGroup.ContainsKey(group))
          {
            foreach (var mappingID in MappingProductGroup[group])
              ProductsPerMapping[mappingID].Add(product.ProductID);
          }
        }
      }
      return ProductsPerMapping;
    }

    private void AddProductsToTree(AssortmentTree tree, Dictionary<int, List<int>> productsPerProductGroupMapping, List<Content> Contents, Dictionary<int, List<int>> MappingProductGroup)
    {
      var productMapping = (from c in Contents
                            select new
                            {
                              c.ProductID,
                              ProductGroupMappings = (from pg in c.ProductGroupVendors
                                                      where MappingProductGroup.ContainsKey(pg)
                                                      select MappingProductGroup[pg]).SelectMany(pg => pg).ToList()
                            }).ToDictionary(c => c.ProductID, c => c.ProductGroupMappings);

      foreach (var mappingGroup in productsPerProductGroupMapping)
      {
        tree.AddProducts(mappingGroup.Value, mappingGroup.Key, productMapping, false);
      }
    }

    private AssortmentTree BuildTree(List<ProductGroupMapping> productGroupMappings)
    {
      AssortmentTree tree = new AssortmentTree();
      foreach (var mapping in productGroupMappings.Where(c => c.ParentProductGroupMappingID == null).ToList())
      {
        //roots
        tree.AddToTree(mapping.ProductGroupMappingID, flattenHierarchy: mapping.FlattenHierarchy, filterByParent: mapping.FilterByParentGroup);
        foreach (var childMapping in productGroupMappings.Where(c => c.ParentProductGroupMappingID == mapping.ProductGroupMappingID))
        {
          AddChild(tree, childMapping, productGroupMappings);
        }
      }
      return tree;
    }

    private void AddChild(AssortmentTree tree, ProductGroupMapping current, List<ProductGroupMapping> productGroupMappings)
    {
      tree.AddToTree(current.ProductGroupMappingID, current.ParentProductGroupMappingID, flattenHierarchy: current.FlattenHierarchy, filterByParent: current.FilterByParentGroup);

      foreach (var childMapping in productGroupMappings.Where(c => c.ParentProductGroupMappingID == current.ProductGroupMappingID))
      {
        AddChild(tree, childMapping, productGroupMappings);
      }
    }

    private void SyncNewContentProductGroups(List<ContentProductGroupModel> newCpg, Database db, int connectorID)
    {
      string tableName = string.Format("Temp_Content_Product_Group_{0}", connectorID);
      try
      {

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
                                ProductGroupMappingID int not null,                                
                                CreatedBy int not null)", tableName);
        db.Execute(q); //create temp
        using (var connection = new SqlConnection(Connection))
        {
          connection.Open();
          using (SqlBulkCopy copyBulk = new SqlBulkCopy(connection))
          {
            copyBulk.BatchSize = 100000;
            copyBulk.BulkCopyTimeout = 3600;
            copyBulk.DestinationTableName = tableName;
            copyBulk.NotifyAfter = 100000;
            copyBulk.SqlRowsCopied += (s, e) => log.DebugFormat("{0} Records inserted ", e.RowsCopied);

            using (var collection = new GenericCollectionReader<ContentProductGroupModel>(newCpg))
            {
              copyBulk.WriteToServer(collection);
            }
          }
        }

        db.Execute(string.Format(@"MERGE ContentProductGroup trg using {0} src
                                   on src.connectorid = trg.connectorid and src.productid = trg.productid and src.productgroupmappingid = trg.productgroupmappingid and trg.iscustom = 0
                                    when not matched by target
                                         then insert (productid, connectorid, productgroupmappingid, createdby, [exists], isexported)
                                         values (src.productid, src.connectorid, src.productgroupmappingid, src.createdby, 1, 1)
                                    when not matched by source and trg.connectorid = {1} and trg.iscustom = 0
                                          then delete;", tableName, connectorID));

        db.Execute(string.Format("Drop table {0}", tableName));
      }
      catch (Exception e)
      {
        log.Debug("Synchronization of content product groups failed", e);
        throw e;
      }
    }

    private void SyncCustomContentProducts(Connector con, Database pDB)
    {
      if (!con.ParentConnectorID.HasValue) return; //no need to sync anything. It is a parent
      
      string syncCustomContentProductsQuery = string.Format(@"merge contentproductgroup target
					using(
						select cpg.* from contentproductgroup cpg
						inner join content c on c.productid = cpg.productid and c.connectorid = {1}
						where cpg.connectorid = {0} and iscustom = 1 
					) src
					on src.productid = target.productid and src.productgroupmappingid = target.productgroupmappingid and target.connectorid = {1}
					when not matched by target
					then 
					insert (productid, connectorid, productgroupmappingid, createdby, creationtime, iscustom, [exists])
					values (src.productid, {1}, src.productgroupmappingid, src.createdby, src.creationtime, src.iscustom, src.[exists]);", con.ParentConnectorID.Value, con.ConnectorID);

      pDB.Execute(syncCustomContentProductsQuery);

    }

    private void CheckConnectorPublication(Content content, List<ConnectorPublication> connectorPublicationRules, VendorAdds vendorAdd, IUnitOfWork unit,
       Dictionary<int, Content> contentList

      )
    {
      if (vendorAdd == null)
      {
        return;
      }

      if (vendorAdd.ConcentratorStatusID.HasValue)
      {
        connectorPublicationRules = connectorPublicationRules.Where(x => !x.StatusID.HasValue || x.StatusID == vendorAdd.ConcentratorStatusID.Value).ToList();

        if (connectorPublicationRules.Count > 0)
        {
          var matchedrules = connectorPublicationRules.Where(x => x.ProductID.HasValue && x.ProductID == vendorAdd.ProductID).ToList();

          if (matchedrules.Count > 0)
          {
            if (matchedrules.Any(x => !x.Publish.Try(c => c.Value, true)) || matchedrules.Any(x => x.Publish.Value && x.PublishOnlyStock.Value) && vendorAdd.QuantityOnHand < 1)
              contentList.Remove(content.ProductID);
          }

          matchedrules = connectorPublicationRules.Where(x => x.BrandID.HasValue && x.BrandID == vendorAdd.BrandID).ToList();
          matchedrules.AddRange(connectorPublicationRules.Where(x => x.ProductGroupID.HasValue && content.ProductGroupVendors.Contains(x.ProductGroupID.Value)));

          if (content.Product == null)
          {
            content.Product = unit.Scope.Repository<Product>().GetSingle(c => c.ProductID == content.ProductID);

          }

          var matchedAttributes = (from x in connectorPublicationRules
                                   let val = content.Product.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == x.AttributeID)
                                   where (x.AttributeID.HasValue && !String.IsNullOrEmpty(x.AttributeValue)) &&
                                   ((val != null ? val.Value == x.AttributeValue : false))
                                   select x).ToList();

          if (matchedAttributes.Count() > 0)
            matchedrules.AddRange(matchedAttributes);

          if (matchedrules.Count < 1)
            matchedrules.AddRange(connectorPublicationRules.Where(x => !x.BrandID.HasValue && !x.ProductID.HasValue && !x.ProductGroupID.HasValue && (!x.AttributeID.HasValue || String.IsNullOrEmpty(x.AttributeValue))));

          //if (matchedrules.OrderBy(x => x.ProductContentIndex).Any(x => !x.Publish) || matchedrules.OrderBy(x => x.ProductContentIndex).Any(x => x.Publish && x.PublishOnlyStock) && vendorStock < 1)

          var ruleMQ = matchedrules.OrderBy(x => x.ProductContentIndex).FirstOrDefault(c => c.MinimumStock.HasValue);
          var minQuantity = ruleMQ == null ? 1 : ruleMQ.MinimumStock.Value;

          if (matchedrules.OrderBy(x => x.ProductContentIndex).Any(x => (x.PublishOnlyStock.HasValue && x.PublishOnlyStock.Value)) && vendorAdd.QuantityOnHand < minQuantity)
          {
            contentList.Remove(content.ProductID);
          }
          else if (matchedrules.OrderBy(x => x.ProductContentIndex).Any(x => !x.PublishOnlyStock.HasValue && !x.Publish.HasValue || (x.Publish.HasValue && !x.Publish.Value)))
          {
            contentList.Remove(content.ProductID);
          }
        }
      }
    }

    public class FilterInfo
    {
      public List<int> ProductGroupFilter = new List<int>();
      public bool Flatten { get; set; }
    }



    private void ProcessProductGroups(
      PetaPoco.Database pDb,
          Connector connector,
          List<Content> content,
          List<ProductGroupMapping> productGroupMappings,
          FilterInfo filter,
          ProductGroupMapping parent,
      List<ContentProductGroup> contentProductGroups,
      List<ContentProductGroup> newContentProductGroupsToInsertInDb
      )
    {


      if (filter.Flatten)
      {
        if (parent == null)
        {
          log.WarnFormat("FlattenHierarchy is not supported on root mappings");
          return;
        }
        //combine all child groups into this one
        List<int> childIDList = productGroupMappings.Select(x => x.ProductGroupID).ToList(); //recursive?

        var groupProducts = content.AsQueryable();

        if (parent.FilterByParentGroup)
        {
          if (filter.ProductGroupFilter.Count > 0)
          {
            foreach (int id in filter.ProductGroupFilter)
              groupProducts = groupProducts.Where(x => x.ProductGroupVendors.Contains(id));
          }
        }


        //filter all
        groupProducts = groupProducts.Where(x => x.ProductGroupVendors.Any(y => childIDList.Contains(y)));

        foreach (var mapping in productGroupMappings)
        {
          if (mapping.FilterByParentGroup)
          {
            if (filter.ProductGroupFilter.Count > 0)
            {
              foreach (int id in filter.ProductGroupFilter)
                groupProducts = groupProducts.Where(x => x.ProductGroupVendors.Contains(id));
            }
          }
        }

        foreach (var product in groupProducts)
        {
          var currentRecord =
            contentProductGroups.Where(
              x => x.ConnectorID == connector.ConnectorID && x.ProductID == product.ProductID
                   && x.ProductGroupMappingID == parent.ProductGroupMappingID).SingleOrDefault();

          if (currentRecord == null)
          {

            var newRecord =

              new ContentProductGroup()
              {
                ConnectorID = connector.ConnectorID,
                ProductID = product.ProductID,
                ProductGroupMappingID = parent.ProductGroupMappingID,
                Exists = true,
                CreationTime = DateTime.Now,
                CreatedBy = Concentrator.Objects.Web.Client.User.UserID
              };
            newContentProductGroupsToInsertInDb.Add(newRecord);

            contentProductGroups.Add(newRecord);

          }
          else
            currentRecord.Exists = true;
        }

        //unit.Save();
      }
      else
      {
        foreach (var mapping in productGroupMappings)
        {

          var childMappings = pDb.Fetch<ProductGroupMapping>("SELECT * FROM ProductGroupMapping WHERE ParentProductGroupMappingID = @0", mapping.ProductGroupMappingID);
          bool hasChildren = childMappings.Count > 0;
          if (hasChildren)
          {
            var childFilter = new FilterInfo();

            // do child groups
            if (mapping.FilterByParentGroup)
              childFilter.ProductGroupFilter.AddRange(filter.ProductGroupFilter);

            childFilter.ProductGroupFilter.Add(mapping.ProductGroupID);

            childFilter.Flatten = mapping.FlattenHierarchy;

            ProcessProductGroups(pDb, connector, content, childMappings, childFilter, mapping, contentProductGroups, newContentProductGroupsToInsertInDb);
          }
          else
          {

            var groupProducts =
              content.Where(c => c.ProductGroupVendors.Contains(mapping.ProductGroupID)).ToList();

            if (mapping.FilterByParentGroup)
            {
              if (filter.ProductGroupFilter.Count > 0)
              {
                foreach (int id in filter.ProductGroupFilter)
                  groupProducts = groupProducts.Where(x => x.ProductGroupVendors.Contains(id)).ToList();
              }
            }

            foreach (var product in groupProducts)
            {
              var currentRecord =
           contentProductGroups.Where(
             x => x.ConnectorID == connector.ConnectorID && x.ProductID == product.ProductID
                  && x.ProductGroupMappingID == mapping.ProductGroupMappingID).SingleOrDefault();

              if (currentRecord == null)
              {
                var newRecord =
                  new ContentProductGroup()
                    {
                      ConnectorID = connector.ConnectorID,
                      ProductID = product.ProductID,
                      ProductGroupMappingID = mapping.ProductGroupMappingID,
                      Exists = true,
                      CreationTime = DateTime.Now,
                      CreatedBy = Concentrator.Objects.Web.Client.User.UserID
                    };

                newContentProductGroupsToInsertInDb.Add(newRecord);
                contentProductGroups.Add(newRecord);
              }
              else
                currentRecord.Exists = true;
            }
          }
        }
      }
    }
  }

  public class VendorAdds
  {
    public int VendorAssortmentID { get; set; }
    public int? ConcentratorStatusID { get; set; }
    public int QuantityOnHand { get; set; }
    public int ProductID { get; set; }
    public int BrandID { get; set; }
  }
}

