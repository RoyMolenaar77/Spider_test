using System.Diagnostics;
using System.Globalization;
using AuditLog4Net.Adapter;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Plugins.Axapta.Helpers;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Plugins.Axapta.Repositories;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Concentrator.Plugins.Axapta.Services
{
  public class PurchaseOrderService : IPurchaseOrderService, IExportPurchaseOrderReceivedConfirmationService
  {
    #region "Variables"

    private readonly FtpSetting _ftpSetting = new FtpSetting();
    private int _sapphConnectorID;

    private string _failedPurchaseOrderNumber;
    private string FailedPurchaseOrderFileName
    {
      get
      {
        return String.Format("FailedPurchaseOrder_{0:yyyy-MM-dd_Hmmss}_{1}.csv", DateTime.Now, _failedPurchaseOrderNumber);
      }
    }
    private static string PurchaseOrderReceivedConfirmationFileName
    {
      get
      {
        return String.Format("RCOrder_{0:yyyy-MM-dd_Hmmss_ff}.csv", DateTime.Now);
      }
    }

    private readonly List<DatColReceivedPurchaseOrderConfirmation> _listOfPurchaseOrders = new List<DatColReceivedPurchaseOrderConfirmation>();

    protected virtual Regex FileNameRegex
    {
      get
      {
        return new Regex(".*\\.txt$", RegexOptions.IgnoreCase);
      }
    }

    List<int> vendorIDs = new List<int>() 
    {
      SapphBEVendorID,
      SapphVendorID
    };
    
    const int SapphBEVendorID = 51;
    const int SapphVendorID = 50;
    const string RelatedConnectorID = "RelatedConnectorID";

    const string FtpAddressSettingKey = "FtpAddress";
    const string FtpUsernameSettingKey = "FtpUsername";
    const string FtpPasswordSettingKey = "FtpPassword";
    const string PathSettingKey = "Ax Ftp Dir PurchaseOrder";
    const string PathReceivedConfirmationPurchaseOrderSettingkey = "Ax Ftp Dir ReceivedConfirmation PurchaseOrder";

    private const string SapphVendorName = "Sapph";

    #endregion

    private readonly IAuditLogAdapter _log;
    private readonly IOrderRepository _orderRepo;
    private readonly IOrderHelper _orderHelper;
    private readonly IArchiveService _archiveService;
    private readonly IProductRepository _productRepo;
    private readonly IVendorRepository _vendorRepo;
    private readonly IVendorSettingRepository _vendorSettingRepo;
    public PurchaseOrderService(
      IVendorSettingRepository vendorSettingRepo,
      IAuditLogAdapter log,
      IArchiveService archiveService,
      IOrderRepository orderRepo,
      IProductRepository productRepo, IVendorRepository vendorRepo, IOrderHelper orderHelper)
    {
      _log = log;
      _orderRepo = orderRepo;
      _productRepo = productRepo;
      _vendorRepo = vendorRepo;
      _orderHelper = orderHelper;
      _vendorSettingRepo = vendorSettingRepo;

      _archiveService = archiveService;
    }

    public void Process()
    {
      var vendorSapph = _vendorRepo
        .GetVendor(SapphVendorName);

      if (vendorSapph == null)
      {
        _log.AuditError("Vendor Sapph does not exist. Please insert vendor Sapph.");

        return;
      }

      var vendorSettings = vendorSapph.VendorSettings.ToDictionary(x => x.SettingKey, x => x.Value);

      if (!vendorSettings.ContainsKey(RelatedConnectorID))
      {
        _log.AuditError("SettingKey RelatedConnectorID for vendor Sapph does not exist. Please insert VendorSetting RelatedConnectorID for vendor Sapph.");

        return;
      }

      if (!int.TryParse(vendorSettings[RelatedConnectorID], NumberStyles.Integer, new CultureInfo("en-US"), out _sapphConnectorID))
      {
        _log.AuditError(string.Format("SettingKey RelatedConnectorID for vendor Sapph has not integer value '{0}'. Please change the value.", vendorSettings[RelatedConnectorID]));

        return;
      }

      if (!vendorSettings.ContainsKey(PathSettingKey))
      {
        _log.AuditError(string.Format("SettingKey '{0}' for vendor Sapph does not exist. Please insert VendorSetting '{0}' for vendor Sapph.", PathSettingKey));

        return;
      }

      if (!vendorSettings.ContainsKey(FtpAddressSettingKey))
      {
        _log.AuditError(string.Format("SettingKey '{0}' for vendor Sapph does not exist. Please insert VendorSetting '{0}' for vendor Sapph.", FtpAddressSettingKey));

        return;
      }

      if (!vendorSettings.ContainsKey(FtpUsernameSettingKey))
      {
        _log.AuditError(string.Format("SettingKey '{0}' for vendor Sapph does not exist. Please insert VendorSetting '{0}' for vendor Sapph.", FtpUsernameSettingKey));

        return;
      }

      if (!vendorSettings.ContainsKey(FtpPasswordSettingKey))
      {
        _log.AuditError(string.Format("SettingKey '{0}' for vendor Sapph does not exist. Please insert VendorSetting '{0}' for vendor Sapph.", FtpPasswordSettingKey));

        return;
      }

#if DEBUG
      _ftpSetting.Path = vendorSettings[PathSettingKey].Replace("Staging", "Test");
#else
      _ftpSetting.Path = vendorSettings[PathSettingKey];
#endif
      _ftpSetting.FtpAddress = vendorSettings[FtpAddressSettingKey];
      _ftpSetting.FtpUsername = vendorSettings[FtpUsernameSettingKey];
      _ftpSetting.FtpPassword = vendorSettings[FtpPasswordSettingKey];

      ProcessFiles();

      //FillSettings(FtpSettingTypes.PurchaseOrder);

      //ReadPurchaseFile();
    }

    private void ProcessFiles()
    {
      var ftpManager = new FtpManager(_ftpSetting.FtpUri, null, false, true);
      var regex = FileNameRegex;

      foreach (var fileName in ftpManager.GetFiles())
      {
        if (!regex.IsMatch(fileName)) continue;

        using (var stream = ftpManager.Download(Path.GetFileName(fileName)))
        {
          using (var streamReader = new StreamReader(stream))
          {
            try
            {
              var engine = new FileHelperEngine(typeof(DatColPurchaseOrder));
              var listofPickTickets = engine.ReadStream(streamReader) as DatColPurchaseOrder[];

              ProcessFile(listofPickTickets);

              _archiveService.CopyToArchive(ftpManager.BaseUri.AbsoluteUri, SaveTo.PurchaseOrderDirectory, fileName);

              ftpManager.Delete(fileName);
            }
            catch (Exception)
            {
              _log.AuditError(string.Format("Failed to process the Purchase order file. File name: '{0}'", fileName));
            }
          }
        }
      }
    }

    private void ProcessFile(IEnumerable<DatColPurchaseOrder> listofPickTickets)
    {
      var orders = listofPickTickets.GroupBy(x => x.OrderNumber);
      foreach (var order in orders)
      {
        var firstOrder = order.First();

        var code = firstOrder.OrderNumber.Substring(0, 3).ToLower();

        var isPurchaseCode = code == "lio";

        var isPurchaseOrder = isPurchaseCode;
        var isOrderCorrupt = !isPurchaseCode;

        if (isPurchaseOrder)
        {
          if (ProcessPurchaseOrder(firstOrder, order))
          {
            _log.AuditSuccess(string.Format("Purchase Order '{0}' successfully inserted", firstOrder.OrderNumber));
          }
          else
          {
            _log.AuditError("Failed to create Purchase Order. Please read '.log' file.");
          }
        }

        if (isOrderCorrupt)
        {
          _failedPurchaseOrderNumber = firstOrder.OrderNumber;
          _archiveService.ExportToAxapta(order, SaveTo.CorruptPurchaseOrderDirectory, FailedPurchaseOrderFileName);
        }
      }
    }

    private bool ProcessPurchaseOrder(DatColPurchaseOrder firstOrder, IEnumerable<DatColPurchaseOrder> orderLines)
    {
      List<DatColErrorMessage> listOfErrors;

      var listOfPurchaseOrderLines = orderLines as DatColPurchaseOrder[] ?? orderLines.ToArray();

      if (_orderHelper.IsValidPurchaseOrder(listOfPurchaseOrderLines, _sapphConnectorID, vendorIDs, out listOfErrors, true))
      {
        if (SavePurchaseOrder(firstOrder, listOfPurchaseOrderLines))
        {
          return true;
        }

        _failedPurchaseOrderNumber = firstOrder.OrderNumber;
        _archiveService.ExportToAxapta(listOfPurchaseOrderLines, SaveTo.CorruptPurchaseOrderDirectory, FailedPurchaseOrderFileName);

        return false;
      }

      _failedPurchaseOrderNumber = firstOrder.OrderNumber;
      _archiveService.ExportToAxapta(listOfPurchaseOrderLines, SaveTo.CorruptPurchaseOrderDirectory, FailedPurchaseOrderFileName);

      var fileName = string.Format("{0}.log", Path.GetFileNameWithoutExtension(FailedPurchaseOrderFileName));

      _archiveService.ExportToAxapta(listOfErrors, SaveTo.CorruptPurchaseOrderDirectory, fileName);

      return false;
    }

    private bool SavePurchaseOrder(DatColPurchaseOrder firstOrder, IEnumerable<DatColPurchaseOrder> listOfPurchaseOrderLines)
    {
      try
      {
        var listOfCurrentWebOrderNumbers = _orderRepo
          .GetOrderByContainsWebOrderNumber(firstOrder.OrderNumber)
          .Select(x => x.WebSiteOrderNumber)
          .ToArray();

        string tempWebOrderNumber;
        if (!_orderHelper.ConvertOriginalOrderNumberToConcentratorOrderNumber(firstOrder.OrderNumber, listOfCurrentWebOrderNumbers, 9, out tempWebOrderNumber))
        {
          _log.AuditError(string.Format("OrderNumber {0} cannot be converted. This ordernumbers are already exist: {1}",
            firstOrder.OrderNumber,
            string.Join(", ", listOfCurrentWebOrderNumbers)));
          return false;
        }

        var orderToSave = CreateOrder(tempWebOrderNumber, firstOrder, listOfPurchaseOrderLines);

        if (!_orderRepo.InsertOrder(orderToSave))
        {
          _log.AuditError(string.Format("Failed to insert order: {0}", orderToSave.WebSiteOrderNumber));
          return false;
        }
        return true;
      }
      catch (Exception)
      {
        _log.AuditError(string.Format("Database Error: Failed to save the order. OrderNumber '{0}'", firstOrder.OrderNumber));
        return false;
      }
    }

    private Order CreateOrder(string webOrderNumber, DatColPurchaseOrder order, IEnumerable<DatColPurchaseOrder> orderLines)
    {
      var orderToSave = new Order
      {
        OrderLines = new List<OrderLine>(),
        ConnectorID = _sapphConnectorID,
        CustomerOrderReference = string.Format("{0} - {1}", order.VendorCode, order.VendorName),
        HoldOrder = false,
        WebSiteOrderNumber = webOrderNumber,
        PaymentTermsCode = "SHOP",
        PaymentInstrument = string.Empty,
        ReceivedDate = DateTime.Now.ToUniversalTime(),
        OrderType = (int)OrderTypes.PurchaseOrder
      };

      var engine = new FileHelperEngine(typeof(DatColPurchaseOrder));

      var currentVendorAssortments = _productRepo
        .GetListOfSkusByVendorIDs(vendorIDs)
        .ToLookup(x => Regex.Replace(x.VendorItemNumber, @"\s+", ""))
        .ToDictionary(x => x.Key, x => x.First());

      foreach (var orderLine in orderLines)
      {
        var orderLineToSave = new OrderLine
        {
          OrderID = orderToSave.OrderID,
          CustomerOrderNr = orderToSave.WebSiteOrderNumber,
          ProductID = currentVendorAssortments[Regex.Replace(orderLine.CustomItemNumber, @"\s+", "")].ProductID,
          Quantity = int.Parse(orderLine.Quantity, NumberStyles.Number, new CultureInfo("nl-NL")),
          isDispatched = false,
          OriginalLine = engine.WriteString(new List<DatColPurchaseOrder> { orderLine }),
          PriceOverride = false,
          WareHouseCode = orderLine.StockWarehouse
        };
        orderToSave.OrderLines.Add(orderLineToSave);
      }
      return orderToSave;
    }

    void FillSettings(FtpSettingTypes type)
    {
      switch (type)
      {
        case FtpSettingTypes.PurchaseOrder:
          _ftpSetting.Path = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PathSettingKey);
          break;
        case FtpSettingTypes.PurchaseOrderReceivedConfirmation:
          _ftpSetting.Path = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PathReceivedConfirmationPurchaseOrderSettingkey);
          break;
        default:
          throw new NotImplementedException();
      }

      _ftpSetting.FtpAddress = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpAddressSettingKey);
      _ftpSetting.FtpUsername = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpUsernameSettingKey);
      _ftpSetting.FtpPassword = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpPasswordSettingKey);

      var connectorID = _vendorSettingRepo.GetVendorSetting(SapphVendorID, RelatedConnectorID).ParseToInt();
      if (connectorID == null) throw new NullReferenceException("ConnectorID cannot be null");

      _sapphConnectorID = connectorID.Value;
    }

    public void ExportReceivedConfirmation(IEnumerable<OrderResponseLine> listOfOrderResponseLines)
    {
      FillSettings(FtpSettingTypes.PurchaseOrderReceivedConfirmation);
      FillPurchaseOrders(listOfOrderResponseLines);
      UploadReceivedConfirmation();
      //CopyReceivedConfirmationFileToArchive();
    }

    void FillPurchaseOrders(IEnumerable<OrderResponseLine> listOfOrderResponseLines)
    {
      var engine = new FileHelperEngine(typeof(DatColPurchaseOrder));

      foreach (var orderResponseLineGroup in listOfOrderResponseLines.GroupBy(x => x.OrderLineID))
      {
        OrderResponseLine orderResponseLine = orderResponseLineGroup.Count() == 1 ? orderResponseLineGroup.First() : orderResponseLineGroup.FirstOrDefault(x => x.Shipped > 0);

        if (orderResponseLine == null || !orderResponseLine.OrderLineID.HasValue) continue;

        var orderLine = _orderRepo.GetOrderLineByID(orderResponseLine.OrderLineID.Value);
        if (orderLine == null || !orderLine.ProductID.HasValue) continue;

        //var product = _productRepo.GetProductByID(orderLine.ProductID.Value);
        //if (product == null) continue;

        var purchaseOrder = engine.ReadString(orderLine.OriginalLine) as DatColPurchaseOrder[];
        var firstRow = purchaseOrder.First();
        if (firstRow == null) continue;

        var datColReceivedPurchaseOrderConfirmation = new DatColReceivedPurchaseOrderConfirmation
          {
            VendorCode = firstRow.VendorCode,
            ReceivedDate = firstRow.ReceivedDate,
            VendorName = firstRow.VendorName,
            OrderNumber = firstRow.OrderNumber,
            SeasonCode = firstRow.SeasonCode,
            ModelCode = firstRow.ModelCode,
            ModelDescription = firstRow.ModelDescription,
            ModelColor = firstRow.ModelColor,
            ModelColorDescription = firstRow.ModelColorDescription,
            Size = firstRow.Size,
            Subsize = firstRow.Subsize,
            StockWarehouse = firstRow.StockWarehouse,
            Quantity = firstRow.Quantity,
            ReceivedQuantity = orderResponseLine.Shipped.ToString(CultureInfo.InvariantCulture),
            CustomItemNumber = firstRow.CustomItemNumber,
            Barcode = firstRow.Barcode,
            PurchaseOrderID = firstRow.PurchaseOrderID
          };

        _listOfPurchaseOrders.Add(datColReceivedPurchaseOrderConfirmation);
      }
    }
    void UploadReceivedConfirmation()
    {
      if (_listOfPurchaseOrders.Count <= 0) return;

      var fileName = PurchaseOrderReceivedConfirmationFileName;
      _archiveService.ExportToAxapta(_listOfPurchaseOrders, SaveTo.ReceivedPurchaseOrderConfirmationDirectory, fileName);
      _archiveService.ExportToArchive(_listOfPurchaseOrders, SaveTo.ReceivedPurchaseOrderConfirmationDirectory, fileName);
    }
  }
}
