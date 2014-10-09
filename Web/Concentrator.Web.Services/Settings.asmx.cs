using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Concentrator.Objects;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Services.Base;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for Settings
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class RelationSetting
  {
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

    public EdiConnectorTypeEnum? ConnectorType { get; set; }

    public OutboundMessageTypeEnum? OutboundMessageType { get; set; }

    public String AuthorisationAddresses { get; set; }

    public AccountPrivilegesEnum? AccountPrivileges { get; set; }

    public Boolean? UseFtp { get; set; }

    public Int32? ProviderType { get; set; }

    public FtpTypeEnum? FtpType { get; set; }

    public Int32? FtpFrequency { get; set; }

    public String FtpAddress { get; set; }

    public String FtpPass { get; set; }

    public Int32? FtpPort { get; set; }

    public XractTypeEnum? XtractType { get; set; }

    public Int32? LanguageID { get; set; }

    public FtpConnectionTypeEnum? FtpConnectionType { get; set; }

    public String AdministrationCode { get; set; }

    public String OrderType { get; set; }

    public string Username { get; set; }

    public bool IsActive { get; set; }

    public string Password { get; set; }

    public int? ConnectorID { get; set; }

    public string OutboundOrderConfirmation { get; set; }

    public string OutboundShipmentConfirmation { get; set; }

    public string OutboundInvoiceConfirmation { get; set; }

    public ProviderTypeEnum ProviderTypeEnum { get; set; }
  }

  public class Settings : BaseConcentratorService
  {
    [WebMethod(Description = "Get EDI account settings", BufferResponse = false)]
    public RelationSetting GetAccountSettings(string customerid)
    {
      using (var unit = GetUnitOfWork())
      {
        var connectorRelation = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.CustomerID == customerid);

        RelationSetting rSet = new RelationSetting();

        if (connectorRelation == null)
        {
          rSet.CustomerID = customerid;
          rSet.AccountPrivileges = AccountPrivilegesEnum.XtractS;
          rSet.ConnectorType = EdiConnectorTypeEnum.HttpConnector;
          rSet.XtractType = XractTypeEnum.WebshopPriceList;
          rSet.LanguageID = 1;
        }
        else
        {
          rSet.ConnectorRelationID = connectorRelation.ConnectorRelationID;

          rSet.CustomerID = connectorRelation.CustomerID;

          rSet.OrderConfirmation = connectorRelation.OrderConfirmation;

          rSet.ShipmentConfirmation = connectorRelation.ShipmentConfirmation;

          rSet.InvoiceConfirmation = connectorRelation.InvoiceConfirmation;

          rSet.OrderConfirmationEmail = connectorRelation.OrderConfirmationEmail;

          rSet.ShipmentConfirmationEmail = connectorRelation.ShipmentConfirmationEmail;

          rSet.InvoiceConfirmationEmail = connectorRelation.InvoiceConfirmationEmail;

          rSet.OutboundTo = connectorRelation.OutboundTo;

          rSet.OutboundPassword = connectorRelation.OutboundPassword;

          rSet.OutboundUsername = connectorRelation.OutboundUsername;

          rSet.Name = connectorRelation.Name;

          rSet.Contact = connectorRelation.Contact;

          rSet.ConnectorType = connectorRelation.ConnectorType.HasValue ? (EdiConnectorTypeEnum)connectorRelation.ConnectorType : EdiConnectorTypeEnum.HttpConnector;

          rSet.OutboundMessageType = connectorRelation.OutboundMessageType.HasValue ? (OutboundMessageTypeEnum)connectorRelation.OutboundMessageType : OutboundMessageTypeEnum.None;

          rSet.AuthorisationAddresses = connectorRelation.AuthorisationAddresses;

          rSet.AccountPrivileges = connectorRelation.AccountPrivileges.HasValue ? (AccountPrivilegesEnum)connectorRelation.AccountPrivileges : AccountPrivilegesEnum.None;

          rSet.UseFtp = connectorRelation.UseFtp;

          rSet.ProviderType = connectorRelation.ProviderType;

          rSet.FtpType = connectorRelation.FtpType.HasValue ? (FtpTypeEnum)connectorRelation.FtpType : FtpTypeEnum.Xtract;

          rSet.FtpFrequency = connectorRelation.FtpFrequency;

          rSet.FtpAddress = connectorRelation.FtpAddress;

          rSet.FtpPass = connectorRelation.FtpPass;

          rSet.FtpPort = connectorRelation.FtpPort;

          rSet.XtractType = connectorRelation.XtractType.HasValue && connectorRelation.XtractType.Value > 0 ? (XractTypeEnum)connectorRelation.XtractType : XractTypeEnum.WebshopPriceList;

          rSet.LanguageID = connectorRelation.LanguageID;

          rSet.FtpConnectionType = connectorRelation.FtpConnectionType.HasValue ? (FtpConnectionTypeEnum)connectorRelation.FtpConnectionType : FtpConnectionTypeEnum.None;

          rSet.AdministrationCode = connectorRelation.AdministrationCode;

          rSet.OrderType = connectorRelation.OrderType;

          rSet.Username = connectorRelation.Username;

          rSet.IsActive = connectorRelation.IsActive;

          rSet.Password = connectorRelation.Password;

          rSet.ConnectorID = connectorRelation.ConnectorID;

          rSet.OutboundOrderConfirmation = connectorRelation.OutboundOrderConfirmation;

          rSet.OutboundShipmentConfirmation = connectorRelation.OutboundShipmentConfirmation;

          rSet.OutboundInvoiceConfirmation = connectorRelation.OutboundInvoiceConfirmation;
        }

        return rSet;
      }
    }

    [WebMethod(Description = "Save EDI account settings", BufferResponse = false)]
    public bool SaveAccountSettings(XtractConnectorRelation btAccountSettings, int connectorID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var _relationRepo = unit.Scope.Repository<ConnectorRelation>();

          var user = unit.Scope.Repository<User>().GetSingle(x => x.Username == "SYSTEM");
          ConcentratorPrincipal.Login("SYSTEM", user.Password);
          ConnectorRelation exConnectorRelation = _relationRepo.GetSingle(x => x.CustomerID == btAccountSettings.CustomerID);

          if (exConnectorRelation == null)
          {
            exConnectorRelation = new ConnectorRelation();
            exConnectorRelation.ConnectorID = connectorID;
            exConnectorRelation.CustomerID = btAccountSettings.CustomerID;
            _relationRepo.Add(exConnectorRelation);
          }

          exConnectorRelation.AccountPrivileges = btAccountSettings.AccountPrivileges;
          exConnectorRelation.AuthorisationAddresses = btAccountSettings.AuthorisationAddresses;
          exConnectorRelation.ConnectorType = btAccountSettings.ConnectorType;
          exConnectorRelation.FtpAddress = btAccountSettings.FtpAddress;
          exConnectorRelation.FtpConnectionType = btAccountSettings.FtpConnectionType;
          exConnectorRelation.FtpFrequency = btAccountSettings.FtpFrequency;
          exConnectorRelation.FtpPass = btAccountSettings.FtpPass;
          exConnectorRelation.FtpPort = btAccountSettings.FtpPort;
          exConnectorRelation.FtpType = btAccountSettings.FtpType;
          exConnectorRelation.InvoiceConfirmation = btAccountSettings.InvoiceConfirmation;
          exConnectorRelation.OutboundInvoiceConfirmation = btAccountSettings.OutboundInvoiceConfirmation;
          exConnectorRelation.LanguageID = btAccountSettings.LanguageID;
          exConnectorRelation.OrderConfirmation = btAccountSettings.OrderConfirmation;
          exConnectorRelation.OutboundOrderConfirmation = btAccountSettings.OutboundOrderConfirmation;
          exConnectorRelation.OutboundMessageType = btAccountSettings.OutboundMessageType;
          exConnectorRelation.OutboundPassword = btAccountSettings.OutboundPassword;
          exConnectorRelation.OutboundTo = btAccountSettings.OutboundTo;
          exConnectorRelation.OutboundUsername = btAccountSettings.OutboundUsername;
          exConnectorRelation.ProviderType = btAccountSettings.ProviderType;
          exConnectorRelation.ShipmentConfirmation = btAccountSettings.ShipmentConfirmation;
          exConnectorRelation.OutboundShipmentConfirmation = btAccountSettings.OutboundShipmentConfirmation;
          exConnectorRelation.UseFtp = btAccountSettings.UseFtp;
          exConnectorRelation.XtractType = btAccountSettings.XtractType;

          unit.Save();
          return true;
        }
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public class XtractConnectorRelation
    {
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

      public Int32? XtractType { get; set; }

      public Int32 LanguageID { get; set; }

      public Int32? FtpConnectionType { get; set; }

      public String AdministrationCode { get; set; }

      public String OrderType { get; set; }

      public string Username { get; set; }

      public bool IsActive { get; set; }

      public string Password { get; set; }

      public int? ConnectorID { get; set; }

      public string OutboundOrderConfirmation { get; set; }

      public string OutboundShipmentConfirmation { get; set; }

      public string OutboundInvoiceConfirmation { get; set; }

      public string FreightProduct { get; set; }

      public string FinChargesProduct { get; set; }
    }
  }
}
