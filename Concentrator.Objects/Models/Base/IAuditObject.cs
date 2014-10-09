using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Base
{
  public interface IAuditObject
  {
    DateTime CreationTime { get; set; }

    DateTime? LastModificationTime { get; set; }

    int CreatedBy { get; set; }

    int? LastModifiedBy { get; set; }
  }
}
