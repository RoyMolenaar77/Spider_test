using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMS.Objects.MiddleWare;
using System.Data.SqlClient;
using WMS.Objects;
using WMS.Objects.Products;
using System.Data.Linq;

namespace WMS.MiddleWare.BAS
{
  public class DailyImport : MiddleWarePlugin
  {

    private static int HistoryDays = 40;

    public override string Name
    {
      get { return "BAS Daily Import"; }
    }

    public DailyImport()
    {
    }

    
    protected override void Process()
    {
      //CalculateProductGroupRunRates();
      CalculateProductRunRates();


    }


    private void CalculateProductRunRates()
    {

      using (WMSDataContext ctx = new WMSDataContext())
      {

        using (SqlConnection cn = new SqlConnection(Properties.Settings.Default.JDEConnectionString))
        {
          cn.Open();
          using (SqlCommand cmd = cn.CreateCommand())
          {
            int startDate = Utility.ToJulianDate(DateTime.Now.AddDays(-HistoryDays));

            cmd.CommandText = @"SELECT LTRIM(RTRIM(SDITM)) AS BackendProductID, (SUM(SDSOQS) / COUNT(DISTINCT SDIVD)) AS AvgSalesPerDay
                              FROM F42119
                              WHERE SDDCTO IN ('SO','SI','SX','SW')
                              AND SDIVD >= @Date
                              GROUP BY SDITM";

            cmd.Parameters.AddWithValue("@Date", startDate);
            cmd.CommandType = System.Data.CommandType.Text;

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
              while (reader.Read())
              {
                Product product = (from p in ctx.Products
                                   where p.BackendProductID == reader.GetString(0)
                                   && p.CompanyID == this.CompanyID
                                   select p).FirstOrDefault();

                if (product == null)
                  continue;

                product.RunRate = reader.GetDouble(1);
                ctx.SubmitChanges(ConflictMode.ContinueOnConflict);
              }
            }

          
          }

          cn.Close();
        }
      }
    }


    private void CalculateProductGroupRunRates()
    {

      using (WMSDataContext ctx = new WMSDataContext())
      {

        using (SqlConnection cn = new SqlConnection(Properties.Settings.Default.JDEConnectionString))
        {
          cn.Open();
          using (SqlCommand cmd = cn.CreateCommand())
          {
            int startDate = Utility.ToJulianDate(DateTime.Now.AddDays(-HistoryDays));

            cmd.CommandText = @"SELECT LTRIM(RTRIM(SDSRP2)) AS BackendProductGroupCode, (SUM(SDSOQS) / COUNT(DISTINCT SDIVD)) AS AvgSalesPerDay
                              FROM F42119
                              WHERE SDDCTO IN ('SO','SI','SX','SW')
                              AND SDIVD >= @Date
                              GROUP BY SDSRP2";

            cmd.Parameters.AddWithValue("@Date", startDate);
            cmd.CommandType = System.Data.CommandType.Text;

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
              while (reader.Read())
              {
                ProductGroup group = (from pg in ctx.ProductGroups
                                      where pg.CompanyID == this.CompanyID
                                      && pg.BackendID == reader["BackendProductGroupCode"].ToString()
                                      select pg).FirstOrDefault();

                if (group == null)
                  continue;

                group.RunRate = reader.GetDouble(1);

                ctx.SubmitChanges();
              }
            }
          }

          cn.Close();
        }
      }
    }


  }
}
