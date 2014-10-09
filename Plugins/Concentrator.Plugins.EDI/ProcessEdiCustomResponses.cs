using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI;
using System.Configuration;
using Quartz;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.EDI.Mapping;
using System.Data;
using Concentrator.Objects.DataAccess.Repository;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Linq.Expressions;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Enumerations;

namespace Concentrator.Plugins.EDI
{
  public class ProcessEdiCustomResponses : ConcentratorEDIPlugin
  {
    public override string Name
    {
      get { return "Start EDI custom response service"; }
    }

    private int ediVendorID;

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var connectorRelations = unit.Scope.Repository<ConnectorRelation>().GetAll(x => x.XtractType == (int)XractTypeEnum.CustomExport).ToList();

        connectorRelations.ForEach(relation =>
        {
          try
          {
            ediProcessor.GetCustomReponses(unit, Config, relation.CustomerID, relation, invoiceDate: DateTime.Now.AddDays(-1));
            unit.Save();
          }
          catch (Exception ex)
          {
            log.Error("Error get custom responses", ex);
          }
        });
      }
    }

    public override Configuration Config
    {
      get { return GetConfiguration(); }
    }
  }
}
