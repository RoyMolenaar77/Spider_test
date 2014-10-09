using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Models.Vendors;
using System;
using System.IO;
using Concentrator.Plugins.PFA.Helpers;
using System.Text.RegularExpressions;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.PFA
{
	public class ATMediaImport : MediaImportBase
	{
		protected override string GetWatchDirectory(IUnitOfWork unit)
		{
			return unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == 2 && c.SettingKey == "ImageDirectory").Value;
		}

		protected override string GetBackupDirectory()
		{
			return GetConfiguration().AppSettings.Settings["ATBackupDirectory"].Value;
		}

		protected override string GetFilename(string fullImagePath)
		{
			return string.Format("{0}-{1}", DateTime.Now.ToString("ddMMyyyy-hhmm"), Path.GetFileName(fullImagePath));
		}

		protected override VendorImageInfo GetImageInfo(string imageName)
		{
			VendorImageInfo info = new VendorImageInfo();


			string[] filearray = imageName.Split('_');
			var reg = "^\\w+_\\d+_[f|b|F|B|L|l|H|h]$";

			if (!Regex.IsMatch(imageName, reg))
			{

				log.InfoFormat("{0} is not correct", imageName);
				log.InfoFormat("ERROR!! File name is not in the right format... .");

				return null;
			}

			info.Name = imageName;
			info.Description = filearray[0];
			var imageDiscriminator = filearray[2].ToLower();

			try
			{
				info.Sequence = ATMediaHelper.GetImageSequence(filearray[2].ToLower());
			}
			catch (Exception e)
			{
				log.Warn("Invalid image " + imageName);
				info.Sequence = 7;
			}

			info.IsThumbnail = false;
			return info;
		}

		protected override List<VendorAssortment> GetVendorProducts(IUnitOfWork unit, string imageName)
		{
			string[] filearray = imageName.Split('_');

			string CustomItemNumber = filearray[0];
			string ColorCode = filearray[1];

			return unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == VendorID && (x.CustomItemNumber.Trim() ==
						CustomItemNumber ||
						 x.CustomItemNumber.Trim() == CustomItemNumber + " " + ColorCode ||
						x.CustomItemNumber.Trim().StartsWith(CustomItemNumber + " " + ColorCode + " "))).ToList();
		}

		protected override int VendorID
		{
			get { return 2; }
		}

		public override string Name
		{
			get { return "America Today FTP Image import"; }
		}

		protected override bool NeedsBackup()
		{
			return false;
		}
	}
}
