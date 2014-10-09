using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using System.Drawing;
using Concentrator.Objects.Models.Media;
using System.IO;
using System.Configuration;
using Concentrator.Objects.Converters;
using Concentrator.Objects.Images;

namespace Concentrator.Plugins.ImageDownloader
{
  public class ThumbGenerator : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Thumbnail generator"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var thumbs = unit.Scope.Repository<ThumbnailGenerator>().GetAll().ToList();
        var tumbTypes = GetConfiguration().AppSettings.Settings["ThumbTypeIDs"].Value.Split(',').Select(x => int.Parse(x));
        var mediaPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
        var thumbPath = Path.Combine(mediaPath, ConfigurationManager.AppSettings["FTPMediaProductThumbDirectory"]);

        var productImages = unit.Scope.Repository<ProductMedia>().GetAll(x => tumbTypes.Contains(x.TypeID) && x.MediaPath != null).ToList();

        thumbs.ForEach(thumb =>
        {
          var destinationPath = Path.Combine(thumbPath, thumb.ThumbnailGeneratorID.ToString());

          if (!Directory.Exists(Path.Combine(destinationPath, "Products")))
            Directory.CreateDirectory(Path.Combine(destinationPath, "Products"));

          int width = thumb.Width;
          int heigth = thumb.Height;

          int couterProduct = 0;
          int logCount = 0;
          int totalProducts = productImages.Count;
          log.DebugFormat("Found {0} media items to process for thumb generator {1}", totalProducts, thumb.Description);

          productImages.ForEach(media =>
          {
            couterProduct++;
            logCount++;
            if (logCount == 50)
            {
              log.DebugFormat("Media Processed : {0}/{1}", couterProduct, totalProducts);
              logCount = 0;
            }

            try
            {
              var pathToLook = Path.Combine(mediaPath, media.MediaPath);

              var ext = Path.GetExtension(pathToLook).ToLower();
              var isTiff = (ext == ".tiff" || ext == ".tif");
               FileInfo inf = new FileInfo(pathToLook);
              var destinationFilePath = Path.Combine(destinationPath, inf.Name);

              ProductMediaTumbnail productThumb = unit.Scope.Repository<ProductMediaTumbnail>().GetSingle(x => x.MediaID == media.MediaID && x.ThumbnailGeneratorID == thumb.ThumbnailGeneratorID);
               
              if (System.IO.File.Exists(pathToLook) && (productThumb == null || !File.Exists(destinationFilePath) || System.IO.File.GetLastWriteTime(pathToLook) > System.IO.File.GetLastWriteTime(destinationFilePath)))
              {

                if (!File.Exists(destinationFilePath))
                {
                  using (Image img = Image.FromFile(pathToLook))
                  {
                    if (isTiff)
                    {
                      TiffConverter converter = new TiffConverter(Path.Combine(pathToLook));
                      converter.WriteTo(destinationFilePath.Replace(ext, ".png"), width, heigth);
                    }
                    else
                    {
                      var image = ImageUtility.GetFixedSizeImage(img, width > 0 ? width : img.Width, heigth > 0 ? heigth : img.Height, true, Color.White);
                      image.Save(destinationFilePath.Replace(ext, ".png"), System.Drawing.Imaging.ImageFormat.Png);
                    }
                  }
                }

                if (productThumb == null)
                {
                  productThumb = new ProductMediaTumbnail()
                  {
                    MediaID = media.MediaID,
                    ThumbnailGeneratorID = thumb.ThumbnailGeneratorID,
                    Path = Path.Combine(thumb.ThumbnailGeneratorID.ToString(),inf.Name.Replace(ext,".png"))
                  };

                  unit.Scope.Repository<ProductMediaTumbnail>().Add(productThumb);
                  unit.Save();
                }
              }
            }
            catch (Exception ex)
            {
              log.Error("Fail to Generate thumb for " + media.MediaPath, ex);
            }
          });
        });
      }
    }
  }
}
