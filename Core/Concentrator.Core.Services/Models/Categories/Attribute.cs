using System;
using System.Collections.Generic;
using Concentrator.Core.Services.Models.Localization;

namespace Concentrator.Core.Services.Models.Categories
{
  public class Attribute
  {
    public string Code { get; set; }

    public Type AttributeType { get; set; }

    public bool IsMedia { get; set; }

    public IEnumerable<LocalizedContent> Translation { get; set; }
  }
}
