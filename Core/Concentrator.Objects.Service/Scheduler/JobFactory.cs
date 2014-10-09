using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz.Spi;
using Quartz;

namespace Concentrator.Objects.Service.Scheduler
{
  public class JobFactory : IJobFactory
  {
    #region IJobFactory Members

    public Quartz.IJob NewJob(TriggerFiredBundle bundle)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public class JobManager
  {
    private readonly IJobFactory _jobFactory;
    private readonly ISchedulerFactory _schedulerFactory;

    public JobManager(ISchedulerFactory sFactory, IJobFactory jFactory)
    {
      _jobFactory = jFactory;
      _schedulerFactory = sFactory;
    }

    public IScheduler Add<T>(Trigger trigger) where T : IJob {
      IScheduler scheduler = _schedulerFactory.GetScheduler();
      scheduler.JobFactory = _jobFactory;
      scheduler.Start();
    }
  }

}
