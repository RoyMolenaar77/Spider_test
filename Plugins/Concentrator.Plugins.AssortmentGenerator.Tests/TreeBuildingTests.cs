using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Objects.Assortment;

namespace Concentrator.Plugins.AssortmentGenerator.Tests
{
  [TestClass]
  public class TreeBuildingTests
  {
    private AssortmentTree _tree;
    private int _rootID = -1;

    [TestInitialize]
    public void Setup()
    {
      _tree = new AssortmentTree();
    }

    [TestMethod]
    public void Initialize_Tree_Should_Be_Initialized_With_A_Root()
    {
      Assert.IsTrue(_tree.Root != null && _tree.Root.MasterGroupMappingID == _rootID);
    }

    [TestMethod]
    public void Add_Category_Should_Be_Added_To_Tree_Root()
    {
      _tree.AddToTree(1);
      var addedNode = _tree.GetNode(1);

      Assert.IsTrue(addedNode != null && addedNode.MasterGroupMappingID == 1 && addedNode.Parent.MasterGroupMappingID == _rootID);
    }

    [TestMethod]
    public void Add_Category_Level_Two_Should_Attach_To_Parent_And_Parent_To_Root()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1);

      var level2 = _tree.GetNode(2);
      Assert.IsTrue(level2 != null && level2.MasterGroupMappingID == 2 && level2.Parent.MasterGroupMappingID == 1);
    }

    [TestMethod]
    public void Add_Category_Level_Three_Should_Attach_To_Parent_And_Parent_To_Root()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 2);

      var level3 = _tree.GetNode(3);
      Assert.IsTrue(level3 != null && level3.MasterGroupMappingID == 3 && level3.Parent.MasterGroupMappingID == 2);
    }

    [TestMethod]
    public void Add_Category_Level_Three_With_Two_Roots_Should_Attach_To_Parent_And_Parent_To_Root()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 1);

      _tree.AddToTree(4);
      _tree.AddToTree(5, 4);
      _tree.AddToTree(6, 4);

      var level3 = _tree.GetNode(6);
      Assert.IsTrue(level3 != null && level3.MasterGroupMappingID == 6 && level3.Parent.MasterGroupMappingID == 4);
    }

    [TestMethod]
    public void Add_To_Tree_With_Filters_SHould_Add_The_Filters_To_The_Categories()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, true, true);

      var node = _tree.GetNode(2);
      Assert.IsTrue(node.FlattenHierarchy && node.FilterByParent);
    }
  }
}
