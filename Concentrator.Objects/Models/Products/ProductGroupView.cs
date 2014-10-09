using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupView
  {
    public Int32 ProductID { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public Int32 LanguageID { get; set; }
          
    public Int32 ProductGroupID { get; set; }
          
    public Int32 ParentProductGroupID { get; set; }
          
    public Int32 Score { get; set; }
          
    public String ProductGroupName { get; set; }
          
    public String ParentProductGroupName { get; set; }
          
    public Int32 ParentProductGroupScore { get; set; }
          
    public Int32 GrandParentProductGroupID { get; set; }
          
  }
}