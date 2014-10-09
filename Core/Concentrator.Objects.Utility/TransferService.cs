#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Concentrator.Objects.Utility.TransferServices;
using Concentrator.Objects.Utility.TransferServices.Interfaces;
using Concentrator.Objects.Utility.TransferServices.Models;

#endregion

namespace Concentrator.Objects.Utility
{
  public class TransferService
  {
    //TODO: Kies een logische configuratie
    public const String FileSystemImplementation = "Concentrator.Objects.Utility.TransferServices.Implementations.FileSystemTransfer";
    public const String FtpImplementation = "Concentrator.Objects.Utility.TransferServices.Implementations.FtpTransfer";
    public const String SftpImplementation = "Concentrator.Objects.Utility.TransferServices.Implementations.SftpTransfer";
    
    protected static readonly TraceSource DefaultTraceSource = new TraceSource("Concentrator TransferService");
    private TraceSource _traceSource;

    static TransferService()
    {
      Default = new TransferService();
      Partial = new TransferServicePartial();
    }

    public static TransferService Default { get; private set; }
    public static TransferServicePartial Partial { get; private set; }

    /// <summary>
    ///   Gets or sets the trace source of logging.
    /// </summary>
    public TraceSource TraceSource
    {
      get { return _traceSource ?? DefaultTraceSource; }
      set { _traceSource = value; }
    }

    public bool Process(Uri sourceResource, Uri destinationResource)
    {
      return Transfer(new TransferResourceModel
        {
          Destination = destinationResource,
          Source = sourceResource
        });
    }

    public virtual bool Process(IEnumerable<TransferResourceModel> transferResources, TraceSource traceSource, params Object[] arguments)
    {
      if (transferResources == null || !transferResources.Any())
      {
        TraceSource.TraceError("Transfer resources are empty or NULL");
        return false;
      }

      return Transfer(transferResources);
    }

    public bool Transfer(TransferResourceModel transferResource)
    {
      if (transferResource.Source == null)
      {
        TraceSource.TraceError("Source not valid for this? file");
        return false;
      }

      if (transferResource.Destination == null)
      {
        TraceSource.TraceError("Destination not valid for this? file");
        return false;
      }

      var source = TransferDownload(transferResource.Source);

      TraceSource.TraceInformation("Uploading file '{0}'", transferResource.Source.LocalPath);

      if (!TransferUpload(source, transferResource.Destination))
      {
        TraceSource.TraceError("Error uploading file {0} to {1}", transferResource.Source.LocalPath, transferResource.Destination.LocalPath);
        return false;
      }

      return true;
    }

    public virtual bool Transfer(IEnumerable<TransferResourceModel> transferResources)
    {
      foreach (var transferResource in transferResources)
      {
        Transfer(transferResource);
      }
      return true;
    }

    public Stream TransferDownload(Uri source)
    {
      var adapter = GetAdapter(source);
      if (adapter == null)
        return null;

      if (initAdapter(source, adapter))
        return adapter.Download(source.LocalPath);
      
      return null;
    }

    private Boolean initAdapter(Uri uri, ITransferAdapter adapter)
    {
      return adapter.Init(uri);
    }

    private ITransferAdapter GetAdapter(Uri uri)
    {
      var assemblyName = GetAssemblyName(uri.Scheme);
      if (string.IsNullOrWhiteSpace(assemblyName))
        return null;

      ITransferAdapter adapter;
      try
      {
        adapter = (ITransferAdapter) Activator.CreateInstance(Assembly.GetAssembly(typeof (ITransferAdapter)).GetType(assemblyName), TraceSource);
      }
      catch (Exception e)
      {
        TraceSource.TraceError("Instance {0} cannot be created: Error: {1}", assemblyName, e.Message);
        return null;
      }

      return adapter;
    }

    public bool TransferUpload(Stream file, Uri destination)
    {
      var adapter = GetAdapter(destination);
      if (adapter == null)
      {
        return false;
      }

      if (initAdapter(destination, adapter))
        return adapter.Upload(file, destination.LocalPath);
      
      return false;
    }

    private string GetAssemblyName(string scheme)
    {
      switch (scheme)
      {
        case "file":
          return FileSystemImplementation;
        case "ftp":
          return FtpImplementation;
        case "sftp":
          return SftpImplementation;
        case "gopher":
        case "http":
        case "mailto":
        case "https":
        case "news":
          TraceSource.TraceError("Scheme name {0} not supported", scheme);
          break;
        default:
          TraceSource.TraceError("Scheme name {0} not available", scheme);
          break;
      }
      return null;
    }
  }
}