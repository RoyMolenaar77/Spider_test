using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Products
{
  public enum MatchStatuses
  {
    /// <summary>
    /// Waiting for acceptance
    /// </summary>
    New = 1,

    /// <summary>
    /// Reported as a match
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Reported as a non match
    /// </summary>
    Declined = 3
  }

}
