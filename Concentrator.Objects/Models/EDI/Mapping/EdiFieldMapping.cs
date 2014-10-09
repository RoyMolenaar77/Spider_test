using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.EDI.Order;

namespace Concentrator.Objects.Models.EDI.Mapping
{
  public class EdiFieldMapping : BaseModel<EdiFieldMapping>
  {
    public Int32 EdiMappingID { get; set; }

    public string TableName { get; set; }

    public string FieldName { get; set; }

    public Int32 EdiVendorID { get; set; }

    public string VendorFieldName { get; set; }

    public string VendorTableName { get; set; }

    public string VendorFieldLength { get; set; }

    public string VendorDefaultValue { get; set; }

    public int? VendorFieldType { get; set; }

    public int? EdiCommunicationID { get; set; }

    public virtual EdiCommunication EdiCommunication { get; set; }

    public bool MatchField { get; set; }

    public Int32 EdiType { get; set; }

    public virtual EdiVendor EdiVendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiFieldMapping, bool>> GetFilter()
    {
      return null;
    }
  }
}
