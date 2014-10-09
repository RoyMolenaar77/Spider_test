﻿using Quartz;

namespace Concentrator.Objects.Service.Scheduler.Provider
{

  public interface ISchedulerProvider
  {
    /// <summary>
    /// Initializes provider and creates all necessary instances 
    /// (scheduler factoriy and scheduler itself).
    /// </summary>
    void Init();

    /// <summary>
    /// Gets scheduler instance. Should return same instance on every call.
    /// </summary>
    IScheduler Scheduler { get; }
  }

}
