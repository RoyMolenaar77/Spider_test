using Concentrator.Plugins.PfaCommunicator.Objects.Models;
using Concentrator.Plugins.PfaCommunicator.Objects.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PfaCommunicator.Objects.Helpers
{
  public class PFACommunicatorHelper
  {
    private const string BASE_LOCAL_PATH_VENDOR_SETTING_KEY = "BaseLocalMessagePath";
    private const string BASE_REMOTE_PATH_VENDOR_SETTING_KEY = "BaseRemoteMessagePath";
    private const string ARCHIVE_DIRECTORY_SETTING_KEY = "ArchiveDirectoryPath";
    private const string USERNAME_REMOTE_PATH_ACCESS_SETTING_KEY = "RemotePathUsername";
    private const string PASSWORD_REMOTE_PATH_ACCESS_SETTING_KEY = "RemotePathPassword";

    private int _vendorID;
    private PfaCommunicatorRepository _communicatorRepository;

    public PFACommunicatorHelper(PfaCommunicatorRepository repository, int vendorID)
    {
      _communicatorRepository = repository;
      _vendorID = vendorID;
    }

    public VendorMessageModel GetMessageTypesForVendor()
    {
      return new VendorMessageModel()
            {
              LocalDirectory = GetLocalBaseMessagePath(),
              RemoteDirectory = GetRemoteBaseMessagePath(),
              ArchiveDirectory = GetArchiveDirectory(),
              UsernameForRemoteDirectory = GetUsernameForRemoteAccess(),
              PasswordForRemoteDirectory = GetPasswordForRemoteAccess(),
              Messages = _communicatorRepository.GetMessageTypesForVendor(_vendorID)
            };
    }
   

    public string GetLocalMessagePath(MessageTypes type)
    {
      var model = _communicatorRepository.GetMessageForVendor(type, _vendorID);

      model.ThrowIfNull("Message type not found for vendor");

      return Path.Combine(GetLocalBaseMessagePath(), model.LocalSubPath); 
    }

    private string GetLocalBaseMessagePath()
    {
      var value = _communicatorRepository.GetVendorSetting(_vendorID, BASE_LOCAL_PATH_VENDOR_SETTING_KEY);

      if (string.IsNullOrEmpty(value))
        throw new ArgumentException("BaseLocalMessagePath setting not found in Vendor settings for vendor " + _vendorID);

      return value;
    }

    private string GetArchiveDirectory()
    {
      var value = _communicatorRepository.GetVendorSetting(_vendorID, ARCHIVE_DIRECTORY_SETTING_KEY);

      if (string.IsNullOrEmpty(value))
        throw new ArgumentException("ArchiveDirectoryPath setting not found in Vendor settings for vendor " + _vendorID);

      return value;
    }

    private string GetRemoteBaseMessagePath()
    {
      var value = _communicatorRepository.GetVendorSetting(_vendorID, BASE_REMOTE_PATH_VENDOR_SETTING_KEY);

      if (string.IsNullOrEmpty(value))
        throw new ArgumentException("BaseRemoteMessagePath setting not found in Vendor settings for vendor " + _vendorID);

      return value;
    }

    private string GetUsernameForRemoteAccess()
    {
      var value = _communicatorRepository.GetVendorSetting(_vendorID, USERNAME_REMOTE_PATH_ACCESS_SETTING_KEY);

      if (string.IsNullOrEmpty(value))
        throw new ArgumentException("DatcolLocationUsername setting not found in Vendor settings for vendor " + _vendorID);

      return value;
    }

    private string GetPasswordForRemoteAccess()
    {
      var value = _communicatorRepository.GetVendorSetting(_vendorID, PASSWORD_REMOTE_PATH_ACCESS_SETTING_KEY);

      if (string.IsNullOrEmpty(value))
        throw new ArgumentException("DatcolLocationPassword setting not found in Vendor settings for vendor " + _vendorID);

      return value;
    }
  }
}
