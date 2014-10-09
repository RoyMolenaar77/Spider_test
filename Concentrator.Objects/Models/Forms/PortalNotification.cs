using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Forms
{
  public class PortalNotification
  {
    public int FormID { get; set; }

    public string Name { get; set; }

    public string Priority { get; set; }

    public string ArticleNumber { get; set; }

    public string ProductName { get; set; }

    public int NotificationType { get; set; }

    public string Description { get; set; }

    public DateTime CreationTime { get; set; }

    public bool IsResolved { get; set; }
  }
}
