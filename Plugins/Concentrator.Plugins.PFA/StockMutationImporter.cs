using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.PFA
{
  /// <summary>
  /// Represents the return notification importer from TNT Fashion.
  /// </summary>
  public class StockMutationImporter : TNTImporter
  {
    protected override Regex FileNameRegex
    {
      get
      {
        return new Regex(@"^stock_mutations", RegexOptions.IgnoreCase);
      }
    }

    protected override string ValidationFileName
    {
      get
      {
        return TNTFashionSection.Current.StockMutationImport != null
          ? TNTFashionSection.Current.StockMutationImport.ValidationFileName
          : null;
      }
    }

    protected override bool Process(String file, Vendor vendor)
    {
      var document = Documents[file];
      var datColStockList = new List<DatColStock>();

      foreach (var element in document.XPathSelectElements("stock_mutations/stock_mutation"))
      {
        var navigator = element.CreateNavigator();

        var sku = GetSku(element, true).Replace(" ", "");
        var product = Unit.Scope.Repository<Product>().GetSingle(item => item.VendorItemNumber.Replace(" ", "") == sku);

        if (product == null)
        {
          var message = String.Format("No product found with the vendor item number '{0}'.", sku);

          Log.AuditError(message);

          throw new Exception(message);
        }

        string vendorStockType = GetStockMutationVendorStockType();

        var vendorStock = product.VendorStocks.SingleOrDefault(stock => stock.Vendor.VendorID == vendor.VendorID && stock.VendorStockType.StockType == vendorStockType);

        if (vendorStock == null)
        {
          var message = String.Format("No vendor stock found for the product with vendor item number '{0}' for vendor '{1}' and stock type '{2}'."
            , GetSku(element, false)
            , vendor.Name
            , vendorStockType);

          Log.AuditError(message);

          throw new Exception(message);
        }

        var quantity = Convert.ToInt32(navigator.Evaluate("number(quantity/text())"));

        Log.DebugFormat("Modifying quantity on hand for SKU '{0}' by {1} units.", sku, quantity);

        vendorStock.QuantityOnHand += quantity;

        var datColStock = new DatColStock
        {
          CustomItemNumber = GetSku(element, false),
          ModelCode = (String)element.XPathEvaluate("string(article_code/text())"),
          Quantity = quantity,
          StockWarehouse = (String)element.XPathEvaluate("string(status/text())")
        };

        datColStockList.Add(datColStock);
      }
      SendToAxapta(vendor, datColStockList);

      Unit.Save();
      return true;
    }

    private static string GetStockMutationVendorStockType()
    {
      return "Webshop";
    }

    public StockMutationImporter(Vendor vendor, IUnitOfWork unit, IAuditLogAdapter logAdapter)
      : base(vendor, unit, logAdapter)
    {
    }
  }
}
