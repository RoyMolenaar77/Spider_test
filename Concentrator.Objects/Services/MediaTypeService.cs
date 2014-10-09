using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Services.Base;

namespace Concentrator.Objects.Services
{
  public class MediaTypeService : Service<MediaType>
  {

    public override IQueryable<MediaType> Search(string queryTerm)
    {
      var query = queryTerm.IfNullOrEmpty("").ToLower();
      return Repository().GetAllAsQueryable(c => c.Type.ToLower().Contains(query));
    }

  }
}
