using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.ui.Management.Models;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Web.Models;
using Concentrator.Objects.Models.Products;
using Concentrator.ui.Management.Models.Anychart;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Web.Shared.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class NotificationsController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetNotifications()
    {
      var connectorID = Client.User.ConnectorID;
      connectorID.ThrowIf(c => !c.HasValue);

      using (var unit = GetUnitOfWork())
      {
        var notifications = ((IContentService)unit.Service<Content>()).GetIncompleteMappingInfo(connectorID.Value);

        return PartialView("MappingNotifications", new MappingNotificationViewModel
        {
          ProductMatches = notifications.ProductMatches,
          MissingSpecifications = notifications.MissingSpecifications,
          ProductsNoImages = notifications.ProductsNoImages,
          UnmatchedConnectorStatuses = notifications.UnmatchedConnectorStatuses,
          UnmatchedBrands = notifications.UnmatchedBrands,
          UnmatchedProductGroupsCount = notifications.UnmatchedProductGroupsCount,
          UnmatchedVendorStatuses = notifications.UnmatchedVendorStatuses,
          ProductsSmallImages = notifications.ProductsSmallImages
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetProductsPerVendor()
    {
      using (var unit = GetUnitOfWork())
      {
        var overlayProducts = unit.Service<VendorAssortment>().GetAll(c => c.Vendor.Name == "Overlay").Select(c => c.VendorID).Count();
        var baseProducts = unit.Service<VendorAssortment>().GetAll(c => c.Vendor.Name == "Base").Select(c => c.VendorID).Count();
        var productsInOtherVendor = unit.Service<VendorAssortment>().GetAll(c => c.Vendor.Name != "Base" && c.Vendor.Name != "Overlay").Select(c => c.ProductID).Distinct().Count();

        var callbackUI = "products-item";

        var vendorOverlay = unit.Service<Vendor>().Get(x => x.Name == "Overlay").VendorID;
        var vendorBase = unit.Service<Vendor>().Get(x => x.Name == "Base").VendorID;
        var remainder = (from i in unit.Service<Vendor>().GetAll(x => x.Name != "Base" && x.Name != "Overlay") select i.VendorID).ToList();

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>(){
          new PieChartPoint("Products in Overlay",   overlayProducts, action: new AnychartAction(callbackUI, new { VendorIdentification = vendorOverlay })),
          new PieChartPoint("Products in Base", baseProducts, action: new AnychartAction(callbackUI, new { VendorIdentification = vendorBase })),
          new PieChartPoint("Products in Remainder", productsInOtherVendor, action: new AnychartAction(callbackUI, new { RemainderIdentification = string.Join(",",remainder) } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultAccumulationChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetMatchedProducts()
    {
      using (var unit = GetUnitOfWork())
      {
        var unmatchedProducts = unit.Service<ProductMatch>().GetAll(x => x.isMatched == false).Select(x => x.ProductMatchID).Count();
        var matchedProducts = unit.Service<ProductMatch>().GetAll(x => x.isMatched == true).Select(x => x.ProductMatchID).Count();

        var callbackUI = "product-matching";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>(){
          new PieChartPoint("Matched products",   matchedProducts, action: new AnychartAction(callbackUI, new { MatchedProduct = true } )),
          new PieChartPoint("Non-matched products", unmatchedProducts, action: new AnychartAction(callbackUI, new { UnmatchedProduct = true } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultAccumulationChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetActiveProductPerGroup()
    {
      using (var unit = GetUnitOfWork())
      {
        var missingActiveProducts = (from v in unit.Service<VendorAssortment>().GetAll(v => v.IsActive == true)
                                     where !(v.Product.ContentProductGroups.Count() > 0)
                                     select v.ProductID).Count();

        var activeProducts = (from v in unit.Service<VendorAssortment>().GetAll(v => v.IsActive == true)
                              where v.Product.ContentProductGroups.Count() > 0
                              select v.ProductID).Count();

        var callbackUI = "vendor-assortment-item";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>(){
          new PieChartPoint("Missing Active Products",   missingActiveProducts, action: new AnychartAction(callbackUI, new {  MissingActiveProducts = true } )),
          new PieChartPoint("Active Products", activeProducts, action: new AnychartAction(callbackUI, new { ActiveProducts = true } )),          
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetActiveProductGroups()
    {
      using (var unit = GetUnitOfWork())
      {
        var missingActiveGroups = (from v in unit.Service<ProductGroupVendor>().GetAll(v => v.ProductGroupID < 0)
                                   select v.ProductGroupVendorID).Count();

        var activeGroups = (from v in unit.Service<ProductGroupVendor>().GetAll(v => v.ProductGroupID > 0)
                            select v.ProductGroupVendorID).Count();

        var callbackUI = "vendor-product-group-mapping";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>() {          
          new PieChartPoint("Products in Remainder", activeGroups, action: new AnychartAction(callbackUI, new { ActivePgvIdentification = true } )),
          new PieChartPoint("Missing Active Product Groups",   missingActiveGroups, action: new AnychartAction(callbackUI, new { UnactivePgvIdentification = true } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetMappedBrandVendors(ContentPortalFilter filter)
    {
      using (var unit = GetUnitOfWork())
      {
        var filteredBrandVendors = (unit.Service<BrandVendor>().GetAll(v => v.BrandID > 0));

        var unmappedBrandVendors = (from v in unit.Service<BrandVendor>().GetAll(v => v.BrandID < 0)
                                    select v.BrandID).Count();

        var mappedBrandVendors = (from v in unit.Service<BrandVendor>().GetAll(v => v.BrandID > 0)
                                  select v.BrandID).Count();

        if (filter.Connectors != null && filter.Connectors.Count() > 0)
        {
          filteredBrandVendors = filteredBrandVendors.Where(x => x.Vendor.ContentProducts.Any(v => filter.Connectors.Contains(v.ConnectorID)));
          mappedBrandVendors = filteredBrandVendors.Count();
        }
        if (filter.Vendors != null && filter.Vendors.Count() > 0)
        {
          filteredBrandVendors = filteredBrandVendors.Where(x => x.Vendor.ContentProducts.Any(v => filter.Connectors.Contains(v.ConnectorID)));
        }
        //if (filter.AfterDate != null)
        //{ 
        //  filteredBrandVendors = filteredBrandVendors.Where(x => x.
        //}
        if (filter.IsActive != null && filter.IsActive == true)
        {
          filteredBrandVendors = filteredBrandVendors.Where(x => x.Vendor.VendorAssortments.Any(v => v.IsActive == true));
        }
        if (filter.IsActive != null && filter.IsActive == false)
        {
          filteredBrandVendors = filteredBrandVendors.Where(x => x.Vendor.VendorAssortments.Any(v => v.IsActive == false));
        }
        if (filter.ProductGroups != null && filter.ProductGroups.Count() > 0)
        {
          filteredBrandVendors = filteredBrandVendors.Where(x => x.Vendor.ProductGroupVendors.Any(v => filter.ProductGroups.Contains(v.ProductGroupID)));
        }
        if (filter.Brands != null && filter.Brands.Count() > 0)
        {
          filteredBrandVendors = filteredBrandVendors.Where(x => filter.Brands.Contains(x.BrandID));
        }
        if (filter.EqualStockCount != null)
        {

        }

        var callbackUI = "vendor-brands-item";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>() {
          new PieChartPoint("Unmapped Brand Vendors",   unmappedBrandVendors, action: new AnychartAction(callbackUI, new { UnmappedBrandVendor = true } )), 
          new PieChartPoint("Mapped Brand Vendors",   mappedBrandVendors, action: new AnychartAction(callbackUI, new { MappedBrandVendor = true }))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultAccumulationChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetPreferredContentPerVendor()
    {
      using (var unit = GetUnitOfWork())
      {
        var preferredContent = unit.Service<MissingContent>().GetAll(x => x.ContentVendorID != null).Count();
        var unpreferredContent = unit.Service<MissingContent>().GetAll(x => x.ContentVendorID == null).Count();

        if (preferredContent == 0 && unpreferredContent == 0)
        {
          return AnychartError();
        }

        var callbackUI = "content-item";

        var serie = new Serie(new List<Point>() 
        {
          new PieChartPoint("Preferred Content",   preferredContent, action: new AnychartAction(callbackUI, new { PreferredContent = true } )), 
          new PieChartPoint("Unpreferred Content", unpreferredContent, action: new AnychartAction(callbackUI, new { UnpreferredContent = true } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetEdiOrdersForToday(ContentPortalFilter filter)
    {
      using (var unit = GetUnitOfWork())
      {
        var processed = unit.Service<EdiOrderListener>().GetAll(x => x.Processed == true);

        var unprocessed = unit.Service<EdiOrderListener>().GetAll(x => x.Processed == false);

        if (filter.OnDate != null || filter.BeforeDate != null || filter.AfterDate != null)
        {
          processed = processed.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
                          || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
                          || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          unprocessed = unprocessed.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
                           || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
                           || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);
        }

        var callbackUI = "edi-order-listener-item";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>() {          
                  new PieChartPoint("Processed Orders", processed.Count(), action: new AnychartAction(callbackUI, new { ProcessedOrder = true } )),
                  new PieChartPoint("Unprocessed Orders",   unprocessed.Count(), action: new AnychartAction(callbackUI, new { UnprocessedOrder = true } ))
                }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }


    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult PuplishedProducts()
    {
      using (var unit = GetUnitOfWork())
      {
        var publishedProducts = unit.Service<ContentProductGroup>().GetAll(x => x.IsCustom == true).Count();
        var unpublishedProducts = unit.Service<ContentProductGroup>().GetAll(x => x.IsCustom == false).Count();

        if (publishedProducts == 0 && unpublishedProducts == 0)
        {
          return AnychartError();
        }

        var callbackUI = "products-item";

        var serie = new Serie(new List<Point>() {
          new PieChartPoint("Published Products",   publishedProducts, action: new AnychartAction(callbackUI, new { PublishedProduct = true } )), 
          new PieChartPoint("Unpublished Products", unpublishedProducts, action: new AnychartAction(callbackUI, new { UnpublishedProduct = true } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetEdiOrdersInError(ContentPortalFilter filter)
    {
      using (var unit = GetUnitOfWork())
      {
        //var Received = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 0).Count();
        //var Validate = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 100).Count();
        //var ProcessOrder = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 200).Count();
        //var WaitForOrderResponse = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 300).Count();
        //var WaitForAcknowledgement = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 310).Count();
        //var ReceiveAcknowledgement = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 315).Count();
        //var WaitForShipmentNotification = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 320).Count();
        //var ReceiveShipmentNotification = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 325).Count();
        //var WaitForInvoiceNotification = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 330).Count();
        //var ReceivedInvoiceNotification = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 335).Count();
        //var OrderComplete = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 500).Count();
        //var Error = unit.Service<EdiOrder>().GetAll(x => x.ReceivedDate == DateTime.Today && x.Status == 999).Count();

        var Received = unit.Service<EdiOrder>().GetAll(x => x.Status == 0);
        var Validate = unit.Service<EdiOrder>().GetAll(x => x.Status == 100);
        var ProcessOrder = unit.Service<EdiOrder>().GetAll(x => x.Status == 200);
        var WaitForOrderResponse = unit.Service<EdiOrder>().GetAll(x => x.Status == 300);
        var WaitForAcknowledgement = unit.Service<EdiOrder>().GetAll(x => x.Status == 310);
        var ReceiveAcknowledgement = unit.Service<EdiOrder>().GetAll(x => x.Status == 315);
        var WaitForShipmentNotification = unit.Service<EdiOrder>().GetAll(x => x.Status == 320);
        var ReceiveShipmentNotification = unit.Service<EdiOrder>().GetAll(x => x.Status == 325);
        var WaitForInvoiceNotification = unit.Service<EdiOrder>().GetAll(x => x.Status == 330);
        var ReceivedInvoiceNotification = unit.Service<EdiOrder>().GetAll(x => x.Status == 335);
        var OrderComplete = unit.Service<EdiOrder>().GetAll(x => x.Status == 500);
        var Error = unit.Service<EdiOrder>().GetAll(x => x.Status == 999);

        if (filter.OnDate != null || filter.BeforeDate != null || filter.AfterDate != null)
        {
          Received = Received.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          Validate = Validate.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          ProcessOrder = ProcessOrder.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          WaitForOrderResponse = WaitForOrderResponse.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          WaitForAcknowledgement = WaitForAcknowledgement.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          ReceiveAcknowledgement = ReceiveAcknowledgement.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          WaitForShipmentNotification = WaitForShipmentNotification.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          ReceiveShipmentNotification = ReceiveShipmentNotification.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          WaitForInvoiceNotification = WaitForInvoiceNotification.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
              || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          ReceivedInvoiceNotification = ReceivedInvoiceNotification.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
          || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          OrderComplete = OrderComplete.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
          || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);

          Error = Error.Where(x => filter.OnDate != null ? x.ReceivedDate == filter.OnDate : true
              || filter.BeforeDate != null ? x.ReceivedDate < filter.BeforeDate : true
          || filter.AfterDate != null ? x.ReceivedDate > filter.AfterDate : true);
        }

        var callbackUI = "edi-order-item";

        var serie = new Serie(new List<Point>() {
                  new PieChartPoint("Received", Received.Count(), action: new AnychartAction(callbackUI, new { Status = 0 } )), 
                  new PieChartPoint("Validate", Validate.Count(), action: new AnychartAction(callbackUI, new { Status = 100 } )),  
                  new PieChartPoint("Process Order", ProcessOrder.Count(), action: new AnychartAction(callbackUI, new { Status = 200 } )), 
                  new PieChartPoint("Wait For Order Response", WaitForOrderResponse.Count(), action: new AnychartAction(callbackUI, new { Status = 300 } )), 
                  new PieChartPoint("Wait For Acknowledgement", WaitForAcknowledgement.Count(), action: new AnychartAction(callbackUI, new { Status = 310 } )), 
                  new PieChartPoint("Receive Acknowledgement", ReceiveAcknowledgement.Count(), action: new AnychartAction(callbackUI, new { Status = 315 } )), 
                  new PieChartPoint("Wait For Shipment Notification", WaitForShipmentNotification.Count(), action: new AnychartAction(callbackUI, new { Status = 320 } )), 
                  new PieChartPoint("Receive Shipment Notification", ReceiveShipmentNotification.Count(), action: new AnychartAction(callbackUI, new { Status = 325 } )), 
                  new PieChartPoint("Wait For Invoice Notification", WaitForInvoiceNotification.Count(), action: new AnychartAction(callbackUI, new { Status = 330 } )), 
                  new PieChartPoint("Received Invoice Notification", ReceivedInvoiceNotification.Count(), action: new AnychartAction(callbackUI, new { Status = 335 } )), 
                  new PieChartPoint("Order Complete", OrderComplete.Count(), action: new AnychartAction(callbackUI, new { Status = 500 } )),
                  new PieChartPoint("Error", Error.Count(), action: new AnychartAction(callbackUI, new { Error = true } )), 
                }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultAccumulationChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetEdiOrdersSendBack(ContentPortalFilter filter)
    {
      using (var unit = GetUnitOfWork())
      {
        //var Acknowledgement = unit.Service<EdiOrderResponse>().GetAll(x => x.ReceiveDate == DateTime.Today && x.ResponseType == 100).Count();
        //var CancelNotification = unit.Service<EdiOrderResponse>().GetAll(x => x.ReceiveDate == DateTime.Today && x.ResponseType == 110).Count();
        //var ShipmentNotification = unit.Service<EdiOrderResponse>().GetAll(x => x.ReceiveDate == DateTime.Today && x.ResponseType == 200).Count();
        //var InvoiceNotification = unit.Service<EdiOrderResponse>().GetAll(x => x.ReceiveDate == DateTime.Today && x.ResponseType == 300).Count();
        //var PurchaseAcknowledgement = unit.Service<EdiOrderResponse>().GetAll(x => x.ReceiveDate == DateTime.Today && x.ResponseType == 400).Count();

        var Acknowledgement = unit.Service<EdiOrderResponse>().GetAll(x => x.ResponseType == 100);
        var CancelNotification = unit.Service<EdiOrderResponse>().GetAll(x => x.ResponseType == 110);
        var ShipmentNotification = unit.Service<EdiOrderResponse>().GetAll(x => x.ResponseType == 200);
        var InvoiceNotification = unit.Service<EdiOrderResponse>().GetAll(x => x.ResponseType == 300);
        var PurchaseAcknowledgement = unit.Service<EdiOrderResponse>().GetAll(x => x.ResponseType == 400);

        if (filter.OnDate != null || filter.BeforeDate != null || filter.AfterDate != null)
        {
          Acknowledgement = Acknowledgement.Where(x => filter.OnDate != null ? x.ReceiveDate == filter.OnDate : true
                          || filter.BeforeDate != null ? x.ReceiveDate < filter.BeforeDate : true
                          || filter.AfterDate != null ? x.ReceiveDate > filter.AfterDate : true);

          CancelNotification = CancelNotification.Where(x => filter.OnDate != null ? x.ReceiveDate == filter.OnDate : true
                           || filter.BeforeDate != null ? x.ReceiveDate < filter.BeforeDate : true
                           || filter.AfterDate != null ? x.ReceiveDate > filter.AfterDate : true);

          ShipmentNotification = ShipmentNotification.Where(x => filter.OnDate != null ? x.ReceiveDate == filter.OnDate : true
                           || filter.BeforeDate != null ? x.ReceiveDate < filter.BeforeDate : true
                           || filter.AfterDate != null ? x.ReceiveDate > filter.AfterDate : true);

          InvoiceNotification = InvoiceNotification.Where(x => filter.OnDate != null ? x.ReceiveDate == filter.OnDate : true
                           || filter.BeforeDate != null ? x.ReceiveDate < filter.BeforeDate : true
                           || filter.AfterDate != null ? x.ReceiveDate > filter.AfterDate : true);

          PurchaseAcknowledgement = PurchaseAcknowledgement.Where(x => filter.OnDate != null ? x.ReceiveDate == filter.OnDate : true
                          || filter.BeforeDate != null ? x.ReceiveDate < filter.BeforeDate : true
                          || filter.AfterDate != null ? x.ReceiveDate > filter.AfterDate : true);
        }

        var callbackUI = "edi-order-item";

        var serie = new Serie(new List<Point>()
        {
          new PieChartPoint("Acknowledgement", Acknowledgement.Count(), action: new AnychartAction(callbackUI, new { ResponseType = 100 } )),           
          new PieChartPoint("Cancel Notification", CancelNotification.Count(), action: new AnychartAction(callbackUI, new { ResponseType = 110 } )),           
          new PieChartPoint("Shipment Notification", ShipmentNotification.Count(), action: new AnychartAction(callbackUI, new { ResponseType = 200 } )),           
          new PieChartPoint("Invoice Notification", InvoiceNotification.Count(), action: new AnychartAction(callbackUI, new { ResponseType = 300 } )),           
          new PieChartPoint("Purchase Acknowledgement", PurchaseAcknowledgement.Count(), action: new AnychartAction(callbackUI, new { ResponseType = 400 } )),
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });
        //return View("Anychart/DefaultAccumulationChart", model);
        return View("Anychart/DefaultPieChart", model);
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetTopTenProducts()
    {
      using (var unit = GetUnitOfWork())
      {
        var notifications = unit.Service<EdiOrderLine>().GetAll(x => x.EdiOrder.ReceivedDate == DateTime.Today).GroupBy(x => x.CustomerItemNumber).OrderByDescending(x => x.Count()).Take(10).Select(x => new { x.Key, count = x.Count() }).ToList();

        List<TopTenProductViewModel> topTenList = new List<TopTenProductViewModel>();
        notifications.ForEach(x =>
        {
          var va = unit.Service<VendorAssortment>().GetAll(y => y.CustomItemNumber == x.Key).FirstOrDefault();

          topTenList.Add(new TopTenProductViewModel
          {
            CustomItemNumber = x.Key,
            Count = x.count,
            ProductName = va != null ? va.ShortDescription : string.Empty,
            ProductID = va.ProductID
          });
        });

        return PartialView("TopTenProducts", topTenList);
      }
    }

    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetMissingPrices()
    {
      using (var unit = GetUnitOfWork())
      {
        var missingPrices = unit.Service<ContentPrice>().GetAll(x => x.FixedPrice == null).Select(x => x.ContentPriceRuleID).Count();
        var activePrices = unit.Service<ContentPrice>().GetAll(x => x.FixedPrice != null).Select(x => x.ContentPriceRuleID).Count();

        var callbackUI = "content-prices-item";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>(){
          new PieChartPoint("Missing Prices",   missingPrices, action: new AnychartAction(callbackUI, new { MissingPrice = true } )),
          new PieChartPoint("Active Prices", activePrices, action: new AnychartAction(callbackUI, new { ActivePrice = true } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultAccumulationChart", model);
      }
    }


    [RequiresAuthentication(Functionalities.GetNotifications)]
    public ActionResult GetActiveProducts()
    {
      using (var unit = GetUnitOfWork())
      {
        //var inactiveProducts = unit.Service<VendorAssortment>().GetAll(x => x.IsActive == false).Select(x => x.VendorAssortmentID).Count();
        //var activeProducts = unit.Service<VendorAssortment>().GetAll(x => x.IsActive == true).Select(x => x.VendorAssortmentID).Count();
        var activeProducts = unit.Service<VendorAssortment>().GetAll(x => x.IsActive == true).Count();
        var inactiveProducts = unit.Service<VendorAssortment>().GetAll(x => x.IsActive == false).Count();

        var callbackUI = "vendor-assortment-item";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>(){
          new PieChartPoint("Active products",   activeProducts , action: new AnychartAction(callbackUI, new { ActiveProducts = true } )),
          new PieChartPoint("Inactive products", inactiveProducts, action: new AnychartAction(callbackUI, new { ActiveProducts = false } ))
        }, "Statistics", "Default");

        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }

  }
}
