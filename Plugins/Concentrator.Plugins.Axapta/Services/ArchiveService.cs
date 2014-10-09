using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Concentrator.Objects.Ftp;
using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Plugins.Axapta.Repositories;
using FileHelpers;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Services
{
  public class ArchiveService : IArchiveService
  {
    private string _directoryPathName;
    private readonly string _archiveDirectoryPath;

    private const int SapphVendorID = 50;

    #region Vendor Setting Keys
    private const string FtpAddressSettingKey = "FtpAddress";
    private const string FtpUsernameSettingKey = "FtpUsername";
    private const string FtpPasswordSettingKey = "FtpPassword";

    private const string ArchiveDirectorySettingKey = "Archive Directory";

    private const string StockDirectorySettingKey = "Ax Ftp Dir Stock";
    private const string CorrectionStockDirectorySettingKey = "Ax Ftp Dir StockMutation";
    private const string CorruptStockDirectorySettingKey = "Ax Ftp Dir CorruptImportedStock";
    private const string CorruptCorrectionStockDirectorySettingKey = "Ax Ftp Dir Corrupt StockMutation";

    private const string PurchaseOrderDirectorySettingKey = "Ax Ftp Dir PurchaseOrder";
    private const string PurchaseOrderReceivedConfirmationDirectorySettingKey = "Ax Ftp Dir ReceivedConfirmation PurchaseOrder";
    private const string CorruptPurchaseOrderDirectorySettingKey = "Ax Ftp Dir Corrupt Purchase Order";

    private const string PickTicketDirectorySettingKey = "Ax Ftp Dir PickTicket";
    private const string TransferShipmentConfirmationDirectorySettingKey = "Ax Ftp Dir ShipmentConfirmation Transfer";
    private const string PickingShipmentConfirmationDirectorySettingKey = "Ax Ftp Dir ShipmentConfirmation Picking";
    private const string CorruptPickTicketDirectorySettingKey = "Ax Ftp Dir Corrupt PickTicket Order";
    private const string CorruptNotificationDirectorySettingKey = "Ax Ftp Dir Corrupt Notification";

    private const string CustomerInformationDirectorySettingKey = "Ax Ftp Dir CustomerInformation";
    private const string CorruptCustomerInformationDirectorySettingKey = "Ax Ftp Dir Corrupt CustomerInformation";
    #endregion

    private readonly IVendorSettingRepository _vendorSettingRepo;

    public ArchiveService(IVendorSettingRepository vendorSettingRepo)
    {
      _vendorSettingRepo = vendorSettingRepo;

      _archiveDirectoryPath = _vendorSettingRepo.GetVendorSetting(SapphVendorID, ArchiveDirectorySettingKey);
    }

    public void ExportToArchive<T>(IEnumerable<T> listOfDatCol, SaveTo saveTo, string fileName)
    {
      SetArchiveDirectoryPath(saveTo);

      var filePathName = Path.Combine(_directoryPathName, fileName);
      if (!File.Exists(filePathName))
      {
        File.Create(filePathName).Dispose();
      }

      var engine = new FileHelperEngine(typeof(T));
      engine.WriteFile(filePathName, listOfDatCol);
    }

    public void CopyToArchive(string ftpUri, SaveTo saveTo, string fileNameToCopy)
    {
      SetArchiveDirectoryPath(saveTo);

      var ftpManager = new FtpManager(ftpUri, null, false, true);

      foreach (var file in ftpManager.GetFiles())
      {
        var fileName = Path.GetFileName(file);

        if (fileName != null && !fileName.Equals(Path.GetFileName(fileNameToCopy))) continue;
        try
        {
          using (var stream = ftpManager.Download(fileName))
          {
            var filePath = Path.Combine(_directoryPathName, fileNameToCopy);

            try
            {
              SaveStreamToFile(filePath, stream);
            }
            catch (IOException exception)
            {
              throw new Exception(exception.Message);
            }
          }
          return;
        }
        catch (Exception e)
        {
          throw new Exception(string.Format("Could not process file {0}! Error: {1}", file, e.Message));
        }
      }
    }

    private static void SaveStreamToFile(string fileFullPath, Stream stream)
    {
      if (stream.Length == 0) return;
      using (var fileStream = File.Create(fileFullPath, (int)stream.Length))
      {
        var bytesInStream = new byte[stream.Length];
        stream.Read(bytesInStream, 0, bytesInStream.Length);

        fileStream.Write(bytesInStream, 0, bytesInStream.Length);
      }
    }

    public void ExportToAxapta<T>(IEnumerable<T> listOfDatCol, SaveTo saveTo, string fileName)
    {
      var ofDatCol = listOfDatCol as IList<T> ?? listOfDatCol.ToList();
      if (!ofDatCol.Any()) return;

      using (Stream memoryStream = new MemoryStream())
      using (var streamWriter = new StreamWriter(memoryStream))
      {
        var engine = new FileHelperEngine(typeof(T));
        engine.WriteStream(streamWriter, ofDatCol.ToList());

        streamWriter.Flush();

        var ftpManager = GetFtpManager(saveTo);
        ftpManager.Upload(memoryStream.Reset(), fileName);
      }
    }

    public void StreamToAxapta(Stream stream, SaveTo saveTo, string fileName)
    {
      using (var streamWriter = new StreamWriter(stream))
      {
        streamWriter.Flush();

        var ftpManager = GetFtpManager(saveTo);
        ftpManager.Upload(stream.Reset(), fileName);
      }
    }

    private void SetArchiveDirectoryPath(SaveTo saveTo)
    {
      var directory = GetDirectory(saveTo);

      _directoryPathName = Path.Combine(_archiveDirectoryPath, String.Format("{0:yyyy-MM}", DateTime.Now), directory, String.Format("{0:dd}", DateTime.Now));

      if (!Directory.Exists(_directoryPathName))
      {
        Directory.CreateDirectory(_directoryPathName);
      }
    }

    private FtpManager GetFtpManager(SaveTo saveTo)
    {
      var ftpSetting = new FtpSetting
        {
          FtpAddress = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpAddressSettingKey),
          FtpUsername = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpUsernameSettingKey),
          FtpPassword = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpPasswordSettingKey),
          Path = GetDirectory(saveTo)
        };

      var ftpManager = new FtpManager(ftpSetting.FtpUri, null, false, true);

      return ftpManager;
    }

    private string GetDirectory(SaveTo saveTo)
    {
      string directory;
      switch (saveTo)
      {
        case SaveTo.StockDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, StockDirectorySettingKey);
          break;
        case SaveTo.CorrectionStockDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorrectionStockDirectorySettingKey);
          break;
        case SaveTo.CorruptStockDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorruptStockDirectorySettingKey);
          break;
        case SaveTo.CorruptCorrectionStockDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorruptCorrectionStockDirectorySettingKey);
          break;

        case SaveTo.PurchaseOrderDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PurchaseOrderDirectorySettingKey);
          break;
        case SaveTo.ReceivedPurchaseOrderConfirmationDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PurchaseOrderReceivedConfirmationDirectorySettingKey);
          break;
        case SaveTo.CorruptPurchaseOrderDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorruptPurchaseOrderDirectorySettingKey);
          break;

        case SaveTo.PickTicketDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PickTicketDirectorySettingKey);
          break;
        case SaveTo.PickingPickTicketShipmentConfirmationDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PickingShipmentConfirmationDirectorySettingKey);
          break;
        case SaveTo.TransferPickTicketShipmentConfirmationDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, TransferShipmentConfirmationDirectorySettingKey);
          break;
        case SaveTo.CorruptPickTicketDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorruptPickTicketDirectorySettingKey);
          break;
        case SaveTo.CorruptNotificationDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorruptNotificationDirectorySettingKey);
          break;

        case SaveTo.CustomerInformationDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CustomerInformationDirectorySettingKey);
          break;
        case SaveTo.CorruptCustomerInformationDirectory:
          directory = _vendorSettingRepo.GetVendorSetting(SapphVendorID, CorruptCustomerInformationDirectorySettingKey);
          break;

        default:
          throw new NotImplementedException();
      }

#if DEBUG
      return directory.Replace("Staging", "Test");
#else
      return directory;
#endif
    }
  }
}
