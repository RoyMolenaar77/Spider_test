using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Media
{
  public class ImageStore : BaseModel<ImageStore>
  {
    public Int32 ImageID { get; set; }

    public String ManufacturerID { get; set; }

    public Int32 BrandID { get; set; }

    public String ImageUrl { get; set; }

    public Int32 SizeID { get; set; }

    public String CustomerProductID { get; set; }

    public Int32 ProductGroupID { get; set; }

    public String ImageType { get; set; }

    public Int32 Sequence { get; set; }

    public String BrandCode { get; set; }

    public Int32 ConcentratorProductID { get; set; }


    public override System.Linq.Expressions.Expression<Func<ImageStore, bool>> GetFilter()
    {
      return null;
    }
  }
}