using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Utility.FingerPrinting
{

    public class FingerPrintModel
    {
      public DateTime LastWriteMoment { set; get; }
      public string PartialFilePath { set; get; }

      public long Length { set; get; }
      public string SHA1 { set; get; }
      public string MD5 { set; get; }
      public string ProductId { get; set; }
      public string Sequence { get; set; }

      public FingerPrintModel()
      {
        LastWriteMoment = DateTime.MinValue;
        PartialFilePath = "";
        Length = 0;
        SHA1 = "";
        MD5 = "";
        ProductId = "";
        Sequence = "";
      }

      public override string ToString()
      {
        return String.Format("{0}|{1}|{2}|{3}|{4}", this.ProductId, this.Sequence, this.Length, this.SHA1, this.MD5);
      }
    }
}
