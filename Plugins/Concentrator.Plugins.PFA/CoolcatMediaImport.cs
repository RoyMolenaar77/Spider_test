using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Models.Vendors;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.PFA
{
  public class CoolcatMediaImport : MediaImportBase
  {
    protected override VendorImageInfo GetImageInfo(string imageName)
    {
      VendorImageInfo info = new VendorImageInfo();

      string[] filearray = imageName.Split('_');
      var reg = "^\\w+_\\w+_\\d+_[f|b|F|B|i|I]$";

      if (!Regex.IsMatch(imageName, reg))
      {

        log.InfoFormat("{0} is not correct", imageName);
        log.InfoFormat("ERROR!! File name is not in the right format... File name needs three underscores.");

        return null;
      }

      info.Name = imageName;
      info.Description = filearray[0];

      //Determine sequence
      switch (filearray[3].ToLower())
      {
        case "f":
          info.Sequence = 0;
          break;
        case "b":
          info.Sequence = 1;
          break;
        case "i":
          info.Sequence = 2;
          break;
        default:
          info.Sequence = 1;
          break;
      }

      return info;
    }

    protected override string GetFilename(string fullImagePath)
    {
      return string.Format("{0}-{1}", DateTime.Now.ToString("ddMMyyyy-hhmm"), Path.GetFileName(fullImagePath));
    }

    protected override List<VendorAssortment> GetVendorProducts(IUnitOfWork unit, string imageName)
    {
      string[] filearray = imageName.Split('_');

      string CustomItemNumber = filearray[1];
      string ColorCode = filearray[2];

      return unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == VendorID && (x.CustomItemNumber.Trim() ==
            CustomItemNumber ||
             x.CustomItemNumber.Trim() == CustomItemNumber + " " + ColorCode ||
            x.CustomItemNumber.Trim().StartsWith(CustomItemNumber + " " + ColorCode + " "))).ToList();
    }

    protected override int VendorID
    {
      get { return 1; }
    }

    public override string Name
    {
      get { return "Coolcat FTP Image import"; }
    }

    protected override bool NeedsBackup()
    {
      return false;
    }
  }
}
