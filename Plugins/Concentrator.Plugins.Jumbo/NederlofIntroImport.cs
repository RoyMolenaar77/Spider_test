using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Data;
using Concentrator.Objects.Enumerations;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Drawing;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Plugins.Jumbo
{
  public class NederlofIntroImport : JumboBase
  {
    public override string Name
    {
      get { return "Jumbo Nederlof product introtekst import"; }
    }

    private const int UnMappedID = -1;

    protected override void Process()
    {
      int vendorID = int.Parse(GetConfiguration().AppSettings.Settings["NederlofVendorID"].Value);
      var dir = GetConfiguration().AppSettings.Settings["NederlofIntroFiles"].Value;

      List<NederLofData> productData = new List<NederLofData>();
      //string destinationPath = Path.Combine(GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value, "Products");

      foreach (var f in Directory.GetFiles(dir))
      {
        XDocument xdoc = null;
        using (StreamReader reader = new StreamReader(f))
        {
          xdoc = XDocument.Parse(reader.ReadToEnd());
        };

        XNamespace xName = "http://www.graphit.nl/xmlns/netpublishpro/entity-info/1.4";

        //-		xdoc.Elements().Elements(xName + "object").First().Elements(xName + "field").FirstOrDefault().Element(xName + "value")	<value xmlns="http://www.graphit.nl/xmlns/netpublishpro/entity-info/1.4">15 mei 2008 9:21:21 CEST</value>	System.Xml.Linq.XElement

        var fields = xdoc.Elements().Elements(xName + "object").Elements(xName + "field").Select(x => x.Attribute("name").Value).Distinct().ToList();
        var itemFields = xdoc.Elements().Elements(xName + "object").Elements(xName + "items").Elements(xName + "item").Elements(xName + "field").Select(x => x.Attribute("name").Value).Distinct().ToList();
        var productInfoFields = xdoc.Elements().Elements(xName + "object").Elements(xName + "objects").Where(x => x.Attribute("classname").Value == "Jumbo_Product_lanquage").Elements(xName + "object").Elements(xName + "field").Select(x => x.Attribute("name").Value).Distinct().ToList();

        var products = (from pr in xdoc.Elements().Elements(xName + "object")
                        select new NederLofData
                        {
                          File = f,                          
                          VendorItemNumber = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product code") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product code").Element(xName + "value").Value : string.Empty,
                          productName = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product name") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product name").Element(xName + "value").Value : string.Empty,
                          ProductInfo = pr.Elements(xName + "objects").Where(x => x.Attribute("classname").Value == "Jumbo_introtext").Elements(xName + "object").Select(x => new NederlofProductLanguageInfo
                          {
                            IntoTekst = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Intro Text") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Intro Text").Element(xName + "value").Value : string.Empty,
                            Language = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Language") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Language").Element(xName + "value").Value : string.Empty,
                            //ProductName = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Product name") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Product name").Element(xName + "value").Value : string.Empty
                          }).ToList()
                        }).ToList();

        productData.AddRange(products);
      }


      using (IUnitOfWork unit = GetUnitOfWork())
      {
        int counter = 0;
        int total = productData.Count;
        int totalNumberOfProductsToProcess = total;
        log.InfoFormat("Start import {0} products", total);
        
        foreach (var product in productData)
        {
          try
          {
            var currLanguage = 2;

            if (counter == 100)
            {
              counter = 0;
              log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
            }
            totalNumberOfProductsToProcess--;
            counter++;

            #region Product
            string barcode = product.barcode;

            var item = unit.Scope.Repository<Product>().GetSingle(p => p.VendorAssortments.Any(y => y.CustomItemNumber == product.VendorItemNumber));

            if (item == null)
            {
              log.DebugFormat("No product found for vendoritemnumber {0} and customitemnumber {1}", barcode, product.VendorItemNumber);
              continue;
            }

            #endregion Product

            #region Descriptions
            foreach (var pInf in product.ProductInfo)
            {
              int languageID = 1;

              if (pInf.Language == "Dutch")
                languageID = 2;
              else if (pInf.Language == "French")
                languageID = 3;
              else if (pInf.Language == "International" || pInf.Language == "English")
                languageID = 1;
              else if (pInf.Language == "German")
                languageID = 4;
              else
                log.DebugFormat("Unkown language {0}", pInf.Language);

              if (item.ProductDescriptions == null) item.ProductDescriptions = new List<ProductDescription>();

              var desc = item.ProductDescriptions.FirstOrDefault(pd => pd.LanguageID == languageID && pd.VendorID == vendorID);
              if (desc == null)
              {
                desc = new ProductDescription
                {
                  VendorID = vendorID,
                  LanguageID = languageID,
                  Product = item,
                  ShortSummaryDescription = pInf.IntoTekst,
                  ProductName = product.productName
                };
                unit.Scope.Repository<ProductDescription>().Add(desc);
              }
              else
              {
                if (string.IsNullOrEmpty(desc.ShortSummaryDescription))
                  desc.ShortSummaryDescription = pInf.IntoTekst;

                if (string.IsNullOrEmpty(desc.ProductName))
                  desc.ProductName = product.productName;
              }
            }


            #endregion Descriptions

            unit.Save();
          }
          catch (Exception ex)
          {
            log.AuditError("Error JDE product Import", ex);
          }
        }

        //}
        log.AuditInfo("Products processing finished. Processed " + (total - totalNumberOfProductsToProcess) + " products");
      }
    }
  }
}


