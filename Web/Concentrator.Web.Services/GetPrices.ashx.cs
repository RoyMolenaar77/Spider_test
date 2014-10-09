using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Web.Services;
using Concentrator.Objects;
using Concentrator.Web.Services.Vendors.BAS.WebService;
using Concentrator.Objects.Logic;
using Concentrator.Web.Services.Base;
using System.Globalization;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for $codebehindclassname$
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  public class GetPrices : BaseHandler
  {

    public override void ProcessRequest(HttpContext context)
    {
      context.Response.ContentType = "text/plain";

      //DataSet ds = new DataSet();
      int connectorID = 0;
      int customerID = 0;
      string password = "8W8u6X2A";

      if (string.IsNullOrEmpty(context.Request.Params["CustomerID"]))
      {
        context.Response.Write("Invalid Customer Number");
        return;
      }
      if (!int.TryParse(context.Request.Params["CustomerID"], out customerID) && customerID < 1)
      {
        context.Response.Write("Invalid Customer Number");
        return;
      }

      if (string.IsNullOrEmpty(context.Request.Params["Password"]))
      {
        context.Response.Write("Invalid Password");
        return;
      }
      if (context.Request.Params["Password"] != password)
      {
        context.Response.Write("Invalid Password");
        return;
      }

      if (string.IsNullOrEmpty(context.Request.Params["ConnectorID"]))
      {
        context.Response.Write("Invalid ConnectorID");
        return;
      }
      if (!int.TryParse(context.Request.Params["ConnectorID"], out connectorID) && connectorID < 1)
      {
        context.Response.Write("Invalid ConnectorID");
        return;
      }

      using (var unit = GetUnitOfWork())
      {
        Connector con = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

        var preferredVendor = con.PreferredConnectorVendors.FirstOrDefault(c => c.isPreferred).Vendor;

        JdeAssortmentSoapClient soap = new JdeAssortmentSoapClient();
        var prices = soap.GenerateFullProductList(
          customerID,//int.Parse(preferredVendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(preferredVendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(preferredVendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(preferredVendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          false
          );

        string separator = ";";

        //int languageid = EntityExtensions.GetValueByKey(con.ConnectorSettings, "LanguageID", 2);
        var funcRepo = ((IFunctionScope)unit.Scope).Repository();
        var records = funcRepo.GetAssortmentContentView(connectorID).Where(x => x.BrandID > 0);
        var language = con.ConnectorLanguages.FirstOrDefault();
        language.ThrowArgNull("No Language specified for connector");
        var languageid = language.LanguageID;


        var assortment = (from a in records.ToList()
                          join advancedPrice in prices.Tables[0].AsEnumerable() on a.VendorItemNumber equals advancedPrice.Field<string>("VendorItemNumber").Trim().ToUpper()
                          //barcodes.ContainsKey(a.ProductID) ? barcodes[a.ProductID] : new List<string>()
                          select new
                          {
                            ManufacturerID = a.VendorItemNumber,
                            ConcentratorProductID = a.ProductID,
                            PriceExVat = advancedPrice.Field<decimal>("UnitPrice").ToString().Replace(',','.'),
                            VatRate = decimal.Parse(advancedPrice.Field<double>("TaxRate").ToString(), CultureInfo.InvariantCulture),
                            MinimumQuantity = advancedPrice.Field<int>("MinimumQuantity") > 0 ? advancedPrice.Field<int>("MinimumQuantity") : 0,
                            Sbo = 0,//advancedPrice.Field<string>("SBO"),
                            CustomerQuantityOnHand = advancedPrice.Field<double>("QuantityOnHand")
                          });


        foreach (var row in assortment)
        {
          var line = String.Format("{0};{1};{2};{3};{4};{5};{6}\r\n", row.ManufacturerID, row.ConcentratorProductID, row.PriceExVat, row.VatRate, row.MinimumQuantity,row.Sbo,row.CustomerQuantityOnHand);
          context.Response.Write(line);
        }

      }
    }


    public override bool IsReusable
    {
      get
      {
        return false;
      }
    }
  }
}
