using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI;
using System.Configuration;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.EDI.Response;

namespace Concentrator.Plugins.EDI
{
  public class ProcessEdiOrderResponses : ConcentratorEDIPlugin
  {
    public override string Name
    {
      get { return "EDI order responses"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var responses = (from o in unit.Scope.Repository<EdiOrderResponseLine>().GetAll()
                      where o.processed == false && (!o.EdiOrderResponse.EdiOrderID.HasValue || o.EdiOrderLine.EdiOrder.Status == 999 || o.EdiOrderResponse.VendorDocumentNumber != "-1")
                      select o.EdiOrderResponse).Distinct().ToList();

        foreach (var response in responses)
        {
          try
          {
            //if(response.EdiOrderID.HasValue)
            //var order = unit.Scope.Repository<EdiOrder>().GetSingle(x => x.EdiOrderID == response.EdiOrderID.Value);

              ediProcessor.GenerateOrderResponse(response, unit, Config);

              response.EdiOrderResponseLines.ForEach((line, idx) =>
              {
                line.processed = true;
              });
              unit.Save();
          }
          catch (Exception ex)
          {
            log.AuditError(string.Format("Generate Reponse failed for order {0}", response.EdiOrderID), ex, "Generate Response EDI orders");

          }
        }
      }
    }

    public override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

  }
}
