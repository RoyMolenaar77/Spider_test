using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.DTO.Base;

namespace Concentrator.Objects.Services.DTO
{
  public class BrandDTO : DtoBase
  {
    public string Name { get; set; }

    public int BrandID { get; set; }
  }
}
