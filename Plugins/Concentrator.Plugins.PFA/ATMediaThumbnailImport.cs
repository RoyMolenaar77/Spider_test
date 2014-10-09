using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.PFA
{
	class ATMediaThumbnailImport : ATMediaImport
	{
		protected override string GetWatchDirectory(IUnitOfWork unit)
		{
			return unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == 2 && c.SettingKey == "ThumbImageDirectory").Value;
		}

		protected override string GetBackupDirectory()
		{
			return GetConfiguration().AppSettings.Settings["ATBackupDirectory"].Value;
		}

		protected override VendorImageInfo GetImageInfo(string imageName)
		{
			VendorImageInfo info = new VendorImageInfo();

			string[] filearray = imageName.Split('_');

			if (filearray.Length < 3)
			{

				log.InfoFormat("{0} is not correct", imageName);
				log.InfoFormat("ERROR!! File name is not in the right format... File name needs two underscores.");

				return null;
			}

			if (filearray[2].ToLower().Equals("f") || filearray[2].ToLower().Equals("b"))
			{
				return null;
			}

			info.Name = imageName;
			info.Description = filearray[0];
			info.Sequence = 2;
			info.IsThumbnail = true;

			return info;
		}

		public override string Name
		{
			get { return "America Today FTP Thumbnail Image import"; }
		}

		protected override bool NeedsBackup()
		{
			return false;
		}
	}
}
