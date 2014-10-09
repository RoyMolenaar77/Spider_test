using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Concentrator.Service.Contracts
{
  [ServiceContract(CallbackContract = typeof(IConcentratorService))]
  public interface IConcentratorService
  {
    [OperationContract(IsOneWay = false)]
    List<JobInfo> GetScheduledJobs();

    [OperationContract(IsOneWay = true)]
    void StartJob(string jobName);

    [OperationContract(IsOneWay = true)]
    void StopJob(string jobName);


  }

}
