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
  public class ThumbnailGenerator : AuditObjectBase<ThumbnailGenerator>
  {
    public Int32 ThumbnailGeneratorID { get; set; }

    public Int32 Width { get; set; }

    public Int32 Height { get; set; }

    public Int32? Resolution { get; set; }

    public String Description { get; set; }

    public virtual ICollection<ProductMediaTumbnail> ProductMediaTumbnails { get; set; }

    public override System.Linq.Expressions.Expression<Func<ThumbnailGenerator, bool>> GetFilter()
    {
      return null;
    }
  }
}