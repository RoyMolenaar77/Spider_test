using System;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.SFTP;
using Concentrator.Plugins.Wehkamp.ExtensionMethods;
using Concentrator.Plugins.Wehkamp.Helpers;
using SBSftpCommon;
using Concentrator.Plugins.Wehkamp.Enums;
using SBUtils;

namespace Concentrator.Plugins.Wehkamp
{
  public class Communicator : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Communicator Plugin"; }
    }

    private Dictionary<string, decimal> _downloadedFiles;

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      // secureblackbox license
      var m = new SBLicenseManager.TElSBLicenseManager();
      m.LicenseKey = "8F1317CD48AC4E68BABA5E339D8B365414D7ADA0289CA037E9074D29AD95FF3EC5D796BEFF0FBADB3BD82F48644C9EB810D9B5A305E0D2A1885C874D8BF974B9608CE918113FBE2AA5EEF8264C93B25ABEA98715DB4AD265F47CE02FC9952D69F2C3530B6ABAAA4C43B45E7EF6A8A0646DA038E34FBFB629C2BF0E83C6B348726E622EBD52CA05CF74C68F1279849CCD0C13EA673916BA42684015D658B8E7626F15BD826A4340EDB36CE55791A051FDBCF9FA1456C3B5008AD9990A0185C0EA3B19F9938CB7DA1FE82736ED4C7A566D4BFD53411E8380F4B020CB50E762520EFAE190836FD253B00DB18D4A485C7DC918AA4DCEC856331DD231CC8DC9C741C3";

      foreach (var vendorID in ConfigurationHelper.VendorIdsToDownload)
      {
        DownloadMessageFiles(vendorID);
        UploadMessageFiles(vendorID);
      }

      _monitoring.Notify(Name, 1);
    }

    private void DownloadMessageFiles(int vendorID)
    {
      var remoteFileList = new ArrayList();
      _downloadedFiles = new Dictionary<string, decimal>();

      var ftpClient = SftpHelper.CreateClient(VendorSettingsHelper.GetSFTPSetting(vendorID), CommunicatorHelper.GetWehkampPrivateKeyFilename(vendorID), "D1r@ct379");

      if (ftpClient == null)
      {
        log.AuditCritical("SFTP failed to connect");
        return;
      }

      remoteFileList.Clear();
      var allianceDirectoryName = string.Format("TO_{0}", VendorSettingsHelper.GetAlliantieName(vendorID).ToUpperInvariant());

      try
      {
        ftpClient.ListDirectory(allianceDirectoryName, remoteFileList);

        foreach (TElSftpFileInfo fileinfo in remoteFileList)
        {
          //log.Info(string.Format("Checking remote item '{0}'", fileinfo.Name));

          if (fileinfo.Attributes.FileType != TSBSftpFileType.ftFile && (fileinfo.Attributes.FileType != TSBSftpFileType.ftUnknown && !fileinfo.Name.Contains(".xml")))
          {
            //log.Info(string.Format("Not a file. Skip item '{0}'", fileinfo.Name));
            continue;
          }
          var message = MessageHelper.GetMessageByFilename(fileinfo.Name);

          // if this is a second attempt, only try again if the validation has failed or there was an error downloading
          if (message != null && message.Status != WehkampMessageStatus.ErrorDownload &&
              message.Status != WehkampMessageStatus.ValidationFailed)
          {
            //log.Info(string.Format("Skip downloading file '{0}'", fileinfo.Name));
            continue;
          }
          try
          {
            //log.Info(string.Format("Start downloading file '{0}'", fileinfo.Name));
            ftpClient.DownloadFile(Path.Combine(allianceDirectoryName, fileinfo.Name).ToUnixPath(), Path.Combine(ConfigurationHelper.IncomingFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), fileinfo.Name));
            
            ftpClient.RemoveFile(Path.Combine(allianceDirectoryName, fileinfo.Name).ToUnixPath());
          }
          catch (Exception e)
          {
            log.AuditError(string.Format("Cannot access {0} over SFTP: {1}\n{2}", allianceDirectoryName, e.Message, e.StackTrace));

            if (message != null)
              MessageHelper.UpdateMessageStatus(message.MessageID, WehkampMessageStatus.ErrorDownload);
          }
          finally
          {
            if (message != null)
            {
              MessageHelper.UpdateMessageAttempt(message.MessageID);

              if (message.Attempts + 1 >= 10)
                MessageHelper.UpdateMessageStatus(message.MessageID, WehkampMessageStatus.MaxRetryExceeded);
            }
          }

          if (message == null || message.Attempts + 1 < 10)
            _downloadedFiles.Add(fileinfo.Name, message != null ? message.MessageID : -1);
        }
      }
      catch (Exception e)
      {
        log.AuditError(string.Format("Cannot access {0} over SFTP: {1}\n{2}", allianceDirectoryName, e.Message, e.StackTrace));
      }

      CreateMessagesFromDownloadedFiles(vendorID);

      ftpClient.Dispose();


    }

    private void CreateMessagesFromDownloadedFiles(int vendorID)
    {
      var pluginLocation = GetPluginLocation();

      var typesXsdPath = Path.Combine(pluginLocation, "XSD", "wdpTypes.xsd");

      foreach (var filename in new List<string>(_downloadedFiles.Keys))
      {
        var path = Path.Combine(ConfigurationHelper.IncomingFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), filename);
        try
        {
          var doc = XDocument.Load(path);

          if (doc.Root == null)
            continue;

          var type = MessageHelper.DetermineType(doc.Root.Name.LocalName);

          if (_downloadedFiles[filename] < 0)
            _downloadedFiles[filename] = MessageHelper.InsertMessage(type, filename, vendorID);

          MessageHelper.UpdateMessageRecieved(_downloadedFiles[filename], DateTime.Now.ToUniversalTime());

          var xsdPath = Path.Combine(pluginLocation, "XSD", string.Format("{0}.xsd", doc.Root.Name.LocalName));

          if (!File.Exists(xsdPath) || !File.Exists(typesXsdPath))
          {
            throw new FileNotFoundException(String.Format("Cannot find Xsd validation files\n{0}\n{1}", xsdPath, typesXsdPath));
          }

          var schemas = new XmlSchemaSet();

          schemas.Add(null, XElement.Load(xsdPath).CreateReader());
          schemas.Add(null, XElement.Load(typesXsdPath).CreateReader());
          schemas.Compile();

          doc.Validate(schemas, null);

          var targetPath = MessageHelper.GetMessageFolderByType(type, vendorID);

          MessageHelper.UpdateMessagePath(_downloadedFiles[filename], targetPath);
          targetPath = Path.Combine(targetPath, filename);
          if (File.Exists(targetPath))
          {
            log.AuditError(string.Format("File already exists in input folder : {0}", targetPath));
            File.Delete(targetPath);
          }

          File.Move(path, targetPath);
        }
        catch (Exception e)
        {
          if (e is XmlSchemaValidationException)
          {
            MessageHelper.UpdateMessageStatus(_downloadedFiles[filename], WehkampMessageStatus.ValidationFailed);
            log.AuditError(string.Format("XML validation failed for file {0} : {1}\n{2}", filename, e.Message, e.StackTrace));
          }
          else
          {
            MessageHelper.UpdateMessageStatus(_downloadedFiles[filename], WehkampMessageStatus.Error);
            log.AuditError(string.Format("Error processing file {0} : {1}\n{2}", filename, e.Message, e.StackTrace));
          }

          var targetPath = Path.Combine(ConfigurationHelper.FailedFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), filename);
          if (File.Exists(targetPath))
          {
            log.AuditError(string.Format("File already exists in failed files folder : {0}", targetPath));
            File.Delete(targetPath);
          }

          MessageHelper.UpdateMessagePath(_downloadedFiles[filename], Path.Combine(ConfigurationHelper.FailedFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture)));

          File.Move(path, targetPath);
        }
      }
      _downloadedFiles.Clear();
    }

    private void UploadMessageFiles(int vendorID)
    {
      var pluginLocation = GetPluginLocation();

      var typesXsdPath = Path.Combine(pluginLocation, "XSD", "wdpTypes.xsd");

      // first move all files to the outgoing directory
      var directories = MessageHelper.GetMessageFolders("To", vendorID);
      foreach (var directory in directories)
      {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
          var filename = Path.GetFileName(file);

          if (filename == null)
            continue;

          var wkm = MessageHelper.GetMessageByFilename(filename);
          if (wkm != null && (wkm.MessageType == MessageHelper.WehkampMessageType.ProductAttribute || wkm.MessageType == MessageHelper.WehkampMessageType.ShipmentNotification) &&
              wkm.LastModified.HasValue &&
              wkm.LastModified.Value.AddMinutes(20) > DateTime.Now.ToUniversalTime())
            continue;

          //Only export files with the status Success
          if (wkm == null || wkm.Status != WehkampMessageStatus.Success)
            continue;

          File.Move(Path.Combine(directory, filename), Path.Combine(ConfigurationHelper.OutgoingFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), filename));
        }
      }
      var ftpClient = SftpHelper.CreateClient(VendorSettingsHelper.GetSFTPSetting(vendorID), CommunicatorHelper.GetWehkampPrivateKeyFilename(vendorID), "D1r@ct379");

      if (ftpClient == null)
      {
        log.AuditCritical("SFTP failed to connect");
        return;
      }

      // then we upload all of them
      var remoteFileList = new ArrayList();

      if (ConfigurationHelper.ListFTPFilesCheck)
        ftpClient.ListDirectory("TO_WK", remoteFileList);

      var remoteFiles = (TElSftpFileInfo[])remoteFileList.ToArray(typeof(TElSftpFileInfo));

      foreach (var file in Directory.EnumerateFiles(Path.Combine(ConfigurationHelper.OutgoingFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture))))
      {
        var filename = Path.GetFileName(file);
        var message = MessageHelper.GetMessageByFilename(filename);

        if (message == null || (message.Status != WehkampMessageStatus.ErrorUpload && message.Status != WehkampMessageStatus.Success))
          continue;

        if (message.Status == WehkampMessageStatus.Success)
          MessageHelper.ResetMessageAttempt(message.MessageID);

        var error = false;
        MessageHelper.UpdateMessageAttempt(message.MessageID);
        if (!ConfigurationHelper.ListFTPFilesCheck || remoteFiles.All(c => c.Name != file))
        {
          try
          {
            var path = Path.Combine(ConfigurationHelper.OutgoingFilesRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture), filename);
            var doc = XDocument.Load(path);

            if (doc.Root == null)
              continue;

            if (_downloadedFiles.ContainsKey(filename))
            {
              _downloadedFiles[filename] = message.MessageID;
            }
            else
            {
              _downloadedFiles.Add(filename, message.MessageID);
            }

            MessageHelper.UpdateMessageRecieved(message.MessageID, DateTime.Now.ToUniversalTime());

            var xsdPath = Path.Combine(pluginLocation, "XSD", string.Format("{0}.xsd", doc.Root.Name.LocalName));

            if (!File.Exists(xsdPath) || !File.Exists(typesXsdPath))
            {
              throw new FileNotFoundException(String.Format("Cannot find Xsd validation files\n{0}\n{1}", xsdPath, typesXsdPath));
            }

            var schemas = new XmlSchemaSet();

            schemas.Add(null, XElement.Load(xsdPath).CreateReader());
            schemas.Add(null, XElement.Load(typesXsdPath).CreateReader());

            schemas.Compile();

            doc.Validate(schemas, null);

            ftpClient.UploadFile(path, Path.Combine("TO_WK", filename).ToUnixPath(), TSBFileTransferMode.ftmOverwrite);

            File.Move(Path.Combine(ConfigurationHelper.OutgoingFilesRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture), filename), Path.Combine(ConfigurationHelper.ArchivedRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture), filename));

            MessageHelper.UpdateMessageStatus(message.MessageID, WehkampMessageStatus.Archived);
            MessageHelper.UpdateMessagePath(message.MessageID, ConfigurationHelper.ArchivedRootFolder);
            MessageHelper.UpdateMessageSent(message.MessageID, DateTime.Now.ToUniversalTime());
          }
          catch (Exception e)
          {
            if (e is XmlSchemaValidationException)
            {
              MessageHelper.UpdateMessageStatus(_downloadedFiles[filename], WehkampMessageStatus.ValidationFailed);
              log.AuditError(string.Format("XML validation failed for file {0} : {1}\n{2}", filename, e.Message, e.StackTrace));
            }
            else
            {
              MessageHelper.UpdateMessageStatus(message.MessageID, WehkampMessageStatus.ErrorUpload);
              log.AuditError(string.Format("Cannot upload file {0}", filename), e);
            }
            MessageHelper.UpdateMessagePath(message.MessageID, ConfigurationHelper.OutgoingFilesRootFolder);
            error = true;
          }
        }
        else
        {
          MessageHelper.UpdateMessageStatus(message.MessageID, WehkampMessageStatus.ErrorUpload);
          MessageHelper.UpdateMessagePath(message.MessageID, ConfigurationHelper.OutgoingFilesRootFolder);
          log.AuditError(string.Format("Cannot upload file {0}, File already exists on remote server.", filename));
          error = true;
        }

        if (error && message.Attempts + 1 > 10)
        {
          MessageHelper.UpdateMessageStatus(message.MessageID, WehkampMessageStatus.MaxRetryExceeded);
          File.Move(Path.Combine(ConfigurationHelper.OutgoingFilesRootFolder, filename), Path.Combine(ConfigurationHelper.FailedFilesRootFolder, filename));
          MessageHelper.UpdateMessagePath(message.MessageID, ConfigurationHelper.FailedFilesRootFolder);
        }
      }
      ftpClient.Dispose();
    }

    private static string GetPluginLocation()
    {
      return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    }
  }
}
