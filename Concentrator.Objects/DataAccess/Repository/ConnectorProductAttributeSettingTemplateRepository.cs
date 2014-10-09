using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper;

namespace Concentrator.Objects.DataAccess.Repository
{
  using Models;
  using Models.Connectors;

  /// <summary>
  /// Defines the basic CRUD-methods for the <see cref="ConnectorProductAttributeSetting"/>-model.
  /// </summary>
  public interface IConnectorProductAttributeSettingTemplateRepository
  {
    /// <summary>
    /// Returns a single instance of <see cref="ConnectorProductAttributeSettingTemplate"/> using the specified arguments.
    /// </summary>
    ConnectorProductAttributeSettingTemplate GetSingle(Int32 connectorSystemID, String code);

    /// <summary>
    /// Returns a single instance of <see cref="ConnectorProductAttributeSettingTemplate"/> using the specified arguments.
    /// </summary>
    ConnectorProductAttributeSettingTemplate GetSingle(ConnectorSystem connectorSystem, String code);

    /// <summary>
    /// Returns a collection of <see cref="ConnectorProductAttributeSettingTemplate"/> using the specified arguments as filter-values.
    /// </summary>
    IEnumerable<ConnectorProductAttributeSettingTemplate> GetMultiple(Int32? connectorSystemID = null, String code = null);

    /// <summary>
    /// Returns a collection of <see cref="ConnectorProductAttributeSettingTemplate"/> using the specified arguments as filter-values.
    /// </summary>
    IEnumerable<ConnectorProductAttributeSettingTemplate> GetMultiple(ConnectorSystem connectorSystem = null, String code = null);
  }

  /// <summary>
  /// Represents the default implementation of the <see cref="IConnectorProductAttributeSettingRepository"/>-interface.
  /// </summary>
  public class ConnectorProductAttributeSettingTemplateRepository : RepositoryBase, IConnectorProductAttributeSettingTemplateRepository
  {
    public ConnectorProductAttributeSettingTemplateRepository(IDbConnection connection)
      : base(connection)
    {
    }

    public ConnectorProductAttributeSettingTemplate GetSingle(Int32 connectorSystemID, String code)
    {
      if (code == null)
      {
        throw new ArgumentNullException("code");
      }

      return GetMultiple(connectorSystemID, code).SingleOrDefault();
    }

    public ConnectorProductAttributeSettingTemplate GetSingle(ConnectorSystem connectorSystem, String code)
    {
      if (connectorSystem == null)
      {
        throw new ArgumentNullException("connectorSystem");
      }

      return GetSingle(connectorSystem.ConnectorSystemID, code);
    }

    public IEnumerable<ConnectorProductAttributeSettingTemplate> GetMultiple(Int32? connectorSystemID = null, String code = null)
    {
      using (var command = new ConcentratorDatabase.SelectConnectorProductAttributeSettingTemplateCommand(Connection)
      {
        ConnectorSystemID = connectorSystemID,
        Code = code
      })
      using (EnsureOpenStatus())
      {
        return command.ExecuteQuery<ConnectorProductAttributeSettingTemplate>();
      }
    }

    public IEnumerable<ConnectorProductAttributeSettingTemplate> GetMultiple(ConnectorSystem connectorSystem = null, String code = null)
    {
      return GetMultiple(connectorSystem != null ? (Int32?)connectorSystem.ConnectorSystemID : null, code);
    }
  }
}
