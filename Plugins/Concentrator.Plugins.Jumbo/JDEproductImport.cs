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
using System.Data.OleDb;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.Jumbo
{
  public class ProductImport : JumboBase
  {
    public override string Name
    {
      get { return "Jumbo JDE product import"; }
    }

    private const int UnMappedID = -1;
    private string[] AttributeMapping = new[] {"Category", "Segment", "SubBrand", "Concept", "EA_length", "EA_depth", "EA_height", "EA_Volume",
      "CR_Length", "CR_depth", "CR_height", "CR_Volume", "Licensor", "CountryOfOrigin", 
      "License", "Character", "Pieces", "Gender", "WeigthTotalPackage", "AmountsOfProdctsPerOuterCarton","RelatedProductDesc", "Material" };

    private string[] FilterAttributeMapping = new[] { "Segment", "Character", "Pieces", "Material" };

    protected override void Process()
    {
      string jdeQuery = @"select
imdsc1 as ProductName,
imLitm as ProductCode,
imaitm as Barcode,
case when Smmm01 is null or smmm01 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm01) end as Category,
smmm01 as CategoryCode,
case when Smmm02 is null or smmm02 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm02) end as Segment,
smmm02 as SegmentCode,
case when Smmm03 is null or smmm03 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm03) end as Brand,
smmm03 as BrandCode,
case when Smmm04 is null or smmm04 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm04) end as subbrand,
smmm04 as SubBrandCode,
case when Smmm05 is null or smmm05 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm05) end as concept,
smmm05 as ConceptCode,
case when Smmm06 is null or smmm06 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm06) end as License,
smmm06 as LicenseCode,
case when Smmm07 is null or smmm07 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm07) end as [Character],
smmm07 as CharacterCode,
case when Smmm08 is null or smmm08 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm08) end as Pieces,
smmm08 as PiecesCode,
case when Smmm09 is null or smmm09 = '' then '' else (select DRDL01 from prodctl.F0005 where ltrim(DRKY) = smmm09) end as Gender,
smmm09 as GenderCode,
imsrp7 as CommercialStatus,
imsrp0 as Licensor,
--d.ExpireDateLicense,
c.iborig as CountryOfOrigin,
w.umCONV / 10000000 as WeigthTotalPackage,
P.umConv / 10000000 as AmountsOfProdctsPerOuterCarton,
v.ibmcu,
MCDL01 as BranchName,
v.ibstkt as StockStatus,
ea.iqgwid as EA_length,
ea.iqgdep as EA_depth,
ea.iqghet as EA_height,
ea.iqgcub as EA_Volume,
cr.iqgwid as CR_length,
cr.iqgdep as CR_depth,
cr.iqghet as CR_height,
cr.iqgcub as CR_Volume,
bat.RelatedQty,
bat.RelatedProductCode,
cast(bat.RelatedQty as nvarchar) + 'x ' + bat.RelatedProductDesc as RelatedProductDesc,
case when c.iborig = 'CN ' then 'Plastic' else
case when c.iborig = 'NL ' then 'Carton' else '' end end as Material  
from proddta.f4101
left join proddta.f4102 v on v.ibitm = imitm
left join proddta.f564101 on smitm = imitm
left join proddta.f41002 w on w.umitm = imitm and w.umum = 'EA' and w.umrum = 'GM'
left join proddta.f41002 p on p.umitm = imitm and p.umum = 'CR' and p.umrum = 'EA'
left join proddta.f46011 ea on ea.iqitm = imitm and ea.iqmcu = '          SC' and ea.IQUOM = 'EA'
left join proddta.f46011 cr on cr.iqitm = imitm and cr.iqmcu = '          SC' and cr.IQUOM = 'CR'
--left join (SELECT 	CAST (
--		'01-01-' + CAST ( 1900 + CAST( SUBSTRING( CAST( max(dfexdj) AS char ), 1, 3 ) AS int ) AS char ) 
--		AS datetime )
--		+ CAST( SUBSTRING( CAST( max(dfexdj) AS char ), 4, 3 ) AS int ) - 1 as ExpireDateLicense,
--		dfitm
--		from proddta.f38011
--group by dfitm) d on d.dfitm = imitm
left join proddta.f4102 c on c.ibmcu = '          SC' and c.ibitm = imitm
left join proddta.F0006 b on v.ibmcu = b.mcmcu
left join (select  
ixqnty as RelatedQty,
ixlitm as RelatedProductCode,
imdsc1 as RelatedProductDesc,
ixkit
from proddta.f3002
inner join proddta.f4101 on ixitm = imitm
where ixtbm = 'INF') bat on bat.ixkit = imitm
where 
imsrp7 in(
'A', 
'B',
'C',
'N',
'SJ',
'PD',
'U',
'9',
'D'
) 
and 
v.ibmcu in (
'         310'
,'         410'
,'          JS'
,'          FG'
,'          SC' 
)order by categorycode desc, v.ibmcu desc";

      DataSet jdeData = new DataSet();
