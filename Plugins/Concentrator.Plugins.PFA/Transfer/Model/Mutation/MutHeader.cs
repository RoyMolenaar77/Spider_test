using System;
using System.Collections.Generic;
using System.Globalization;

namespace Concentrator.Plugins.PFA.Transfer.Model.Mutation
{
  public class MutHeader : MutBase
  {
    public MutHeader(string line)
      : base(line)
    {
      SendingStoreNumber = line.SubstringNullOrTrim(30, 3);
      ReceivingStoreNumber = line.SubstringNullOrTrim(34, 3);
      Default2 = line.SubstringNullOrTrim(38, 1);

      DateTime parsedSystemDate;

      parsedSystemDate = ParseDateTime(line.SubstringNullOrTrim(40, 10));
      
      SystemDate = parsedSystemDate;

      DateTime parsedTransportDate;

      parsedTransportDate = ParseDateTime(line.SubstringNullOrTrim(51, 10));        

      TransportDate = parsedSystemDate;

      Details = new List<MutDetail>();
    }

    private DateTime ParseDateTime(string dateTime) {
      DateTime parsedDate;

      if (!DateTime.TryParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
        throw new Exception("Unable to parse SystemDate");

      return parsedDate;
    }

    public String SendingStoreNumber;

    public String ReceivingStoreNumber;

    public String Default2;

    public DateTime SystemDate;

    public DateTime TransportDate;

    public List<MutDetail> Details;

    public MutTotal Total;
  }
}
