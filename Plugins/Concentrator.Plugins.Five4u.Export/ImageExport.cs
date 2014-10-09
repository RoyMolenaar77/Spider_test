using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;
using System.Drawing;

namespace Concentrator.Plugins.Five4u.Export
{
  public class ImageExport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Five4u Image Export"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var images = (from i in unit.Scope.Repository<ProductMedia>().GetAll(x => x.Sequence == 0 && x.MediaPath != null)
                      let va = i.Product.VendorAssortments.FirstOrDefault()
                      select new
                      {
                        ItemNumber = va != null ? va.CustomItemNumber : i.Product.VendorItemNumber,
                        i.MediaPath
                      }).ToList();

        var config = GetConfiguration();

        string mediaPath = config.AppSettings.Settings["ConcentratorMedia"].Value;
        string exportMediaPath = config.AppSettings.Settings["ExportPath"].Value;

        images.ForEach(x =>
        {
          string fileName = Regex.Replace(x.ItemNumber, @"[?:\/*""<>|]", "") + ".png";

          string fullPath = Path.Combine(mediaPath, x.MediaPath);

          try
          {

            if (File.Exists(fullPath))
            {
              var exportFile = Path.Combine(exportMediaPath, fileName);

              using (Image img = Image.FromFile(fullPath))
              {
                img.Save(exportFile, System.Drawing.Imaging.ImageFormat.Png);
              }
            }
          }
          catch (Exception ex)
          {
            log.Error(ex);
          }

        });
      }
    }
  }
}
