using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Contents;
using PetaPoco;

namespace Concentrator.Plugins.AssortmentGenerator
{
  public class GenerateAttributes : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Attributes Generator Plugin"; }
    }

    protected override void Process()
    {
      using (var db = new Database(Connection, "System.Data.SqlClient"))
      {
        try
        {
          db.CommandTimeout = 720;
          var result = db.Execute("EXECUTE sp_GenerateContentAttributes");
        }
        catch (Exception e)
        {
          log.AuditError("Stored procedure sp_GenerateConttentAttributes failed", e);
        }

        #region Shopweek sorting workaround
        log.DebugFormat("Starting Shopweek sorting workaround");

        var currentYear = DateTime.Now.ToUniversalTime().Year;
        var currentWeek = GetIso8601WeekOfYear(DateTime.Now.ToUniversalTime());
        var currentShopweek = Convert.ToInt32(string.Format("{0}{1}", currentYear, currentWeek));

        var modifiedShopWeek = GetModifiedShopWeek(currentYear, currentWeek, 3);

        var contentAttribute = db.Execute(@"update ContentAttribute Set AttributeValue = @2
                          where attributeID = @0 and connectorid in (5,7,8) and cast(attributevalue as int) > @1 ", 75, currentShopweek, modifiedShopWeek);

        log.DebugFormat("Finished Shopweek sorting workaround");
        #endregion
      }
    }

    private string GetModifiedShopWeek(int curYear, int weekNumber, int amountOfWeeksToSubtract)
    {
      return string.Format("{0}{1}", weekNumber <= 3 ? curYear - 1 : curYear, weekNumber <= 3 ? (52 - amountOfWeeksToSubtract + weekNumber).ToString("D2") : (weekNumber - amountOfWeeksToSubtract).ToString("D2"));
    }

    // This presumes that weeks start with Sunday.
    // Week 1 is the 1st week of the year with a Wednesday in it.
    public static int GetIso8601WeekOfYear(DateTime time)
    {
      // Seriously cheat.  If its Sunday, Monday or Tuesday, then it'll 
      // be the same week# as whatever Wednesday, Thursday or Friday,
      // and we always get those right
      DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
      if (day >= DayOfWeek.Sunday && day <= DayOfWeek.Tuesday)
      {
        time = time.AddDays(3);
      }

      // Return the week of our adjusted day
      return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
    }
  }
}
