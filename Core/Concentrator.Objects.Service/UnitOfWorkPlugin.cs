using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;
using System.Configuration;

namespace Concentrator.Objects.Service
{
  public class UnitOfWorkPlugin
  {
    /// <summary>
    /// Provides a unit of work: Container  for repositories and persistance to database
    /// </summary>
    /// <returns></returns>
    protected IUnitOfWork GetUnitOfWork()
    {
      return ServiceLocator.Current.GetInstance<IUnitOfWork>();
    }

    private string _connection;

    /// <summary>
    /// Returns the connection string for direct connection to the database
    /// </summary>
    protected string Connection
    {
      get
      {
        if (string.IsNullOrEmpty(_connection)) _connection = ConfigurationManager.ConnectionStrings[Environments.Environments.Current.Name].ConnectionString;

        return _connection;
      }
    }
  }
}
