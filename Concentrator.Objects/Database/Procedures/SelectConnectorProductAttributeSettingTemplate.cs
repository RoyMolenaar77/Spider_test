using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using Dapper;

namespace Concentrator.Objects
{
  public partial class ConcentratorDatabase
  {
    public partial class SelectConnectorProductAttributeSettingTemplateCommand : IDisposable
    {
      private readonly IDbCommand _command = null;

      private readonly IDbDataParameter _connectorSystemIDParameter = null;

      public Int32? ConnectorSystemID
      {
        get
        {
          return (Int32)_connectorSystemIDParameter.Value;
        }
        set
        {
          _connectorSystemIDParameter.Value = value;
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

      public SelectConnectorProductAttributeSettingTemplateCommand(IDbConnection connection, IDbTransaction transaction = null, Int32? commandTimeout = null)
      {
        _command = connection.CreateCommand();
        _command.CommandText = "[dbo].[SelectConnectorProductAttributeSettingTemplate]";

        if (commandTimeout.HasValue)
        {
          _command.CommandTimeout = commandTimeout.Value;
        }

        _command.CommandType = CommandType.StoredProcedure;
        _command.Transaction = transaction;

        _connectorSystemIDParameter = _command.CreateParameter();
        _connectorSystemIDParameter.DbType = DbType.Int32;
        _connectorSystemIDParameter.Direction = ParameterDirection.Input;
        _connectorSystemIDParameter.ParameterName = "@ConnectorSystemID";
        _connectorSystemIDParameter.Size = 4;
        _command.Parameters.Add(_connectorSystemIDParameter);

        _codeParameter = _command.CreateParameter();
        _codeParameter.DbType = DbType.String;
        _codeParameter.Direction = ParameterDirection.Input;
        _codeParameter.ParameterName = "@Code";
        _codeParameter.Size = 256;
        _command.Parameters.Add(_codeParameter);
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
