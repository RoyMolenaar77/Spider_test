using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using Concentrator.Objects.Web.Models;
using Concentrator.Web.Shared.Models;
using System.Configuration;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Environments;
using Concentrator.ui.Management.Models;


namespace Concentrator.ui.Management.Controllers
{
  public class
    MissingContentController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetMissingContent)]
    public ActionResult GetList(int? connectorID, ContentFilter filter, ContentPortalFilter filterPortal, bool? hasLongContentDescription)
    {
      string readyForWehKampAndSendToWehkampQuery = "";

      int? vendorIDForDescriptions = null;
      if (!connectorID.HasValue)
        connectorID = Client.User.ConnectorID;

      if (ConfigurationManager.AppSettings["CheckForDescriptionVendorID"] != null)
        vendorIDForDescriptions = int.Parse(ConfigurationManager.AppSettings["CheckForDescriptionVendorID"]);

      if (Session[ContentPortalFilter.SessionKey] != null)
      {
        filterPortal = (ContentPortalFilter)Session[ContentPortalFilter.SessionKey];
      }

      int ReadyForWehkampAttributeID = int.Parse(ConfigurationManager.AppSettings["ReadyForWehkampAttributeID"]);
      int SentToWehkampAttributeID = int.Parse(ConfigurationManager.AppSettings["SentToWehkampAttributeID"]);
      int SentToWehkampAsDummyAttributeID = int.Parse(ConfigurationManager.AppSettings["SentToWehkampAsDummyAttributeID"]);
      int WehkampProductNumberAttributeID = int.Parse(ConfigurationManager.AppSettings["WehkampProductNumberAttributeID"]);

      using (var pp = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {


        string baseQuery = string.Format(@"
        
					select 
          mc.ConcentratorProductID,
          mc.IsActive as Active,
          VendorItemNumber,
          CustomItemNumber,
          BrandName,
          ShortDescription,
          Image,
          0 as YouTube,
          Specifications,
          mc.CreationTime,
          mc.LastModificationTime,
          IsConfigurable,
					case when (select count(*) from productdescription where productid = mc.concentratorproductid and vendorid = 48) > 0 then 1 else 0 end HasDescription,
		      case when (select count(*) from productdescription where productid = mc.concentratorproductid and vendorid = 48 and LanguageID = 3) > 0 then 1 else 0 end HasFrDescription,
          QuantityOnHand,
          isnull(pav.Value, '') as ShopWeek, 
          case when pav2.Value is null then 'false' else pav2.Value end as ReadyForWehkamp, 
          case when pav3.Value is null then 'false' else pav3.Value end as SentToWehkamp, 
          case when pav4.Value is null then 'false' else pav4.Value end as SentToWehkampAsDummy,
          case when pavWehkamp.Value is null then '' else pavWehkamp.Value end as WehkampProductNumber,
          case when pavBtF.Value is null then 'false' else pavBtF.Value end as BtF           

          from missingcontent mc
          left join productattributevalue pav on pav.productid = mc.ConcentratorProductID and pav.attributeid = 75 
          
          left join productattributevalue pav2 on pav2.productid = mc.ConcentratorProductID and pav2.attributeid = {1} 
          left join productattributevalue pav3 on pav3.productid = mc.ConcentratorProductID and pav3.attributeid = {2} 
          left join productattributevalue pav4 on pav4.productid = mc.ConcentratorProductID and pav4.attributeid = {3} 
          left join productattributevalue pavBtF on pavBtF.productid = mc.ConcentratorProductID and pavBtF.attributeid in (select attributeid from productattributemetadata where attributecode = 'Btf')
          
          left join productattributevalue pavWehkamp on pavWehkamp.productid = mc.ConcentratorProductID and pavWehkamp.attributeid = {4}
          
          where mc.connectorid = {0} 
          order by VendorItemNumber
          "
                 , connectorID.Value, ReadyForWehkampAttributeID, SentToWehkampAttributeID, SentToWehkampAsDummyAttributeID, WehkampProductNumberAttributeID);

        var content = pp.Query<MissingContentViewModel>(baseQuery);

        if (filter.hasImage.HasValue && !filter.hasImage.Value)
        {
          content = pp.Query<MissingContentViewModel>(baseQuery + "and [Image] = 0");
        }

        if (filter.Media.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + string.Format(@"and YouTube = {0} and [Image] = {0}", filter.Media.Value ? 1 : 0));

        if (filter.Images.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + string.Format(@"and [Image] = {0}", filter.Images.Value ? 1 : 0));


        if (filter.Images.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + string.Format(@"and [Image] = {0}", filter.Images.Value ? 1 : 0));

        if (filter.MediaAndSpecs.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + string.Format(@"and YouTube = {0} and [Image] = {0} and Specifications = {0}", filter.MediaAndSpecs.Value ? 1 : 0));

        if (filter.Specs.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + string.Format(@"and Specifications = {0}", filter.Specs.Value ? 1 : 0));

        if (filter.Video.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + string.Format(@"and YouTube = {0}", filter.Video.Value ? 1 : 0));

        if (filter.VendorOverlay.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + @"and Contentvendor = 'Overlay'");

        if (filter.VendorBase.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + @"and Contentvendor = 'Base'");

        if (filter.PreferredContent.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + @"and ContentVendorID != null");

        if (filter.UnpreferredContent.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + @"and ContentVendorID = null");


        if (filter.ReadyForWehkamp.HasValue)
        {
          if (filter.ReadyForWehkamp == false)
            readyForWehKampAndSendToWehkampQuery += "and ((pav2.value != 'True' or pav2.value is null) or Image = 0) ";
          else
            readyForWehKampAndSendToWehkampQuery += "and pav2.value = 'True' ";
        }

        if (filter.SentToWehkamp.HasValue)
        {
          if (filter.SentToWehkamp == false)
            readyForWehKampAndSendToWehkampQuery += "and (pav3.value != 'True' or pav3.value is null)";
          else
            readyForWehKampAndSendToWehkampQuery += "and pav3.value = 'True'";
        }

        if (filter.SentToWehkampAsDummy.HasValue)
        {
          if (filter.SentToWehkampAsDummy == false)
            readyForWehKampAndSendToWehkampQuery += "and (pav4.value != 'True' or pav4.value is null)";
          else
            readyForWehKampAndSendToWehkampQuery += "and pav4.value = 'True'";
        }

        if (filter.BtF.HasValue)
        {
          if (!filter.BtF.Value)
            readyForWehKampAndSendToWehkampQuery += "and (pavBtF.value != 'True' or pavBtF.value is null)";
          else
            readyForWehKampAndSendToWehkampQuery += "and pavBtF.value = 'True'";
        }

        if (filter.SentToWehkamp.HasValue || filter.ReadyForWehkamp.HasValue || filter.BtF.HasValue)
          content = pp.Query<MissingContentViewModel>(baseQuery + readyForWehKampAndSendToWehkampQuery);

        PagingParameters pagingParams = GetPagingParams();

        var q = content.AsQueryable().Distinct().Where(x => hasLongContentDescription.HasValue ? !x.HasDescription : true);
        int total = 0;

        var result = GetPagedResultWithCount(q, out total);

        return Json(new
        {
          total,
          results = result.ToList()
        });
      }

    }

    [RequiresAuthentication(Functionalities.GetMissingContent)]
    public ActionResult GetUrgentProducts(ContentPortalFilter filter, int? connectorID)
    {
      MergeSession(filter, ContentPortalFilter.SessionKey);

      if (!connectorID.HasValue)
        connectorID = Client.User.ConnectorID;

      int ReadyForWehkampAttributeID = int.Parse(ConfigurationManager.AppSettings["ReadyForWehkampAttributeID"]);
      int SentToWehkampAttributeID = int.Parse(ConfigurationManager.AppSettings["SentToWehkampAsDummyAttributeID"]);

      using (var ppdb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        string baseQuery = string.Format(@"
					select count(mc.ProductID)
          from content mc 
          left join productattributevalue pav2 on pav2.productid = mc.ProductID and pav2.attributeid = {1} --ReadyForWehkamp
          left join productattributevalue pav3 on pav3.productid = mc.ProductID and pav3.attributeid = {2} --SentToWehkampAsDummy
          where mc.connectorid = {0} 
          and pav3.Value = 'true'
         and ((pav2.Value is null or pav2.Value = 'false') or ((select top 1 productid from productmedia where productid = mc.productid) is null))"
          , connectorID.Value, ReadyForWehkampAttributeID, SentToWehkampAttributeID);

        int countUrgentProducts = ppdb.ExecuteScalar<int>(baseQuery);

        ViewBag.CountUrgentProducts = countUrgentProducts;

        return View("UrgentProducts");

      }


    }


    [RequiresAuthentication(Functionalities.GetContent)]
    public ActionResult GetMediaAndSpecifications(ContentPortalFilter filter)
    {
      MergeSession(filter, ContentPortalFilter.SessionKey);
      using (var unit = GetUnitOfWork())
      {
        var serviceResult = ((IContentService)unit.Service<Content>()).GetMissing(filter.Connectors, filter.Vendors, filter.BeforeDate, filter.AfterDate, filter.OnDate, filter.IsActive, filter.ProductGroups, filter.Brands, filter.LowerStockCount, filter.GreaterStockCount, filter.EqualStockCount, filter.Statuses);

        if (serviceResult.Key > 0)
        {
          MediaAndSpecificationsModel model = new MediaAndSpecificationsModel();
          var content = serviceResult.Value;

          model.ProductWithoutMediaUrlAndVideo = content.Where(c => !c.YouTube).Count();
          model.ProductsWithoutImage = content.Where(c => !c.Image).Count();
          model.ProductsWithoutMedia = content.Where(c => !c.Image && !c.YouTube).Count();
          model.ProductsWithoutSpecs = content.Where(c => !c.Specifications).Count();
          model.ProductsWithoutMediaAndSpecs = content.Where(c => !c.Image && !c.YouTube && !c.Specifications).Count();

          model.TotalProducts = serviceResult.Key;

          return View("MediaAndspecifications", model);
        }
        else
        {
          return HtmlError();
        }
      }
    }

    [RequiresAuthentication(Functionalities.UpdateContent)]
    public ActionResult Update(int id, bool? sentToWehkamp)
    {
      if (!sentToWehkamp.HasValue || sentToWehkamp.Value)
        return Success("Nothing to update - SentToWehkamp can only be unchecked");

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var product = unit.Service<Product>().Get(c => c.ProductID == id);

          int SentToWehkampAttributeID = int.Parse(ConfigurationManager.AppSettings["SentToWehkampAttributeID"]);
          ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeValue(product, SentToWehkampAttributeID, sentToWehkamp.Value.ToString(), true);

          unit.Save();
          return Success("Successfully update product");
        }
      }
      catch (Exception e)
      {
        return Failure(e.Message);
      }
    }
  }

  public class MissingContentViewModel
  {
    public string ConcentratorProductID { get; set; }
    public bool Active { get; set; }
    public string VendorItemNumber { get; set; }
    public string CustomItemNumber { get; set; }
    public string BrandName { get; set; }
    public string ShortDescription { get; set; }
    public bool Image { get; set; }
    public bool YouTube { get; set; }
    public bool Specifications { get; set; }

    private DateTime? _creationTime;
    private DateTime? _lastModificationTime;

    public DateTime? CreationTime
    {
      get
      {
        return _creationTime;
      }
      set
      {
        _creationTime = value.ToNullOrLocal();
      }
    }
    public DateTime? LastModificationTime
    {
      get
      {
        return _lastModificationTime;
      }
      set
      {
        _lastModificationTime = value.ToNullOrLocal();
      }
    }
    public bool IsConfigurable { get; set; }
    public string ShopWeek { get; set; }
    public bool HasDescription { get; set; }
    public bool HasFrDescription { get; set; }
    public int QuantityOnHand { get; set; }
    public bool SentToWehkamp { get; set; }
    public bool ReadyForWehkamp { get; set; }
    public bool SentToWehkampAsDummy { get; set; }
    public string WehkampProductNumber { get; set; }
    public bool BtF { get; set; }
  }
}
