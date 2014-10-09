using System.Collections.Generic;

namespace Concentrator.Web.Services
{
  public class ConcentratorImage
  {
    public string ManufacturerID { get; set; }
    public int SupplierID { get; set; }
    public List<byte[]> ImageBlob { get; set; }
    public string CustomProductID { get; set; }
  }
}
