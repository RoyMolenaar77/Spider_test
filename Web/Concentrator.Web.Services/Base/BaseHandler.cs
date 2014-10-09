using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Ninject;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using System.Net;
using System.IO;

namespace Concentrator.Web.Services.Base
{
  public abstract class BaseHandler : IHttpHandler
  {
    private IUnitOfWork _unit;

    protected IUnitOfWork GetUnitOfWork()
    {
      if (_unit == null)
      {
        var kernel = new StandardKernel();

        kernel.Load<RequestScopeUnitOfWorkModule>();
        kernel.Load<RequestScopeServiceUnitOfWorkModule>();
        kernel.Load<CommonRepositoryModule>();
        kernel.Load<ServiceModule>();
        kernel.Load<ContextRequestScopeModule>();
        kernel.Load<ManagementServiceModule>();
        

        _unit = kernel.Get<IUnitOfWork>();
      }
      return _unit;
    }

    #region IHttpHandler Members

    public abstract bool IsReusable
    {
      get;
    }

    public abstract void ProcessRequest(HttpContext context);

    #endregion
  }
}