using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.AssortmentGenerator.Tests
{
  public class ProductGroupMappingMock
  {
    public int MappingID { get; set; }

    public List<ProductGroupMappingMock> ChildMappings { get; set; }
  }
}
