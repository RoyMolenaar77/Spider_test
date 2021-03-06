﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.BAS.WebExport.Shop
{
  [ConnectorSystem(1)]
  public class ShopImportThirdPartyProduct : ConcentratorPlugin
  {
    private XDocument products;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();

    public override string Name
    {
      get { return "MyCom Shops Import Third Party Products Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var repoOrderResponses = unit.Scope.Repository<OrderResponseLine>();
        var orderLedgerRepo = unit.Scope.Repository<OrderLedger>();
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.ShopAssortment)))
        {
          log.DebugFormat("Start Process shop Third Party import for {0}", connector.Name);
          
          DateTime start = DateTime.Now;

          try
          {
            using (AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient())
            {

            var products = repoOrderResponses.GetAll(o => o.OrderLineID.HasValue && o.OrderLine.OrderLedgers.Max(x => x.Status) == (int)OrderLineStatus.PushProduct).Distinct().ToList();

              foreach (var product in products)
              {
                try
                {
                  if (product.OrderLine.DispatchedToVendorID.HasValue)
                  {
                  var vendorAss = product.OrderLine.Product.VendorAssortments.Where(x => x.VendorID == product.OrderLine.DispatchedToVendorID.Value || (x.Vendor.ParentVendorID.HasValue && x.Vendor.ParentVendorID.Value == product.OrderLine.DispatchedToVendorID.Value)).FirstOrDefault();

                    decimal unitcost = 0;

                  if (vendorAss.VendorPrices.FirstOrDefault().CostPrice.HasValue)
                    unitcost = vendorAss.VendorPrices.FirstOrDefault().CostPrice.Value;
                  else if (vendorAss.VendorPrices.FirstOrDefault().Price.HasValue)
                    unitcost = vendorAss.VendorPrices.FirstOrDefault().Price.Value;

                    var input = soap.GetOAssortment(connector.ConnectorID, false, product.VendorItemNumber.Trim());

                    if (input != null)
                    {
                    XDocument o_Products = XDocument.Parse(input);
                      if (o_Products != null && o_Products.Root.Elements("Product").Count() > 0)
                      {
                      ImportOItems(o_Products, connector, (decimal)(product.OrderLine.Price.HasValue ? (decimal)product.OrderLine.Price.Value : product.Price), unitcost);
                      product.OrderLine.SetStatus(OrderLineStatus.ReadyToOrder, orderLedgerRepo);
                      unit.Save();
                      }
                      else
                        log.Info("Get Third Party product failed");
                    }
                    else
                    {
                      log.Info("Empty Third Party product Feed");
                    }
                  }
                }
                catch (Exception ex)
                {
                  log.Error(string.Format("Failed to insert product {0} to shop", product.VendorItemNumber.Trim()), ex);
                }

              }
            }
          }
          catch (Exception ex)
          {
            log.Fatal(ex);
          }
          
          log.DebugFormat("Finish Process shop Third Party product import for {0}", connector.Name);

        }
      }
    }

    private void ImportOItems(XDocument o_Products, Connector connector, decimal unitprice, decimal costprice)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          var ids = (from r in o_Products.Root.Elements("Product")
                     select r.Attribute("CustomProductID").Value).Distinct().ToArray();

          var productList = (from p in context.Products
                             select p).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

          foreach (string id in ids)
          {
            int prodid = 0;
            if (!int.TryParse(id, out prodid))
            {
              log.InfoFormat("Could not process product {0}", id);
              continue;
            }

            var pr = (from c in productList
                      where c.ProductID == prodid
                      select c).SingleOrDefault();

            if (pr != null && !existingProducts.ContainsKey(pr.ProductID))
              existingProducts.Add(pr.ProductID, pr);
          }

          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

          var productgroups = (from p in context.ProductGroups
                               select p).ToList();

          List<int> addedcustomOItemNumbers = new List<int>();

          foreach (var r in o_Products.Root.Elements("Product"))
          {
            Product product = null;
            string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
            string longDescription = r.Element("Content").Attribute("LongDescription").Value;

            int customproductID = -1;
            if (int.TryParse(r.Attribute("CustomProductID").Value, out customproductID))
            {

              try
              {
                if (!addedcustomOItemNumbers.Contains(customproductID))
                {
                  int? taxRateID = (from t in context.TaxRates
                                    where t.TaxRate1.HasValue
                                    && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                    select t.TaxRateID).SingleOrDefault();

                  if (!taxRateID.HasValue)
                    taxRateID = 1;


                  int? productgroupid = (from p in productgroups
                                         where p.ProductGroupCode == "n.v.t."
                                         select p.ProductGroupID).FirstOrDefault();


                  int brandID = 0;

                  if (!brands.ContainsKey(r.Attribute("BrandCode").Value.Trim()))
                  {
                    Brand br = new Brand
                    {
                      BrandCode = r.Attribute("BrandCode").Value.Trim(),
                      BrandName = r.Attribute("BrandCode").Value.Trim(),
                      CreatedBy = 0,
                      CreationTime = DateTime.Now,
                      LastModifiedBy = 0,
                      LastModificationTime = DateTime.Now
                    };
                    context.Brands.InsertOnSubmit(br);
                    context.SubmitChanges();
                    brandID = br.BrandID;
                    brands = (from b in context.Brands
                              select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

                  }
                  else
                  {
                    brandID = brands[r.Attribute("BrandCode").Value.Trim()].BrandID;
                  }

                  if (connector.ConcatenateBrandName && brandID > 0)
                  {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(brands[r.Attribute("BrandCode").Value.Trim()].BrandName);
                    sb.Append(" ");
                    sb.Append(shortDescription);
                    shortDescription = sb.ToString();
                  }

                  if (productgroupid.Value == 0)
                  {
                    //int? parentProductGroupID = (from p in productgroups
                    //                             where p.BackendProductGroupCode == r.Attribute("ProductGroupID").Value
                    //                             select p.ParentProductGroupID).FirstOrDefault();

                    //if (!parentProductGroupID.HasValue && parentProductGroupID.Value > 0)
                    //{
                    int? parentProductGroupID = (from p in productgroups
                                                 where p.ProductGroupName == "Missing category"
                                                 select p.ProductGroupID).FirstOrDefault();

                    if (parentProductGroupID.Value == 0)
                    {
                      ProductGroup ppg = new ProductGroup
                      {
                        ProductGroupCode = "n.v.t.",
                        BackendProductGroupCode = "n.v.t.",
                        ProductGroupName = "Missing category",
                        CreationTime = DateTime.Now,
                        LastModificationTime = DateTime.Now
                      };
                      context.ProductGroups.InsertOnSubmit(ppg);
                      context.SubmitChanges();
                      parentProductGroupID = ppg.ProductGroupID;
                      productgroups = (from p in context.ProductGroups
                                       select p).ToList();
                    }
                    //}

                    ProductGroup pg = new ProductGroup
                    {
                      ProductGroupCode = r.Attribute("ProductGroupID").Value,
                      BackendProductGroupCode = r.Attribute("ProductSubGroup").Value,
                      ParentProductGroupID = parentProductGroupID.Value,
                      ProductGroupName = r.Attribute("ProductGroupName").Value,
                      CreationTime = DateTime.Now,
                      LastModificationTime = DateTime.Now
                    };
                    context.ProductGroups.InsertOnSubmit(pg);
                    context.SubmitChanges();
                    productgroupid = pg.ProductGroupID;
                    productgroups = (from p in context.ProductGroups
                                     select p).ToList();
                  }

                  bool extended = false;
                  bool.TryParse(r.Element("ShopInformation").Element("ExtendedCatalog").Value, out extended);

                  if (existingProducts.ContainsKey(customproductID))
                  {
                    continue;
                    // product = existingProducts[int.Parse(r.Attribute("CustomProductID").Value)];
                    //if (!product.IsVisible)
                    //{

                    //if (
                    //  existingProducts[customproductID].ShortDescription.Trim() != shortDescription.Trim()
                    //  || existingProducts[customproductID].LongDescription.Trim() != longDescription.Trim()
                    //  || existingProducts[customproductID].UnitPrice != decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture)
                    //  || existingProducts[customproductID].ProductStatus != r.Attribute("CommercialStatus").Value
                    //  || existingProducts[customproductID].ManufacturerID != r.Attribute("ManufacturerID").Value
                    //  || existingProducts[customproductID].IsVisible != false
                    //  || existingProducts[customproductID].BrandID != brandID
                    //  || existingProducts[customproductID].TaxRateID != taxRateID.Value
                    //  || existingProducts[customproductID].LineType != r.Attribute("LineType").Value.Trim()
                    //  //|| existingProducts[customproductID].ProductGroupID != productgroupid.Value
                    //  || existingProducts[customproductID].LedgerClass != r.Element("ShopInformation").Element("LedgerClass").Value.Trim()
                    //  || existingProducts[customproductID].ProductDesk != r.Element("ShopInformation").Element("ProductDesk").Value
                    //  || existingProducts[customproductID].ExtendedCatalog != extended
                    //  || existingProducts[customproductID].UnitCost != decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture))
                    //{
                    //  product.ShortDescription = shortDescription.Trim();
                    //  product.LongDescription = longDescription.Trim();
                    //  product.BrandID = brandID;
                    //  product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                    //  product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                    //  product.ProductStatus = r.Attribute("CommercialStatus").Value;
                    //  product.LastModificationTime = DateTime.Now;
                    //  product.IsVisible = false;
                    //  product.TaxRateID = taxRateID.Value;
                    //  product.LineType = r.Attribute("LineType").Value.Trim();
                    //  //product.ProductGroupID = productgroupid.Value;
                    //  product.LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim();
                    //  product.ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value;
                    //  product.ExtendedCatalog = extended;
                    //  product.UnitCost = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);

                    //  log.InfoFormat("updateproduct {0}", customproductID);
                    // }
                    //}
                  }
                  else
                  {
                    product = new Product
                    {
                      ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                      ProductGroupID = productgroupid.Value,
                      ShortDescription = shortDescription.Trim(),
                      LongDescription = longDescription.Trim(),
                      ManufacturerID = r.Attribute("ManufacturerID").Value,
                      BrandID = brandID,
                      TaxRateID = taxRateID.Value,
                      UnitPrice = unitprice,
                      UnitCost = costprice,
                      IsCustom = false,
                      LineType = r.Attribute("LineType").Value.Trim(),
                      ProductStatus = r.Attribute("CommercialStatus").Value,
                      IsVisible = false,
                      CreationTime = DateTime.Now,
                      LastModificationTime = DateTime.Now,
                      LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim(),
                      ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value,
                      ExtendedCatalog = extended

                    };

                    context.Products.InsertOnSubmit(product);
                    log.InfoFormat("addedproduct {0}", customproductID);
                    //}
                    //else
                    //{
                    //  if (!string.IsNullOrEmpty(r.Attribute("BrandCode").Value))
                    //  {
                    //    log.DebugFormat("Insert product failed, brand {0} not availible", r.Attribute("BrandCode").Value.Trim());
                    //  }
                    //  else
                    //    log.Debug("Insert product failed");
                    //}
                    try
                    {
                      foreach (var bar in r.Elements("Barcodes").Elements("Barcode"))
                      {
                        if (!string.IsNullOrEmpty(bar.Value))
                        {
                          string barcodecode = bar.Value.Trim();
                          string barcodetype;
                          switch (barcodecode.Length)
                          {
                            case 8:
                              barcodetype = "Ean8";
                              break;
                            case 12:
                              barcodetype = "UpcA";
                              break;
                            case 13:
                              barcodetype = "Ean13";
                              break;
                            default:
                              barcodetype = "Ean13";
                              break;
                          }

                          if (!context.ProductBarcodes.Any(x => x.Barcode == barcodecode && x.BarcodeType == barcodetype))
                          {
                            ProductBarcode barcode = ShopUtility.InsertBarcode(int.Parse(r.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                            context.ProductBarcodes.InsertOnSubmit(barcode);
                            context.SubmitChanges();
                          }
                        }
                      }
                    }
                    catch (Exception ex)
                    {
                      log.Error("Error insert Third party Barcode for product " + r.Attribute("CustomProductID").Value, ex);
                    }

                  }
                  addedcustomOItemNumbers.Add(customproductID);

                  context.SubmitChanges();
                }
              }
              catch (Exception ex)
              {
                log.Error("Error processing product" + customproductID, ex);
                throw ex;
              }
            }
            else
            {
              log.InfoFormat("Import failed for product {0}", r.Attribute("CustomProductID").Value);
            }
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import O products", ex);
        throw ex;
      }
    }

    private void ImportOBarcodes(XDocument o_Products, Connector connector)
    {
      using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
      {
        var ids = (from r in o_Products.Root.Elements("Product")
                   where !String.IsNullOrEmpty(r.Attribute("CustomProductID").Value)
                   select r.Attribute("CustomProductID").Value).Distinct().ToArray();

        var barcodes = (from b in context.ProductBarcodes
                        select b).ToList();

        Dictionary<int, ProductBarcode> existingBarcode = new Dictionary<int, ProductBarcode>();

        for (int i = barcodes.Count - 1; i >= 0; i--)
        {
          ProductBarcode bar = barcodes[i];

          var pr = (from p in o_Products.Root.Elements("Product")
                    where p.Attribute("CustomProductID").Value == bar.ProductID.ToString()
&& p.Elements("Barcodes").Where(x => x.Element("Barcode").Value.Trim() == bar.Barcode).Count() > 0
                    select p).FirstOrDefault();

          if (pr == null)
          {
            log.InfoFormat("Delete productbarcode item {0} ({1})", bar.ProductID, bar.Barcode);

            context.ProductBarcodes.DeleteOnSubmit(bar);
            context.SubmitChanges();
          }
        }

        barcodes = (from b in context.ProductBarcodes
                    select b).ToList();

        foreach (string id in ids)
        {
          int prodid = 0;
          if (!int.TryParse(id, out prodid))
          {
            log.InfoFormat("Could not process product {0}", id);
            continue;
          }

          var stk = (from c in barcodes
                     where c.ProductID == prodid
                     select c).FirstOrDefault();

          if (stk != null && !existingBarcode.ContainsKey(stk.ProductID))
            existingBarcode.Add(stk.ProductID, stk);
        }

        List<ProductBarcode> barcodeList = new List<ProductBarcode>();

        foreach (var prod in o_Products.Root.Elements("Product"))
        {
          try
          {
            int prodid = 0;
            if (!int.TryParse(prod.Attribute("CustomProductID").Value, out prodid))
            {
              log.InfoFormat("Could not process product {0}", prod.Attribute("CustomProductID").Value);
              continue;
            }

            foreach (var bar in prod.Elements("Barcodes"))
            {
              if (bar.Element("Barcode") != null && !string.IsNullOrEmpty(bar.Element("Barcode").Value))
              {
                string barcodecode = bar.Element("Barcode").Value.Trim();
                string barcodetype;
                switch (barcodecode.Length)
                {
                  case 8:
                    barcodetype = "Ean8";
                    break;
                  case 12:
                    barcodetype = "UpcA";
                    break;
                  case 13:
                    barcodetype = "Ean13";
                    break;
                  default:
                    barcodetype = "Ean13";
                    break;
                }

                ProductBarcode barcode = null;
                if (existingBarcode.ContainsKey(int.Parse(prod.Attribute("CustomProductID").Value)))
                {
                  int custom = int.Parse(prod.Attribute("CustomProductID").Value);
                  if (existingBarcode[custom].Barcode.Trim() != barcodecode
                    || existingBarcode[custom].BarcodeType.Trim() != barcodetype)
                  {
                    barcode = existingBarcode[custom];
                    context.ProductBarcodes.DeleteOnSubmit(barcode);
                    context.SubmitChanges();

                    if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                      && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1)
                    {
                      barcode = ShopUtility.InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                      context.ProductBarcodes.InsertOnSubmit(barcode);
                      barcodeList.Add(barcode);
                    }
                  }
                }
                else
                {
                  if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                    && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1)
                  {
                    barcode = ShopUtility.InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                    context.ProductBarcodes.InsertOnSubmit(barcode);
                    context.SubmitChanges();
                    barcodeList.Add(barcode);
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            log.Error("Error import barcode", ex);
          }
        }
        context.SubmitChanges();

      }
    }


  }
}

