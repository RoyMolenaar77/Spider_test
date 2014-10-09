using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using FirebirdSql.Data.FirebirdClient;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
	public abstract class FirebirdRepository<TModel> : IDisposable
	{
		protected FbConnection Connection
    {
      get;
      private set;
    }

    protected DateTime? LastExecutionTime
    {
      get;
      private set;
    }

		public FirebirdRepository(String connectionString, DateTime? lastExecutionTime = null)
		{
      EmbeddedResourceHelper.Bind(this);

			Connection = new FbConnection(connectionString);
			Connection.Open();

      LastExecutionTime = lastExecutionTime;
		}

		public void Dispose()
		{
      if (Connection != null)
      {
        Connection.Close();
        Connection = null;
      }
		}

    public IEnumerable<TModel> Execute()
    {
      using (var command = new FbCommand(GetQuery(), Connection))
      using (var reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          yield return GetModel(reader);
        }
      }
    }

    protected abstract String GetQuery();
    protected abstract TModel GetModel(FbDataReader reader);
  }
}
