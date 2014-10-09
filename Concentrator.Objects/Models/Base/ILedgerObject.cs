using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Data.Objects;
using System.Data.Common;

namespace Concentrator.Objects.Models.Base
{
  public interface ILedgerObject
  {
    string ReturnPrimaryKeyHash();
  }
}
