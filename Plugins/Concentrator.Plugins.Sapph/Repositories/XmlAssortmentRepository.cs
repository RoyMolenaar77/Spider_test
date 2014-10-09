using System.Web;
using Concentrator.Objects.Ftp;
using Concentrator.Plugins.Sapph.Models;
using Concentrator.Objects.DataAccess.UnitOfWork;

using AuditLog4Net.Adapter;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Concentrator.Objects.Models.Vendors;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Concentrator.Plugins.Sapph.Repositories
{
	public class XmlAssortmentRepository
	{
	  protected Dictionary<String, XDocument> Documents { get; private set; }
		protected IAuditLogAdapter Log { get; private set; }
		protected IUnitOfWork Unit { get; private set; }
		private readonly Dictionary<string, string> _vendorSetting = new Dictionary<string, string>();

		protected virtual Regex FileNameRegex
		{
			get
			{
				return new Regex(".*\\.xml$", RegexOptions.IgnoreCase);
			}
		}

    const int DefaultVendorID = 50;
		const string FtpAddressSettingKey = "FtpAddress";
		const string FtpUsernameSettingKey = "FtpUsername";
		const string FtpPasswordSettingKey = "FtpPassword";
    const string PathSettingKey = "Ax Ftp Dir ArticleInformation";

		public XmlAssortmentRepository(int vendorID, IUnitOfWork unit, IAuditLogAdapter log)
		{
			Log = log;
			Unit = unit;
			Documents = new Dictionary<string, XDocument>();

			_vendorSetting = Unit
				.Scope
				.Repository<VendorSetting>()
				.GetAll(x => x.VendorID == vendorID)
				.ToDictionary(item => item.SettingKey, item => item.Value);
		}

		public IEnumerable<AssortmentResult> GetAssortment(int vendorID)
		{
			var results = new List<AssortmentResult>();

      var currentAxaptaAccountNumber = Unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == vendorID && c.SettingKey == "AxaptaAccountNumber");      

      if (currentAxaptaAccountNumber != null)
      {
        LoadDocuments(currentAxaptaAccountNumber.Value);

        foreach (var file in Documents)
        {
          var result = new AssortmentResult
          {
            FileName = file.Key,
            Assortments =
              (from a in file.Value.Element("Envelop").Element("Header").Element("GroupOfArticles").Elements("Line")
               select new Assortment()
               {
                 SupplierArticlecode = a.Element("SupplierArticlecode").Value,
                 ColourcodeSupplier = a.Element("ColourcodeSupplier").Value,
                 SizeSupplier = a.Element("SizeSupplier").Value,
                 SubsizeSupplier = a.Element("SubsizeSupplier").Value,
                 Brand = a.Element("Brand").Value,
                 Barcode = a.Element("Article-identifier").Value,
                 ShortDescription = a.Element("Description").Value,
                 LongDescription = a.Element("Articlegroup-descriptionSector").Value,

                 Website = a.Element("Website").Value,

                 ProductGroupCode1 = a.Element("ArticlegroupcodeSector").Value,
                 ProductGroupCode2 = a.Element("SeasonSupplier").Value,

                 AdvicePrice = decimal.Parse(a.Element("AdvicePrice").Value, new CultureInfo("en-US")),
                 LabellingPrice = decimal.Parse(a.Element("LabellingPrice").Value, new CultureInfo("en-US"))

               }).ToList()
          };

          results.Add(result);
        }
      }
      else
      {
        Log.ErrorFormat("No VendorSetting found for SettingKey: AxaptaAccountNumber, skipping VendorID: {0}", vendorID);
        return null;
      }

			return results;
		}

    private void LoadDocuments(string currentAxaptaAccountNumber)
		{
      var regex = new Regex(".*_" + currentAxaptaAccountNumber + ".xml", RegexOptions.IgnoreCase);

      //#if DEBUG
      //foreach (var fileName in Directory.GetFiles(@"D:\tmp\Sapph\Axapta\ArticleInformation"))
      //{
      //  if (regex.IsMatch(Path.GetFileName(fileName)))
      //  {
      //    using (var stream = File.Open(fileName, FileMode.Open))
      //    {
      //      Documents[fileName] = XDocument.Load(stream);
      //    }
      //  }
      //}
      //#endif

      var ftpManager = new FtpManager(GetFtpUri(), Log, usePassive:true);

      foreach (var fileName in ftpManager.GetFiles())
      {
        if (Documents.Count >= 10) return;

        if (!regex.IsMatch(fileName)) continue;
        
        using (var stream = ftpManager.Download(Path.GetFileName(fileName)))
        {
          Documents[fileName] = XDocument.Load(stream);
        }
      }
		}

		private string GetFtpUri()
		{
      //If VendorSetting doesn't exist, take the VendorSetting from the DefaultVendorID
      var ftpUserNameSettingKey = _vendorSetting.ContainsKey(FtpUsernameSettingKey) ? _vendorSetting[FtpUsernameSettingKey] : Unit.Scope.Repository<VendorSetting>().GetSingle(x => x.VendorID == DefaultVendorID && x.SettingKey == FtpUsernameSettingKey).Value;
      var ftpPasswordSettingKey = _vendorSetting.ContainsKey(FtpPasswordSettingKey) ? _vendorSetting[FtpPasswordSettingKey] : Unit.Scope.Repository<VendorSetting>().GetSingle(x => x.VendorID == DefaultVendorID && x.SettingKey == FtpPasswordSettingKey).Value;
      var ftpAddressSettingKey = _vendorSetting.ContainsKey(FtpAddressSettingKey) ? _vendorSetting[FtpAddressSettingKey] : Unit.Scope.Repository<VendorSetting>().GetSingle(x => x.VendorID == DefaultVendorID && x.SettingKey == FtpAddressSettingKey).Value;
      var pathSettingKey = _vendorSetting.ContainsKey(PathSettingKey) ? _vendorSetting[PathSettingKey] : Unit.Scope.Repository<VendorSetting>().GetSingle(x => x.VendorID == DefaultVendorID && x.SettingKey == PathSettingKey).Value;

			var uriStr = string.Format("ftp://{0}:{1}@{2}/{3}",
        HttpUtility.UrlEncode(ftpUserNameSettingKey),
				HttpUtility.UrlEncode(ftpPasswordSettingKey),
        ftpAddressSettingKey,
        pathSettingKey);
			return uriStr;
		}
	}
}


