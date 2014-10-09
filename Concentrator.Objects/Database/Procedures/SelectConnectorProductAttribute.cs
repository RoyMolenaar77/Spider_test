using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using Dapper;

namespace Concentrator.Objects
{
  public partial class ConcentratorDatabase
  {
    public partial class SelectConnectorProductAttributeCommand : IDisposable
    {
      private readonly IDbCommand _command = null;

      private readonly IDbDataParameter _connectorProductAttributeIDParameter = null;

      public Int32? ConnectorProductAttributeID
      {
        get
        {
          return (Int32)_connectorProductAttributeIDParameter.Value;
        }
        set
        {
          _connectorProductAttributeIDParameter.Value = value;
        }
      }

      private readonly IDbDataParameter _connectorIDParameter = null;

      public Int32? ConnectorID
      {
        get
        {
          return (Int32)_connectorIDParameter.Value;
        }
        set
        {
          _connectorIDParameter.Value = value;
        }
      }

      private readonly IDbDataParameter _attributeIDParameter = null;

      public Int32? AttributeID
      {
        get
        {
          return (Int32)_attributeIDParameter.Value;
        }
        set
        {
          _attributeIDParameter.Value = value;
        }
      }

      private readonly IDbDataParameter _isFilterParameter = null;

      public Boolean? IsFilter
      {
        get
        {
          return (Boolean)_isFilterParameter.Value;
        }
        set
        {
          _isFilterParameter.Value = value;
        }
      }

      public SelectConnectorProductAttributeCommand(IDbConnection connection, IDbTransaction transaction = null, Int32? commandTimeout = null)
      {
        _command = connection.CreateCommand();
        _command.CommandText = "[dbo].[SelectConnectorProductAttribute]";

        if (commandTimeout.HasValue)
        {
          _command.CommandTimeout = commandTimeout.Value;
        }

        _command.CommandType = CommandType.StoredProcedure;
        _command.Transaction = transaction;

        _connectorProductAttributeIDParameter = _command.CreateParameter();
        _connectorProductAttributeIDParameter.DbType = DbType.Int32;
        _connectorProductAttributeIDParameter.Direction = ParameterDirection.Input;
        _connectorProductAttributeIDParameter.ParameterName = "@ConnectorProductAttributeID";
        _connectorProductAttributeIDParameter.Size = 4;
        _command.Parameters.Add(_connectorProductAttributeIDParameter);

        _connectorIDParameter = _command.CreateParameter();
        _connectorIDParameter.DbType = DbType.Int32;
        _connectorIDParameter.Direction = ParameterDirection.Input;
        _connectorIDParameter.ParameterName = "@ConnectorID";
        _connectorIDParameter.Size = 4;
        _command.Parameters.Add(_connectorIDParameter);

        _attributeIDParameter = _command.CreateParameter();
        _attributeIDParameter.DbType = DbType.Int32;
        _attributeIDParameter.Direction = ParameterDirection.Input;
        _attributeIDParameter.ParameterName = "@AttributeID";
        _attributeIDParameter.Size = 4;
        _command.Parameters.Add(_attributeIDParameter);

        _isFilterParameter = _command.CreateParameter();
        _isFilterParameter.DbType = DbType.Boolean;
        _isFilterParameter.Direction = ParameterDirection.Input;
        _isFilterParameter.ParameterName = "@IsFilter";
        _command.Parameters.Add(_isFilterParameter);
      }

      void IDisposable.Dispose()
      {
        _command.Dispose();
      }

      public Int32 Execute()
      {
        return _command.ExecuteNonQuery();
      }

      public IEnumerable<TObject> ExecuteQuery<TObject>()
      {
        using (var dataReader = _command.ExecuteReader())
        {
          var mappingFunction = SqlMapper.GetTypeDeserializer(typeof(TObject), dataReader);

          while (dataReader.Read())
          {
            yield return (TObject)mappingFunction(dataReader);
          }
        }
      }

      public IDataReader ExecuteReader()
      {
        return _command.ExecuteReader();
      }

      public Object ExecuteScalar()
      {
        return _command.ExecuteScalar();
      }

      public TResult ExecuteScalar<TResult>()
      {
        return (TResult)_command.ExecuteScalar();
      }
    }
  }
}
