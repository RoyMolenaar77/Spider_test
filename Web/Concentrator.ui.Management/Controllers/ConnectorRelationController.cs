using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Enumerations;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;


namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorRelationController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetList(int? connectorRelationID)
    {
      return List(unit => (from cr in unit.Service<ConnectorRelation>().GetAll().ToList()
                           where connectorRelationID.HasValue ? cr.ConnectorRelationID == connectorRelationID.Value : true
                           select new
                           {
                             cr.ConnectorRelationID,
                             cr.CustomerID,
                             cr.LanguageID,
                             cr.Name,
                             cr.Contact,
                             cr.AdministrationCode,
                             cr.OrderType,
                             cr.IsActive,
                             cr.FreightProduct,
                             cr.FinChargesProduct,
                             cr.EdiVendorID,
                             ConnectorType = cr.ConnectorType.ToString(),
                             cr.XtractType,
                             EdiVendor = cr.EdiVendor != null ? cr.EdiVendor.Name : String.Empty,
                             cr.OrderConfirmation,
                             cr.ShipmentConfirmation,
                             cr.InvoiceConfirmation,
                             cr.OutboundInvoiceConfirmation,
                             cr.OutboundShipmentConfirmation,
                             cr.OutboundOrderConfirmation,
                             cr.OutboundTo,
                             cr.OutboundPassword,
                             cr.OutboundUsername,
                             OutboundMessageType = cr.OutboundMessageType.ToString(),
                             cr.AuthorisationAddresses,
                             AccountPrivileges = cr.AccountPrivileges.ToString(),
                             cr.UseFtp,
                             ProviderType = cr.ProviderType.ToString(),
                             FtpType = cr.ProviderType.ToString(),
                             cr.FtpFrequency,
                             cr.FtpAddress,
                             cr.FtpPass,
                             cr.FtpPort,
                             cr.Password,
                             cr.FtpUserName
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateConnectorRelation)]
    public ActionResult Create(int LanguageID)
    {
      return Create<ConnectorRelation>();
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorRelation)]
    public ActionResult Delete(string _CustomerID)
    {
      return Delete<ConnectorRelation>(c => c.CustomerID == _CustomerID);
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorRelation)]
    public ActionResult Update(int _ConnectorRelationID)
    {
      return Update<ConnectorRelation>(c => c.ConnectorRelationID == _ConnectorRelationID);
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetConnectorTypes()
    {
      return Json(new
      {
        connectorTypes = (from c in Enums.Get<ConnectorTypeEnum>().AsQueryable()
                   select new
                   {
                     ConnectorTypeName = c.ToString(),
                     ConnectorTypeID = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetOutboundMessageTypes()
    {
      return Json(new
      {
        results = (from c in Enums.Get<OutboundMessageTypeEnum>().AsQueryable()
                   select new
                   {
                     OutboundMessageTypeName = c.ToString(),
                     OutboundMessageTypeID = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetAccountPrivileges()
    {
      return Json(new
      {
        results = (from c in Enums.Get<AccountPrivilegesEnum>().AsQueryable()
                   select new
                   {
                     AccountPrivilegeName = c.ToString(),
                     AccountPrivilegeID = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetProviderTypes()
    {
      return Json(new
      {
        results = (from c in Enums.Get<ProviderTypeEnum>().AsQueryable()
                   select new
                   {
                     ProviderTypeName = c.ToString(),
                     ProviderTypeID = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetFtpTypes()
    {
      return Json(new
      {
        results = (from c in Enums.Get<FtpTypeEnum>().AsQueryable()
                   select new
                   {
                     FtpTypeName = c.ToString(),
                     FtpTypeID = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetXtractTypes()
    {
      return Json(new
      {
        xtractTypes = (from c in Enums.Get<XractTypeEnum>().AsQueryable()
                   select new
                   {
                     XtractTypeName = c.ToString(),
                     XtractTypeID = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnectorRelation)]
    public ActionResult GetExportList(int connectorRelationID)
    {
      return List(unit => (from cr in unit.Service<ConnectorRelationExport>().GetAll(x => x.ConnectorRelationID == connectorRelationID).ToList()                           
                           select new
                           {
                             cr.ConnectorRelationExportID,
                             cr.ConnectorRelationID,
                             cr.SourcePath,
                             cr.DestinationPath
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateConnectorRelation)]
    public ActionResult CreateConnectorRelationExport(int ConnectorRelationID, string _DestinationPath)
    {
      return Create<ConnectorRelationExport>(onCreatingAction: (unit, cps) =>
      {
        cps.DestinationPath = _DestinationPath ?? string.Empty;
      });
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorRelation)]
    public ActionResult DeleteConnectorRelationExport(int _connectorRelationExportID)
    {
      return Delete<ConnectorRelationExport>(c => c.ConnectorRelationExportID == _connectorRelationExportID);
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorRelation)]
    public ActionResult UpdateConnectorRelationExport(int _ConnectorRelationExportID)
    {
      return Update<ConnectorRelationExport>(c => c.ConnectorRelationExportID == _ConnectorRelationExportID);
    }

  }
}
