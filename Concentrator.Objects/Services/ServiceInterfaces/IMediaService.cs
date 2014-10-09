using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.DTO;
using System.Drawing;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IMediaService
  {
    /// <summary>
    /// Retrieves an image for a product
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="height">If specified the image will be resized</param>
    /// <param name="width">If specified the image will be resized</param>
    /// <param name="sequence">Sequence of the image. Defaults to 0</param>
    /// <param name="connectorID">If not supplied, the current user's will be used</param>
    /// <param name="defaultImagePath">If supplied, this image will be used when an image is not found</param>
    /// <returns></returns>
    ImageResultDto GetImageForProduct(int productID, decimal? height = null, decimal? width = null, int sequence = 0, int? connectorID = null, string defaultImagePath = "");

    string GetImagePath(int productID, int? connectorID = null, int sequence = 0);

    ImageView[] GetImagePaths(int productID, int connectorID);

    Dictionary<int, ImageView[]> GetImagePathsCombined(int[] productID, int connectorID);
  }
}
