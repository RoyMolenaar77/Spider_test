using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.PfaCommunicator.Objects.Services;
using Concentrator.Plugins.Wehkamp.Helpers;
using System;
using System.IO;

namespace Concentrator.Plugins.Wehkamp
{
  public class StockPhotoImport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Stock Photo Import"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      var messages = MessageHelper.GetMessagesByStatusAndType(Enums.WehkampMessageStatus.Created, MessageHelper.WehkampMessageType.StockPhoto);
      foreach (var message in messages)
      {
        log.Info(string.Format("{0} - Copying; file: {1}", Name, message.Filename));

        try
        {
          File.Copy(Path.Combine(message.Path, message.Filename), Path.Combine(CommunicatorService.GetMessagePath(message.VendorID, PfaCommunicator.Objects.Models.MessageTypes.StockPhoto).MessagePath, message.Filename));
        }
        catch (Exception e)
        {
          log.AuditError(string.Format("Error while processing file {0}", message.Filename), e);
          MessageHelper.Error(message);
          continue;
        }
        MessageHelper.Archive(message);
      }

      _monitoring.Notify(Name, 1);
    }
  }
}
