using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.IO;

namespace Concentrator.Plugins.Sapph.MediaImport
{
  public class ThumbnailImport : MediaImportBase
  {
    public override string Name
    {
      get { return "Sappph thumbnail import"; }
    }

    protected override bool MoveAfterProcess()
    {
      return true;
    }

    protected override int VendorID
    {
      get { return Int32.Parse(GetConfiguration().AppSettings.Settings["VendorID"].Value); }
    }

    protected override string GetWatchDirectory(IUnitOfWork unit)
    {
#if DEBUG
      return @"D:\sapph\color";
#endif

      return unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == VendorID && c.SettingKey == "ThumbnailDirectory").Value;
    }

    protected override VendorImageInfo GetImageInfo(string imageName)
    {
      var info = new VendorImageInfo { Name = imageName, Description = "Color palette", Sequence = 5, IsThumbnail = true };

      return info;
    }

    protected override List<VendorAssortment> GetVendorProducts(IUnitOfWork unit, string imageName)
    {
      Int32[] _vendorIDs = GetConfiguration().AppSettings.Settings["VendorIDs"].Value.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
      List<VendorAssortment> vendorAssortment = new List<VendorAssortment>();


      if (imageName.Contains('_'))
      {
        foreach (var vendorID in _vendorIDs)
        {
          var queryBuilder = new StringBuilder();

          queryBuilder.Append(string.Format(@"
          SELECT va.* 
          FROM dbo.Product p
          INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
          WHERE va.vendorid = {0} ", vendorID
          ));

          var splitImageName = imageName.Split(new char[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

          splitImageName.ForEach(
            (part, idx) => queryBuilder.AppendLine(string.Format(" and VendorItemNumber like '%{0}%'", part)));

          var assortment = unit.ExecuteStoreQuery<VendorAssortment>(queryBuilder.ToString()).ToList();

          vendorAssortment.AddRange(assortment);
        }

        return vendorAssortment;
      }

      foreach (var vendorID in _vendorIDs)
      {
        var va = unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == vendorID).ToList();
        
        var assortment = (from p in va
              let splitCIN = p.CustomItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
              where splitCIN.Count() > 1 && splitCIN[1].ToLower().Replace('/', '-') == imageName.ToLower()
              select p).ToList();

        vendorAssortment.AddRange(assortment);
      }

      return vendorAssortment;
    }

    protected override string GetFilename(string fullImagePath)
    {
      return Path.GetFileName(fullImagePath);
    }
  }
}
