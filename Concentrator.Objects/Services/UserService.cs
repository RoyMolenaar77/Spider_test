using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Connectors;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Security.Cryptography;

namespace Concentrator.Objects.Services
{
  public class UserService : Service<User>, IUserService
  {
    public override User Get(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
      return Repository().Include(c => c.Connector, c => c.UserRoles).GetSingle(predicate);
    }

    public IQueryable<FunctionalityResult> GetFunctionalitiesPerRole(int roleID)
    {

      var functionalitiesInDatabase = (from r in Repository<FunctionalityRole>().GetAll()
                                       where r.RoleID == roleID
                                       select new
                                       {
                                         r.FunctionalityName,
                                         IsEnabled = true
                                       }).ToArray();

      var allFunctionalities = (from r in
                                  (from n in Enum.GetNames(typeof(Functionalities))
                                   where !functionalitiesInDatabase.Any(x => x.FunctionalityName == n)
                                   select new
                                   {
                                     FunctionalityName = n,
                                     IsEnabled = false
                                   }).Union(functionalitiesInDatabase)
                                let type = typeof(Functionalities)
                                let field = type.GetField(r.FunctionalityName)
                                where field != null
                                let attribute = ((FunctionalityInfoAttribute)field.GetCustomAttributes(typeof(FunctionalityInfoAttribute), false).Single())
                                select new FunctionalityResult()
                                {
                                  DisplayName = attribute.DisplayName,
                                  FunctionalityName = r.FunctionalityName,
                                  Group = attribute.Group,
                                  IsEnabled = r.IsEnabled
                                }).AsQueryable();

      return allFunctionalities;
    }

    public void UpdateFunctionalitiesPerRole(int roleID, string[] enabledfunctionalities, string[] disabledfunctionalities)
    {
      var role = Repository<Role>().GetSingle(c => c.RoleID == roleID);

      enabledfunctionalities.ForEach(
        (funct, idx) =>
        {
          if (!string.IsNullOrEmpty(funct))
          {
            var ar = new FunctionalityRole { FunctionalityName = funct, RoleID = roleID };

            if (role.FunctionalityRoles.Where(x => x.FunctionalityName == funct && x.RoleID == roleID).Count() < 1)
            {
              Repository<FunctionalityRole>().Add(ar);
            }
          }
        });

      disabledfunctionalities.ForEach(
        (disabledFunc, idx) =>
        {
          if (!string.IsNullOrEmpty(disabledFunc))
          {
            var ar = role.FunctionalityRoles.Where(x => x.FunctionalityName == disabledFunc && x.RoleID == roleID).FirstOrDefault();

            if (ar != null)
            {
              Repository<FunctionalityRole>().Delete(ar);
            }
          }
        });
    }

    #region IUserService Members

    public void SaveState(UserState state)
    {
      var stateRepo = Repository<UserState>();

      var originalState = stateRepo.GetSingle(c => c.UserID == Client.User.UserID && c.EntityName == state.EntityName);

      if (originalState == null)
      {
        originalState = new UserState
        {
          UserID = Client.User.UserID,
          EntityName = state.EntityName
        };

        stateRepo.Add(originalState);
      }

      originalState.UserID = state.UserID;
      originalState.EntityName = state.EntityName;
      originalState.SavedState = state.SavedState;
    }

    #endregion

    #region IUserService Members


    public void SetContentSettings(int? languageID, int? connectorID)
    {
      var user = Repository<User>().GetSingle(c => c.UserID == Client.User.UserID);

      if (languageID.HasValue)
      {
        user.LanguageID = languageID.Value;
        Client.User.LanguageID = languageID.Value;
      }

      if (connectorID != Client.User.ConnectorID && connectorID.HasValue)
      {
        user.ConnectorID = connectorID;
        Client.User.ConnectorID = connectorID;
      }

    }
    #endregion

    #region IUserService Members


    public virtual bool ValidateUser(string username, string password)
    {
      username.ThrowIfNullOrEmpty(new ArgumentNullException("Username cannot be empty"));
      password.ThrowIfNullOrEmpty(new ArgumentNullException("Password cannot be empty"));

      return Repository().GetAll().ToList().FirstOrDefault(c => c.Username == username && c.Password == password) != null;
    }

    #endregion

    #region IUserService Members

    public virtual UserIdentityModel GetIdentityModel(string username, string password = null)
    {
      var users = Repository().Include(c => c.Connector, c => c.UserRoles).GetAllAsQueryable(c => c.Username.ToLower() == username);

      var user = users.FirstOrDefault();

      if (user != null && !user.Password.EndsWith("==") && user.Password == password) // if user doesn't have hashed password, update it
      {
        using (var unit = ServiceLocator.Current.GetInstance<IServiceUnitOfWork>())
        {
          user.Password = CalculatePasswordHash(user.Username, user.UserID, user.Password);
          unit.Save();
        }
      }

      if (!string.IsNullOrEmpty(password) && (user == null || user.Password != CalculatePasswordHash(user.Username, user.UserID, password)))
      {
        throw new InvalidOperationException("Invalid credentials");
      }

      string connectorName = string.Empty;
      string logo = user.Logo;
      if (user.ConnectorID.HasValue)
      {
        var connector = Repository<Connector>().GetSingle(c => c.ConnectorID == user.ConnectorID.Value);
        connectorName = connector.Name;
        if (!string.IsNullOrEmpty(connector.ConnectorLogoPath))
          logo = connector.ConnectorLogoPath;
      }

      return new UserIdentityModel(user.Username, user.UserID, logo, user.Timeout, user.Firstname + " " + user.Lastname, user.LanguageID,
                          user.UserRoles.FirstOrDefault().Role, user.ConnectorID, user.Connector.Try(c => c.Name, string.Empty),
                          (from r in user.UserRoles
                           from b in r.Role.FunctionalityRoles
                           select b.FunctionalityName).Distinct().ToList(), user.UserRoles.Select(c => c.VendorID).ToArray(), user.OrganizationID);

    }

    public string CalculatePasswordHash(string userName, int userID, string password)
    {
      SHA512 hashProvider512 = SHA512CryptoServiceProvider.Create();
      SHA256 hashProvider256 = SHA256CryptoServiceProvider.Create();

      string salt = userID.ToString() + userName; // construct the salt from the useriD + username
      byte[] saltBytes = new byte[salt.Length * sizeof(char)]; // initialise new bytearray, size of the string times size of each char
      System.Buffer.BlockCopy(salt.ToCharArray(), 0, saltBytes, 0, salt.Length); // don't use encoding, just copy pure byte data

      byte[] saltHash = hashProvider256.ComputeHash(saltBytes); // compute the hash from the salt string
      string saltString = Convert.ToBase64String(saltHash); // convert the hash back to a string
      string passwordString = password + saltString; // construct the complete password from the password and the saltstring

      byte[] passwordBytes = new byte[passwordString.Length * sizeof(char)]; // and initialise the new bytearray
      System.Buffer.BlockCopy(passwordString.ToCharArray(), 0, passwordBytes, 0, passwordString.Length); // copy it over


      byte[] passwordHash = hashProvider512.ComputeHash(passwordBytes); // generate the final hash
      string passwordHashString = Convert.ToBase64String(passwordHash); // and transform that back into a stringr

      return passwordHashString;
    }

    #endregion
  }

}

