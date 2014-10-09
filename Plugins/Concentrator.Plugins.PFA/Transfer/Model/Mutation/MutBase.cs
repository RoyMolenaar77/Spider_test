using System;

namespace Concentrator.Plugins.PFA.Transfer.Model.Mutation
{
  public class MutBase
  {
    public MutBase(string line)
    {
      Chain = line.SubstringNullOrTrim(0, 2);
      ReceivingShopnumber = line.SubstringNullOrTrim(3, 3);
      RecordType = line.SubstringNullOrTrim(7, 1);
      Default1 = line.SubstringNullOrTrim(9, 2);
      CountryCode = line.SubstringNullOrTrim(12, 2);
      LanguageCode = line.SubstringNullOrTrim(15, 2);
      Type = line.SubstringNullOrTrim(18, 3);
      TransferReceipt = line.SubstringNullOrTrim(22, 7);      
    }

    public String Chain;

    public String ReceivingShopnumber;

    public String RecordType;

    public String Default1;

    public String CountryCode;

    public String LanguageCode;

    public String Type;

    public String TransferReceipt;
  }
}
