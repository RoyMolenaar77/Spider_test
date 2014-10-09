using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentBrand
  {
    public int BrandID { get; set; }

    public int ParentBrandID { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public AssortmentBrand ParentBrand { get; set; }
  }
}
