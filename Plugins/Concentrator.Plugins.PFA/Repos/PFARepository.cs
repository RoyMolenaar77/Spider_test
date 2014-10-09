using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Concentrator.Plugins.PFA.Models;
using log4net;
using log4net.Core;
using AuditLog4Net.Adapter;

namespace Concentrator.Plugins.PFA.Repos
{
  public abstract class PFARepository : IPriceRepository
  {
    protected string ConnectionString { get; private set; }
    protected const string providerName = "System.Data.Odbc";
    protected IAuditLogAdapter Logger { get; private set; }

    //TODO: Move this class to the objects and abstract it into a assortment repository
    public PFARepository(string connectionString, IAuditLogAdapter logger)
    {
      ConnectionString = connectionString;
      Logger = logger;
    }

    /// <summary>
    /// Retrieves a lookup for color codes and their translations
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> GetColorLookup()
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Dictionary<string, string>("select distinct \"klr-code\", \"klr-oms\" from \"PUB\".\"klr\" ");
      }
    }

    public TaxOverrideResult GetProductTaxOverride(string itemNumber, string countryCode)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.FirstOrDefault<TaxOverrideResult>(string.Format("select bt.\"btw-code\" as \"override_tax_code\", " +
        " btw.\"btw-begin-dd\" as \"override_tax_dates\", " +
        " btw.\"btw-perc-%\" as \"override_tax_rates\" " +

        " from \"PUB\".\"bta\" bt " +
        " left join \"PUB\".\"btw\" btw on btw.\"btw-code\" = bt.\"btw-code\" " +

        " where bt.\"art-code\" = '{0}' and  bt.\"lnd-code\"='{1}' ", itemNumber, countryCode));
      }
    }

    /// <summary>
    /// Retrieve product prices
    /// </summary>
    /// <param name="productCode">Product code</param>
    /// <returns></returns>
    public List<PriceResult> GetProductPriceRules(string itemNumber, string currencyCode)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Query<PriceResult>(string.Format(" select 	vkp.\"art-code\", " +
        " vkp.\"klr-code\" as \"color_code\", " +
        " vkp.\"mat-code\" as \"size_code\", " +
        " vkp.\"vkp-prijs-$\" as \"price\", " +
        " vkp.\"vkp-ing-dd\" as \"start_date\", " +
        " afp.\"afp-eind-dd\" as \"end_date\"," +
        " vkp.\"afs-code\" as \"discount_code\", " +
        " vkp.\"val-code\" as \"currency_code\", " +
        " vkp.\"lnd-code\" as \"country_code\" " +

        " from \"PUB\".\"vkp\" vkp" +
        " left join \"PUB\".\"afp\" afp ON (vkp.\"art-code\" = afp.\"art-code\" " +
        " AND vkp.\"val-code\" = afp.\"val-code\"" +
        " AND vkp.\"klr-code\" = afp.\"klr-code\")" +
        " left join \"PUB\".\"afc\" afc ON (afp.\"afc-code\" = afc.\"afc-code\" " +
        " AND afc.\"val-code\" = afp.\"val-code\"" +
        " AND vkp.\"afs-code\" = afc.\"afs-code\" )" +
         " where vkp.\"art-code\" = '{0}' and  vkp.\"val-code\"='{1}' ", itemNumber, currencyCode)).ToList();
      }
    }


    /// <summary>
    /// Retrieve the shop week of a product
    /// </summary>
    /// <param name="itemNumber"></param>
    /// <returns></returns>
    public DateTime? GetShopWeek(string itemNumber)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.SingleOrDefault<DateTime?>(string.Format(" select top 1 \"orp-winkel-plan-dd\" from \"PUB\".\"orp\" " +
                                            " where \"art-code\" = '{0}'" +
                                            " order by \"orp-winkel-dd\"", itemNumber));
      }
    }

    public List<ProductLooseSpecificationModel> GetProductLooseSpecifications(string itemNumber, string colorCode)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Fetch<ProductLooseSpecificationModel>(string.Format("select \"art-code\" as \"ItemNumber\", \"klr-code\" as \"ColorCode\", \"frm-code\" as \"Specification\" from \"pub\".\"kfo\" where \"art-code\" = '{0}' and \"klr-code\" = '{1}'", itemNumber, colorCode)).ToList();
      }
    }


    /// <summary>
    /// Retrieves a lookup of item numbers and shop weeks
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, DateTime?> GetShopWeekLookup()
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        var sql = " select \"art-code\" as ItemNumber, \"orp-winkel-plan-dd\" as ShopWeek from \"PUB\".\"orp\" ";

        return (from p in db.Query<ShopWeekModel>(sql)
                group p by p.ItemNumber into item
                select new
                {
                  item.Key,
                  Value = (from c in item select c.ShopWeek).Max()

                }).OrderBy(c => c.Key).ToDictionary(c => c.Key, c => c.Value);

      }
    }

    public List<SkuSpecification> GetSkuSpecifications(string itemNumber)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Query<SkuSpecification>(string.Format(" select \"Spec\".\"kmk-code\" as \"KmkCode\", \"Spec\".\"kmk-oms\" as \"KmkDescription\", \"SpecGroup\".\"kms-code\" as \"KmsCode\", \"SpecGroup\".\"kms-oms\" as \"KmsDescription\", \"ProductColorSpec\".\"klr-code\" as \"ColorCode\"  from " +
                                            " \"PUB\".\"akk\" \"ProductColorSpec\"" +
                                            " inner join \"PUB\".\"kmk\" \"Spec\" on  \"Spec\".\"kmk-code\" = \"ProductColorSpec\".\"kmk-code\"" +
                                            " inner join \"PUB\".\"kms\" \"SpecGroup\" on \"SpecGroup\".\"kms-code\" = \"Spec\".\"kms-code\"" +
                                            " where \"ProductColorSpec\".\"art-code\" = '{0}'", itemNumber)).ToList();
      }
    }

    /// <summary>
    /// Retrieves the size code lookups
    /// </summary>
    /// <returns></returns>
    public List<SizeCodeResult> GetSizeCodeLookup()
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Query<SizeCodeResult>(" SELECT " +
                                    " \"mat\".\"mat-code\" as \"SizeCode\"" +
                                    " , \"mtomttmtb\".\"mtb-code\" as \"MtbCode\"" +
                                    " , \"mtomttmtb\".\"mto-oms\" as \"PfaCode\"" +
                                  " FROM \"PUB\".\"mat\"" +
                                      " left join (" +
                                      " SELECT \"mat-code\", \"mto-oms\", mtb.\"mtb-code\", mtb.\"mtb-oms\", mto.\"mtt-code\", mtt.\"mtt-oms\" FROM \"PUB\".\"mto\" as mto" +
                                      " left join (SELECT \"mtt-oms\", \"mtt-code\" FROM \"PUB\".\"mtt\") as mtt" +
                                          " ON mto.\"mtt-code\" = mtt.\"mtt-code\"" +
                                      " left join (SELECT \"mtb-oms\", \"mtb-code\" FROM \"PUB\".\"mtb\") as mtb" +
                                      " ON mto.\"mtb-code\" = mtb.\"mtb-code\"" +
                                      " ) as mtomttmtb" +
                                  " ON mat.\"mtb-code\" = mtomttmtb.\"mtb-code\"" +
                                  " AND mat.\"mat-code\" = mtomttmtb.\"mat-code\"" +
                                  " Where mtomttmtb.\"mtb-code\" is not null and mtomttmtb.\"mtt-code\" = 'ka'").ToList();
      }
    }

    /// <summary>
    /// Constructs the query for retrieving general product information
    /// </summary>
    /// <param name="itemNumber">The item number of the product</param>
    /// <param name="pfaMaterialFieldNumber">1 for CC, 2 for AT</param>
    /// <returns></returns>
    protected string ConstructGenerateProductInformationQuery(string itemNumber, int pfaMaterialFieldNumber, string countryCode, string productNameColumn = "art-levoms")
    {
      return string.Format(string.Format(
             " select \"p\".\"art-levoms\" as \"ShortDescription\", \"p\".\"{2}\" as \"ProductName\", \"p\".\"art-oms\" as \"LongDescription\", \"btw\".\"btw-begin-dd\" as \"TaxRateDates\"," +
             " \"hgr\".\"hgr-oms\" as GroupName1," +
             " \"hgr\".\"hgr-code\" as GroupCode1," +
             " \"grp\".\"grp-oms\" as GroupName2," +
             " \"grp\".\"grp-code\" as GroupCode2," +
             " \"subH\".\"shg-oms\" as GroupName3, " +
             " \"subH\".\"shg-code\" as GroupCode3," +
             " \"Season\".\"szn-oms\" as \"SeasonCode\", " +
             " \"p\".\"art-mtb-code-1\" as MtbCode1, " +
             " \"p\".\"art-mtb-code-2\" as MtbCode2, " +
             " \"p\".\"art-mtb-code-3\" as MtbCode3, " +
             " \"p\".\"art-mtb-code-4\" as MtbCode4, " +
             " \"btw\".\"btw-perc-%\" as \"TaxRatePercentage\", " +
             " \"btw\".\"btw-code\" as \"TaxCode\", " +
             " \"ACM\".\"art-comm{0}\" as Material, " +
             " \"bt\".\"btw-code\" as \"override_tax_code\"," +
             " \"btwOverride\".\"btw-begin-dd\" as \"override_tax_dates\", " +
             " \"btwOverride\".\"btw-perc-%\" as \"override_tax_rates\" " +

             " FROM \"PUB\".\"art\" \"p\" inner join" +
             " \"PUB\".\"btw\" \"btw\" on \"btw\".\"btw-code\" = \"p\".\"btw-code\"" +
             " left join \"PUB\".\"bta\" bt on \"bt\".\"art-code\" = \"p\".\"art-code\" and \"bt\".\"lnd-code\" = '{3}'" +
             " left join \"PUB\".\"btw\" btwOverride on btwOverride.\"btw-code\" = bt.\"btw-code\" " +
             " inner join \"PUB\".\"grp\" \"grp\" on \"grp\".\"grp-code\" = \"p\".\"grp-code\"" +
             " inner join \"PUB\".\"hgr\" \"hgr\" on \"hgr\".\"hgr-code\" = \"grp\".\"hgr-code\"" +
             " inner join \"PUB\".\"shg\" \"subH\" on \"subH\".\"shg-code\" =  \"grp\".\"shg-code\"" +
             " inner join \"PUB\".\"szn\" \"Season\" on \"Season\".\"szn-code\" = \"p\".\"szn-code\"" +
             " left join \"PUB\".\"acm\" \"ACM\" on \"ACM\".\"art-code\" = \"p\".\"art-code\"" +
             " where \"p\".\"art-code\" = '{1}'", pfaMaterialFieldNumber, itemNumber, productNameColumn, countryCode));
    }
  }
}
