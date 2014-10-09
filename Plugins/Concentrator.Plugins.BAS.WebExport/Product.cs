using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace Concentrator.Plugins.BAS.WebExport
{
  public partial class Product
  {
    private Nullable<DateTime> _cutoffTime;
    partial void OnCutoffTimeChanging(System.Nullable<DateTime> value);
    partial void OnCutoffTimeChanged();

    [Column(Storage = "_cutoffTime", DbType = "DateTime")]
    public System.Nullable<DateTime> CutoffTime
    {
      get
      {
        return this._cutoffTime;
      }
      set
      {
        if ((this._cutoffTime != value))
        {
          this.OnCutoffTimeChanging(value);
          this.SendPropertyChanging();
          this._cutoffTime = value;
          this.SendPropertyChanged("CutoffTime");
          this.OnCutoffTimeChanged();
        }
      }
    }
  }
}
