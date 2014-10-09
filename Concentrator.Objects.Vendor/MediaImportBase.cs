using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Vendors
{
  public abstract class MediaImportBase : ConcentratorPlugin
  {
    public abstract override string Name { get; }

    /// <summary>
    /// The vendorID for which the media import will run
    /// </summary>
    protected new abstract int VendorID { get; }

    protected virtual bool NeedsBackup()
    {
      return true;
    }

    protected virtual bool MoveAfterProcess()
    {
      return true;
    }

    /// <summary>
    /// Retrieve the image info based on the imageName
    /// </summary>
    /// <param name="imageName"></param>
    /// <returns></returns>
    protected abstract VendorImageInfo GetImageInfo(string imageName);

    /// <summary>
    /// Implement this function to retrieve a product/product to which the image will be attached based on the filename
    /// </summary>
    /// <param name="imageName"></param>
    /// <returns></returns>
    protected abstract List<VendorAssortment> GetVendorProducts(IUnitOfWork unit, string imageName);

    private string _ftpMediaDirectory;
    private string _productFtpDirectory;

    protected virtual string FtpMediaDirectory
    {
      get
      {
        if (string.IsNullOrEmpty(_ftpMediaDirectory))
          _ftpMediaDirectory = ConfigurationManager.AppSettings["FTPMediaDirectory"];

        return _ftpMediaDirectory;
      }
    }

    protected virtual string ProductFtpDirectory
    {
      get
      {
        if (string.IsNullOrEmpty(_productFtpDirectory))
          _productFtpDirectory = ConfigurationManager.AppSettings["ProductFtpDirectory"];

        return _productFtpDirectory;
      }
    }

    protected virtual string GetWatchDirectory(IUnitOfWork unit)
    {
      return unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == VendorID).VendorSettings.GetValueByKey("ImageDirectory", string.Empty);
    }

    protected virtual string GetBackupDirectory()
    {
      return ConfigurationManager.AppSettings["FTPMediaProductBackupDirectory"];
    }

    protected override void Process()
    {
      string FTPMediaDirectory = ConfigurationManager.AppSettings["FTPMediaDirectory"];

      string productFtpDirectory = ConfigurationManager.AppSettings["FTPMediaProductDirectory"];

      string productBackupDirectory = string.Empty;

      productBackupDirectory = GetBackupDirectory();

      using (var unit = GetUnitOfWork())
      {
        string ImageWatchDirectory = string.Empty;

        ImageWatchDirectory = GetWatchDirectory(unit);

        ImageWatchDirectory.ThrowIfNullOrEmpty(new InvalidDataException("Vendor directory cannot be null or empty. Set vendor setting 'ImageDirectory'"));

        string ProcessedDir = Path.Combine(ImageWatchDirectory, "Processed");

        if (!Directory.Exists(ProcessedDir))
          Directory.CreateDirectory(ProcessedDir);

        string[] files = Directory.GetFiles(ImageWatchDirectory);

        int TypeID = unit.Scope.Repository<MediaType>().GetSingle(x => x.Type == "Image").TypeID;

        int counter = 0;
        int total = files.Count();
        int totalNumberOfImagesToProcess = total;

        log.InfoFormat("Start: adding {0} images", total);
        var mediaRepo = unit.Scope.Repository<ProductMedia>();
        foreach (var image in files)
        {

          var _productmediaRepo = unit.Scope.Repository<ProductMedia>();

          #region Imageinfo
          //Retrieve all info; look in watchedfolder and look up corresponding info in database

          string originalFilename = Path.GetFileName(image);
          string filename = GetFilename(image);

          string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(image);

          string Extension = Path.GetExtension(image);
          string Resolution = MediaUtility.getRes(Path.Combine(ImageWatchDirectory, filename));
          int? Size = MediaUtility.getSize(Path.Combine(ImageWatchDirectory, filename)).HasValue ? MediaUtility.getSize(Path.Combine(ImageWatchDirectory, filename)) : null;


          var imageInfo = GetImageInfo(fileNameWithoutExtension);
          if (imageInfo == null) continue;//short circuit to the next item

          var vendorAssortment = GetVendorProducts(unit, fileNameWithoutExtension);

          if (vendorAssortment == null) vendorAssortment = new List<VendorAssortment>(); //catch null returns

          bool foundAssortment = vendorAssortment.Count > 0;

          foreach (var item in vendorAssortment)
          {
            var path = Path.Combine(productFtpDirectory, filename);

            var mediaItem = mediaRepo.GetSingle(c => c.VendorID == item.VendorID && c.ProductID == item.ProductID && c.TypeID == TypeID && c.FileName == fileNameWithoutExtension);

            if (mediaItem == null)
            {
              mediaItem = new ProductMedia()
              {
                VendorID = item.VendorID,
                ProductID = item.ProductID,
                Sequence = imageInfo.Sequence,
                Size = Size,
                Description = imageInfo.Description,
                TypeID = TypeID,
                Resolution = Resolution,
                IsThumbNailImage = imageInfo.IsThumbnail,
                LastChanged = DateTime.Now.ToUniversalTime()

              };
              mediaRepo.Add(mediaItem);
            }
            mediaItem.FileName = imageInfo.Name;
            mediaItem.MediaPath = Path.Combine(productFtpDirectory, filename);
            mediaItem.Resolution = Resolution;
            mediaItem.Sequence = imageInfo.Sequence;
          }

          #endregion

          #region Progresscount
          //Progresscounter
          if (counter % 200 == 0)
            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfImagesToProcess, total, total - totalNumberOfImagesToProcess);

          totalNumberOfImagesToProcess--;
          counter++;
          #endregion

          //move image to concentrator ftp dir 
          var pathOfFile = Path.Combine(FTPMediaDirectory, productFtpDirectory, filename);
          var pathOfBackupFile = Path.Combine(productBackupDirectory, filename);

          if (!foundAssortment) continue;

          try
          {
            System.IO.File.Copy(Path.Combine(ImageWatchDirectory, originalFilename), pathOfFile, true);

            unit.Save();

            //if (NeedsBackup())
            //{
            //  System.IO.File.Copy(Path.Combine(ImageWatchDirectory, originalFilename), pathOfBackupFile, true);
            //}

            if (File.Exists(Path.Combine(ProcessedDir, originalFilename)))
            {
              if (MoveAfterProcess())
                File.Delete(Path.Combine(ProcessedDir, originalFilename));
            }

            if (MoveAfterProcess())
              File.Move(Path.Combine(ImageWatchDirectory, originalFilename), Path.Combine(ProcessedDir, originalFilename));
          }
          catch (Exception e)
          {
            log.Debug("Failed to copy/move pictures ", e);
          }
        }

      }
    }

    /// <summary>
    /// Defaults to Path.GetFilename.
    /// Can be overriden for custom names (date prepended for example)
    /// </summary>
    /// <param name="fullImagePath"></param>
    /// <returns></returns>
    protected virtual string GetFilename(string fullImagePath)
    {
      return Path.GetFileName(fullImagePath);
    }
  }
}
