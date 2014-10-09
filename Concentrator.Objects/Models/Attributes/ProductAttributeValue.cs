using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Objects.Models.Attributes
{
    public class ProductAttributeValue : AuditObjectBase<ProductAttributeValue>
    {
        public ProductAttributeValue()
        { }

        public Int32? AttributeValueGroupID { get; set; }

        public Int32 AttributeValueID { get; set; }

        public Int32 AttributeID { get; set; }

        public Int32 ProductID { get; set; }

        public String Value { get; set; }

        public Int32? LanguageID { get; set; }

        public virtual Language Language { get; set; }

        public virtual Products.Product Product { get; set; }

        public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

        public override System.Linq.Expressions.Expression<Func<ProductAttributeValue, bool>> GetFilter()
        {
            return null;
        }
    }
}