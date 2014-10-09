using System;
using System.Collections.Generic;

namespace Concentrator.Tasks.Vlisco.Common
{
  using Models;

  public class ArticleColorSizeComparer : IEqualityComparer<Article>
  {
    public static readonly ArticleColorSizeComparer Default = new ArticleColorSizeComparer();

    public virtual Boolean Equals(Article left, Article right)
    {
      return left.ArticleCode == right.ArticleCode
        && left.ColorCode == right.ColorCode
        && left.SizeCode == right.SizeCode;
    }

    public Int32 GetHashCode(Article article)
    {
      return article.GetHashCode();
    }
  }
}
