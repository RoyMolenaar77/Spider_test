using System;
using System.Collections.Generic;
using Concentrator.Core.Services.Models.Localization;

namespace Concentrator.Core.Services.Models.Attributes
{
  public class Attribute
  {
    public string Code { get; set; }

    public IEnumerable<LocalizedContent> Translations { get; set; }

    public IEnumerable<ConnectorAttributeSetting> ConnectorAttributeSettings { get; set; }

    public IEnumerable<AttributeGroup> Groups { get; set; }

    public bool IsFilterAttribute { get; set; }

    public AppliesTo AppliesToProductType { get; set; }

    public string ImageUrl { get; set; }
  }

  [Flags]
  public enum AppliesTo
  {
    Simple = 1,
    Configurable = 2
  }
}
