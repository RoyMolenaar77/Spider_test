using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Environments;
using Ninject;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.DataAccess.Repository;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Services.ServiceInterfaces;

namespace Concentrator.Objects.Web.Providers
{
  public class ConcentratorMembershipProvider : System.Web.Security.MembershipProvider
  {
    private string _applicationName = "ConcentratorMembershipProvider";

    public override bool ValidateUser(string username, string password)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IServiceUnitOfWork>())
      {
        return ((IUserService)unit.Service<User>()).ValidateUser(username, password);
      }
    }

    #region Properties
    public override string ApplicationName
    {
      get
      {
        return _applicationName;
      }
      set
      {
        _applicationName = value;
      }
    }

    public override string Description
    {
      get
      {
        return "Custom membership provider for WMS";
      }
    }

    public override int MaxInvalidPasswordAttempts
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override int MinRequiredNonAlphanumericCharacters
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override int MinRequiredPasswordLength
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override int PasswordAttemptWindow
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override System.Web.Security.MembershipPasswordFormat PasswordFormat
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override string PasswordStrengthRegularExpression
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    public override bool RequiresQuestionAndAnswer
    {
      get { return false; }
    }

    public override bool RequiresUniqueEmail
    {
      get { return true; }
    }
    public override bool EnablePasswordReset
    {
      get { return false; }
    }
    public override bool EnablePasswordRetrieval
    {
      get { return false; }
    }


    #endregion

    #region Methods

    public override string ResetPassword(string username, string answer)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override bool UnlockUser(string userName)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override void UpdateUser(System.Web.Security.MembershipUser user)
    {
      throw new Exception("The method or operation is not implemented.");
    }
    public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
    {
      throw new NotImplementedException();
    }

    public override System.Web.Security.MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
    {
      throw new NotImplementedException();
    }
    public override System.Web.Security.MembershipUser GetUser(object providerUserKey, bool userIsOnline)
    {
      throw new NotImplementedException();
    }
    public override string GetPassword(string username, string answer)
    {
      throw new NotImplementedException();
    }
    public override System.Web.Security.MembershipUser GetUser(string username, bool userIsOnline)
    {
      User user = ServiceLocator.Current.GetInstance<IRepository<User>>().GetSingle(u => u.IsActive && u.Username == username);

      if (user != default(User))
        return new ConcentratorMembershipUser(user.UserID, user.Username);


      return null;
    }
    public override string GetUserNameByEmail(string email)
    {
      throw new NotImplementedException();
    }
    public override System.Web.Security.MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
      throw new NotImplementedException();
    }
    public override System.Web.Security.MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
      throw new NotImplementedException();
    }
    public override int GetNumberOfUsersOnline()
    {
      throw new NotImplementedException();
    }
    public override bool DeleteUser(string username, bool deleteAllRelatedData)
    {
      throw new NotImplementedException();
    }

    public override bool ChangePassword(string username, string oldPassword, string newPassword)
    {
      throw new NotImplementedException();
    }
    public override System.Web.Security.MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out System.Web.Security.MembershipCreateStatus status)
    {
      throw new NotImplementedException();
    }


    #endregion

  }
}
