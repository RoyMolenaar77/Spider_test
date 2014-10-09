using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Users
{
  public class FunctionalityResult
  {
    public string FunctionalityName { get; set; }
    public string Group { get; set; }
    public string DisplayName { get; set; }
    public bool IsEnabled { get; set; }
  }
}
