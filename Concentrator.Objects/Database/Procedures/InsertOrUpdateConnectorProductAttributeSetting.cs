﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using Dapper;

namespace Concentrator.Objects
{
  public partial class ConcentratorDatabase
  {
    public partial class InsertOrUpdateConnectorProductAttributeSettingCommand : IDisposable
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

      private readonly IDbDataParameter _codeParameter = null;

      public String Code
      {
        get
        {
          return (String)_codeParameter.Value;
        }
        set
        {
          _codeParameter.Value = value;
        }
      }

      private readonly IDbDataParameter _typeParameter = null;

      public String Type
      {
        get
        {
          return (String)_typeParameter.Value;
        }
        set
        {
          _typeParameter.Value = value;
        }
      }

      private readonly IDbDataParameter _valueParameter = null;

      public String Value
      {
        get
        {
          return (String)_valueParameter.Value;
        }
        set
        {
          _valueParameter.Value = value;
        }
      }

      public InsertOrUpdateConnectorProductAttributeSettingCommand(IDbConnection connection, IDbTransaction transaction = null, Int32? commandTimeout = null)
      {
        _command = connection.CreateCommand();
        _command.CommandText = "[dbo].[InsertOrUpdateConnectorProductAttributeSetting]";

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

        _codeParameter = _command.CreateParameter();
        _codeParameter.DbType = DbType.String;
        _codeParameter.Direction = ParameterDirection.Input;
        _codeParameter.ParameterName = "@Code";
        _codeParameter.Size = 256;
        _command.Parameters.Add(_codeParameter);

        _typeParameter = _command.CreateParameter();
        _typeParameter.DbType = DbType.String;
        _typeParameter.Direction = ParameterDirection.Input;
        _typeParameter.ParameterName = "@Type";
        _command.Parameters.Add(_typeParameter);

        _valueParameter = _command.CreateParameter();
        _valueParameter.DbType = DbType.String;
        _valueParameter.Direction = ParameterDirection.Input;
        _valueParameter.ParameterName = "@Value";
        _command.Parameters.Add(_valueParameter);
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
