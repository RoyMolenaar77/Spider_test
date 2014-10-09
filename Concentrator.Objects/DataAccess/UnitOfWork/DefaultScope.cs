using Ninject;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Data.Objects;
using Ninject.Parameters;

namespace Concentrator.Objects.DataAccess.UnitOfWork
{
  public class DefaultScope : IScope, IFunctionScope
  {
    private IKernel _kernel;
    private ObjectContext _context;


    public DefaultScope(IKernel kernel, ObjectContext context)
    {
      _context = context;
      _kernel = kernel;
    }

    #region IScope Members

    public Repository.IRepository<TModel> Repository<TModel>() where TModel : class, new()
    {
      return _kernel.Get<IRepository<TModel>>(new ConstructorArgument("context", _context));
    }

    #endregion

    #region IFunctionScope Members

    public IFunctionRepository Repository()
    {
      return _kernel.Get<IFunctionRepository>(new ConstructorArgument("context", _context));
    }

    #endregion
  }
}
