using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Faq;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Models.Localization
{
  public class Language : BaseModel<Language>
  {
    public Int32 LanguageID { get; set; }

    public String Name { get; set; }

    public String DisplayCode { get; set; }

    public virtual ICollection<ConnectorLanguage> ConnectorLanguages { get; set; }

    public virtual ICollection<ProductDescription> ProductDescriptions { get; set; }

    public virtual ICollection<ProductAttributeDescription> ProductAttributeDescriptions { get; set; }

    public virtual ICollection<ProductAttributeGroupName> ProductAttributeGroupNames { get; set; }

    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; }

    public virtual ICollection<ProductAttributeName> ProductAttributeNames { get; set; }

    public virtual ICollection<ProductGroupLanguage> ProductGroupLanguages { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public virtual ICollection<FaqProduct> FaqProducts { get; set; }

    public virtual ICollection<FaqTranslation> FaqTranslations { get; set; }

    public virtual ICollection<ConnectorRelation> ConnectorRelations { get; set; }

    public virtual ICollection<ProductAttributeValueGroupName> ProductAttributeValueGroupNames { get; set; }

    public virtual ICollection<ProductAttributeValueLabel> ProductAttributeValueLabels { get; set; }

    public virtual ICollection<ProductGroupMappingDescription> ProductGroupMappingDescriptions { get; set; }

    public virtual ICollection<MasterGroupMappingDescription> MasterGroupMappingDescriptions { get; set; }

    public virtual ICollection<ProductGroupMappingCustomLabel> ProductGroupMappingCustomLabels { get; set; }

    public virtual ICollection<MasterGroupMappingCustomLabel> MasterGroupMappingCustomLabels { get; set; }

    public virtual IList<PaymentMethodDescription> PaymentMethodDescriptions { get; set; }

    public virtual ICollection<MasterGroupMappingLanguage> MasterGroupMappingLanguages { get; set; }

    public virtual ICollection<MasterGroupMappingMedia> MasterGroupMappingMedias { get; set; }
    
    public virtual ICollection<SeoTexts> SeoTexts { get; set; }

    public override System.Linq.Expressions.Expression<Func<Language, bool>> GetFilter()
    {
      return null;
    }
  }
}