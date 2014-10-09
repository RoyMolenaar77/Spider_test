using System.Collections.Generic;
using Concentrator.Core.Services.Models.Localization;

namespace Concentrator.Core.Services.Models.Categories
{
  public class Category
  {
    public IEnumerable<LocalizedContent> NameTranslations { get; set; }

    public string BackendCategoryID { get; set; }

    public List<Category> Children { get; set; }
  }
}
