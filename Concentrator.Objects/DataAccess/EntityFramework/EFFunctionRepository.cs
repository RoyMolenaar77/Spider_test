using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Objects;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Services;
using Concentrator.Objects.Models.Prices;

namespace Concentrator.Objects.DataAccess.EntityFramework
{
  public class EFFunctionRepository : IFunctionRepository
  {
    private ObjectContext _context;
    private const string _containerName = "ConcentratorEntitiesContainer";

    public EFFunctionRepository(ObjectContext context)
    {
      _context = context;
      _context.CommandTimeout = 222200;
    }

    public IEnumerable<FetchProductMatchesResult> FetchProductMatches(int? productID, int? vendorID)
    {
      ObjectParameter productIDParameter;

      if (productID.HasValue) productIDParameter = new ObjectParameter("ProductID", productID);
      else productIDParameter = new ObjectParameter("ProductID", typeof(Int32));

      ObjectParameter vendorIDParameter;

      if (vendorID.HasValue)
      {
        vendorIDParameter = new ObjectParameter("VendorID", vendorID);
      }
      else
      {
        vendorIDParameter = new ObjectParameter("VendorID", typeof(Int32));
      }

      var a = _context.ExecuteFunction<FetchProductMatchesResult>(string.Format("{0}.FetchProductMatches", _containerName), productIDParameter, vendorIDParameter);

      return a;
    }

    public IEnumerable<ProductMatchResult> GetProductMatches()
    {
      return _context.ExecuteFunction<ProductMatchResult>(string.Format("{0}.GetProductMatches", _containerName));
    }

    public IEnumerable<VendorAssortmentResult> GetVendorAssortment(string doc, int connectorID)
    {
      ObjectParameter connectorIDParameter = new ObjectParameter("ConnectorID", connectorID);
      ObjectParameter docParameter = new ObjectParameter("Vendors", doc);

      return _context.ExecuteFunction<VendorAssortmentResult>(string.Format("{0}.GetVendorAssortment", _containerName), docParameter, connectorIDParameter);
    }

    public IEnumerable<VendorPriceResult> GetVendorPrice(string doc, int connectorID)
    {
      ObjectParameter connectorIDParameter = new ObjectParameter("ConnectorID", connectorID);
      ObjectParameter docParameter = new ObjectParameter("Vendors", doc);


      return _context.ExecuteFunction<VendorPriceResult>(string.Format("{0}.GetVendorPrice", _containerName), docParameter, connectorIDParameter);
    }

    public IEnumerable<CustomItemNumberResult> GetCustomItemNumbers(int connectorID)
    {
      ObjectParameter connectorIDParameter = new ObjectParameter("ConnectorID", connectorID);

      return _context.ExecuteFunction<CustomItemNumberResult>(string.Format("{0}.GetCustomItemNumbers", _containerName), connectorIDParameter);
    }

    public IEnumerable<VendorStockResult> GetVendorRetailStock(string doc, int connectorID)
    {
      ObjectParameter connectorIDParameter = new ObjectParameter("ConnectorID", connectorID);
      ObjectParameter docParameter = new ObjectParameter("Vendors", doc);


      return _context.ExecuteFunction<VendorStockResult>(string.Format("{0}.GetVendorRetailStock", _containerName), docParameter, connectorIDParameter);
    }

    public IEnumerable<ContentAttribute> GetProductAttributes(int? productID, int languageID, int connectorID, DateTime? lastUpdate)
    {
      ObjectParameter productIDParameter;
      ObjectParameter languageIDParameter = new ObjectParameter("LanguageID", languageID);
      ObjectParameter connectorIDParameter = new ObjectParameter("ConnectorID", connectorID);
      ObjectParameter luParameter;

      if (lastUpdate.HasValue) luParameter = new ObjectParameter("LastUpdate", lastUpdate.Value);
      else luParameter = new ObjectParameter("LastUpdate", typeof(DateTime));

      if (productID.HasValue) productIDParameter = new ObjectParameter("ProductID", productID);
      else productIDParameter = new ObjectParameter("ProductID", typeof(Int32));

      return _context.ExecuteFunction<ContentAttribute>(string.Format("{0}.GetProductAttributes", _containerName), productIDParameter, connectorIDParameter, languageIDParameter, luParameter);
    }

