using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Dashboards;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Models.WebToPrint;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Templates;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Users
{
  public class User : AuditObjectBase<User>
  {
    public Int32 UserID { get; set; }

    public String Username { get; set; }

    public String Password { get; set; }

    public String Firstname { get; set; }

    public String Lastname { get; set; }

    public Boolean IsActive { get; set; }

    public Int32 LanguageID { get; set; }

    public Int32? ConnectorID { get; set; }

    public String Logo { get; set; }

    public Int32 Timeout { get; set; }

    public String Email { get; set; }

    public int OrganizationID { get; set; }

    public virtual Organization Organization { get; set; }

    public virtual ICollection<Contents.Content> Contents { get; set; }

    public virtual ICollection<Contents.Content> Contents1 { get; set; }

    public virtual ICollection<CrossLedgerclass> CrossLedgerclasses { get; set; }

    public virtual ICollection<CrossLedgerclass> CrossLedgerclasses1 { get; set; }

    public virtual Language Language { get; set; }

    public virtual ICollection<Products.Product> Products { get; set; }

    public virtual ICollection<Products.Product> Products1 { get; set; }

    public virtual ICollection<ProductDescription> ProductDescriptions { get; set; }

    public virtual ICollection<ProductDescription> ProductDescriptions1 { get; set; }

    public virtual ICollection<RelatedProduct> RelatedProducts { get; set; }

    public virtual ICollection<RelatedProduct> RelatedProducts1 { get; set; }

    public virtual ICollection<UserPortal> UserPortals { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }

    public virtual ICollection<UserState> UserStates { get; set; }

    public virtual ICollection<ProductCompetitorPrice> ProductCompetitorPrices { get; set; }

    public virtual ICollection<ProductCompetitorPrice> ProductCompetitorPrices1 { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<UserDownload> UserDownloads { get; set; }

    public virtual ICollection<Event> Events { get; set; }

    public virtual ICollection<Event> Events1 { get; set; }

    public virtual ICollection<ContentStock> ContentStocks { get; set; }

    public virtual ICollection<ContentStock> ContentStocks1 { get; set; }


    public ICollection<ExportTemplate> ExportTemplates { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }


    #region WebToPrint
    public virtual ICollection<WebToPrintProject> WebToPrintProjects { get; set; }
    #endregion

    public override System.Linq.Expressions.Expression<Func<User, bool>> GetFilter()
    {

      return null;
    }
  }
}