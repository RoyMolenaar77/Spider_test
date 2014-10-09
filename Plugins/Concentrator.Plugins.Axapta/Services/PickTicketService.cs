using System.Globalization;
using System.Xml.Linq;
using AuditLog4Net.Adapter;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
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
  public class PickTicketService : IPickTicketService
  {
    #region "Variables"

    private string _failedPickTicketOrderNumber;
    private string FailedPickTicketOrderFileName
    {
      get
      {
        return String.Format("FailedPickTicketOrder_{0:yyyy-MM-dd_Hmmss}_{1}.csv", DateTime.Now, _failedPickTicketOrderNumber);
      }
    }

    protected virtual Regex FileNameRegex
    {
      get
      {
        return new Regex(".*\\.txt$", RegexOptions.IgnoreCase);
      }
    }

    private const string RelatedConnectorID = "RelatedConnectorID";
    private const string SapphVendorName = "Sapph";
    private const string PathSettingKey = "Ax Ftp Dir PickTicket";
    private const string FtpAddressSettingKey = "FtpAddress";
    private const string FtpUsernameSettingKey = "FtpUsername";
    private const string FtpPasswordSettingKey = "FtpPassword";

    private int _sapphConnectorID;

    private readonly FtpSetting _ftpSetting = new FtpSetting();
    private Vendor _vendorSapph;

    List<int> vendorIDs = new List<int>() 
    {
      SapphBEVendorID,
      SapphVendorID
    };

    const int SapphBEVendorID = 51;
    const int SapphVendorID = 50;
    
    private readonly IAuditLogAdapter _log;
    private readonly IOrderHelper _orderHelper;
    private readonly IOrderRepository _orderRepo;
    private readonly IArchiveService _archiveService;
    private readonly IProductRepository _productRepo;
    private readonly IVendorRepository _vendorRepo;

    #endregion

    public PickTicketService(
      IAuditLogAdapter log,
      IArchiveService archiveService,
      IOrderRepository orderRepo, IVendorRepository vendorRepo,
      IProductRepository productRepo, IOrderHelper orderHelper)
    {
      _log = log;
      _orderRepo = orderRepo;
      _productRepo = productRepo;
      _orderHelper = orderHelper;
      _vendorRepo = vendorRepo;

      _archiveService = archiveService;
    }

    public void Process()
    {
      _vendorSapph = _vendorRepo
        .GetVendor(SapphVendorName);

      if (_vendorSapph == null)
      {
        _log.AuditError("Vendor Sapph does not exist. Please insert vendor Sapph.");

        return;
      }

      var vendorSettings = _vendorSapph.VendorSettings.ToDictionary(x => x.SettingKey, x => x.Value);

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

      //FillSettings(FtpSettingTypes.PickTicket);
      //ReadWebPickTicketFile();
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
              var engine = new FileHelperEngine(typeof(DatColPickTicket));
              var listofPickTickets = engine.ReadStream(streamReader) as DatColPickTicket[];

              ProcessFile(listofPickTickets);

              _archiveService.CopyToArchive(ftpManager.BaseUri.AbsoluteUri, SaveTo.PickTicketDirectory, fileName);

              ftpManager.Delete(fileName);
            }
            catch (Exception)
            {
              _log.AuditError(string.Format("Failed to process the Sales/Transfer order file. File name: '{0}'", fileName));
            }
          }
        }
      }
    }

    private void ProcessFile(IEnumerable<DatColPickTicket> listofPickTickets)
    {
      var orders = listofPickTickets.GroupBy(x => x.OrderNumber);
      foreach (var order in orders)
      {
        var firstOrder = order.First();

        var pickTicketCode = firstOrder.OrderNumber.Substring(0, 2).ToLower();

        var isSdPickTicketCode = pickTicketCode == "sd";
        var isVoPickTicketCode = pickTicketCode == "vo";

        var isSalesOrder = isSdPickTicketCode && !isVoPickTicketCode;
        var isTransferOrder = isVoPickTicketCode && !isSdPickTicketCode;
        var isOrderCorrupt = !isSalesOrder && !isTransferOrder;

        if (isSalesOrder)
        {
          if (ProcessSalesOrder(firstOrder, order))
          {
            _log.AuditSuccess(string.Format("Sales Order '{0}' successfully inserted", firstOrder.OrderNumber));
          }
          else
          {
            _log.AuditError("Failed to create Sales Order. Please read '.log' file.");
          }
        }

        if (isTransferOrder)
        {
          if (ProcessTransferOrder(firstOrder, order))
          {
            _log.AuditSuccess(string.Format("Transfer Order '{0}' successfully inserted", firstOrder.OrderNumber));
          }
          else
          {
            _log.AuditError("Failed to create Transfer Order. Please read '.log' file.");
          }
        }

        if (isOrderCorrupt)
        {
          _failedPickTicketOrderNumber = firstOrder.OrderNumber;
          _archiveService.ExportToAxapta(order, SaveTo.CorruptPickTicketDirectory, FailedPickTicketOrderFileName);
        }
      }
    }

    private bool ProcessTransferOrder(DatColPickTicket firstOrder, IEnumerable<DatColPickTicket> orderLines)
    {
      List<DatColErrorMessage> listOfErrors;

      var listOfTransferOrderLines = orderLines as DatColPickTicket[] ?? orderLines.ToArray();

      if (_orderHelper.IsValidTransferOrder(listOfTransferOrderLines, _sapphConnectorID, vendorIDs, out listOfErrors, true))
      {
        if (SaveTransferOrder(firstOrder, listOfTransferOrderLines))
        {
          return true;
        }

        _failedPickTicketOrderNumber = firstOrder.OrderNumber;
        _archiveService.ExportToAxapta(listOfTransferOrderLines, SaveTo.CorruptPickTicketDirectory, FailedPickTicketOrderFileName);

        return false;
      }

      _failedPickTicketOrderNumber = firstOrder.OrderNumber;
      _archiveService.ExportToAxapta(listOfTransferOrderLines, SaveTo.CorruptPickTicketDirectory, FailedPickTicketOrderFileName);

      var fileName = string.Format("{0}.log", Path.GetFileNameWithoutExtension(FailedPickTicketOrderFileName));

      _archiveService.ExportToAxapta(listOfErrors, SaveTo.CorruptPickTicketDirectory, fileName);

      return false;
    }

    private bool SaveTransferOrder(DatColPickTicket firstOrder, IEnumerable<DatColPickTicket> listOfTransferOrderLines)
    {
      try
      {
        var transferOrders = listOfTransferOrderLines.GroupBy(x => x.CustomerNumber).ToList();

        var subOrderCounter = 0;
        var orderNumberParts = firstOrder.OrderNumber.Split('_');
        var listOfOrders = new List<Order>();

        foreach (var transferOrder in transferOrders)
        {
          var substituteOrderNumber = string.Format("{0}_{1:D3}", orderNumberParts[0], ++subOrderCounter);
          var firstTransferOrder = transferOrder.First();
          var orderToSave = CreateOrder(substituteOrderNumber, firstTransferOrder, transferOrder);

          orderToSave.Document = firstOrder.OrderNumber;

          listOfOrders.Add(orderToSave);
        }

        if (!_orderRepo.InsertOrders(listOfOrders))
        {
          _log.AuditError(string.Format("Failed to insert order: {0}", firstOrder.OrderNumber));
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

    private bool ProcessSalesOrder(DatColPickTicket firstOrder, IEnumerable<DatColPickTicket> orderLines)
    {
      List<DatColErrorMessage> listOfErrors;

      var listOfSalesOrderLines = orderLines as DatColPickTicket[] ?? orderLines.ToArray();

      if (_orderHelper.IsValidSalesOrder(listOfSalesOrderLines, _sapphConnectorID, vendorIDs, out listOfErrors, true))
      {
        if (SaveSalesOrder(firstOrder, listOfSalesOrderLines))
        {
          return true;
        }

        _failedPickTicketOrderNumber = firstOrder.OrderNumber;
        _archiveService.ExportToAxapta(listOfSalesOrderLines, SaveTo.CorruptPickTicketDirectory, FailedPickTicketOrderFileName);

        return false;
      }

      _failedPickTicketOrderNumber = firstOrder.OrderNumber;
      _archiveService.ExportToAxapta(listOfSalesOrderLines, SaveTo.CorruptPickTicketDirectory, FailedPickTicketOrderFileName);

      var fileName = string.Format("{0}.log", Path.GetFileNameWithoutExtension(FailedPickTicketOrderFileName));

      _archiveService.ExportToAxapta(listOfErrors, SaveTo.CorruptPickTicketDirectory, fileName);

      return false;
    }

    private bool SaveSalesOrder(DatColPickTicket order, IEnumerable<DatColPickTicket> orderLines)
    {
      try
      {
        var listOfCurrentWebOrderNumbers = _orderRepo
          .GetOrderByContainsWebOrderNumber(order.OrderNumber)
          .Select(x => x.WebSiteOrderNumber)
          .ToArray();

        string tempWebOrderNumber;
        if (!_orderHelper.ConvertOriginalOrderNumberToConcentratorOrderNumber(order.OrderNumber, listOfCurrentWebOrderNumbers, 9, out tempWebOrderNumber))
        {
          _log.AuditError(string.Format("OrderNumber {0} cannot be converted. This ordernumbers are already exist: {1}",
            order.OrderNumber,
            string.Join(", ", listOfCurrentWebOrderNumbers)));
          return false;
        }

        var orderToSave = CreateOrder(tempWebOrderNumber, order, orderLines);

        if (!_orderRepo.InsertOrder(orderToSave))
        {
          _log.AuditError(string.Format("Failed to insert order: {0}", orderToSave.WebSiteOrderNumber));
          return false;
        }
        return true;
      }
      catch (Exception)
      {
        _log.AuditError(string.Format("Database Error: Failed to save the order. OrderNumber '{0}'", order.OrderNumber));
        return false;
      }
    }

    private Order CreateOrder(string webOrderNumber, DatColPickTicket order, IEnumerable<DatColPickTicket> orderLines)
    {
      var orderToSave = new Order
      {
        OrderLines = new List<OrderLine>(),
        ConnectorID = _sapphConnectorID,
        CustomerOrderReference = order.CustomerNumber,
        HoldOrder = false,
        WebSiteOrderNumber = webOrderNumber,
        PaymentTermsCode = "SHOP",
        PaymentInstrument = string.Empty,
        ReceivedDate = DateTime.Now.ToUniversalTime(),
        OrderType = (int)OrderTypes.PickTicketOrder
      };

      var engine = new FileHelperEngine(typeof(DatColPickTicket));

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
            Quantity = int.Parse(orderLine.QuantityOnPickTicket, NumberStyles.Number, new CultureInfo("nl-NL")),
            isDispatched = false,
            OriginalLine = engine.WriteString(new List<DatColPickTicket> { orderLine }),
            PriceOverride = false
          };
        orderToSave.OrderLines.Add(orderLineToSave);
      }
      return orderToSave;
    }
  }

  public class ConfirmShipmentService : IExportPickTicketShipmentConfirmation
  {
    #region "Variables"

    private const string OrderResponseLineStatusToShip = "WaitingForShipmentToAxapta";
    private const string SettingKeyRelatedConnectorID = "RelatedConnectorID";
    private const string SapphVendorName = "Sapph";

    private int _sapphConnectorID;

    private Vendor _vendorSapph;

    private static string OrderResponseLineStatusShippedToAxapta
    {
      get
      {
        return String.Format("ShippedToAxapta on {0:dd-MM-yyyy H:mm:ss}", DateTime.Now);
      }
    }
    private static string OrderResponseLineStatusWaitingForAxaptaResponse
    {
      get
      {
        return String.Format("WaitingForAxaptaResponse. Update time '{0:dd-MM-yyyy H:mm:ss}'", DateTime.Now);
      }
    }
    private static string OrderResponseLineStatusShipmentNotRequierd
    {
      get
      {
        return String.Format("ShipmentNotRequierd. Update time '{0:dd-MM-yyyy H:mm:ss}'", DateTime.Now);
      }
    }

    private static string _orderNumber;
    private static string NotificationFileName
    {
      get
      {
        return String.Format("SHIPCONFWEB_{0:yyyy-MM-dd_Hmmss_ff}_{1}.txt", DateTime.Now, _orderNumber);
      }
    }
    private static string ErrorNotificationFileName
    {
      get
      {
        return String.Format("ErrorLog_Notification_{0}.log", _orderNumber);
      }
    }

    private readonly IAuditLogAdapter _log;
    private readonly IOrderRepository _orderRepo;
    private readonly IArchiveService _archiveService;
    private readonly IVendorRepository _vendorRepo;
    private readonly INotificationHelper _notificationHelper;

    #endregion

    public ConfirmShipmentService(
      IOrderRepository orderRepository,
      IArchiveService archiveService,
      IAuditLogAdapter log, IVendorRepository vendorRepo, INotificationHelper notificationHelper)
    {
      _orderRepo = orderRepository;
      _archiveService = archiveService;
      _log = log;
      _vendorRepo = vendorRepo;
      _notificationHelper = notificationHelper;
    }

    public void Process()
    {
      _vendorSapph = _vendorRepo
        .GetVendor(SapphVendorName);

      if (_vendorSapph == null)
      {
        _log.AuditError("Vendor Sapph does not exist. Please insert vendor Sapph.");

        return;
      }

      var vendorSettings = _vendorSapph.VendorSettings.ToDictionary(x => x.SettingKey, x => x.Value);

      if (!vendorSettings.ContainsKey(SettingKeyRelatedConnectorID))
      {
        _log.AuditError("SettingKey RelatedConnectorID for vendor Sapph does not exist. Please insert VendorSetting RelatedConnectorID for vendor Sapph.");

        return;
      }

      if (!int.TryParse(vendorSettings[SettingKeyRelatedConnectorID], NumberStyles.Integer, new CultureInfo("en-US"), out _sapphConnectorID))
      {
        _log.AuditError(string.Format("SettingKey RelatedConnectorID for vendor Sapph has not integer value '{0}'. Please change the value.", vendorSettings[SettingKeyRelatedConnectorID]));

        return;
      }

      ProcessNotifications();
    }

    private void ProcessNotifications()
    {
      var listOfNotifications = _orderRepo
        .GetListOfOrderWithNotification(_sapphConnectorID, OrderResponseLineStatusToShip)
        .ToList();

      ProcessWebOrderNotifications(listOfNotifications);
      ProcessSalesOrderNotifications(listOfNotifications);
      ProcessTransferOrderNotifications(listOfNotifications);
    }

    private void ProcessWebOrderNotifications(IEnumerable<OrderNotification> listOfNotifications)
    {
      var listOfOrderResponseLine = listOfNotifications
        .Where(x => x.OrderType == (int)OrderTypes.SalesOrder)
        .ToList();

      if (listOfOrderResponseLine.Any())
        UpdateOrderResponseLines(listOfOrderResponseLine, OrderResponseLineStatusShipmentNotRequierd);
    }

    private void ProcessSalesOrderNotifications(IEnumerable<OrderNotification> listOfNotifications)
    {
      var listOfOrders = listOfNotifications
        .Where(x => x.OrderType == (int)OrderTypes.PickTicketOrder && x.WebSiteOrderNumber.StartsWith("SDO"))
        .GroupBy(x => x.OrderID);

      foreach (var salesOrder in listOfOrders)
      {
        var orderNumber = salesOrder.First().WebSiteOrderNumber;

        _log.AuditInfo(string.Format("Start processing ShipmentNotification for OrderNumber '{0}'", orderNumber));

        ProcessSalesOrderNotification(salesOrder, orderNumber);
      }
    }

    private void ProcessTransferOrderNotifications(IEnumerable<OrderNotification> listOfNotifications)
    {
      var listOfOrders = listOfNotifications
        .Where(x => x.OrderType == (int)OrderTypes.PickTicketOrder && x.WebSiteOrderNumber.StartsWith("VO"))
        .GroupBy(x => x.Document);

      foreach (var transferOrders in listOfOrders)
      {
        ProcessTransferOrderNotification(transferOrders, transferOrders.Key);
      }
    }

    private void ProcessSalesOrderNotification(IEnumerable<OrderNotification> salesOrder, string orderNumber)
    {
      List<DatColErrorMessage> listOfErrors;
      var listOfTransferOrderLines = salesOrder as OrderNotification[] ?? salesOrder.ToArray();

      if (_notificationHelper.IsValidSalesNotification(listOfTransferOrderLines, out listOfErrors, true))
      {
        List<DatColPickTicket> listOfNotifications;
        if (CreateSalesOrderNotification(listOfTransferOrderLines, out listOfNotifications))
        {
          UploadNotification(listOfNotifications, orderNumber, SaveTo.PickingPickTicketShipmentConfirmationDirectory);

          if (UpdateOrderResponseLines(listOfTransferOrderLines, OrderResponseLineStatusShippedToAxapta))
            _log.AuditInfo(string.Format("Successfully processed ShipmentNotification. OrderNumber '{0}'", orderNumber));
        }
      }
      else
      {
        _orderNumber = orderNumber;
        var fileName = ErrorNotificationFileName;
        _archiveService.ExportToAxapta(listOfErrors, SaveTo.CorruptNotificationDirectory, fileName);
        _log.AuditInfo(string.Format("Failed to process ShipmentNotification. Please look at file '{0}' in CorruptNotification map on ftp server for error", fileName));
      }
    }

    private bool CreateSalesOrderNotification(IEnumerable<OrderNotification> salesOrder, out List<DatColPickTicket> listOfNotifications)
    {
      listOfNotifications = new List<DatColPickTicket>();
      var engine = new FileHelperEngine(typeof(DatColPickTicket));

      var orderLines = salesOrder
        .GroupBy(x => x.OrderLineID, x => x);

      foreach (var orderLine in orderLines)
      {
        DatColPickTicket originalLine = null;

        try
        {
          var originalLines = engine.ReadString(orderLine.First().OriginalLine) as DatColPickTicket[];

          if (originalLines != null) originalLine = originalLines.First();

          if (originalLine == null) return false;
        }
        catch
        {
          return false;
        }

        var shippedItems = GetShippedItems(orderLine);
        var cancelledItems = GetCancelledItems(orderLine);

        originalLine.PickedItems = shippedItems.ToString(CultureInfo.InvariantCulture);

        listOfNotifications.Add(originalLine);
      }

      return true;
    }

    private void ProcessTransferOrderNotification(IEnumerable<OrderNotification> transferOrders, string documentNumber)
    {
      var transferOrderLines = transferOrders as OrderNotification[] ?? transferOrders.ToArray();
      var currentTransferOrders = _orderRepo.GetOrders(_sapphConnectorID, OrderTypes.PickTicketOrder).Where(x => x.Document == documentNumber);

      List<DatColErrorMessage> listOfErrors;

      if (_notificationHelper.IsAllSubTransferOrdersAreShipped(transferOrderLines, currentTransferOrders, out listOfErrors, true))
      {
        if (_notificationHelper.IsValidTransferNotification(transferOrderLines, out listOfErrors, true))
        {
          List<DatColOrderTransfer> listOfNotifications;
          if (CreateTransferOrderNotification(transferOrderLines, out listOfNotifications))
          {
            UploadNotification(listOfNotifications, documentNumber, SaveTo.TransferPickTicketShipmentConfirmationDirectory);
            UpdateOrderResponseLines(transferOrderLines, OrderResponseLineStatusShippedToAxapta);
          }
        }
        else
        {
          _orderNumber = documentNumber;
          var fileName = ErrorNotificationFileName;
          _archiveService.ExportToAxapta(listOfErrors, SaveTo.CorruptNotificationDirectory, fileName);
          _log.AuditInfo(string.Format("Failed to process ShipmentNotification. Please look at file '{0}' in CorruptNotification map on ftp server for error", fileName));
        }
      }
      else
      {
        _orderNumber = documentNumber;
        var fileName = ErrorNotificationFileName;
        _archiveService.ExportToAxapta(listOfErrors, SaveTo.CorruptNotificationDirectory, fileName);
        _log.AuditInfo(string.Format("Failed to process ShipmentNotification. Please look at file '{0}' in CorruptNotification map on ftp server for error", fileName));
      }
    }

    private bool CreateTransferOrderNotification(IEnumerable<OrderNotification> listOfSalesOrderLines, out List<DatColOrderTransfer> listOfNotifications)
    {
      listOfNotifications = new List<DatColOrderTransfer>();
      var engine = new FileHelperEngine(typeof(DatColPickTicket));

      var orderLines = listOfSalesOrderLines
        .GroupBy(x => x.OrderLineID, x => x);

      foreach (var orderLine in orderLines)
      {
        DatColPickTicket originalLine = null;

        try
        {
          var originalLines = engine.ReadString(orderLine.First().OriginalLine) as DatColPickTicket[];

          if (originalLines != null) originalLine = originalLines.First();

          if (originalLine == null) return false;
        }
        catch
        {
          return false;
        }

        var shippedItems = GetShippedItems(orderLine);
        var cancelledItems = GetCancelledItems(orderLine);

        var transferOrder = new DatColOrderTransfer
          {
            FromWarehouse = originalLine.Warehouse,
            ToWarehouse = originalLine.CustomerNumber,
            AxaptaCustomerNumber = originalLine.CustomItemNumber,
            DeliveryDate = originalLine.DeliveryDate,
            PickedItems = shippedItems.ToString(CultureInfo.InvariantCulture)
          };

        listOfNotifications.Add(transferOrder);
      }
      return true;
    }

    private int GetShippedItems(IEnumerable<OrderNotification> orderLine)
    {
      var shippedItem = orderLine
        .Where(x => x.Shipped.HasValue)
        .Sum(x => x.Shipped.Value);

      return shippedItem;
    }

    private int GetCancelledItems(IEnumerable<OrderNotification> orderLine)
    {
      var cancelledItem = orderLine
        .Where(x => x.Cancelled.HasValue)
        .Sum(x => x.Cancelled.Value);

      return cancelledItem;
    }

    private void UploadNotification<T>(ICollection<T> listOfNotifications, string orderNumber, SaveTo saveTo)
    {
      if (!listOfNotifications.Any()) return;

      _orderNumber = orderNumber;
      var fileName = NotificationFileName;

      _archiveService.ExportToAxapta(listOfNotifications, saveTo, fileName);
      _archiveService.ExportToArchive(listOfNotifications, saveTo, fileName);
    }

    private bool UpdateOrderResponseLines(IEnumerable<OrderNotification> order, string htmlStatus)
    {
      var tryCounter = 0;
      var tryAgain = true;
      var successfullyUpdated = false;

      var listOfOrderResponseLineIDs = order
        .Where(x => x.OrderResponseLineID.HasValue)
        .Select(x => x.OrderResponseLineID.Value)
        .ToArray();

      try
      {
        do
        {
          if (_orderRepo.UpdateHtmlOfOrderResponseLines(listOfOrderResponseLineIDs, htmlStatus))
          {
            tryAgain = false;
            successfullyUpdated = true;
          }
          else
          {
            if (tryCounter < 6)
            {
              _log.AuditWarning(string.Format("Failed to update OrderResponseLines. OrderResponseLineIDs '{0}' Try #: {1}"
                , string.Join(", ", listOfOrderResponseLineIDs)
                , tryCounter));
              tryCounter++;
            }
            else
            {
              tryAgain = false;
            }
          }
        } while (tryAgain);
      }
      catch
      {
        _log.AuditError(string.Format("Try Catch: Failed to update OrderResponseLines. OrderResponseLineIDs '{0}'", string.Join(", ", listOfOrderResponseLineIDs)));
      }

      return successfullyUpdated;
    }
  }
}