#if DEBUG
      string path = @"F:\tmp\Jumbo\test.xls";

      using (OleDbConnection con = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=YES;\""))
      {
        con.Open();
        using (System.Data.DataTable exceldt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
        {
          foreach (DataRow row in exceldt.Rows)
          {
            string sheet = row["TABLE_NAME"].ToString();
            OleDbDataAdapter da = null;
            DataTable dt = null;

            da = new OleDbDataAdapter("select * from [" + sheet + "] where barcode IS NOT NULL", con);
            dt = new DataTable();
            da.Fill(dt);
            jdeData.Tables.Add(dt);
          }
        }
      }
#else
      using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["JDE"].ConnectionString))
      {
        conn.Open();
        SqlDataAdapter dataAdapter = new SqlDataAdapter(jdeQuery, conn);
        dataAdapter.Fill(jdeData);
      }
#endif

      using (var unit = GetUnitOfWork())
      {
        List<ProductAttributeMetaData> attributes;
        SetupAttributes(unit, AttributeMapping, out attributes, null);
        var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == DefaultVendorID).ToList();

        var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

        var vendors = unit.Scope.Repository<Vendor>().GetAll().ToList();

        var vendorDic = (from v in jdeData.Tables[0].AsEnumerable()
                         where !string.IsNullOrEmpty(v.Field<string>("ibmcu"))
                         group v by v.Field<string>("ibmcu") into buAss
                         select buAss).ToDictionary(x => x.Key.Trim(), x => x.Select(y => y).ToList());

        //var assortmentList =  List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

        vendorDic.Keys.ForEach((ibmcu, idx) =>
        {
          var vendor = vendors.FirstOrDefault(x => x.VendorID == DefaultVendorID);
          var firstRow = vendorDic[ibmcu].FirstOrDefault();

          if (!string.IsNullOrEmpty(ibmcu))
          {
            vendor = vendors.FirstOrDefault(x => x.BackendVendorCode == ibmcu);

            if (vendor == null)
            {
              vendor = new Vendor()
              {
                VendorType = (int)VendorType.Assortment,
                Name = firstRow.Field<string>("branchName").Trim(),
                Description = firstRow.Field<string>("branchName").Trim(),
                BackendVendorCode = ibmcu,
                IsActive = true,
                ParentVendorID = DefaultVendorID
              };
              unit.Scope.Repository<Vendor>().Add(vendor);
              vendors.Add(vendor);
              unit.Save();
            }

            var dataList = (from d in vendorDic[ibmcu].AsEnumerable()
                            //let barcode = SetDataSetValue("Barcode", d)
                            //where !string.IsNullOrEmpty(barcode)
                            select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
                            {
                              #region BrandVendor
                              BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                          {
                            new VendorAssortmentBulk.VendorImportBrand(){
                              VendorID = DefaultVendorID,
                              VendorBrandCode = SetDataSetValue("BrandCode", d) ?? string.Empty,
                              ParentBrandCode = null,
                              Name = !string.IsNullOrEmpty(SetDataSetValue("Brand", d)) ? SetDataSetValue("Brand", d) : "No Brand",
                            },
                             new VendorAssortmentBulk.VendorImportBrand(){
                              VendorID = DefaultVendorID,
                              VendorBrandCode = SetDataSetValue("SubBrandCode", d) ?? string.Empty,
                               ParentBrandCode = SetDataSetValue("BrandCode", d),
                               Name = !string.IsNullOrEmpty(SetDataSetValue("SubBrand", d)) ? SetDataSetValue("SubBrand", d) : "No Subbrand",                            
                            }
                          },
                              #endregion
                              #region GeneralProductInfo
                              VendorProduct = new VendorAssortmentBulk.VendorProduct
                              {
                                VendorItemNumber = SetDataSetValue("Barcode", d),
                                CustomItemNumber = VendorImportUtility.SetDataSetValue("ProductCode", d),
                                ShortDescription = VendorImportUtility.SetDataSetValue("ProductName", d),
                                LongDescription = null,
                                LineType = null,
                                LedgerClass = null,
                                ProductDesk = null,
                                ExtendedCatalog = null,
                                VendorID = vendor.VendorID,
                                DefaultVendorID = DefaultVendorID,
                                VendorBrandCode = VendorImportUtility.SetDataSetValue("SubBrandCode", d) ?? string.Empty,
                                Barcode = SetDataSetValue("Barcode", d),
                                VendorProductGroupCode1 = VendorImportUtility.SetDataSetValue("CategoryCode", d),
                                VendorProductGroupCodeName1 = SetDataSetValue("Category", d),
                                VendorProductGroupCode2 = VendorImportUtility.SetDataSetValue("SegmentCode", d),
                                VendorProductGroupCodeName2 = SetDataSetValue("Segment", d),
                                VendorProductGroupCode3 = VendorImportUtility.SetDataSetValue("SubBrandCode", d),
                                VendorProductGroupCodeName3 = VendorImportUtility.SetDataSetValue("SubBrand", d) ?? string.Empty,
                                VendorProductGroupCode4 = VendorImportUtility.SetDataSetValue("ConceptCode", d),
                                VendorProductGroupCodeName4 = SetDataSetValue("Concept", d),
                                VendorProductGroupCode5 = SetDataSetValue("CharacterCode", d),
                                VendorProductGroupCodeName5 = !string.IsNullOrEmpty(SetDataSetValue("Character", d)) ? SetDataSetValue("Character", d) : "No Character",
                                VendorProductGroupCode6 = SetDataSetValue("LicenseCode", d),
                                VendorProductGroupCodeName6 = SetDataSetValue("License", d),
                                VendorProductGroupCode7 = SetDataSetValue("PiecesCode", d),
                                VendorProductGroupCodeName7 = SetDataSetValue("Pieces", d),
                                VendorProductGroupCode8 = SetDataSetValue("GenderCode", d),
                                VendorProductGroupCodeName8 = SetDataSetValue("Gender", d),
                                VendorProductGroupCode9 = null,
                                VendorProductGroupCodeName9 = null,
                                //VendorProductGroupCode9 = SetDataSetValue("SubBrandCode", d),
                                //VendorProductGroupCodeName9 = !string.IsNullOrEmpty(SetDataSetValue("SubBrand", d)) ? SetDataSetValue("SubBrand", d) : "No Subbrand",
                                VendorProductGroupCode10 = null,
                                VendorProductGroupCodeName10 = null


                                //    string groupCode1 = SetDataSetValue("CategoryCode", product);
                                //    string groupName1 = SetDataSetValue("Category", product);
                                //    string groupCode2 = SetDataSetValue("SegmentCode", product);
                                //    string groupName2 = SetDataSetValue("Segment", product);
                                //    string groupCode4 = SetDataSetValue("ConceptCode", product);
                                //    string groupName4 = SetDataSetValue("Concept", product);
                                //    string groupCode5 = SetDataSetValue("CharacterCode", product);
                                //    string groupName5 = SetDataSetValue("Character", product);
                                //    string groupCode6 = SetDataSetValue("LicenseCode", product);
                                //    string groupName6 = SetDataSetValue("License", product);
                                //    string groupCode7 = SetDataSetValue("PiecesCode", product);
                                //    string groupName7 = SetDataSetValue("Pieces", product);
                                //    string groupCode8 = SetDataSetValue("GenderCode", product);
                                //    string groupName8 = SetDataSetValue("Gender", product);
                              },
                              #endregion
                              #region RelatedProducts
                              RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                                {
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct{
                                    VendorID = vendor.VendorID,
                                    DefaultVendorID = DefaultVendorID,
                                    CustomItemNumber = VendorImportUtility.SetDataSetValue("ProductCode", d),
                                    RelatedProductType = VendorImportUtility.SetDataSetValue("RelatedProductCode", d) != null ? VendorImportUtility.SetDataSetValue("RelatedProductCode", d).Substring(0,3) : null,
                                    RelatedCustomItemNumber = VendorImportUtility.SetDataSetValue("RelatedProductCode", d)
                                  }
          },
                              #endregion
                              #region Attribures
                              VendorImportAttributeValues = (from attr in AttributeMapping
                                                             let prop = d.Field<object>(attr)
                                                             where prop != null
                                                             let attributeID = attributeList.ContainsKey(attr) ? attributeList[attr] : -1
                                                             let value = prop.ToString()
                                                             where !string.IsNullOrEmpty(value)
                                                             select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                                             {
                                                               VendorID = vendor.VendorID,
                                                               DefaultVendorID = DefaultVendorID,
                                                               CustomItemNumber = VendorImportUtility.SetDataSetValue("ProductCode", d),
                                                               AttributeID = attributeID,
                                                               Value = value,
                                                               LanguageID = null,
                                                               AttributeCode = attr,
                                                             }).ToList(),
                              #endregion
                              #region Prices
                              VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
                              VendorID = vendor.VendorID,
                              DefaultVendorID = DefaultVendorID,
                              CustomItemNumber = VendorImportUtility.SetDataSetValue("ProductCode", d),
                              Price = "0",
                              CostPrice = "0",
                              TaxRate = "19",
                              MinimumQuantity = 0,
                              CommercialStatus = VendorImportUtility.SetDataSetValue("CommercialStatus",d)
                            }
                          },

                              #endregion
                              #region Stock
                              VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock(){
                              VendorID = vendor.VendorID,
                              DefaultVendorID = DefaultVendorID,
                              CustomItemNumber = VendorImportUtility.SetDataSetValue("ProductCode", d),
                              QuantityOnHand = 0,
                              StockType = "Assortment",
                              StockStatus = VendorImportUtility.SetDataSetValue("StockStatus",d)
                            }
                          },

                              #endregion
                            });


              using (var vendorAssortmentBulk = new VendorAssortmentBulk(dataList, vendor.VendorID, DefaultVendorID))
              {
                vendorAssortmentBulk.Init(unit.Context);
                vendorAssortmentBulk.Sync(unit.Context);
              }

            //assortmentList.AddRange(dataList);
          }
        });

        


        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<ProductGroupVendor>(x => x.VendorProductGroupAssortments);
        ////options.LoadWith<VendorAssortment>(x => x.VendorPrice);
        //options.LoadWith<Product>(x => x.ProductBarcodes);
        //options.LoadWith<Product>(x => x.VendorAssortment);
        //options.LoadWith<Product>(x => x.ProductAttributeValues);
        ////options.LoadWith<VendorAssortment>(x => x.VendorStock);
        //options.LoadWith<ProductGroupLanguage>(x => x.ProductGroup);
        //ctx.LoadOptions = options;



        //var vendorRepo = unit.Scope.Repository<Vendor>();
        //var brandRepo = unit.Scope.Repository<Brand>();
        //var brandVendorRepo = unit.Scope.Repository<BrandVendor>();
        //var productRepo = unit.Scope.Repository<Product>();
        //var assortmentRepo = unit.Scope.Repository<VendorAssortment>();
        //var productGroupLanguageRepo = unit.Scope.Repository<ProductGroupLanguage>();
        //var productGroupRepo = unit.Scope.Repository<ProductGroup>();
        //var productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
        //var stockRepo = unit.Scope.Repository<VendorStock>();
        //var relatedProductRepo = unit.Scope.Repository<RelatedProduct>();
        //var attributeValueRepo = unit.Scope.Repository<ProductAttributeValue>();
        //var attributeValues = unit.Scope.Repository<ProductAttributeValue>().GetAll(x => x.ProductAttributeMetaData.VendorID == DefaultVendorID).Select(x => new JumboAttributeValue
        //{
        //  AttributeID = x.AttributeID,
        //  ProductID = x.ProductID,
        //  Value = x.Value,
        //  LanguageID = x.LanguageID
        //}).ToList();



        //ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), DefaultVendorID);

        //var currentProductGroupVendors = unit.Scope.Repository<ProductGroupVendor>().GetAll(v => v.Vendor.VendorID == DefaultVendorID).ToList();

        //var vendorBrands = brandVendorRepo.GetAll(bv => bv.VendorID == DefaultVendorID).ToList();

        //var brands = brandRepo.GetAll().ToList();


        //var productGroups = unit.Scope.Repository<ProductGroupLanguage>().GetAll().ToList();


        //var productGroupVendorRecords = unit.Scope.Repository<ProductGroupVendor>().GetAll(c => c.VendorID == DefaultVendorID).ToList();




        //var productAttributeGroups = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetAll(g => g.VendorID == DefaultVendorID).ToList();

        //var vendorStock = stockRepo.GetAll().Select(x => new JumboVendorStock
        //{
        //  VendorID = x.VendorID,
        //  ProductID = x.ProductID
        //}).ToList();



        //var _vendorProductGroupDictionary = unit.Scope.Repository<VendorAssortment>().Include(x => x.ProductGroupVendors)
        //.GetAll().Select(x => new
        //{
        //  x.VendorAssortmentID,
        //  list = x.ProductGroupVendors.Select(y => y.ProductGroupVendorID)
        //}).ToDictionary(x => x.VendorAssortmentID, y => y.list.ToList());


        //int counter = 0;
        //int total = jdeData.Tables[0].Rows.Count;
        //int totalNumberOfProductsToProcess = total;
        //log.InfoFormat("Start import {0} products", total);


        //var inActiveVendorAssortment = assortmentRepo.GetAll().Select(x => x.VendorAssortmentID).ToList();

        //foreach (var product in jdeData.Tables[0].AsEnumerable())
        //{
        //  try
        //  {
        //    var vendor = vendors.FirstOrDefault(x => x.VendorID == DefaultVendorID);
        //    string ibmcu = product.Field<string>("ibmcu");

        //    if (!string.IsNullOrEmpty(ibmcu))
        //    {
        //      ibmcu = ibmcu.Trim();

        //      vendor = vendors.FirstOrDefault(x => x.BackendVendorCode == ibmcu);

        //      if (vendor == null)
        //      {
        //        vendor = new Vendor()
        //        {
        //          VendorType = (int)VendorType.Assortment,
        //          Name = product.Field<string>("branchName").Trim(),
        //          Description = product.Field<string>("branchName").Trim(),
        //          BackendVendorCode = ibmcu,
        //          IsActive = true,
        //          ParentVendorID = DefaultVendorID
        //        };
        //        vendorRepo.Add(vendor);
        //        vendors.Add(vendor);
        //      }
        //    }

        //    if (counter == 100)
        //    {
        //      counter = 0;
        //      log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
        //      unit.Save();
        //    }
        //    totalNumberOfProductsToProcess--;
        //    counter++;

        //    #region Brand
        //    string brandName = SetDataSetValue("Brand", product);
        //    string brandCode = SetDataSetValue("BrandCode", product);
        //    string subBrandCode = SetDataSetValue("SubBrandCode", product);
        //    string subBrandName = SetDataSetValue("SubBrand", product);

        //    #region Master Brand
        //    var vbrand = vendorBrands.FirstOrDefault(vb => vb.VendorBrandCode == brandCode);

        //    if (vbrand == null)
        //    {
        //      var brand = brands.FirstOrDefault(x => x.Name == brandName);

        //      if (brand == null)
        //      {
        //        brand = new Brand()
        //        {
        //          Name = brandName
        //        };

        //        brandRepo.Add(brand);
        //      }

        //      vbrand = new BrandVendor
        //      {
        //        VendorID = DefaultVendorID,
        //        VendorBrandCode = brandCode,
        //        Brand = brand
        //      };

        //      vendorBrands.Add(vbrand);

        //      brandVendorRepo.Add(vbrand);
        //    }

        //    vbrand.Name = brandName;
        //    #endregion

        //    #region Sub Brand
        //    BrandVendor subBrand = null;
        //    if (!string.IsNullOrEmpty(subBrandCode))
        //    {
        //      subBrand = vendorBrands.FirstOrDefault(vb => vb.VendorBrandCode == subBrandCode);

        //      if (subBrand == null)
        //      {
        //        var brand = brands.FirstOrDefault(x => x.Name == subBrandName && x.ParentBrand == vbrand.Brand);

        //        if (brand == null)
        //        {
        //          brand = new Brand()
        //          {
        //            Name = subBrandName,
        //            ParentBrand = vbrand.Brand
        //          };

        //          brandRepo.Add(brand);
        //        }

        //        subBrand = new BrandVendor
        //        {
        //          VendorID = DefaultVendorID,
        //          VendorBrandCode = subBrandCode,
        //          Brand = brand
        //        };

        //        vendorBrands.Add(subBrand);

        //        brandVendorRepo.Add(subBrand);
        //        unit.Save();
        //      }

        //      subBrand.Name = subBrandName;
        //    }
        //    #endregion

        //    #endregion Brand

        //    #region Product
        //    string barcode = SetDataSetValue("Barcode", product);

        //    if (string.IsNullOrEmpty(barcode))
        //    {
        //      log.DebugFormat("No barcode for {0}", SetDataSetValue("ProductCode", product));
        //      continue;
        //    }

        //    var brandID = (subBrand != null ? subBrand.BrandID : vbrand.Brand.BrandID);

        //    var item = productRepo.GetSingle(p => p.VendorItemNumber == barcode);// && p.BrandID == brandID);
        //    if (item == null)
        //    {
        //      item = new Product
        //      {
        //        VendorItemNumber = barcode,
        //        BrandID = brandID,
        //        SourceVendorID = vendor.VendorID
        //      };
        //      productRepo.Add(item);
        //      unit.Save();
        //    }
        //    else
        //    {
        //      item.BrandID = brandID;
        //    }

        //    #endregion Product

        //    #region Vendor assortment
        //    if (item.VendorAssortments == null) item.VendorAssortments = new List<VendorAssortment>();
        //    var assortment = item.VendorAssortments.FirstOrDefault(va => va.VendorID == vendor.VendorID);
        //    if (assortment == null)
        //    {
        //      assortment = new VendorAssortment
        //      {
        //        Vendor = vendor,
        //        Product = item,
        //        IsActive = true
        //      };
        //      assortmentRepo.Add(assortment);
        //      assortment.CustomItemNumber = SetDataSetValue("ProductCode", product);
        //    }

        //    assortment.ShortDescription = SetDataSetValue("ProductName", product);

        //    inActiveVendorAssortment.Remove(assortment.VendorAssortmentID);
        //    assortment.IsActive = true;

        //    #region Product group

        //    string groupCode1 = SetDataSetValue("CategoryCode", product);
        //    string groupName1 = SetDataSetValue("Category", product);
        //    string groupCode2 = SetDataSetValue("SegmentCode", product);
        //    string groupName2 = SetDataSetValue("Segment", product);
        //    string groupCode4 = SetDataSetValue("ConceptCode", product);
        //    string groupName4 = SetDataSetValue("Concept", product);
        //    string groupCode5 = SetDataSetValue("CharacterCode", product);
        //    string groupName5 = SetDataSetValue("Character", product);
        //    string groupCode6 = SetDataSetValue("LicenseCode", product);
        //    string groupName6 = SetDataSetValue("License", product);
        //    string groupCode7 = SetDataSetValue("PiecesCode", product);
        //    string groupName7 = SetDataSetValue("Pieces", product);
        //    string groupCode8 = SetDataSetValue("GenderCode", product);
        //    string groupName8 = SetDataSetValue("Gender", product);

        //    #region VendorProductGroup1
        //    if (!string.IsNullOrEmpty(groupName1))
        //    {
        //      var vGroup = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode1 == groupCode1.Trim());

        //      var pg1 = productGroups.FirstOrDefault(x => x.Name == groupName1);

        //      if (pg1 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg1 = new ProductGroupLanguage
        //        {
        //          Name = groupName1,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg1);
        //        productGroups.Add(pg1);
        //      }

        //      if (vGroup == null)
        //      {
        //        vGroup = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode1 = groupCode1,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg1.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup);
        //        productGroupVendorRepo.Add(vGroup);
        //        unit.Save();
        //      }
        //      vGroup.VendorName = groupName1;
        //      vGroup.ProductGroupID = pg1.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup2

        //    if (!string.IsNullOrEmpty(groupName2))
        //    {
        //      var vGroup2 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode2 == groupCode2);

        //      var pg2 = productGroups.FirstOrDefault(x => x.Name == groupName2);

        //      if (pg2 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg2 = new ProductGroupLanguage
        //        {
        //          Name = groupName2,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg2);
        //        productGroups.Add(pg2);
        //      }

        //      if (vGroup2 == null)
        //      {
        //        vGroup2 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode2 = groupCode2,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg2.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup2);
        //        productGroupVendorRepo.Add(vGroup2);
        //        unit.Save();
        //      }
        //      vGroup2.VendorName = groupName2;
        //      vGroup2.ProductGroupID = pg2.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup3
        //    if (!string.IsNullOrEmpty(subBrandName))
        //    {
        //      var vGroup3 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode3 == subBrandCode);

        //      var pg3 = productGroups.FirstOrDefault(x => x.Name == subBrandName);
        //      if (pg3 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg3 = new ProductGroupLanguage
        //        {
        //          Name = subBrandName,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg3);
        //        productGroups.Add(pg3);
        //      }

        //      if (vGroup3 == null)
        //      {
        //        vGroup3 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode3 = subBrandCode,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg3.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup3);
        //        productGroupVendorRepo.Add(vGroup3);
        //        unit.Save();
        //      }
        //      vGroup3.VendorName = subBrandName;
        //      vGroup3.ProductGroupID = pg3.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup4
        //    if (!string.IsNullOrEmpty(groupName4))
        //    {
        //      var vGroup4 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode4 == groupCode4);

        //      var pg4 = productGroups.FirstOrDefault(x => x.Name == groupName4);

        //      if (pg4 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg4 = new ProductGroupLanguage
        //        {
        //          Name = groupName4,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg4);
        //        productGroups.Add(pg4);
        //      }

        //      if (vGroup4 == null)
        //      {
        //        vGroup4 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode4 = groupCode4,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg4.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup4);
        //        productGroupVendorRepo.Add(vGroup4);
        //        unit.Save();
        //      }
        //      vGroup4.VendorName = groupName4;
        //      vGroup4.ProductGroupID = pg4.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup5
        //    if (!string.IsNullOrEmpty(groupName5))
        //    {
        //      var vGroup5 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode5 == groupCode5);

        //      var pg5 = productGroups.FirstOrDefault(x => x.Name == groupName5);

        //      if (pg5 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg5 = new ProductGroupLanguage
        //        {
        //          Name = groupName5,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg5);
        //        productGroups.Add(pg5);
        //      }

        //      if (vGroup5 == null)
        //      {
        //        vGroup5 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode5 = groupCode5,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg5.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup5);
        //        productGroupVendorRepo.Add(vGroup5);
        //        unit.Save();
        //      }
        //      vGroup5.VendorName = groupName5;
        //      vGroup5.ProductGroupID = pg5.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup6
        //    if (!string.IsNullOrEmpty(groupName6))
        //    {
        //      var vGroup6 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode6 == groupCode6);

        //      var pg6 = productGroups.FirstOrDefault(x => x.Name == groupName6);

        //      if (pg6 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg6 = new ProductGroupLanguage
        //        {
        //          Name = groupName6,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg6);
        //        productGroups.Add(pg6);
        //      }

        //      if (vGroup6 == null)
        //      {
        //        vGroup6 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode6 = groupCode6,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg6.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup6);
        //        productGroupVendorRepo.Add(vGroup6);
        //        unit.Save();
        //      }
        //      vGroup6.VendorName = groupName6;
        //      vGroup6.ProductGroupID = pg6.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup7
        //    if (!string.IsNullOrEmpty(groupName7))
        //    {
        //      var vGroup7 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode7 == groupCode7);

        //      var pg7 = productGroups.FirstOrDefault(x => x.Name == groupName7);

        //      if (pg7 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg7 = new ProductGroupLanguage
        //        {
        //          Name = groupName7,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg7);
        //        productGroups.Add(pg7);
        //      }

        //      if (vGroup7 == null)
        //      {
        //        vGroup7 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode7 = groupCode7,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg7.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup7);
        //        productGroupVendorRepo.Add(vGroup7);
        //        unit.Save();
        //      }
        //      vGroup7.VendorName = groupName7;
        //      vGroup7.ProductGroupID = pg7.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroup8
        //    if (!string.IsNullOrEmpty(groupName8))
        //    {
        //      var vGroup8 = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode8 == groupCode8);

        //      var pg8 = productGroups.FirstOrDefault(x => x.Name == groupName8);

        //      if (pg8 == null)
        //      {
        //        var pg = new ProductGroup() { Score = 0 };
        //        productGroupRepo.Add(pg);

        //        pg8 = new ProductGroupLanguage
        //        {
        //          Name = groupName8,
        //          ProductGroup = pg,
        //          LanguageID = 1
        //        };
        //        productGroupLanguageRepo.Add(pg8);
        //        productGroups.Add(pg8);
        //      }

        //      if (vGroup8 == null)
        //      {
        //        vGroup8 = new ProductGroupVendor
        //        {
        //          VendorProductGroupCode8 = groupCode8,
        //          VendorID = DefaultVendorID,
        //          ProductGroup = pg8.ProductGroup
        //        };

        //        productGroupVendorRecords.Add(vGroup8);
        //        productGroupVendorRepo.Add(vGroup8);
        //        unit.Save();
        //      }
        //      vGroup8.VendorName = groupName8;
        //      vGroup8.ProductGroupID = pg8.ProductGroupID;
        //    }
        //    #endregion

        //    #region VendorProductGroupBrand
        //    if (!string.IsNullOrEmpty(brandName))
        //    {
        //      var brandGroup = productGroupVendorRecords.FirstOrDefault(pg => pg.BrandCode == brandCode);

        //      if (brandGroup == null)
        //      {
        //        brandGroup = new ProductGroupVendor
        //        {
        //          BrandCode = brandCode,
        //          VendorID = DefaultVendorID,
        //          ProductGroupID = UnMappedID
        //        };

        //        productGroupVendorRecords.Add(brandGroup);
        //        productGroupVendorRepo.Add(brandGroup);
        //      }
        //      brandGroup.VendorName = brandName;
        //    }
        //    #endregion

        //    //#region Sync
        //    //if (currentProductGroupVendors.Contains(vGroup))
        //    //{
        //    //  currentProductGroupVendors.Remove(vGroup);
        //    //}
        //    //#endregion

        //    #endregion Product group

        //    #region Vendor Product Group Assortment

        //    var records = (from l in productGroupVendorRecords
        //                   where
        //                     ((!string.IsNullOrEmpty(brandCode) && l.BrandCode != null &&
        //                        l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
        //                     &&
        //                     ((!string.IsNullOrEmpty(groupCode1) && l.VendorProductGroupCode1 != null &&
        //                       l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
        //                     &&
        //                     ((!string.IsNullOrEmpty(groupCode2) && l.VendorProductGroupCode2 != null &&
        //                       l.VendorProductGroupCode2.Trim() == groupCode2) || l.VendorProductGroupCode2 == null)
        //                     &&
        //                     ((!string.IsNullOrEmpty(subBrandCode) && l.VendorProductGroupCode3 != null &&
        //                       l.VendorProductGroupCode3.Trim() == subBrandCode) || l.VendorProductGroupCode3 == null)
        //                        &&
        //                     ((!string.IsNullOrEmpty(groupCode4) && l.VendorProductGroupCode4 != null &&
        //                       l.VendorProductGroupCode4.Trim() == groupCode4) || l.VendorProductGroupCode4 == null)
        //                        &&
        //                     ((!string.IsNullOrEmpty(groupCode5) && l.VendorProductGroupCode5 != null &&
        //                       l.VendorProductGroupCode5.Trim() == groupCode5) || l.VendorProductGroupCode5 == null)
        //                        &&
        //                     ((!string.IsNullOrEmpty(groupCode6) && l.VendorProductGroupCode6 != null &&
        //                       l.VendorProductGroupCode6.Trim() == groupCode6) || l.VendorProductGroupCode6 == null)
        //                            &&
        //                     ((!string.IsNullOrEmpty(groupCode7) && l.VendorProductGroupCode7 != null &&
        //                       l.VendorProductGroupCode7.Trim() == groupCode7) || l.VendorProductGroupCode7 == null)
        //                            &&
        //                     ((!string.IsNullOrEmpty(groupCode8) && l.VendorProductGroupCode8 != null &&
        //                       l.VendorProductGroupCode8.Trim() == groupCode8) || l.VendorProductGroupCode8 == null)
        //                   select l).ToList();



        //    List<int> existingProductGroupVendors = new List<int>();

        //    foreach (ProductGroupVendor prodGroupVendor in records)
        //    {
        //      existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

        //      if (!_vendorProductGroupDictionary.ContainsKey(assortment.VendorAssortmentID) || prodGroupVendor.VendorAssortments == null)
        //        prodGroupVendor.VendorAssortments = new List<VendorAssortment>();
        //      else
        //      {
        //        if (_vendorProductGroupDictionary[assortment.VendorAssortmentID].Contains(prodGroupVendor.ProductGroupVendorID))
        //          continue;
        //      }

        //      prodGroupVendor.VendorAssortments.Add(assortment);
        //    }

        //    unit.Save();
        //    #endregion

        //    #endregion Vendor assortment

        //    string commercialStatus = SetDataSetValue("CommercialStatus", product);
        //    string stockStatus = SetDataSetValue("StockStatus", product);
        //    #region Stock

        //    var stock = vendorStock.FirstOrDefault(c => c.VendorID == assortment.VendorID && c.ProductID == assortment.ProductID);
        //    if (stock == null)
        //    {
        //      var vstock = new VendorStock
        //      {
        //        VendorID = assortment.VendorID,
        //        Product = item,
        //        VendorStockTypeID = 1,
        //        QuantityOnHand = 0, //TODO
        //        StockStatus = stockStatus,
        //        ConcentratorStatusID = mapper.SyncVendorStatus(stockStatus, -1)
        //      };
        //      stockRepo.Add(vstock);
        //      vendorStock.Add(new JumboVendorStock
        //      {
        //        VendorID = assortment.VendorID,
        //        ProductID = item.ProductID
        //      });
        //    }

        //    #endregion Stock

        //    #region Price
        //    if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
        //    var price = assortment.VendorPrices.FirstOrDefault();
        //    if (price == null)
        //    {
        //      price = new VendorPrice
        //      {
        //        VendorAssortment = assortment,
        //        MinimumQuantity = 0
        //      };

        //      unit.Scope.Repository<VendorPrice>().Add(price);
        //    }
        //    price.CommercialStatus = commercialStatus;
        //    price.BasePrice = 0;
        //    price.ConcentratorStatusID = mapper.SyncVendorStatus(commercialStatus, -1);

        //    #endregion Price

        //    #region ProductBarcode

        //    if (!string.IsNullOrEmpty(barcode))
        //    {

        //      if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
        //      if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == barcode))
        //      {
        //        //create ProductBarcode if not exists
        //        unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
        //        {
        //          Product = item,
        //          Barcode = barcode,
        //          Vendor = vendor,
        //          BarcodeType = (int)BarcodeTypes.Default
        //        });
        //      }
        //    }

        //    #endregion

        //    #region RelatedProducts
        //    string relatedProductCode = SetDataSetValue("RelatedProductCode", product);
        //    string relatedProudctDesc = SetDataSetValue("RelatedProductDesc", product);
        //    string relatedProudctQuantity = SetDataSetValue("RelatedQty", product);

        //    if (!string.IsNullOrEmpty(relatedProductCode))
        //    {
        //      var relatedProduct = assortmentRepo.GetSingle(x => x.CustomItemNumber == relatedProductCode);

        //      if (relatedProduct != null)
        //      {
        //        if (item.RelatedProductsSource == null) item.RelatedProductsSource = new List<RelatedProduct>();
        //        if (!item.RelatedProductsSource.Any(pb => pb.RelatedProductID == relatedProduct.ProductID))
        //        {
        //          var relatedProductType = unit.Scope.Repository<RelatedProductType>().GetSingle(x => x.Type == relatedProductCode.Substring(0, 3));

        //          if (relatedProductType == null)
        //          {
        //            relatedProductType = new RelatedProductType()
        //            {
        //              Type = relatedProductCode.Substring(0, 3)
        //            };
        //            unit.Scope.Repository<RelatedProductType>().Add(relatedProductType);
        //          }

        //          relatedProductRepo.Add(new RelatedProduct
        //           {
        //             SourceProduct = item,
        //             RelatedProductID = relatedProduct.ProductID,
        //             RelatedProductType = relatedProductType,
        //             VendorID = DefaultVendorID
        //           });
        //        }
        //      }
        //    }

        //    #endregion

        //    #region Attributes

        //    var productAttributeValues = attributeValues.Where(x => x.ProductID == item.ProductID && !x.LanguageID.HasValue).ToList();

        //    foreach (var attr in AttributeMapping)
        //    {
        //      var prop = product.Field<object>(attr);
        //      if (prop == null)
        //        prop = string.Empty;

        //      int metaId = -1;
        //      attributeList.TryGetValue(attr, out metaId);

        //      var value = prop.ToString();

        //      if (!string.IsNullOrEmpty(value))
        //      {
        //        var val = productAttributeValues.Where(x => x.AttributeID == metaId).FirstOrDefault();
        //        if (val == null)
        //        {
        //          var pval = new ProductAttributeValue
        //          {
        //            AttributeID = metaId,
        //            ProductID = item.ProductID,
        //            Value = !string.IsNullOrEmpty(value) ? value.Trim() : String.Empty
        //          };
        //          attributeValueRepo.Add(pval);
        //          attributeValues.Add(new JumboAttributeValue { ProductID = item.ProductID, AttributeID = metaId });
        //        }
        //        else
        //        {
        //          if (!string.IsNullOrEmpty(value) && val.Value != value.Trim())
        //          {
        //            var pval = attributeValueRepo.GetSingle(x => x.ProductID == item.ProductID && x.AttributeID == metaId);
        //            pval.Value = value.Trim();
        //          }
        //        }
        //      }
        //    }
        //    #endregion
        //  }
        //  catch (Exception ex)
        //  {
        //    log.ErrorFormat("Error JDE product {0} Import, error:{1},{2}", SetDataSetValue("Barcode", product), ex.StackTrace, ex.InnerException);
        //  }
        //}
        //unit.Save();

        //#region delete unused productgroupvendor records
        ////currentProductGroupVendors.Where(x => x.ProductGroupID == -1).ToList().ForEach(y => productGroupVendorRepo.Delete(y));


        //inActiveVendorAssortment.ForEach(id =>
        //{
        //  var va = unit.Scope.Repository<VendorAssortment>().GetSingle(x => x.VendorAssortmentID == id);

        //  if (va != null)
        //    va.IsActive = false;
        //});

        ////inActiveVendorAssortment.ForEach(x => x.IsActive = false);
        //unit.Save();
        //#endregion


        //}
        //log.Info("Products processing finished. Processed " + (total - totalNumberOfProductsToProcess) + " products");
      }
    }

    private string SetDataSetValue(string name, DataRow row)
    {
      try
      {
        if (row.Field<object>(name) == null)
          return null;
        else
          return !string.IsNullOrEmpty(row.Field<object>(name).ToString()) ? row.Field<object>(name).ToString().Trim() : null;
      }
      catch
      {
        return string.Empty;
      }
    }
  }

  public class JumboAttributeValue
  {
    public int ProductID { get; set; }
    public int AttributeID { get; set; }
    public string Value { get; set; }
    public int? LanguageID { get; set; }
  }

  public class JumboVendorStock
  {
    public int VendorID { get; set; }
    public int ProductID { get; set; }
  }
}

