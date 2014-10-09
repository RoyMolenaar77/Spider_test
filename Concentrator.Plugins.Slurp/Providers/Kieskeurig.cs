using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using log4net;

namespace Concentrator.Plugins.Slurp.Providers
{
  public class Kieskeurig : ISlurpSite
  {

    ILog log = LogManager.GetLogger("Kieskeurig");

    private static CultureInfo culture = new CultureInfo("nl-NL");

    #region ISlurpSite Members

    public SlurpResult Process(string manufacturerId)
    {


      using (WebClient client = new WebClient())
      {
        SlurpResult result = new SlurpResult(manufacturerId);
        result.SiteName = this.GetType().Name;


        var searchUrl = String.Format("http://www.kieskeurig.nl/zoeken/index.html?q={0}", manufacturerId);

        string searchResults = client.DownloadString(searchUrl);

        if (searchResults.Contains("Helaas zijn er geen resultaten gevonden voor je zoekopdracht"))
          return result;

        log.InfoFormat("Processing product {0}", manufacturerId);


        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(searchResults);

        var form = doc.DocumentNode.SelectSingleNode("//form[@class=\"productList\"]").Attributes["action"].Value;

        if (form != "/winkelvergelijk")
        {

          var productSelectQuery = (from row in doc.DocumentNode.SelectNodes("//table[starts-with(@class, \"resultTable\")]/tbody/tr").Cast<HtmlNode>()
                                    select row).ToList();

          string productUrl = null;

          for (int idx = 0; idx < productSelectQuery.Count; idx++)
          {


            if (productSelectQuery[idx].InnerHtml.Contains("type=\"checkbox\""))
              continue;

            productUrl = "http://www.kieskeurig.nl/" + doc.DocumentNode.SelectSingleNode("//table[starts-with(@class, \"resultTable\")]/tbody/tr[" + (idx + 1).ToString() + "]/td[1]//a").Attributes["href"].Value;
            break;
          }

          //subselect product


          if (String.IsNullOrEmpty(productUrl))
          {
            log.InfoFormat("Product {0} cannot be found", manufacturerId);
            return result;
          }


          string productPage = client.DownloadString(productUrl);

          doc.LoadHtml(productPage);
        }

        var tableNode = doc.DocumentNode.SelectSingleNode("//table[@id=\"priceTable\"]/tbody");

        int cellOffset = 0;
        if (tableNode == null)
        {
          tableNode = doc.DocumentNode.SelectSingleNode("//table[contains(@class, \"priceTable\")]/tbody");
          cellOffset = 1;
        }


        var query = from row in tableNode.SelectNodes("tr").Cast<HtmlNode>()
                    select row;
        //from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
        //select new { Table = table.Id, CellText = cell.InnerText };

        foreach (var row in query)
        {
          var cells = row.SelectNodes("td").Cast<HtmlNode>();
          if (cellOffset == 0)
          {
            if (row.Attributes["class"] != null && (row.Attributes["class"].Value.Contains("moreOffers")
              || row.Attributes["class"].Value.Contains("otherprice")))
              continue;

          }
          else
          {
            if (!cells.Any(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("deliverYPrice")))
              continue;
          }

          var imageNodes = cells.First().SelectNodes("//img");
          var shop = "";

          HtmlNodeCollection cellNodes = row.SelectNodes("td");
          HtmlNode bannerNode = cellNodes[2];
          string javascript = bannerNode.ChildNodes[1].ChildNodes[1].Attributes["onclick"].Value;

          var firstStatement = javascript.Split(';')[0]; //split statements
          var parameters = firstStatement.Split('(',')')[1].Replace("\n", String.Empty).Replace(Environment.NewLine,String.Empty); // get parameters
          shop = parameters.Split(',')[6].Trim().Trim('\''); // get company name

          //var shop = cells.Skip(cellOffset).Take(1).First().SelectSingleNode(".//img").Attributes["alt"].Value;

          ////img").Attributes["alt"].Value;
          //&euro; 359,-

          decimal? price = null;
          decimal? total = null;

          string txt = String.Empty;
          if (cellOffset == 0)
            txt = row.SelectSingleNode("td[@class=\"deliveryPrice\"]").InnerHtml;
          else
            txt = row.SelectSingleNode("td[@class=\"deliverYPrice\"]/div[@class=\"il\"]").InnerHtml;

          total = StripPrice2(txt);

          //txt = row.SelectSingleNode("td[@class=\"totalPrice\"]").InnerHtml;

          //var total = StripPrice(txt);
          ////var price = Decimal.Parse(row.SelectSingleNode("td[@class=\"price\"]").InnerText.Split(' ')[1].Trim(new char[] { '-', ' ' }));
          ////var total = Decimal.Parse(row.SelectSingleNode("td[@class=\"totalPrice\"]").InnerText.Split(' ')[1].Trim(new char[] { '-', ' ' }));

          DeliveryStatus deliveryStatus = DeliveryStatus.Unknown;
          if (cellOffset == 0)
          {
            var deliveryNode = row.SelectSingleNode("td[@class=\"deliveryTime\"]/div");
            var delivery = deliveryNode.Attributes["title"].Value;

            var parts = deliveryNode.Attributes["class"].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var deliveryIcon = parts.FirstOrDefault(x => x != "icon");

            deliveryStatus = ParseIcon(deliveryIcon);
          }
          //var line = String.Format("Shop: {0}\t\tPrice: {1}\tTotal: {2}\tDelivery:{3}" + Environment.NewLine, shop, price, total, delivery);

          //Console.WriteLine(line);

          result.Shops.Add(new ShopResult()
          {
            Name = shop,
            Delivery = deliveryStatus,
            TotalPrice = total,
            Price = price
          });
        }


        //using (var reader = new StringReader(productPage))
        //{
        //  Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader();
        //  sgmlReader.DocType = "HTML";
        //  sgmlReader.WhitespaceHandling = WhitespaceHandling.All;
        //  sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;
        //  sgmlReader.InputStream = reader;

        //  XDocument xdoc = XDocument.Load(sgmlReader);


        //  var rows = xdoc.Root.Descendants("table");//.Where(t => t.Attribute("id").Value == "priceTable");
        //    //;.Element("tbody").Elements("tr");


        //  foreach (var row in rows)
        //  {
        //    var cells = row.Elements("td");

        //    var shop = cells.First().Element("img").Attribute("alt").Value;

        //    var price = 0m;
        //    var total = 0m;
        //    var delivery = String.Empty;

        //    //var txt = cells.First(x => x.Attribute("class").Value == "price").Value;
        //    //var price = StripPrice2(txt);

        //    //txt = cells.First(x => x.Attribute("class").Value == "totalPrice").Element("div").Element("a").Value;

        //    //var total = StripPrice2(txt);

        //    //var deliveryCell = cells.First(x => x.Attribute("class").Value == "delivery");
        //    //var img = deliveryCell.Element("img");
        //    //string delivery = String.Empty;

        //    //if (img != null)
        //    //  delivery = img.Attribute("alt").Value;
        //    //else
        //    //  delivery = deliveryCell.Element("span").Attribute("title").Value;

        //    //var line = String.Format("Shop: {0}\t\tPrice: {1}\tTotal: {2}\tDelivery:{3}" + Environment.NewLine, shop, price, total, delivery);

        //    //Console.WriteLine(line);

        //    result.Shops.Add(new ShopResult()
        //    {
        //      Name = shop,
        //      Price = price,
        //      TotalPrice = total,
        //      Delivery = delivery
        //    });

        //  }

        return result;

      }
    }

    private DeliveryStatus ParseIcon(string deliveryIcon)
    {
      switch (deliveryIcon)
      {
        case "icongreen1":
          return DeliveryStatus.NextDay;
        case "icongreen2":
          return DeliveryStatus.WithinTwoDays;
        case "iconyellow":
          return DeliveryStatus.WithinThreeDays;
        case "iconorange":
          return DeliveryStatus.WithinTwoToFiveDays;
        case "iconred":
          return DeliveryStatus.WithinOneWeek;
        case "iconblack":
        default:
          return DeliveryStatus.Unknown;
      }
    }

    #endregion

    private static decimal? StripPrice2(string txt)
    {
      if (txt.Contains("http://images.kieskeurig.nl/images/kieskeurig/no.gif"))
        return null;

      var x = Regex.Match(txt, "&euro; (.*?)[<\n]");
      //.Captures[0].Value.Replace(",", "."), culture);

      return Decimal.Parse(x.Groups[1].Value.Trim('-'), culture);
    }

  }
}
