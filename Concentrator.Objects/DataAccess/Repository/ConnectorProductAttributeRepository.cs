using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper;

namespace Concentrator.Objects.DataAccess.Repository
{
  using Enumerations;
  using Models;
  
  /// <summary>
  /// Defines the basic CRUD-methods for the <see cref="ConnectorProductAttribute"/>-model.
  /// </summary>
  public interface IConnectorProductAttributeRepository
  {
    /// <summary>
    /// Deletes a connector product attribute.
    /// </summary>
    RepositoryOperationStatus Delete(ConnectorProductAttribute connectorProductAttribute);

    /// <summary>
    /// Gets a single connector product attribute.
    /// </summary>
    ConnectorProductAttribute GetSingle(Int32 connectorProductAttributeID);

    /// <summary>
    /// Gets a single connector product attribute.
    /// </summary>
    ConnectorProductAttribute GetSingle(Int32 connectorID, Int32 productAttributeID, Boolean isFilter);

    /// <summary>
    /// Gets a connector product attribute.
    /// </summary>
    IEnumerable<ConnectorProductAttribute> GetMultiple(Int32? connectorID = null, Int32? productAttributeID = null, Boolean? isFilter = null);

    /// <summary>
    /// Creates or updates a connector product attribute.
    /// </summary>
    /// <returns>
    /// Returns true if the <see cref="ConnectorProductAttribute"/> 
    /// </returns>
    RepositoryOperationStatus Upsert(ConnectorProductAttribute connectorProductAttribute);
  }

  /// <summary>
  /// Represents the default implementation of the <see cref="IConnectorProductAttributeRepository"/>-interface.
  /// </summary>
  public class ConnectorProductAttributeRepository : RepositoryBase, IConnectorProductAttributeRepository
  {
    public ConnectorProductAttributeRepository(IDbConnection connection)
      : base(connection)
    {
    }

    public RepositoryOperationStatus Delete(ConnectorProductAttribute connectorProductAttribute)
    {
      Validate(connectorProductAttribute);

      using (var command = new ConcentratorDatabase.DeleteConnectorProductAttributeCommand(Connection)
      {
        ConnectorProductAttributeID = connectorProductAttribute.ConnectorProductAttributeID
      })
      using (EnsureOpenStatus())
      {
        return command.Execute() > 0
          ? RepositoryOperationStatus.Deleted
          : RepositoryOperationStatus.Nothing;
      }
    }

    public ConnectorProductAttribute GetSingle(int connectorProductAttributeID)
    {
      using (var command = new ConcentratorDatabase.SelectConnectorProductAttributeCommand(Connection)
      {
        ConnectorProductAttributeID = connectorProductAttributeID
      })
      using (EnsureOpenStatus())
      {
        return command.ExecuteQuery<ConnectorProductAttribute>().SingleOrDefault();
      }
    }

    public ConnectorProductAttribute GetSingle(int connectorID, int productAttributeID, bool isFilter)
    {
      return GetMultiple(connectorID, productAttributeID, isFilter).SingleOrDefault();
    }

    public IEnumerable<ConnectorProductAttribute> GetMultiple(int? connectorID = null, int? productAttributeID = null, bool? isFilter = null)
    {
      using (var command = new ConcentratorDatabase.SelectConnectorProductAttributeCommand(Connection)
      {
        AttributeID = productAttributeID,
        ConnectorID = connectorID,
        IsFilter = isFilter
      })
      using (EnsureOpenStatus())
      {
        return command.ExecuteQuery<ConnectorProductAttribute>();
      }
    }

    public RepositoryOperationStatus Upsert(ConnectorProductAttribute connectorProductAttribute)
    {
      Validate(connectorProductAttribute, false);

      using (var command = new ConcentratorDatabase.InsertOrUpdateConnectorProductAttributeCommand(Connection)
      {
        AttributeID = connectorProductAttribute.ProductAttributeID,
        AttributeType = connectorProductAttribute.ProductAttributeType,
        ConnectorID = connectorProductAttribute.ConnectorID,
        DefaultValue = connectorProductAttribute.DefaultValue,
        IsFilter = connectorProductAttribute.IsFilter
      })
      using (EnsureOpenStatus())
      {
        return command.ExecuteScalar<RepositoryOperationStatus>();
      }
    }

    private void Validate(ConnectorProductAttribute connectorProductAttribute, Boolean checkConnectorProductAttributeID = true)
    {
      if (connectorProductAttribute == null)
      {
        throw new ArgumentNullException("connectorProductAttribute");
      }

      if (connectorProductAttribute.ConnectorProductAttributeID == null)
      {
        throw new ArgumentNullException("ConnectorProductAttributeID property cannot be null");
      }
    }
  }
}
