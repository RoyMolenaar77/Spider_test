using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

using Mvc.Mailer;

namespace Concentrator.ui.Management.Controllers
{
  using Objects.Models.Users;
  using Objects.Web;
  using Mailers;
  using Web.Shared.Controllers;

  [HandleError]
  [OutputCache(Location = OutputCacheLocation.None)]
  public class AccountController : UnitOfWorkController
  {
    private IUserMailer _userMailer = new UserMailer();
    public IUserMailer UserMailer
    {
      get { return _userMailer; }
      set { _userMailer = value; }
    }

    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult Login()
    {
      ViewData["Title"] = "Login";
      return View();
    }

    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult Login(string username, string password, bool? rememberMe)
    {
      string message = "Incorrect credentials";
      bool loginSuccessful;

      try
      {
        loginSuccessful = ConcentratorPrincipal.Login(username, password);
      }
      catch (Exception ex)
      {
        loginSuccessful = false;
        message = ex.Message;
      }

      if (loginSuccessful)
      {
        message = "Login succesful";
        FormsAuthentication.SetAuthCookie(Client.User.UserName, true);
      }

      return Json(
        new
          {
            success = loginSuccessful,
            message,
            functionalities = String.Join("', '", (Client.User != null) ? Client.User.Functionalities.ToArray() : new string[] { }),
            fullName = Client.User != null ? Client.User.Name : null,
            userID = Client.User != null ? (int?)Client.User.UserID : null
          });
    }

    public ActionResult DoLogout()
    {
      FormsAuthentication.SignOut();

      return Json(
      new
      {
        success = true,
        authorized = false
      });

    }

    public ActionResult Unlock(string username, string password)
    {
      string message = "Incorrect credentials";
      bool loginSuccessful;
      try
      {
        loginSuccessful = ConcentratorPrincipal.Login(username, password);
      }
      catch (Exception ex)
      {
        loginSuccessful = false;
        message = ex.Message;
      }

      if (loginSuccessful)
      {
        message = "Login succesful";
        FormsAuthentication.SetAuthCookie(username, false);
      }

      return Json(
        new
        {
          success = loginSuccessful,
          message,
          //roles = String.Join("', '", (Client.User != null) ? Client.User.Roles.ToArray() : new string[] { }),
          functionalities = String.Join("', '", (Client.User != null) ? Client.User.Functionalities.ToArray() : new string[] { }),
          fullName = Client.User != null ? Client.User.Name : null,
          userID = Client.User != null ? (int?)Client.User.UserID : null
        });
    }

    public ActionResult Logout()
    {
      FormsAuthentication.SignOut();
      return RedirectToAction("Index", "Application");
    }

    public ActionResult ResetPassword(string EmailAddress)
    {
      using (var unit = GetUnitOfWork())
      {
        var user = GetUnitOfWork().Service<User>().Get(x => x.Email.ToLower() == EmailAddress.ToLower());

        if (user == null)
        {
          return Json(new
          {
            success = false,
            message = "Email address doesn't exist"
          });
        }
        else
        {
          String password = CreateRandomPassword(10);
          String firstName = user.Firstname;
          String userName = user.Username;

          user.Password = password;
          unit.Save();

          //Send() is an extension method: using Mvc.Mailer
          UserMailer.PasswordReset(EmailAddress, password, firstName, userName).Send();

          return Json(new
          {
            success = true,
            message = "Password has been sent to your address"
          });
        }

      }
    }

    private static string CreateRandomPassword(int passwordLength)
    {
      string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
      char[] chars = new char[passwordLength];
      Random rd = new Random();

      for (int i = 0; i < passwordLength; i++)
      {
        chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
      }

      return new string(chars);
    }


  }
}
