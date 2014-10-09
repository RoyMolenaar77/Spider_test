using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Drawing
{
  public class SpriteMap
  {
    private List<SpriteCoordinate> _coordinates;

    public List<SpriteCoordinate> Coordinates
    {
      get { return _coordinates; }
    }
    private decimal _canvasWidth;

    public decimal CanvasWidth
    {
      get { return _canvasWidth; }
    }
    private decimal _canvasHeight;

    public decimal CanvasHeight
    {
      get { return _canvasHeight; }
    }

    public SpriteMap(List<SpriteCoordinate> coordinates, decimal canvasWidth, decimal canvasHeight)
    {
      _coordinates = coordinates;
      _canvasWidth = canvasWidth;
      _canvasHeight = canvasHeight;
    }
  }
}
