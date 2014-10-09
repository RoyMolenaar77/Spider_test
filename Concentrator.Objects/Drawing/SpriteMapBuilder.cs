using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Web.WebModels;

namespace Concentrator.Objects.Drawing
{
  public class SpriteMapBuilder
  {
    private List<SpriteMapModel> _spriteMapModel;
    private int _margin;

    /// <summary>
    /// Initializes a new instance of the map
    /// </summary>
    /// <param name="spriteMapModel"></param>
    public SpriteMapBuilder(List<SpriteMapModel> spriteMapModel, int margin = 2)
    {
      _spriteMapModel = spriteMapModel;
      _margin = margin;
    }

    /// <summary>
    /// Creates a map for the sprite based on the images and their heights and widths
    /// </summary>
    /// <returns></returns>
    public SpriteMap BuildMap()
    {
      List<SpriteCoordinate> result = new List<SpriteCoordinate>();

      var maxWidth = _spriteMapModel.Max(c => c.Width);
      var maxHeight = _spriteMapModel.Max(c => c.Height);


      var biggestElement = maxHeight >= maxWidth ? _spriteMapModel.FirstOrDefault(c => c.Height == maxHeight) : _spriteMapModel.FirstOrDefault(c => c.Width == maxWidth);
      var xOffset = biggestElement.Width + _margin;

      result.Add(new SpriteCoordinate(biggestElement.Path, biggestElement.Image, biggestElement.Height, biggestElement.Width, 0, 0));
      _spriteMapModel.Remove(biggestElement);

      while (_spriteMapModel.Count > 0)
      {

        decimal batchHeight = 0;
        decimal maxBatchHeight = 70;//maxHeight;
        decimal batchYOffset = 0;
        decimal batchWidth = 0;

        do
        {
          //get first from collection
          var curr = _spriteMapModel.First();
          //result.Add(new SpriteCoordinate(curr.Path, curr.Image, biggestElement.Height, biggestElement.Width, batchYOffset, xOffset));
          result.Add(new SpriteCoordinate(curr.Path, curr.Image, curr.Height, curr.Width, batchYOffset, xOffset));
          batchYOffset += curr.Height + _margin;
          batchHeight += curr.Height + _margin;

          if (curr.Width > batchWidth) batchWidth = curr.Width;
          _spriteMapModel.Remove(curr);

        } while (_spriteMapModel.Count > 0 && (batchHeight + _spriteMapModel.First().Height) < maxBatchHeight);

        xOffset += batchWidth;
      }


      return new SpriteMap(result, xOffset, maxHeight);
    }
  }
}
