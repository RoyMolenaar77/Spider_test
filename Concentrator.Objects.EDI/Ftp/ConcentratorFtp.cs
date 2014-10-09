using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Configuration;

namespace Concentrator.Objects.EDI.Ftp
{
  public static class ConcentratorFtp
  {
    #region Public Methods

    /// <summary>
    /// Add new user to the ftp server configuration file
    /// </summary>
    /// <param name="i">instrument that is assigned to the new user</param>
    public static void AddUserToConfig(string userName, string password)
    {
      XmlDocument xml = new XmlDocument();

      try
      {
        string configFile = ConfigurationManager.AppSettings["ConcentratorFtpServerConfig"].ToString();
        if (File.Exists(configFile))
        {
          xml.Load(configFile);

          XmlElement usersRoot = (XmlElement)xml.GetElementsByTagName("Users").Item(0);
          XmlElement userRoot = xml.CreateElement("User");
          userRoot.SetAttribute("Name", userName);
          usersRoot.AppendChild(userRoot);

          userRoot.AppendChild(MakeOptionSub("Pass", GeneratePass(password), xml));
          userRoot.AppendChild(MakeOptionSub("Group", ConfigurationManager.AppSettings["ConcentratorFtpGroup"].ToString(), xml));
          userRoot.AppendChild(MakeOptionSub("Bypass server userlimit", "0", xml));
          userRoot.AppendChild(MakeOptionSub("User Limit", "0", xml));
          userRoot.AppendChild(MakeOptionSub("IP Limit", "0", xml));
          userRoot.AppendChild(MakeOptionSub("Enabled", "1", xml));
          userRoot.AppendChild(MakeOptionSub("Comments", "Xtract user account", xml));
          userRoot.AppendChild(MakeOptionSub("ForceSsl", "0", xml));

          XmlElement ipfilterRoot = xml.CreateElement("IpFilter");
          userRoot.AppendChild(ipfilterRoot);
          ipfilterRoot.AppendChild(MakeElement("Disallowed", xml));
          ipfilterRoot.AppendChild(MakeElement("Allowed", xml));

          XmlElement permissionsRoot = xml.CreateElement("Permissions");
          userRoot.AppendChild(permissionsRoot);
          XmlElement permissionRoot = xml.CreateElement("Permission");
          string userPath = Path.Combine(ConfigurationManager.AppSettings["ConcentratorFtpUserDir"].ToString(), userName);

          if (!Directory.Exists(userPath))
            Directory.CreateDirectory(userPath);
          permissionRoot.SetAttribute("Dir", userPath);
          permissionsRoot.AppendChild(permissionRoot);

          userRoot.AppendChild(MakeOptionSub("FileRead", "1", xml));

          permissionRoot.AppendChild(MakeOptionSub("FileRead", "1", xml));
          permissionRoot.AppendChild(MakeOptionSub("FileWrite", "0", xml));
          permissionRoot.AppendChild(MakeOptionSub("FileDelete", "0", xml));
          permissionRoot.AppendChild(MakeOptionSub("FileAppend", "0", xml));
          permissionRoot.AppendChild(MakeOptionSub("DirCreate", "0", xml));
          permissionRoot.AppendChild(MakeOptionSub("DirDelete", "0", xml));
          permissionRoot.AppendChild(MakeOptionSub("DirList", "1", xml));
          permissionRoot.AppendChild(MakeOptionSub("DirSubdirs", "1", xml));
          permissionRoot.AppendChild(MakeOptionSub("IsHome", "1", xml));
          permissionRoot.AppendChild(MakeOptionSub("AutoCreate", "1", xml));

          XmlElement speedlimitRoot = xml.CreateElement("SpeedLimits");
          speedlimitRoot.SetAttribute("DlType", "0");
          speedlimitRoot.SetAttribute("DlLimit", "0");
          speedlimitRoot.SetAttribute("ServerDlLimitBypass", "0");
          speedlimitRoot.SetAttribute("UlType", "0");
          speedlimitRoot.SetAttribute("UlLimit", "0");
          speedlimitRoot.SetAttribute("ServerUlLimitBypass", "0");
          userRoot.AppendChild(speedlimitRoot);

          speedlimitRoot.AppendChild(MakeElement("Download", xml));
          speedlimitRoot.AppendChild(MakeElement("Upload", xml));

          // save Xml Document
          xml.Save(configFile);
          ReloadConfig();
        }
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }



    public static bool HomeDirCreated(string userName)
    {
      XmlDocument xml = new XmlDocument();

      try
      {
        string configFile = ConfigurationManager.AppSettings["ConcentratorFtpServerConfig"].ToString();
        if (File.Exists(configFile))
        {
          xml.Load(configFile);
          XmlElement usersRoot = (XmlElement)xml.GetElementsByTagName("Users").Item(0);

          int CountItems = (from XmlElement el in usersRoot.ChildNodes
                            where el.Attributes[0].Value == userName
                            select el).Count();

          if (CountItems > 0)
            return true;
        }
        return false;
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    private static XmlElement MakeOptionSub(string elementAttName, string elementValue, XmlDocument xmlDoc)
    {
      XmlElement XmlElement = xmlDoc.CreateElement("Option");
      XmlElement.SetAttribute("Name", elementAttName);
      if (!string.IsNullOrEmpty(elementValue))
        XmlElement.InnerText = elementValue;
      return XmlElement;
    }

    private static XmlElement MakeElement(string elementName, XmlDocument xmlDoc)
    {
      XmlElement XmlElement = xmlDoc.CreateElement(elementName);
      return XmlElement;
    }


    #region Example


    //    <User Name="MediaShare">
    //        <Option Name="Pass">127d948c9637ede3abd0331a19b70572</Option>
    //        <Option Name="Group" />
    //        <Option Name="Bypass server userlimit">0</Option>
    //        <Option Name="User Limit">0</Option>
    //        <Option Name="IP Limit">0</Option>
    //        <Option Name="Enabled">1</Option>
    //        <Option Name="Comments" />
    //        <Option Name="ForceSsl">0</Option>
    //        <IpFilter>
    //            <Disallowed />
    //            <Allowed />
    //        </IpFilter>
    //        <Permissions>
    //            <Permission Dir="C:\MyComWEB\MyComNL\Images">
    //                <Option Name="FileRead">1</Option>
    //                <Option Name="FileWrite">0</Option>
    //                <Option Name="FileDelete">0</Option>
    //                <Option Name="FileAppend">0</Option>
    //                <Option Name="DirCreate">0</Option>
    //                <Option Name="DirDelete">0</Option>
    //                <Option Name="DirList">1</Option>
    //                <Option Name="DirSubdirs">1</Option>
    //                <Option Name="IsHome">1</Option>
    //                <Option Name="AutoCreate">0</Option>
    //            </Permission>
    //        </Permissions>
    //        <SpeedLimits DlType="0" DlLimit="10" ServerDlLimitBypass="0" UlType="0" UlLimit="10" ServerUlLimitBypass="0">
    //            <Download />
    //            <Upload />
    //        </SpeedLimits>
    //    </User>
    //</Users>

    #endregion

    #endregion

    #region Private Methods

    private static string GeneratePass(string password)
    {
      System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
      byte[] bs = System.Text.Encoding.UTF8.GetBytes(password);
      bs = x.ComputeHash(bs);
      System.Text.StringBuilder s = new System.Text.StringBuilder();
      foreach (byte b in bs)
      {
        s.Append(b.ToString("x2").ToLower());
      }

      return s.ToString();
    }

    private static void Start()
    {
      ProcessStartInfo startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["ConcentratorFtpServerExecutable"].ToString(), "-compat -start");
      Process.Start(startInfo);
      Thread.Sleep(300);
    }

    /// <summary>
    /// Stop the remote server
    /// </summary>
    private static void Stop()
    {
      ProcessStartInfo startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["ConcentratorFtpServerExecutable"].ToString(), "-compat -stop");
      Process.Start(startInfo);
      Thread.Sleep(300);
    }

    /// <summary>
    /// Start the server with new configuration
    /// </summary>
    public static void ReloadConfig()
    {
      ConcentratorFtp.Stop();
      ConcentratorFtp.Start();
      ProcessStartInfo startInfo = new ProcessStartInfo(ConfigurationManager.AppSettings["ConcentratorFtpServerExecutable"].ToString(), "/reload-config");
      Process.Start(startInfo);
      Thread.Sleep(1000);
    }
    #endregion
  }
}
