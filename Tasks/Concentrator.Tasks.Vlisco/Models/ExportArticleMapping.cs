using System;

namespace Concentrator.Tasks.Vlisco.Models
{
  sealed class ExportArticleMapping : ArticleMapping
  {
    public ExportArticleMapping()
    {
      Map(c => c.ProFrom).Index(32).Default(String.Empty);
      Map(c => c.ProTo).Index(33).Default(String.Empty);
    }
  }
}
