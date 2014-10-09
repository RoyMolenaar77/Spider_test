using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.PFA.Models;
using System.IO;
using Excel;
using System.Data;

namespace Concentrator.Plugins.PFA.Repos
{
	public class StoreOrderRepository
	{
		private string excelPath;
		public StoreOrderRepository(string pathToExcel)
		{
			excelPath = pathToExcel;
		}

		public List<StoreOrderModel> GetOrders()
		{
			if (!File.Exists(excelPath))
				throw new FileNotFoundException("Excel file with order not found: " + excelPath);


			using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
			{
				using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
				{

					DataSet result = excelReader.AsDataSet();

					System.Data.DataTable dt = result.Tables[0];

					return (from c in dt.AsEnumerable()
									where !string.IsNullOrEmpty(c.Field<string>(0))
									group c by c.Field<string>(4) into storeOrders
									let fi = storeOrders.FirstOrDefault()
									where fi != null
									select new StoreOrderModel()
									{
										Store_Number = storeOrders.Key + "X",
										Address1 = fi.Field<string>(5),
										Address2 = fi.Field<string>(6),
										City = fi.Field<string>(8),
										Country = fi.Field<string>(9),
										Postcode = fi.Field<string>(7),
										OrderLines = (from m in storeOrders
																	select new StoreOrderLineModel()
																	{
																		Art_Number = m.Field<string>(0).Trim(),
																		Size_Code = m.Field<string>(2).Trim(),
																		Color_Code = m.Field<string>(1).Trim(),
																		Quantity = int.Parse(m.Field<string>(3))
																	}).ToList()
									}).ToList();
				}
			}
		}
	}
}
