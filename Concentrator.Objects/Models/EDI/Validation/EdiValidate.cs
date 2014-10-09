using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Validation
{
  public class EdiValidate : BaseModel<EdiValidate>
  {
    public Int32 EdiValidateID { get; set; }

    public string TableName { get; set; }

    public string FieldName { get; set; }

    public Int32 EdiVendorID { get; set; }

    public Int32? MaxLength { get; set; }

    public string Type { get; set; }

    public string Value { get; set; }

    public bool IsActive { get; set; }

    public Int32 EdiType { get; set; }

    public Int32 EdiValidationType { get; set; }

    public virtual EdiVendor EdiVendor { get; set; }

    public string Connection { get; set; }

    public int EdiConnectionType { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiValidate, bool>> GetFilter()
    {
      return null;
    }
  }
}
