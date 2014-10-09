using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Helpers
{
  public class Config
  {
    public String Name { get; set; }
    public String Value { get; set; }
  }

  public class TaskConfigHelper
  {
    public static IDictionary<String, Boolean> GetTasksConfiguration()
    {
      using (var database = new Database(Concentrator.Objects.Environments.Environments.Current.Connection, Database.MsSqlClientProvider))
      {
        return database
          .Query<Config>("SELECT Name, Value FROM dbo.Config WHERE Name LIKE '%Task'")
          .Where(config => config.Value.ParseToBool().HasValue)
          .ToDictionary(config => config.Name, config => config.Value.ParseToBool().GetValueOrDefault());
      }
    }
  }
}
