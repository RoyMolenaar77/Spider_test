using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Models.Media
{
  public class ProductMediaTumbnail : BaseModel<ProductMediaTumbnail>
  {
    public Int32 ThumbnailGeneratorID { get; set; }

    public Int32 MediaID { get; set; }

    public String Path { get; set; }

    public virtual ProductMedia ProductMedia { get; set; }
    public virtual ThumbnailGenerator ThumbnailGenerator { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductMediaTumbnail, bool>> GetFilter()
    {
      return null;
    }
  }
}