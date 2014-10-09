using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Ninject;
using PetaPoco;

namespace Concentrator.Objects.Bootstrapper.Installation.Roles
{
  abstract class RoleInstaller : IInstaller
  {
    class Identified<T>
    {
      public Identified(T value, int id)
      {
        ID = id;
        Value = value;
      }

      public int ID { get; private set; }
      public T Value { get; private set; }
    }

    [Inject]
    public Func<Database> DatabaseFactory { get; set; }

    protected abstract IEnumerable<RoleToInstall> GetRolesToInstall();

    public void Run()
    {
      var rolesToInstall = GetRolesToInstall().ToArray();

      if (!rolesToInstall.Any()) return;

      using (var transaction = new TransactionScope())
      using (var db = DatabaseFactory())
      {

        var nonExistingRoles = ExcludeExistingRoles(db, rolesToInstall);

        if (!nonExistingRoles.Any())
          return;

        var identifiedRoles = IdentifyRoles(db, nonExistingRoles);

        foreach (var role in identifiedRoles)
        {
          CreateFunctionalities(db, role);
        }
        transaction.Complete();
      }
    }

    IEnumerable<RoleToInstall> ExcludeExistingRoles(Database db, IEnumerable<RoleToInstall> roles)
    {
      var varCount = 0;
      var selectQuery = string.Format(
        "SELECT RoleName FROM Role WHERE RoleName IN ({0})", 
        string.Join(", ", roles.Select(x => "@" + (varCount++)).ToArray())
      );
      var result = db.Fetch<dynamic>(selectQuery, roles.Select(x => x.Name).Cast<object>().ToArray());

      var existing = result.Select(x => (string)x.RoleName);
      return roles.Where(x => !existing.Contains(x.Name));
    }

    static IEnumerable<Identified<RoleToInstall>> IdentifyRoles(Database db, IEnumerable<RoleToInstall> rolesToInstall)
    {
      const string createQuery = @"
IF NOT exists (SELECT * FROM Role WHERE RoleName = @0)
BEGIN
    INSERT INTO Role (RoleName, IsHidden) VALUES (@0, @1)
END
";
      foreach (var roleToAdd in rolesToInstall)
      {
        db.Execute(createQuery, roleToAdd.Name, roleToAdd.IsHidden);
      }

      var varCount = 0;
      var selectQuery = string.Format(
        "SELECT RoleID, RoleName FROM Role WHERE RoleName IN ({0})", 
        string.Join(", ", rolesToInstall.Select(x => "@" + (varCount++)).ToArray())
      );

      var result = db.Fetch<dynamic>(selectQuery, rolesToInstall.Select(x => x.Name).Cast<object>().ToArray());

      return result.Select(
        row => new Identified<RoleToInstall>(rolesToInstall.First(x => x.Name == row.RoleName), row.RoleID)
      );
    }

    private static void CreateFunctionalities(Database db, Identified<RoleToInstall> roleToInstall)
    {
      const string createQuery = @"
IF NOT exists (SELECT * FROM FunctionalityRole WHERE RoleID = @0 AND FunctionalityName = @1)
BEGIN
    INSERT INTO FunctionalityRole (RoleID, FunctionalityName) VALUES (@0, @1)
END
";
      var functionalities = roleToInstall.Value.Functionalities.Distinct().ToArray();
      foreach (var functionality in functionalities)
      {
        db.Execute(createQuery, roleToInstall.ID, functionality.ToString());
      }
    }
  }
}