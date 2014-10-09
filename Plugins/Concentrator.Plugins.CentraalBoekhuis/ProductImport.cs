using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Linq;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Ftp;
using System.Xml.Linq;
using System.Xml;
using Concentrator.Objects.CSV;
using System.Text;

namespace Concentrator.Plugins.CentraalBoekhuis
{
  public class ProductImport : VendorBase
  {
    protected override Configuration Config
    {
      get { return GetConfiguration(); }
    }

    protected override int VendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["DefaultVendorID"].Value); }
    }

    //List of all the columns that will be stored as attributes
    private string[] AttributeMapping = new[] 
    { 
        "BIBLIOGRAFISCHE_DRUK", "GEILLUSTREERD_IND", "AANTAL_PAGINAS", "TAAL_KD", "COMM_VERSCHIJNINGSDATUM", "DRM_CP_AANT_PAG","DRM_CP_AANT_PAG_EENH",
        "DRM_PRINT_AANT_PAG", "DRM_PRINT_AANT_PAG_EENH", "DRM_VOORLEES_IND", "DRM_AANTAL_APPARATEN"
    };

    public ProductImport() { }

    public override string Name
    {
      get { return "Centraal boekhuis product import"; }
    }

    private void GetContent()
    {
      using (var unit = GetUnitOfWork())
      {
        //Config settings
        KeyValueConfigurationCollection config = GetConfiguration().AppSettings.Settings;

        List<ProductAttributeMetaData> attributes;

        SetupAttributes(unit, AttributeMapping, out attributes, VendorID);

        //Used for VendorImportAttributeValues
        var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == VendorID).ToList();
        var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

        //Will contain all the columns in a row
        string[] column = new string[88];
        string[] lines = null;

        #if !DEBUG

          // Connection to ftp folder
          FtpManager manager = new FtpManager(config["CentraalBoekhuisFtpUrl"].Value, config["CentraalBoekhuisFilePath"].Value,
                 config["CentraalBoekhuisUser"].Value, config["CentraalBoekhuisPassword"].Value, false, true, log);

          // Gets the first file in the folder
          var data = manager.FirstOrDefault().Data;          
          
          // Stream reader to read the file
          using (StreamReader reader = new StreamReader(data, Encoding.UTF8))
          {
            lines = reader.ReadToEnd().Split('\n');
          };
        
        #else
        
          var directory = config["CentraalBoekhuisDirectory"].Value;
          var fileName = config["CentraalBoekhuisFile"].Value;

          String fullPath = Path.Combine(directory, fileName);

          lines = File.ReadAllLines(fullPath);

        #endif

        List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

        // Loops through all the rows
        lines.ForEach((line, idx) =>
        {
          // Line is a row of data seperated by semicolons. The values will be split and added to the column array
          column = line.Split(';');

          //Creates a list of type VendorAssortmentItem         
          var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
          {
            #region BrandVendor
            BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = column[3].Trim(), //UITGEVER_ID
                                ParentBrandCode = null,
                                Name = column[4].Trim() //UITGEVER_NM
                            }
                        },
            #endregion

            #region GeneralProductInfo
            VendorProduct = new VendorAssortmentBulk.VendorProduct
            {
              VendorItemNumber = column[0].Trim(), //EAN
              CustomItemNumber = column[0].Trim(), //EAN
              ShortDescription = column[2].Trim(), //Subtitel
              LongDescription = column[84].Trim(),
              LineType = null,
              LedgerClass = null,
              ProductDesk = null,
              ExtendedCatalog = null,
              VendorID = VendorID,
              DefaultVendorID = VendorID,
              VendorBrandCode = column[3].Trim(), //UITGEVER_ID
              Barcode = column[0].Trim(), //EAN
              VendorProductGroupCode1 = column[9].Trim(), //REEKS_NR
              VendorProductGroupCodeName1 = column[8].Trim(), //REEKS_NM
              VendorProductGroupCode2 = column[7].Trim(),//BOEKSOORT_KD
              VendorProductGroupCodeName2 = null //GEEN NAAM                        
            },
            #endregion

            #region RelatedProducts
            RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = column[0].Trim(), //EAN
                                RelatedProductType = column[19].Trim(), //ISBN_FYSIEK_BOEK
                                RelatedCustomItemNumber = column[19].Trim(), //ISBN_FYSIEK_BOEK
                            }
                        },
            #endregion

            #region Attributes
            VendorImportAttributeValues = (from attr in AttributeMapping
                                           let prop = column.Contains(attr)//d.Field<object>(attr)                                                       
                                           let attributeID = attributeList.ContainsKey(attr) ? attributeList[attr] : 2
                                           let value = prop.ToString()
                                           where !string.IsNullOrEmpty(value)
                                           select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                           {
                                             VendorID = VendorID,
                                             DefaultVendorID = DefaultVendorID,
                                             CustomItemNumber = column[0].Trim(), //EAN
                                             AttributeID = attributeID,
                                             Value = value,
                                             LanguageID = "1",
                                             AttributeCode = attr,
                                           }).ToList(),
            #endregion

            #region Prices
            VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = column[0].Trim(), //EAN
                                Price = column[24].Trim(), //ADVIESPRIJS
                                CostPrice = column[25].Trim(), //NETTOPRIJS
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = column[18].Trim()//STADIUM_LEVENSCYCLUS_KD
                            }
                        },
            #endregion

            #region Stock
            VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = column[0].Trim(), //EAN
                                QuantityOnHand = 0,
                                StockType = "Assortment",
                                StockStatus = column[18].Trim()//STADIUM_LEVENSCYCLUS_KD
                            }
                        },
            #endregion
          };

          // assortment will be added to the list defined outside of this loop
          assortmentList.Add(assortment);
        });

        // Creates a new instance of VendorAssortmentBulk(Passes in the AssortmentList defined above, vendorID and DefaultVendorID)
        using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, VendorID))
        {
          vendorAssortmentBulk.Init(unit.Context);
          vendorAssortmentBulk.Sync(unit.Context);
        }
      }
    }

    protected override void SyncProducts()
    {
      GetContent();
    }

  }
}

