using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Web.Providers
{
  public class ConcentratorMembershipUser : System.Web.Security.MembershipUser
  {
    public int UserID { get; set; }
    public string LoginName { get; set; }

    public ConcentratorMembershipUser()
      : base()
    {

    }

    public ConcentratorMembershipUser(int userID, string loginName)
      : this()
    {
      this.UserID = userID;
      this.LoginName = loginName;
    }
  }
}
