using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Transfer.Helper;
using Concentrator.Plugins.PFA.Transfer.Model.Mutation;
using Concentrator.Plugins.PfaCommunicator.Objects.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Plugins.PFA.Objects.Helper;

namespace Concentrator.Plugins.PFA.Transfer
{
  public class WehkampTransferOrders : ConcentratorPlugin
  {
    private System.Configuration.Configuration _config;


    public WehkampTransferOrders()
    {
      _config = GetConfiguration();

    }

    public override string Name
    {
      get { return "Wehkamp Transfer Orders"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var vendors = unit.Scope.Repository<Vendor>().GetAll().ToList().Where(c => ((VendorType)c.VendorType).Has(VendorType.SupportsPFATransferOrders)).ToList();

        foreach (var vendor in vendors)
        {
          try
          {
            var messagePaths = CommunicatorService.GetMessagePath(vendor.VendorID, MessageTypes.TransferOrder);
            var messagePath = messagePaths.MessagePath;
            var logLocation = messagePaths.ProcessedPath;
            var errorLocation = messagePaths.ErrorPath;

            if (!Directory.Exists(logLocation)) Directory.CreateDirectory(logLocation);
            if (!Directory.Exists(errorLocation)) Directory.CreateDirectory(errorLocation);


            var publicationRule = vendor.ContentProducts.FirstOrDefault(c => c.IsAssortment);
            publicationRule.ThrowIfNull("Publication rule with IsAssortment is missing for vendor " + vendor.Name);

            var connectorID = publicationRule.ConnectorID;

            Directory.GetFiles(messagePath).ForEach((file, idx) =>
                        {
                          try
                          {
                            List<MutHeader> message = GenerateTransferMutationMessage(file);

                            CreateTransferOrders(file, message, connectorID, unit);
                            unit.Save();

                            Files.MoveFileToPath(logLocation, file, true);
                          }
                          catch (Exception ex)
                          {
                            log.AuditError("Something went wrong processing " + file + ". See log file.");
                            Files.LogExceptionAndMove(errorLocation, file, ex);
                          }
                        });
          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Error trying to process wehkamp transfer order because : {0}", ex.Message), ex, Name);
          }
        }
      }
    }

    public void CreateTransferOrders(string file, List<MutHeader> orderList, int connectorID, IUnitOfWork unit)
    {
      orderList.ForEach(item =>
      {
        var order = new Order
        {
          ConnectorID = connectorID,
          CustomerOrderReference = item.TransferReceipt,
          HoldOrder = false,
          ReceivedDate = DateTime.Now.ToUniversalTime(),
          Document = Path.GetFileName(file),
          OrderType = (int)OrderTypes.TransferOrder,
          WebSiteOrderNumber = item.TransferReceipt
        };

        unit.Scope.Repository<Order>().Add(order);

        item.Details.ForEach(detail =>
        {
          int productId = GetProductIdentification(detail.Barcode);

          if (productId == 0)
            throw new InvalidOperationException(string.Format("Product not found for articlecode {0} and barcode {1}", detail.ArticleCode, detail.Barcode));

          var orderLine = new OrderLine
          {
            CustomerItemNumber = string.Format("{0} {1} {2}", detail.ArticleCode, detail.ColorCode, detail.Size),
            Order = order,
            ProductID = productId,
            Quantity = detail.NumberOfSKUs,
            Price = 0,
            OrderLedgers = new List<OrderLedger>()
          };

          unit.Scope.Repository<OrderLine>().Add(orderLine);

        });
      });
    }

    public int GetProductIdentification(string incompleteBarcode)
    {
      using (var pDb = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        pDb.CommandTimeout = 5 * 60;

        string completeBarcode = BarcodeHelper.AddCheckDigitToBarcode(incompleteBarcode);

        int match = pDb.FirstOrDefault<int>("SELECT PRODUCTID FROM PRODUCTBARCODE WHERE BARCODE = @0", completeBarcode);

        return match;
      }
    }

    public List<MutHeader> GenerateTransferMutationMessage(string file)
    {
      List<MutHeader> list = new List<MutHeader>();

      MutHeader header = null;

      File.ReadAllLines(file).ForEach((line, index) =>
      {
        string recordType = line.Substring(7, 1);

        if (recordType.Equals("0"))
          header = new MutHeader(line);

        else if (recordType.Equals("5"))
          header.Details.Add(new MutDetail(line));

        else if (recordType.Equals("9"))
        {
          header.Total = new MutTotal(line);

          list.Add(header);
        }
      });

      return list;
    }
  }
}