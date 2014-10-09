using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;
                  
namespace Concentrator.Objects.Models 
{
  public class UserState : BaseModel<UserState>
  {
    public Int32 StateID { get; set; }
          
    public String EntityName { get; set; }
          
    public Int32 UserID { get; set; }
          
    public String SavedState { get; set; }
          
    public virtual User User { get;set;}


    public override System.Linq.Expressions.Expression<Func<UserState, bool>> GetFilter()
    {
      return null;
    }
  }
}