using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBSimpleSftp;
using SBSSHCommon;
using SBSftpCommon;
using Concentrator.Objects;
namespace Concentrator.Plugins.Magento.Helpers
{
  public class FtpInfo
  {
    public string FtpAddress { get; set; }
    public string FtpUserName { get; set; }
    public string FtpPassword { get; set; }
    public string FtpPath { get; set; }
  }

  public class SftpHelper
  {
    static TElSftpFileAttributes defaultAttributes = new TElSftpFileAttributes();
    public static TElSftpFileAttributes DefaultAttributes { get { return defaultAttributes; } }

    static SftpHelper()
    {
      defaultAttributes = new TElSftpFileAttributes();
      defaultAttributes.UserExecute = defaultAttributes.UserWrite = defaultAttributes.UserRead = true;
      defaultAttributes.GroupExecute = defaultAttributes.GroupWrite = defaultAttributes.GroupRead = true;
      defaultAttributes.OtherExecute = defaultAttributes.OtherWrite = defaultAttributes.OtherRead = true;

      defaultAttributes.IncludedAttributes = SBSftpCommon.Unit.saPermissions;

    }
    public static TElSimpleSFTPClient GetSFTPClient(FtpInfo info)
    {

      TElSimpleSFTPClient client = new TElSimpleSFTPClient();

      client.Address = info.FtpAddress.Replace("ftp://", "");
      client.Port = 22;

      client.Username = info.FtpUserName;
      client.Password = info.FtpPassword;

      client.OnKeyValidate += new TSSHKeyValidateEvent(client_OnKeyValidate);

      try
      {
        client.Open();
        return client;
      }
      catch (Exception e)
      {
        System.Console.WriteLine("Connection failed due to exception: " + e.Message);
        System.Console.WriteLine("If you have ensured that all connection parameters are correct and you still can't connect,");
        System.Console.WriteLine("please contact EldoS support as described on http://www.eldos.com/sbb/support-tech.php");
        System.Console.WriteLine("Remember to provide details about the error that happened.");
        if (client.ServerSoftwareName.Length > 0)
        {
          System.Console.WriteLine("Server software identified itself as: " + client.ServerSoftwareName);
        }
        try
        {
          client.Close(true);
          return null;
        }
        catch
        {
        }

      }


      return client;

    }

    private static void client_OnKeyValidate(object Sender, SBSSHKeyStorage.TElSSHKey ServerKey, ref bool Validate)
    {
      Validate = true; // NEVER do this in production code. You MUST check the key validity. The sample procedure is described in SecureBlackbox FAQ on http://www.eldos.com/sbb/articles/
    }

    public static void EnsurePath(TElSimpleSFTPClient client, string path)
    {
      try
      {
        var handle = client.OpenDirectory(path);
      }
      catch
      {
        client.MakeDirectory(path, defaultAttributes);
      }
      //client.SetAttributes(path, defaultAttributes);
    }

    public static FtpInfo GetFtpTriggerIndexInfo(Objects.Models.Connectors.Connector Connector)
    {
      return new FtpInfo
      {
        FtpAddress = Connector.ConnectorSettings.GetValueByKey<string>("FtpAddress", null),
        FtpUserName = Connector.ConnectorSettings.GetValueByKey<string>("FtpUserName", null),
        FtpPassword = Connector.ConnectorSettings.GetValueByKey<string>("FtpPassword", null),
        FtpPath = Connector.ConnectorSettings.GetValueByKey<string>("TrgFilePath", null)
      };
    }

    public static FtpInfo GetFtpInfo(Objects.Models.Connectors.Connector Connector)
    {
      return new FtpInfo
      {
        FtpAddress = Connector.ConnectorSettings.GetValueByKey<string>("FtpAddress", null),
        FtpUserName = Connector.ConnectorSettings.GetValueByKey<string>("FtpUserName", null),
        FtpPassword = Connector.ConnectorSettings.GetValueByKey<string>("FtpPassword", null),
        FtpPath = Connector.ConnectorSettings.GetValueByKey<string>("FtpPath", null)
      };
    }
  }
}
