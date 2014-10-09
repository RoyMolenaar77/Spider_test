using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Service.Contracts;


namespace Concentrator.Objects.Service
{
  public class ConcentratorMiddleWareClient : IConcentratorService
  {
    #region IConcentratorService Members

    public List<JobInfo> GetScheduledJobs()
    {
      throw new NotImplementedException();
    }

    public void RefreshConfiguration()
    {
      throw new NotImplementedException();
    }

    public void StartJob(string jobName)
    {
      throw new NotImplementedException();
    }

    public void StopJob(string jobName)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
