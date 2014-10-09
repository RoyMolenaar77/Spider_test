using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Models.Vendors;
using System.IO;

namespace Concentrator.Plugins.Sapph.MediaImport
{
  public class SapphMediaImport : MediaImportBase
  {
    public override string Name
    {
      get { return "Started importing media for Sapph"; }
    }

    protected override bool NeedsBackup()
    {
      return false;
    }

    protected override int VendorID
    {
      get { return Int32.Parse(GetConfiguration().AppSettings.Settings["VendorID"].Value); }
    }

    protected override string GetWatchDirectory(Objects.DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      return unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == VendorID && c.SettingKey == "ImageDirectory").Value;
    }

    protected override VendorImageInfo GetImageInfo(string imageName)
    {
      VendorImageInfo info = new VendorImageInfo();

      String[] filearray = imageName.Split('_');

      if (filearray.Length < 3)
      {
        log.InfoFormat("{0} is not correct", imageName);

        log.InfoFormat("ERROR!! File name is not in the right format... File name needs two underscores.");

        return null;
      }

      int seq = 0;

      switch (filearray[2].ToLower())
      {
        case "m":
          seq = 1;
          break;
        case "b1":
          seq = 2;
          break;
        case "d1":
          seq = 3;
          break;
        case "f":
          seq = 4;
          break;
        case "b":
          seq = 5;
          break;
        case "d":
          seq = 6;
          break;
        case "d2":
          seq = 7;
          break;
        default:
          seq = 9;
          break;
      }

      info.Name = imageName;
      info.Description = filearray[0];
      info.Sequence = seq;
      info.IsThumbnail = false;

      return info;
    }

    protected override List<Objects.Models.Vendors.VendorAssortment> GetVendorProducts(Objects.DataAccess.UnitOfWork.IUnitOfWork unit, string imageName)
    {
      Int32[] _vendorIDs = GetConfiguration().AppSettings.Settings["VendorIDs"].Value.Split(',').Select(x => Convert.ToInt32(x)).ToArray();

      String[] filearray = imageName.Split('_');

      String CustomItemNumber = filearray[0];
      String ColorCode = filearray[1].Replace('-', '/');

      var vendorAssortment = new List<Objects.Models.Vendors.VendorAssortment>();

      foreach (var vendorID in _vendorIDs)
      {
        var assortment = unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == vendorID && (x.CustomItemNumber.Trim() == CustomItemNumber
        || x.CustomItemNumber.Trim() == CustomItemNumber + " " + ColorCode
        || x.CustomItemNumber.Trim().StartsWith(CustomItemNumber + " " + ColorCode + " "))).ToList();

        vendorAssortment.AddRange(assortment);
      }

      return vendorAssortment;
    }

    protected override string GetFilename(string fullImagePath)
    {
      return string.Format("{0}-{1}", DateTime.Now.ToString("ddMMyyyy-hhmm"), Path.GetFileName(fullImagePath));
    }
  }
}
