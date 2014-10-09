using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Models
{
  public class PetaPocoMasterGroupMappingModel
  {
    public Int32 MasterGroupMappingID { get; set; }

    public Int32? ParentMasterGroupMappingID { get; set; }

    public Int32 ProductGroupID { get; set; }

    public Int32? Score { get; set; }

    public Int32? ConnectorID { get; set; }

    public Int32? SourceMasterGroupMappingID { get; set; }

    public Int32? SourceProductGroupMappingID { get; set; }

    public Int32? ExportID { get; set; }

    public Boolean FlattenHierarchy { get; set; }

    public Boolean FilterByParentGroup { get; set; }
  }
}
