using Concentrator.Objects.Utility.FingerPrinting;
using Concentrator.Objects.Utility.TransferServices.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace Concentrator.Objects.Utility.TransferServices
{
  public class TransferServicePartial : TransferService
  {
    public const String FingerprintArchiveFileArgument = "FingerprintArchiveFile";

    protected string ArchiveFile = string.Empty;

    public override bool Process(IEnumerable<TransferResourceModel> transferResources, TraceSource traceSource, params Object[] arguments)
    {
      TraceSource = traceSource;

      ArchiveFile = GetArgumentValue(arguments, FingerprintArchiveFileArgument);

      if (ArchiveFile.IsNullOrWhiteSpace())
      {        
        TraceSource.TraceError("Parameter FingerprintArchiveFile is not provided within the arguments. This setting is mandetory for the TransferServicePartial");
        return false;
      }

      return Transfer(transferResources);      
    }

    public override bool Transfer(IEnumerable<TransferResourceModel> transferResources)
    {
      var archiveLines = new List<string>();
      var stopwatch = new Stopwatch();

      if (File.Exists(ArchiveFile))
      {
        archiveLines = File.ReadAllLines(ArchiveFile).ToList();        
      } 
      else 
      {
        TraceSource.TraceInformation("New Fingerprint archive file will be created @{0}",ArchiveFile);
      }      

      foreach (var transferResource in transferResources)
      {
        TraceSource.TraceInformation("Processing file '{0}'", transferResource.Source);

        if (transferResource.Source.Scheme != Uri.UriSchemeFile)
        {
          TraceSource.TraceWarning("Scheme {0} not supported for partial tranfer", transferResource.Source.Scheme);
          continue;
        }

        var sequence = HttpUtility.ParseQueryString(transferResource.Source.Query).Get("sequence");
        if (sequence.IsNullOrWhiteSpace())
        {
          TraceSource.TraceWarning("Sequence not provided within query. transfer will be skipped. Resource local {0}. Resource remote {1}", transferResource.Source.LocalPath, transferResource.Destination.LocalPath);
          continue;
        }

        var productId = HttpUtility.ParseQueryString(transferResource.Source.Query).Get("productId");
        if (productId.IsNullOrWhiteSpace())
        {
          TraceSource.TraceWarning("productId not provided within query. transfer will be skipped. Resource local {0}. Resource remote {1}", transferResource.Source.LocalPath, transferResource.Destination.LocalPath);
          continue;
        }

        if (File.Exists(transferResource.Source.LocalPath))
        {
          stopwatch.Restart();

          FingerPrintModel fingerprint = FingerPrintHelper.ExtractFingerPrintInfo(transferResource.Source.LocalPath, productId, sequence);

          TraceSource.TraceInformation("Took '{0}' to create FingerPrint.", stopwatch.ElapsedMilliseconds);
          
          if (fingerprint != null)
          {
            if (!archiveLines.Contains(fingerprint.ToString()))
            {
              if (Transfer(transferResource))              
                archiveLines.Add(fingerprint.ToString());             
            }
          }
          else
          {
            TraceSource.TraceError("Cannot setup fingerprint for file: {0}", transferResource.Source.LocalPath);
            return false;
          }
        }
        else
          TraceSource.TraceWarning("File cannot be found, the transfer will be skipped. Resource local {0}. Resource remote {1}", transferResource.Source.LocalPath, transferResource.Destination.LocalPath);
      }

      try
      {
        File.WriteAllLines(ArchiveFile, archiveLines, Encoding.ASCII);
      }
      catch (Exception e)
      {
        TraceSource.TraceError("Error writing to fingerprint archive file: {0}. Error:", ArchiveFile, e.Message);
        return false;
      }
          
      return true;
    }

    private string GetArgumentValue(IEnumerable<object> arguments, string propertyName)
    {
      if (arguments == null)
        return null;
      foreach (var arg in arguments)
      {        
        IList<PropertyInfo> properties;

        try
        {          
          properties = new List<PropertyInfo>(arg.GetType().GetProperties());
        }
        catch
        {
          return null;
        }

        var property = properties.FirstOrDefault(p => p.Name == propertyName);
        if (property == null)
          return null;

        return (string)property.GetValue(arg, null);       
      }
      return null;      
    }
  }
}
