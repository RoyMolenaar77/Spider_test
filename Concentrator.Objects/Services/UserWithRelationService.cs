using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Model.Users;

namespace Concentrator.Objects.Services
{
  public class UserWithRelationService : UserService
  {
    public override bool ValidateUser(string username, string password)
    {
      return Repository<UserCredential>().GetSingle(c => c.Username == username && c.Password == password) != null;
    }

    public override UserIdentityModel GetIdentityModel(string username, string password = null)
    {
      var users = Repository<UserCredential>().GetAllAsQueryable(c => c.Username == username);


      if (!string.IsNullOrEmpty(password))
      {
        users = users.Where(c => c.Password == password);
      }

      var user = users.FirstOrDefault();

      user.ThrowIfNull("User cannot be null");
      var userRepo = Repository();



      string connectorName = string.Empty;
      string logo = user.Logo;
      if (user.ConnectorID.HasValue)
      {
        var connector = Repository<Connector>().GetSingle(c => c.ConnectorID == user.ConnectorID.Value);
        connectorName = connector.Name;
        if (!string.IsNullOrEmpty(connector.ConnectorLogoPath))
          logo = connector.ConnectorLogoPath;
      }

      Role r = Repository<Role>().GetSingle(c => c.RoleName.ToLower() == "user");

      var userE = Repository<User>().GetSingle(c => c.UserID == user.UserID);


      return new UserIdentityModel(username, user.UserID, logo, 200, username, user.LanguageID, r, user.ConnectorID, connectorName, new List<string>(), userE == null ? userE.UserRoles.Select(c => c.VendorID).ToArray() : new int[1] { 0 }, user.OrganizationID);
    }
  }
}
