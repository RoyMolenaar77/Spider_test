using System;
using System.Collections.Generic;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.PFA.ConcentratorRepos.Models;
namespace Concentrator.Plugins.PFA.ConcentratorRepos
{
  public interface IGetTheLookRepository
  {
    List<LookGroup> GetMatchedLookGroups(int targetGroupAttributeID, int inputCodeAttributeID, int seasonAttributeID, int productVendorID);
  }
}
