using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Concentrator.Objects.Services.DTO;
using System.Drawing.Imaging;

namespace Concentrator.Objects.Drawing
{
  public class Sprite
  {
    private SpriteMap _map;

    public Sprite(SpriteMap map)
    {
      _map = map;

    }

    public Image GetSpriteImage()
    {
      Bitmap canvas = new Bitmap((int)_map.CanvasWidth, (int)_map.CanvasHeight);

      using (Graphics g = Graphics.FromImage(canvas))
      {
        g.Clear(Color.Transparent);

        _map.Coordinates.ForEach((crd) =>
        {
          if (crd.Image != null)
            g.DrawImage(crd.Image, (int)crd.X + (int)((float)crd.Width - (float)crd.Image.Width) / 2f, (int)crd.Y + (int)((float)crd.Height - (float)crd.Image.Height) / 2f);
        });
      }
      return canvas;
    }
  }
}
