using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using log4net;

namespace Concentrator.Objects.Ftp
{
	public class FtpManager : IEnumerable<FtpManager.RemoteFile>
	{
		public const String ErrorExtension = ".err";
		public const String CompleteExtension = ".comp";

		public Uri BaseUri
		{
			get;
			private set;
		}

		public String UserName
		{
			get;
			private set;
		}

		public String Password
		{
			get;
			private set;
		}

		public Boolean UseSSL
		{
			get;
			private set;
		}

		public Boolean UsePassive
		{
			get;
			private set;
		}

		private ILog Log
		{
			get;
			set;
		}

		public FtpManager(String address
			, String path = null
			, String userName = null
			, String password = null
			, Boolean useSSL = true
			, Boolean usePassive = true
			, ILog log = null)
		{
			BaseUri = new Uri(new Uri(address), path ?? String.Empty + "/");
			UserName = userName ?? String.Empty;
			Password = password ?? String.Empty;
			UsePassive = usePassive;
			UseSSL = useSSL;
			Log = log;
		}

		public FtpManager(String uri
			, ILog log = null
			, Boolean useSSL = false
			, Boolean usePassive = false)
			: this(new Uri(uri), log, useSSL, usePassive)
		{
		}

		public FtpManager(Uri uri
			, ILog log = null
			, Boolean useSSL = false
			, Boolean usePassive = false)
		{
			BaseUri = uri;
			UsePassive = usePassive;
			UseSSL = useSSL;
			Log = log;
		}

		private FtpWebRequest CreateRequest(Uri uri, String method = null)
		{
			var request = (FtpWebRequest)FtpWebRequest.Create(uri);

			request.Credentials = !String.IsNullOrWhiteSpace(UserName)
				? new NetworkCredential(UserName, Password)
				: request.Credentials;

			method.ThrowIfNullOrEmpty(new InvalidOperationException("Ftp method can't be null or empty"));

			request.Method = method;
			request.EnableSsl = UseSSL;
			request.UsePassive = UsePassive;
			request.KeepAlive = false;
			request.UseBinary = true;

			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

			return request;
		}

		public DateTime GetFileTimestamp(String fileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, fileName)
			};

			var request = CreateRequest(uriBuilder.Uri);

			request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

