using System.Diagnostics;

namespace Concentrator.Tasks.Euretco.RSO.MediaExporter.Models
{

  [DebuggerDisplay("ProductID = {ProductID}, VendorID={VendorID}")]
  public class ProductMediaModel
  {
    public int VendorID { get; set; }
    public int ProductID { get; set; }
    public int Sequence { get; set; }
    public bool IsConfigurable { get; set; }
    public string FileName { get; set; }
    public string MediaPath { get; set; }
    public int TypeID { get; set; }
    public string Barcode { get; set; }
    public string VendorItemNumber { get; set; }
  }
}
