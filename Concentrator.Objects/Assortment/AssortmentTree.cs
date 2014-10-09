using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Assortment
{
  public class AssortmentTree
  {
    public Node Root { get; private set; }

    public AssortmentTree()
    {
      Root = new Node(null, -1);
    }

    /// <summary>
    /// Adds a mapping level to the tree
    /// </summary>
    /// <param name="masterGroupMappingID"></param>
    /// <param name="parentID"></param>
    /// <param name="flattenHierarchy"></param>
    /// <param name="filterByParent"></param>
    public void AddToTree(int masterGroupMappingID, int? parentID = null, bool flattenHierarchy = false, bool filterByParent = false, bool retainProducts = false)
    {

      if (!parentID.HasValue)
        Root.Children.Add(new Node(Root, masterGroupMappingID, filterByParent, flattenHierarchy, retainProducts));
      else
      {
        var attachNode = TraverseAndFind(parentID.Value, Root);

        attachNode.Children.Add(new Node(attachNode, masterGroupMappingID, filterByParent, flattenHierarchy, retainProducts));
      }
    }

    /// <summary>
    /// Finds a mapping from the tree
    /// </summary>
    /// <param name="masterGroupMappingID"></param>
    /// <returns></returns>
    public Node GetNode(int masterGroupMappingID)
    {
      if (Root == null) return null;

      return TraverseAndFind(masterGroupMappingID, Root);
    }

    private Node TraverseAndFind(int masterGroupMappingID, Node current)
    {
      if (current.MasterGroupMappingID == masterGroupMappingID) return current;

      foreach (var child in current.Children)
      {
        var node = TraverseAndFind(masterGroupMappingID, child);
        if (node != null) return node;
      }

      return null; //didn't find anything
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Root: " + Root.MasterGroupMappingID);

      foreach (var child in Root.Children)
      {
        BuildStringTree(1, child, sb);
      }
      return sb.ToString();
    }

    private void BuildStringTree(int level, Node current, StringBuilder sb)
    {
      sb.AppendLine(string.Format("|{0}{1} : Products: {2}; Filters : Filter By Parent {3}; Flatten hierarchy {4}", new String('\t', level).ToString(), current.MasterGroupMappingID.ToString(), current.Products.Count, current.FilterByParent, current.FlattenHierarchy));
      foreach (var child in current.Children)
      {
        BuildStringTree(level + 1, child, sb);
      }
    }

    /// <summary>
    /// Adds a product to tree - used only in tests
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="mappingID"></param>
    //public void AddProduct(int productID, int mappingID)
    //{
    //  AddProducts(new List<int> { productID }, mappingID);
    //}

    /// <summary>
    /// Adds a product to the tree - used only in tests
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="mappingID"></param>
    /// <param name="productProductGroupMapping"></param>
    public void AddProduct(int productID, int mappingID, List<int> productProductGroupMapping)
    {
      AddProducts(new List<int> { productID }, mappingID, new Dictionary<int, List<int>>() { { productID, productProductGroupMapping } }, false);
    }

    /// <summary>
    /// Adds a collection of products to a signle level of the tree
    /// </summary>
    /// <param name="productIDs">The product IDs</param>
    /// <param name="mappingID">MappingID</param>
    /// <param name="productProductGroupMapping">Mapping dictionary between product IDs and the mappings in which they belong</param>
    public void AddProducts(List<int> productIDs, int mappingID, Dictionary<int, List<int>> productProductGroupMapping, bool insertProductsToAllGroups = false)
    {
      var nodeToAttachTo = TraverseAndFind(mappingID, Root);

      var nodesWithFlatten = new List<Node>();
      var filterByParentNodes = new List<int>();

      TraverseUpAndBuildFilterInformation(nodeToAttachTo, nodesWithFlatten, filterByParentNodes);

      productIDs = ProductsSatisfyingFilterByParent(productIDs, productProductGroupMapping, filterByParentNodes);

      if (nodesWithFlatten.Count > 0 & !nodeToAttachTo.HasChildren)
      {
        nodesWithFlatten.Last().AddProducts(productIDs);
      }
      else if (!nodeToAttachTo.HasChildren || insertProductsToAllGroups) //if not -> attach if it doesn't have children
      {
        nodeToAttachTo.AddProducts(productIDs);
      }
      else if (nodeToAttachTo.RetainProducts)
      {
        nodeToAttachTo.AddProducts(productIDs);
      }
    }

    private List<int> ProductsSatisfyingFilterByParent(List<int> productIDs, Dictionary<int, List<int>> productProductGroupMapping, List<int> filterByParentNodes)
    {
      return productIDs.Where(c => filterByParentNodes.Except(productProductGroupMapping[c]).Count() == 0).ToList();
    }

    ///// <summary>
    ///// Adds products to the tree. Used only in tests.
    ///// </summary>
    ///// <param name="productIDs"></param>
    ///// <param name="mappingID"></param>
    //[Obsolete]
    //public void AddProducts(List<int> productIDs, int mappingID)
    //{
    //  AddProducts(productIDs, mappingID, new Dictionary<int, List<int>>() { { 1, new List<int> { mappingID } }, }, false); //shortcut incase called without product mapping
    //}

    private void TraverseUpAndBuildFilterInformation(Node current, List<Node> nodesWithFlatten, List<int> nodesWithFilterByParent)
    {
      if (current.FlattenHierarchy) nodesWithFlatten.Add(current);

      if (current.FilterByParent || (current.Parent != null && current.Parent.FilterByParent))
        nodesWithFilterByParent.AddRange(new List<int>() { current.MasterGroupMappingID, current.Parent.MasterGroupMappingID });

      if (current.Parent != Root)
        TraverseUpAndBuildFilterInformation(current.Parent, nodesWithFlatten, nodesWithFilterByParent);
    }


    public List<ContentProductGroupModel> FlattenTreeToList(int connectorIDToUse, int createdByToUse)
    {
      List<ContentProductGroupModel> result = new List<ContentProductGroupModel>();

      TraverseDownAndFillUpList(Root, result, connectorIDToUse, createdByToUse);

      return result;
    }

    private void TraverseDownAndFillUpList(Node current, List<ContentProductGroupModel> result, int connectorIDToUse, int createdByToUse)
    {
      result.AddRange(from r in current.Products
                      select new ContentProductGroupModel
                      {
                        ConnectorID = connectorIDToUse,
                        CreatedBy = createdByToUse,
                        MasterGroupMappingID = current.MasterGroupMappingID,
                        ProductID = r
                      });

      foreach (var child in current.Children)
      {
        TraverseDownAndFillUpList(child, result, connectorIDToUse, createdByToUse);
      }
    }
  }
}
