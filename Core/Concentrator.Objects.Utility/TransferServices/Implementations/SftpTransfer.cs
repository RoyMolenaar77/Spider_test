using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Concentrator.Objects.Sftp;
using Concentrator.Objects.SFTP;
using Concentrator.Objects.Utility.TransferServices.Interfaces;
using SBSimpleSftp;
using System.Diagnostics;

namespace Concentrator.Objects.Utility.TransferServices.Implementations
{
  public class SftpTransfer : ITransferAdapter
  {
    public const String PrivateKeyFilePathParameter = "privateKeyFile";
    protected TElSimpleSFTPClient sftpClient;

    private TraceSource TraceSource { set; get; }
    public SftpTransfer(TraceSource traceSource)
    {
      TraceSource = traceSource;
    }

    public bool Init(Uri uri)
    {
      using (new SBLicenseManager.TElSBLicenseManager
      {
        LicenseKey = "8F1317CD48AC4E68BABA5E339D8B365414D7ADA0289CA037E9074D29AD95FF3EC5D796BEFF0FBADB3BD82F48644C9EB810D9B5A305E0D2A1885C874D8BF974B9608CE918113FBE2AA5EEF8264C93B25ABEA98715DB4AD265F47CE02FC9952D69F2C3530B6ABAAA4C43B45E7EF6A8A0646DA038E34FBFB629C2BF0E83C6B348726E622EBD52CA05CF74C68F1279849CCD0C13EA673916BA42684015D658B8E7626F15BD826A4340EDB36CE55791A051FDBCF9FA1456C3B5008AD9990A0185C0EA3B19F9938CB7DA1FE82736ED4C7A566D4BFD53411E8380F4B020CB50E762520EFAE190836FD253B00DB18D4A485C7DC918AA4DCEC856331DD231CC8DC9C741C3"
      })
      {
        var host = string.Format(@"sftp://{0}", uri.Host);
        var privateKeyFile = HttpUtility.ParseQueryString(uri.Query).Get(PrivateKeyFilePathParameter);

        try
        {
          sftpClient = !string.IsNullOrWhiteSpace(privateKeyFile)
            ? SftpHelper.CreateClient(host, privateKeyFile, String.Empty, uri.UserInfo)
            : SftpHelper.CreateClient(host);
        }
        catch (Exception e)
        {
          TraceSource.TraceError("Error setting up sftpClient: Host {0}: Error {1}", host, e.Message);
          return false;
        }
        return sftpClient != null;
      }
    }

    public bool Upload(Stream file, string fileName)
    {
      try
      {
        sftpClient.UploadStream(file, fileName, SBUtils.TSBFileTransferMode.ftmOverwrite);
        return true;
      }
      catch (Exception e)
      {
        TraceSource.TraceError("File cannot be uploaded to sftp server, the transfer will be skipped. local name {0}. Error: {1}", fileName, e.Message);
        return false;
      }
    }

    public Stream Download(string fileName)
    {
      TraceSource.TraceError("Download function not implemented for sftp");
      return null;
    }
  }
}
