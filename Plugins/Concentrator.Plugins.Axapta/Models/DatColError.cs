using System;
using FileHelpers;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord(";")]
  public class DatColErrorFileName
  {
    public string Code = "FileName ";
    public string FileName;
  }

  [DelimitedRecord(";")]
  public class DatColErrorMessage
  {
    public string Code = "Error Message ";
    public string Message;
  }

  [DelimitedRecord(";")]
  public class DatColErrorOrderLines
  {
    public string Code = "OrderLine ";
    public string Line;
  }

  [DelimitedRecord(";")]
  public class DatColErrorReport
  {
    public string Code = "Resume ";
    public String Today = string.Format("{0:dd-MM-yyyy} at {0:H:mm:ss}", DateTime.Now);
    public int TotalErrors;
    public int TotalOrderLines;
  }
}
