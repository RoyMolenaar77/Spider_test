using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Ordering.Purchase;
using Concentrator.Objects.Models;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Vendors.PFA.FileFormats;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects;
using System.IO;
using FileHelpers;
using log4net.Core;
using log4net;
using System.Xml.Linq;

namespace Concentrator.Objects.Ordering.Purchase
{
	public class PfaCommunication : IPurchase
	{
		#region IPurchase Members

		public bool PurchaseOrders(Concentrator.Objects.Models.Orders.Order order, List<Concentrator.Objects.Models.Orders.OrderLine> orderLines, Concentrator.Objects.Models.Vendors.Vendor administrativeVendor, Concentrator.Objects.Models.Vendors.Vendor vendor, bool directShipment, IUnitOfWork unit, ILog logger)
		{
			var filiaal890StockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "Filiaal890");
			if (filiaal890StockType == null)
				throw new Exception("Stocklocation Filiaal890 does not exists, skip order process");

			var cmStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "CM");
			if (cmStockType == null)
				throw new Exception("Stocklocation CM does not exists, skip order process");

			var transferStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "Transfer");
			if (transferStockType == null)
				throw new Exception("Stocklocation Transfer does not exists, skip order process");

			var webShopStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "Webshop");
			if (webShopStockType == null)
				throw new Exception("Stocklocation Webshop does not exists, skip order process");

			var fileLocation = vendor.VendorSettings.GetValueByKey("casmutLocation", string.Empty);
			if (string.IsNullOrEmpty(fileLocation))
				throw new Exception("No camutlocation vendorsetting for vendor" + vendor.VendorID);

			#region File Network Location
			NetworkExportUtility util = new NetworkExportUtility();

			var userName = vendor.VendorSettings.GetValueByKey("CasMutLocationUserName", string.Empty);
			if (string.IsNullOrEmpty(userName))
				throw new Exception("No Casmutlocation UserName");

			var password = vendor.VendorSettings.GetValueByKey("CasMutLocationPassword", string.Empty);
			if (string.IsNullOrEmpty(password))
				throw new Exception("No Casmutlocation Password");

#if DEBUG
			fileLocation = @"D:\tmp\CC";
#else
			fileLocation = util.ConnectorNetworkPath(fileLocation, "Y:", userName, password);
#endif
			#endregion

			var triggerFilePath = Path.Combine(fileLocation, "kasmut.oke");



			if (File.Exists(triggerFilePath))
				try
				{
					File.Delete(triggerFilePath);
				}
				catch (Exception) { } //in case in use by PFA process

			using (var engine = new MultiRecordEngine(typeof(Casmut), typeof(DatColNormalSales)))
			{

				var file = Path.Combine(fileLocation, "kasmut");

				if (!File.Exists(file))
				{
					File.Create(file).Dispose();
				}
				engine.BeginAppendToFile(file);

				orderLines.Where(c => (!c.Product.IsNonAssortmentItem.HasValue || (c.Product.IsNonAssortmentItem.HasValue && !c.Product.IsNonAssortmentItem.Value))).ToList().ForEach(line =>
			 {
				 var filiaal890Stock = line.Product.VendorStocks.FirstOrDefault(x => x.VendorStockTypeID == filiaal890StockType.VendorStockTypeID && x.VendorID == 1);
				 if (filiaal890Stock == null)
					 throw new Exception(string.Format("Stocklocation Filiaal890 does not exists for product {0}, skip order process", line.ProductID));

				 var cmStock = line.Product.VendorStocks.FirstOrDefault(x => x.VendorStockTypeID == cmStockType.VendorStockTypeID && x.VendorID == 1);
				 if (cmStock == null)
					 throw new Exception(string.Format("Stocklocation CM does not exists for product {0}, skip order process", line.ProductID));

				 var transferStock = line.Product.VendorStocks.FirstOrDefault(x => x.VendorStockTypeID == transferStockType.VendorStockTypeID && x.VendorID == 1);
				 if (transferStock == null)
					 throw new Exception(string.Format("Stocklocation Transfer does not exists for product {0}, skip order process", line.ProductID));

				 var webshopStock = line.Product.VendorStocks.FirstOrDefault(x => x.VendorStockTypeID == webShopStockType.VendorStockTypeID && x.VendorID == 1);
				 if (webshopStock == null)
					 throw new Exception(string.Format("Stocklocation Webshop does not exists for product {0}, skip order process", line.ProductID));
				 try
				 {
					 logger.Info("For order " + order.WebSiteOrderNumber + " and orderline  " + line.OrderLineID);
					 logger.Info("Transfer quantity is " + transferStock.QuantityOnHand);
					 logger.Info("CM quantity is " + cmStock.QuantityOnHand);
					 logger.Info("WMS quantity is " + filiaal890Stock.QuantityOnHand);
					 logger.Info("Desired qty is " + line.Quantity);
				 }
				 catch (Exception)
				 {

				 }
				 int transferQuantity = (filiaal890Stock.QuantityOnHand + transferStock.QuantityOnHand) - line.Quantity;

				 if (transferQuantity < 0)
				 {
					 int transferPrePickQuanity = cmStock.QuantityOnHand - Math.Abs(transferQuantity);
					 int quantityToDispatch = Math.Abs(transferQuantity);

					 if (transferPrePickQuanity < 0)
					 {
						 quantityToDispatch = Math.Abs(transferQuantity) - Math.Abs(transferPrePickQuanity);
					 }


					 if (quantityToDispatch < Math.Abs(transferQuantity))
					 {
						 int quantityCancelled = Math.Abs(transferQuantity) - quantityToDispatch;

						 line.SetStatus(OrderLineStatus.Cancelled, unit.Scope.Repository<OrderLedger>(), quantityCancelled);

						 CancelLine(line, unit, quantityCancelled, "Out of stock, no CM stock availible");
					 }

					 if (quantityToDispatch > 0)
					 {
						 var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);

						 Casmut casmut = new Casmut()
						 {
							 EAN = barcode != null ? barcode.Barcode : string.Empty,
							 PickTicketNumber = line.OrderLineID,
							 PosNumber = 100,
							 TicketNumber = line.OrderLineID,
							 SalesDate = DateTime.Now,
							 ShopNumber = 890,
							 SKUCount = quantityToDispatch
						 };

						 line.SetStatus(OrderLineStatus.ProcessedKasmut, unit.Scope.Repository<OrderLedger>(), quantityToDispatch);

             // If complete quantity is send 
             if (quantityToDispatch == line.Quantity)
             {
               line.SetStatus(OrderLineStatus.ProcessedExportNotification, unit.Scope.Repository<OrderLedger>());
             }

						 casmut.SalesValue = (int)Math.Round((double)(line.UnitPrice.HasValue ? (int)Math.Round((((line.UnitPrice.Value - (line.LineDiscount.HasValue ? line.LineDiscount.Value : 0)) * quantityToDispatch) * 100)) : 0));

						 engine.WriteNext(casmut);
#if DEBUG
						 engine.Flush();
#endif
						 cmStock.QuantityOnHand = cmStock.QuantityOnHand - quantityToDispatch;
						 transferStock.QuantityOnHand = transferStock.QuantityOnHand + quantityToDispatch;

						 unit.Save();
					 }
				 }

				 filiaal890Stock.QuantityOnHand = filiaal890Stock.QuantityOnHand - line.GetDispatchQuantity();

				 webshopStock.QuantityOnHand = cmStock.QuantityOnHand + transferStock.QuantityOnHand + filiaal890Stock.QuantityOnHand;

				 line.SetStatus(OrderLineStatus.ReadyToOrder, unit.Scope.Repository<OrderLedger>());
				 try
				 {
					 logger.Info("After processing for order " + order.WebSiteOrderNumber + " and orderline  " + line.OrderLineID);
					 logger.Info("Transfer quantity is " + transferStock.QuantityOnHand);
					 logger.Info("CM quantity is " + cmStock.QuantityOnHand);
					 logger.Info("WMS quantity is " + filiaal890Stock.QuantityOnHand);
				 }
				 catch (Exception e)
				 {
					 logger.Debug(e);
				 }
			 });

				engine.Flush();

