using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Concentrator.Tasks.Management.Contracts
{
  using Models;

  /// <summary>
  /// Defines the methods to access and control the task scheduler.
  /// </summary>
  [ServiceContract]
  public interface ITaskManager
  {
    /// <summary>
    /// Returns a collection of names of all the installed tasks.
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "task/{path}")]
    TaskInformation GetTask(String path);

    /// <summary>
    /// Returns a collection of names of all the installed tasks.
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "task/{path}/{start}/{finish}")]
    TaskInformation GetTask(String path, DateTime start, DateTime finish);

    /// <summary>
    /// Returns a collection of names of all the installed tasks.
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "tasks")]
    IEnumerable<TaskInformation> GetTasks();

    /// <summary>
    /// Returns a collection of names of all the installed tasks.
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "tasks/{start}/{finish}")]
    IEnumerable<TaskInformation> GetTasks(DateTime start, DateTime finish);

    /// <summary>
    /// Returns a collection of names of all the installed tasks.
    /// </summary>
    [OperationContract]
    [WebGet(UriTemplate = "tasks/{filter}/{start}/{finish}")]
    IEnumerable<TaskInformation> GetTasks(String filter, DateTime start, DateTime finish);
    
    /// <summary>
    /// Runs the specified task.
    /// </summary>
    [OperationContract]
    [WebInvoke(UriTemplate = "task/{path}")]
    void RunTask(String path);

    /// <summary>
    /// Runs the specified task with arguments.
    /// </summary>
    [OperationContract]
    [WebInvoke(UriTemplate = "task/{path}/{arguments}")]
    void RunTask(String path, String arguments);
  }
}
