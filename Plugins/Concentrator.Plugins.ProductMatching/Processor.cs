using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Transactions;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.ProductMatching
{
  public class Processor : ConcentratorPlugin
  {
    private const int partialVPNMatch = -1;
    private const int iceCatListMatch = 25;

    public override string Name
    {
      get { return "Product matching"; }
    }

    private List<ProductMatch> productMatches;

    protected override void Process()
    {
      log.Info("Starting product matching...");

      using (var unit = GetUnitOfWork())
      {
        var _matchRepo = unit.Scope.Repository<ProductMatch>();

        var matchedProducts = ((IFunctionScope)unit.Scope).Repository().GetProductMatches();
        productMatches = _matchRepo.GetAll().ToList();

        //group all the matched products on productid --- cproductid
        var groups = (
                      from m in matchedProducts
                      where m.CProductID.HasValue
                      group m by new { m.ProductID, m.CProductID } into matchGroup
                      select matchGroup
                     ).ToList();

        var matchList = (from b in productMatches
                         group b by b.ProductMatchID into grouped
                         select grouped).ToDictionary(x => x.Key, x => x.ToList());

        //Dictionary<int, List<int>> matchList = new Dictionary<int,List<int>>();
        int newKey = matchList.Keys.Try(x => x.Max(), 0);

        foreach (var p in groups)
        {
          var m = matchList.FirstOrDefault(x => x.Value.Any(y => y.ProductID == p.Key.ProductID));
          var m2 = matchList.FirstOrDefault(x => x.Value.Any(y => y.ProductID == p.Key.CProductID.Value));

          if (m.Key == m2.Key)
          {
            if (m.Key < 1)
            {
              int currentKey = ++newKey;
              List<ProductMatch> pList = new List<ProductMatch>();
              var pMatch = NewProductMatch(p.Key.ProductID, p.Where(x => x.ProductID == p.Key.ProductID).ToList(), currentKey, _matchRepo);
              if (pMatch != null)
                pList.Add(pMatch);

              var cpMatch = NewProductMatch(p.Key.CProductID.Value, p.Where(x => x.CProductID == p.Key.CProductID).ToList(), currentKey, _matchRepo);
              if (cpMatch != null)
                pList.Add(cpMatch);

              matchList.Add(newKey, pList);
            }
          }
          else
          {
            if (m.Key > 0 && m2.Key < 1)
            {
              var pMatch = NewProductMatch(p.Key.ProductID, p.Where(x => x.ProductID == p.Key.ProductID).ToList(), m.Key, _matchRepo);
              var cpMatch = NewProductMatch(p.Key.CProductID.Value, p.Where(x => x.CProductID == p.Key.CProductID).ToList(), m.Key, _matchRepo);
              if (cpMatch != null)
                matchList[m.Key].Add(cpMatch);

            }
            else if (m2.Key > 0 && m.Key < 1)
            {
              var cpMatch = NewProductMatch(p.Key.CProductID.Value, p.Where(x => x.CProductID == p.Key.CProductID).ToList(), m2.Key, _matchRepo);
              var pMatch = NewProductMatch(p.Key.ProductID, p.Where(x => x.ProductID == p.Key.ProductID).ToList(), m2.Key, _matchRepo);
              if (pMatch != null)
                matchList[m2.Key].Add(pMatch);
            }
            else
              log.DebugFormat("Product {0} in {1} en product {2} in {3}", p.Key.CProductID.Value, m2.Key, p.Key.ProductID, m.Key);
          }
          unit.Save();
        }
      }
      log.AuditComplete("Processing product matches complete", "Product matching");
    }

    private ProductMatch NewProductMatch(int productID, List<ProductMatchResult> matchGroup, int productMatchID, IRepository<ProductMatch> repo)
    {
      try
      {

        //calculated vendorpartnumber matches
        var calculatedMatchesVPN = matchGroup.Where(c => c.MatchPercentage == partialVPNMatch).ToList();
        //ice cat calculated matches
        var iceCatSheetMatches = matchGroup.Where(c => c.MatchPercentage == iceCatListMatch).ToList();

        //in case that a group contains only calculated matches skip that group
        if ((iceCatSheetMatches.Count + calculatedMatchesVPN.Count) == matchGroup.Count()) return null;

        //determine the percentage of vpn match

        int addedPercentage = 0;
        if (calculatedMatchesVPN.Count > 0)
        {
          var vpnMatch = calculatedMatchesVPN.FirstOrDefault();

          //which is the larget vpn 
          string baseVpn = vpnMatch.CVendorItemNumber.Length > vpnMatch.VendorItemNumber.Length ? vpnMatch.CVendorItemNumber : vpnMatch.VendorItemNumber;
          string shortenedVpn = vpnMatch.CVendorItemNumber.Length > vpnMatch.VendorItemNumber.Length ? vpnMatch.VendorItemNumber : vpnMatch.CVendorItemNumber;

          //calculate percentage
          double charPercentage = 100 / baseVpn.Length;

          double vpnPercentageMatch = charPercentage * shortenedVpn.Length; //get the percentage match

          //assign correct added percentage
          addedPercentage += (vpnPercentageMatch >= 60 && vpnPercentageMatch < 70) ? 15 : (vpnPercentageMatch >= 70 && vpnPercentageMatch < 80) ? 20 : (vpnPercentageMatch >= 80 && vpnPercentageMatch <= 100) ? 25 : 0;
        }

        if (iceCatSheetMatches.Count > 0)
          addedPercentage += iceCatListMatch;

        var match = matchGroup.Except(iceCatSheetMatches).Except(calculatedMatchesVPN).OrderByDescending(c => c.MatchPercentage).FirstOrDefault(); //loop the actuals and add them to the product match table

        var matchPr = Math.Min(match.MatchPercentage + addedPercentage, 100);

        ProductMatch productMatch = productMatches.FirstOrDefault(x => x.ProductID == productID && x.ProductMatchID == productMatchID);

        if (productMatch == null)
        {
          productMatch = new ProductMatch
      {
        ProductID = productID,
        MatchStatus = (int)MatchStatuses.New,
        ProductMatchID = productMatchID
      };
          repo.Add(productMatch);
          productMatches.Add(productMatch);
        }

        if (productMatch.MatchStatus == (int)MatchStatuses.New)
        {
          productMatch.MatchPercentage = Math.Min(matchPr, 100);
          productMatch.CalculatedMatch = addedPercentage == 0 ? false : true;
          productMatch.isMatched = matchPr == 100 ? true : false;
        }

        return productMatch;
      }
      catch (Exception e)
      {
        log.AuditError("Processing product matches failed", e, "Product matching");
      }
      return null;

    }
  }
}

