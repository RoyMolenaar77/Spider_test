using System;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using System.Configuration;

namespace Concentrator.Plugins.MissingDescriptionGeneration
{
  public class GenerateMissingDescriptions : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Missing Description Generator"; }
    }

    protected override void Process()
    {
      var vendorIDs = GetConfiguration().AppSettings.Settings["SourceVendorIDs"].Value.Split(',').Select(int.Parse).ToList();

      foreach (var vendorId in vendorIDs)
      {
        log.InfoFormat("Starting regeneration of missing descriptions for VendorID: {0}", vendorId);

        using (var pDb = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
        {
          var vendorIDsUsedForEnrichment = pDb.Query<int>("select vendorid from vendor where vendortype & @0 = @0", (int)VendorType.UsedForProductEnrichment);
          int attributeIDForMaterial = int.Parse(ConfigurationManager.AppSettings["MaterialAttributeID"]);
          int attributeIDForMaterialDescription = int.Parse(ConfigurationManager.AppSettings["MaterialDescriptionAttributeID"]);
          try
          {
            var insertQuery = string.Format(@"INSERT INTO ProductDescription 
                                                (productid, 
                                                languageid ,
                                                vendorid, 
                                                ShortContentDescription, 
                                                LongContentDescription, 
                                                productname, 
                                                createdby, 
                                                creationtime)
                                              SELECT 
                                                pColor.productid, 
                                                pd.languageid, 
                                                pd.vendorid, 
                                                pd.ShortContentDescription, 
                                                pd.LongContentDescription, 
                                                pd.ProductName, 
                                                1, 
                                                getdate()  
                                              FROM Product p
                                              INNER JOIN product pColor on pColor.parentproductid = p.productid
                                              INNER JOIN ProductDescription pd on pd.productid = p.productid and pd.vendorid in ({1})
                                              LEFT JOIN ProductDescription pdColor on pColor.productid = pdColor.productid and pdColor.vendorid = pd.vendorid
                                              WHERE p.SourceVendorID = {0} and 
                                                    (p.isconfigurable = 1 and p.parentproductid is null) and 
                                                    (pColor.isconfigurable = 1 and pColor.ParentProductID  is not null) and 
                                                    pdColor.productid is null", vendorId, string.Join(",", vendorIDsUsedForEnrichment));

            pDb.Execute(insertQuery);


            pDb.Execute(string.Format(@"insert into productattributevalue (productid, attributeid, value, languageid, createdby, creationtime)
	select pColor.productID, pav.attributeid, pav.value, pav.languageid, pav.createdby, pav.creationtime 
	from productattributevalue pav 
	inner join product pParent on pParent.productid = pav.productid
	inner join product pColor on pColor.parentproductid = pParent.productid
	left join productattributevalue pavColor on pavColor.productid = pColor.productid and pavColor.attributeid = pav.attributeid
	where (pParent.ParentProductID is null and pParent.IsConfigurable = 1) and (pColor.isconfigurable = 1 and pColor.ParentProductID is not null) and pav.attributeid = {1} and pavColor.productid is null and pParent.sourcevendorid = {0}", vendorId, attributeIDForMaterial));


            pDb.Execute(string.Format(@"insert into productattributevalue (productid, attributeid, value, languageid, createdby, creationtime)
	select pColor.productID, pav.attributeid, pav.value, pav.languageid, pav.createdby, pav.creationtime 
	from productattributevalue pav 
	inner join product pParent on pParent.productid = pav.productid
	inner join product pColor on pColor.parentproductid = pParent.productid
	left join productattributevalue pavColor on pavColor.productid = pColor.productid and pavColor.attributeid = pav.attributeid
	where (pParent.ParentProductID is null and pParent.IsConfigurable = 1) and (pColor.isconfigurable = 1 and pColor.ParentProductID is not null) and pav.attributeid = {1} and pavColor.productid is null and pParent.sourcevendorid = {0}", vendorId, attributeIDForMaterialDescription));

            log.AuditSuccess("Missing descriptions were regenerated successfully", "Missing Description Generator");
          }
          catch (Exception e)
          {
            log.AuditFatal("Missing description generation failed", e, "Missing Description Generator");
          }
        }
          
        log.InfoFormat("Finished regeneration of missing descriptions for VendorID: {0}", vendorId);
      }
    }
  }
}
