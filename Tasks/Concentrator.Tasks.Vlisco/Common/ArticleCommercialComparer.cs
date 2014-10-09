using System;

namespace Concentrator.Tasks.Vlisco.Common
{
  using Models;

  public class ArticleCommercialComparer : ArticleColorSizeComparer
  {
    public new static readonly ArticleCommercialComparer Default = new ArticleCommercialComparer();

    public override Boolean Equals(Article left, Article right)
    {
      return base.Equals(left, right)
        && left.CountryCode == right.CountryCode
        && left.CurrencyCode == right.CurrencyCode;
    }
  }
}
