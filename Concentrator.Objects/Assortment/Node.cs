using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Assortment
{
  public class Node
  {
    public Node Parent { get; set; }

    public List<Node> Children { get; set; }

    public List<int> Products { get; private set; }

    public void AddProduct(int id)
    {
      AddProducts(new List<int> { id });
    }

    public void AddProducts(List<int> ids)
    {
      Products.AddRange(ids.Except(Products));
    }

    public int MasterGroupMappingID { get; private set; }

    public bool FilterByParent { get; private set; }

    public bool FlattenHierarchy { get; private set; }

    public bool RetainProducts { get; private set; }

    public Node(Node parent, int masterGroupMappingID, bool filterByParent = false, bool flattenHierarchy = false, bool retainProducts = false)
    {
      Children = new List<Node>();
      Products = new List<int>();

      Parent = parent;
      MasterGroupMappingID = masterGroupMappingID;
      FilterByParent = filterByParent;
      FlattenHierarchy = flattenHierarchy;
      RetainProducts = retainProducts;
    }

    public bool HasChildren { get { return Children.Count > 0; } }
  }
}
