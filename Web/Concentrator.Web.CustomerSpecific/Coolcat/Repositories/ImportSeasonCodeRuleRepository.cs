using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Web.CustomerSpecific.Coolcat.Models;
using System.Xml.Linq;
using System.Configuration;

namespace Concentrator.Web.CustomerSpecific.Coolcat.Repositories
{
	public class ImportSeasonCodeRuleRepository
	{
		private XDocument doc;

		public ImportSeasonCodeRuleRepository(XDocument document)
		{
			doc = document;
		}


		public void Add(ImportSeasonCodeRuleModel model)
		{
			doc.Root.Add(new XElement("Rule",
					new XElement("Season", model.SeasonCode),
					new XElement("ProductGroups", model.ProductGroupCodes)
				 ));
		}

		public List<ImportSeasonCodeRuleModel> GetAllRules()
		{
			List<ImportSeasonCodeRuleModel> models = new List<ImportSeasonCodeRuleModel>();

			models = (from r in doc.Root.Elements("Rule")
								select new ImportSeasonCodeRuleModel()
								{
									SeasonCode = r.Element("Season").Value,
									ProductGroupCodes = r.Element("ProductGroups").Value
								}).ToList();


			return models;
		}

		public void Update(ImportSeasonCodeRuleModel model)
		{
			var xmlEntry = doc.Root.Elements("Rule").Where(c => c.Element("Season").Value == model.SeasonCode).FirstOrDefault();
			xmlEntry.Element("ProductGroups").Value = model.ProductGroupCodes;
		}

		internal void Delete(ImportSeasonCodeRuleModel model)
		{
			var xmlEntry = doc.Root.Elements("Rule").Where(c => c.Element("Season").Value == model.SeasonCode).FirstOrDefault();
			xmlEntry.Remove();
		}
	}
}
