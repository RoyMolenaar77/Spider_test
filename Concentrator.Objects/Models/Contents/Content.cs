using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Contents
{
  public class Content : AuditObjectBase<Content>
  {
    public Int32 ProductID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32? ConnectorPublicationRuleID { get; set; }

    public String ShortDescription { get; set; }

    public String LongDescription { get; set; }

    public String LineType { get; set; }

    public String LedgerClass { get; set; }

    public String ProductDesk { get; set; }

    public Boolean? ExtendedCatalog { get; set; }

    public Int32? ProductContentID { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ContentProduct ContentProduct { get; set; }

    public virtual Products.Product Product { get; set; }

    public List<int> ProductGroupVendors { get; set; }

    public virtual ICollection<ContentProductGroup> ContentProductGroup { get; set; }

    public virtual User User { get; set; }
    public virtual User User1 { get; set; }

    public override System.Linq.Expressions.Expression<Func<Content, bool>> GetFilter()
    {
      return null;
    }
  }
}