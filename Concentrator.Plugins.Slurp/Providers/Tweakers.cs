using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using log4net;

namespace Concentrator.Plugins.Slurp.Providers
{
  public class Tweakers : ISlurpSite
  {
    ILog log = LogManager.GetLogger("Tweakers");
    private static CultureInfo culture = new CultureInfo("en-US");

    #region ISlurpSite Members

    public SlurpResult Process(string manufacturerId)
    {
      using (WebClient client = new WebClient())
      {
        SlurpResult result = new SlurpResult(manufacturerId);
        result.SiteName = this.GetType().Name;

        var tweakerUrl = String.Format("http://tweakers.net/pricewatch/zoeken/?keyword={0}", manufacturerId);

        string searchResults = client.DownloadString(tweakerUrl);

        if (searchResults.Contains("Er werden geen producten gevonden."))
          return result;

        log.InfoFormat("Processing product {0}", manufacturerId);

        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(searchResults);

        var productUrl = doc.DocumentNode.SelectSingleNode("//table[@class=\"priceTable\"]/tbody/tr[1]/td[2]/p/a").Attributes["href"].Value;

        string productPage = client.DownloadString(productUrl);

        if (productPage.Contains("Van dit product worden geen prijzen meer getoond.") || productPage.Contains("Geen actuele prijzen bekend."))
        {
          result.ProductStatus = ProductStatus.Obsolete;
          return result;
        }

        using (var reader = new StringReader(productPage))
        {
          XDocument xdoc = new XDocument();
          using (Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader())
          {
            sgmlReader.DocType = "HTML";
            sgmlReader.WhitespaceHandling = WhitespaceHandling.All;
            sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
            sgmlReader.InputStream = reader;

            xdoc = XDocument.Load(sgmlReader);
          }

          var rows = xdoc.Root.Descendants("table").Where(t => t.Attribute("class").Value == "priceTable").First().Element("tbody").Elements("tr");


          foreach (var row in rows)
          {



            var cells = row.Elements("td");
            if (cells.First().Attribute("colspan") != null && Int32.Parse(cells.First().Attribute("colspan").Value) > 1)
              continue;

            var shop = cells.First().Value;
            string txt;
            var priceCell = cells.FirstOrDefault(x => x.Attribute("class") != null && x.Attribute("class").Value == "price");

            decimal? price = null;
            if (priceCell != null)
              price = StripPrice2(priceCell.Value);


            var totalPriceCell = cells.FirstOrDefault(x => x.Attribute("class") != null && x.Attribute("class").Value == "totalPrice");
            if (totalPriceCell != null && totalPriceCell.Element("div") != null)
              txt = totalPriceCell.Element("div").Element("a").Value;
            else
              txt = null;


            var total = StripPrice2(txt);

            var deliveryCell = cells.First(x => x.Attribute("class") != null && x.Attribute("class").Value == "delivery");
            var img = deliveryCell.Element("img");
            string delivery = String.Empty;

            if (img != null)
              delivery = img.Attribute("src").Value;
            else
              delivery = String.Empty;
            //delivery = deliveryCell.Element("span").Attribute("title").Value;

            var deliveryStatus = ParseIcon(delivery);

            result.Shops.Add(new ShopResult()
            {
              Name = shop,
              Price = price,
              TotalPrice = total,
              Delivery = deliveryStatus
            });

          }




        }

        return result;
      }
    }

    private DeliveryStatus ParseIcon(string deliveryIcon)
    {

      deliveryIcon = deliveryIcon.Trim();

      DeliveryStatus status = DeliveryStatus.Unknown;

      if (deliveryIcon.EndsWith("lkw0.png"))
        status = DeliveryStatus.NextDay;

      if (deliveryIcon.EndsWith("lkw1.png"))
        status = DeliveryStatus.WithinThreeDays;

      if (deliveryIcon.EndsWith("lkw2.png"))
        status = DeliveryStatus.WithinTwoToFiveDays;

      if (deliveryIcon.EndsWith("lkw3.png"))
        status = DeliveryStatus.WithinTwoWeeks;

      if (deliveryIcon.EndsWith("lkw5.png"))
        status = DeliveryStatus.LongerAsTwoWeeks;


      if (deliveryIcon.EndsWith("unknown.gif"))
        status = DeliveryStatus.Unknown;

      return status;

    }


    #endregion


    private decimal? StripPrice2(string txt)
    {
      if (string.IsNullOrEmpty(txt))
        return null;

      txt = txt.Replace(",-", ",00");

      return Decimal.Parse(txt, NumberStyles.Any);
      //return Decimal.Parse(txt.Trim(' ', '€', '-').Replace(",", "."), culture);
    }
  }
}
