using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.Models.EDI.Response;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorRelation : BaseModel<ConnectorRelation>
  {

    public string Email { get; set; }

    public Int32 ConnectorRelationID { get; set; }

    public String CustomerID { get; set; }

    public Boolean? OrderConfirmation { get; set; }

    public Boolean? ShipmentConfirmation { get; set; }

    public Boolean? InvoiceConfirmation { get; set; }

    public String OrderConfirmationEmail { get; set; }

    public String ShipmentConfirmationEmail { get; set; }

    public String InvoiceConfirmationEmail { get; set; }

    public String OutboundTo { get; set; }

    public String OutboundPassword { get; set; }

    public String OutboundUsername { get; set; }

    public string Name { get; set; }

    public string Contact { get; set; }

    public int? ConnectorType { get; set; }

    public Int32? OutboundMessageType { get; set; }

    public String AuthorisationAddresses { get; set; }

    public Int32? AccountPrivileges { get; set; }

    public Boolean? UseFtp { get; set; }

    public Int32? ProviderType { get; set; }

    public Int32? FtpType { get; set; }

    public Int32? FtpFrequency { get; set; }

    public String FtpAddress { get; set; }

    public String FtpPass { get; set; }

    public Int32? FtpPort { get; set; }

    public String FtpUserName { get; set; }

    public Boolean? FtpSSL { get; set; }

    public Int32? XtractType { get; set; }

    public Int32 LanguageID { get; set; }

    public Int32? FtpConnectionType { get; set; }

    public String AdministrationCode { get; set; }

    public String OrderType { get; set; }

    public string Username { get; set; }

    public bool IsActive { get; set; }

    public string Password { get; set; }

    public int? ConnectorID { get; set; }

    public virtual Connector Connector { get; set; }

    public string OutboundOrderConfirmation { get; set; }

    public virtual ICollection<EdiOrder> EdiOrders { get; set; }

    public virtual ICollection<EdiOrderPost> EdiOrderPosts { get; set; }

    public virtual Language Language { get; set; }

    public string OutboundShipmentConfirmation { get; set; }

    public string OutboundInvoiceConfirmation { get; set; }

    public Int32? EdiVendorID { get; set; }

    public string FreightProduct { get; set; }

    public string FinChargesProduct { get; set; }

    public virtual EdiVendor EdiVendor { get; set; }

    public virtual ICollection<EdiOrderResponse> EdiOrderResponses { get; set; }

    public virtual ICollection<ConnectorRelationExport> ConnectorRelationExports { get; set; }

    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }

    public override System.Linq.Expressions.Expression<Func<ConnectorRelation, bool>> GetFilter()
    {
      return null;
    }
  }
}