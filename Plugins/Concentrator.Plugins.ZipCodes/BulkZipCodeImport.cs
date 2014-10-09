using System;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using System.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Sftp;
using Concentrator.Objects.ZipUtil;
using System.Transactions;
using System.Configuration;
using System.Data.SqlClient;
using Concentrator.Objects.Environments;
using System.Data;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace Concentrator.Plugins.ZipCodes
{
  public class BulkZipCodeImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Bulk ZipCode import"; }
    }

    protected override void Process()
    {
      log.Info("Bulk Zipcode import started");
      using (var uni = GetUnitOfWork())
      {
        var config = GetConfiguration().AppSettings.Settings;

        //using (StreamReader sr = new StreamReader(config["FullZipPath"].Value))
        //{
        //  int linecount = 0;
        //  int logline = 0;

        //  string line;
        //  var zipcodes = (from z in dbcontext.ZipCodes
        //                  select z).ToList();

        //  while ((line = sr.ReadLine()) != null)
        //  {
        //    linecount++;
        //    if (linecount <= 1 || line.StartsWith("**"))
        //      continue;

        //    #region Parse PostCode
        //    Concentrator.Objects.Vendors.ZipCode newzip = new Concentrator.Objects.Vendors.ZipCode()
        //    {
        //      PCWIJK = line.Substring(0, 4).Trim(),
        //      PCLETTER = line.Substring(4, 2).Trim(),
        //      PCREEKSID = line.Substring(6, 1).Trim(),
        //      PCREEKSVAN = string.IsNullOrEmpty(line.Substring(7, 5).Trim()) ? 0 : int.Parse(line.Substring(7, 5).Trim()),
        //      PCREEKSTOT = string.IsNullOrEmpty(line.Substring(12, 5).Trim()) ? 0 : int.Parse(line.Substring(12, 5).Trim()),
        //      PCCITYTPG = line.Substring(17, 18).Trim(),
        //      PCCITYNEN = line.Substring(35, 24).Trim(),
        //      PCSTRTPG = line.Substring(59, 17).Trim(),
        //      PCSTRNEN = line.Substring(76, 24).Trim(),
        //      PCSTROFF = line.Substring(100, 43).Trim(),
        //      PCCITYEXT = line.Substring(143, 4).Trim(),
        //      PCSTREXT = line.Substring(147, 5).Trim(),
        //      PCGEMEENTEID = string.IsNullOrEmpty(line.Substring(152, 4).Trim()) ? 0 : int.Parse(line.Substring(152, 4).Trim()),
        //      PCGEMEENTENAAM = line.Substring(156, 24).Trim(),
        //      PCPROVINCIE = line.Substring(180, 1).Trim(),
        //      PCCEBUCO = string.IsNullOrEmpty(line.Substring(181, 3).Trim()) ? 0 : int.Parse(line.Substring(181, 3).Trim())
        //    };


        //    var existingVar = (from c in zipcodes
        //                       where
        //                         c.PCWIJK.Trim() == newzip.PCWIJK
        //                      && c.PCLETTER.Trim() == newzip.PCLETTER
        //                      && c.PCREEKSID.Trim() == newzip.PCREEKSID
        //                      && c.PCREEKSVAN == newzip.PCREEKSVAN
        //                      && c.PCREEKSTOT == newzip.PCREEKSTOT
        //                      && c.PCCITYTPG.Trim() == newzip.PCCITYTPG
        //                      && c.PCCITYNEN.Trim() == newzip.PCCITYNEN
        //                      && c.PCSTRTPG.Trim() == newzip.PCSTRTPG
        //                      && c.PCSTRNEN.Trim() == newzip.PCSTRNEN
        //                      && c.PCSTROFF.Trim() == newzip.PCSTROFF
        //                      && c.PCCITYEXT.Trim() == newzip.PCCITYEXT
        //                      && c.PCSTREXT.Trim() == newzip.PCSTREXT
        //                      && c.PCGEMEENTEID == newzip.PCGEMEENTEID
        //                      && c.PCGEMEENTENAAM.Trim() == newzip.PCGEMEENTENAAM
        //                      && c.PCPROVINCIE.Trim() == newzip.PCPROVINCIE
        //                      && c.PCCEBUCO == newzip.PCCEBUCO
        //                       select c).FirstOrDefault();

        //    if (existingVar == null)
        //    {
        //      existingVar = new Concentrator.Objects.Vendors.ZipCode()
        //                      {
        //                        PCWIJK = newzip.PCWIJK,
        //                        PCLETTER = newzip.PCLETTER,
        //                        PCREEKSID = newzip.PCREEKSID,
        //                        PCREEKSVAN = newzip.PCREEKSVAN,
        //                        PCREEKSTOT = newzip.PCREEKSTOT,
        //                        PCCITYTPG = newzip.PCCITYTPG,
        //                        PCCITYNEN = newzip.PCCITYNEN,
        //                        PCSTRTPG = newzip.PCSTRTPG,
        //                        PCSTRNEN = newzip.PCSTRNEN,
        //                        PCSTROFF = newzip.PCSTROFF,
        //                        PCCITYEXT = newzip.PCCITYEXT,
        //                        PCSTREXT = newzip.PCSTREXT,
        //                        PCGEMEENTEID = newzip.PCGEMEENTEID,
        //                        PCGEMEENTENAAM = newzip.PCGEMEENTENAAM,
        //                        PCPROVINCIE = newzip.PCPROVINCIE,
        //                        PCCEBUCO = newzip.PCCEBUCO
        //                      };
        //      dbcontext.ZipCodes.InsertOnSubmit(existingVar);
        //      log.DebugFormat("Add new zipcode {0}{1} line {2}", newzip.PCWIJK, newzip.PCLETTER, linecount);
        //      dbcontext.SubmitChanges();
        //      zipcodes.Add(existingVar);
        //    }

        //    #endregion
        //  }
        //}




        SqlBulkCopy copy = new SqlBulkCopy(Environments.Current.Connection);
        copy.BatchSize = 10000;
        copy.BulkCopyTimeout = 300;
        copy.DestinationTableName = "[ZipCode]";
        copy.NotifyAfter = 1000;
        copy.SqlRowsCopied += (s, e) => log.DebugFormat("Processed {0} rows", e.RowsCopied);

        using (var fileStream = new FileStream(config["FullZipPath"].Value, FileMode.Open, FileAccess.Read, FileShare.Read, 5120, FileOptions.SequentialScan))
        {

          List<DataRow> dataRows = new List<DataRow>();

          using (StreamReader sr = new StreamReader(fileStream))
          {
            int linecount = 0;
            int logline = 0;

            string line;

            DataTable table = new DataTable();
            table.Columns.Add("PCWIJK");
            table.Columns.Add("PCLETTER");
            table.Columns.Add("PCREEKSID");
            table.Columns.Add("PCREEKSVAN");
            table.Columns.Add("PCREEKSTOT");
            table.Columns.Add("PCCITYTPG");
            table.Columns.Add("PCCITYNEN");
            table.Columns.Add("PCSTRTPG");
            table.Columns.Add("PCSTRNEN");
            table.Columns.Add("PCSTROFF");
            table.Columns.Add("PCCITYEXT");
            table.Columns.Add("PCSTREXT");
            table.Columns.Add("PCGEMEENTEID");
            table.Columns.Add("PCGEMEENTENAAM");
            table.Columns.Add("PCPROVINCIE");
            table.Columns.Add("PCCEBUCO");

            while ((line = sr.ReadLine()) != null)
            {
              linecount++;
              if (linecount <= 1 || line.StartsWith("**"))
                continue;

              List<string> items = new List<string>();

              items.Add(line.Substring(0, 4).Trim());
              items.Add(line.Substring(4, 2).Trim());
              items.Add(line.Substring(6, 1).Trim());
              items.Add(string.IsNullOrEmpty(line.Substring(7, 5).Trim()) ? "0" : line.Substring(7, 5).Trim());
              items.Add(string.IsNullOrEmpty(line.Substring(12, 5).Trim()) ? "0" : line.Substring(12, 5).Trim());
              items.Add(line.Substring(17, 18).Trim());
              items.Add(line.Substring(35, 24).Trim());
              items.Add(line.Substring(59, 17).Trim());
              items.Add(line.Substring(76, 24).Trim());
              items.Add(line.Substring(100, 43).Trim());
              items.Add(line.Substring(143, 4).Trim());
              items.Add(line.Substring(147, 5).Trim());
              items.Add(string.IsNullOrEmpty(line.Substring(152, 4).Trim()) ? "0" : line.Substring(152, 4).Trim());
              items.Add(line.Substring(156, 24).Trim());
              items.Add(line.Substring(180, 1).Trim());
              items.Add(string.IsNullOrEmpty(line.Substring(181, 3).Trim()) ? "0" : line.Substring(181, 3).Trim());

              DataRow row = table.NewRow();
              row.ItemArray = items.ToArray();
              table.Rows.Add(row);
            }



            copy.ColumnMappings.Add(0, "PCWIJK");
            copy.ColumnMappings.Add(1, "PCLETTER");
            copy.ColumnMappings.Add(2, "PCREEKSID");
            copy.ColumnMappings.Add(3, "PCREEKSVAN");
            copy.ColumnMappings.Add(4, "PCREEKSTOT");
            copy.ColumnMappings.Add(5, "PCCITYTPG");
            copy.ColumnMappings.Add(6, "PCCITYNEN");
            copy.ColumnMappings.Add(7, "PCSTRTPG");
            copy.ColumnMappings.Add(8, "PCSTRNEN");
            copy.ColumnMappings.Add(9, "PCSTROFF");
            copy.ColumnMappings.Add(10, "PCCITYEXT");
            copy.ColumnMappings.Add(11, "PCSTREXT");
            copy.ColumnMappings.Add(12, "PCGEMEENTEID");
            copy.ColumnMappings.Add(13, "PCGEMEENTENAAM");
            copy.ColumnMappings.Add(14, "PCPROVINCIE");
            copy.ColumnMappings.Add(15, "PCCEBUCO");
            copy.WriteToServer(table);

          }
        }


      }
      log.AuditSuccess("Post Code import finished", "Post Code Import");
    }
  }
}