    public void GenerateContentAttributes()
    {
      _context.CommandTimeout = 222200;
      _context.ExecuteFunction(string.Format("{0}.GenerateContentAttributes", _containerName));
    }

    public void UpdateProductCompetitorPrice(int productCompareID, int productCompetitorID)
    {
      ObjectParameter compareProductIDParam = new ObjectParameter("CompareProductID", productCompareID);
      ObjectParameter productCompetitorIDParam = new ObjectParameter("ProductCompetitorMappingID", productCompetitorID);

      _context.ExecuteFunction(string.Format("{0}.UpdateProductCompetitorPrice", _containerName), compareProductIDParam, productCompetitorIDParam);
    }

    public void UpdateProductCompare(int productCompareID)
    {
      ObjectParameter compareProductIDParam = new ObjectParameter("CompareProductID", productCompareID);

      _context.ExecuteFunction(string.Format("{0}.UpdateProductCompare", _containerName), compareProductIDParam);
    }

    public IEnumerable<Models.Products.ProductResult> GetCatalogResult(int vendorid)
    {
      var query = @"select 
va.CustomItemNumber as Product,
p.VendorItemNumber as Artnr,
(Select top 1 VendorProductGroupCode1 from ProductGroupVendor pgv
inner join VendorProductGroupAssortment vpga on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
where VendorProductGroupCode1 is not null and vpga.VendorAssortmentID = va.VendorAssortmentID) as Chapter,
(Select top 1 VendorProductGroupCode2 from ProductGroupVendor pgv
inner join VendorProductGroupAssortment vpga on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
where VendorProductGroupCode2 is not null and vpga.VendorAssortmentID = va.VendorAssortmentID) as Category,
(Select top 1 VendorProductGroupCode3 from ProductGroupVendor pgv
inner join VendorProductGroupAssortment vpga on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
where VendorProductGroupCode3 is not null and vpga.VendorAssortmentID = va.VendorAssortmentID) as Subcategory,
(Select top 1 value from ProductAttributeValue pav 
	inner join ProductAttributeMetaData pamd on pav.AttributeID = pamd.AttributeID 

    
	where pamd.AttributeCode = 'PriceGroup' and pav.ProductID = va.ProductID) as PriceGroup,
va.ShortDescription as [Description1],
va.LongDescription as [Description2],
vp.Price as PriceNL,
(select vp2.Price from VendorAssortment va2
inner join VendorPrice vp2 on va2.VendorAssortmentID = vp2.VendorAssortmentID
where va2.VendorID = 52 and va2.ProductID = va.productid) as PriceBE,
vp.CostPrice as VatExcl,
(select top 1 mediapath from productmedia where productid = va.productid and description = 'ProductImage' order by sequence )  as ProductImage
from VendorAssortment va
inner join Product p on va.ProductID = p.ProductID
inner join VendorPrice vp on vp.VendorAssortmentID = va.VendorAssortmentID
where va.VendorID = " + vendorid;

      return _context.ExecuteStoreQuery<ProductResult>(query);
    }

    public IEnumerable<ProductSearchResult> SearchProducts(int languageID, string query, bool? includeDescriptions = null, bool? includeBrands = null, bool? includeProductGroups = null, bool? includeIds = null)
    {
      ObjectParameter dParam = null;
      if (includeDescriptions.HasValue)
        dParam = new ObjectParameter("includeProductDescriptions", includeDescriptions);

      else
        dParam = new ObjectParameter("includeProductDescriptions", typeof(bool?));


      ObjectParameter bParam = null;

      if (includeBrands.HasValue)
        bParam = new ObjectParameter("includeBrands", includeBrands);
      else
        bParam = new ObjectParameter("includeBrands", typeof(bool?));


      ObjectParameter iParam = null;
      if (includeIds.HasValue)
        iParam = new ObjectParameter("includeProductIdentifiers", includeIds);
      else
        iParam = new ObjectParameter("includeProductDescriptions", typeof(bool?));

      ObjectParameter pgParam = null;
      if (includeProductGroups.HasValue)
        pgParam = new ObjectParameter("includeProductGroups", includeProductGroups);
      else
        pgParam = new ObjectParameter("includeProductGroups", typeof(bool?));

      ObjectParameter lParam = new ObjectParameter("LanguageID", languageID);
      ObjectParameter qParam = new ObjectParameter("Query", query);

      return _context.ExecuteFunction<ProductSearchResult>(string.Format("{0}.ProductSearch", _containerName), dParam, bParam, iParam, pgParam, lParam, qParam);

    }

