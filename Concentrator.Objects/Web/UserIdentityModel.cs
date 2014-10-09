using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Model.Users;

namespace Concentrator.Objects.Web
{
  public class UserIdentityModel
  {
    public string UserName { get; private set; }

    public int UserID { get; private set; }

    public string Logo { get; private set; }

    public int Timeout { get; private set; }

    public string Name { get; private set; }

    public int LanguageID { get; private set; }

    public Role Role { get; private set; }

    public int? ConnectorID { get; private set; }

    public int OrganizationID { get; private set; }

    public string Connector { get; private set; }

    public List<string> Functionalities { get; private set; }

    public int[] VendorIDs { get; private set; }

    public UserIdentityModel()
    {
    }

    public UserIdentityModel(int userID)
    {
      UserID = userID;
    }

    public UserIdentityModel(string userName, int userID, string logo, int timeout, string name, int languageID, Role role, int? connectorID, string connector, List<string> functionalities, int[] vendorIDs, int organizationID)
    {
      UserName = userName;
      UserID = userID;
      OrganizationID = organizationID;
      Logo = logo;
      Timeout = timeout;
      Name = name;
      LanguageID = languageID;
      Role = role;
      ConnectorID = connectorID;
      Connector = connector;
      Functionalities = functionalities;
      VendorIDs = vendorIDs;
    }
  }
}
