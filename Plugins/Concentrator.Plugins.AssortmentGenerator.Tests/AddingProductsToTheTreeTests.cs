using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Objects.Assortment;

namespace Concentrator.Plugins.AssortmentGenerator.Tests
{
  [TestClass]
  public class AddingProductsToTheTreeTests
  {
    private AssortmentTree _tree;
    private Dictionary<int, List<int>> _productProductGroupMapping;


    [TestInitialize]
    public void Setup()
    {
      _tree = new AssortmentTree();
      _productProductGroupMapping = new Dictionary<int, List<int>>();

      for (int i = 0; i < 20000; i++)
      {
        _productProductGroupMapping.Add(i, Enumerable.Range(0, 900).ToList());
      }
    }

    [TestMethod]
    public void Add_Product_To_Level_1_Should_Attach_The_Products_To_The_Right_Node()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1);

      _tree.AddProducts(Enumerable.Range(1, 10000).ToList(), 2, _productProductGroupMapping, false);

      Assert.IsTrue(_tree.GetNode(2).Products.Count == 10000);
    }

    [TestMethod]
    public void Add_Products_To_Level_With_Children_Should_Not_Attach_To_Parent_category()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(3, 1);

      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);

      var node = _tree.GetNode(1);
      Assert.IsTrue(node.Products.Count == 0);
    }

    [TestMethod]
    public void Add_Product_To_Two_Parent_Child_Nodes_In_Tree_Should_Only_Attach_To_The_Child_Node()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(3, 1);

      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 1 }, 3, _productProductGroupMapping);

      var node = _tree.GetNode(3);
      Assert.IsTrue(node.Products.Count == 1);
    }

    [TestMethod]
    public void Add_Product_To_Two_Child_Nodes_Should_Attach_To_Both_Nodes()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(3, 1);

      _tree.AddToTree(2);
      _tree.AddToTree(4, 2);

      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 1 }, 2, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 1 }, 3, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 2 }, 4, _productProductGroupMapping);

      var node = _tree.GetNode(3);
      var nodeOtherChild = _tree.GetNode(4);
      var parentNode = _tree.GetNode(1);
      var otherParent = _tree.GetNode(2);

      Assert.IsTrue(node.Products.Count == 1 && nodeOtherChild.Products.Count == 1 && parentNode.Products.Count == 0 && otherParent.Products.Count == 0);

    }

    /// <summary>
    /// product x -> categories {1,2}. 1 = flatten
    /// Should attach only to 1
    /// </summary>
    [TestMethod]
    public void Add_Product_To_Child_Category_With_Parent_Flatten_Should_Attach_To_Parent()
    {
      _tree.AddToTree(1, flattenHierarchy: true);
      _tree.AddToTree(2, 1);

      _tree.AddProducts(new List<int>() { 1 }, 2, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 1 && _tree.GetNode(2).Products.Count == 0);

    }

    /// <summary>
    /// product x -> category {2}. 1 = flatten. 
    /// Should attach to 1
    /// </summary>
    [TestMethod]
    public void Add_Product_Only_Attached_To_Child_Category_To_Child_Category_With_Parent_Flatten_Should_Attach_To_Parent()
    {
      _tree.AddToTree(1, flattenHierarchy: true);
      _tree.AddToTree(2, 1);

      _tree.AddProducts(new List<int>() { 1 }, 2, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 1 && _tree.GetNode(2).Products.Count == 0);
    }

    /// <summary>
    /// product x -> category {1}. 1 = flatten. 1 has children
    /// Should not attach
    /// </summary>
    [TestMethod]
    public void Add_Product_Only_Attached_To_Parent_Category_To_Child_Category_With_Parent_Flatten_Should_Attach_To_Parent()
    {
      _tree.AddToTree(1, flattenHierarchy: true);
      _tree.AddToTree(2, 1);

      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 0 && _tree.GetNode(2).Products.Count == 0);
    }

    /// <summary>
    /// product x -> category {3}. 1 = flatten, 2 = flatten. 
    /// Should attach to 1
    /// </summary>
    [TestMethod]
    public void Add_Product_Attached_To_Level_3_With_All_Levels_On_Flatten_Should_Attach_To_Level_1()
    {
      _tree.AddToTree(1, flattenHierarchy: true);
      _tree.AddToTree(2, 1, flattenHierarchy: true);
      _tree.AddToTree(3, 2);

      _tree.AddProducts(new List<int>() { 1 }, 3, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 1 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(3).Products.Count == 0);
    }

    /// <summary>
    /// product x -> category {3}. 1 = flatten
    /// Should attach to 1
    /// </summary> 
    [TestMethod]
    public void Add_Product_Attached_To_Level_3_With_Level_1_On_Flatten_Level_2_Not_Flatten_Should_Attach_To_1()
    {
      _tree.AddToTree(1, flattenHierarchy: true);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 2);

      _tree.AddProducts(new List<int>() { 1 }, 3, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 1 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(3).Products.Count == 0);
    }

    [TestMethod]
    public void Add_To_Category_With_Filter_By_Parent_Should_Not_Attach_If_Not_In_Parent_Group()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true);

      _tree.AddProduct(1, 2, new List<int>() { 2 });
      Assert.IsTrue(_tree.GetNode(2).Products.Count == 0);
    }

    [TestMethod]
    public void Add_To_Category_With_Filter_By_Parent_Should_Attach_If_In_Parent_Group()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true);

      _tree.AddProduct(1, 2, new List<int>() { 1, 2 });
      Assert.IsTrue(_tree.GetNode(2).Products.Count == 1);
    }

    /// <summary>
    /// product -> x category {1,2,3} , 2,3 = filter by parent
    /// Should attach to 3
    /// </summary>
    [TestMethod]
    public void Add_To_Category_With_Filter_By_Parent_Two_Levels_Deep()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true);
      _tree.AddToTree(3, 2, filterByParent: true);

      _tree.AddProduct(1, 3, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 1, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 2, new List<int>() { 1, 2, 3 });

      Assert.IsTrue(_tree.GetNode(3).Products.Count == 1 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(1).Products.Count == 0);
    }

    /// <summary>
    /// product -> x category {1,2,3} , 2 = filter by parent
    /// Should attach to 3
    /// </summary>
    [TestMethod]
    public void Add_To_Category_With_Filter_By_Parent_Two_Levels_Deep_Level_3_Without_FBP()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true);
      _tree.AddToTree(3, 2);

      _tree.AddProduct(1, 3, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 1, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 2, new List<int>() { 1, 2, 3 });

      Assert.IsTrue(_tree.GetNode(3).Products.Count == 1 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(1).Products.Count == 0);
    }

    /// <summary>
    /// product -> x category {1,2,3} , 2 = filter by parent
    /// Should attach to 3
    /// </summary>
    [TestMethod]
    public void Add_To_Category_With_Filter_By_Parent_Two_Levels_Deep_Should_Not_Attach_If_Product_Doesnt_Have_Correct_Groups()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true);
      _tree.AddToTree(3, 2, filterByParent: true);

      _tree.AddProduct(1, 3, new List<int>() { 2, 3 });
      _tree.AddProduct(1, 1, new List<int>() { 2, 3 });
      _tree.AddProduct(1, 2, new List<int>() { 2, 3 });

      Assert.IsTrue(_tree.GetNode(3).Products.Count == 0 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(1).Products.Count == 0);
    }

    /// <summary>
    /// product -> x category {3} , 2,3 = filter by parent
    /// Should not attachj
    /// </summary>
    [TestMethod]
    public void Add_To_Category_With_Filter_By_Parent_Two_Levels_Deep_Should_Not_Attach_If_Product_Only_Has_Deepest_Child()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true);
      _tree.AddToTree(3, 2, filterByParent: true);

      _tree.AddProduct(1, 3, new List<int>() { 3 });
      _tree.AddProduct(1, 1, new List<int>() { 3 });
      _tree.AddProduct(1, 2, new List<int>() { 3 });

      Assert.IsTrue(_tree.GetNode(3).Products.Count == 0 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(1).Products.Count == 0);
    }

    /// <summary>
    /// product -> x category {1,2,3} , 2,3 = filter by parent, 1 = flatten
    /// Should attach to 1
    /// </summary>
    [TestMethod]
    public void Add_To_Category_Two_Levels_Deep_Flatten_On_Top_And_Filter_By_Parent_Deeper_Should_Attach_To_Top()
    {
      _tree.AddToTree(1, flattenHierarchy: true);
      _tree.AddToTree(2, 1, filterByParent: true);
      _tree.AddToTree(3, 2, filterByParent: true);

      _tree.AddProduct(1, 3, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 1, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 2, new List<int>() { 1, 2, 3 });

      Assert.IsTrue(_tree.GetNode(3).Products.Count == 0 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(1).Products.Count == 1);
    }

    /// <summary>
    /// product -> x category {1,2,3} , 2,3 = filter by parent, 1 = flatten
    /// Should attach to 1
    /// </summary>
    [TestMethod]
    public void Add_To_Category_Two_Levels_Deep_Flatten_On_Second_And_Filter_By_Parent_Deeper_Should_Attach_To_Middle()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1, filterByParent: true, flattenHierarchy: true);
      _tree.AddToTree(3, 2, filterByParent: true);

      _tree.AddProduct(1, 3, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 1, new List<int>() { 1, 2, 3 });
      _tree.AddProduct(1, 2, new List<int>() { 1, 2, 3 });

      Assert.IsTrue(_tree.GetNode(3).Products.Count == 0 && _tree.GetNode(2).Products.Count == 1 && _tree.GetNode(1).Products.Count == 0);
    }

    /*
     * Test Scenario's Retain Products
    */
    [TestMethod]
    public void Add_Products_To_Parent_With_Children_And_Retain_Products_On_Parent()
    {
      _tree.AddToTree(1, retainProducts: true);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 1);

      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 2 }, 2, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 3 }, 3, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 1 && _tree.GetNode(2).Products.Count == 1 && _tree.GetNode(3).Products.Count == 1);
    }

    [TestMethod]
    public void Add_Products_To_Parent_With_Children_And_Retain_Products_And_Flatten()
    {
      _tree.AddToTree(1, flattenHierarchy: true, retainProducts: true);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 1);

      _tree.AddProducts(new List<int>() { 1 }, 1, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 2 }, 2, _productProductGroupMapping);
      _tree.AddProducts(new List<int>() { 3 }, 3, _productProductGroupMapping);

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 3 && _tree.GetNode(2).Products.Count == 0 && _tree.GetNode(3).Products.Count == 0);
    }

    [TestMethod]
    public void Add_Products_To_Parent_With_Children_And_Retain_Products_And_FilterByParent()
    {
      _tree.AddToTree(1, retainProducts: true);
      _tree.AddToTree(2, 1, filterByParent: true);

      _tree.AddProduct(1, 1, new List<int>() { 1, 2 });
      _tree.AddProduct(1, 2, new List<int>() { 1, 2 });
      _tree.AddProduct(2, 2, new List<int>() { 2 });

      Assert.IsTrue(_tree.GetNode(1).Products.Count == 1 && _tree.GetNode(2).Products.Count == 1);
    }

    [TestMethod]
    public void Add_Products_To_Parent_With_Children_And_Retain_Products_And_FilterByParent_Two_Levels()
    {
      _tree.AddToTree(1);
      _tree.AddToTree(2, 1);
      _tree.AddToTree(3, 2, retainProducts: true);
      _tree.AddToTree(4, 3, filterByParent: true);

      _tree.AddProduct(1, 3, new List<int> { 3, 4 });
      _tree.AddProduct(1, 4, new List<int> { 3, 4 });


      Assert.IsTrue(_tree.GetNode(4).Products.Count == 1 && _tree.GetNode(3).Products.Count == 1);
    }


  }
}
