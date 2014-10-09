using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.IO;
using System.Configuration;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Security.AccessControl;
using System.Security.Permissions;
using Excel;
using System.Data;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;

namespace Concentrator.ui.Management.Controllers
{
	[HandleError]
	[OutputCache(Location = OutputCacheLocation.None)]
	public class FilesController : BaseController
	{
		[RequiresAuthentication(Functionalities.GetFiles)]
		public ActionResult GetList()
		{
			List<FileInfo> fileInfo = new List<FileInfo>();

			//var dirs = Directory.GetDirectories(ConfigurationManager.AppSettings["FilesExportPath"]).ToList();

			//foreach (var dir in dirs)
			//{
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["FilesExportPath"]);

			var filesInPath = (from r in files
												 select new FileInfo(r)).ToList();

			fileInfo.AddRange(filesInPath);
			//}      

			return Json(new
			{
				total = fileInfo.Count,
				results = (from r in fileInfo
									 select new
									 {
										 r.Name,
										 r.FullName,
										 CreationTime = r.LastAccessTime

									 })
			});
		}

		[RequiresAuthentication(Functionalities.GetFiles)]
		public ActionResult Download(string path)
		{
			if (path.EndsWith("xls"))
				return new Concentrator.Web.Shared.Results.FileResult(path, string.Empty, "application/vnd.ms-excel");
			else if (path.EndsWith("xlsx"))
				return new Concentrator.Web.Shared.Results.FileResult(path, string.Empty, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			else if (path.EndsWith("xml"))
				return new Concentrator.Web.Shared.Results.FileResult(path, string.Empty, "text/xml");
			else
				return new Concentrator.Web.Shared.Results.FileResult(path, string.Empty);
		}

		[RequiresAuthentication(Functionalities.GetFiles)]
		public ActionResult UploadFile(HttpPostedFileBase file)
		{
			try
			{
				using (var unit = GetUnitOfWork())
				{
					var path = ConfigurationManager.AppSettings["FTPFileUploadDirectory"];
					String fullPath = String.Empty;

					if (file.FileName.Contains("NL") || file.FileName.Contains("BE") || file.FileName.Contains("Import_"))
					{
						file.SaveAs(Path.Combine(path, file.FileName));
						file.InputStream.Close();
					}
					else if (file.FileName.StartsWith("TPP_"))
					{
						String oldName = file.FileName; String newName = oldName.Remove(0, 4);
						var thirdPartyPath = Path.Combine(path, ConfigurationManager.AppSettings["FTPThirdPartyDirectory"]);

						if (!Directory.Exists(thirdPartyPath)) Directory.CreateDirectory(thirdPartyPath);

						file.SaveAs(Path.Combine(thirdPartyPath, newName));
						file.InputStream.Close();
					}
					else if (file.FileName.StartsWith("WIP_"))
					{
						var webImportPath = Path.Combine(path, ConfigurationManager.AppSettings["FTPWebImportDirectory"]);

						if (!Directory.Exists(webImportPath)) Directory.CreateDirectory(webImportPath);

						file.SaveAs(Path.Combine(webImportPath, file.FileName));
						file.InputStream.Close();
					}
					else if (file.FileName.Contains("AdvancedPricing"))
					{
						var webImportPath = Path.Combine(path, ConfigurationManager.AppSettings["FTPWebImportDirectory"]);
						if (!Directory.Exists(webImportPath))
						{
							Directory.CreateDirectory(webImportPath);
						}
						string fileDateExtension = createDateExtension();
						string filename = makeFileName(file.FileName, fileDateExtension);
						string fullName = Path.Combine(webImportPath, filename);
						file.SaveAs(fullName);
						file.InputStream.Close();
						importExcelToDb(fullName);
					}
					else
					{
						Exception e = new Exception("File upload failed (filename does not contain NL/BE/import_)");
						return Failure("Failure: ", e, false);
					}

					unit.Save();

					return Success(file.FileName + " has been uploaded succesfully", isMultipartRequest: true);
				}
			}
			catch (Exception e)
			{
				return Failure("File upload failed for ", e, true);
			}
		}

		private void importExcelToDb(string fullName)
		{
			using (var unit = GetUnitOfWork())
			{
				var currentConnectorID = Client.User.ConnectorID;
				if (System.IO.File.Exists(fullName))
				{
					FileStream excelStream = System.IO.File.Open(fullName, FileMode.Open, FileAccess.Read);
					IExcelDataReader excelReader = null;
					excelReader = ExcelReaderFactory.CreateOpenXmlReader(excelStream);

					var ds = excelReader.AsDataSet();

					var content = (from p in ds.Tables[0].AsEnumerable().Skip(2)
												 let numberOfFields = p.ItemArray.Count()
												 select new
												 {
													 connectorID = Convert.ToInt32(p.Field<object>(numberOfFields - 1)),
													 vendorID = Convert.ToInt32(p.Field<object>(numberOfFields - 2)),
													 productID = Convert.ToInt32(p.Field<object>(numberOfFields - 3)),
													 Label = Convert.ToString(p.Field<object>(numberOfFields - 4)),
													 Price = Convert.ToDecimal(p.Field<object>(numberOfFields - 5)),
													 toDate = DateTime.FromOADate(Convert.ToDouble(p.Field<object>(numberOfFields - 6))),
													 fromDate = DateTime.FromOADate(Convert.ToDouble(p.Field<object>(numberOfFields - 7)))
												 }).ToList();
					//insert the content into the db
					foreach (var c in content)
					{
						var rows = unit.Service<ContentPrice>().GetAll().Where(x => (x.ProductID == c.productID) && (x.ConnectorID == currentConnectorID) && (x.VendorID == c.vendorID)).ToList();
						if (rows != null)
						{
							if (rows.Count != 0)
							{
								foreach (var r in rows)
								{
									if (!String.IsNullOrEmpty(c.Label))
									{
										r.ContentPriceLabel = c.Label;
										r.FromDate = c.fromDate;
										r.ToDate = c.toDate;
										r.FixedPrice = c.Price;
									}
								}
							}
							else
							{
								//create a new row
								var product = unit.Service<Product>().GetAll().Where(x => x.ProductID == c.productID).SingleOrDefault();
								if (product != null)
								{
									// find the brandid
									var brandID = product.BrandID;
									var ContentPriceRuleIndex = 4;
									var PriceRuleType = 1;
									var Margin = "+";
									var VendorID = Convert.ToInt32(c.vendorID);

									ContentPrice cp = new ContentPrice() { VendorID = VendorID, ConnectorID = currentConnectorID ?? 0, BrandID = brandID, ProductID = product.ProductID, Margin = Margin, CreatedBy = Client.User.UserID, CreationTime = DateTime.Now, ContentPriceRuleIndex = ContentPriceRuleIndex, PriceRuleType = PriceRuleType, FromDate = c.fromDate, ToDate = c.toDate, FixedPrice = c.Price, ContentPriceLabel = c.Label };
									unit.Service<ContentPrice>().Create(cp);

									//unit.Save();
								}

							}

						}

					}
					unit.Save();
				}
			}
		}



		private string makeFileName(string filename, string fileDateExtension)
		{
			string[] elements = filename.Split(new char[] { '.' });
			string extension = elements[elements.Length - 1];
			List<string> pathElements = new List<string>();
			for (var i = 0; i < elements.Length - 1; i++)
			{
				pathElements.Add(elements[i]);
			}
			pathElements.Add(fileDateExtension);
			pathElements.Add(extension);
			return String.Join(".", pathElements);
		}

		private string createDateExtension()
		{
			var d = DateTime.Now;
			var monthString = d.Month.ToString();
			if (d.Month < 10)
			{
				monthString = String.Format("0{0}", monthString);
			}
			var dayString = d.Day.ToString();
			if (d.Day < 10)
			{
				dayString = String.Format("0{0}", dayString);
			}
			var result = String.Format("{0}{1}{2}", d.Year.ToString(), monthString, dayString);
			return result;
		}

	}

}
