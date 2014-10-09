using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.ExtensionMethods;
using Concentrator.Plugins.Wehkamp.Helpers;
using PetaPoco;
using System.IO;
using Concentrator.Objects.SFTP;
using SBUtils;

namespace Concentrator.Plugins.Wehkamp
{
  public class ProductMediaExport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Product Media Export"; }
    }

    private const string VendorSettingKey = "WehkampMediaExportLastRun";

    public ProductMediaExport()
    {
      // secureblackbox license
      var m = new SBLicenseManager.TElSBLicenseManager();
      m.LicenseKey = "8F1317CD48AC4E68BABA5E339D8B365414D7ADA0289CA037E9074D29AD95FF3EC5D796BEFF0FBADB3BD82F48644C9EB810D9B5A305E0D2A1885C874D8BF974B9608CE918113FBE2AA5EEF8264C93B25ABEA98715DB4AD265F47CE02FC9952D69F2C3530B6ABAAA4C43B45E7EF6A8A0646DA038E34FBFB629C2BF0E83C6B348726E622EBD52CA05CF74C68F1279849CCD0C13EA673916BA42684015D658B8E7626F15BD826A4340EDB36CE55791A051FDBCF9FA1456C3B5008AD9990A0185C0EA3B19F9938CB7DA1FE82736ED4C7A566D4BFD53411E8380F4B020CB50E762520EFAE190836FD253B00DB18D4A485C7DC918AA4DCEC856331DD231CC8DC9C741C3";
    }

    internal void ProcessProductImages(List<int> productIDs, int vendorID)
    {
      if (!productIDs.Any())
        return;

      // loop door alle productid's en verwerk de images gerelateerd aan deze producten
      var pids = string.Join(",", productIDs.ToArray());

      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var images = db.Fetch<ProductMedia>(string.Format("SELECT MediaID, BrandID, ProductID, ManufacturerID, ImagePath, LastChanged FROM ImageView WHERE ProductID IN ({0})", pids));
        ProcessProductImages(db, images, vendorID);
      }
    }

    private bool ProcessProductImages(Database db, List<ProductMedia> productMedias, int vendorID)
    {
      if (!productMedias.Any())
        return true;

      var attributeid = db.FirstOrDefault<int>("SELECT AttributeID FROM ProductAttributeMetaData WHERE AttributeCode = @0", "WehkampProductNumber");
      if (attributeid == 0)
        throw new ConfigurationErrorsException("Attribute \"WehkampProductNumber\" is missing from the ProductAttributeMetadata table");

      var wehkampPids = new Dictionary<int, string>();

      if (productMedias.Count <= 500)
      {
        var productMediaArray = string.Join(",", productMedias.Select(c => c.ProductID).ToArray());
        wehkampPids = db.Dictionary<int, string>(string.Format("SELECT ProductID, Value FROM ProductAttributeValue WHERE AttributeID = {1} AND ProductID IN ({0})", productMediaArray, attributeid));
      }
      else
      {
        for (var i = 0; i < productMedias.Count; i += 500)
        {
          var productMediaArray = string.Join(",", productMedias.Skip(i).Take(Math.Min(500, productMedias.Count - i)).Select(c => c.ProductID).ToArray());
          var temp = db.Dictionary<int, string>(string.Format("SELECT ProductID, Value FROM ProductAttributeValue WHERE AttributeID = {1} AND ProductID IN ({0})", productMediaArray, attributeid));

          foreach (var pm in temp)
          { 
            if (!wehkampPids.ContainsKey(pm.Key))
              wehkampPids.Add(pm.Key, pm.Value);
          }
        }
      }

      var ftpClient = SftpHelper.CreateClient(VendorSettingsHelper.GetSFTPSetting(vendorID), CommunicatorHelper.GetWehkampPrivateKeyFilename(vendorID), "D1r@ct379");

      if (ftpClient == null)
      {
        log.AuditCritical("SFTP failed to connect");
        return false;
      }


      string manid, path, newFilename;
      int index;
      foreach (var media in productMedias)
      {
        if (!wehkampPids.ContainsKey(media.ProductID))
          continue;

        manid = media.ManufacturerID.Split(' ')[0];

        if (string.IsNullOrEmpty(manid))
          continue;

        index = media.ImagePath.LastIndexOf(manid, StringComparison.InvariantCultureIgnoreCase);

        if (index < 0)
          continue;

        path = Path.Combine(ConfigurationHelper.FTPMediaDirectory, media.ImagePath);

        if (!File.Exists(path))
        {
          log.Error(string.Format("Could not find image file {0}", path));
          continue;
        }

        newFilename = string.Format("{0}_{1}", wehkampPids[media.ProductID], media.ImagePath.Substring(index));

        try
        {
          //Only upload files ending on:
          //1. _b.png - Back
          //2. _f.png - Front
          //3. _h.png - Hoover

          //if (newFilename.ToLowerInvariant().EndsWith("_b.png") || newFilename.ToLowerInvariant().EndsWith("_f.png") || newFilename.ToLowerInvariant().EndsWith("_h.png"))
          if (newFilename.ToLowerInvariant().EndsWith("_b.png") || newFilename.ToLowerInvariant().EndsWith("_f.png") || newFilename.ToLowerInvariant().EndsWith("_h.png"))
          {
            ftpClient.UploadFile(path, Path.Combine("TO_WK", "Pictures", newFilename).ToUnixPath(), TSBFileTransferMode.ftmOverwrite);
          }
        }
        catch (Exception e)
        {
          log.Error(string.Format("Cannot upload file : {0}", media.ImagePath), e);
          return false;
        }
      }

      return true;
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        foreach (var vendorID in ConfigurationHelper.VendorIdsToDownload)
        {
          _monitoring.Notify(Name, vendorID);

          DateTime lastRun;
          var runStart = DateTime.Now;

          if (!DateTime.TryParse(db.SingleOrDefault<string>(string.Format("SELECT Value FROM VendorSetting WHERE VendorID='{0}' AND SettingKey='{1}'", vendorID, VendorSettingKey)), out lastRun))
            log.Info(string.Format("VendorSetting for \"{1}\" does not exist or has invalid data for vendorID {0} , doing a media export from the beginning of time", vendorID, VendorSettingKey));

          // if setting not present
          if (lastRun == DateTime.MinValue)
          {
            lastRun = SqlDateTime.MinValue.Value; // new DateTime(1753, 1, 1, 0, 0, 0); // tsql minimum date value
            db.Execute(string.Format("INSERT INTO VendorSetting (VendorID, SettingKey, Value) Values ({0}, '{1}', '{2}')", vendorID,
                                                                                                      VendorSettingKey,
                                                                                                      runStart.ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture)));
          }
          else
          {
            lastRun = lastRun.ToUniversalTime();
          }

          const string query = "SELECT MediaID, BrandID, ProductID, ManufacturerID, ImagePath, LastChanged FROM ImageView WHERE ConnectorID=@0 AND (@1 IS NULL OR LastChanged > @1)";

          var images = db.Fetch<ProductMedia>(query,
                                              VendorSettingsHelper.GetConnectorIDByVendorID(vendorID),
                                              lastRun);

          var processed = ProcessProductImages(db, images, vendorID);

          // finished
          if (processed)
          {
            db.Execute(string.Format("UPDATE VendorSetting SET Value='{2}' WHERE VendorID={0} AND SettingKey='{1}'",
              vendorID,
              VendorSettingKey,
              runStart.ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture)));
          }
        }
      }

      _monitoring.Notify(Name, 1);
    }
  }

  [PrimaryKey("MediaID")]
  internal class ProductMedia
  {
    public int MediaID { get; set; }
    public int BrandID { get; set; }
    public int ProductID { get; set; }
    public string ManufacturerID { get; set; }
    public string ImagePath { get; set; }
    public DateTime LastChanged { get; set; }
  }
}
