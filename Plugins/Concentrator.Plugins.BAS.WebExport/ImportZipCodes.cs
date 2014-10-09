using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Web.ServiceClient.ZipCodeService;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportZipCodes : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website ZipCodes Export Plugin"; }
    }

    protected override void Process()
    {

      foreach (Connector connector in Connectors.Where(x => x.ZipCodes))
      {
        log.DebugFormat("Start Process zipcode import for {0}", connector.Name);
        
        DateTime start = DateTime.Now;

        try
        {
          using (ZipCodesSoapClient soap = new ZipCodesSoapClient())
          {
            using (WebsiteDataContext context = new WebsiteDataContext(ConfigurationManager.ConnectionStrings[connector.Connection].ConnectionString))
            {
              var zips = (from p in context.Postcodes
                          select p).ToList();

              var ms = soap.GetZipcodes();

              Stream s = new MemoryStream(ms);

              XmlSerializer x = new XmlSerializer(typeof(Postcode));
              List<Postcode> importZips = (List<Postcode>)x.Deserialize(s);
              
              foreach (var newzip in importZips)
              {
                var existingVar = (from c in zips
                                   where c.ZipCodeID == newzip.ZipCodeID
                                   select c).FirstOrDefault();

                if (existingVar == null)
                {
                  log.DebugFormat("Try inserting new zipcode {0}", newzip.PCWIJK);
                  Postcode newZipCode = new Postcode
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
                    PCGEMEENTEID = newzip.PCGEMEENTEID,
                    PCGEMEENTENAAM = newzip.PCGEMEENTENAAM,
                    PCPROVINCIE = newzip.PCPROVINCIE.Value,
                    PCCEBUCO = newzip.PCCEBUCO,
                    PCSTREXT = newzip.PCSTREXT
                  };
                  context.Postcodes.InsertOnSubmit(newZipCode);
                }
                else
                {
                  existingVar.PCWIJK = newzip.PCWIJK;
                  existingVar.PCLETTER = newzip.PCLETTER;
                  existingVar.PCREEKSID = newzip.PCREEKSID.Value;
                  existingVar.PCREEKSVAN = newzip.PCREEKSVAN;
                  existingVar.PCREEKSTOT = newzip.PCREEKSTOT;
                  existingVar.PCCITYTPG = newzip.PCCITYTPG;
                  existingVar.PCCITYNEN = newzip.PCCITYNEN;
                  existingVar.PCSTRTPG = newzip.PCSTRTPG;
                  existingVar.PCSTRNEN = newzip.PCSTRNEN;
                  existingVar.PCSTROFF = newzip.PCSTROFF;
                  existingVar.PCCITYEXT = newzip.PCCITYEXT;
                  existingVar.PCGEMEENTEID = newzip.PCGEMEENTEID;
                  existingVar.PCGEMEENTENAAM = newzip.PCGEMEENTENAAM;
                  existingVar.PCPROVINCIE = newzip.PCPROVINCIE.Value;
                  existingVar.PCCEBUCO = newzip.PCCEBUCO;
                  existingVar.PCSTREXT = newzip.PCSTREXT;
                }
                context.SubmitChanges();
              }

            }
          }
        }
        catch (Exception ex)
        {
          log.Fatal(ex);
        }
        
        log.DebugFormat("Finish Process zipcode import for {0}", connector.Name);

      }

    }
  }
}
