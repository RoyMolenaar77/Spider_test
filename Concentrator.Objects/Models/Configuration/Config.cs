using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Configuration
{
  public class Config : BaseModel<Config>
  {
    public String Name { get; set; }

    public String Value { get; set; }

    public String Description { get; set; }


    public override System.Linq.Expressions.Expression<Func<Config, bool>> GetFilter()
    {
      return null;
    }
  }
}