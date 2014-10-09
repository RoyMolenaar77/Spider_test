using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models;
using Concentrator.Objects.Web;
using System.Linq.Expressions;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IUserService
  {
    /// <summary>
    /// Returns all the 
    /// </summary>
    /// <param name="roleID"></param>
    /// <returns></returns>
    IQueryable<FunctionalityResult> GetFunctionalitiesPerRole(int roleID);

    /// <summary>
    /// Updates the functionalities per role
    /// </summary>
    /// <param name="roleID"></param>
    /// <param name="enabledfunctionalities"></param>
    /// <param name="disabledfunctionalities"></param>
    void UpdateFunctionalitiesPerRole(int roleID, string[] enabledfunctionalities, string[] disabledfunctionalities);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    void SaveState(UserState state);

    /// <summary>
    /// Sets the content settings for a specific user
    /// </summary>
    /// <param name="languageID">The language id the user will see content for</param>
    /// <param name="connectorID">The connector id the user will see content for</param>
    void SetContentSettings(int? languageID, int? connectorID);

    /// <summary>
    /// Validates a user. Returns true if user is valid
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>True if user is valid</returns>
    bool ValidateUser(string username, string password);

    /// <summary>
    /// Returns a model used for the identity
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    UserIdentityModel GetIdentityModel(string username, string password = null);
  }
}
