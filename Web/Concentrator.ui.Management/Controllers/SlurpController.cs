using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Enumerations;

namespace Concentrator.ui.Management.Controllers
{
  public class SlurpController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetSlurp)]
    public ActionResult GetSlurpScheduleList()
    {
      return List(unit => (from s in unit.Service<SlurpSchedule>().GetAll().ToList()
                           let productName = s.Product != null ?
                           s.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                           s.Product.ProductDescriptions.FirstOrDefault() : null
                           let productGroupName = s.ProductGroupMapping != null ?(s.ProductGroupMapping.ProductGroup != null ? 
                              s.ProductGroupMapping.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                              s.ProductGroupMapping.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null) : null
                           select new
                           {
                             s.SlurpScheduleID,
                             s.ProductCompareSourceID,
                             Source = s.ProductCompareSource.Source,
                             s.ProductGroupMappingID,
                             ProductGroupName = productGroupName != null ? productGroupName.Name : string.Empty,
                             s.ProductID,
                             ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
                             s.Product != null ? s.Product.VendorItemNumber : string.Empty,
                             s.Interval,
                             IntervalName = Enum.GetName(typeof(IntervalType), s.IntervalType),
                             s.IntervalType
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateSlurp)]
    public ActionResult CreateSlurpSchedule()
    {
      return Create<SlurpSchedule>();
    }

    [RequiresAuthentication(Functionalities.UpdateSlurp)]
    public ActionResult UpdateSlurpSchedule(int id)
    {
      return Update<SlurpSchedule>(x => x.SlurpScheduleID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteSlurp)]
    public ActionResult DeleteSlurpSchedule(int id)
    {
      var queue = GetUnitOfWork().Service<SlurpQueue>().Get(x => x.SlurpScheduleID == id);
      if (queue != null)
      {
        Exception e = new Exception("A record in Slurp Queue is depending on this record");

        return Failure("Failed to delete object. ", e);
      }
      else
      {
        return Delete<SlurpSchedule>(x => x.SlurpScheduleID == id);
      }
    }

    [RequiresAuthentication(Functionalities.GetSlurp)]
    public ActionResult GetSlurpQueueList()
    {
      return List(unit => (from s in unit.Service<SlurpQueue>().GetAll().ToList()
                           let productName = s.Product != null ?
                           s.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                           s.Product.ProductDescriptions.FirstOrDefault() : null
                           select new
                           {
                             s.QueueID,
                             s.ProductID,
                             ProductDescription = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
                             s.Product != null ? s.Product.VendorItemNumber : string.Empty,
                             s.ProductCompareSourceID,
                             Source = s.ProductCompareSource.Source,
                             s.SlurpScheduleID,
                             s.CreationTime,
                             s.CompletionTime,
                             s.IsCompleted
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateSlurp)]
    public ActionResult CreateSlurpQueue()
    {
      return Create<SlurpQueue>();
    }

    [RequiresAuthentication(Functionalities.UpdateSlurp)]
    public ActionResult UpdateSlurpQueue(int id)
    {
      return Update<SlurpQueue>(x => x.QueueID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteSlurp)]
    public ActionResult DeleteSlurpQueue(int id)
    {
      return Delete<SlurpQueue>(x => x.QueueID == id);
    }

    [RequiresAuthentication(Functionalities.GetSlurp)]
    public ActionResult GetIntervalType()
    {
      //string[] names = Enum.GetNames(typeof(IntervalType));
      //return Json(new
      //{
      //  results = SimpleList<SlurpSchedule>(c => new
      //  {
      //    IntervalType = c.IntervalType,
      //    IntervalName = Enum.GetName(typeof(IntervalType), c.IntervalType)//names[c.IntervalType],
      //  })
      //});

      List<IntervalType> enums = EnumHelper.EnumToList<IntervalType>();

      return Json(new
      {
        results = (from e in enums
                   select new
                   {
                     IntervalType = (int)e,
                     IntervalName = Enum.GetName(typeof(IntervalType), e)
                   }).ToArray()
      });

    }


    [RequiresAuthentication(Functionalities.GetSlurp)]
    public ActionResult GetProductGroupMappingStore()
    {
      return Json(new
      {
        results = List(unit => (from c in unit.Service<ProductGroupMapping>().GetAll()
                   let productGroupName = c.ProductGroupID != null ?
                     c.ProductGroup.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                     c.ProductGroup.ProductGroupLanguages.FirstOrDefault() : null
                   select new
                   {
                     ProductGroupMappingID = (int)c.ProductGroupMappingID,
                     ProductGroupName = productGroupName
                   }).AsQueryable())
      });
        
      //  SimpleList<ProductGroupMapping>(c => new
      //  {
      //    ProductGroupMappingID = c.ProductGroupMappingID,
      //    ProductGroupName = c.ProductGroup.
      //  })
      //});
    }

  }
}
