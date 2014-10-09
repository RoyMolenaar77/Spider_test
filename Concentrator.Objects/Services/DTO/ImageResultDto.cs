using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Concentrator.Objects.Services.DTO.Base;
using System.Drawing;
using System.Drawing.Imaging;

namespace Concentrator.Objects.Services.DTO
{
  public class ImageResultDto : DtoBase
  {
    /// <summary>
    /// The actual image. Null if not found
    /// </summary>
    public Bitmap Image { get; set; }

    public string Extension { get; set; }

    public ImageFormat Format { get; set; }
  }
}
