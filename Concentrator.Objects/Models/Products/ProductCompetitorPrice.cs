using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Models.Products
{
    public class ProductCompetitorPrice : AuditObjectBase<ProductCompetitorPrice>
    {
        public Int32 ProductCompetitorPriceID { get; set; }

        public Int32 ProductCompetitorMappingID { get; set; }

        public Int32? CompareProductID { get; set; }

        public Int32 ProductID { get; set; }

        public Decimal Price { get; set; }

        public String Stock { get; set; }

        public DateTime? LastImport { get; set; }

        public virtual Product Product { get; set; }

        public virtual ProductCompare ProductCompare { get; set; }

        public virtual ICollection<ProductCompetitorLedger> ProductCompetitorLedgers { get; set; }

        public virtual ProductCompetitorMapping ProductCompetitorMapping { get; set; }

        public virtual User User { get; set; }

        public virtual User User1 { get; set; }

        public override System.Linq.Expressions.Expression<Func<ProductCompetitorPrice, bool>> GetFilter()
        {
          return null;
        }
    }
}