using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.PfaCommunicator.Objects.Helpers;
using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using System.IO;
using Concentrator.Plugins.PfaCommunicator.Objects.Repositories;
using PetaPoco;
using System;
using Concentrator.Objects;

namespace Concentrator.Plugins.PfaCommunicator
{
  public class Communicator : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "PFA Communicator"; }
    }

    protected override void Process()
    {
      using (var db = new Database(Connection, "System.Data.SqlClient"))
      {
        PfaCommunicatorRepository repo = new PfaCommunicatorRepository(db);

        var vendorsForCommunication = repo.GetVendorsWithPfaCommunication();

        foreach (var vendor in vendorsForCommunication)
        {
          string remoteFileLocation = string.Empty;
          NetworkExportUtility util = new NetworkExportUtility(log);
          try
          {
            log.Info("Checking messages for vendor " + vendor.Name);
            PFACommunicatorHelper helper = new PFACommunicatorHelper(repo, vendor.VendorID);
            var messageTypes = helper.GetMessageTypesForVendor();


            try
            {
              remoteFileLocation = util.ConnectorNetworkPath(messageTypes.RemoteDirectory, "O:", messageTypes.UsernameForRemoteDirectory, messageTypes.PasswordForRemoteDirectory);
            }
            catch (Exception e)
            {
              throw new InvalidOperationException("Could not connect to remote location");
            }

            EnsureLocalDirectoryExists(messageTypes.LocalDirectory);
            EnsureLocalDirectoryExists(messageTypes.ArchiveDirectory);

            foreach (var message in messageTypes.Messages)
            {
              var localDirectory = Path.Combine(messageTypes.LocalDirectory, message.LocalSubPath);
              var localArchiveDirectory = Path.Combine(messageTypes.ArchiveDirectory, message.LocalSubPath);

              EnsureLocalDirectoryExists(localDirectory);
              EnsureLocalDirectoryExists(localArchiveDirectory);

              if (message.Incoming)
              {
                SyncLocalMessages(message, remoteFileLocation + "\\", messageTypes.LocalDirectory, localArchiveDirectory);
              }
              else
              {
                SyncRemoteMessages(message, remoteFileLocation + "\\", messageTypes.LocalDirectory, localArchiveDirectory);
              }
            }
          }
          catch (Exception e)
          {
            log.AuditError("Something went wrong with Pfa communication for vendor " + vendor.VendorID, e);
            throw;
          }
          finally
          {
            util.DisconnectNetworkPath(remoteFileLocation);

          }
        }
      }
    }

    private void SyncLocalMessages(MessageModel message, string remoteLocation, string localLocation, string archiveLocation)
    {
      foreach (var file in Directory.GetFiles(Path.Combine(remoteLocation, message.RemoteSubPath)))
      {
        MoveFileAndArchive(file, Path.Combine(localLocation, message.LocalSubPath, Path.GetFileName(file)), Path.Combine(archiveLocation, Path.GetFileName(file)), true);
      }
    }

    private void SyncRemoteMessages(MessageModel message, string remoteLocation, string localLocation, string archiveLocation)
    {
      foreach (var file in Directory.GetFiles(Path.Combine(localLocation, message.LocalSubPath)))
      {
        MoveFileAndArchive(file, Path.Combine(remoteLocation, message.RemoteSubPath, Path.GetFileName(file)), Path.Combine(archiveLocation, Path.GetFileName(file)));
      }
    }

    private void MoveFileAndArchive(string fromPath, string toPath, string archivePath, bool remoteArchiving = false)
    {
      try
      {
        File.Copy(fromPath, archivePath, true);

        try
        {
          if (remoteArchiving)
          {
            var remoteArchivePath = Path.Combine(fromPath, "processed");
            if (!Directory.Exists(remoteArchivePath)) Directory.CreateDirectory(remoteArchivePath);

            File.Copy(fromPath, remoteArchivePath);
          }
        }
        catch (Exception e)
        {
          log.AuditError("Remote archiving failed for " + fromPath);
        }

        File.Move(fromPath, toPath);
      }
      catch (Exception e)
      {
        log.AuditError(string.Format("Failed to move or archive file {0}. Destination path is {1}", fromPath, toPath), e);
      }
    }

    private void EnsureLocalDirectoryExists(string directory)
    {
      if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
    }
  }
}
