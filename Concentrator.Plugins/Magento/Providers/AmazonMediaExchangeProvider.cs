using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Concentrator.Plugins.Magento.Providers
{
  using Contracts;
  using Objects.Models.Connectors;

  public sealed class AmazonMediaExchangeProvider : IMediaExchangeProvider
  {
    private const String AccessKeySettingKey = "AwsAccessKey";
    private const String SecretKeySettingKey = "AwsSecretKey";
    private const String BucketNameSettingKey = "AwsBucketName";

    private String BucketName
    {
      get;
      set;
    }

    private AmazonS3Client Client
    {
      get;
      set;
    }

    public AmazonMediaExchangeProvider(Connector connector)
    {
      if (connector == null)
      {
        throw new ArgumentNullException("connector");
      }

      if (connector.ConnectorSettings == null)
      {
        throw new ArgumentException("connector has no connector settings");
      }

      var connectorSettings = connector.ConnectorSettings.ToDictionary(connectorSetting => connectorSetting.SettingKey, connectorSetting => connectorSetting.Value);

      var accessKey = String.Empty;

      if (!connectorSettings.TryGetValue(AccessKeySettingKey, out accessKey))
      {
        throw new ArgumentException(String.Format("Connector setting '{0}' does not exist", AccessKeySettingKey));
      }

      var secretKey = String.Empty;

      if (!connectorSettings.TryGetValue(SecretKeySettingKey, out secretKey))
      {
        throw new ArgumentException(String.Format("Connector setting '{0}' does not exist", SecretKeySettingKey));
      }

      var bucketName = String.Empty;

      if (!connectorSettings.TryGetValue(BucketNameSettingKey, out bucketName))
      {
        throw new ArgumentException(String.Format("Connector setting '{0}' does not exist", BucketNameSettingKey));
      }

      BucketName = bucketName;

      Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), RegionEndpoint.EUWest1);
    }

    public void Delete(String remoteFile)
    {
      if (remoteFile.StartsWith("/")) remoteFile = remoteFile.Substring(1);
      Client.DeleteObject(new DeleteObjectRequest
      {
        BucketName = BucketName,
        Key = remoteFile
      });
    }

    public void Dispose()
    {
      Client.Dispose();
    }

    public void Download(String localPath, String remotePath)
    {
      using (var getObjectResponse = Client.GetObject(new GetObjectRequest
      {
        BucketName = BucketName,
        Key = remotePath
      }))
      using (var fileStream = File.Open(localPath, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        getObjectResponse.ResponseStream.CopyTo(fileStream);
      }
    }

    public IEnumerable<MediaExchangeItem> List()
    {
      var listObjectResponse = Client.ListObjects(new ListObjectsRequest
      {
        BucketName = BucketName,
      });

      foreach (var listObject in listObjectResponse.S3Objects)
      {
        yield return new MediaExchangeItem
        {
          Name = listObject.Key,
          Size = listObject.Size,
          Time = listObject.LastModified
        };
      }
    }

    public void Upload(String localPath, String remotePath)
    {
      Client.PutObject(new PutObjectRequest
      {
        BucketName = BucketName,
        CannedACL = S3CannedACL.PublicRead,
        Key = remotePath,
        FilePath = localPath
      });
    }

    public void Upload(Stream mediaStream, String remotePath)
    {
      if (remotePath.StartsWith("/")) remotePath = remotePath.Substring(1);

      PutObjectRequest request = new PutObjectRequest()
      {
        BucketName = BucketName,
        CannedACL = S3CannedACL.PublicRead,
        InputStream = mediaStream,
        Key = remotePath
      };

      request.Metadata.Add("x-amz-meta-uid", "33");
      request.Metadata.Add("x-amz-meta-gid", "33");
      request.Metadata.Add("x-amz-meta-mode", "33188");
      request.Metadata.Add("x-amz-meta-mtime", (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
      request.Metadata.Add("Content-Type", "image/png");

      Client.PutObject(request);
    }
  }
}
