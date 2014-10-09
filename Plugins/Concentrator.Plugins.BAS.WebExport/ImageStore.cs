using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace Concentrator.Plugins.BAS.WebExport
{
  public partial class ImageStore
  {
    private System.Nullable<DateTime> _LastModificationTime;

    partial void OnConcentratorLastModificationTimeChanging(System.Nullable<DateTime> value);
    partial void OnConcentratorLastModificationTimeChanged();

    [Column(Storage = "_LastModificationTime", DbType = "DateTime")]
    public System.Nullable<DateTime> LastModificationTime
    {
      get
      {
        return this._LastModificationTime;
      }
      set
      {
        if ((this._LastModificationTime != value))
        {
          this.OnConcentratorLastModificationTimeChanging(value);
          this.SendPropertyChanging();
          this._LastModificationTime = value;
          this.SendPropertyChanged("LastModificationTime");
          this.OnConcentratorLastModificationTimeChanged();
        }
      }
    }
  }
}
