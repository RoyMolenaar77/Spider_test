using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.ConcentratorService.Scheduler
{
  public class TriggerGroupData : ActivityNode<TriggerData>
  {
    public TriggerGroupData(string name)
      : base(name)
    {
    }

    public IList<TriggerData> Triggers { get; set; }

    protected override IList<TriggerData> ChildrenActivities
    {
      get { return Triggers; }
    }
  }

}
