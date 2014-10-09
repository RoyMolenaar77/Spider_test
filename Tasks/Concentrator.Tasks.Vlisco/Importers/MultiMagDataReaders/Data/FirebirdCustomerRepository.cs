using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
  using Models;

	public class FirebirdCustomerRepository : FirebirdRepository<Customer>
	{
		[Resource]
		private readonly String Query = null;

		public FirebirdCustomerRepository(String connectionString, DateTime lastExecutionTime)
			: base(connectionString, lastExecutionTime)
    {
		}

    protected override Customer GetModel(FirebirdSql.Data.FirebirdClient.FbDataReader reader)
    {
      return new Customer
      {
        ShopCode              = reader.GetString(0),
        Client                = reader.GetInt32(1),
        Name                  = reader.GetString(2),
        FirstName             = reader.GetString(3),
        Address1              = reader.GetString(4),
        Address2              = reader.GetString(5),
        Address3              = reader.GetString(6),
        Email                 = reader.GetString(7),
        PostCode              = reader.GetString(8),
        City                  = reader.GetString(9),
        TelephonePersonal     = reader.GetString(10),
        TelephoneBusiness     = reader.GetString(11),
        BirthDay              = reader.GetDateTime(12),
        CreditCard            = reader.GetString(13),
        TotalAmountSpend      = reader.GetDecimal(14),
        FirstBuy              = reader.GetDateTime(15),
        LastBuy               = reader.GetDateTime(16),
        CreationTime          = reader.GetDateTime(17),
        LastModificationTime  = reader.GetDateTime(18)
      };
    }

    protected override String GetQuery()
    {
      return String.Format(Query, LastExecutionTime.GetValueOrDefault(DateTime.Now).ToString("yyyy-MM-dd hh:mm:ss"));
    }
	}
}
