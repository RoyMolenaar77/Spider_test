using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Users
{
  public class UserCredential
  {
    public int UserID { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public int? ConnectorID { get; set; }

    public bool? IsRelation { get; set; }

    public int LanguageID { get; set; }

    public string Logo { get; set; }

    public int OrganizationID { get; set; }
  }
}
