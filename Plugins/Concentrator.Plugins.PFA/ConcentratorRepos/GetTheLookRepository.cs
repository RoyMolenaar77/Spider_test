using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.PFA.ConcentratorRepos.Models;
using PetaPoco;

namespace Concentrator.Plugins.PFA.ConcentratorRepos
{
  public class GetTheLookRepository : BaseRepo, Concentrator.Plugins.PFA.ConcentratorRepos.IGetTheLookRepository
  {
    public GetTheLookRepository(string connection) : base(connection) { }

    public List<LookGroup> GetMatchedLookGroups(int targetGroupAttributeID, int inputCodeAttributeID, int seasonAttributeID, int productVendorID)
    {
      using (var db = new Database(Connection, "System.Data.SqlClient"))
      {
        return (from pm in db.Query<dynamic>(@"select distinct colorProduct.productid as ProductID, pavTargetCode.value as TargetGroup ,pavInputCode.value as InputCode, pavSeason.value as Season   from product p 
                                                      inner join product colorProduct on colorProduct.ParentProductID = p.productid                                                      
                                                      inner join productattributevalue pavTargetCode on p.productid = pavTargetCode.productid and pavTargetCode.attributeid = @0
                                                      inner join productattributevalue pavInputCode on p.productid = pavInputCode.productid and pavInputCode.attributeid = @1
                                                      inner join productattributevalue pavSeason on p.productid = pavSeason.productid and pavSeason.attributeid = @2
                                                      inner join vendorassortment va on va.productid = p.productid 
                                                      where va.isactive = 1 and va.vendorid = @3 and pavInputCode.value like 'W%' and p.parentProductID is null", targetGroupAttributeID, inputCodeAttributeID, seasonAttributeID, productVendorID)
                group pm by new { pm.TargetGroup, pm.InputCode, pm.Season } into matchGroup
                select new LookGroup()
                {
                  Products = matchGroup.Select(c => (int)c.ProductID).ToList(),
                  InputCode = matchGroup.Key.InputCode,
                  TargetGroup = matchGroup.Key.TargetGroup,
                  Season = matchGroup.Key.Season
                }).ToList();

      }
    }
  }
}


