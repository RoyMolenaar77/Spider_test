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
  public class Processor : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Missing Content Generator"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {

        foreach (var connector in unit.Scope.Repository<Connector>().GetAll().ToList().Where(c => c.ConnectorSettings.GetValueByKey<bool>("ReindexMissingContent", false)))
          try
          {
            ((IFunctionScope)unit.Scope).Repository().RegenerateMissingContent(connector.ConnectorID);
            log.AuditSuccess("Missing content was regenerated successfully", "Missing content generator");
          }
          catch (Exception e)
          {
            log.AuditFatal("The missing content generated failed", e, "Missing content generator");
          }

        try
        {
          ((IFunctionScope)unit.Scope).Repository().RegenerateSearchResults();
          log.AuditSuccess("Search results were regenerated successfully", "Missing content generator");
        }
        catch (Exception e)
        {
          log.AuditFatal("The search result generation failed", e, "Missing content generator");
        }
      }
    }
  }
}
