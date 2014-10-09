using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Configuration;

namespace Concentrator.Objects.Utility.General
{
  public class Utility
  {
    public static bool IsDefaultContentVendor(int contentVendorID, IScope scope)
    {
      
        string dfContentVendorID = (from c in scope.Repository<Config>().GetAllAsQueryable()
                                    where c.Name == "DefaultContentVendorID"
                                    select c.Value).SingleOrDefault();

        if (dfContentVendorID == contentVendorID.ToString())
          return true;
        else
          return false;
      
    }

    public static IEnumerable<T> Get<T>()
    {
      return System.Enum.GetValues(typeof(T)).Cast<T>();
    }
  }
}
