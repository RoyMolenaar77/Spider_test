using System.Configuration;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.ConnectorProductSync.Config;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync
{
 /// <summary>
  /// 
  /// To add this plugin execute:
  /// 
  /// INSERT INTO Plugin
  /// (PluginName, PluginType, PluginGroup, CronExpression, IsActive, ExecuteOnStartup)
  /// VALUES
  /// ('Synchronize CrossReferenced RelatedProducts','Concentrator.Plugins.ConnectorProductSync.SynchronizeCrossReferencedRelatedProducts','Internal','0 0/5 * * * ?', 1, 0)
  /// 
  /// </summary>
  public class SynchronizeCrossReferencedRelatedProducts : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Cross reference product relations"; }
    }
    const string Query = "EXEC sp_SynchronizeCrossReferencedRelatedProducts @@CreateRelatedProductsWithVendorID = @0, @@CreatedByUserID = @1";

    protected override void Process()
    {
      var config = (ProcessCrossReferenceProductRelationsConfigSection)GetConfiguration().GetSection("ProcessCrossReferenceProductRelations");
      var createRelatedProductsWithVendorID = config.CreateRelatedProductsWithVendorID;
      var createRelatedProductsWithUserID = config.CreateRelatedProductsWithUserID;

      using (var db = new Database(Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 70;

        db.Execute(
          Query, 
          createRelatedProductsWithVendorID, 
          createRelatedProductsWithUserID
          );
      }
    }
  }
}