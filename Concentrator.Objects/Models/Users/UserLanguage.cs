using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Users
{
  public class UserLanguage : BaseModel<UserLanguage>
  {
    public Int32 UserID { get; set; }
    public Int32 LanguageID { get; set; }

    public override System.Linq.Expressions.Expression<Func<UserLanguage, bool>> GetFilter()
    {

      return null;
    }
  }
}
