using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.AssortmentGenerator.Tests
{
  public class Mocker
  {
    public List<ProductGroupMappingMock> MockTree(int depth, int nodesPerLevel)
    {
      List<ProductGroupMappingMock> mocks = new List<ProductGroupMappingMock>();
      if (depth == 0) return mocks;

      for (int i = 0; i < nodesPerLevel; i++)
      {
        var map = new ProductGroupMappingMock()
        {
          MappingID = depth + i,
          ChildMappings = new List<ProductGroupMappingMock>()
        };
        mocks.Add(map);
        BuildTree(depth - 1, nodesPerLevel, map);
      }

      return mocks;
    }

    private void BuildTree(int level, int nrNodes, ProductGroupMappingMock currentMapping)
    {
      if (level == 0) return;

      for (int i = 0; i < nrNodes; i++)
      {
        var map = new ProductGroupMappingMock()
        {
          MappingID = (i * 10) + level,
          ChildMappings = new List<ProductGroupMappingMock>()
        };
        currentMapping.ChildMappings.Add(map);


        BuildTree(level - 1, nrNodes, map);
      }
    }
  }
}
