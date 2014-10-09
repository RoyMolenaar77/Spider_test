using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper;

namespace Concentrator.Objects.DataAccess.Repository
{
  using Models;

  /// <summary>
  /// Defines the basic CRUD-methods for the <see cref="ConnectorProductAttributeSetting"/>-model.
  /// </summary>
  public interface IConnectorProductAttributeSettingRepository
  {
    void CreateOrUpdate(ConnectorProductAttributeSetting connectorProductAttributeSetting);

    void Delete(Int32 connectorProductAttributeID);

    void Delete(ConnectorProductAttributeSetting connectorProductAttributeSetting);

    ConnectorProductAttributeSetting GetSingle(Int32 connectorProductAttributeID, String code);

    IEnumerable<ConnectorProductAttributeSetting> GetMultiple(Int32? connectorProductAttributeID = null, String code = null);
  }

  /// <summary>
  /// Represents the default implementation of the <see cref="IConnectorProductAttributeSettingRepository"/>-interface.
  /// </summary>
  public class ConnectorProductAttributeSettingRepository : RepositoryBase, IConnectorProductAttributeSettingRepository
  {
    public ConnectorProductAttributeSettingRepository(IDbConnection connection)
      : base(connection)
    {
    }

    public void CreateOrUpdate(ConnectorProductAttributeSetting connectorProductAttributeSetting)
    {
      Validate(connectorProductAttributeSetting);

      using (var command = new ConcentratorDatabase.InsertOrUpdateConnectorProductAttributeSettingCommand(Connection)
      {
        ConnectorProductAttributeID = connectorProductAttributeSetting.ConnectorProductAttributeID,
        Code = connectorProductAttributeSetting.Code,
        Type = connectorProductAttributeSetting.Type,
        Value = connectorProductAttributeSetting.Value
      })
      using (EnsureOpenStatus())
      {
        command.Execute();
      }
    }

    public void Delete(ConnectorProductAttributeSetting connectorProductAttributeSetting)
    {
      Validate(connectorProductAttributeSetting, false);

      using (var command = new ConcentratorDatabase.DeleteConnectorProductAttributeSettingCommand(Connection)
      {
        ConnectorProductAttributeID = connectorProductAttributeSetting.ConnectorProductAttributeID,
        Code = connectorProductAttributeSetting.Code
      })
      using (EnsureOpenStatus())
      {
        command.Execute();
      }
    }

    public ConnectorProductAttributeSetting GetSingle(Int32 connectorProductAttributeID, String code)
    {
      if (connectorProductAttributeID == default(Int32))
      {
        throw new ArgumentException("connectorProductAttributeID cannot be zero");
      }

      if (String.IsNullOrEmpty(code))
      {
        throw new ArgumentException("name cannot be null or empty");
      }

      return GetMultiple(connectorProductAttributeID, code).SingleOrDefault();
    }

    public IEnumerable<ConnectorProductAttributeSetting> GetMultiple(Int32? connectorProductAttributeID = null, String code = null)
    {
      using (var command = new ConcentratorDatabase.SelectConnectorProductAttributeSettingCommand(Connection)
      {
        ConnectorProductAttributeID = connectorProductAttributeID,
        Code = code
      })
      using (EnsureOpenStatus())
      {
        return command.ExecuteQuery<ConnectorProductAttributeSetting>();
      }
    }
    
    private void Validate(ConnectorProductAttributeSetting connectorProductAttributeSetting, Boolean fullCheck = true)
    {
      if (connectorProductAttributeSetting == null)
      {
        throw new ArgumentNullException("connectorProductAttributeSetting");
      }

      if (fullCheck && connectorProductAttributeSetting.ConnectorProductAttributeID == default(Int32))
      {
        throw new ArgumentException("ConnectorProductAttributeID-property is zero");
      }

      if (fullCheck && connectorProductAttributeSetting.Code == null)
      {
        throw new ArgumentException("Code-property is null");
      }

      if (fullCheck && connectorProductAttributeSetting.Type == null)
      {
        throw new ArgumentException("Type-property is null");
      }

      if (fullCheck && connectorProductAttributeSetting.Value == null)
      {
        throw new ArgumentException("Value-property is null");
      }
    }


    public void Delete(int connectorProductAttributeID)
    {
      throw new NotImplementedException();
    }
  }
}
