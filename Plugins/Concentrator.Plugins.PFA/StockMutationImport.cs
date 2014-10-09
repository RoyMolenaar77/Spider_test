using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using AuditLog4Net;
using AuditLog4Net.Adapter;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Stocks;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.SFTP;
using SBUtils;

namespace Concentrator.Plugins.PFA
{
	using Configuration;
	using Concentrator.Objects.Ftp;

	/// <summary>
	/// Represents the plugin that imports the stock mutations. 
	/// </summary>
	public sealed class StockMutationImport : ConcentratorPlugin
	{
		private const String StockMutationFileNamePattern = "stock_mutation_*.xml";

		public Dictionary<Uri, XDocument> Documents
		{
			get;
			private set;
		}

		public override String Name
		{
			get
			{
				return "TNT Fashion: Stock Mutation Import";
			}
		}

		protected override void Process()
		{
			using (var unit = GetUnitOfWork())
			{
				foreach (Vendor vendor in unit.Scope.Repository<Vendor>().GetAll().ToList().Where(x => x.VendorSettings.GetValueByKey<bool>("UseStockMutationImport", false)))
				{
					var stockMutationImporter = new StockMutationImporter(vendor, unit, log);

					stockMutationImporter.Execute(vendor.VendorSettings.GetValueByKey<string>("TNTSourceURI", string.Empty), vendor);
				}
			}
		}
	}
}