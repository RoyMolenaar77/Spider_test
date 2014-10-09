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
  public class MediaType : BaseModel<MediaType>
  {
    public Int32 TypeID { get; set; }

    public String Type { get; set; }

    public virtual ICollection<ProductMedia> ProductMedias { get; set; }

    public virtual ICollection<BrandMedia> BrandMedias { get; set; }
    public virtual ICollection<UserDownload> UserDownloads { get; set; }

    public override System.Linq.Expressions.Expression<Func<MediaType, bool>> GetFilter()
    {
      return null;
    }
  }
}