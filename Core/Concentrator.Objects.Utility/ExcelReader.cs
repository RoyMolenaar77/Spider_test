using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Enumerations;
using Concentrator.Web.Objects.EDI.Purchase;
using System.Xml.Serialization;
using Concentrator.Web.Objects.EDI;
using Concentrator.Objects.Utility.Address;
using Concentrator.Objects.Ordering.EDI;
using Concentrator.Objects.Models.EDI.Enumerations;

namespace Concentrator.Objects.Utility
{
  public class ExcelReader
  {
    static volatile bool _running;
    private log4net.ILog _log;
    private System.Configuration.Configuration _config;
    private IUnitOfWork _unit;
    private ConnectorRelation _connectorRelation;

    public ExcelReader(string watchPath, log4net.ILog log, IUnitOfWork unit, bool activateWatch = false)
    {
      if (activateWatch)
      {
        System.IO.FileSystemWatcher _fileWatcher = new FileSystemWatcher(watchPath);
        _fileWatcher.Filter = "*.*";  //single file doesn't work here...
        _fileWatcher.NotifyFilter = NotifyFilters.Attributes;
        _fileWatcher.Created += new FileSystemEventHandler(OnChangedFile1);
        _fileWatcher.EnableRaisingEvents = true;
      }
      _log = log;
      _unit = unit;

    }

