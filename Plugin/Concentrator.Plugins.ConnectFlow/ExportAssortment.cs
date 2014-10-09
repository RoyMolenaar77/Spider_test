using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using Concentrator.Objects.ConcentratorService;
using SAPiDocConnector.Helpers;
using SAPiDocConnector;
using Concentrator.Objects;
using Concentrator.Objects.Models.Connectors;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Globalization;
using System.IO;
using System.Data.SqlClient;
using Concentrator.Objects.Models.Vendors;
using System.Configuration;

namespace Concentrator.Plugins.ConnectFlow
{
  [ConnectorSystem(4)]
  public class ExportAssortment : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "ConnectFlow assortment Export"; }
    }

    protected override void Process()
    {
      List<int> ActiveConcentratorProducts = new List<int>();

      var path = Path.Combine(GetConfiguration().AppSettings.Settings["ExportPath"].Value);

      bool networkDrive = false;
      bool.TryParse(GetConfiguration().AppSettings.Settings["IsNetworkDrive"].Value, out networkDrive);

      if (networkDrive)
      {
        string userName = GetConfiguration().AppSettings.Try(x => x.Settings["NetworkUserName"].Value, string.Empty);
        string passWord = GetConfiguration().AppSettings.Try(x => x.Settings["NetworkPassword"].Value, string.Empty);

        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          oNetDrive.LocalDrive = "H:";
          oNetDrive.ShareName = path;

          //oNetDrive.MapDrive("diract", "D1r@ct379");
          if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
            oNetDrive.MapDrive(userName, passWord);
          else
            oNetDrive.MapDrive();

          path = "H:";
        }
        catch (Exception err)
        {
          log.Error("Invalid network drive", err);
        }
        oNetDrive = null;
      }


      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);


      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.ShopAssortment)))
      {
        using (var unit = GetUnitOfWork())
        {
          log.DebugFormat("Start Process connectflow export for {0}", connector.Name);

          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();


          var products = new XDocument(soap.GetAdvancedPricingAssortment(connector.ConnectorID, false, true, null, null, true, 2, false));

          //Dictionary<string, string> productTranslation = ConnectFlowUtility.GetCrossProducts(products);
          Dictionary<string, int> insurances = ConnectFlowUtility.GetInsurances(products);

          try
          {
          //  MultiRecordEngine engine = new MultiRecordEngine(RecordTypeCache.AllTypes);
          //  engine.RecordSelector = new RecordTypeSelector(FileFormatSelector.RecordSelector);

            string senderPort = connector.ConnectorSettings.GetValueByKey<string>("SenderPort", string.Empty);
            if (string.IsNullOrEmpty(senderPort))
              throw new Exception(string.Format("No SenderPort set for connector {0} in settings", connector.Name));

            string partnerNumber = connector.ConnectorSettings.GetValueByKey<string>("PartnerNumber", string.Empty);
            if (string.IsNullOrEmpty(partnerNumber))
              throw new Exception(string.Format("No PartnerNumber set for connector {0} in settings", connector.Name));

            string client = connector.ConnectorSettings.GetValueByKey<string>("Client", string.Empty);
            if (string.IsNullOrEmpty(client))
              throw new Exception(string.Format("No Client set for connector {0} in settings", connector.Name));

            string organisation = connector.ConnectorSettings.GetValueByKey<string>("Organization", string.Empty);
            if (string.IsNullOrEmpty(client))
              throw new Exception(string.Format("No Organization set for connector {0} in settings", connector.Name));

            #region SAPfile
            //  //CONNECTORRELATION
          //  //CustomerID = JDE_nummer
          //  //AdministrationCode = Filiaal
          //  log.Debug("Start process productgroups Idoc for " + connector.Name);

          //  int idoc = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
          //  var output = new List<object>();

          //  #region ProductGroup
          //  #region Header
          //  // add lines to output;
          //  SAPiDocConnector.FileFormats.EDI_DC40 groupHeader = new SAPiDocConnector.FileFormats.EDI_DC40()
          //  {
          //    TABNAM = "EDI_DC40",
          //    MANDT = client,
          //    DOCNUM = idoc,
          //    DOCREL = "700",
          //    STATUS = "30",
          //    DIRECT = "1",
          //    OUTMOD = "4",
          //    EXPRSS = string.Empty,
          //    TEST = string.Empty,
          //    IDOCTYP = "WPDWGR01",
          //    CIMTYP = string.Empty,
          //    MESTYP = "WPDWGR",
          //    MESCOD = string.Empty,
          //    MESFCT = string.Empty,
          //    STD = string.Empty,
          //    STDVRS = string.Empty,
          //    STDMES = string.Empty,
          //    SNDPOR = senderPort,
          //    SNDPRT = "LS",
          //    SNDPFC = string.Empty,
          //    SNDPRN = partnerNumber,
          //    SNDSAD = string.Empty,
          //    SNDLAD = string.Empty,
          //    RCVPOR = "ZUNI_BASIC",
          //    RCVPRT = "KU",
          //    RCVPFC = string.Empty,
          //    RCVPRN = organisation.PadLeft(10, '0'),
          //    RCVSAD = string.Empty,
          //    RCVLAD = string.Empty,
          //    CREDAT = DateTime.Now,
          //    CRETIM = DateTime.Now,
          //    REFINT = string.Empty,
          //    SERIAL = DateTime.Now.ToString("yyyyMMddHHmmss")
          //  };
          //  output.Add(groupHeader);
          //  #endregion

          //  var productgroups = (from pg in products.Root.Elements("Product").Elements("ProductGroupHierarchy").Elements("ProductGroup")
          //                       select new
          //                       {
          //                         GroupID = pg.Attribute("MappingID").Value,
          //                         Name = pg.Attribute("Name").Value,
          //                       }).Distinct().ToList();

          //  productgroups.Add(new
          //  {
          //    GroupID = "014093",
          //    Name = "Geen Goederengroep"
          //  });

          //  foreach (var group in productgroups)
          //  {
          //    int lineIndex = 1;
          //    #region E2WPW01
          //    SAPiDocConnector.FileFormats.WPDWGR01.E2WPW01 E2WPW01 = new SAPiDocConnector.FileFormats.WPDWGR01.E2WPW01()
          //  {
          //    SEGNAM = "E2WPW01",
          //    MANDT = groupHeader.MANDT,
          //    DOCNUM = groupHeader.DOCNUM,
          //    SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //    PSGNUM = "000000",
          //    HLEVEL = "02",
          //    FILIALE = organisation.PadLeft(10, '0'),
          //    AENDKENNZ = "MODI",
          //    AKTIVDATUM = DateTime.Now,
          //    AENDDATUM = DateTime.MaxValue,
          //    WARENGR = group.GroupID.PadLeft(6, '0')
          //  };
          //    output.Add(E2WPW01);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPW01
          //    SAPiDocConnector.FileFormats.WPDWGR01.E2WPW02 E2WPW02 = new SAPiDocConnector.FileFormats.WPDWGR01.E2WPW02()
          //    {
          //      SEGNAM = "E2WPW02",
          //      MANDT = groupHeader.MANDT,
          //      DOCNUM = groupHeader.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = "000000",
          //      HLEVEL = "03",
          //      VORZEICHEN = string.Empty,
          //      BEZEICH = group.Name,
          //      HIERARCHIE = "00",
          //      VERKNUEPFG = "10",
          //      TASTEERL = string.Empty,
          //      VERKSPERRE = string.Empty,
          //      RABERLAUBT = string.Empty,
          //      WAAGENART = string.Empty
          //    };
          //    output.Add(E2WPW02);
          //    #endregion
          //    lineIndex++;
          //  }

          //  var fileName = string.Format("{0}_{1}_WPDWGR", organisation.PadLeft(10, '0'), idoc.ToString().PadLeft(8, '0'));

          //  engine.WriteFile(Path.Combine(path, "SAP2CF/Import/BasicData", fileName), output);
          //  #endregion

          //  log.Debug("Finish process productgroups Idoc for " + connector.Name);
          //  log.Debug("Start process product Idoc for " + connector.Name);

          //  #region PLU file
          //  engine = new MultiRecordEngine(RecordTypeCache.AllTypes);
          //  engine.RecordSelector = new RecordTypeSelector(FileFormatSelector.RecordSelector);
          //  output = new List<object>();

          //  #region Header
          //  // add lines to output;
          //  SAPiDocConnector.FileFormats.EDI_DC40 header = new SAPiDocConnector.FileFormats.EDI_DC40()
          //  {
          //    TABNAM = "EDI_DC40",
          //    MANDT = client,
          //    DOCNUM = idoc,
          //    DOCREL = "700",
          //    STATUS = "30",
          //    DIRECT = "1",
          //    OUTMOD = "4",
          //    EXPRSS = string.Empty,
          //    TEST = string.Empty,
          //    IDOCTYP = "WP_PLU02",
          //    CIMTYP = string.Empty,
          //    MESTYP = "WP_PLU",
          //    MESCOD = string.Empty,
          //    MESFCT = string.Empty,
          //    STD = string.Empty,
          //    STDVRS = string.Empty,
          //    STDMES = string.Empty,
          //    SNDPOR = senderPort,
          //    SNDPRT = "LS",
          //    SNDPFC = string.Empty,
          //    SNDPRN = partnerNumber,
          //    SNDSAD = string.Empty,
          //    SNDLAD = string.Empty,
          //    RCVPOR = "ZUNI_BASIC",
          //    RCVPRT = "KU",
          //    RCVPFC = string.Empty,
          //    RCVPRN = organisation.PadLeft(10, '0'),
          //    RCVSAD = string.Empty,
          //    RCVLAD = string.Empty,
          //    CREDAT = DateTime.Now,
          //    CRETIM = DateTime.Now,
          //    REFINT = string.Empty,
          //    SERIAL = DateTime.Now.ToString("yyyyMMddHHmmss")
          //  };
          //  output.Add(header);
          //  #endregion

          //  int productCount = 1;
          //  foreach (var r in products.Root.Elements("Product"))
          //  {
          //    var posNr = r.Attribute("CustomProductID").Value; //ConnectFlowUtility.GetAtricleNumber(r.Attribute("ProductID").Value, productTranslation, true).Trim();
          //    var group = r.Elements("ProductGroupHierarchy").Elements("ProductGroup").FirstOrDefault();
          //    var groupID = group != null ? group.Attribute("MappingID").Value : "14093"; //DUMMIE Group vaatwasser

          //    int lineIndex = 1;
          //    #region E2WPA01
          //    var ean = r.Element("Barcodes").Elements("Barcode").FirstOrDefault();
          //    var artNr = 0;

          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA01 e2wpa01 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA01()
          //    {
          //      SEGNAM = "E2WPA01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = "000000",
          //      HLEVEL = "02",
          //      FILIALE = organisation.PadLeft(10, '0'),
          //      AENDKENNZ = "MODI",
          //      AENDTYP = "ALL",
          //      AKTIVDATUM = DateTime.Now,
          //      AENDDATUM = DateTime.MaxValue,
          //      AENDERER = string.Empty,
          //      HAUPTEAN = ean != null ? ean.Value : string.Empty,
          //      ARTIKELNR = int.TryParse(posNr, out artNr) ? artNr.ToString().PadLeft(18, '0') : posNr,
          //      POSME = "ST"
          //    };
          //    output.Add(e2wpa01);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA02002
          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA02002 e2wpa02002 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA02002()
          //    {
          //      SEGNAM = "E2WPA02002",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      WARENGR = groupID.PadLeft(6, '0'),
          //      VERPGEW = "0",
          //      WAAGENKR = string.Empty,
          //      MEINPVERB = string.Empty,
          //      VKINPERFOR = string.Empty,
          //      VERKSPERRE = " ",//??
          //      RABERLAUBT = string.Empty,
          //      WAAGENART = string.Empty,
          //      MWSTFLAG = string.Empty,
          //      PHOHEITFIL = string.Empty,
          //      PRDRUCK = string.Empty,
          //      ARTIKANZ = string.Empty,
          //      MHDRZ = "0",
          //      MHDHB = "0",
          //      ARTCNT = string.Empty,
          //      WMAKG = string.Empty
          //    };
          //    output.Add(e2wpa02002);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA03 ShortDescription
          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA03 e2wpa03 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA03()
          //    {
          //      SEGNAM = "E2WPA03",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      QUALARTTXT = "LTXT",
          //      SPRASCODE = "N",
          //      TEXT = string.Format("{0} {1}",r.Element("Brands").Element("Brand").Element("Name").Value, r.Element("Content").Attribute("ShortDescription").Value),
          //      LFDNR = "0001"
          //    };
          //    output.Add(e2wpa03);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA03 LongDescription
          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA03 e2wpa03_2 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA03()
          //    {
          //      SEGNAM = "E2WPA03",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      QUALARTTXT = "02",
          //      SPRASCODE = "N",
          //      TEXT = r.Element("Content").Attribute("ShortDescription").Value,//r.Element("Content").Attribute("LongDescription").Value,
          //      LFDNR = "0001"
          //    };
          //    output.Add(e2wpa03_2);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA04 CostPrice
          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA04 e2wpa04_2 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA04()
          //    {
          //      SEGNAM = "E2WPA04",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      KONDART = "VPRS",
          //      AKTIONSNR = string.Empty,
          //      BEGINDATUM = DateTime.Now.ToString("yyyyMMdd"),
          //      BEGINNZEIT = DateTime.Now.ToString("HHmm"),
          //      ENDDATUM = DateTime.MaxValue.ToString("yyyyMMdd"),
          //      ENDZEIT = DateTime.MaxValue.ToString("HHmm"),
          //      FREIVERW1 = string.Empty
          //    };
          //    output.Add(e2wpa04_2);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA05002 CostPrice

          //    //var costPrice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, new CultureInfo("en-US")) * ((decimal.Parse(r.Element("Price").Attribute("TaxRate").Value.Replace("\"", string.Empty).Trim(), new CultureInfo("en-US")) / 100) + 1);

          //    //TODO: Check if correct
          //    var costPriceValue = r.Try(c => decimal.Parse(r.Element("Price").Element("CostPrice").Value, new CultureInfo("en-US")), 0);
          //    var taxRateValue = r.Try(c => decimal.Parse(r.Element("Price").Attribute("TaxRate").Value.Replace("\"", string.Empty).Trim(), new CultureInfo("en-US")));

          //    var costPrice = costPriceValue * ((taxRateValue / 100) + 1);

          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA05002 e2wpa05002_2 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA05002()
          //    {
          //      SEGNAM = "E2WPA05002",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "04",
          //      VORZEICHEN = "+",
          //      KONDSATZ = string.Empty,
          //      KONDWERT = decimal.Round(costPrice, 2).ToString(),
          //      MENGE = "1",
          //      CURRENCY = "EUR",
          //      CONTENT_UNIT = string.Empty
          //    };
          //    output.Add(e2wpa05002_2);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA04 Price
          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA04 e2wpa04 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA04()
          //    {
          //      SEGNAM = "E2WPA04",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      KONDART = "VKP0",
          //      AKTIONSNR = string.Empty,
          //      BEGINDATUM = DateTime.Now.ToString("yyyyMMdd"),
          //      BEGINNZEIT = DateTime.Now.ToString("HHmm"),
          //      ENDDATUM = DateTime.MaxValue.ToString("yyyyMMdd"),
          //      ENDZEIT = DateTime.MaxValue.ToString("HHmm"),
          //      FREIVERW1 = string.Empty
          //    };
          //    output.Add(e2wpa04);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA05002 Price

          //    var price = r.Element("Price") != null && !string.IsNullOrEmpty(r.Element("Price").Element("UnitPrice").Value) ? decimal.Parse(r.Element("Price").Element("UnitPrice").Value, new CultureInfo("en-US"))
          //      * ((decimal.Parse(r.Element("Price").Attribute("TaxRate").Value.Replace("\"", string.Empty).Trim(), new CultureInfo("en-US")) / 100) + 1) : 0;

          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA05002 e2wpa05002 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA05002()
          //    {
          //      SEGNAM = "E2WPA05002",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "04",
          //      VORZEICHEN = "+",
          //      KONDSATZ = string.Empty,
          //      KONDWERT = decimal.Round(price, 2).ToString(),
          //      MENGE = "1",
          //      CURRENCY = "EUR",
          //      CONTENT_UNIT = string.Empty
          //    };
          //    output.Add(e2wpa05002);
          //    #endregion
          //    lineIndex++;

          //    #region E2WPA07 TaxRate
          //    var tax = decimal.Parse(r.Try(c => c.Element("Price").Attribute("TaxRate").Value.Replace("\"", string.Empty).Trim(), "0"), new CultureInfo("en-US"));

          //    SAPiDocConnector.FileFormats.WP_PLU.E2WPA07 e2wpa07 = new SAPiDocConnector.FileFormats.WP_PLU.E2WPA07()
          //    {
          //      SEGNAM = "E2WPA07",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      MWSKZ = tax == 19 ? "A3" : "A1"
          //    };
          //    output.Add(e2wpa07);
          //    #endregion
          //    lineIndex++;

          //    #region E2WXX01 Type
          //    string type = r.Element("ShopInformation").Element("LedgerClass").Value;

          //    //                HAWA-> STCK
          //    //REPA-> NSRP
          //    //DIEN-> NSSV
          //    //OENA-> FOTO
          //    //VERZ-> ADDW
          //    //Z100-> STSV

          //    //switch (r.Element("ShopInformation").Element("LedgerClass").Value)
          //    //{
          //    //  case "NSRP":
          //    //    type = "REPA";
          //    //    break;
          //    //  case "NSSV":
          //    //    type = "DIEN";
          //    //    break;
          //    //  case "FOTO":
          //    //    type = "OENA";
          //    //    break;
          //    //  case "ADDW":
          //    //    type = "VERZ";
          //    //    break;
          //    //  case "STSV":
          //    //    type = "Z100";
          //    //    break;
          //    //};

          //    SAPiDocConnector.FileFormats.E1WXX01 e1wxx01 = new SAPiDocConnector.FileFormats.E1WXX01()
          //    {
          //      SEGNAM = "E2WXX01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      FLDGRP = "TYPE",
          //      FLDNAME = "ATTYP",
          //      FLDVAL = type
          //    };
          //    output.Add(e1wxx01);
          //    #endregion
          //    lineIndex++;

          //    #region E2WXX01 Salesflow
          //    SAPiDocConnector.FileFormats.E1WXX01 e1wxx01_s = new SAPiDocConnector.FileFormats.E1WXX01()
          //    {
          //      SEGNAM = "E2WXX01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      FLDGRP = string.Empty,
          //      FLDNAME = "SALESFLOW",
          //      FLDVAL = string.Empty
          //    };
          //    output.Add(e1wxx01_s);
          //    #endregion
          //    lineIndex++;

          //    if (insurances.ContainsKey(r.Attribute("ProductID").Value))
          //    {
          //      #region E2WXX01 Salesflow
          //      SAPiDocConnector.FileFormats.E1WXX01 e1wxx01_5 = new SAPiDocConnector.FileFormats.E1WXX01()
          //      {
          //        SEGNAM = "E2WXX01",
          //        MANDT = header.MANDT,
          //        DOCNUM = header.DOCNUM,
          //        SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //        PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //        HLEVEL = "03",
          //        FLDGRP = "KENM",
          //        FLDNAME = "RISKGROUP",
          //        FLDVAL = insurances[r.Attribute("ProductID").Value].ToString(),
          //      };
          //      output.Add(e1wxx01_5);
          //      #endregion
          //      lineIndex++;
          //    }

          //    #region E2WXX01 MVKE
          //    SAPiDocConnector.FileFormats.E1WXX01 e1wxx01_2 = new SAPiDocConnector.FileFormats.E1WXX01()
          //    {
          //      SEGNAM = "E2WXX01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      FLDGRP = "MVKE",
          //      FLDNAME = "WMSTA",
          //      FLDVAL = "20" //??
          //    };
          //    output.Add(e1wxx01_2);
          //    #endregion
          //    lineIndex++;

          //    #region E2WXX01 MVKE
          //    SAPiDocConnector.FileFormats.E1WXX01 e1wxx01_3 = new SAPiDocConnector.FileFormats.E1WXX01()
          //    {
          //      SEGNAM = "E2WXX01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      FLDGRP = "KENM",
          //      FLDNAME = "VENDORWAR",
          //      FLDVAL = "12" //??
          //    };
          //    output.Add(e1wxx01_3);
          //    #endregion
          //    lineIndex++;

          //    //HAWA = STCK
          //    //DIEN = SERV
          //    //KADO = ?? 
          //    //VERZ = WAR
          //    //OENA = ??
          //    //REPA = ??

          //    #region E2WXX01 MVKE
          //    SAPiDocConnector.FileFormats.E1WXX01 e1wxx01_4 = new SAPiDocConnector.FileFormats.E1WXX01()
          //    {
          //      SEGNAM = "E2WXX01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      FLDGRP = "KENM",
          //      FLDNAME = "BRAND",
          //      FLDVAL = r.Element("Brands").Element("Brand").Element("Name").Value
          //    };
          //    output.Add(e1wxx01_4);
          //    #endregion
          //    lineIndex++;

          //    #region E2WXX01 MVKE
          //    string status = r.Element("Price") != null ? r.Element("Price").Attribute("CommercialStatus").Value : string.Empty;
          //    string cfStatus = "60";

          //    if (status == "S")
          //      cfStatus = "20";
          //    else if (status == "U")
          //      cfStatus = "40";

          //    SAPiDocConnector.FileFormats.E1WXX01 e1wxx01_6 = new SAPiDocConnector.FileFormats.E1WXX01()
          //    {
          //      SEGNAM = "E2WXX01",
          //      MANDT = header.MANDT,
          //      DOCNUM = header.DOCNUM,
          //      SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //      PSGNUM = productCount.ToString().PadLeft(6, '0'),
          //      HLEVEL = "03",
          //      FLDGRP = "MVKE",
          //      FLDNAME = "VMSTA",
          //      FLDVAL = cfStatus
          //    };
          //    output.Add(e1wxx01_6);
          //    #endregion
          //    lineIndex++;

          //    productCount++;
          //  }

          //  //TODO Check with mark
          //  try
          //  {
          //    fileName = string.Format("{0}_{1}_WP_PLU", organisation.PadLeft(10, '0'), idoc.ToString().PadLeft(8, '0'));

          //    engine.WriteFile(Path.Combine(path, "SAP2CF/Import/BasicData", fileName), output);
          //  }
          //  catch (Exception e)
          //  {
          //    log.AuditCritical(string.Format("There was an error accessing: {0}", fileName), e, "Connect flow export");
          //  }
          //  #endregion

          //  log.Debug("Finish process product Idoc for " + connector.Name);
          //  log.Debug("Start process EAN Idoc for " + connector.Name);
          //  productCount = 0;
          //  #region Ean
          //  //CONNECTORRELATION
          //  //CustomerID = JDE_nummer
          //  //AdministrationCode = Filiaal


          //  engine = new MultiRecordEngine(RecordTypeCache.AllTypes);
          //  engine.RecordSelector = new RecordTypeSelector(FileFormatSelector.RecordSelector);
          //  output = new List<object>();
          //  //idoc = int.Parse(string.Format("{0}{1}{2}", connector.ConnectorID, connectorRelation.CustomerID, docNr));

          //  // add lines to output;
          //  SAPiDocConnector.FileFormats.EDI_DC40 eanheader = new SAPiDocConnector.FileFormats.EDI_DC40()
          //  {
          //    TABNAM = "EDI_DC40",
          //    MANDT = client,
          //    DOCNUM = idoc,
          //    DOCREL = "700",
          //    STATUS = "30",
          //    DIRECT = "1",
          //    OUTMOD = "4",
          //    EXPRSS = string.Empty,
          //    TEST = string.Empty,
          //    IDOCTYP = "WP_EAN01",
          //    CIMTYP = string.Empty,
          //    MESTYP = "WP_EAN",
          //    MESCOD = string.Empty,
          //    MESFCT = string.Empty,
          //    STD = string.Empty,
          //    STDVRS = string.Empty,
          //    STDMES = string.Empty,
          //    SNDPOR = senderPort,
          //    SNDPRT = "LS",
          //    SNDPFC = string.Empty,
          //    SNDPRN = partnerNumber,
          //    SNDSAD = string.Empty,
          //    SNDLAD = string.Empty,
          //    RCVPOR = "ZUNI_BASIC",
          //    RCVPRT = "KU",
          //    RCVPFC = string.Empty,
          //    RCVPRN = organisation.PadLeft(10, '0'),
          //    RCVSAD = string.Empty,
          //    RCVLAD = string.Empty,
          //    CREDAT = DateTime.Now,
          //    CRETIM = DateTime.Now,
          //    REFINT = string.Empty,
          //    SERIAL = DateTime.Now.ToString("yyyyMMddHHmmss")
          //  };
          //  output.Add(eanheader);

          //  productCount = 1;
          //  foreach (var r in products.Root.Elements("Product"))
          //  {
          //    var sapNumber = r.Attribute("CustomProductID").Value;//ConnectFlowUtility.GetAtricleNumber(r.Attribute("ProductID").Value, productTranslation, true);

          //    int lineIndex = 1;
          //    #region E2WPE01
          //    var artNr = 0;

          //    r.Element("Barcodes").Elements("Barcode").ForEach((xel, x) =>
          //    {
          //      if (lineIndex == 1)
          //      {
          //        SAPiDocConnector.FileFormats.WP_EAN01.E2WPE01 e2wpe01 = new SAPiDocConnector.FileFormats.WP_EAN01.E2WPE01()
          //        {
          //          SEGNAM = "E2WPE01",
          //          MANDT = header.MANDT,
          //          DOCNUM = header.DOCNUM,
          //          SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //          PSGNUM = "000000",
          //          HLEVEL = "02",
          //          FILIALE = organisation.PadLeft(10, '0'),
          //          AENDKENNZ = "MODI",
          //          AKTIVDATUM = DateTime.Now,
          //          AENDDATUM = DateTime.MaxValue,
          //          AENDERER = string.Empty,
          //          HAUPTEAN = xel.Value,
          //          ARTIKELNR = int.TryParse(sapNumber, out artNr) ? artNr.ToString().PadLeft(18, '0') : sapNumber,
          //          POSME = "ST"
          //        };
          //        output.Add(e2wpe01);
          //    #endregion
          //        lineIndex++;
          //      }
          //      else
          //      {

          //        #region E2WPE02
          //        SAPiDocConnector.FileFormats.WP_EAN01.E2WPE02 e2wpe02 = new SAPiDocConnector.FileFormats.WP_EAN01.E2WPE02()
          //        {
          //          SEGNAM = "E2WPE02",
          //          MANDT = header.MANDT,
          //          DOCNUM = header.DOCNUM,
          //          SEGNUM = lineIndex.ToString().PadLeft(6, '0'),
          //          PSGNUM = "000000",
          //          HLEVEL = "03",
          //          EAN = xel.Value
          //        };
          //        output.Add(e2wpe02);
          //        #endregion
          //      }
          //      lineIndex++;
          //    });
          //  }

          //  // fileName = string.Format("WP_EAN_{0}_{1}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"), idoc.ToString().PadLeft(16, '0'));
          //  fileName = string.Format("{0}_{1}_WP_EAN", organisation.PadLeft(10, '0'), idoc.ToString().PadLeft(8, '0'));

          //  engine.WriteFile(Path.Combine(path, "SAP2CF/Import/BasicData", fileName), output);
          //  #endregion

          //  log.Debug("Finish process EAN Idoc for " + connector.Name);
#endregion
            log.Debug("Start import/update products in Aggregator for " + connector.Name);


            #region Aggregators
            var productList = (from p in products.Root.Elements("Product")
                               let SAPNumber = p.Attribute("CustomProductID").Value//ConnectFlowUtility.GetAtricleNumber(p.Attribute("ProductID").Value, productTranslation, true)
                               let pg = p.Elements("ProductGroupHierarchy").Elements("ProductGroup").FirstOrDefault()
                               let groupID = pg != null ? pg.Attribute("MappingID").Value : "14093" //DUMMIE Group vaatwasser
                               let groupName = pg != null ? pg.Attribute("Name").Value : "Unknown"
                               let taxRate = p.Try(c => decimal.Parse(p.Element("Price").Attribute("TaxRate").Value.Replace("\"", string.Empty).Trim(), new CultureInfo("en-US")), decimal.Parse("19")) //quick fix
                               select new
                               {
                                 JdeNumber = p.Attribute("CustomProductID").Value.Try(x => int.Parse(x), 0),
                                 ConcentratorProductID = int.Parse(p.Attribute("ProductID").Value),
                                 Organization = partnerNumber,
                                 POSnumber = string.IsNullOrEmpty(SAPNumber) ? string.Empty : SAPNumber.Trim(),
                                 Description = p.Element("Content").Attribute("ShortDescription").Value,
                                 LongDescription = p.Element("Content").Attribute("LongDescription").Value,
                                 ProductGroup = groupID,
                                 GroupName = groupName,
                                 Vat = (taxRate / 100),
                                 UnitPrice = (p.Element("Price") != null && !string.IsNullOrEmpty(p.Element("Price").Element("UnitPrice").Value)) ? (decimal.Parse(p.Element("Price").Element("UnitPrice").Value, new CultureInfo("en-US")) *
                                 ((taxRate / 100) + 1)) : 0,
                                 CostPrice = p.Element("Price") != null ? decimal.Parse(p.Element("Price").Element("CostPrice").Value, new CultureInfo("en-US")) : 0,
                                 Insurance = insurances.ContainsKey(p.Attribute("ProductID").Value) ? insurances[p.Attribute("ProductID").Value] : 0,
                                 LedgerClass = p.Element("ShopInformation").Element("OriginalLedgerClass").Value,
                                 VendorItemNumber = p.Attribute("ManufacturerID").Value,
                                 BrandName = p.Element("Brands").Element("Brand").Element("Name").Value,
                                 CommercialStatus = p.Element("Price") != null ? p.Element("Price").Attribute("CommercialStatus").Value : "O",
                               }).ToList();

            #region Aggregator Products
            

            int total = productList.Count;
            int productsImported = 0;
            int counter = 0;
            List<string> aggregatorError = new List<string>();

            //var aggregatorProductList = (from p in productList
            //                             select new AggregatorProducts
            //                             {
            //                               ProductId = p.POSnumber, 
            //                               Description = p.Description.Replace("'", "''"),
            //                               VatRate = p.Vat, 
            //                               ConcentratorProductId = p.ConcentratorProductID, 
            //                               JdeProductId = p.JdeNumber,
            //                               LedgerClass = p.LedgerClass, 
            //                               RiskGroup = p.Insurance, 
            //                               ManufacturerId = p.VendorItemNumber, 
            //                               Manufacturer = p.BrandName.Replace("'", "''"), 
            //                               Description2 = p.LongDescription, 
            //                               ProductGroupName = p.GroupName
            //                             }).ToList();

            //using (AggregatorBulk bulk = new AggregatorBulk(aggregatorProductList, ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
            //{
            //  bulk.Init(unit.Context);
            //  bulk.Sync(unit.Context);
            //}

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
            {
              connection.Open();

              productList.ForEach(x =>
              {
                productsImported++;
                counter++;

                if (counter == 250)
                {
                  log.DebugFormat("Processing product to Aggregator {0}/{1} for {2}", productsImported, total, connector.Name);
                  counter = 0;
                }

                try
                {
                  string query = string.Format(@"UPDATE [Aggregator].[dbo].[Products] SET Description = '{1}' 
, vatrate = {2}, concentratorProductID = {3}, ledgerclass = '{5}', riskgroup = '{6}', ManufacturerId = '{7}', Manufacturer = '{8}', ProductId = '{0}',[Description2] = '{9}',[ProductGroupName] = '{10}', [CF_ProductGroupId] = {11}
 WHERE jdeproductid = {4} and isstatic != 1
IF @@ROWCOUNT = 0 AND not exists (select productid from products where jdeproductid = {4}) 
BEGIN
INSERT INTO [Aggregator].[dbo].[Products] ([ProductId],[Description],[VatRate],[ConcentratorProductID],[JdeProductId],[LedgerClass],[RiskGroup],[ManufacturerId],[Manufacturer],[Description2],[ProductGroupName],[CF_ProductGroupId])
VALUES ('{0}','{1}',{2},{3},{4},'{5}',{6},'{7}','{8}','{9}','{10}',{11}) 
END", x.POSnumber, x.Description.Replace("'", "''"), x.Vat, x.ConcentratorProductID, x.JdeNumber, x.LedgerClass, x.Insurance, x.VendorItemNumber.Replace("'", "''"), x.BrandName.Replace("'", "''"), x.LongDescription.Replace("'", "''"), x.GroupName.Replace("'", "''"), x.ProductGroup).Trim();

                  using (SqlCommand command = new SqlCommand(query, connection))
                  {
                    command.ExecuteNonQuery();
                  }
                }
                catch (Exception ex)
                {
                  string error = string.Format("Error insert product in aggregator, JDE: {0}, SAP:{1}, Concentrator:{2}", x.JdeNumber, x.POSnumber, x.ConcentratorProductID);

                  log.Error(error, ex);
                  aggregatorError.Add(error);
                }
              });
            }

            #endregion
            
            log.Debug("Finish import/update products in Aggregator for " + connector.Name);
            log.Debug("Start import/update prices in Aggregator for " + connector.Name);

            #region Aggregator Pricing

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
            {
              connection.Open();
              productsImported = 0;
              counter = 0;

              productList.ForEach(x =>
              {
                ActiveConcentratorProducts.Add(x.ConcentratorProductID);
                productsImported++;
                counter++;

                if (counter == 250)
                {
                  log.DebugFormat("Processing prices to Aggregator {0}/{1} for {2}", productsImported, total, connector.Name);
                  counter = 0;
                }

                try
                {
                  using (SqlCommand command = new SqlCommand("ChangeProductPrice", connection))
                  {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@RelationId", connector.BackendEanIdentifier);
                    command.Parameters.AddWithValue("@ConcentratorProductID", x.ConcentratorProductID);
                    command.Parameters.AddWithValue("@UnitCost", x.CostPrice);
                    command.Parameters.AddWithValue("@UnitPRice", x.UnitPrice);
                    command.Parameters.AddWithValue("@CommercialStatus", x.CommercialStatus);
                    command.ExecuteNonQuery();
                  }
                }
                catch (Exception ex)
                {
                  string error = string.Format("Error update price in aggregator, JDE: {0}, SAP:{1}, Concentrator:{2}", x.JdeNumber, x.POSnumber, x.ConcentratorProductID);
                  log.Error("Error insert price in aggregator", ex);
                  aggregatorError.Add(error);
                }



                //                  DECLARE @RC int
                //DECLARE @RelationId int
                //DECLARE @SAPItemNumber nvarchar(20)
                //DECLARE @UnitCost decimal(18,2)
                //DECLARE @UnitPrice decimal(18,2)

                //-- TODO: Set parameter values here.

                //EXECUTE @RC = [PosContext].[dbo].[ChangeProductPrice] 
                //   @RelationId
                //  ,@SAPItemNumber
                //  ,@UnitCost
                //  ,@UnitPrice


              });
            }
            #endregion
            log.Debug("Finish import/update prices in Aggregator for " + connector.Name);

            log.Debug("Start import/update barcodes in Aggregator for " + connector.Name);
            #region barcode
//            var barcodes = new Dictionary<int,List<string>>();

//            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
//            {
//              connection.Open();

//              using (SqlCommand command = new SqlCommand(@"select 
//p.JdeProductId,
//b.Code
//from Barcodes b
//inner join Products p on b.ProductId = p.Id", connection))
//              {
//                using (SqlDataReader r = command.ExecuteReader())
//                {
//                  while (r.Read())
//                  {
//                    int prodid = (int)r["JdeProductId"];
//                    string barcode = (string)r["Code"];

//                    if (barcodes.ContainsKey(prodid))
//                      barcodes[prodid].Add(barcode);
//                    else
//                      barcodes.Add(prodid, new List<string>() { barcode });

//                  }
//                }
//              }

//              try
//              {
//                products.Root.Elements("Product").ForEach((prod, x) =>
//                {
//                  int JdeNumber = prod.Attribute("CustomProductID").Value.Try(j => int.Parse(j), 0);
//                  var unusedBarcodes = new List<string>();

//                  if (barcodes.ContainsKey(JdeNumber))
//                    unusedBarcodes = barcodes[JdeNumber];
                  
//                  prod.Element("Barcodes").Elements("Barcode").ForEach((bar, idx) =>
//                  {
//                    string barcode = bar.Value.Trim();

//                    if (unusedBarcodes.Contains(barcode))
//                    {
//                      unusedBarcodes.Remove(barcode);
//                    }
//                    else
//                    {
//                      string query = string.Format(@"insert into barcodes (productid,code)
//select id, '{0}' from products where jdeproductid = {1}", barcode, JdeNumber);
//                      //unusedBarcodes.Remove(barcode);

//                      using (SqlCommand command = new SqlCommand(query, connection))
//                      {
//                        command.ExecuteNonQuery();
//                      }
//                    }
//                  });

//                  unusedBarcodes.ForEach(b =>
//                  {
//                    string deleteBarcodeQuery = string.Format(@"DELETE b
//FROM barcodes b
//INNER JOIN products p on b.productid = p.id
//where p.jdeproductid = {0} and code = '{1}'",JdeNumber,b);

//                    using (SqlCommand command = new SqlCommand(deleteBarcodeQuery, connection))
//                    {
//                      command.ExecuteNonQuery();
//                    }
//                  });

//                });
//              }
//              catch (Exception ex)
//              {
//                log.Error("Error export barcodes to aggregator", ex);
//              }
//            }
            #endregion
            log.Debug("Finish import/update products in Aggregator for " + connector.Name);

            #endregion

            try
            {
              if (aggregatorError.Count() > 0)
              {
                if (GetConfiguration().AppSettings.Settings["AggregatorLog"] != null)
                {
                  string aggratorLogpath = GetConfiguration().AppSettings.Settings["AggregatorLog"].Value;

                  aggratorLogpath = Path.Combine(aggratorLogpath, DateTime.Now.ToShortDateString());

                  if (!Directory.Exists(aggratorLogpath))
                    Directory.CreateDirectory(aggratorLogpath);

                  string aggregatorLog = Path.Combine(aggratorLogpath, "Aggregator_" + connector.Name + ".log");

                  using (StreamWriter sw = new StreamWriter(aggregatorLog))
                  {
                    aggregatorError.ForEach((x) => sw.WriteLine(x));
                  }
                }
              }

            }
            catch (Exception ex)
            {
              log.Error("Failed to write Aggregator Log");
            }


          }
          catch (Exception ex)
          {
            log.Error("Error export assortment connectflow", ex);
          }


          log.DebugFormat("Finish Process connectflow export for {0}", connector.Name);

        }
      }

      //#region Trigger
      //string readyFile = Path.Combine(path, "SAP2CF/READY.TRG");

      //using (StreamWriter sw = new StreamWriter(readyFile))
      //{
      //  sw.Write("Trigger");
      //}
      //#endregion

      #region ActiveProducts
      using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(@"Update products set isactive = 0 where isstatic != 1", connection))
        {
          command.ExecuteNonQuery();
        }
      }

      ActiveConcentratorProducts.ForEach(x =>
      {
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Aggregator"].ConnectionString))
        {
          connection.Open();

          using (SqlCommand command = new SqlCommand(string.Format("Update products set isactive = 1 where concentratorproductid = {0}",x), connection))
          {
            command.ExecuteNonQuery();
          }
        }
      });
      #endregion

      if (networkDrive)
      {
        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          oNetDrive.LocalDrive = path;
          oNetDrive.UnMapDrive();
        }
        catch (Exception err)
        {
          log.Error("Error unmap drive" + err.InnerException);
        }
        oNetDrive = null;
      }
    }
  }
}
