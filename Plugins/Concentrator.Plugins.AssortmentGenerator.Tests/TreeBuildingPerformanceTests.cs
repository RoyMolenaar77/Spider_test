using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Objects.Assortment;

namespace Concentrator.Plugins.AssortmentGenerator.Tests
{
  [TestClass]
  public class TreeBuildingPerformanceTests
  {
    private AssortmentTree _tree;
    private int _rootID = -1;
    private Dictionary<int, List<int>> _productProductGroupMapping;


    [TestInitialize]
    public void Setup()
    {
      _tree = new AssortmentTree();
      _productProductGroupMapping = new Dictionary<int, List<int>>();

      for (int i = 0; i < 10000; i++)
      {
        _productProductGroupMapping.Add(i, Enumerable.Range(0, 900).ToList());
      }
    }
    /// <summary>
    /// This is just a performance test
    /// </summary>
    //[TestMethod]
    public void Add_Product_To_Level_1_Should_Attach_The_Products_To_The_Right_Node_500000_Products()
    {
      Random r = new Random();

      _tree.AddToTree(1);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 1);
      _tree.AddToTree(4, 1);
      _tree.AddToTree(5, 1);

      for (int i = 6; i < 700; i++)
      {
        _tree.AddToTree(i, i % 2 == 0 ? 2 : 3);
        _tree.AddProducts(Enumerable.Range(0, r.Next(2000)).ToList(), i, _productProductGroupMapping, false);
      }
    }
  }
}
