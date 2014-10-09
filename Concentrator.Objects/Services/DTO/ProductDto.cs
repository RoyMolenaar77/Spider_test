using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.DTO.Base;

namespace Concentrator.Objects.Services.DTO
{
  public class ProductDto : DtoBase
  {
    public string Name { get; set; }

    public int ProductID { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Expected { get; set; }

    public string Hierarchy { get; set; }

    public BrandDTO Brand { get; set; }

  }
}
