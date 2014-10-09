using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.DTO.Base;

namespace Concentrator.Objects.Services.DTO
{
  public class ProductGroupMappingDto : DtoBase
  {
    public int ProductGroupMappingID { get; set; }
  
    /// <summary>
    /// Name of level. From product group
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Child levels
    /// </summary>
    public List<ProductGroupMappingDto> Children { get; set; }

    /// <summary>
    /// All products withing this level
    /// </summary>
    public List<ProductDto> Products { get; set; }

    /// <summary>
    /// Indicates whether this product group mapping has children. 
    /// They might not be currently loaded
    /// </summary>
    public bool HasChildren { get; set; }

    public string Lineage { get; set; }

    public int Depth { get; set; }
  }
}
