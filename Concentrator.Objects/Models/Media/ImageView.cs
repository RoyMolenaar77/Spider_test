using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Media
{
  public class ImageView : BaseModel<ImageView>
  {
    public Int32 BrandID { get; set; }

    public String name { get; set; }

    public Int32 ProductID { get; set; }

    public String ManufacturerID { get; set; }

    public String ImagePath { get; set; }

    public String ImageUrl { get; set; }

    public Int32 ContentVendorSettingID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32? Sequence { get; set; }

    public String ImageType { get; set; }

    public Int32 mediaid { get; set; }

    public string Description { get; set; }

    public bool IsThumbNailImage { get; set; }

		public DateTime LastChanged { get; set; }

    public override System.Linq.Expressions.Expression<Func<ImageView, bool>> GetFilter()
    {
      return null;
    }
  }
}