#region Usings

using System;

#endregion

namespace Concentrator.Tasks.Euretco.RSO.Navision.Models
{
  public class ProductAttributeModel
  {
    public Int32 ProductID { get; set; }
    public int AttributeID { get; set; }
    public String AttributeValue { get; set; }
  }
}