//string EAN = column[0].Trim();
//string TITEL = column[1].Trim();
//string SUBTITEL = column[2].Trim();
//string UITGEVER_ID = column[3].Trim();
//string UITGEVER_NM = column[4].Trim();
//string IMPRINT_NM = column[5].Trim();
//string NUR_KD = column[6].Trim();
//string BOEKSOORT_KD = column[7].Trim();
//string REEKS_NM = column[8].Trim();
//string REEKS_NR = column[9].Trim();
//string BIBLIOGRAFISCHE_DRUK = column[10].Trim();

//string GEILLUSTREERD_IND = column[11].Trim();
//string COMMERCIELE_TITEL = column[12].Trim();
//string AANTAL_PAGINAS = column[13].Trim();
//string TAAL_KD = column[14].Trim();
//string PRODUCT_VORM = column[15].Trim();
//string PRODUCT_VORM_DETAIL = column[16].Trim();
//string COMM_VERSCHIJNINGSDATUM = column[17].Trim();
//string STADIUM_LEVENSCYCLUS_KD = column[18].Trim();
//string ISBN_FYSIEK_BOEK = column[19].Trim();
//string AFBEELDING = column[20].Trim();


//string VERVANGEND_ARTIKEL_KD = column[21].Trim();
//string AUTEUR_TITEL = column[22].Trim();
//string MUTATIE_DTD = column[23].Trim();
//string ADVIESPRIJS = column[24].Trim();
//string NETTOPRIJS = column[25].Trim();
//string BEDRAG_BTW_LAAG = column[26].Trim();
//string BEDRAG_BTW_HOOG = column[27].Trim();
//string BESTANDSGROOTTE = column[28].Trim();
//string DRM_CP_AANT_PAG = column[29].Trim();
//string DRM_CP_AANT_PAG_EENH = column[30].Trim();

//string DRM_PRINT_AANT_PAG = column[31].Trim();
//string DRM_PRINT_AANT_PAG_EENH = column[32].Trim();
//string DRM_VOORLEES_IND = column[33].Trim();
//string KORTING_PERC = column[34].Trim();
//string HFDAUT_ANAAM = column[35].Trim();
//string HFDAUT_VOORL = column[36].Trim();
//string HFDAUT_VOORVGSL = column[37].Trim();
//string COAUT_1_ANAAM = column[38].Trim();
//string COAUT_1_VOORL = column[39].Trim();
//string COAUT_1_VOORVGSL = column[40].Trim();

//string COAUT_2_ANAAM = column[41].Trim();
//string COAUT_2_VOORL = column[42].Trim();
//string COAUT_2_VOORVGSL = column[43].Trim();
//string SECAUT_ANAAM = column[44].Trim();
//string SECAUT_VOORL = column[45].Trim();
//string SECAUT_VOORVGSL = column[46].Trim();
//string CORP_NAAM = column[47].Trim();
//string REDSAM_1_ANAAM = column[48].Trim();
//string REDSAM_1_VOORL = column[49].Trim();
//string REDSAM_1_VOORVGSL = column[50].Trim();

//string REDSAM_2_ANAAM = column[51].Trim();
//string REDSAM_2_VOORL = column[52].Trim();
//string REDSAM_2_VOORVGSL = column[53].Trim();
//string REDSAM_3_ANAAM = column[53].Trim();
//string REDSAM_3_VOORL = column[54].Trim();
//string REDSAM_3_VOORVGSL = column[55].Trim();
//string BEW_1_ANAAM = column[57].Trim();
//string BEW_1_VOORL = column[58].Trim();
//string BEW_1_VOORVGSL = column[59].Trim();
//string BEW_2_ANAAM = column[60].Trim();

//string BEW_2_VOORL = column[61].Trim();
//string BEW_2_VOORVGSL = column[62].Trim();
//string BEW_3_ANAAM = column[63].Trim();
//string BEW_3_VOORL = column[64].Trim();
//string BEW_3_VOORVGSL = column[65].Trim();
//string ILL_1_ANAAM = column[66].Trim();
//string ILL_1_VOORL = column[67].Trim();
//string ILL_1_VOORVGSL = column[68].Trim();
//string ILL_2_ANAAM = column[69].Trim();
//string ILL_2_VOORL = column[70].Trim();

//string ILL_2_VOORVGSL = column[71].Trim();
//string ILL_3_ANAAM = column[72].Trim();
//string ILL_3_VOORL = column[73].Trim();
//string ILL_3_VOORVGSL = column[74].Trim();
//string VERT_1_ANAAM = column[75].Trim();
//string VERT_1_VOORL = column[76].Trim();
//string VERT_1_VOORVGSL = column[77].Trim();
//string VERT_2_ANAAM = column[78].Trim();
//string VERT_2_VOORL = column[79].Trim();
//string VERT_2_VOORVGSL = column[80].Trim();

//string VERT_3_ANAAM = column[81].Trim();
//string VERT_3_VOORL = column[82].Trim();
//string VERT_3_VOORVGSL = column[83].Trim();
//string BESCHRIJVING = column[84].Trim();
//string MAG_BESTELLEN_IND = column[85].Trim();
//string EERSTE_HOOFDSTUK_STATUS = column[86].Trim();
//string EERSTE_HOOFDSTUK_TYPE = column[87].Trim();
//string DRM_AANTAL_APPARATEN = column[88].Trim();