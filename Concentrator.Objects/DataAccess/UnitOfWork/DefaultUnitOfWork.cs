using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;

using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ninject.Parameters;

using ThisMember.Core;
using ThisMember.Core.Interfaces;

namespace Concentrator.Objects.DataAccess.UnitOfWork
{
  using EntityFramework;
  using DependencyInjection.NinjectModules;
  using Services.Base;

  public class DefaultUnitOfWork : Provider<ObjectContext>, IUnitOfWork, IServiceUnitOfWork
  {
    private IKernel _kernel;
    private ObjectContext _context;
    private IScope _scope;
    private IMemberMapper mapper = new MemberMapper();

    public DefaultUnitOfWork(IKernel kernel)
    {
      _kernel = kernel;

      _context = new ConcentratorDataContext();
      _context.CommandTimeout = 600;

      _scope = _kernel.Get<IScope>(new ConstructorArgument("context", _context));
    }

    #region IDisposable Members

    public void Dispose()
    {

    }

    #endregion

    #region Ninject provider members
    protected override ObjectContext CreateInstance(IContext context)
    {
      return _context;
    }
    #endregion

    #region IServiceUnitOfWork Members

    public IService<TServiceModel> Service<TServiceModel>() where TServiceModel : class, new()
    {
      var service = _kernel.Get<IService<TServiceModel>>();

      service.SetScope(Scope);

      return service;
    }

    #endregion

    #region IUnitOfWork Members
    public void Save()
    {
      _context.SaveChanges();
    }

    public IScope Scope
    {
      get { return _scope; }
    }

    public ConcentratorDataContext Context
    {
      get { return (_context as ConcentratorDataContext); }
    }

    public void ExecuteStoreCommand(string command, params object[] parameters)
    {
      _context.ExecuteStoreCommand(command, parameters);
    }
    #endregion

    #region IUnitOfWork Members


    public IQueryable<TModel> ExecuteStoreQuery<TModel>(string query)
    {
      return _context.ExecuteStoreQuery<TModel>(query).AsQueryable();
    }

    #endregion
  }
}
