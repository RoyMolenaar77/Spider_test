#region Usings

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Concentrator.Objects.Environments;
using Dapper;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.EDI.Importers
{
  [Task("EDI Brand Importer")]
  public class BrandImporter : RsoTaskBase
  {
    #region Resourced queries

    [Resource] private static readonly String CreateBrandTable = null;
    [Resource] private static readonly String MergeBrands = null;
    [Resource] private static readonly String MergeBrandVendor = null;

    #endregion

    private DataTable GetPricatBrands()
    {
      return PricatBrands
        .Select(item => new
          {
            item.Code,
            Name = item.Name.ToLower().ToTitle()
          })
        .ToDataTable();
    }

    protected override void ExecutePricatTask()
    {
      EmbeddedResourceHelper.Bind(typeof (BrandImporter));

      using (var connection = new SqlConnection(Environments.Current.Connection))
      {
        connection.Open();
        connection.Execute(CreateBrandTable);

        using (var brandDataTable = GetPricatBrands())
        using (var bulkCopy = new SqlBulkCopy(connection))
        {
          bulkCopy.DestinationTableName = "#Brand";
          bulkCopy.WriteToServer(brandDataTable);
        }

        using (var transaction = connection.BeginTransaction())
        {
          try
          {
            var importCount = connection.Execute(MergeBrands, null, transaction);

            connection.Execute(string.Format(MergeBrandVendor, DefaultVendor.VendorID), null, transaction);

            transaction.Commit();

            TraceVerbose("{0} brand(s) created or updated.", importCount);
          }
          catch (Exception exception)
          {
            transaction.Rollback();

            TraceError("Unable to import the brands. {0}", exception.Message);
          }
        }
      }
    }
  }
}