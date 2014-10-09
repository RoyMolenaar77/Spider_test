using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ordering.Rules;


namespace Concentrator.Order.RuleImport
{
  public class RuleImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Import order dispatching rules"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        foreach (var rule in AllRules)
        {
          var orderRepo = unit.Scope.Repository<OrderRule>();

          var existingRule = orderRepo.GetSingle(r => r.Name == rule.Name);

          if (existingRule == null)
          {
            existingRule = new OrderRule
                             {
                               Name = rule.Name
                             };
            orderRepo.Add(existingRule);
          }
        }

        try
        {
          unit.Save();          
        }
        catch (Exception e)
        {
          log.AuditFatal("Import failed", e, "Order Rule import");
        }
      }
    }

    private List<RuleBase> AllRules
    {
      get
      {
        if (_allRules == null)
        {
          var type = typeof(RuleBase);
          _allRules = (from t in Assembly.GetAssembly(typeof(RuleBase)).GetTypes()
                       where !t.IsAbstract &&
                              type.IsAssignableFrom(t)
                       select (RuleBase)Activator.CreateInstance(t)).ToList();

        }
        return _allRules;
      }
    }

    private List<RuleBase> _allRules;

  }
}
