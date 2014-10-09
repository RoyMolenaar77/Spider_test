using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Media
{
  public class BrandMediaView : BaseModel<BrandMediaView>
  {
    public Int32 BrandID { get; set; }

    public String name { get; set; }

    public String ImagePath { get; set; }

    public String Logo { get; set; }

    public Int32 ContentVendorSettingID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32? Sequence { get; set; }

    public String ImageType { get; set; }

    //public Int32 MediaID { get; set; }

    public override System.Linq.Expressions.Expression<Func<BrandMediaView, bool>> GetFilter()
    {
      return null;
    }
  }
}