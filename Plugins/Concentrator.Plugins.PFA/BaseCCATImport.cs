using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Configuration;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models;

namespace Concentrator.Plugins.PFA
{
  public abstract class BaseCCATImport : ConcentratorPlugin
  {
    public abstract override string Name{get;}

    protected abstract override void Process();

    protected void ConfigureProductAttributes(List<int> sourceVendorIDs, IUnitOfWork unit)
    {

      unit.ExecuteStoreCommand(string.Format(@"
      merge productattributeconfiguration target
      using(

      select p.productid, size.attributeid from 
      product p, productattributemetadata size
      where p.isconfigurable = 1 and size.attributeid = 41 and p.sourcevendorid in ({0})

      union all 

      select p.productid, size.attributeid from 
      product p, productattributemetadata size
      where p.isconfigurable = 1 and size.attributeid = 40 and p.sourcevendorid in ({0})

      ) source
      on source.productid = target.productid and source.attributeid = target.attributeid
      when not matched by target
      then insert (productid, attributeid)
      values (source.productid, source.attributeid);
", string.Join(",", sourceVendorIDs)));
    }

    protected bool GetSoldenPeriod(int connectorID, out bool soldenPeriod)
    {
      using (var unit = GetUnitOfWork())
      {
        soldenPeriod = false;

        var connectorSettings =
          unit.Scope.Repository<ConnectorSetting>()
              .GetAll(x => x.ConnectorID == connectorID)
              .ToDictionary(x => x.SettingKey, y => y.Value);

        if (!connectorSettings.ContainsKey("SoldenStartPeriod")) return false;
        if (!connectorSettings.ContainsKey("SoldenEndPeriod")) return false;

        DateTime soldenStartPeriod;
        DateTime soldenEndPeriod;

        if (!DateTime.TryParse(connectorSettings["SoldenStartPeriod"], new CultureInfo("nl-NL"), DateTimeStyles.None, out soldenStartPeriod)) return false;
        if (!DateTime.TryParse(connectorSettings["SoldenEndPeriod"], new CultureInfo("nl-NL"), DateTimeStyles.None, out soldenEndPeriod)) return false;

        if (DateTime.Now > soldenStartPeriod && DateTime.Now < soldenEndPeriod)
          soldenPeriod = true;

        return true;       
      }
    }
  }
}
