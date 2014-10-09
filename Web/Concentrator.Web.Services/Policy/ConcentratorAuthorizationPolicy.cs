using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Policy;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Security.Principal;
using System.IdentityModel.Claims;
using Ninject;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using Concentrator.Objects.Web;

namespace Concentrator.Web.Services.Policy
{
  public class ConcentratorAuthorizationPolicy : IAuthorizationPolicy
  {

    public ConcentratorAuthorizationPolicy()
    {
      Id = Guid.NewGuid().ToString();
    }

    #region IAuthorizationPolicy Members

    private IUnitOfWork _unit;

    protected IUnitOfWork GetUnitOfWork()
    {
      if (_unit == null)
      {
        var kernel = new StandardKernel(new UnitOfWorkModule());


        _unit = kernel.Get<IUnitOfWork>();
      }
      return _unit;
    }

    public bool Evaluate(EvaluationContext evaluationContext, ref object state)
    {
      // var repository = UnitOfWork.Current.Repository<IPortfolioRepository>();

      // get the authenticated client identity
      IIdentity client = GetClientIdentity(evaluationContext);

      //var portfolio = repository.GetPortfolioByServiceCredentials(client.Name);

      // if (portfolio == null || !portfolio.IsActive)
      // {
      //   throw new Exception("Invalid User Data");
      // }

      ConcentratorIdentity id = new ConcentratorIdentity(client.Name);
      ConcentratorPrincipal prin = new ConcentratorPrincipal(id);

      evaluationContext.Properties["Principal"] = prin;

      return true;
    }

    private IIdentity GetClientIdentity(EvaluationContext evaluationContext)
    {
      object obj;
      if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
        throw new Exception("No Identity found");

      IList<IIdentity> identities = obj as IList<IIdentity>;
      if (identities == null || identities.Count <= 0)
        throw new Exception("No Identity found");

      return identities[0];
    }



    public ClaimSet Issuer
    {
      get { return ClaimSet.System; }
    }

    #endregion

    #region IAuthorizationComponent Members

    public string Id
    {
      get;
      private set;
    }

    #endregion
  }

}