    public void CopyProductGroupMappings(int sourceConnectorID, int destinationConnectorID, int? root = null, int? rootID = null, bool? copyAttributes = null, bool? copyPrices = null, bool? copyProducts = null, bool? copyContetnVendorSettings = null, bool? copyPublications = null, bool? copyConnectorProductStatuses = null, bool? preferredContentSettings = null)
    {
      ObjectParameter sourceC = new ObjectParameter("SourceConnectorID", sourceConnectorID);
      ObjectParameter destinationC = new ObjectParameter("DestinationConnectorID", destinationConnectorID);

      ObjectParameter rootP = null;
      ObjectParameter rootIDP = null;
      ObjectParameter caP = null;
      ObjectParameter cpP = null;
      ObjectParameter ccpP = null;
      ObjectParameter cvsP = null;
      ObjectParameter ccpPubP = null;
      ObjectParameter ccps = null;
      ObjectParameter pcs = null;

      if (root.HasValue)
        new ObjectParameter("Root", root);
      else
        rootP = new ObjectParameter("Root", typeof(int?));

      if (rootID.HasValue)
        rootIDP = new ObjectParameter("RootID", rootID);
      else
        rootIDP = new ObjectParameter("RootID", typeof(int?));

      if (copyAttributes.HasValue)
        caP = new ObjectParameter("CopyAttributes", copyAttributes);
      else
        caP = new ObjectParameter("CopyAttributes", typeof(bool?));

      if (copyPrices.HasValue)
        cpP = new ObjectParameter("CopyContentPrices", copyPrices);
      else
        cpP = new ObjectParameter("CopyContentPrices", typeof(bool?));

      if (copyProducts.HasValue)
        ccpP = new ObjectParameter("CopyContentProducts", copyProducts);
      else
        ccpP = new ObjectParameter("CopyContentProducts", typeof(bool?));

      if (copyContetnVendorSettings.HasValue)
        cvsP = new ObjectParameter("CopyContentVendorSettings", copyContetnVendorSettings);
      else
        cvsP = new ObjectParameter("CopyContentVendorSettings", typeof(bool?));

      if (copyPublications.HasValue)
        ccpPubP = new ObjectParameter("CopyConnectorPublications", copyPublications);
      else
        ccpPubP = new ObjectParameter("CopyConnectorPublications", typeof(bool?));

      if (copyConnectorProductStatuses.HasValue)
        ccps = new ObjectParameter("CopyConnectorProductStatuses", copyConnectorProductStatuses);
      else
        ccps = new ObjectParameter("CopyConnectorProductStatuses", typeof(bool?));

      if (preferredContentSettings.HasValue)
        pcs = new ObjectParameter("CopyPreferredSettings", preferredContentSettings);
      else
        pcs = new ObjectParameter("CopyPreferredSettings", typeof(bool?));

      _context.CommandTimeout = 222200;
      _context.ExecuteFunction(string.Format("{0}.CopyPublishableContent", _containerName), sourceC, destinationC, rootP, rootIDP, caP, cpP, ccpP, ccpPubP, cvsP, ccps);
    }

