using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Concentrator.Objects.Drawing
{
  public class SpriteCoordinate
  {
    private string _identifier;

    private Image _image;

    private readonly decimal x;

    private readonly decimal y;

    private decimal _height;

    public decimal Height
    {
      get { return _height; }

    }

    public bool Loaded = false;

    private decimal _width;

    public decimal Width
    {
      get { return _width; }

    }

    public SpriteCoordinate(string id, Image image, decimal height, decimal width, decimal y = 0, decimal x = 0)
    {
      _identifier = id;
      this.x = x;
      this.y = y;
      _image = image;
      _height = height;
      _width = width;
    }

    /// <summary>
    /// Identifier of the image
    /// </summary>
    public string Path { get { return _identifier; } }

    /// <summary>
    /// X offset
    /// </summary>
    public decimal X { get { return x; } }

    /// <summary>
    /// Y offset
    /// </summary>
    public decimal Y { get { return y; } }

    /// <summary>
    /// The actual image
    /// </summary>
    public Image Image
    {
      get { return _image; }
      set { _image = value; }
    }
  }
}
