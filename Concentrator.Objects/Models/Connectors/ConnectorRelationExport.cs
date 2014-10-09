using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.Models.EDI.Response;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorRelationExport
  {
    public Int32 ConnectorRelationExportID { get; set; }

    public Int32 ConnectorRelationID { get; set; }

    public String SourcePath { get; set; }

    public String DestinationPath { get; set; }

    public ConnectorRelation ConnectorRelation { get; set; }
  }
}