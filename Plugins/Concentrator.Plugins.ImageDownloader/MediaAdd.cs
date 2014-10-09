using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Configuration;
using System.IO;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.ImageDownloader
{
  public class MediaAdd : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Imagedownload MediaAdd Plugin"; }
    }

    protected override void Process()
    {
      string FTPMediaDirectory = ConfigurationManager.AppSettings["FTPMediaDirectory"];
      string ImageWatchDirectory = ConfigurationManager.AppSettings["ImageWatchDirectory"];

      string[] files = Directory.GetFiles(ImageWatchDirectory);

      using (var unit = GetUnitOfWork())
      {
        int TypeID = unit.Scope.Repository<MediaType>().GetSingle(x => x.Type == "Image").TypeID;

        int counter = 0;
        int total = files.Count();
        int totalNumberOfImagesToProcess = total;

        log.InfoFormat("Start: adding {0} images", total);

        foreach (var image in files)
        {
          var _productmediaRepo = unit.Scope.Repository<ProductMedia>();

          #region Imageinfo
          //Retrieve all info; look in watchedfolder and look up corresponding info in database

          string filename = Path.GetFileName(image);
          string filewithoutextension = Path.GetFileNameWithoutExtension(image);

          string[] filearray = filewithoutextension.Split('_');
          string CustomItemNumber = filearray[0];
          string ColorCode = filearray[1];
          string FrontOrBack = filearray[2];
          string Extension = Path.GetExtension(image);
          string Resolution = MediaUtility.getRes(Path.Combine(ImageWatchDirectory, filename));
          int? Size = MediaUtility.getSize(Path.Combine(ImageWatchDirectory, filename)).HasValue ? MediaUtility.getSize(Path.Combine(ImageWatchDirectory, filename)) : null;
          string Description = filearray[1] + filearray[2];

          string Sequence = FrontOrBack.Equals("f") ? "0" : "1";

          var vendorAssortment = (from a in unit.Scope.Repository<VendorAssortment>().GetAll(x => x.CustomItemNumber.Trim() == 
            CustomItemNumber || x.CustomItemNumber.Trim().StartsWith(CustomItemNumber + ColorCode)).ToList()
                                  select new ProductMedia
                                  {
                                    ProductID = a.ProductID,
                                    Sequence = int.Parse(Sequence),
                                    VendorID = a.VendorID,
                                    TypeID = TypeID,
                                    MediaUrl = null,
                                    MediaPath = Path.Combine("Products", filename),
                                    FileName = null,
                                    MediaID = 0,
                                    Description = Description,
                                    Resolution = Resolution,
                                    Size = Size
                                  });
          #endregion

          #region Progresscount
          //Progresscounter

          log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfImagesToProcess, total, total - totalNumberOfImagesToProcess);

          totalNumberOfImagesToProcess--;
          counter++;
          #endregion

          //Add to database
          _productmediaRepo.Add(vendorAssortment);

          //Move image to FTPMediaDirectory
          System.IO.File.Move(Path.Combine(ImageWatchDirectory, filename), Path.Combine(FTPMediaDirectory + filename));
        }
        unit.Save();
      }
    }
  }
}

