using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.DataAccess.Repository
{
  public class PagedResult<TResult>
  {
    public const int NoLimit = -1;

    /// <summary>
    /// Number of objects skipped from source
    /// </summary>
    public int Offset { get; private set; }

    /// <summary>
    /// Maximum number of objects returned in Page
    /// </summary>
    public int Limit { get; private set; }

    /// <summary>
    /// Total number of objects in source
    /// </summary>
    public int Total { get; private set; }

    /// <summary>
    /// Found objects in source
    /// </summary>
    public IEnumerable<TResult> Page { get; private set; }

    public PagedResult(IEnumerable<TResult> page, int total, int offset = 0, int limit = NoLimit)
    {
      if (limit != NoLimit)
        page = page.Take(limit);
      
      Page = page;
      Total = total;
      Limit = limit;
      Offset = offset;
    }
  }
}
