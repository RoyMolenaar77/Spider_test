using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
                  
namespace Concentrator.Objects.Models.Products 
{
  public class ReviewSource : BaseModel<ReviewSource>
  {
    public Int32 SourceID { get; set; }
          
    public String Name { get; set; }
          
    public String CountryCode { get; set; }
          
    public String LanguageCode { get; set; }
          
    public String SourceUrl { get; set; }
          
    public String SourceLogoUrl { get; set; }
          
    public Int32? SourceRank { get; set; }
          
    public Int32 CustomSourceID { get; set; }
          
    public virtual ICollection<ProductReview> ProductReviews { get;set;}


    public override System.Linq.Expressions.Expression<Func<ReviewSource, bool>> GetFilter()
    {
      return null;
    }
  }
}