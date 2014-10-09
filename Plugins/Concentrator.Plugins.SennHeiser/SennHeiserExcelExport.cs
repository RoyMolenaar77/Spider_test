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
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.SennHeiser
{
  public class SennheiserExcelExport : ConcentratorPlugin
  {
    private const int connectorID = 1;

    public override string Name
    {
      get { return "Sennheiser Excel Export"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var config = GetConfiguration();
        List<ExcelColumn> columns = new List<ExcelColumn>();

        columns.Add(new ExcelColumn("Product", "ProductName", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("ArtNr", "ManufacturerID", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("Chapter", "Name", SourceValueType.XmlAttribute, "ProductGroup/Name", 2));
        columns.Add(new ExcelColumn("Category", "ProductName", SourceValueType.XmlAttribute, "ProductGroup/Name", 1));
        columns.Add(new ExcelColumn("SubCategory", "ProductName", SourceValueType.XmlAttribute, "ProductGroup/Name", 0));
        columns.Add(new ExcelColumn("Group", "PriceGroup", SourceValueType.AttributeName));
        columns.Add(new ExcelColumn("New", "CommercialStatus", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("Description 1", "ShortDescription", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("Specifications short", "LongDescription", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("BELUX incl.", "TaxRate", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("NL incl.", "TaxRate", SourceValueType.XmlAttribute));
        columns.Add(new ExcelColumn("VAT EXCL", "UnitPrice", SourceValueType.XmlElement));
        columns.Add(new ExcelColumn("PriceChange", "PriceChange", SourceValueType.XmlAttribute));

        var query = @"select 
va.CustomItemNumber as Product,
p.VendorItemNumber as Artnr,
(Select top 1 VendorProductGroupCode1 from ProductGroupVendor pgv
inner join VendorProductGroupAssortment vpga on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
where VendorProductGroupCode1 is not null and vpga.VendorAssortmentID = va.VendorAssortmentID) as Chapter,
(Select top 1 VendorProductGroupCode2 from ProductGroupVendor pgv
inner join VendorProductGroupAssortment vpga on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
where VendorProductGroupCode2 is not null and vpga.VendorAssortmentID = va.VendorAssortmentID) as Category,
(Select top 1 VendorProductGroupCode3 from ProductGroupVendor pgv
inner join VendorProductGroupAssortment vpga on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
where VendorProductGroupCode3 is not null and vpga.VendorAssortmentID = va.VendorAssortmentID) as Subcategory,
(Select top 1 value from ProductAttributeValue pav 
	inner join ProductAttributeMetaData pamd on pav.AttributeID = pamd.AttributeID 
	where pamd.AttributeCode = 'New' and pav.ProductID = va.ProductID) as NEW,
va.ShortDescription as [Description 1],
va.LongDescription as [Specification short],
vp.Price as [NL incl],
(select vp2.Price from VendorAssortment va2
inner join VendorPrice vp2 on va2.VendorAssortmentID = vp2.VendorAssortmentID
where va2.VendorID = 52 and va2.ProductID = va.productid) as [BE incl.],
vp.CostPrice as [VAT Excl]
from VendorAssortment va
inner join Product p on va.ProductID = p.ProductID
inner join VendorPrice vp on vp.VendorAssortmentID = va.VendorAssortmentID
where va.VendorID = 51";

        DataSet data = new DataSet();
        using (SqlConnection connection = new SqlConnection(Connection))
        {
          SqlCommand command = connection.CreateCommand();

          command.CommandText = query;
          //log.DebugFormat("Try execute: {0}", command.CommandText);

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


        //var book = ExcelHelper.GenerateExcel(1, columns, false);
        //book.Save(Path.Combine(config.AppSettings.Settings["OutPutPath"].Value, "Product_Export_" + DateTime.Now.ToString("dd-MM-YYYY-HHmm") + ".xls"));
        string path = Path.Combine(config.AppSettings.Settings["OutPutPath"].Value, "Product_Export_" + DateTime.Now.ToString("dd-MM-yyyy-HHmm") + ".xls");
        ToExcel(data.Tables[0], path);
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

  }
}
