using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.MissingContentGeneration
{
  public class ContentProcessor : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Description Content Generator"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        foreach (var connector in unit.Scope.Repository<Connector>().GetAll().Select(c => c.ConnectorID).ToList())
        {
          try
          {
            ((IFunctionScope)unit.Scope).Repository().RegenarateContentFlat(connector);
            log.AuditSuccess("Content flat was regenerated successfully", "Content processor");
          }
          catch (Exception e)
          {
            log.AuditFatal("The content flat processing failed", e, "Content processor");
          }
        }
      }
    }
  }
}
