using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductReview : BaseModel<ProductReview>
  {
    public Int32 ReviewID { get; set; }
          
    public Int32 ProductID { get; set; }
          
    public Int32 SourceID { get; set; }
          
    public String Author { get; set; }
          
    public Boolean IsSummary { get; set; }
          
    public Int32? Rating { get; set; }
          
    public DateTime? Date { get; set; }
          
    public String Title { get; set; }
          
    public String Verdict { get; set; }
          
    public String Summary { get; set; }
          
    public String ReviewURL { get; set; }
          
    public Int32? SourceRating { get; set; }
          
    public String CustomID { get; set; }
          
    public String RatingImageURL { get; set; }
          
    public virtual Product Product { get;set;}
            
    public virtual ReviewSource ReviewSource { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductReview, bool>> GetFilter()
    {
      return null;
    }
  }
}