using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Tasks.Core.Processors
{
  using Objects;
  using Objects.Models;
  using Objects.Models.Connectors;
  using Objects.Models.Contents;
  using Objects.Models.MastergroupMapping;
  using Objects.Models.Products;
  using Objects.Models.Vendors;
  using Objects.Sql;

  [Task("Assortment Generator")]
  public class AssortmentGeneratorTask : TaskBase
  {
    private class ContentComparer : IEqualityComparer<Content>
    {
      public readonly static ContentComparer Default = new ContentComparer(false);
      public readonly static ContentComparer OnlyProductID = new ContentComparer(true);

      private readonly Boolean _compareOnlyProductID;

      private ContentComparer(Boolean compareOnlyProductID)
      {
        _compareOnlyProductID = compareOnlyProductID;
      }

      public Boolean Equals(Content left, Content right)
      {
        return !_compareOnlyProductID 
          ? left.ConnectorID == right.ConnectorID && left.ProductID == right.ProductID
          : left.ProductID == right.ProductID;
      }

      public Int32 GetHashCode(Content content)
      {
        return content != null
          ? !_compareOnlyProductID
            ? content.ConnectorID ^ content.ProductID
            : content.ProductID
          : 0;
      }
    }

    private Connector[] Connectors
    {
      get;
      set;
    }

    public AssortmentGeneratorTask()
    {
      EmbeddedResourceHelper.Bind(this);
    }

    protected override void ExecuteTask()
    {
      Connectors = Database
        .Query<Connector>(@"SELECT [ConnectorID], [Name] FROM [dbo].[Connector] WHERE [IsActive] = 1 AND (([ConnectorType] & 2 = 2) or ([ConnectorType] & 4 = 4))")
        .ToArray();

      TraceInformation("{0} Assortment Connectors found.", Connectors.Length);

      var options = new ParallelOptions
      {
#if DEBUG
        MaxDegreeOfParallelism = 1
#endif
      };

      Parallel.ForEach(Connectors, options, ProcessConnector);
    }

    private void ProcessConnector(Connector connector)
    {
      var traceSource = TraceSource.Clone(String.Join(" - ", Name, connector.Name));

      GenerateConnectorProducts(connector, traceSource);
    }

    private IEnumerable<Content> GetConnectorPublicationRuleProducts(ConnectorPublicationRule connectorPublicationRule)
    {
      var queryBuilder = new QueryBuilder()
        .From("[dbo].[VendorAssortment] AS [VA]")
        .Where("[VendorID] = @VendorID");

      if (connectorPublicationRule.ProductID.HasValue)
      {
        queryBuilder.Where("[VA].[ProductID] = @ProductID");
      }

      if (connectorPublicationRule.BrandID.HasValue)
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[Product] AS [P]", "[VA].[ProductID] = [P].[ProductID]")
          .Where("[P].[BrandID] = @BrandID");
      }

      if (connectorPublicationRule.MasterGroupMappingID.HasValue && connectorPublicationRule.MasterGroupMappingID.Value > 0)
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[MasterGroupMappingProduct] AS [MGMP]", "[MGMP].[MasterGroupMappingID] = @MasterGroupMappingID AND [MGMP].[ProductID] = [VA].[ProductID]")
          .Where("MGMP.[IsProductMapped] = 1");

        if (connectorPublicationRule.OnlyApprovedProducts)
        {
          queryBuilder
            .Join(JoinType.Inner, "[dbo].[MasterGroupMapping] AS [MGM]", "[MGMP].[MasterGroupMappingID] = [MGM].[MasterGroupMappingID]")
            .Where("[MGMP].[IsApproved] = 1")
            .Where("[MGM].[ConnectorID] IS NULL");
        }
      }
      
      if (connectorPublicationRule.PublishOnlyStock.GetValueOrDefault())
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[VendorStock] AS [VS]", "[VA].[ProductID] = VS.[ProductID] AND [VA].[VendorID] = [VS].[VendorID]")
          .Where("[VS].[QuantityOnHand] > 0");
      }

      if (connectorPublicationRule.StatusID.HasValue || connectorPublicationRule.FromPrice.HasValue || connectorPublicationRule.ToPrice.HasValue)
      {
        queryBuilder.Join(JoinType.Inner, "[dbo].[VendorPrice] AS [VP]", "[VA].[VendorAssortmentID] = [VP].[VendorAssortmentID]");

        if (connectorPublicationRule.StatusID.HasValue)
        {
          queryBuilder.Where("[VP].[ConcentratorStatusID] = @StatusID");
        }

        if (connectorPublicationRule.FromPrice.HasValue)
        {
          queryBuilder.Where("[VP].[Price] > @FromPrice");
        }

        if (connectorPublicationRule.FromPrice.HasValue)
        {
          queryBuilder.Where("[VP].[Price] > @ToPrice");
        }
      }

      if (connectorPublicationRule.AttributeID.HasValue || !String.IsNullOrEmpty(connectorPublicationRule.AttributeValue))
      {
        queryBuilder
          .Join(JoinType.Inner, "[dbo].[ProductAttributeValue] AS [PAV]", "[VA].[ProductID] = [PAV].[ProductID] AND [VA].[VendorID] = [PAV].[VendorID]")
          .Where("[PAV].[AttributeID] = @AttributeID")
          .Where("[PAV].[Value] = @AttributeValue");
      }

      var query = queryBuilder.Select("@ConnectorID AS [ConnectorID]", "@ConnectorPublicationRuleID AS [ConnectorPublicationRuleID]", "[VA].[ProductID]");

      return Database.Query<Content>(query, connectorPublicationRule);
    }

    private void GenerateConnectorProducts(Connector connector, TraceSource traceSource)
    {
      TraceInformation("Generating assortment for Connector '{0}'...", connector.Name);

      var query = new QueryBuilder()
        .From("[dbo].[ConnectorPublicationRule]")
        .Where("[ConnectorID] = @ConnectorID")
        .Where("[IsActive] = 1")
        .Select("*");

      var connectorPublicationRules = Database
        .Query<ConnectorPublicationRule>(query, connector)
        .Where(connectorPublicationRule => !connectorPublicationRule.FromDate.HasValue || connectorPublicationRule.FromDate < DateTime.UtcNow)
        .Where(connectorPublicationRule => !connectorPublicationRule.ToDate.HasValue || connectorPublicationRule.ToDate > DateTime.UtcNow)
        .OrderByDescending(connectorPublicationRule => connectorPublicationRule.PublicationIndex)
        .ToArray();

      var connectorAssortment = new Content[0];

      foreach (var connectorPublicationRule in connectorPublicationRules)
      {
        var result = GetConnectorPublicationRuleProducts(connectorPublicationRule)
          .Distinct(ContentComparer.Default)
          .ToArray();

        if (result.Any())
        {
          var currentSize = connectorAssortment.Length;

          switch ((ConnectorPublicationRuleType)connectorPublicationRule.PublicationType)
          {
            case ConnectorPublicationRuleType.Exclude:
              connectorAssortment = connectorAssortment
                .Except(result, ContentComparer.Default)
                .ToArray();

              traceSource.TraceVerbose("Publication Rule '{0}' ({1}) removed {2} assortment items."
                , connectorPublicationRule.ConnectorPublicationRuleID
                , connectorPublicationRule.PublicationIndex
                , currentSize - connectorAssortment.Count());
              break;

            case ConnectorPublicationRuleType.Include:
              connectorAssortment = connectorAssortment
                .Concat(result)
                .Distinct(ContentComparer.Default)
                .ToArray();

              traceSource.TraceVerbose("Publication Rule '{0}' ({1}) added {2} assortment items."
                , connectorPublicationRule.ConnectorPublicationRuleID
                , connectorPublicationRule.PublicationIndex
                , connectorAssortment.Count() - currentSize);
              break;
          }

        }
      }

      SynchronizeContentProducts(connector, connectorAssortment, traceSource);
    }

    [Resource]
    private static readonly String CreateContentTable = null;

    [Resource]
    private static readonly String MergeContentTable = null;

    private void SynchronizeContentProducts(Connector connector, IEnumerable<Content> assortment, TraceSource traceSource)
    {
      var temporaryTableName = "Content-" + connector.ConnectorID;

      Database.Execute(String.Format(CreateContentTable, temporaryTableName));

      using (var connection = new SqlConnection(Environments.Current.Connection))
      {
        connection.Open();

        using (var bulkCopy = new SqlBulkCopy(connection)
        {
          BatchSize = 10000,
          BulkCopyTimeout = 600,
          DestinationTableName = String.Format("[tmp].[{0}]", temporaryTableName),
          NotifyAfter = 10000
        })
        using (var contentTable = assortment
          .Select(content => new
          {
            content.ConnectorPublicationRuleID,
            content.ConnectorID,
            content.ProductID,
            content.ShortDescription,
            content.LongDescription
          }).ToDataTable())
        {
          bulkCopy.SqlRowsCopied += (sender, arguments) => traceSource.TraceVerbose("{0} Records inserted ", arguments.RowsCopied);
          bulkCopy.WriteToServer(contentTable);
        }
      }

      Database.Execute(String.Format(MergeContentTable, temporaryTableName), connector);
    }
  }
}
