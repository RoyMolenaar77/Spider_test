using System;

namespace Concentrator.Plugins.PFA.Transfer.Model.Mutation
{
  public class MutDetail : MutBase
  {
    public MutDetail(string line)
      : base(line)
    {
      LastSixReceiptNumbers = line.SubstringNullOrTrim(33, 6);
      ArticleCode = line.SubstringNullOrTrim(40, 10);
      ColorCode = line.SubstringNullOrTrim(54, 3);
      ColorDescription = line.SubstringNullOrTrim(58, 7);
      SizeCode = line.SubstringNullOrTrim(66, 4);
      Size = line.SubstringNullOrTrim(71, 7);
      Default2 = line.SubstringNullOrTrim(79, 1);

      int parsedNumberOfSKUs;

      if (!int.TryParse(line.SubstringNullOrTrim(81, 5), out parsedNumberOfSKUs))
        throw new Exception("Unable trying to parse number of skus");

      NumberOfSKUs = parsedNumberOfSKUs;
      ArticleName = line.SubstringNullOrTrim(88, 12);
      Barcode = line.SubstringNullOrTrim(109, 12);
    }

    public String LastSixReceiptNumbers;

    public String ArticleCode;

    public String ColorCode;

    public String ColorDescription;

    public String SizeCode;

    public String Size;

    public String Default2;

    public Int32 NumberOfSKUs;

    public String ArticleName;

    public String Barcode;
  }
}