			using (FtpWebResponse response = request.GetResponse() as FtpWebResponse)
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				return response.LastModified;
			}
		}

		public IEnumerable<String> GetFilesToDownload(String remoteDirectory = null)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = remoteDirectory ?? BaseUri.LocalPath
			};

			return GetFilesToDownload(uriBuilder.Uri);
		}

		private IEnumerable<String> GetFilesToDownload(Uri uri)
		{
			var request = CreateRequest(uri, WebRequestMethods.Ftp.ListDirectory);

			Console.WriteLine("FTP LIST " + uri.ToString());

			using (var response = request.GetResponse())
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				while (true)
				{
					String line = reader.ReadLine();

					if (line == null)
					{
						yield break;
					}
					else if (line.EndsWith(ErrorExtension) || line.EndsWith(CompleteExtension))
					{
						continue;
					}

					yield return line;
				}
			}
		}

		/// <summary>
		/// Download the specified file.
		/// </summary>
		public Stream Download(String fileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, fileName)
			};

			var request = CreateRequest(uriBuilder.Uri, WebRequestMethods.Ftp.DownloadFile);

			using (var response = request.GetResponse())
			using (var responseStream = response.GetResponseStream())
			{
				var returnStream = new MemoryStream();

				responseStream.CopyTo(returnStream);

				return returnStream.Reset();
			}
		}

		public String DownloadToDisk(String downloadDir, String fileName)
		{
			var savePath = Path.Combine(downloadDir, Path.GetFileName(fileName));

			Log.Info("Downloading file: " + fileName);

			try
			{
				var uriBuilder = new UriBuilder(BaseUri)
				{
					Path = Path.Combine(BaseUri.LocalPath, fileName)
				};

				var request = CreateRequest(uriBuilder.Uri, WebRequestMethods.Ftp.DownloadFile);

				using (var resp = request.GetResponse())
				{
					using (Stream stream = resp.GetResponseStream())
					using (FileStream file = File.Create(savePath))
					{
						stream.CopyTo(file);
					}

					Log.Info("Done downloading file: " + fileName);
				}
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
			}

			return savePath;
		}

		public RemoteFile OpenFile(String fileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, fileName)
			};

			var request = CreateRequest(uriBuilder.Uri);

			request.Method = WebRequestMethods.Ftp.DownloadFile;

			using (var response = request.GetResponse())
			using (var responseStream = response.GetResponseStream())
			using (var returnStream = new MemoryStream())
			{
				responseStream.CopyTo(returnStream);

				return new RemoteFile(fileName, returnStream.Reset());
			}
		}

		#region IEnumerable<Stream> Members

		public List<String> GetFiles()
		{
			return GetFilesToDownload(BaseUri).ToList();
		}

		public IEnumerator<RemoteFile> GetEnumerator()
		{
			var sourceFiles = GetFilesToDownload(BaseUri).ToList();

			foreach (var sourceFile in sourceFiles)
			{
				if (Log != null)
					Log.DebugFormat("Try get file {0}", new Uri(BaseUri, sourceFile));

				using (var remoteFile = OpenFile(sourceFile))
				{
					yield return remoteFile;
				}
			}

			yield break;
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		public void Delete(String remoteFileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, remoteFileName)
			};

			var request = CreateRequest(uriBuilder.Uri, WebRequestMethods.Ftp.DeleteFile);

			using (request.GetResponse())
			{
			}
		}

		public void MarkAsError(String remoteFileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, remoteFileName)
			};

			var request = CreateRequest(uriBuilder.Uri);

			request.Method = WebRequestMethods.Ftp.Rename;
			request.RenameTo = Path.GetFileName(remoteFileName) + ErrorExtension;

			using (request.GetResponse())
			{
			}
		}

		public void MarkAsComplete(String remoteFileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, remoteFileName)
			};

			var request = CreateRequest(uriBuilder.Uri);

			request.Method = WebRequestMethods.Ftp.Rename;
			request.RenameTo = Path.GetFileName(remoteFileName) + CompleteExtension;

			using (request.GetResponse())
			{
			}
		}

		public void Upload(Stream data, String remoteFileName)
		{
			var uriBuilder = new UriBuilder(BaseUri)
			{
				Path = Path.Combine(BaseUri.LocalPath, remoteFileName)
			};

			var request = CreateRequest(uriBuilder.Uri, WebRequestMethods.Ftp.UploadFile);

			using (var requestStream = request.GetRequestStream())
			{
				data.CopyTo(requestStream);
			}
		}

		public Boolean DirectoryExists(String path)
		{
			var request = CreateRequest(new Uri(BaseUri, path));

			request.UseBinary = true;
			request.KeepAlive = true;
			request.Timeout = Timeout.Infinite;
			request.ServicePoint.ConnectionLimit = 6;
			request.ReadWriteTimeout = Timeout.Infinite;
			request.Method = WebRequestMethods.Ftp.ListDirectory;

			try
			{
				using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
				{
					return true;
				}
			}
			catch (WebException)
			{
				return false;
			}
		}

		public void UploadFileInDir(String fileUri, Stream data, String fileName, ILog log)
		{
			String path = string.Empty;

			foreach (var part in fileUri.Split('\\'))
			{
				if (!string.IsNullOrEmpty(part))
				{
					if (!string.IsNullOrEmpty(path))
						path += "/" + part;
					else
						path = part;

					//if (!DirectoryExists(path))
					// { 
					try
					{
						var pathRequest = new Uri(BaseUri, path);
						var request = CreateRequest(pathRequest);
						request.Method = WebRequestMethods.Ftp.MakeDirectory;
						using (FtpWebResponse ftpResp = request.GetResponse() as FtpWebResponse)
						{
						}
						request.Abort();
					}
					catch
					{
					}
					//}
				}
			}

			Upload(data, fileName);
		}

		public struct RemoteFile : IDisposable
		{
			public readonly String FileName;
			public readonly Stream Data;

			public RemoteFile(String fileName, Stream data)
			{
				FileName = fileName;
				Data = data;
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (Data != null)
					Data.Dispose();

			}

			#endregion
		}
	}
}
