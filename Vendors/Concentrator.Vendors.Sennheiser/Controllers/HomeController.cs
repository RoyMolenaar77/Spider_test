using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Vendors.Sennheiser.Models;
using System.Configuration;
using Concentrator.Objects.Utility;
using System.IO;
using Concentrator.Objects;
using Concentrator.Objects.Images;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Vendors.Sennheiser.Controllers
{
  public class HomeController : Controller
  {
    #region Query
    private string query = @" select * from 
(select distinct
av.productid as ConcentratorProductID,
isnull((Select top 1 ProductName from ProductDescription pd where pd.ProductID = av.ProductID), av.CustomItemNumber) as ProductID,
av.VendorItemNumber as Artnr,
 (
				CASE
				WHEN brandname.name is not null then brandname.name else
					CASE WHEN brandname.name is null and chapter.name  is not null then chapter.name  else
						CASE WHEN brandname.name is null and chapter.name  is null and category.name is not null then category.name else
							CASE WHEN brandname.name is null and chapter.name  is null and category.name is null and subcategory.name is not null then subcategory.name
								ELSE
									NULL								
							END 
						END
					END
				END
				) as BrandName,
				(CASE
				WHEN brandname.Name is not null then brandname.Score else
					CASE WHEN brandname.Name is null and chapter.Name  is not null then chapter.Score  else
						CASE WHEN brandname.Name is null and chapter.Name  is null and category.Name is not null then category.Score else
							CASE WHEN brandname.Name is null and chapter.Name  is null and category.Name is null and subcategory.Name is not null then subcategory.Score
								ELSE
									0								
							END 
						END
					END
				END
				) as BrandScore,
 (
			CASE WHEN chapter.name  is not null and brandname.name is not null and category.name is not null and subcategory.name is not null then chapter.name else
				CASE WHEN chapter.name  is null and category.name is not null and subcategory.name is not null then subcategory.name else
					CASE WHEN brandname.name is null and chapter.name  is not null and category.name is not null and subcategory.name is not null then category.name else
					NULL
					END
				END
			END
	) as Chapter ,
	(
			CASE WHEN chapter.name  is not null and brandname.name is not null and category.name is not null and subcategory.name is not null then chapter.Score else
				CASE WHEN chapter.name  is null and category.name is not null and subcategory.name is not null then subcategory.Score else
					CASE WHEN brandname.name is null and chapter.name  is not null and category.name is not null and subcategory.name is not null then category.Score else
					0
					END
				END
			END
	) as ChapterScore ,
	
	(
		CASE WHEN chapter.name is not null and brandname.name is not null and category.name is not null and subcategory.name is not null then category.name else
			CASE WHEN brandname.name is null and chapter.name is not null and category.name is not null and subcategory.name is not null then subcategory.name else
			NULL
			END
		END
	) as Category,
	(
		CASE WHEN chapter.name is not null and brandname.name is not null and category.name is not null and subcategory.name is not null then category.Score else
			CASE WHEN brandname.name is null and chapter.name is not null and category.name is not null and subcategory.name is not null then subcategory.Score else
			0
			END
		END
	) as CategoryScore,
	
	 (
	CASE WHEN chapter.name  is not null and brandname.name is not null and category.name is not null and subcategory.name is not null then subcategory.name 
	ELSE
		NULL
	END
	) as SubCategory,
	 (
	CASE WHEN chapter.name  is not null and brandname.name is not null and category.name is not null and subcategory.name is not null then subcategory.Score 
	ELSE
		0
	END
	) as SubCategoryScore,

(Select top 1 value from ProductAttributeValue pav 
  inner join ProductAttributeMetaData pamd on pav.AttributeID = pamd.AttributeID 
  where pamd.AttributeCode = 'New' and pav.ProductID = av.ProductID) as NEW,
  (Select top 1 value from ProductAttributeValue pav 
  inner join ProductAttributeMetaData pamd on pav.AttributeID = pamd.AttributeID 
  where pamd.AttributeCode = 'PriceGroup' and pav.ProductID = av.ProductID) as PriceGroup,
isnull((Select top 1 ShortContentDescription from ProductDescription pd where pd.ProductID = av.ProductID), '') as [Description1],
--(Select top 1 LongContentDescription from ProductDescription pd where pd.ProductID = av.ProductID)as [Description2],
'' as [Description2],
--(select dbo.AttributeString(av.productid)) as Features,
(select dbo.AttributeString(av.productid, 'TechnicalData', 1)) as Features,
Cast(NL.Price as decimal(18,2)) as PriceNL,
Cast(BE.Price as decimal(18,2)) as PriceBE,
Cast(NL.CostPrice as decimal(18,2)) as VatExcl,
Cast(BE.CostPrice as decimal(18,2)) as BEVatExcl,
(Select top 1 WarrantyInfo from ProductDescription pd where pd.ProductID = av.ProductID)as Warranty,
(Select top 1 mediaurl from productmedia pd where pd.ProductID = av.ProductID and description = 'Product Sheet')as [Product Sheet],
(Select top 1 mediaurl from productmedia pd where pd.ProductID = av.ProductID and description = 'Fact sheet')as [Fact sheet],
(Select top 1 mediaurl from productmedia pd where pd.ProductID = av.ProductID and description = 'Instruction for use')as [Instruction for use],
(Select top 1 Replace(Barcode, ' ', '') from ProductBarcode where BarcodeType = 1 and productid = av.ProductID) as EAN,
(Select top 1 Replace(Barcode, ' ', '') from ProductBarcode where BarcodeType = 2 and productid = av.ProductID) as Barcode,
(select top 1 mediapath from productmedia where productid = av.productid and typeid in (1,4,9,10) and mediapath is not null order by Sequence )  as ProductImage
--chapter.Score,
--category.Score,
--subcategory.Score
from AssortmentContentView av 
inner join ContentProductGroup cpg on av.ProductID = cpg.ProductID and av.ConnectorID = cpg.ConnectorID
--inner join ProductGroupMapping subcategory on subcategory.ProductGroupMappingID = cpg.ProductGroupMappingID
--left join ProductGroupMapping category on category.ProductGroupMappingID = subcategory.ParentProductGroupMappingID
--left join ProductGroupMapping chapter on chapter.ProductGroupMappingID = category.ParentProductGroupMappingID
--left join ProductGroupLanguage subcategoryName on subcategoryName.ProductGroupID = subcategory.ProductGroupID and subcategoryName.LanguageID = 1
--left join ProductGroupLanguage categoryName on categoryName.ProductGroupID = category.ProductGroupID and categoryName.LanguageID = 1
--left join ProductGroupLanguage chapterName on chapterName.ProductGroupID = chapter.ProductGroupID and chapterName.LanguageID = 1
inner join (select cpg.productid,cpg.connectorid,pgl.name, pgm.parentproductgroupmappingid,pgm.Score from contentproductgroup cpg 
			inner join productgroupmapping pgm on cpg.productgroupmappingid = pgm.productgroupmappingid
			inner join productgrouplanguage pgl on pgm.productgroupid = pgl.productgroupid
			where pgl.languageid = 1) subcategory on subcategory.productid = av.productid and subcategory.connectorid = av.connectorid
			
left join (select pgl.name,pgm.productgroupmappingid, pgm.parentproductgroupmappingid,pgm.Score from productgroupmapping pgm
			inner join productgrouplanguage pgl on pgm.productgroupid = pgl.productgroupid
			where pgl.languageid = 1) category on category.productgroupmappingid = subcategory.parentproductgroupmappingid
			
left join (select pgl.name,pgm.productgroupmappingid, pgm.parentproductgroupmappingid, pgm.Score from productgroupmapping pgm
			inner join productgrouplanguage pgl on pgm.productgroupid = pgl.productgroupid
			where pgl.languageid = 1) chapter on chapter.productgroupmappingid = category.parentproductgroupmappingid
			
left join (select pgl.name,pgm.productgroupmappingid, pgm.parentproductgroupmappingid, pgm.Score from productgroupmapping pgm
			inner join productgrouplanguage pgl on pgm.productgroupid = pgl.productgroupid
			where pgl.languageid = 1) brandname on chapter.parentproductgroupmappingid = brandname.productgroupmappingid
			
left join (Select va.ProductID,vp.Price,vp.CostPrice,va.longdescription,va.isactive from VendorAssortment va
      inner join VendorPrice vp on va.VendorAssortmentID = vp.VendorAssortmentID
      where va.VendorID = 51 and va.isactive = 1) NL on NL.ProductID = av.ProductID
left join (Select va.ProductID,vp.Price,vp.CostPrice,va.isactive from VendorAssortment va
      inner join VendorPrice vp on va.VendorAssortmentID = vp.VendorAssortmentID
      where va.VendorID = 52 and va.isactive = 1) BE on BE.ProductID = av.ProductID
where av.ConnectorID = 2 and av.CustomItemNumber not like 'Â»%' 
and (NL.Isactive = 1 or BE.Isactive = 1)
and (NL.Price is not null or BE.Price is not null)
) a
{0}
order by BrandScore desc, ChapterScore desc, CategoryScore desc, SubCategoryScore desc, productid";
    #endregion

    public ActionResult Index(int? brandID)
    {
      ViewBag.Message = "Welcome to ASP.NET MVC!";
      if(brandID.HasValue)
        ViewBag.Products = PrintPage.GeneratePages(GetBrandPriceList(brandID.Value), 11);
      else
      ViewBag.Products = PrintPage.GeneratePages(GetPriceList(), 11);

      return View();
    }

    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult PrintPriceList()
    {
      return new PdfResult();
    }

    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult PrintBrandPriceList(int brandID)
    {
      var result = new PdfResult();
      result.BrandID = brandID;

      return result;
    }

    public ActionResult About()
    {
      return View();
    }

//    av.BrandName,
//chapterName.Name as Chapter,
//categoryName.Name as Category,
//subcategoryName.Name as Subcategory,

    private List<PriceListModel> GetPriceList()
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        var result = unit.ExecuteStoreQuery<PriceListModel>(string.Format(query, string.Empty)).ToList();        

        int picSize = 50;
        int.TryParse(ConfigurationManager.AppSettings["PicSize"], out picSize);

        var products = (from c in result
                        let attributes = unit.Scope.Repository<ProductAttributeValue>().GetAll(a => a.ProductID == c.ConcentratorProductID && a.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.Any(g => g.Name == "TechnicalData")).ToList().Take(5)
                        let values = attributes.Select(l => l.Value).ToList()
                        select new PriceListModel
                        {
                          ConcentratorProductID = c.ConcentratorProductID,
                          ProductID = c.ProductID,
                          Artnr = c.Artnr,
                          BrandName = c.BrandName,
                          Chapter = c.Chapter,
                          Category = c.Category,
                          BEVatExcl = c.BEVatExcl,
                          Subcategory = c.Subcategory,
                          PriceGroup = c.PriceGroup,
                          Description1 = c.Description1,
                          Description2 = c.Description2,
                          PriceNL = c.PriceNL,
                          PriceBE = c.PriceBE,
                          VatExcl = c.VatExcl,
                          Image = Url.Action("FetchImage", "Home", new { path = c.ProductImage }),
                          // Image = c.ProductImage != null ? ImageUtility.LoadToBase64(Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], c.ProductImage), picSize, picSize) : string.Empty,
                          Features = String.Join(" • ", values),
                          New = c.New
                        }).ToList();


        return products;
      }
    }

    private List<PriceListModel> GetBrandPriceList(int brandID)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        var result = unit.ExecuteStoreQuery<PriceListModel>(string.Format(query, "where brandname = (select name from Brand where BrandID = " + brandID + ")")).ToList();
        
        int picSize = 70;
        int.TryParse(ConfigurationManager.AppSettings["PicSize"], out picSize);

        var products = (from c in result
                        let attributes = unit.Scope.Repository<ProductAttributeValue>().GetAll(a => a.ProductID == c.ConcentratorProductID && a.ProductAttributeMetaData.ProductAttributeGroupMetaData.ProductAttributeGroupNames.Any(g => g.Name == "TechnicalData")).ToList().Take(5)
                        let values = attributes.Select(l => l.Value).ToList()
                        select new PriceListModel
                        {
                          ConcentratorProductID = c.ConcentratorProductID,
                          ProductID = c.ProductID,
                          Artnr = c.Artnr,
                          BrandName = c.BrandName,
                          Chapter = c.Chapter,
                          Category = c.Category,
                          BEVatExcl = c.BEVatExcl,
                          Subcategory = c.Subcategory,
                          PriceGroup = c.PriceGroup,
                          Description1 = c.Description1,
                          Description2 = c.Description2,
                          PriceNL = c.PriceNL,
                          PriceBE = c.PriceBE,
                          VatExcl = c.VatExcl,
                          Image = Url.Action("FetchImage", "Home", new { path = c.ProductImage }),
                          // Image = c.ProductImage != null ? ImageUtility.LoadToBase64(Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], c.ProductImage), picSize, picSize) : string.Empty,
                          Features = String.Join(" • ", values),
                          New = c.New
                        }).ToList();


        return products;
      }
    }

    public void FetchImage(string path)
    {

      Uri baseUri = new Uri(ConfigurationManager.AppSettings["baseUri"].ToString());

      path = path.IfNullOrEmpty("Media\\unknown.png"); //temp

      Uri uri = new Uri(baseUri, path);


      Image img = Image.FromStream(WebRequest.Create(uri).GetResponse().GetResponseStream());

      int picSize = 70;
      int.TryParse(ConfigurationManager.AppSettings["PicSize"], out picSize);


      string extension = path.Substring(path.LastIndexOf('.') + 1);


      ImageFormat _format = ImageFormat.Jpeg;
      switch (extension.ToLower())
      {
        case "jpg":
        case "jpeg":
          _format = ImageFormat.Jpeg;
          Response.ContentType = "image/jpeg";
          break;
        case "png":
        case "tif":
          _format = ImageFormat.Png;
          Response.ContentType = "image/png";

          break;
        case "gif":
          _format = ImageFormat.Gif;
          Response.ContentType = "image/gif";

          break;
        // for unknown types
        default:

          _format = ImageFormat.Png;
          Response.ContentType = "image/png";

          break;

      }
      var bmp = ImageUtility.GetFixedSizeImage(img, picSize, picSize, true, Color.White);
      bmp.Save(Response.OutputStream, _format);
    }
  }
}
