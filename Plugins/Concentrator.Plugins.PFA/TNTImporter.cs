using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ftp;
using Concentrator.Objects;
using Concentrator.Plugins.Axapta;
using System.Configuration;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.PFA
{
  /// <summary>
  /// Represents the base for a TNT Fashion related document importer.
  /// </summary>
  public abstract class TNTImporter
  {
    protected Dictionary<String, XDocument> Documents
    {
      get;
      private set;
    }

    protected virtual Regex FileNameRegex
    {
      get
      {
        return new Regex(".*\\.xml$", RegexOptions.IgnoreCase);
      }
    }

    protected virtual String ValidationFileName
    {
      get
      {
        return null;
      }
    }

    protected IAuditLogAdapter Log
    {
      get;
      private set;
    }

    protected IUnitOfWork Unit
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the vendor associated to this importer.
    /// </summary>
    protected Vendor Vendor
    {
      get;
      private set;
    }

    /// <summary>
    /// Executes the TNT Fashion importer.
    /// </summary>
    public void Execute(string sourceUri, Vendor vendor)
    {
      LoadDocuments(sourceUri);

      foreach (var fileName in Documents.Keys)
      {
        try
        {

          //ValidateDocument(fileName);


          if (Process(fileName, vendor))
          {
            try
            {

              ArchiveDocument(fileName, sourceUri);

            }

            catch (Exception exception)
            {
              Log.AuditError(String.Format("Failure while archiving '{0}'.", fileName), exception);
            }
          }
        }
        catch (Exception exception)
        {
          Log.AuditError(String.Format("Failure while processing '{0}'.", fileName), exception);
        }
      }
    }

    protected virtual void ArchiveDocument(String fileName, string sourceUri)
    {
      var directoryInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Archive"));

      if (!directoryInfo.Exists)
        directoryInfo.Create();

      Documents[fileName].Save(Path.Combine(directoryInfo.FullName, fileName));

      new FtpManager(sourceUri, Log).Delete(fileName);
    }

    protected virtual void ValidateDocument(String fileName)
    {
      if (!String.IsNullOrWhiteSpace(ValidationFileName) && File.Exists(ValidationFileName))
      {
        using (var fileStream = File.OpenRead(ValidationFileName))
        {
          var validationSchema = XmlSchema.Read(fileStream, (sender, arguments) =>
          {
            if (arguments.Severity == XmlSeverityType.Error)
            {
              throw new Exception(arguments.Message);
            }
          });

          var validationSchemaSet = new XmlSchemaSet();

          validationSchemaSet.Add(validationSchema);

          Documents[fileName].Validate(validationSchemaSet, (sender, arguments) =>
          {
            switch (arguments.Severity)
            {
              case XmlSeverityType.Error:
                Log.AuditError(arguments.Message);

                throw new Exception(arguments.Message);

              case XmlSeverityType.Warning:
                Log.AuditWarning(arguments.Message);
                break;
            }
          });
        }
      }
    }

    public static string GetSku(XElement element, bool trimmed)
    {
      string sku = string.Empty;

      if (trimmed)
      {
        sku = String.Join(""
            , (String)element.XPathEvaluate("string(article_code/text())")
            , (String)element.XPathEvaluate("string(color_code/text())")
            , (String)element.XPathEvaluate("string(size_code/text())"));
      }
      else
      {
        sku = String.Join(" "
            , (String)element.XPathEvaluate("string(article_code/text())")
            , (String)element.XPathEvaluate("string(color_code/text())")
            , (String)element.XPathEvaluate("string(size_code/text())"));
      }

      return sku.Trim();
    }

    public static string GetShipmentCostsProduct(Connector connector, bool useKialaShipmentCosts)
    {
      string shippingCostProduct = string.Empty;

      if (useKialaShipmentCosts)
      {
        shippingCostProduct = connector.ConnectorSettings.GetValueByKey("KialaShipmentCostsVendorItemNumber", string.Empty);
        shippingCostProduct.ThrowIfNullOrEmpty(new InvalidOperationException("KialaShipmentCostsVendorItemNumber must be defined for connector " + connector.Name));
      }
      else
      {
        shippingCostProduct = connector.ConnectorSettings.GetValueByKey("ShipmentCostsVendorItemNumber", string.Empty);
        shippingCostProduct.ThrowIfNullOrEmpty(new InvalidOperationException("ShipmentCostsVendorItemNumber must be defined for connector " + connector.Name));
      }
      return shippingCostProduct;
    }

    public static string GetShipmentCostsProduct(int connectorID, IUnitOfWork unit, bool useKialaShipmentCosts)
    {
      Connector connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

      return GetShipmentCostsProduct(connector, useKialaShipmentCosts);
    }

    public static string GetReturnCostsProduct(Connector connector, bool useKialaReturnCosts)
    {
      string returnCostProduct = string.Empty;

      if (useKialaReturnCosts)
      {
        returnCostProduct = connector.ConnectorSettings.GetValueByKey("KialaReturnCostsVendorItemNumber", string.Empty);
        returnCostProduct.ThrowIfNullOrEmpty(new InvalidOperationException("KialaReturnCostsVendorItemNumber must be defined for connector " + connector.Name));
      }
      else
      {
        returnCostProduct = connector.ConnectorSettings.GetValueByKey("ReturnCostsVendorItemNumber", string.Empty);
        returnCostProduct.ThrowIfNullOrEmpty(new InvalidOperationException("ReturnCostsVendorItemNumber must be defined for connector " + connector.Name));
      }

      return returnCostProduct;
    }

    public static void SendToAxapta(Order order, OrderResponseTypes orderResponseType, List<OrderResponseLine> orderResponseLines)
    {
      bool useAxapta = order.Connector.ConnectorSettings.GetValueByKey("UseAxapta", false);

      if (useAxapta)
      {
        switch (orderResponseType)
        {
          case OrderResponseTypes.ReceivedNotification:
            using (var received = new ProcessPurchaseOrderReceivedConfirmation())
            {
              received.Process(orderResponseLines);
            }
            break;

          case OrderResponseTypes.ShipmentNotification:
            //using (var pickticket = new ProcessPickTicketShipmentConfirmation())
            //{
            //  pickticket.Process(orderResponseLines);
            //}
            break;

          default:
            throw new NotImplementedException();
        }
      }
    }

    public static void SendToAxapta(Vendor vendor, List<DatColStock> datColStock)
    {
      bool useAxapta = vendor.VendorSettings.GetValueByKey("UseAxapta", false);

      if (useAxapta)
      {
        using (var stockMutation = new ProcessExportCorrectionStock())
        {
          stockMutation.Process(datColStock);
        }
      }
    }

    protected virtual void LoadDocuments(string sourceUri)
    {
      var ftpManager = new FtpManager(sourceUri, Log, usePassive: true);
      var regex = FileNameRegex;

      foreach (var fileName in ftpManager.GetFiles())
      {
        if (regex.IsMatch(fileName))
        {
          using (var stream = ftpManager.Download(fileName))
          {
            Documents[fileName] = XDocument.Load(stream);
          }
        }
      }
    }

    /// <summary>
    /// Process the specified file.
    /// </summary>
    /// <param name="fileName">
    /// Represents the key for a document in the <see cref="Documents"/>-dictionary.
    /// </param>
    protected abstract bool Process(String fileName, Vendor vendor);

    protected TNTImporter(Vendor vendor, IUnitOfWork unit, IAuditLogAdapter log)
    {
      Documents = new Dictionary<String, XDocument>();
      Log = log;
      Unit = unit;
      Vendor = vendor;
    }
  }
}
