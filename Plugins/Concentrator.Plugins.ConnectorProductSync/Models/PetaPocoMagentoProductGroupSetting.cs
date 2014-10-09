using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Models
{
  public class PetaPocoMagentoProductGroupSetting
  {
    public Int32 MagentoProductGroupSettingID { get; set; } //todo: remove this

    public Int32 ProductGroupmappingID { get; set; }

    public Boolean? ShowInMenu { get; set; }

    public Boolean? DisabledMenu { get; set; }

    public Boolean? IsAnchor { get; set; }

    public Int32? MasterGroupMappingID { get; set; }

    public Int32 CreatedBy { get; set; }
  }
}