    public void UpdateVendorProductStatus(int vendorID, int concentratorStatusIDOld, int concentratorStatusIDNew, string vendorStatus)
    {
      ObjectParameter vendorIDParam = new ObjectParameter("VendorID", vendorID);
      ObjectParameter sOldParam = new ObjectParameter("ConcentratorStatusID_Old", concentratorStatusIDOld);
      ObjectParameter sNewParam = new ObjectParameter("ConcentratorStatusID_New", concentratorStatusIDNew);
      ObjectParameter vendorStatusParam = new ObjectParameter("VendorStatus", vendorStatus);

      _context.ExecuteFunction(string.Format("{0}.UpdateVendorProductStatus", _containerName), vendorIDParam, sOldParam, sNewParam, vendorStatusParam);
    }

    public void UpdateConnectorProductStatus(int connectorID, int connectorStatusIDOld, int connectorStatusIDNew, int concentratorStatusIDOld, int concentratorStatusIDNew, string connectorStatus)
    {
      ObjectParameter connectorIDParam = new ObjectParameter("ConnectorID", connectorID);
      ObjectParameter cOldParam = new ObjectParameter("ConnectorStatusID_Old", concentratorStatusIDOld);
      ObjectParameter cNewParam = new ObjectParameter("ConnectorStatusID_New", concentratorStatusIDNew);
      ObjectParameter sOldParam = new ObjectParameter("ConcentratorStatusID_Old", concentratorStatusIDOld);
      ObjectParameter sNewParam = new ObjectParameter("ConcentratorStatusID_New", concentratorStatusIDNew);
      ObjectParameter connectorStatusParam = new ObjectParameter("ConnectorStatus", connectorStatus);

      _context.ExecuteFunction(string.Format("{0}.UpdateConnectorProductStatus", _containerName), connectorIDParam, cOldParam, cNewParam, sOldParam, sNewParam, connectorStatusParam);
    }

    public void RegenerateMissingContent(int connectorID)
    {
      _context.CommandTimeout = 222200;
      ObjectParameter connectorIDParam = new ObjectParameter("ConnectorID", connectorID);
      _context.ExecuteFunction(string.Format("{0}.GenerateMissingContent", _containerName), connectorIDParam);
    }

    public IEnumerable<AssortmentContentView> GetAssortmentContentView(int connectorId)
    {
      return _context.ExecuteFunction<AssortmentContentView>(string.Format("{0}.GetAssortmentContentView", _containerName),

        new ObjectParameter("ConnectorId", connectorId));
    }

    public IEnumerable<CalculatedPriceView> GetCalculatedPriceView(int connectorId)
    {
      return _context.ExecuteFunction<CalculatedPriceView>(string.Format("{0}.GetCalculatedPriceView", _containerName),

        new ObjectParameter("ConnectorId", connectorId));
    }

    public void RegenerateSearchResults()
    {
      _context.CommandTimeout = 222200;
      _context.ExecuteFunction(string.Format("{0}.GenerateSearchResults", _containerName));
    }

