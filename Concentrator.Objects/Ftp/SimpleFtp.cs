using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Concentrator.Objects.Ftp
{
  public class SimpleFtp
  {

    private string _Username;
    private string _Password;
    private string _Url;
    private AuditLog4Net.Adapter.IAuditLogAdapter _log;
    private bool? _isPassive;


    /// <summary>
    /// Simple Ftp client to download and upload files from/to an Ftp server
    /// </summary>
    /// <param name="Url"></param>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public SimpleFtp(string Url, string Username, string Password, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      _log = log;
      _Url = Url;
      _Username = Username;
      _Password = Password;
    }

    /// <summary>
    /// Simple Ftp client to download and upload files from/to an Ftp server
    /// </summary>
    /// <param name="Url"></param>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public SimpleFtp(string Url, string Username, string Password, AuditLog4Net.Adapter.IAuditLogAdapter log, bool Passive)
    {
      _log = log;
      _Url = Url;
      _Username = Username;
      _Password = Password;
      _isPassive = Passive;
    }

    /// <summary>
    /// Returnes the stream of file on the ftp server, can be used to download file
    /// </summary>
    /// <param name="Path"></param>
    /// <param name="Filename"></param>
    /// <returns></returns>
    public Stream GetFtpFileStream(string Path, string Filename)
    {
      // Get the object used to communicate with the server.
      //TODO: Clean up duplicate code
      FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(new Uri(new Uri(_Url), Path), Filename));

      request.KeepAlive = false;
      request.Method = WebRequestMethods.Ftp.DownloadFile;
      if (_isPassive.HasValue) request.UsePassive = _isPassive.Value;

      request.Credentials = new NetworkCredential(_Username, _Password);

      FtpWebResponse response = (FtpWebResponse)request.GetResponse();
      
      return response.GetResponseStream();
    }

    /// <summary>
    /// Upload a file to the specfied Ftp server
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public bool UploadFile(string PathToFile, string PathOnFtp)
    {
      try
      {
        string Filename = Path.GetFileName(PathToFile);
        // Get the object used to communicate with the server.
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(new Uri(new Uri(_Url), PathOnFtp), Filename));
        request.Method = WebRequestMethods.Ftp.UploadFile;
        if (_isPassive.HasValue) request.UsePassive = _isPassive.Value;

        request.Credentials = new NetworkCredential(_Username, _Password);

        // Copy the contents of the file to the request stream.
        byte[] fileContents = File.ReadAllBytes(PathToFile);

        request.ContentLength = fileContents.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
          requestStream.Write(fileContents, 0, fileContents.Length);
          requestStream.Close();
        }
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
          _log.InfoFormat("Upload File Complete, status {0}", response.StatusDescription);
          response.Close();
        }
        return true;
      }
      catch (Exception ex)
      {
        _log.AuditFatal("Upload File Failed, status {0}", ex.Message);
        return false;
      }
    }

    /// <summary>
    /// Upload a file to the specfied Ftp server
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public bool UploadFile(Stream stream, string Filename, string PathOnFtp)
    {
      try
      {
        // Get the object used to communicate with the server.
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(new Uri(new Uri(_Url), PathOnFtp), Filename));
        request.Method = WebRequestMethods.Ftp.UploadFile;
        if (_isPassive.HasValue) request.UsePassive = _isPassive.Value;

        request.Credentials = new NetworkCredential(_Username, _Password);

        // Copy the contents of the file to the request stream.
        //TODO: Read directly from input stream and write
        byte[] fileContents;
        using (StreamReader sourceStream = new StreamReader(stream))
        {
          fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
        }
        request.ContentLength = fileContents.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
          requestStream.Write(fileContents, 0, fileContents.Length);
          requestStream.Close();
        }
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
          _log.InfoFormat("Upload File Complete, status {0}", response.StatusDescription);
          response.Close();
        }
        return true;
      }
      catch (Exception ex)
      {
        _log.AuditFatal("Upload File Failed, status {0}", ex.Message);
        return false;
      }
    }

    /// <summary>
    /// Upload a file to the specfied Ftp server
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public bool UploadFile(byte[] fileInBytes, string Filename, string PathOnFtp)
    {
      try
      {
        var fileContents = fileInBytes;

        // Get the object used to communicate with the server.
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(new Uri(new Uri(_Url), PathOnFtp), Filename));
        request.Method = WebRequestMethods.Ftp.UploadFile;
        if (_isPassive.HasValue) request.UsePassive = _isPassive.Value;

        request.Credentials = new NetworkCredential(_Username, _Password);

        request.ContentLength = fileContents.Length;

        using (Stream requestStream = request.GetRequestStream())
        {
          requestStream.Write(fileContents, 0, fileContents.Length);
          requestStream.Close();
        }
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
          _log.InfoFormat("Upload File Complete, status {0}", response.StatusDescription);
          response.Close();
        }
        return true;
      }
      catch (Exception ex)
      {
        _log.AuditFatal("Upload File Failed, status {0}", ex.Message);
        return false;
      }
    }

    /// <summary>
    /// Returns a list containing all the filenames on the Ftp server
    /// </summary>
    /// <returns></returns>
    public List<clsDirDetails> GetList(string PathOnFtp)
    {
      List<clsDirDetails> files = new List<clsDirDetails>();

      // Get the object used to communicate with the server.
      FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(new Uri(_Url), PathOnFtp));
      request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
      if (_isPassive.HasValue) request.UsePassive = _isPassive.Value;

      // This example assumes the FTP site uses anonymous logon.
      request.Credentials = new NetworkCredential(_Username, _Password);

      FtpWebResponse response = (FtpWebResponse)request.GetResponse();

      using (Stream responseStream = response.GetResponseStream())
      {
        using (StreamReader reader = new StreamReader(responseStream))
        {
          while (!reader.EndOfStream)
          {

            var file = new clsDirDetails(_log);
            //once .Unparsed is set, clsDirDetails internally calcs 
            //and assigns DirMemberType, LastModified and PathOrFilename
            //These assignments are made just as soon as the .Unparsed
            //property is set, BEFORE details are added to the List
            file.Unparsed = reader.ReadLine();
            files.Add(file);

          }
          reader.Close();
        }
        Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription);

      }
      return files;
    }
  }

  public class clsDirDetails
  {

    //a class to hold, in a convenient way, the details that are returned
    //by WebRequestMethods.Ftp.ListDirectoryDetails

    public clsDirDetails(AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      _log = log;
    }

    public enum DirectoryDetail { IsFileInDir, IsSubdirInDir };

    #region clsDirDetails_PrivateMembers

    private DirectoryDetail dirMemberType; //is it a file or subdirectory?
    private string pathOrFilename; //path of subdir or filename if it's a file
    private string lastModified; //last time file got modified (applies to files only)
    private string unparsed; //the unparsed line that contains details
    private string ownerPermissions; //usually this will be rwx (read/write/execute)
    private bool ownerCanRead; //owner CAN or CANNOT read the specified dir/file
    private bool ownerCanWrite; //same as above, except WRITE rather than READ
    private bool ownerCanExecute; //same as above, except EXECUTE rather than WRITE
    private AuditLog4Net.Adapter.IAuditLogAdapter _log;

    #endregion

    #region clsDirDetails_Properties

    //is it a file or a subdirectory?
    public DirectoryDetail DirMemberType
    {
      get { return dirMemberType; }
      set { dirMemberType = value; }
    }

    //owner permissions
    public string OwnerPermissions
    {
      get { return ownerPermissions; }
      set { ownerPermissions = value; }
    }

    //owner can read?
    public bool OwnerCanRead
    {
      get { return ownerCanRead; }
      set { ownerCanRead = value; }
    }

    //owner can write?
    public bool OwnerCanWrite
    {
      get { return ownerCanWrite; }
      set { ownerCanWrite = value; }
    }

    //owner can execute?
    public bool OwnerCanExecute
    {
      get { return OwnerCanExecute; }
      set { ownerCanExecute = value; }
    }

    //the full path
    public string PathOrFilename
    {
      get { return pathOrFilename; }
      set { pathOrFilename = value; }
    }

    //for files only...
    public string LastModified
    {
      get { return lastModified; }
      set { lastModified = value; }
    }

    //the unparsed line that contains details
    public string Unparsed
    {
      get { return unparsed; }
      set
      {
        unparsed = value;

        LastModified = getDateTimeString(unparsed);

        //also parse out the subdir path or filename                
        PathOrFilename = getPathOrFilename(unparsed);

        //assign DirMemberType
        DirMemberType = getDirectoryDetail(unparsed);

        //assign OwnerPermissions
        ownerPermissions = unparsed.Substring(1, 3);
        if (ownerPermissions.Contains("r"))
        {
          ownerCanRead = true;
        }
        else { ownerCanRead = false; }
        if (ownerPermissions.Contains("w"))
        {
          ownerCanWrite = true;
        }
        else { ownerCanWrite = false; }
        if (ownerPermissions.Contains("x"))
        {
          ownerCanExecute = true;
        }
        else { ownerCanExecute = false; }

        //next right-brace ends set accessor of Unparsed property
      }
      //next right-brace ends Property Unparsed
    }

    #endregion

    #region clsDirDetails_Methods

    clsDirDetails.DirectoryDetail getDirectoryDetail(string unparsedInfo)
    {
      if (unparsed.Substring(0, 1) == "d")
      {
        return clsDirDetails.DirectoryDetail.IsSubdirInDir;
      }
      else
      {
        return clsDirDetails.DirectoryDetail.IsFileInDir;
      }
    }

    #region clsDirDetails_StringMethods

    string getPathOrFilename(string unparsedInfo)
    {
      int j = unparsedInfo.LastIndexOf(' ');
      return unparsedInfo.Substring(j + 1, unparsedInfo.Length - j - 1);
    }

    string getDateTimeString(string unparsedInfo)
    {
      var error = "";
      string result = string.Empty;
      int i = getIndexOfDateBeginning(unparsedInfo);
      if (i < 0)
      {
        error = "Error in clsDirDetails: method " +
            "getDateTimeString()'s sub-method getIndexOfDateBeginning() " +
            "returned a value of -1.";
      }
      result = unparsedInfo.Substring(i, unparsedInfo.Length - (i + 1));
      int j = result.LastIndexOf(" ");
      result = result.Substring(0, j);
      //if, for whatever reason, we've failed to parse out a 
      //valid DateTime, error-log it
      if (!objectIsDate(result))
      {
        error += ", " + ("Error in getDateTimeString() in clsFTPclient.  The " +
            "parsed result does not appear to be a valid DateTime.");
      }

      _log.AuditError(error);

      return result;
    }

    #endregion

    #region clsDirDetails_BooleanMethods

    bool objectIsDate(Object obj)
    {
      string strDate = obj.ToString();
      try
      {
        DateTime dt = DateTime.Parse(strDate);
        if (dt != DateTime.MinValue && dt != DateTime.MaxValue)
          return true;
        return false;
      }
      catch
      {
        return false;
      }
    }

    #endregion

    #region clsDirDetails_IntegerMethods

    int getIndexOfFirstAlphabeticCharacter(string source)
    {
      int i = -1;
      foreach (char c in source)
      {
        i++;
        if (Char.IsLetter(c)) { return i; }
      }
      return i;
    }

    int getIndexOfDateBeginning(string unparsedInfo)
    {
      int i = -1;

      i = unparsedInfo.IndexOf("Jan");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Feb");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Mar");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Apr");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("May");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Jun");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Jul");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Aug");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Sep");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Oct");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Nov");
      if (i > -1) { return i; }

      i = unparsedInfo.IndexOf("Dec");
      if (i > -1) { return i; }

      return i;
    }

    #endregion

    #endregion

    //next right-brace ends clsDirDetails
  }
}
