using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Enumerations
{
  public enum EdiConnectorTypeEnum
  {
    MailConnector = 0,
    HttpConnector = 1,
    FtpConnector = 2
  }

  //TODO: Changed this from -1, 0, 1
  public enum FtpConnectionTypeEnum
  {
    None = 0,
    Active = 1,
    Passive = 2
  }

  // TODO: Changed this from 1, 2
  public enum XractTypeEnum
  {
    WebshopPriceList = 0,
    FullPriceList = 1,
    CustomExport = 2
  }

  //TODO: Changed this from -1, 0, 1, 2
  public enum OutboundMessageTypeEnum
  {
    None = 0,
    SendAsForm = 1,
    SendByMail = 2,
    SendAsXml = 3
  }
  
  public enum AccountPrivilegesEnum
  {
    None = 0,
    XtractS = 1,
    XtractL = 2,
    XtractXL = 3
  }

  public enum ProviderTypeEnum
  {
    BySoap,
    ByXml,
    ByExcel,
    ByCsv
  }

  public enum FtpTypeEnum
  {
    Customer = 0,
    Xtract = 1
  }

  public enum ConnectorTypeEnum
  {
    MailConnector = 0,
    HttpConnector = 1,
    BulkMailConnector = 2
  }

  public enum LanguageTypeEnum
  {
    EN = 1,
    NL = 2
  }
}
