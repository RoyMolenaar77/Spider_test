using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Users
{
  public class UserDownload : BaseModel<UserDownload>
  {
    public int UserID { get; set; }

    public int MediaType { get; set; }

    public int DownloadID { get; set; }

    public string MediaPath { get; set; }

    public int MediaID { get; set; }

    public bool IsProduct { get; set; }

    public String ImageSize { get; set; }

    public string MediaName { get; set; }

    public virtual User User { get; set; }

    public virtual MediaType MediaType1 { get; set; }    

    public override System.Linq.Expressions.Expression<Func<UserDownload, bool>> GetFilter()
    {
      return null;
    }
  }
}
