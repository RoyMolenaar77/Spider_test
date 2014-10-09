using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.EDI.Validation;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Mapping;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Vendor
{
  public class EdiVendor : BaseModel<EdiVendor>
  {
    public Int32 EdiVendorID { get; set; }

    public String Name { get; set; }

    public String EdiVendorType { get; set; }

    public String CompanyCode { get; set; }

    public String DefaultDocumentType { get; set; }

    public String OrderBy { get; set; }

    public virtual ICollection<EdiValidate> EdiValidates { get; set; }

    public virtual ICollection<EdiFieldMapping> EdiFieldMappings { get; set; }

    public virtual ICollection<ConnectorRelation> ConnectorRelations { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiVendor, bool>> GetFilter()
    {
      return null;
    }
  }
}
