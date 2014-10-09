using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;

namespace Concentrator.Web.Shared.Controllers
{
  public class UnitOfWorkController : Controller
  {
    private IServiceUnitOfWork _unit;

    public IServiceUnitOfWork GetUnitOfWork()
    {
      //if (_unit == null)
      _unit = ServiceLocator.Current.GetInstance<IServiceUnitOfWork>();

      return _unit;
    }
  }
}