#if !DEBUG
				util.DisconnectNetworkPath(fileLocation);
#endif
			}

			if (!File.Exists(triggerFilePath))
			{
				try
				{
					File.Create(triggerFilePath);
				}
				catch (Exception) { }

			}
			unit.Save();
			return false;
		}

		private void CancelLine(OrderLine line, IUnitOfWork unit, int quantity, string reason)
		{
			string vendorDocument = string.Empty;

			try
			{
				var document = new XDocument(new XElement("Statuses",
								new XElement("Status",
								new XElement("OrderID", line.OrderID),
								new XElement("LineID", line.OrderLineID),
								new XElement("ProductID", line.Product.VendorItemNumber),
								new XElement("Quantity", quantity),
								new XElement("Status", "Aborted"))));

				vendorDocument = document.ToString();
			}
			catch (Exception)
			{
				vendorDocument = reason;
			}

			OrderResponse orderResponse = new OrderResponse()
			{
				OrderID = line.OrderID,
				Currency = "EUR",
				ReceiveDate = DateTime.Now.ToUniversalTime(),
				ReqDeliveryDate = DateTime.Now.ToUniversalTime(),
				ResponseType = OrderResponseTypes.CancelNotification.ToString(),
				VendorID = 1,
				VendorDocumentNumber = "Concentrator",
				VendorDocumentDate = DateTime.Now.ToUniversalTime(),
				VendorDocument = vendorDocument,
				DocumentName = string.Empty
			};
			unit.Scope.Repository<OrderResponse>().Add(orderResponse);

			OrderResponseLine orderResponseLine = new OrderResponseLine()
			{
				Backordered = 0,
				Cancelled = quantity,
				Ordered = line.Quantity,
				Shipped = 0,
				Delivered = 0,
				VendorItemNumber = line.Product.VendorItemNumber,
				VendorLineNumber = "0",
				Price = line.Price.HasValue ? decimal.Parse(line.Price.Value.ToString()) : 0,
				Processed = false,
				Remark = reason,
				OrderLineID = line.OrderLineID,
				OrderResponse = orderResponse
			};

			unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

			unit.Save();
		}

		public void PurchaseConfirmation(Concentrator.Web.Objects.EDI.Purchase.PurchaseConfirmation purchaseConfirmation, Concentrator.Objects.Models.Vendors.Vendor administrativeVendor, List<Concentrator.Objects.Models.Orders.OrderLine> orderLines)
		{
			return;
		}

		public void InvoiceMessage(Concentrator.Web.Objects.EDI.InvoiceMessage invoiceMessage, Concentrator.Objects.Models.Vendors.Vendor administrativeVendor, List<Concentrator.Objects.Models.Orders.OrderLine> orderLines)
		{
			return;
		}

		public void OrderChange(Concentrator.Web.Objects.EDI.ChangeOrder.ChangeOrderRequest changeOrderRequest, Concentrator.Objects.Models.Vendors.Vendor administrativeVendor, List<Concentrator.Objects.Models.Orders.OrderLine> orderLines)
		{
			return;
		}

		#endregion
	}
}
