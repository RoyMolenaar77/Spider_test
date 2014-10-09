using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.PFA.Objects;
using Concentrator.Plugins.PFA.Objects.Model;
using Concentrator.Plugins.Wehkamp.Helpers;

namespace Concentrator.Plugins.Wehkamp
{
  public class StockMutationImport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Stock Mutation Import"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      var messages = MessageHelper.GetMessagesByStatusAndType(Enums.WehkampMessageStatus.Created, MessageHelper.WehkampMessageType.StockMutation);
#if DEBUG
      messages = messages.Where(c => c.MessageID == 2720).ToList();
#endif

      foreach (var message in messages)
      {
        var stockmutationhelper = new StockMutationGenerator();
        log.Info(string.Format("{0} - Loading file: {1}", Name, message.Filename));

        var stockmutations = new List<WehkampStockMutation>();
        MessageHelper.UpdateMessageStatus(message.MessageID, Enums.WehkampMessageStatus.InProgress);

        voorraadMutaties mutationData;
        var loaded = voorraadMutaties.LoadFromFile(Path.Combine(message.Path, message.Filename), out mutationData);

        if (!loaded)
        {
          log.AuditError(string.Format("Error while loading file {0}", message.Filename));
          MessageHelper.Error(message);
          continue;
        }

        try
        {
          var groupedVoorraadMutatie =
           (from v in mutationData.voorraadMutatie
            group v by new
            {
              v.artikelNummer,
              v.kleurNummer,
              v.maat
            }
              into gvm
              select new WehkampStockMutation
              {
                Articlenumber = gvm.Key.artikelNummer,
                Colorcode = gvm.Key.kleurNummer,
                Size = gvm.Key.maat,
                ProductID = ProductHelper.GetProductIDByWehkampData(gvm.Key.artikelNummer, gvm.Key.kleurNummer, gvm.Key.maat, message.VendorID),
                MutationDate = mutationData.header.berichtDatumTijd,
                MutationQuantity = gvm.Sum(v => GetMutationQuantity(v.mutatieIndicatie, v.mutatieAantal, v.artikelNummer))
              }).ToList();

          foreach (var voorraadMutatie in groupedVoorraadMutatie)
            stockmutations.Add(voorraadMutatie);

          stockmutationhelper.GenerateStockMutations(message.VendorID, stockmutations);
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

    private int GetMutationQuantity(string mutatieIndicatie, string mutatieAantal, string artikelNummer)
    {
      int count;

      if (int.TryParse(mutatieAantal, out count))
      {
        return mutatieIndicatie.ToLower() == "surplus" ? count : count * -1;
      }
      else
      {
        throw new InvalidCastException(string.Format("Cannot convert MutatieAantal {0} for product {1} to a usable number", mutatieAantal, artikelNummer));
      }
    }
  }
}
