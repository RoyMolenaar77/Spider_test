using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Concentrator.Objects.Vendors
{
  public class VendorImageInfo
  {
    /// <summary>
    /// The name with which the image will be saved to the concentrator database
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description with which the image will be saved to the concentrator database
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The sequence with which the image will be saved to the database 
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Specifies whether the image is a thumbnail image
    /// </summary>
    public bool IsThumbnail { get; set; }
  }
}
