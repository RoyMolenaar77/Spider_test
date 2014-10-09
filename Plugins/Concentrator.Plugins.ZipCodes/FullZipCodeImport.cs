using System;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using System.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Sftp;
using Concentrator.Objects.ZipUtil;
using System.Transactions;
using System.Configuration;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.ZipCodes
{
  public class FullZipCodeImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Full ZipCode import"; }
    }

    protected override void Process()
    {
      log.Info("Post Code import started");
      using (var unit = GetUnitOfWork())
      {
        var config = GetConfiguration().AppSettings.Settings;

        using (StreamReader sr = new StreamReader(config["FullZipPath"].Value))
        {
          int linecount = 0;
          int logline = 0;

          string line;
          var zipcodes = unit.Scope.Repository<ZipCode>().GetAll().ToList();


          while ((line = sr.ReadLine()) != null)
          {
            linecount++;
            if (linecount <= 1 || line.StartsWith("**"))
              continue;

            #region Parse PostCode
            ZipCode newzip = new ZipCode()
            {
              PCWIJK = line.Substring(0, 4).Trim(),
              PCLETTER = line.Substring(4, 2).Trim(),
              PCREEKSID = line.Substring(6, 1).Trim(),
              PCREEKSVAN = string.IsNullOrEmpty(line.Substring(7, 5).Trim()) ? 0 : int.Parse(line.Substring(7, 5).Trim()),
              PCREEKSTOT = string.IsNullOrEmpty(line.Substring(12, 5).Trim()) ? 0 : int.Parse(line.Substring(12, 5).Trim()),
              PCCITYTPG = line.Substring(17, 18).Trim(),
              PCCITYNEN = line.Substring(35, 24).Trim(),
              PCSTRTPG = line.Substring(59, 17).Trim(),
              PCSTRNEN = line.Substring(76, 24).Trim(),
              PCSTROFF = line.Substring(100, 43).Trim(),
              PCCITYEXT = line.Substring(143, 4).Trim(),
              PCSTREXT = line.Substring(147, 5).Trim(),
              PCGEMEENTEID = string.IsNullOrEmpty(line.Substring(152, 4).Trim()) ? 0 : int.Parse(line.Substring(152, 4).Trim()),
              PCGEMEENTENAAM = line.Substring(156, 24).Trim(),
              PCPROVINCIE = line.Substring(180, 1).Trim(),
              PCCEBUCO = string.IsNullOrEmpty(line.Substring(181, 3).Trim()) ? 0 : int.Parse(line.Substring(181, 3).Trim())
            };


            var existingVar = (from c in zipcodes
                               where
                                 c.PCWIJK.Trim() == newzip.PCWIJK
                              && c.PCLETTER.Trim() == newzip.PCLETTER
                              && c.PCREEKSID.Trim() == newzip.PCREEKSID
                              && c.PCREEKSVAN == newzip.PCREEKSVAN
                              && c.PCREEKSTOT == newzip.PCREEKSTOT
                              && c.PCCITYTPG.Trim() == newzip.PCCITYTPG
                              && c.PCCITYNEN.Trim() == newzip.PCCITYNEN
                              && c.PCSTRTPG.Trim() == newzip.PCSTRTPG
                              && c.PCSTRNEN.Trim() == newzip.PCSTRNEN
                              && c.PCSTROFF.Trim() == newzip.PCSTROFF
                              && c.PCCITYEXT.Trim() == newzip.PCCITYEXT
                              && c.PCSTREXT.Trim() == newzip.PCSTREXT
                              && c.PCGEMEENTEID == newzip.PCGEMEENTEID
                              && c.PCGEMEENTENAAM.Trim() == newzip.PCGEMEENTENAAM
                              && c.PCPROVINCIE.Trim() == newzip.PCPROVINCIE
                              && c.PCCEBUCO == newzip.PCCEBUCO
                               select c).FirstOrDefault();

            if (existingVar == null)
            {
              existingVar = new ZipCode()
                              {
                                PCWIJK = newzip.PCWIJK,
                                PCLETTER = newzip.PCLETTER,
                                PCREEKSID = newzip.PCREEKSID,
                                PCREEKSVAN = newzip.PCREEKSVAN,
                                PCREEKSTOT = newzip.PCREEKSTOT,
                                PCCITYTPG = newzip.PCCITYTPG,
                                PCCITYNEN = newzip.PCCITYNEN,
                                PCSTRTPG = newzip.PCSTRTPG,
                                PCSTRNEN = newzip.PCSTRNEN,
                                PCSTROFF = newzip.PCSTROFF,
                                PCCITYEXT = newzip.PCCITYEXT,
                                PCSTREXT = newzip.PCSTREXT,
                                PCGEMEENTEID = newzip.PCGEMEENTEID,
                                PCGEMEENTENAAM = newzip.PCGEMEENTENAAM,
                                PCPROVINCIE = newzip.PCPROVINCIE,
                                PCCEBUCO = newzip.PCCEBUCO
                              };
              unit.Scope.Repository<ZipCode>().Add(existingVar);
              log.DebugFormat("Add new zipcode {0}{1} line {2}", newzip.PCWIJK, newzip.PCLETTER, linecount);
              unit.Save();
              zipcodes.Add(existingVar);
            }

            #endregion
          }
        }
      }
      log.AuditSuccess("Post Code import finished", "Post Code Import");
    }
  }
}
