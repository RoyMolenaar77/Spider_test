using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using Dapper;

namespace Concentrator.Objects
{
  public partial class ConcentratorDatabase
  {
    public partial class InsertOrUpdateConnectorProductAttributeCommand : IDisposable
    {
      private readonly IDbCommand _command = null;

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

      private readonly IDbDataParameter _attributeTypeParameter = null;

      public String AttributeType
      {
        get
        {
          return (String)_attributeTypeParameter.Value;
        }
        set
        {
          _attributeTypeParameter.Value = value;
        }
      }

      private readonly IDbDataParameter _defaultValueParameter = null;

      public String DefaultValue
      {
        get
        {
          return (String)_defaultValueParameter.Value;
        }
        set
        {
          _defaultValueParameter.Value = value;
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

      public InsertOrUpdateConnectorProductAttributeCommand(IDbConnection connection, IDbTransaction transaction = null, Int32? commandTimeout = null)
      {
        _command = connection.CreateCommand();
        _command.CommandText = "[dbo].[InsertOrUpdateConnectorProductAttribute]";

        if (commandTimeout.HasValue)
        {
          _command.CommandTimeout = commandTimeout.Value;
        }

        _command.CommandType = CommandType.StoredProcedure;
        _command.Transaction = transaction;

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

        _attributeTypeParameter = _command.CreateParameter();
        _attributeTypeParameter.DbType = DbType.String;
        _attributeTypeParameter.Direction = ParameterDirection.Input;
        _attributeTypeParameter.ParameterName = "@AttributeType";
        _command.Parameters.Add(_attributeTypeParameter);

        _defaultValueParameter = _command.CreateParameter();
        _defaultValueParameter.DbType = DbType.String;
        _defaultValueParameter.Direction = ParameterDirection.Input;
        _defaultValueParameter.ParameterName = "@DefaultValue";
        _command.Parameters.Add(_defaultValueParameter);

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
