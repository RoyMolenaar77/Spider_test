using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using Dapper;

namespace Concentrator.Objects
{
  public partial class ConcentratorDatabase
  {
    public partial class DeleteConnectorProductAttributeCommand : IDisposable
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

      public DeleteConnectorProductAttributeCommand(IDbConnection connection, IDbTransaction transaction = null, Int32? commandTimeout = null)
      {
        _command = connection.CreateCommand();
        _command.CommandText = "[dbo].[DeleteConnectorProductAttribute]";

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
