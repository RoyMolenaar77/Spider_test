using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentProductGroupMapping
  {
    public int? ContentProductGroupID;
    public int ProductGroupMappingID;
    public int? ParentProductGroupMappingID;
    public int? ProductID;
    public string ProductName;
    public int ProductGroupID;
    public string ProductGroupName;
    public int? ConnectorID;
    public bool FlattenHierarchy;
    public bool FilterByParentGroup;
    public int? Score;
  }
}
