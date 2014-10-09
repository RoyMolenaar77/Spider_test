using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft;
using Microsoft.Practices;
using Microsoft.Practices.ServiceLocation;

using PetaPoco;

namespace Concentrator.Objects.Models.Base
{
  using Users;
  using Web;

  public abstract class AuditObjectBase<TModel> : BaseModel<TModel>, IAuditObject
    where TModel : BaseModel<TModel>
  {
    #region IAuditObject Members

    public DateTime CreationTime
    {
      get;
      set;
    }

    public DateTime? LastModificationTime
    {
      get;
      set;
    }

    private int _createdBy;

    public int CreatedBy
    {
      get
      {
        if (_createdBy == 0)
        {
          var userPrincipal = Client.User ?? ServiceLocator.Current.GetInstance<IConcentratorPrincipal>();

          var userIdentity = userPrincipal == null
            ? ServiceLocator.Current.GetInstance<IConcentratorIdentity>()
            : userPrincipal.Identity as IConcentratorIdentity;

          if (userIdentity != null)
          {
            _createdBy = userIdentity.UserID;
          }
        }

        return _createdBy;
      }
      set
      {
        _createdBy = value;
      }
    }

    public int? LastModifiedBy
    {
      get;
      set;
    }

    [ResultColumn]
    public User CreatedByUser
    {
      get;
      set;
    }

    [ResultColumn]
    public User LastModifiedByUser
    {
      get;
      set;
    }

    #endregion

    public abstract override System.Linq.Expressions.Expression<Func<TModel, bool>> GetFilter();
  }
}
