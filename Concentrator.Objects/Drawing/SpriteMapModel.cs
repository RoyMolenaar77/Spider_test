using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Concentrator.Objects.Drawing
{
  public class SpriteMapModel
  {
    public decimal Height { get; set; }

    public decimal Width { get; set; }

    public string Path { get; set; }

    public Image Image { get; set; }
  }
}
