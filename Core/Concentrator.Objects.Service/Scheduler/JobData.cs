using System.Collections.Generic;

namespace Concentrator.Objects.Service.Scheduler
{
  public class JobData : ActivityNode<TriggerData>
  {
    public JobData(string name, string group, IList<TriggerData> triggers)
      : base(name)
    {
      Triggers = triggers;
      GroupName = group;
    }

    public IList<TriggerData> Triggers { get; private set; }

    public string GroupName { get; private set; }

    public bool HaveTriggers
    {
      get
      {
        return Triggers != null && Triggers.Count > 0;
      }
    }

    protected override IList<TriggerData> ChildrenActivities
    {
      get { return Triggers; }
    }
  }

}
