using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using Concentrator.Objects;
using System.Data.Linq;
using System.Data.OleDb;
using System.Data;
using System.IO;
using Concentrator.Objects.Utility;
using System.Data.SqlClient;
using System.Web.UI;
using Concentrator.Objects.Excel;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using System.Web.UI.WebControls;
using Concentrator.Objects.ConcentratorService;
using OfficeOpenXml;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.SennHeiser
{
  public class SennHeiserConnectorExcelExport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sennheiser Excel Connector Export"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.ExcelExport)))
        {
          var config = GetConfiguration();

          var query = @"
select 
Artnr,
Product,
BrandName,
Chapter,
Category,
SubCategory,
NEW,
PriceGroup,
[Short Description],
[Long Description],
Specifications,
[NL incl.],
[BE incl.],
[VAT Excl.],
[BE Excl.],
Warranty,
[Product Sheet],
[Fact sheet],
[Instruction for use],
EAN,
Barcode
from (
select
av.VendorItemNumber as Artnr,
pdd.ProductName as Product,
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
(Select top 1 ShortContentDescription from ProductDescription pd where pd.ProductID = av.ProductID)as [Short Description],
(Select top 1 LongContentDescription from ProductDescription pd where pd.ProductID = av.ProductID)as [Long Description],
(select dbo.AttributeString(av.productid, 'TechnicalData', 1)) as [Specifications],
NL.Price as [NL incl.],
BE.Price as [BE incl.],
NL.CostPrice as [VAT Excl.],
BE.CostPrice as [BE Excl.],

(Select top 1 WarrantyInfo from ProductDescription pd where pd.ProductID = av.ProductID)as Warranty,
(Select top 1 mediaurl from productmedia pd where pd.ProductID = av.ProductID and description = 'Product Sheet')as [Product Sheet],
(Select top 1 mediaurl from productmedia pd where pd.ProductID = av.ProductID and description = 'Fact sheet')as [Fact sheet],
(Select top 1 mediaurl from productmedia pd where pd.ProductID = av.ProductID and description = 'Instruction for use')as [Instruction for use],
(Select top 1 Replace(Barcode, ' ', '') from ProductBarcode where BarcodeType = 1 and productid = av.ProductID) as EAN,
(Select top 1 Replace(Barcode, ' ', '') from ProductBarcode where BarcodeType = 2 and productid = av.ProductID) as Barcode

from AssortmentContentView av 

left join (Select va.ProductID,vp.Price,vp.CostPrice,va.isactive from VendorAssortment va
			inner join VendorPrice vp on va.VendorAssortmentID = vp.VendorAssortmentID
			where va.VendorID = 51 and va.isactive = 1) NL on NL.ProductID = av.ProductID
			
left join (Select va.ProductID,vp.Price,vp.CostPrice,va.isactive from VendorAssortment va
			inner join VendorPrice vp on va.VendorAssortmentID = vp.VendorAssortmentID
			where va.VendorID = 52 and va.isactive = 1) BE on BE.ProductID = av.ProductID
			
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
inner join (Select ProductID, ProductName from productdescription where productname is not null
and vendorid < 52) pdd
 on pdd.ProductID = av.ProductID	
where av.ConnectorID = {0} 
and (nl.IsActive = 1 or be.IsActive = 1)
) assortment
where ('{1}' = '0' or brandname = '{1}')
order by BrandScore desc, ChapterScore desc, CategoryScore desc, SubCategoryScore desc, Product
";

          DataSet data = new DataSet();


          using (SqlConnection connection = new SqlConnection(Connection))
          {
            SqlCommand command = connection.CreateCommand();

            command.CommandText = string.Format(query, connector.ConnectorID, 0);

            try
            {
              connection.Open();
              SqlDataAdapter da = new SqlDataAdapter(command);
              da.Fill(data);
            }
            catch (SqlException ex)
            {
              log.AuditError("Update creationtime column failed", ex);
            }
          }

          ExcelPackage package = new ExcelPackage();

          var worksheet = package.Workbook.Worksheets.Add("Sennheiser Price List");

          //Workbook book = new Workbook();

          //Worksheet sheet = book.Worksheets.Add("Sennheiser PriceList");

          //WorksheetRow row;

          //GenerateStyles(book.Styles);

          int cCount = 0;
          //foreach (DataColumn c in data.Tables[0].Columns)
          //{
          //  int maxStringLength = data.Tables[0].AsEnumerable().Max(x => x.ItemArray[cCount].ToString().Trim().Length);
          //  int minLength = 5;
          //  if (maxStringLength < 100)
          //  {
          //    if (maxStringLength < minLength)
          //      sheet.Table.Columns.Add(50);
          //    else
          //      sheet.Table.Columns.Add(maxStringLength * 10);
          //  }
          //  else
          //    sheet.Table.Columns.Add(maxStringLength);

          //  cCount++;
          //}


          string[] columnNames = (from dc in data.Tables[0].Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();

          GenerateHeaders(worksheet, columnNames);

          //int columnCount = GenerateHeaders(sheet, out rowIndex, columnNames);
          int rowCounter = 2;

          foreach (DataRow r in data.Tables[0].Rows)
          {
            //row = sheet.Table.Rows.Add();
            int columnCounter = 1;
            foreach (var v in r.ItemArray)
            {
              worksheet.Cells[rowCounter, columnCounter].Value = v;
              columnCounter++;
            }

            rowCounter++;
          }

          //sheet.Table.ExpandedColumnCount = columnCount;
          //sheet.Table.ExpandedRowCount = rowIndex;


          string outputPath = config.AppSettings.Settings["OutPutPath"].Value;

          string path = Path.Combine(outputPath, connector.Name + "_Export.xls");

          package.SaveAs(new FileInfo(path));

          //.Save(path);

          var brands = (from b in unit.Scope.Repository<ProductGroupMapping>().GetAll(cp => cp.ConnectorID == connector.ConnectorID && !cp.ParentProductGroupMappingID.HasValue)
                        select new {language = b.ProductGroup.ProductGroupLanguages.FirstOrDefault(), b.Score}).Distinct();  
                          
          package = new ExcelPackage();

          //GenerateStyles(book.Styles);

          foreach (var brand in brands.OrderByDescending(x => x.Score))
          {
            var sheet = package.Workbook.Worksheets.Add(brand.language.Name);

            data = new DataSet();

            using (SqlConnection connection = new SqlConnection(Connection))
            {
              SqlCommand command = connection.CreateCommand();

              command.CommandText = string.Format(query, connector.ConnectorID, brand.language.Name);

              try
              {
                connection.Open();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(data);
              }
              catch (SqlException ex)
              {
                log.AuditError("Update creationtime column failed", ex);
              }
            }

            if (data.Tables[0].Rows.Count > 0)
            {
              cCount = 0;
              //foreach (DataColumn c in data.Tables[0].Columns)
              //{
              //  int maxStringLength = data.Tables[0].AsEnumerable().Max(x => x.ItemArray[cCount].ToString().Trim().Length);
              //  int minLength = 5;
              //  if (maxStringLength < 100)
              //  {
              //    if (maxStringLength < minLength)
              //      sheet.Table.Columns.Add(50);
              //    else
              //      sheet.Table.Columns.Add(maxStringLength * 10);
              //  }
              //  else
              //    sheet.Table.Columns.Add(maxStringLength);

              //  cCount++;
              //}

              columnNames = (from dc in data.Tables[0].Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();
              GenerateHeaders(sheet, columnNames);
              var rowIndex = 2;
              foreach (DataRow r in data.Tables[0].Rows)
              {
                var columnConter = 1;
                foreach (var v in r.ItemArray)
                {
                  sheet.Cells[rowIndex, columnConter].Value = v;
                  columnConter++;
                }
                rowIndex++;
              }
            }
          }

          path = Path.Combine(outputPath, "Sennheiser_tab_pricelist_" + connector.Name + "_Export.xls");
          package.SaveAs(new FileInfo(path));
        }

        log.DebugFormat("Get Assortment Xml finished found");
      }
    }

    private void ToExcel(System.Data.DataTable dataTable, string path)
    {
      try
      {
        System.Web.UI.WebControls.DataGrid grid =
              new System.Web.UI.WebControls.DataGrid();
        grid.HeaderStyle.Font.Bold = true;
        grid.DataSource = dataTable;
        grid.DataMember = dataTable.TableName;

        grid.DataBind();

        // render the DataGrid control to a file
        using (StreamWriter sw = new StreamWriter(path))
        {
          using (HtmlTextWriter hw = new HtmlTextWriter(sw))
          {
            grid.RenderControl(hw);
          }
        }
      }
      catch (Exception ex)
      {

      }
    }

    private static void GenerateStyles(ExcelWorksheet sheet)
    {
      sheet.Column(12).Style.Numberformat.Format = "d";
      sheet.Column(13).Style.Numberformat.Format = "d";
      sheet.Column(14).Style.Numberformat.Format = "d";

    }

    //private static void GenerateStyles(WorksheetStyleCollection styles)
    //{
    //  WorksheetStyle Default = styles.Add("Default");
    //  Default.Name = "Normal";
    //  Default.Alignment.Vertical = StyleVerticalAlignment.Bottom;

    //  WorksheetStyle quantity = styles.Add("Number");
    //  quantity.NumberFormat = "0";

    //  WorksheetStyle euro = styles.Add("Euro");
    //  euro.NumberFormat = "\"€\"\\ #,##0.00_-";

    //  WorksheetStyle header = styles.Add("Header");
    //  header.Font.Bold = true;
    //}

    private void GenerateHeaders(ExcelWorksheet sheet, params string[] headers)
    {
      int counter = 1;
      foreach (var header in headers)
      {
        var cell = sheet.Cells[1, counter];
        cell.Value = header;
        cell.Style.Font.Bold = true;
        counter++;
      }
    }


    //private static int GenerateHeaders(Worksheet sheet, out int rowIndex, params string[] headers)
    //{
    //  rowIndex = 1;
    //  WorksheetRow row = sheet.Table.Rows.Add();
    //  row.Index = rowIndex;
    //  row.Cells.Add("Generated at " + DateTime.Now.ToString(), DataType.String, "Default");

    //  row = sheet.Table.Rows.Add();
    //  rowIndex++;
    //  row.Index = rowIndex;

    //  foreach (string header in headers)
    //  {
    //    row.Cells.Add(header, DataType.String, "Header");
    //  }

    //  return headers.Length;
    //}

  }

  public class BrandEqualityComparer : IEqualityComparer<Brand>
  {
    #region IEqualityComparer<Brand> Members

    public bool Equals(Brand x, Brand y)
    {
      return x.BrandID == y.BrandID;
    }

    public int GetHashCode(Brand obj)
    {
      return obj.BrandID;
    }

    #endregion
  }

  public class ExcelModel
  {
    
  }
}