    public void ProcessFile(string path, string mailAdress)
    {
      try
      {
        _log.InfoFormat("Start process file {0}", path);
        FileInfo file = new FileInfo(path);

        Thread.Sleep(500);

        if (path.Contains(".xls") || path.Contains(".xlsx"))
        {
          OleDbConnection con = null;
          System.Data.DataTable exceldt = null;

          try
          {
            if (path.Contains(".xlsx"))
            {
              con = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=YES;\"");
            }
            else
              con = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=Excel 8.0");


            con.Open();

            using (exceldt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
            {
              if (exceldt == null)
              {
                _log.Error("Fout verwerken file: Ongeldig tablad");
                //Logging.UserMail("Fout bij het verwerken van de excel order, de naam van het tabblad dient onjuist. Of u maakt gebruik van een verouderde versie, vraag uw accountmanager naar de nieuwe EDI Excel template", mailLog.Mailaddress, DocumentType.Excel);
              }

              _log.DebugFormat("Found {0} worksheets in excel", exceldt.Rows.Count);
              int i = 0;

              // Add the sheet name to the string array.
              foreach (DataRow row in exceldt.Rows)
              {
                string sheet = row["TABLE_NAME"].ToString();
                OleDbDataAdapter da = null;
                DataTable dt = null;

                switch (sheet)
                {
                  case "EDI_Order_V2$":
                    #region EDI_Order_V2;
                    da = new OleDbDataAdapter("select * from [EDI_Order_V2$] where EdiIdentifier IS NOT NULL AND  ShiptoID IS NOT NULL AND ItemNumber IS NOT NULL AND Quantity IS NOT NULL", con);
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                      ConsolidateOrderByCustomer(dt, mailAdress);
                    else
                    {
                      //EmailDaemon email = new EmailDaemon();
                      //email.SendErrorMail("Order Bevat geen orderregels", mailLog.Mailaddress, DocumentType.Excel);
                    }
                    break;
                    #endregion
                  case "EDI_Product$":
                    #region EDI_Product
                    da = new OleDbDataAdapter("select * from [EDI_Product$] where BSKIdentifier IS NOT NULL", con);
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                      ImportProducts(dt, mailAdress);
                    else
                    {
                      //EmailDaemon email = new EmailDaemon();
                      //email.SendErrorMail("Order Bevat geen orderregels", mailLog.Mailaddress, DocumentType.ProductExcel);
                    }
                    break;
                    #endregion
                  case "EDI_Publication$":
                    #region EDI_Publish
                    da = new OleDbDataAdapter("select * from [EDI_Publication$] where BSKIdentifier IS NOT NULL", con);
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                      ImportPublish(dt, mailAdress);
                    else
                    {
                      //EmailDaemon email = new EmailDaemon();
                      //email.SendErrorMail("Order Bevat geen orderregels", mailLog.Mailaddress, DocumentType.PublicationExcel);
                    }
                    break;
                    #endregion
                  case "EDI_Purchase$":
                    #region EDI_Purchase
                    da = new OleDbDataAdapter("select * from [EDI_Purchase$] where BSKIdentifier IS NOT NULL", con);
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                      ImportPurchaseOrder(dt, mailAdress);
                    else
                    {
                      //EmailDaemon email = new EmailDaemon();
                      //email.SendErrorMail("Purchase Order Bevat geen orderregels", mailLog.Mailaddress, DocumentType.PurchaseExcel);
                    }
                    break;
                    #endregion
                  case "EDI_RMA$":
                    #region EDI_Purchase
                    da = new OleDbDataAdapter("select * from [EDI_RMA$] where BSKIdentifier IS NOT NULL", con);
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                      ImportRMAOrder(dt, mailAdress);
                    else
                    {
                      //EmailDaemon email = new EmailDaemon();
                      //email.SendErrorMail("RMA Order Bevat geen orderregels", mailLog.Mailaddress, DocumentType.PurchaseExcel);
                    }
                    break;
                    #endregion
                    break;
                  case "EDI_CMA$":
                    #region EDI_Purchase
                    da = new OleDbDataAdapter("select * from [EDI_CMA$] where BSKIdentifier IS NOT NULL", con);
                    dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                      ImportCMAOrder(dt, mailAdress);
                    else
                    {
                      //EmailDaemon email = new EmailDaemon();
                      //email.SendErrorMail("CMA Order Bevat geen orderregels", mailLog.Mailaddress, DocumentType.PurchaseExcel);
                    }
                    break;
                    #endregion
                    break;
                  default:
                    _log.DebugFormat("Ignore {0} sheet for processing", sheet);
                    break;
                }
              }
            }
            if (con != null)
            {
              con.Close();
              con.Dispose();
            }

            File.Move(path, Path.Combine(_config.AppSettings.Settings["ExcelProcessedDirectory"].Value, file.Name + "-" + Guid.NewGuid()));
          }
          catch (IOException ex)
          {
            _log.Error("Fout verwerken file: " + ex.Message);
            File.Move(path, Path.Combine(_config.AppSettings.Settings["ExcelErrorDirectory"].Value, file.Name + "-" + Guid.NewGuid()));
          }
          catch (Exception ex)
          {
            _log.Error("Fout verwerken file: " + ex.Message);
            if (File.Exists(path))
              File.Move(path, Path.Combine(_config.AppSettings.Settings["ExcelErrorDirectory"].Value, file.Name + "-" + Guid.NewGuid()));
            //Logging.UserMail("Fout bij het verwerken van de excel order, de naam van het tabblad dient 'EDI_Order_V2' te zijn. Of u maakt gebruik van een verouderde versie, vraag uw accountmanager naar de nieuwe EDI Excel template", mailLog.Mailaddress, DocumentType.Excel);
          }
          finally
          {
            if (con != null)
            {
              con.Close();
              con.Dispose();
            }
          }
        }
        else if (path.Contains(".csv"))
        {
          try
          {
            using (StreamReader reader = new StreamReader(path))
            {
              int lineCount = 0;
              DataTable table = new DataTable();
              table.Columns.Add("ShiptoID");
              table.Columns.Add("ItemNumber");
              table.Columns.Add("Quantity");
              table.Columns.Add("CustomerOrderNumber");
              table.Columns.Add("CustomerItemNumber");
              table.Columns.Add("ZIPcode");
              table.Columns.Add("HouseNumber");
              table.Columns.Add("HouseNumberExtension");
              table.Columns.Add("Street");
              table.Columns.Add("City");
              table.Columns.Add("Country");
              table.Columns.Add("MailingName");
              table.Columns.Add("EdiIdentifier");
              table.Columns.Add("Attn");
              table.Columns.Add("EndCustomerReference");

              while (reader.Peek() >= 0)
              {
                string[] line = reader.ReadLine().Split(',');
                if (lineCount > 0)
                {
                  Regex reg = new Regex(@"\d+\.?\d*");
                  Regex regString = new Regex(@"([^,""\r\n]*)");

                  if (lineCount == 1)
                    _connectorRelation = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.CustomerID == reg.Match(line[0]).Value && x.AuthorisationAddresses.Contains(mailAdress));
                  //cust = DbUtility.GetBSKIdentifierBySupplier(reg.Match(line[0]).Value);

                  DataRow row = table.NewRow();
                  //row["EdiIdentifier"] = cust.BSKIdentifier;
                  //row["ShiptoID"] = cust.ID;
                  row["ItemNumber"] = reg.Match(line[13]).Value;
                  row["Quantity"] = reg.Match(line[16]).Value;
                  row["CustomerOrderNumber"] = Regex.Replace(line[1], @"[^\w\.@-]", "");
                  row["CustomerItemNumber"] = Regex.Replace(line[12], @"[^\w\.@-]", "");
                  row["ZIPcode"] = Regex.Replace(line[9], @"[^\w\.@-]", "");
                  row["HouseNumber"] = Regex.Replace(line[8], @"[^\w\.@-]", "");
                  row["Street"] = Regex.Replace(line[7], @"[^\w\.@-]", "").Trim();
                  row["City"] = Regex.Replace(line[10], @"[^\w\.@-]", "").Trim();
                  row["Country"] = Regex.Replace(line[11], @"[^\w\.@-]", "").Trim();
                  if (!string.IsNullOrEmpty(Regex.Replace(line[4], @"[^\w\.@-]", "")))
                  {
                    row["MailingName"] = Regex.Replace(line[4], @"[^\w\.@-]", "");
                    row["Attn"] = Regex.Replace(line[5], @"[^\w\.@-]", "");
                  }
                  else
                  {
                    row["MailingName"] = Regex.Replace(line[5], @"[^\w\.@-]", "");
                  }
                  row["EndCustomerReference"] = Regex.Replace(line[6], @"[^\w\.@-]", "").Trim();
                  table.Rows.Add(row);
                }
                lineCount++;
              }

              //CustomerInfo cust = DbUtility.GetBSKIdentifierBySupplier(supplier);

              ConsolidateOrderByCustomer(table, mailAdress);

            }

            File.Move(path, Path.Combine(_config.AppSettings.Settings["ExcelProcessedDirectory"].Value, file.Name + "-" + Guid.NewGuid()));
          }
          catch (IOException ex)
          {
            _log.Error("Fout verwerken csv file: " + ex.Message);
            File.Move(path, Path.Combine(_config.AppSettings.Settings["ExcelErrorDirectory"].Value, file.Name + "-" + Guid.NewGuid()));
          }
          catch (Exception ex)
          {
            _log.Error("Fout verwerken file: " + ex.Message);
            if (File.Exists(path))
              File.Move(path, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], file.Name + "-" + Guid.NewGuid()));
            //Logging.UserMail("Fout bij het verwerken van de csv order, vraag uw accountmanager naar de voorwaarden", mailLog.Mailaddress, DocumentType.Excel);
          }
        }
        else if (path.Contains(".xml"))
        {
          try
          {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            _log.Info("XML received by mail, try to insert");
            _connectorRelation = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.AuthorisationAddresses.Contains(mailAdress));
            InsertEdiListener(_unit, _config, _connectorRelation, doc.InnerXml, mailAdress);

            //DbUtility.InsertOrder("XML by Mail", mailLog.Mailaddress, doc.OuterXml, DocumentType.XML.ToString());

            //OrderInfo info = new OrderInfo();
            //info.ReceivedDocument = doc.OuterXml;
            //info.DocumentID = Guid.NewGuid();

            //WorkflowInstance instance = this.Runtime.CreateWorkflow(
            //                typeof(OrderWorkflow), null, info.DocumentID);

            //instance.Start();
            //instance.Unload();
            //instance.EnqueueItemOnIdle("ReceiveOrder", info, null, null);

            File.Move(path, Path.Combine(ConfigurationManager.AppSettings["ExcelProcessedDirectory"], file.Name + "-" + Guid.NewGuid()));
          }
          catch (IOException ex)
          {
            _log.Error("Fout verwerken file: " + ex.Message);
            File.Move(path, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], file.Name + "-" + Guid.NewGuid()));
          }
          catch (Exception ex)
          {
            _log.ErrorFormat("De XML({1}) die u per email ({0}) verstuurd heeft naar BAS EDI kan niet worden verwerkt", mailAdress, EdiDocumentTypes.XML);
            _log.Error("Fout verwerken file: " + ex.Message);
            if (File.Exists(path))
              File.Move(path, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], file.Name + "-" + Guid.NewGuid()));

          }
        }
        else
        {
          _log.ErrorFormat("De file ({1}) die u verstuurd ({0}) heeft naar EDI kan niet worden verwerkt, neem contact op met uw Accountmanager voor meer informatie", mailAdress, EdiDocumentTypes.XML);
          _log.ErrorFormat("EDI file kan niet worden verwerkt, onjuiste mail bijlage: " + path);
        }
      }
      catch (Exception ex)
      {
        _log.Fatal(ex.InnerException);
      }
    }

    private void OnChangedFile1(object source, FileSystemEventArgs e)
    {
      ProcessFile(e.FullPath, "directory");
    }

    private void ImportPurchaseOrder(DataTable dt, string errormail)
    {
      if (dt.Rows.Count > 0)
      {
        PurchaseRequest request = new PurchaseRequest();
        request.Version = "1.0";
        request.bskIdentifier = dt.Rows[0]["EdiIdentifier"].ToString().Trim();
        request.PurchaseLines = new PurchaseLine[dt.Rows.Count];

        int bskIdentifier = 0;
        if (int.TryParse(request.bskIdentifier, out bskIdentifier))
        {
          //if (!DbUtility.BSKCheck(bskIdentifier))
          //{
          //  Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
          //  return;
          //}
        }
        else
        {
          //Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
          return;
        }

        _connectorRelation = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == bskIdentifier);
        DataTable dtCol = new DataTable();

        int rowcount = 0;
        foreach (DataRow row in dt.Rows)
        {
          try
          {
            request.PurchaseLines[rowcount] = new PurchaseLine();
            request.PurchaseLines[rowcount].SupplierNumber = row["SupplierNumber"].ToString();
            request.PurchaseLines[rowcount].ShipToNumber = row["ShipToNumber"].ToString();
            request.PurchaseLines[rowcount].ItemNumber = row["ItemNumber"].ToString();
            request.PurchaseLines[rowcount].Price = row["Price"].ToString();
            request.PurchaseLines[rowcount].Quantity = int.Parse(row["Quantity"].ToString());
            request.PurchaseLines[rowcount].CostRule = row["Costrule"].ToString();
            request.PurchaseLines[rowcount].RequestDate = DateTime.Parse(row["RequestDate"].ToString());
            request.PurchaseLines[rowcount].SupplierSalesOrder = row["SupplierSO"].ToString().Trim();
            request.PurchaseLines[rowcount].Reference = row["Reference"].ToString().Trim();
            request.PurchaseLines[rowcount].Remark = row["PrintRemark"].ToString().Trim();
          }

          catch (Exception ex)
          {
            dtCol.Rows.Add(row);
          }
          rowcount++;
        }

        if (dtCol != null && dtCol.Rows.Count > 0)
        {
          //Logging.UserMail(dtCol.Select("Purchase error"), errormail);
        }

        if (request.PurchaseLines.Count() > 0)
        {
          StringBuilder replyString = new StringBuilder();

          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          XmlWriter xw = XmlWriter.Create(replyString, settings);
          xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

          XmlSerializer rxs = new XmlSerializer(typeof(PurchaseRequest));
          XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
          ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
          rxs.Serialize(xw, request, ns);

          XmlDocument replyXml = new XmlDocument();
          replyXml.LoadXml(replyString.ToString());

          int EdiRequestID = InsertEdiListener(_unit, _config, _connectorRelation, replyXml.OuterXml, errormail);

          //int EdiRequestID =  DbUtility.InsertOrder("PurchaseImport", errormail, replyXml.OuterXml, DocumentType.PurchaseExcel.ToString());

          _log.InfoFormat("Purchase Order succesvol naar listener voor {0} ({1}) process to JDE next import run", _connectorRelation.Name, _connectorRelation.ConnectorRelationID);

          LogFile("Excel purchase Request", "PurchaseImport", Guid.NewGuid(), replyXml.OuterXml);

          if (_connectorRelation.OrderConfirmation.HasValue && _connectorRelation.OrderConfirmation.Value)
          {
            //EmailDaemon email = new EmailDaemon();
            //email.OrderReceivedSucceed(cus);
          }
        }
      }
    }

    private void ImportRMAOrder(DataTable dt, string errormail)
    {
      if (dt.Rows.Count > 0)
      {
        var groupedOrders = (from o in dt.AsEnumerable()
                             group o by
                             new
                             {
                               ShipToID = o["ShipToID"].ToString(),
                               Reference = o["CustomerOrderNumber"].ToString(),
                               ZipCode = o.Field<string>("ZIPCode"),
                               HouseNumber = o["HouseNumber"].ToString(),
                               HouseNumberExt = o.Field<string>("HouseNumberExtension"),
                               MailingName = o.Field<string>("MailingName"),
                               Attn = o.Field<string>("Attn"),
                               Street = o.Field<string>("Street"),
                               City = o.Field<string>("City"),
                               Country = o.Field<string>("Country")
                             } into ord
                             select ord).ToDictionary(x => x.Key, y => y.ToList());

        foreach (var order in groupedOrders)
        {
          OrderRequest request = new OrderRequest();
          request.Version = "1.0";
          request.OrderHeader = new OrderRequestHeader();
          request.OrderHeader.ShipToCustomer = new Customer();
          request.OrderHeader.ShipToCustomer.EanIdentifier = order.Key.ShipToID.ToString();
          if (!string.IsNullOrEmpty(order.Key.Reference.Trim()))
            request.OrderHeader.CustomerOrderReference = order.Key.Reference.Trim().Cap(25);
          else
            request.OrderHeader.CustomerOrderReference = "BASEDI_RMA-" + System.DateTime.Now.ToString("ddMMyyyy HH:mm:ss");

          int bskIdentifier = 0;
          if (int.TryParse(order.Value[0]["EdiIdentifier"].ToString(), out bskIdentifier))
          {
            request.OrderHeader.BSKIdentifier = bskIdentifier;
            //if (!DbUtility.BSKCheck(bskIdentifier))
            //{
            //  Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
            //  return;
            //}
          }
          else
          {
            //Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
            return;
          }
          _connectorRelation = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == bskIdentifier);

          request.OrderHeader.BSKIdentifier = bskIdentifier;
          request.OrderHeader.RequestedDate = DateTime.Now;
          request.OrderHeader.EdiVersion = "2.0";
          request.OrderDetails = new OrderRequestDetail[order.Value.Count()];

          if (order.Value[0].Table.Columns.Contains("EndCustomerReference"))
          {
            if (order.Value[0]["EndCustomerReference"] != null && !string.IsNullOrEmpty(order.Value[0]["EndCustomerReference"].ToString()))
              request.OrderHeader.EndCustomerOrderReference = order.Value[0]["EndCustomerReference"].ToString();
          }

          if (!string.IsNullOrEmpty(order.Key.ZipCode)
            && !string.IsNullOrEmpty(order.Key.HouseNumber))
          {
            string housenumber = order.Key.HouseNumber;
            if (!string.IsNullOrEmpty(order.Key.HouseNumberExt))
              housenumber += order.Key.HouseNumberExt;

            request.OrderHeader.CustomerOverride = new CustomerOverride();
            request.OrderHeader.CustomerOverride.Dropshipment = true;
            request.OrderHeader.CustomerOverride.OrderAddress = new Concentrator.Web.Objects.EDI.Address();
            request.OrderHeader.CustomerOverride.OrderAddress.Name = order.Key.MailingName.Cap(40);

            request.OrderHeader.CustomerOverride.OrderAddress.AddressLine1 = order.Key.Street + " " + housenumber;
            request.OrderHeader.CustomerOverride.OrderAddress.City = order.Key.City;
            request.OrderHeader.CustomerOverride.OrderAddress.Country = order.Key.Country;
            request.OrderHeader.CustomerOverride.OrderAddress.ZipCode = order.Key.ZipCode;

            if (!string.IsNullOrEmpty(order.Key.Attn))
            {
              request.OrderHeader.CustomerOverride.OrderAddress.AddressLine3 = order.Key.Attn;
            }

            if (dt.Columns.Contains("Email") && !string.IsNullOrEmpty(order.Value[0].Field<string>("Email")))
            {
              request.OrderHeader.CustomerOverride.CustomerContact = new Contact();
              request.OrderHeader.CustomerOverride.CustomerContact.Email = order.Value[0].Field<string>("Email");
            }
          }
          else
          {
            request.OrderHeader.CustomerOverride = new CustomerOverride();
            request.OrderHeader.CustomerOverride.Dropshipment = false;
            request.OrderHeader.CustomerOverride.OrderAddress = new Concentrator.Web.Objects.EDI.Address();
          }

          request.OrderDetails = new OrderRequestDetail[order.Value.Count];
          DataTable dtCol = new DataTable();

          int rowcount = 0;
          foreach (DataRow row in order.Value)
          {
            try
            {
              request.OrderDetails[rowcount] = new OrderRequestDetail();
              request.OrderDetails[rowcount].ProductIdentifier = new ProductIdentifier();
              request.OrderDetails[rowcount].ProductIdentifier.ProductNumber = row["ItemNumber"].ToString();
              request.OrderDetails[rowcount].Quantity = int.Parse(row["QuantityShipped"].ToString());
              request.OrderDetails[rowcount].WareHouseCode = row["ReturnType"].ToString();
            }

            catch (Exception ex)
            {
              dtCol.Rows.Add(row);
            }
            rowcount++;
          }

          if (dtCol != null && dtCol.Rows.Count > 0)
          {
            //Logging.UserMail(dtCol.Select("RMA order error"), errormail);
          }

          StringBuilder replyString = new StringBuilder();

          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          XmlWriter xw = XmlWriter.Create(replyString, settings);
          xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

          XmlSerializer rxs = new XmlSerializer(typeof(OrderRequest));
          XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
          ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
          rxs.Serialize(xw, request, ns);

          XmlDocument replyXml = new XmlDocument();
          replyXml.LoadXml(replyString.ToString());

          int EdiRequestID = InsertEdiListener(_unit, _config, _connectorRelation, replyXml.OuterXml, errormail);

          //int EdiRequestID = DbUtility.InsertOrder("RMA order Import", errormail, replyXml.OuterXml, DocumentType.RMAOrderExcel.ToString());

          _log.InfoFormat("RMA order Order succesvol naar listener voor {0} ({1}) process to JDE next import run", _connectorRelation.Name, _connectorRelation.ConnectorRelationID);

          LogFile("Excel RMA order Request", "RMAOrderImport", Guid.NewGuid(), replyXml.OuterXml);

          if (_connectorRelation.OrderConfirmation.HasValue && _connectorRelation.OrderConfirmation.Value)
          {
            // EmailDaemon email = new EmailDaemon();
            //email.OrderReceivedSucceed(cus);
          }

        }
      }
    }

    private void ImportCMAOrder(DataTable dt, string errormail)
    {
      if (dt.Rows.Count > 0)
      {
        var groupedOrders = (from o in dt.AsEnumerable()
                             group o by
                             new
                             {
                               ShipToID = o["ShipToID"].ToString(),
                               Reference = o["CustomerOrderNumber"].ToString(),
                               ZipCode = o.Field<string>("ZIPCode"),
                               HouseNumber = o["HouseNumber"].ToString(),
                               HouseNumberExt = o.Field<string>("HouseNumberExtension"),
                               MailingName = o.Field<string>("MailingName"),
                               Attn = o.Field<string>("Attn"),
                               Street = o.Field<string>("Street"),
                               City = o.Field<string>("City"),
                               Country = o.Field<string>("Country")
                             } into ord
                             select ord).ToDictionary(x => x.Key, y => y.ToList());

        foreach (var order in groupedOrders)
        {
          OrderRequest request = new OrderRequest();
          request.Version = "1.0";
          request.OrderHeader = new OrderRequestHeader();
          request.OrderHeader.ShipToCustomer = new Customer();
          request.OrderHeader.ShipToCustomer.EanIdentifier = order.Key.ShipToID;
          if (!string.IsNullOrEmpty(order.Key.Reference.Trim()))
            request.OrderHeader.CustomerOrderReference = order.Key.Reference.Trim().Cap(25);
          else
            request.OrderHeader.CustomerOrderReference = "BASEDI_CMA-" + System.DateTime.Now.ToString("ddMMyyyy HH:mm:ss");

          int bskIdentifier = 0;
          if (int.TryParse(order.Value[0]["EdiIdentifier"].ToString(), out bskIdentifier))
          {
            request.OrderHeader.BSKIdentifier = bskIdentifier;
            //if (!DbUtility.BSKCheck(bskIdentifier))
            //{
            //  Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
            //  return;
            //}
          }
          else
          {
            //Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
            return;
          }
          _connectorRelation = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == bskIdentifier);

          request.OrderHeader.BSKIdentifier = bskIdentifier;
          request.OrderHeader.RequestedDate = DateTime.Now;
          request.OrderHeader.EdiVersion = "2.0";
          request.OrderDetails = new OrderRequestDetail[order.Value.Count()];

          if (order.Value[0].Table.Columns.Contains("EndCustomerReference"))
          {
            if (order.Value[0]["EndCustomerReference"] != null && !string.IsNullOrEmpty(order.Value[0]["EndCustomerReference"].ToString()))
              request.OrderHeader.EndCustomerOrderReference = order.Value[0]["EndCustomerReference"].ToString();
          }

          if (!string.IsNullOrEmpty(order.Key.ZipCode)
            && !string.IsNullOrEmpty(order.Key.HouseNumber))
          {
            string housenumber = order.Key.HouseNumber;
            if (!string.IsNullOrEmpty(order.Key.HouseNumberExt))
              housenumber += order.Key.HouseNumberExt;

            request.OrderHeader.CustomerOverride = new CustomerOverride();
            request.OrderHeader.CustomerOverride.Dropshipment = true;
            request.OrderHeader.CustomerOverride.OrderAddress = new Concentrator.Web.Objects.EDI.Address();
            request.OrderHeader.CustomerOverride.OrderAddress.Name = order.Key.MailingName.Cap(40);

            request.OrderHeader.CustomerOverride.OrderAddress.AddressLine1 = order.Key.Street + " " + housenumber;
            request.OrderHeader.CustomerOverride.OrderAddress.City = order.Key.City;
            request.OrderHeader.CustomerOverride.OrderAddress.Country = order.Key.Country;
            request.OrderHeader.CustomerOverride.OrderAddress.ZipCode = order.Key.ZipCode;

            if (!string.IsNullOrEmpty(order.Key.Attn))
            {
              request.OrderHeader.CustomerOverride.OrderAddress.AddressLine3 = order.Key.Attn;
            }

            if (dt.Columns.Contains("Email") && !string.IsNullOrEmpty(order.Value[0].Field<string>("Email")))
            {
              request.OrderHeader.CustomerOverride.CustomerContact = new Contact();
              request.OrderHeader.CustomerOverride.CustomerContact.Email = order.Value[0].Field<string>("Email");
            }
          }
          else
          {
            request.OrderHeader.CustomerOverride = new CustomerOverride();
            request.OrderHeader.CustomerOverride.Dropshipment = false;
            request.OrderHeader.CustomerOverride.OrderAddress = new Concentrator.Web.Objects.EDI.Address();
          }

          request.OrderDetails = new OrderRequestDetail[order.Value.Count];
          DataTable dtCol = new DataTable();

          int rowcount = 0;
          foreach (DataRow row in order.Value)
          {
            int quantity = int.Parse(row["Quantity"].ToString());

            try
            {
              request.OrderDetails[rowcount] = new OrderRequestDetail();
              request.OrderDetails[rowcount].ProductIdentifier = new ProductIdentifier();
              request.OrderDetails[rowcount].ProductIdentifier.ProductNumber = row["ItemNumber"].ToString();
              request.OrderDetails[rowcount].VendorItemNumber = row["CustomerItemNumber"].ToString();
              request.OrderDetails[rowcount].Quantity = (quantity - (2 * quantity));
              request.OrderDetails[rowcount].WareHouseCode = row["ReturnType"].ToString();
            }

            catch (Exception ex)
            {
              dtCol.Rows.Add(row);
            }
            rowcount++;
          }

          if (dtCol != null && dtCol.Rows.Count > 0)
          {
            //Logging.UserMail(dtCol.Select("CMA order error"), errormail);
          }

          StringBuilder replyString = new StringBuilder();

          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          XmlWriter xw = XmlWriter.Create(replyString, settings);
          xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

          XmlSerializer rxs = new XmlSerializer(typeof(OrderRequest));
          XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
          ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
          rxs.Serialize(xw, request, ns);

          XmlDocument replyXml = new XmlDocument();
          replyXml.LoadXml(replyString.ToString());


          int EdiRequestID = InsertEdiListener(_unit, _config, _connectorRelation, replyXml.OuterXml, errormail);
          //int EdiRequestID = DbUtility.InsertOrder("CMA order Import", errormail, replyXml.OuterXml, DocumentType.CMAOrderExcel.ToString());

          _log.InfoFormat("CMA order Order succesvol naar listener voor {0} ({1}) process to JDE next import run", _connectorRelation.Name, _connectorRelation.ConnectorRelationID);

          LogFile("Excel CMA order Request", "RMAOrderImport", Guid.NewGuid(), replyXml.OuterXml);

          if (_connectorRelation.OrderConfirmation.HasValue && _connectorRelation.OrderConfirmation.Value)
          {
            // EmailDaemon email = new EmailDaemon();
            //email.OrderReceivedSucceed(cus);
          }

        }
      }
    }

    private void ImportPublish(DataTable dt, string errormail)
    {
      //if (dt.Rows.Count > 0)
      //{
      //  PublicationRequest request = new PublicationRequest();
      //  request.Version = "1.0";
      //  request.bskIdentifier = dt.Rows[0]["EdiIdentifier"].ToString().Trim();
      //  request.PublicationLines = new PublicationLine[dt.Rows.Count];

      //  int bskIdentifier = 0;
      //  if (int.TryParse(request.bskIdentifier, out bskIdentifier))
      //  {
      //    if (!DbUtility.BSKCheck(bskIdentifier))
      //    {
      //      Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
      //      return;
      //    }
      //  }
      //  else
      //  {
      //    Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.PublicationExcel);
      //    return;
      //  }

      //  CustomerInfo cus = DbUtility.BSKFill(bskIdentifier);
      //  DataTable dtCol = new DataTable();

      //  int rowcount = 0;
      //  foreach (DataRow row in dt.Rows)
      //  {
      //    try
      //    {
      //      request.PublicationLines[rowcount] = new PublicationLine();
      //      request.PublicationLines[rowcount].PublicationNumber = int.Parse(row["publicationnr"].ToString());
      //      request.PublicationLines[rowcount].PublicationType = row["publicationtype"].ToString().Trim();
      //      request.PublicationLines[rowcount].PublicationCompany = row["publicationcompany"].ToString().Trim();
      //      request.PublicationLines[rowcount].PublicationBusinessUnit = row["publicationBU"].ToString().Trim();
      //      request.PublicationLines[rowcount].ItemNumber = row["itemnr"].ToString().Trim();
      //      request.PublicationLines[rowcount].Status = row["status"].ToString().Trim();
      //      request.PublicationLines[rowcount].Price = row["price"].ToString().Trim();
      //      request.PublicationLines[rowcount].FromLevel = row["Fromlevel"].ToString().Trim();
      //      request.PublicationLines[rowcount].Currency = row["Currency"].ToString().Trim();
      //      request.PublicationLines[rowcount].UnitOfMeasure = row["Unitofmeasure"].ToString().Trim();
      //      request.PublicationLines[rowcount].BaseCode = row["basecode"].ToString().Trim();
      //    }

      //    catch (Exception ex)
      //    {
      //      dtCol.Rows.Add(row);
      //    }
      //    rowcount++;
      //  }

      //  if (dtCol != null && dtCol.Rows.Count > 0)
      //  {
      //    Logging.UserMail(dtCol.Select("Publicationnr is not null"), errormail);
      //  }

      //  if (request.PublicationLines.Count() > 0)
      //  {
      //    StringBuilder replyString = new StringBuilder();

      //    XmlWriterSettings settings = new XmlWriterSettings();
      //    settings.Encoding = Encoding.UTF8;
      //    XmlWriter xw = XmlWriter.Create(replyString, settings);
      //    xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

      //    XmlSerializer rxs = new XmlSerializer(typeof(PublicationRequest));
      //    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      //    ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
      //    rxs.Serialize(xw, request, ns);

      //    XmlDocument replyXml = new XmlDocument();
      //    replyXml.LoadXml(replyString.ToString());

      //    int EdiRequestID = DbUtility.InsertOrder("PublicationImport", errormail, replyXml.OuterXml, DocumentType.PublicationExcel.ToString());

      //    Logging.WriteLogMessage(string.Format("Order succesvol naar listener voor {0} ({1}) process to JDE next import run", cus.Name, cus.ID), logCategory);

      //    FileLogger.LogFile("Excel publication Request", "PublicationImport", Guid.NewGuid(), replyXml.OuterXml);

      //    if (cus.OrderConfirmation)
      //    {
      //      EmailDaemon email = new EmailDaemon();
      //      email.PublicationReceivedSucceed(cus);
      //    }
      //  }
      //}
    }

    private void ImportProducts(DataTable dt, string errormail)
    {


      //if (dt.Rows.Count > 0)
      //{
      //  ProductRequest request = new ProductRequest();
      //  request.Version = "1.0";
      //  request.bskIdentifier = dt.Rows[0]["EdiIdentifier"].ToString().Trim();

      //  request.Products = new BAS.EDI.Contracts.DocumentTypes.Product.Product[80];

      //  int bskIdentifier = 0;
      //  if (int.TryParse(request.bskIdentifier, out bskIdentifier))
      //  {
      //    if (!DbUtility.BSKCheck(bskIdentifier))
      //    {
      //      Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.ProductExcel);
      //      return;
      //    }
      //  }
      //  else
      //  {
      //    Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.ProductExcel);
      //    return;
      //  }

      //  CustomerInfo cus = DbUtility.BSKFill(bskIdentifier);

      //  int rowcount = 0;
      //  int splitcount = 0;
      //  foreach (DataRow row in dt.Rows)
      //  {
      //    request.Products[rowcount] = new BAS.EDI.Contracts.DocumentTypes.Product.Product();
      //    request.Products[rowcount].VendorItemNumber = row["VendorItemNumber"].ToString().Trim();
      //    request.Products[rowcount].UnitPrice = new BAS.EDI.Contracts.DocumentTypes.Product.ProductUnitPrice();
      //    request.Products[rowcount].UnitPrice.Text = new string[] { row["UnitPrice"].ToString().Trim() };
      //    if (row["TaxRate"].ToString().Trim() == "HIGH")
      //    {
      //      request.Products[rowcount].UnitPrice.HighTaxRate = true;
      //      request.Products[rowcount].UnitPrice.LowTaxRate = false;
      //    }
      //    else if (row["TaxRate"].ToString().Trim() == "LOW")
      //    {
      //      request.Products[rowcount].UnitPrice.HighTaxRate = false;
      //      request.Products[rowcount].UnitPrice.LowTaxRate = true;
      //    }
      //    else
      //    {
      //      request.Products[rowcount].UnitPrice.HighTaxRate = false;
      //      request.Products[rowcount].UnitPrice.LowTaxRate = false;
      //    }
      //    request.Products[rowcount].UnitCost = decimal.Parse(row["CostPrice"].ToString().Trim());
      //    request.Products[rowcount].ProductGroupCode = row["ProductGroupCode"].ToString().Trim();
      //    request.Products[rowcount].ParentProductGroupCode = row["ParentProductGroupCode"].ToString().Trim();
      //    request.Products[rowcount].ModelNumber = row["ModelNumber"].ToString().Trim();
      //    request.Products[rowcount].Description2 = row["Description2"].ToString().Trim();
      //    request.Products[rowcount].Description = row["Description"].ToString().Trim();
      //    request.Products[rowcount].BrandCode = row["BrandCode"].ToString().Trim();
      //    request.Products[rowcount].UPC = row["UPC"].ToString().Trim();
      //    request.Products[rowcount].EAN = row["EAN"].ToString().Trim();
      //    request.Products[rowcount].LandedCostRule = row["LandedCostRule"].ToString().Trim();
      //    request.Products[rowcount].BuyerNumber = row["BuyerNumber"].ToString().Trim();
      //    request.Products[rowcount].PlannerNumber = row["PlannerNumber"].ToString().Trim();
      //    if (row.Table.Columns.Contains("TemplateItemNumber"))
      //    {
      //      request.Products[rowcount].TemplateItem = int.Parse(row["TemplateItemNumber"].ToString());
      //    }
      //    rowcount++;
      //    splitcount++;

      //    if (splitcount == 80)
      //    {
      //      StringBuilder replySplitString = new StringBuilder();

      //      XmlWriterSettings splitSettings = new XmlWriterSettings();
      //      splitSettings.Encoding = Encoding.UTF8;
      //      XmlWriter splitXw = XmlWriter.Create(replySplitString, splitSettings);
      //      splitXw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

      //      XmlSerializer splitrxs = new XmlSerializer(typeof(ProductRequest));
      //      XmlSerializerNamespaces splitns = new XmlSerializerNamespaces();
      //      splitns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
      //      splitrxs.Serialize(splitXw, request, splitns);

      //      XmlDocument splitReplyXml = new XmlDocument();
      //      splitReplyXml.LoadXml(replySplitString.ToString());

      //      DbUtility.InsertOrder("ProductImport", errormail, splitReplyXml.OuterXml, DocumentType.ProductExcel.ToString());

      //      rowcount = 0;
      //      splitcount = 0;
      //      request = new ProductRequest();
      //      request.Version = "1.0";
      //      request.bskIdentifier = dt.Rows[0]["EdiIdentifier"].ToString().Trim();

      //      request.Products = new BAS.EDI.Contracts.DocumentTypes.Product.Product[80];

      //      bskIdentifier = 0;
      //      if (int.TryParse(request.bskIdentifier, out bskIdentifier))
      //      {
      //        if (!DbUtility.BSKCheck(bskIdentifier))
      //        {
      //          Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.ProductExcel);
      //          return;
      //        }
      //      }
      //      else
      //      {
      //        Logging.UserMail("Ongeldige BSKIdentifier", errormail, DocumentType.ProductExcel);
      //        return;
      //      }
      //    }
      //  }

      //  StringBuilder replyString = new StringBuilder();

      //  XmlWriterSettings settings = new XmlWriterSettings();
      //  settings.Encoding = Encoding.UTF8;
      //  XmlWriter xw = XmlWriter.Create(replyString, settings);
      //  xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

      //  XmlSerializer rxs = new XmlSerializer(typeof(ProductRequest));
      //  XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      //  ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
      //  rxs.Serialize(xw, request, ns);

      //  XmlDocument replyXml = new XmlDocument();
      //  replyXml.LoadXml(replyString.ToString());

      //  int EdiRequestID = DbUtility.InsertOrder("ProductImport", errormail, replyXml.OuterXml, DocumentType.ProductExcel.ToString());

      //  Logging.WriteLogMessage(string.Format("Order succesvol naar listener voor {0} ({1}) process to JDE next import run", cus.Name, cus.ID), logCategory);

      //  FileLogger.LogFile("Excel Product Request", "ProductImport", Guid.NewGuid(), replyXml.OuterXml);

      //  if (cus.OrderConfirmation)
      //  {
      //    EmailDaemon email = new EmailDaemon();
      //    email.ProductReceivedSucceed(cus);
      //  }
      //}
    }

    private void ConsolidateOrderByCustomer(DataTable order, string errormail)
    {
      try
      {
        List<DataRow> dataRows = ValidateExcel(order, errormail);

        if (dataRows.Count() > 0)
        {
          _log.Error("Fout in Excel, order zal niet volledig verwerkt worden!");
          foreach (DataRow row in dataRows)
          {
            order.Rows.Remove(row);
          }
        }
      }
      catch (Exception ex)
      {
        _log.Fatal(ex);
        throw;
      }

      try
      {
        Dictionary<int, DataRow[]> orderList = new Dictionary<int, DataRow[]>();
        List<int> shiptoList = new List<int>();
        Dictionary<string, int> shiptoListWhitoutAddress = new Dictionary<string, int>();
        Dictionary<int, List<ShipToAddress>> shiptoAddress = new Dictionary<int, List<ShipToAddress>>();

        foreach (DataRow row in order.Rows)
        {
          int shiptoID = 0;
          int bskIdentifier = 0;
          if (int.TryParse(row["ShiptoID"].ToString().Trim(), out shiptoID) && int.TryParse(row["EdiIdentifier"].ToString().Trim(), out bskIdentifier))
          {
            string shipToCustomer = shiptoID.ToString();
            _connectorRelation = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == bskIdentifier);

            if (_connectorRelation == null)
            {
              _log.ErrorFormat("Ongeldige EdiIdentifier for shiptoID " + shiptoID + " deze regel zal niet worden verwerkt.", errormail);
              continue;
            }
            else if ((_connectorRelation.ConnectorType.HasValue &&
              _connectorRelation.ConnectorType.Value == 2) || _connectorRelation.CustomerID == shipToCustomer)
            {
              if (!shiptoList.Contains(bskIdentifier))
                shiptoList.Add(bskIdentifier);
            }
          }
          else
          {
            _log.ErrorFormat("Ongeldige EdiIdentifier for shiptoID " + shiptoID + " deze regel zal niet worden verwerkt.", errormail);
            continue;
          }

          if (!string.IsNullOrEmpty(row["ZIPcode"].ToString()) && !string.IsNullOrEmpty(row["HouseNumber"].ToString())
            && !string.IsNullOrEmpty(row["Street"].ToString()) && !string.IsNullOrEmpty(row["City"].ToString())
            && !string.IsNullOrEmpty(row["MailingName"].ToString()) && !string.IsNullOrEmpty(row["Country"].ToString()))
          {
            ShipToAddress address = new ShipToAddress();
            address.ZIPcode = row["ZIPcode"].ToString();
            address.Housenumber = row["HouseNumber"].ToString();

            if (row.Table.Columns.Contains("HouseNumberExtension"))
            {
              if (row["HouseNumberExtension"] != null && !string.IsNullOrEmpty(row["HouseNumberExtension"].ToString()))
                address.HousenumberExtenstion = row["HouseNumberExtension"].ToString();
            }

            if (row.Table.Columns.Contains("Email"))
            {
              if (row["Email"] != null && !string.IsNullOrEmpty(row["Email"].ToString()))
                address.Email = row["Email"].ToString();
            }

            address.Street = row["Street"].ToString();
            address.City = row["City"].ToString();
            address.MailingName = row["MailingName"].ToString();
            #region Country
            if (row["Country"] != null && !string.IsNullOrEmpty(row["Country"].ToString()))
            {
              switch (row["Country"].ToString().Trim())
              {
                case "Nederland":
                case "Nederlands":
                case "NL":
                  address.Country = "NL";
                  break;
                case "BE":
                case "Belgie":
                case "Belgium":
                  address.Country = "BE";
                  break;
                case "UK":
                case "United Kingdom":
                  address.Country = "UK";
                  break;
                default:
                  address.Country = "NL";
                  break;
              }
            }
            else
              address.Country = "NL";
            #endregion

            if (shiptoAddress.ContainsKey(bskIdentifier))
            {
              bool already = false;
              foreach (ShipToAddress add in shiptoAddress[bskIdentifier])
              {
                if (add.ZIPcode == address.ZIPcode && add.Housenumber == address.Housenumber && add.HousenumberExtenstion == add.HousenumberExtenstion)
                  already = true;
              }
              if (!already)
                shiptoAddress[bskIdentifier].Add(address);
            }
            else
            {
              List<ShipToAddress> list = new List<ShipToAddress>();
              list.Add(address);
              shiptoAddress.Add(bskIdentifier, list);
            }
          }
          else
          {
            if (shiptoListWhitoutAddress.ContainsKey(row["ShiptoID"].ToString().Trim()))
            {
              shiptoListWhitoutAddress[row["ShiptoID"].ToString().Trim()] = bskIdentifier;
            }
            else
              shiptoListWhitoutAddress.Add(row["ShiptoID"].ToString().Trim(), bskIdentifier);
          }
        }

        _log.Info("Lijst met shiptonummer opgehaald aantal te verwerken: " + shiptoList.Count.ToString());

        foreach (int bskidentifier in shiptoList)
        {
          //int soldToID = CustomerInfo.GetSoldToCustomerID(shiptoID);

          ConnectorRelation soldToCustomer = _unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == bskidentifier);

          //if (cust != null)
          //  soldToCustomer = cust;
          //else
          //  soldToCustomer = DbUtility.BSKFill(bskidentifier);

          if (soldToCustomer.ProviderType.HasValue)
          {
            //if (soldToCustomer.Code != "HS")
            //{
            DataRow[] rows;
            #region ShipToAddress
            if (shiptoAddress.ContainsKey(bskidentifier))
            {
              foreach (ShipToAddress address in shiptoAddress[bskidentifier])
              {
                try
                {
                  if (!string.IsNullOrEmpty(address.HousenumberExtenstion))
                    rows = order.Select("ShiptoID = '" + soldToCustomer.CustomerID + "' AND ZIPcode = '" + address.ZIPcode + "' AND HouseNumber = '" + address.Housenumber + "' AND HouseNumberExtension = '" + address.HousenumberExtenstion + "'");
                  else
                    rows = order.Select("ShiptoID = '" + soldToCustomer.CustomerID + "' AND ZIPcode = '" + address.ZIPcode + "' AND HouseNumber = '" + address.Housenumber + "'");

                  //SPLIT order on orderreference 
                  List<string> customerorderreferencesShipto = new List<string>();
                  foreach (DataRow row in rows)
                  {
                    try
                    {
                      if (!string.IsNullOrEmpty(row["CustomerOrderNumber"].ToString().Trim()))
                      {
                        if (!customerorderreferencesShipto.Contains(row["CustomerOrderNumber"].ToString().Trim()))
                          customerorderreferencesShipto.Add(row["CustomerOrderNumber"].ToString().Trim());
                      }
                    }
                    catch (Exception ex)
                    {
                      _log.DebugFormat("Door een fout zal de Exel order voor {0} niet verder verwerkt worden met ORDERREFERENCE {1}", soldToCustomer.CustomerID, row["CustomerOrderNumber"].ToString());
                      _log.Debug(ex);
                    }
                  }

                  foreach (string reference in customerorderreferencesShipto)
                  {
                    try
                    {
                      DataRow[] customerRowsWithRef = order.Select("ShiptoID = '" + soldToCustomer.CustomerID + "' AND CustomerOrderNumber = '" + reference + "' AND ZIPcode = '" + address.ZIPcode + "' AND HouseNumber = '" + address.Housenumber + "'");
                      _log.InfoFormat("{0} rijen te verwerken voor klant: {1} met CustomerOrdernumber: {2}", customerRowsWithRef.Count(), soldToCustomer.CustomerID, reference);
                      GenerateExcelDocument(soldToCustomer, soldToCustomer.CustomerID, customerRowsWithRef, errormail, true, bskidentifier);
                    }
                    catch (Exception ex)
                    {
                      _log.ErrorFormat("Door een fout zal de Exel order voor {0} niet verder verwerkt worden", soldToCustomer.CustomerID);
                      _log.Debug(ex);
                    }
                  }

                  //GenerateExcelDocument(soldToCustomer, soldToCustomer.ID, rows, errormail, true, bskidentifier);
                }
                catch (Exception ex)
                {
                  _log.ErrorFormat("Door een fout zal de Exel order voor {0} niet verder verwerkt worden met ZIPCODE {1} en huisnummer {2}", soldToCustomer.CustomerID, address.ZIPcode, address.Housenumber);
                  _log.Debug(ex);
                }
                //}
              }
            }
            #endregion

            if (shiptoListWhitoutAddress.ContainsValue(bskidentifier))
            {
              foreach (string shiptoID in shiptoListWhitoutAddress.Keys)
              {
                //SPLIT order on orderreference 
                DataRow[] customerRows = order.Select("ShiptoID = '" + shiptoID + "' AND ZIPcode IS NULL AND HouseNumber IS NULL");
                List<string> customerorderreferences = new List<string>();
                foreach (DataRow row in customerRows)
                {
                  try
                  {
                    if (!string.IsNullOrEmpty(row["CustomerOrderNumber"].ToString().Trim()))
                    {
                      if (!customerorderreferences.Contains(row["CustomerOrderNumber"].ToString().Trim()))
                        customerorderreferences.Add(row["CustomerOrderNumber"].ToString().Trim());
                    }
                  }
                  catch (Exception ex)
                  {
                    _log.ErrorFormat("Door een fout zal de Exel order voor {0} niet verder verwerkt worden met ORDERREFERENCE {1}", soldToCustomer.CustomerID, row["CustomerOrderNumber"].ToString());
                    _log.Debug(ex);
                  }
                }

                //Processorders whitout customer orderreference
                DataRow[] customerRowsWithoutRef = order.Select("ShiptoID = '" + shiptoID + "' AND CustomerOrderNumber IS NULL AND ZIPcode IS NULL AND HouseNumber IS NULL");
                _log.InfoFormat("{0} rijen te verwerken voor klant: {1} zonder CustomerOrdernumber", customerRowsWithoutRef.Count(), soldToCustomer.CustomerID);
                GenerateExcelDocument(soldToCustomer, shiptoID, customerRowsWithoutRef, errormail, false, bskidentifier);

                foreach (string reference in customerorderreferences)
                {
                  try
                  {
                    DataRow[] customerRowsWithRef = order.Select("ShiptoID = '" + shiptoID + "' AND CustomerOrderNumber = '" + reference + "' AND ZIPcode IS NULL AND HouseNumber IS NULL");
                    _log.InfoFormat("{0} rijen te verwerken voor klant: {1} met CustomerOrdernumber: {2}", customerRowsWithRef.Count(), soldToCustomer.CustomerID, reference);
                    GenerateExcelDocument(soldToCustomer, shiptoID, customerRowsWithRef, errormail, false, bskidentifier);
                  }
                  catch (Exception ex)
                  {
                    _log.ErrorFormat("Door een fout zal de Exel order voor {0} niet verder verwerkt worden", soldToCustomer.CustomerID);
                    _log.Debug(ex);
                  }
                }
              }
            }
          }
          else
          {
            _log.InfoFormat("Klant (klantnummer {0}) mag geen orders plaatsen via BAS EDI, deze order is niet verwerkt", soldToCustomer.CustomerID);
            _log.DebugFormat("Customer (customerID {0}) EDI information: processing instructions not valid", soldToCustomer.CustomerID);
          }
        }
        //}
        //else
        //{
        //  Logging.UserMail(string.Format("Klant (klantnummer {0}) mag geen orders plaatsen via BAS EDI, deze order is niet verwerkt", soldToID), errormail);
        //  Logging.WriteLogAndMailMessage(string.Format("Customer (customerID {0}) EDI information: inhibited from processing", soldToID));
        //}
      }
      catch (Exception ex)
      {
        _log.Debug(ex);
        throw ex;
      }
      finally
      {
        _connectorRelation = null;
      }
    }

    private void GenerateExcelDocument(ConnectorRelation soldToCustomer, string shiptoID, DataRow[] rows, string errormail, bool shipToAddress, int bskIdentifier)
    {
      if (rows.Count() > 0)
      {
        /// if (!Utility.CheckCustomerReference(rows[0]["CustomerOrderNumber"].ToString().Trim(), soldToCustomer.CustomerID, shiptoID))
        //{
        OrderRequest request = new OrderRequest();
        request.Version = "1.0";
        request.OrderHeader = new OrderRequestHeader();
        request.OrderHeader.ShipToCustomer = new Customer();
        request.OrderHeader.ShipToCustomer.EanIdentifier = shiptoID.ToString();
        if (!string.IsNullOrEmpty(rows[0]["CustomerOrderNumber"].ToString().Trim()))
          request.OrderHeader.CustomerOrderReference = rows[0]["CustomerOrderNumber"].ToString().Trim().Cap(25);
        else
          request.OrderHeader.CustomerOrderReference = "BASEDI-" + System.DateTime.Now.ToString("ddMMyyyy HH:mm:ss");

        request.OrderHeader.BSKIdentifier = bskIdentifier;
        request.OrderHeader.RequestedDate = DateTime.Now;
        request.OrderHeader.EdiVersion = "2.0";
        request.OrderDetails = new OrderRequestDetail[rows.Count()];

        if (rows[0].Table.Columns.Contains("EndCustomerReference"))
        {
          if (rows[0]["EndCustomerReference"] != null && !string.IsNullOrEmpty(rows[0]["EndCustomerReference"].ToString()))
            request.OrderHeader.EndCustomerOrderReference = rows[0]["EndCustomerReference"].ToString();
        }



        if (shipToAddress)
        {
          string housenumber = rows[0]["HouseNumber"].ToString();
          if (rows[0].Table.Columns.Contains("HouseNumberExtension"))
          {
            if (rows[0]["HouseNumberExtension"] != null && !string.IsNullOrEmpty(rows[0]["HouseNumberExtension"].ToString()))
              housenumber += rows[0]["HouseNumberExtension"].ToString();
          }

          request.OrderHeader.CustomerOverride = new CustomerOverride();
          request.OrderHeader.CustomerOverride.Dropshipment = true;
          request.OrderHeader.CustomerOverride.OrderAddress = new Concentrator.Web.Objects.EDI.Address();
          if (rows[0]["MailingName"].ToString().Length > 40)
            request.OrderHeader.CustomerOverride.OrderAddress.Name = rows[0]["MailingName"].ToString().Remove(40);
          else
            request.OrderHeader.CustomerOverride.OrderAddress.Name = rows[0]["MailingName"].ToString();
          request.OrderHeader.CustomerOverride.OrderAddress.AddressLine1 = rows[0]["Street"].ToString() + " " + housenumber;
          request.OrderHeader.CustomerOverride.OrderAddress.City = rows[0]["City"].ToString();
          request.OrderHeader.CustomerOverride.OrderAddress.Country = rows[0]["Country"].ToString();
          request.OrderHeader.CustomerOverride.OrderAddress.ZipCode = rows[0]["ZIPcode"].ToString();

          if (rows[0].Table.Columns.Contains("Attn"))
          {
            if (rows[0]["Attn"] != null && !string.IsNullOrEmpty(rows[0]["Attn"].ToString()))
              request.OrderHeader.CustomerOverride.OrderAddress.AddressLine3 = rows[0]["Attn"].ToString();
          }

          if (rows[0].Table.Columns.Contains("Email"))
          {
            if (rows[0]["Email"] != null && !string.IsNullOrEmpty(rows[0]["Email"].ToString()))
            {
              request.OrderHeader.CustomerOverride.CustomerContact = new Contact();
              request.OrderHeader.CustomerOverride.CustomerContact.Email = rows[0]["Email"].ToString();
            }
          }
        }
        else
        {
          request.OrderHeader.CustomerOverride = new CustomerOverride();
          request.OrderHeader.CustomerOverride.Dropshipment = false;
          request.OrderHeader.CustomerOverride.OrderAddress = new Concentrator.Web.Objects.EDI.Address();
        }

        int rowcount = 0;
        foreach (DataRow row in rows)
        {
          request.OrderDetails[rowcount] = new OrderRequestDetail();
          request.OrderDetails[rowcount].ProductIdentifier = new ProductIdentifier();
          request.OrderDetails[rowcount].ProductIdentifier.ProductNumber = row["ItemNumber"].ToString().Trim();
          request.OrderDetails[rowcount].Quantity = int.Parse(row["Quantity"].ToString());
          request.OrderDetails[rowcount].VendorItemNumber = row["CustomerItemNumber"].ToString();
          request.OrderDetails[rowcount].WareHouseCode = string.Empty;

          if (rows[0].Table.Columns.Contains("Price"))
          {
            decimal price = 0;
            if (rows[0]["Price"] != null && decimal.TryParse(rows[0]["Price"].ToString(), out price))
              request.OrderDetails[rowcount].Price = price;
          }
          rowcount++;
        }
        try
        {
          //StringBuilder replyString = new StringBuilder();

          //XmlWriterSettings settings = new XmlWriterSettings();
          //settings.Encoding = Encoding.UTF8;
          //XmlWriter xw = XmlWriter.Create(replyString, settings);
          //xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");

          //XmlSerializer rxs = new XmlSerializer(typeof(EdiOrder));
          //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
          //ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
          //rxs.Serialize(xw, request, ns);

          //XmlDocument replyXml = new XmlDocument();
          //replyXml.LoadXml(replyString.ToString());

          int orderID = -1;
          EdiOrder ediOrder = null;

          AddressNL address = null;

           var account = new Concentrator.Objects.Models.Orders.Customer()
          {
            EANIdentifier = request.OrderHeader.ShipToCustomer.EanIdentifier, //<AccountNumberCustomer>000309</AccountNumberCustomer> 
          };

          if (request.OrderHeader.CustomerOverride != null && request.OrderHeader.CustomerOverride.Dropshipment)
          {
            var customerOverride = request.OrderHeader.CustomerOverride.OrderAddress;
            account.CompanyName = customerOverride.Name;
            account.CustomerName = string.Empty;
            account.CustomerAddressLine1 = customerOverride.AddressLine1; //<Street>BIESENWEG 16</Street> 
            account.HouseNumber = customerOverride.HouseNumber;
            account.PostCode = customerOverride.ZipCode;
            account.City = customerOverride.City;
            account.Country = customerOverride.Country;

          }

             EdiOrderListener ediListener = new EdiOrderListener()
                  {
                    CustomerName = "new",
                    CustomerIP = "Unknown",
                    CustomerHostName = "Directory order",
                    RequestDocument = "Excel",
                    Processed = true,
                    ReceivedDate = DateTime.Now,
                    ConnectorID = 1
                  };
          _unit.Scope.Repository<EdiOrderListener>().Add(ediListener);
        
          ediOrder = new EdiOrder()
          {
            ConnectorID = soldToCustomer.ConnectorID.HasValue ? soldToCustomer.ConnectorID.Value : 1,
            WebSiteOrderNumber = null,
            CustomerOrderReference = request.OrderHeader.CustomerOrderReference, //<OrderNumberCustomer>049732040002</OrderNumberCustomer> 
            //OrderDate = DateTime.ParseExact(xmlHeader.Element("OrderDate").Value, "yyyyMMdd", null), //<OrderDate>20110315</OrderDate> 
            BackOrdersAllowed = true,
            ShippedToCustomer = account,
            SoldToCustomer = account,
            ReceivedDate = DateTime.Now,
            PartialDelivery =  true,
            EdiOrderTypeID = 1,
            EdiOrderListener = ediListener,
            Status = (int)EdiOrderStatus.Validate,
            ConnectorRelationID = bskIdentifier
            //<Reference1 /> 
            //<Reference2 /> 
            //<DeliveryType>99</DeliveryType> 
            // <OrderInformation>notsupplied</OrderInformation>
            //- <OrderOptions>
            //<DeliveryFull>N</DeliveryFull> 
            //<FulfillOptions>K</FulfillOptions> 
            //</OrderOptions>
          };


          _unit.Scope.Repository<Concentrator.Objects.Models.Orders.Customer>().Add(account);


          _unit.Scope.Repository<EdiOrder>().Add(ediOrder);

          foreach (var line in request.OrderDetails)
          {
            var ediLine = new EdiOrderLine()
            {
              //CustomerEdiOrderLineNr = line.Element("LineNumber").Value, //<LineNumber>000009</LineNumber> 
              CustomerItemNumber = line.ProductIdentifier.ProductNumber,
              EndCustomerOrderNr = line.VendorItemNumber,
              //ProductDescription = 
              Quantity = line.Quantity,
              //Price = double.Parse(line.Element("Cost").Value), //<Cost>000000000010.00</Cost> 
              Currency = null, // <Currency>EUR</Currency> 
              UnitOfMeasure = null, // <UnitOfMeasure>EACH</UnitOfMeasure>
              EdiOrder = ediOrder,
            };

            _unit.Scope.Repository<EdiOrderLine>().Add(ediLine);
            ediLine.SetStatus(EdiOrderStatus.Received, _unit);
            ediLine.SetStatus(EdiOrderStatus.Validate, _unit);
          }
          
          //}
          _unit.Save();


          //int requestID = InsertEdiListener(_unit, _config, _connectorRelation, replyXml.OuterXml, errormail);

          //LogFile("Excel OrderRequest", soldToCustomer.Name, Guid.NewGuid(), replyXml.OuterXml);
        }
        catch (Exception ex)
        {
          _log.Error("insert error", ex);
        }

        //info.EdiRequestID = DbUtility.InsertOrder(soldToCustomer.Name, errormail, info.ReceivedDocument, DocumentType.Excel.ToString());

        _log.InfoFormat("Order succesvol naar listener voor {0} ({1}) process to JDE next import run", soldToCustomer.Name, soldToCustomer.CustomerID);

        //WorkflowInstance instance = this.Runtime.CreateWorkflow(
        //      typeof(OrderWorkflow), null, info.DocumentID);

        //instance.Start();
        ////ADD tim 23-4
        //instance.Unload();

        //instance.EnqueueItem("ReceiveOrder", info, null, null);

       

        //EmailDaemon email = new EmailDaemon();
        //email.OrderReceivedSucceed(info.CustomerInfo);
        //}
        //else
        //{
        //  Logging.UserMail(string.Format("CustomerOrderReference ({0}) bestaat al for order {1} (shipto {2})", rows[0]["CustomerOrderNumber"].ToString().Trim(), soldToCustomer.Name, shiptoID), errormail, DocumentType.Excel);
        //  Logging.WriteLogMessage("CustomerOrderReference bestaat al");
        //  return;
        //}
      }
    }

    private List<DataRow> ValidateExcel(DataTable order, string errormail)
    {
      int rowCount = 2;
      List<DataRow> dataRowList = new List<DataRow>();

      foreach (DataRow row in order.Rows)
      {
        if (!row.IsNull("EdiIdentifier"))
        {
          int bskIdentifier = 0;
          if (!int.TryParse(row["EdiIdentifier"].ToString(), out bskIdentifier))
          {
            row.RowError = "EdiIdentifier dient nummeriek te zijn (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("EdiIdentifier dient nummeriek te zijn (rij " + rowCount.ToString());
          }
        }
        else
        {
          row.RowError = "Ongeldige EdiIdentifier (rij " + rowCount.ToString() + ")";
          _log.ErrorFormat("Ongeldige EdiIdentifier (rij " + rowCount.ToString());
        }

        if (!row.IsNull("ShiptoID"))
        {
          int shiptoID = 0;
          if (!int.TryParse(row["ShiptoID"].ToString(), out shiptoID))
          {
            row.RowError = "ShiptoID dient nummeriek te zijn (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("ShiptoID dient nummeriek te zijn (rij " + rowCount.ToString());
          }
        }
        else
        {
          row.RowError = "Ongeldige ShiptoID (rij " + rowCount.ToString() + ")";
          _log.ErrorFormat("Ongeldige ShiptoID (rij " + rowCount.ToString());
        }

        if (!row.IsNull("ItemNumber"))
        {
          int itemNR = 0;
          if (string.IsNullOrEmpty(row["ItemNumber"].ToString()))
          {
            row.RowError = "ItemNumber ongeldig te zijn (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("ItemNumber ongeldig te zijn (rij " + rowCount.ToString());
          }
          else
          {
            //TODO
            //if (!DbUtility.CheckItem(itemNR))
            //{
            //  row.RowError = "ItemNumber onbekend (rij " + rowCount.ToString() + ")";
            //  Logging.WriteLogMessage("ItemNumber onbekend (rij " + rowCount.ToString());
            //}
          }
        }
        else
        {
          row.RowError = "Ongeldige ItemNumber (rij " + rowCount.ToString() + ")";
          _log.ErrorFormat("Ongeldige ItemNumber (rij " + rowCount.ToString());
        }

        if (!row.IsNull("Quantity"))
        {
          int shiptoID = 0;
          if (!int.TryParse(row["Quantity"].ToString(), out shiptoID))
          {
            row.RowError = "Quantity dient nummeriek te zijn (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("Quantity dient nummeriek te zijn (rij " + rowCount.ToString());
          }
          else
          {
            if (shiptoID < 1)
            {
              row.RowError = "Quantity dient groter dan 0 te zijn (rij " + rowCount.ToString() + ")";
              _log.ErrorFormat("Quantity dient groter dan 0 te zijn (rij " + rowCount.ToString());
            }
          }
        }
        else
        {
          row.RowError = "Ongeldige Quantity (rij " + rowCount.ToString() + ")";
          _log.ErrorFormat("Ongeldige Quantity (rij " + rowCount.ToString());
        }

        if (!row.IsNull("CustomerOrderNumber"))
        {
          if (string.IsNullOrEmpty(row["CustomerOrderNumber"].ToString()))
          {
            row.RowError = "Geen of ongeldig CustomerOrderNumber: " + row["CustomerOrderNumber"].ToString() + " (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("Geen of ongeldig CustomerOrderNumber: " + row["CustomerOrderNumber"].ToString() + " (rij " + rowCount.ToString());
          }
          if (row["CustomerOrderNumber"].ToString().Length > 25)
          {
            row.RowError = "Ongeldige Lengte van CustomerOrderNumber (maximaal 25 tekens): " + row["CustomerOrderNumber"].ToString() + " (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("Ongeldige Lengte van CustomerOrderNumber (maximaal 25 tekens): " + row["CustomerOrderNumber"].ToString() + " (rij " + rowCount.ToString());
          }
        }

        if (!row.IsNull("CustomerItemNumber"))
        {
          if (row["CustomerItemNumber"].ToString().Contains(','))
          {
            row.RowError = "Ongeldig teken in CustomerItemNumber: " + row["CustomerItemNumber"].ToString() + " (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("Ongeldig teken in CustomerItemNumber: " + row["CustomerItemNumber"].ToString() + " (rij " + rowCount.ToString());
          }
          if (row["CustomerItemNumber"].ToString().Length > 25)
          {
            row.RowError = "Ongeldige Lengte van CustomerItemNumber (maximaal 25 tekens): " + row["CustomerItemNumber"].ToString() + " (rij " + rowCount.ToString() + ")";
            _log.ErrorFormat("Ongeldige Lengte van CustomerItemNumber (maximaal 25 tekens): " + row["CustomerItemNumber"].ToString() + " (rij " + rowCount.ToString());
          }
        }

        rowCount++;

        if (!string.IsNullOrEmpty(row.RowError))
        {
          dataRowList.Add(row);
        }
      }

      if (order.HasErrors)
      {
        //TODO
        //Logging.UserMail(order.GetErrors(), errormail);

      }
      return dataRowList;

    }

    private int InsertEdiListener(IUnitOfWork unit, System.Configuration.Configuration config, ConnectorRelation relation, string responseString, string mailAddress)
    {
      EdiOrderListener ediListener = new EdiOrderListener()
      {
        CustomerName = relation == null ? "new" : relation.Name,
        CustomerIP = "Mail",
        CustomerHostName = mailAddress,
        RequestDocument = responseString,
        ReceivedDate = DateTime.Now,
        Processed = false,
        ConnectorID = int.Parse(config.AppSettings.Settings["DefaultConnectorID"].Value)
      };

      if (relation != null && relation.ConnectorID.HasValue)
        ediListener.ConnectorID = relation.ConnectorID.Value;

      unit.Scope.Repository<EdiOrderListener>().Add(ediListener);
      unit.Save();
      return ediListener.EdiRequestID;
    }

    private void LogFile(string type, string customerName, Guid instanceID, string fileContents)
    {
      string path = _config.AppSettings.Settings["ArchivePath"].Value;
      if (!path.EndsWith(@"\"))
        path += @"\";

      path += customerName + "-" + instanceID.ToString();
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      if (!path.EndsWith(@"\"))
        path += @"\";
      Guid g = Guid.NewGuid();

      File.WriteAllText(path + type + "-" + g.ToString() + ".txt", fileContents);
    }

   
  }

  class ShipToAddress
  {
    public string ZIPcode { get; set; }
    public string Housenumber { get; set; }
    public string HousenumberExtenstion { get; set; }
    public string Street { get; set; }
    public string MailingName { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Email { get; set; }
  }
}
