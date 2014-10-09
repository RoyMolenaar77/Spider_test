using System;

namespace Concentrator.Objects.Ftp
{
  public static class FtpClientFactory
  {
    public static IFtpClient Create(Uri baseUri, String userName, String password)
    {
      return new FtpClient(baseUri, userName, password);
    }
  }
}