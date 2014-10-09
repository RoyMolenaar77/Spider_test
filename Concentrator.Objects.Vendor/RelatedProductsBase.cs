using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Vendors
{
  public abstract class RelatedProductsBase : ConcentratorPlugin
  {
    public abstract override string Name { get; }

    /// <summary>
    /// Called for every product - product combination to determine whether they are related
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    protected abstract bool IsRelated(Product a, Product b, IUnitOfWork unit);

    /// <summary>
    /// The type of the relation
    /// </summary>
    protected abstract string RelatedProductType { get; }

    protected virtual IQueryable<Product> GetProductsCollectionSource(IUnitOfWork unit)
    {
      return unit.Scope.Repository<Product>().GetAll();
    }

    protected virtual IQueryable<Product> GetProductsCollectionRelated(IUnitOfWork unit)
    {
      return unit.Scope.Repository<Product>().GetAll();
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {

        var productsSource = GetProductsCollectionSource(unit);
        var productsRelated = GetProductsCollectionRelated(unit);

        foreach (var ps in productsSource)
        {
          foreach (var pr in productsRelated)
          {
            if (ps.ProductID == pr.ProductID) continue; //short circuit heres

            if (IsRelated(ps, pr, unit))
            {
              
            }
          }
        }
      }
    }
  }
}
