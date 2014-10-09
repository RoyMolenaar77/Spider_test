using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{


  public class AssortmentProductGroup
  {
    public string Name { get; set; }

    public int Index { get; set; }

    public string Image { get; set; }

    public string ThumbnailImage { get; set; }

    public string MenuIconImage { get; set; }

    public string Description { get; set; }

    public string CustomName { get; set; }

    public string AttributeSet { get; set; }

    public int MappingID { get; set; }

    /// <summary>
    /// The ID of the 
    /// </summary>
    public int ID { get; set; }

    public AssortmentMagentoSetting MagentoSetting { get; set; }

    public AssortmentProductGroup ParentProductGroup { get; set; }

    public List<AssortmentSeoTexts> AssortmentSeoTexts{ get; set; }

    public List<ProductGroupSetting> ProductGroupSettings { get; set; }

  }
}
