using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SBSimpleSftp;
using SBSftpCommon;
using SBSSHCommon;
using SBSSHKeyStorage;

using Concentrator.Objects;

namespace Concentrator.Objects.SFTP
{
  public static class SftpHelper
  {
    private static readonly TElSSHMemoryKeyStorage KeyStorage = new TElSSHMemoryKeyStorage();

    public static TElSftpFileAttributes DefaultAttributes
    {
      get;
      private set;
    }

    static SftpHelper()
    {
      DefaultAttributes = new TElSftpFileAttributes();
      DefaultAttributes.UserExecute = DefaultAttributes.UserWrite = DefaultAttributes.UserRead = true;
      DefaultAttributes.GroupExecute = DefaultAttributes.GroupWrite = DefaultAttributes.GroupRead = true;
      DefaultAttributes.OtherExecute = DefaultAttributes.OtherWrite = DefaultAttributes.OtherRead = true;
      DefaultAttributes.IncludedAttributes = SBSftpCommon.Unit.saPermissions;
    }

    public static TElSimpleSFTPClient CreateClient(String uri)
    {
      return CreateClient(new Uri(uri));
    }

    public static TElSimpleSFTPClient CreateClient(String uri, string privateKeyFile, string privatekeyPassword)
    {
      KeyStorage.Clear();
      var key = new TElSSHKey();
      var keyLoaded = key.LoadPrivateKey(privateKeyFile, privatekeyPassword);

      if (keyLoaded == 0)
      {
        KeyStorage.Add(key);
      }
      else
      {
        throw new Exception(string.Format("Error loading privatekey {0}. Error: {1}", privateKeyFile, keyLoaded));
      }

      return CreateClient(new Uri(uri), true);
    }


    public static TElSimpleSFTPClient CreateClient(String uri, string privateKeyFile, string privatekeyPassword, string userName)
    {
      KeyStorage.Clear();
      var key = new TElSSHKey();
      var keyLoaded = key.LoadPrivateKey(privateKeyFile, privatekeyPassword);

      if (keyLoaded == 0)
      {
        KeyStorage.Add(key);
      }
      else
      {
        throw new Exception(string.Format("Error loading privatekey {0}. Error: {1}", privateKeyFile, keyLoaded));
      }

      var uriBuilder = new UriBuilder(uri);

      if(!userName.IsNullOrWhiteSpace())
        uriBuilder.UserName = userName;

      return CreateClient(uriBuilder.Uri, true);
    }


    public static TElSimpleSFTPClient CreateClient(Uri uri, bool useKeyAuthentication = false)
    {
      var builder = new UriBuilder(uri);

      if (!builder.Scheme.Equals("sftp", StringComparison.InvariantCultureIgnoreCase))
      {
        throw new ArgumentException(String.Format("Expecting Secure FTP Scheme, but instead got '{0}'.", uri.Scheme));
      }

      var client = new TElSimpleSFTPClient
      {
        Address = builder.Host,
        Port = builder.Port != -1
          ? builder.Port
          : 22,
        Username = uri.UserInfo,
        Password = builder.Password
      };

      client.OnKeyValidate += OnKeyValidate;

      if (useKeyAuthentication)
      {
        client.KeyStorage = KeyStorage;
        client.AuthenticationTypes = SBSSHConstants.__Global.SSH_AUTH_TYPE_PUBLICKEY;
      }


      try
      {
        client.Open();

        return client;
      }
      catch (Exception exception)
      {
        Console.WriteLine("Connection failed due to exception: " + exception.ToString());
        Console.WriteLine("If you have ensured that all connection parameters are correct and you still can't connect,");
        Console.WriteLine("please contact EldoS support as described on http://www.eldos.com/sbb/support-tech.php");
        Console.WriteLine("Remember to provide details about the error that happened.");

        if (client.ServerSoftwareName.Length > 0)
        {
          Console.WriteLine("Server software identified itself as: " + client.ServerSoftwareName);
        }

        try
        {
          client.Dispose();
          client = null;
        }
        catch
        {
        }
      }

      return client;
    }

    private static void OnKeyValidate(Object sender, TElSSHKey serverKey, ref bool validate)
    {
      validate = true; // NEVER do this in production code. You MUST check the key validity. The sample procedure is described in SecureBlackbox FAQ on http://www.eldos.com/sbb/articles/
    }

    public static void EnsurePath(TElSimpleSFTPClient client, string path)
    {
      try
      {
        var handle = client.OpenDirectory(path);
      }
      catch
      {
        client.MakeDirectory(path, DefaultAttributes);
      }

      client.SetAttributes(path, DefaultAttributes);
    }
  }
}
