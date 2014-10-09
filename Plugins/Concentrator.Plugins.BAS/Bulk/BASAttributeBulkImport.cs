using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors.Base;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Data.SqlClient;

namespace Concentrator.Plugins.BAS
{
  public class BASAttributeBulkImport : VendorImportLoader<ConcentratorDataContext>
  {
    const string _attributesTable = "[dbo].[BAS_temp_attribute]";

    private string _mergeTabelQuery = @"CREATE TABLE {0}(
	[CustomItemNumber] [nvarchar](40) NOT NULL,
	[Risk] [nvarchar](10) NOT NULL,
  [AttributeID] [int] NOT NULL,
	[ProductID] [int] NULL)";

    private IEnumerable<ConnectFlowAdditionalInformation> _insuranceList;

    public BASAttributeBulkImport(IEnumerable<ConnectFlowAdditionalInformation> insuranceList)
    {
      _mergeTabelQuery = string.Format(_mergeTabelQuery, _attributesTable);
      _insuranceList = insuranceList;
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        context.ExecuteStoreCommand(_mergeTabelQuery);

        using (GenericCollectionReader<ConnectFlowAdditionalInformation> reader = new GenericCollectionReader<ConnectFlowAdditionalInformation>(_insuranceList))
        {
          BulkLoad(_attributesTable, 1000, reader);
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error execture bulk copy");
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      Log.DebugFormat("Start merge attributes");

      string BASattributeMerge = @"merge productattributevalue pav 
using(
select distinct va.productid, bpa.attributeid, bpa.risk, 2 as languageID
from [dbo].[BAS_temp_attribute] bpa
inner join vendorassortment va on bpa.customitemnumber = va.customitemnumber
inner join vendor v on v.vendorid = va.vendorid
where v.vendorid = 1 or v.parentvendorid = 1
) att on att.attributeid = pav.attributeid and att.productid = pav.productID and att.languageid = pav.languageid
when matched then update set 
	pav.value = att.risk
	WHEN NOT Matched by target
THEN
INSERT (AttributeID,
ProductID,
Value,
LanguageID,
CreatedBy,
CreationTime)
values (att.attributeID,
att.ProductID,
att.Risk,
att.LanguageID,
1,
GetDate());";
      context.ExecuteStoreCommand(BASattributeMerge);

      Log.DebugFormat("Finish merge attributes");
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _attributesTable));
    }
  }

  public class ConnectFlowAdditionalInformation
  {
    public int CustomItemNumber { get; set; }
    public int Risk { get; set; }
    public int AttributeID { get; set; }
  }
}
