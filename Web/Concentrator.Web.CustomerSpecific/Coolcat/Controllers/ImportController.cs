using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.CustomerSpecific.Coolcat.Repositories;
using Concentrator.Web.CustomerSpecific.Coolcat.Models;
using System.Xml.Linq;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Web.CustomerSpecific.Coolcat.Controllers
{
	public class ImportController : BaseController
	{
		private int _vendorID = 1;
		private string _settingKey = "ImportRules";

		public ImportController()
		{
			using (var unit = GetUnitOfWork())
			{
				var sett = unit.Service<VendorSetting>().Get(c => c.VendorID == _vendorID && c.SettingKey == _settingKey);
				if (sett == null)
				{
					sett = new VendorSetting()
					{
						VendorID = _vendorID,
						SettingKey = _settingKey,
						Value = (new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Rules"))).ToString()
					};
					unit.Service<VendorSetting>().Create(sett);
					unit.Save();
				}

			}
		}

		[RequiresAuthentication(Functionalities.GetSeasonRules)]
		public ActionResult GetAll()
		{

			using (var unit = GetUnitOfWork())
			{
				var document = XDocument.Parse(unit.Service<VendorSetting>().Get(c => c.VendorID == _vendorID && c.SettingKey == _settingKey).Value);
				var repo = new ImportSeasonCodeRuleRepository(document);

				var results = repo.GetAllRules().AsQueryable();

				return List(results.AsQueryable());
			}
		}

		[RequiresAuthentication(Functionalities.CreateSeasonRules)]
		public ActionResult Create(ImportSeasonCodeRuleModel model)
		{

			using (var unit = GetUnitOfWork())
			{
				try
				{
					var setting = unit.Service<VendorSetting>().Get(c => c.VendorID == _vendorID && c.SettingKey == _settingKey);
					var document = XDocument.Parse(setting.Value);
					var repo = new ImportSeasonCodeRuleRepository(document);

					repo.Add(model);
					setting.Value = document.ToString();

					unit.Save();
					return Success("Successfully added rule");
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
		}

		[RequiresAuthentication(Functionalities.UpdateSeasonRules)]
		public ActionResult Update(ImportSeasonCodeRuleModel model, string _SeasonCode)
		{
			using (var unit = GetUnitOfWork())
			{
				try
				{
					var setting = unit.Service<VendorSetting>().Get(c => c.VendorID == _vendorID && c.SettingKey == _settingKey);
					var document = XDocument.Parse(setting.Value);
					var repo = new ImportSeasonCodeRuleRepository(document);

					model.SeasonCode = _SeasonCode;
					repo.Update(model);
					setting.Value = document.ToString();
					unit.Save();
					return Success("Successfully modified rule");
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
		}

		[RequiresAuthentication(Functionalities.DeleteSeasonRules)]
		public ActionResult Delete(ImportSeasonCodeRuleModel model, string _SeasonCode)
		{
			using (var unit = GetUnitOfWork())
			{
				try
				{
					var setting = unit.Service<VendorSetting>().Get(c => c.VendorID == _vendorID && c.SettingKey == _settingKey);
					var document = XDocument.Parse(setting.Value);
					var repo = new ImportSeasonCodeRuleRepository(document);
					model.SeasonCode = _SeasonCode;
					repo.Delete(model);
					setting.Value = document.ToString();
					unit.Save();
					return Success("Successfully deleted rule");
				}
				catch (Exception e)
				{
					return Failure("Something went wrong: ", e);
				}
			}
		}
	}
}
