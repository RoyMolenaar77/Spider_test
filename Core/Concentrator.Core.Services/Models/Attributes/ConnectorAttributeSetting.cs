using System;

namespace Concentrator.Core.Services.Models.Attributes
{
  public class ConnectorAttributeSetting
  {
    public string Code { get; set; }

    public string Value { get; set; }

    public Type SettingType { get; set; }
  }
}
