using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentProductMedia : AssortmentProduct
  {
    public int MediaID { get; set; }

    public string Description { get; set; }

    public int BrandID { get; set; }

    public int Sequence { get; set; }

    public string ImageType { get; set; }

    public string Url { get; set; }

    public bool IsThumbnailImage { get; set; }

    public AssortmentThumbnail Thumbs { get; set; }
  }
}
