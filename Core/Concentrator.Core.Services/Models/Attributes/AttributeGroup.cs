using System.Collections.Generic;
using Concentrator.Core.Services.Models.Localization;

namespace Concentrator.Core.Services.Models.Attributes
{
  public class AttributeGroup
  {
    public int GroupID { get; set; }

    public IEnumerable<LocalizedContent> Translations { get; set; }
  }
}