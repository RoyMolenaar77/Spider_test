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
using System.Drawing;
using Excel;

namespace Concentrator.Plugins.Jumbo
{
  public class ProductGroupImport : JumboBase
  {
    public override string Name
    {
      get { return "Jumbo productgroup import"; }
    }

    private const int UnMappedID = -1;

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var productGroupExcelFile = GetConfiguration().AppSettings.Settings["BrandProductSegmentationList"].Value;

        using (FileStream stream1 = File.Open(productGroupExcelFile, FileMode.Open, FileAccess.Read))
        {
          IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(stream1);
          excelReader1.IsFirstRowAsColumnNames = true;
          DataSet result1 = excelReader1.AsDataSet();

          var productgroups = (from g in result1.Tables[0].AsEnumerable()
                               select new
                               {
                                 Names = g.Field<string>(1)
                               }).Distinct().ToList();

        }
      }
    }
  }
}