    public List<Services.LanguageDescriptionModel> GetLanguageDescriptionCount(int ConnectorID, int? vendorID = null)
    {
      string query = string.Empty;
      if (!vendorID.HasValue)
      {

        query = string.Format(@"select l.languageid,
            (
            select count(*) from (
            select * from content c 
            where connectorid = 5 
            except
            select c.* from ProductDescription p 
            inner join content c on p.productid = c.productid
            where p.languageid = l.languageid and c.connectorid = {0}
            ) as  counter) as Counter
            from language l
          ", ConnectorID);
      }
      else
      {
        query = string.Format(@"
select l.languageid,
(
select count(*) from (
select * from content c 
where connectorid = 5 
except
select c.* from ProductDescription p 
inner join content c on p.productid = c.productid
where p.vendorid = {1} and p.languageid = l.languageid and c.connectorid = {0}
) as  counter) as Counter
from language l
          ", ConnectorID, vendorID.Value);
      }

      return _context.ExecuteStoreQuery<LanguageDescriptionModel>(query).ToList();
    }

    public void CalculateVendorPrices(int? VendorID = null)
    {
      ObjectParameter vendorID = null;

      if (VendorID.HasValue) vendorID = new ObjectParameter("VendorID", VendorID);
      else vendorID = new ObjectParameter("VendorID", typeof(int?));

      _context.ExecuteFunction(string.Format("{0}.CalculateVendorPrices", _containerName), vendorID);

    }

    public void CalculateConnectorPrices(int? ConnectorID = null)
    {
      ObjectParameter connectorID = null;

      if (ConnectorID.HasValue) connectorID = new ObjectParameter("VendorID", ConnectorID);
      else connectorID = new ObjectParameter("VendorID", typeof(int?));

      _context.ExecuteFunction(string.Format("{0}.CalculateConnectorPrices", _containerName), connectorID);

    }

    public IEnumerable<AttributeValueGroupingResult> GetAttributeValueGrouping(int? connectorID, int languageID)
    {
      ObjectParameter connectorObjectParameter = null;

      if (connectorID.HasValue)
      {
        connectorObjectParameter = new ObjectParameter("ConnectorID", connectorID);
      }
      else
      {
        connectorObjectParameter = new ObjectParameter("ConnectorID", typeof(int?));
      }

      return _context.ExecuteFunction<AttributeValueGroupingResult>(string.Format("{0}.GetAttributeValueGrouping", _containerName),

       connectorObjectParameter,
       new ObjectParameter("LanguageID", languageID)

       );
    }

    public void RegenarateContentFlat(int connectorID)
    {
      _context.CommandTimeout = 10 * 60;

      _context.ExecuteStoreCommand(string.Format(@"
         MERGE ContentFlat flat
          USING (
	          SELECT connectorid, productid, languageid, vendoritemnumber, brandid, brandname, vendorbrandcode, shortdescription, longdescription, linetype, ledgerclass, productdesk, extendedcatalog, productcontentid, vendorid, longcontentdescription, shortcontentdescription, productname, modelname, warrantyinfo, cutofftime, deliveryhours 
	          FROM GenerateContentViewFlat
			      where connectorid= {0}
	          ) as source
          ON	source.languageid = flat.languageid and 
	          source.productid = flat.productid and 
	          source.connectorid = flat.connectorid
	
          WHEN NOT MATCHED BY TARGET
          THEN 
	          Insert(connectorid, productid, languageid, vendoritemnumber, brandid, brandname, vendorbrandcode, shortdescription, longdescription, linetype, ledgerclass, productdesk, extendedcatalog, productcontentid, vendorid, longcontentdescription, shortcontentdescription, productname, modelname, warrantyinfo, cutofftime, deliveryhours)
	          VALUES (source.connectorid, source.productid, source.languageid, source.vendoritemnumber, source.brandid, source.brandname, source.vendorbrandcode, source.shortdescription, source.longdescription, source.linetype, source.ledgerclass, source.productdesk, source.extendedcatalog, source.productcontentid, source.vendorid, source.longcontentdescription, source.shortcontentdescription, source.productname, source.modelname, source.warrantyinfo, source.cutofftime, source.deliveryhours)
          WHEN NOT MATCHED BY SOURCE and flat.connectorid = {0}
          THEN DELETE
          WHEN MATCHED 
      
          THEN	
	          UPDATE 
		          SET	
			          flat.vendoritemnumber = source.vendoritemnumber, 
			          flat.brandid = source.brandid, 
			          flat.brandname = source.brandname, 
			          flat.vendorbrandcode = source.vendorbrandcode, 
			          flat.shortdescription = source.shortdescription, 
			          flat.longdescription = source.longdescription, 
			          flat.linetype = source.linetype, 
			          flat.ledgerclass = source.ledgerclass, 
			          flat.productdesk = source.productdesk, 
			          flat.extendedcatalog = source.extendedcatalog, 
			          flat.productcontentid = source.productcontentid, 
			          flat.longcontentdescription = source.longcontentdescription, 
			          flat.shortcontentdescription = source.shortcontentdescription, 
			          flat.productname = source.productname, 
			          flat.modelname = source.modelname, 
			          flat.warrantyinfo = source.warrantyinfo, 
			          flat.cutofftime = source.cutofftime, 
					  flat.vendorid = source.vendorid,
			          flat.deliveryhours = source.deliveryhours;




          ", connectorID));
    }
  }
}
