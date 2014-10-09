using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.Objects.Web.WebModels
{
  public class ImageRequestModel
  {
    /// <summary>
    /// Can be url or local path
    /// </summary>
    public string ImagePath { get; set; }

    public decimal? Height { get; set; }

    public decimal? Width { get; set; }
  }
}