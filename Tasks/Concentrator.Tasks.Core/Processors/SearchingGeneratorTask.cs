using System;
using System.Collections.Generic;
using System.Linq;

using Lucene.Net.Documents;

namespace Concentrator.Tasks.Core.Processors
{
  using Objects.Lucene;
  using Objects.Sql;

  [Task("Concentrator Searching Generator")]
  public sealed class SearchingGeneratorTask : TaskBase
  {
    private class SearchContentModel
    {
      public String CustomItemNumber
      {
        get;
        set;
      }

      public String ProductID
      {
        get;
        set;
      }

      public String ProductName
      {
        get;
        set;
      }

      public String SearchText
      {
        get;
        set;
      }

      public String VendorItemNumber
      {
        get;
        set;
      }
    }

    protected override void ExecuteTask()
    {
      using (var indexer = new LuceneIndexer())
      {
        TraceVerbose("Generating product search information...");

        Database.Execute("EXEC [dbo].[GenerateSearchResults]");

        var query = new QueryBuilder()
          .From("[dbo].[SearchContent] AS [SC]")
          .Join(JoinType.Inner, "[dbo].[Product] AS [P]", "[P].[ProductID] = [SC].[ProductID]")
          .Select("[SC].[ProductID]", "[SC].[VendorItemNumber]")
          .Column("ISNULL([SC].[ProductName], [SC].[VendorItemNumber]) AS [ProductName]")
          .Column("ISNULL([SC].[CustomItemNumber], [SC].[VendorItemNumber]) AS [CustomItemNumber]")
          .Column("ISNULL([SC].[SearchText], [SC].[VendorItemNumber]) AS [SearchText]")
          .OrderBy("[SC].[VendorItemNumber]");

        var searchContent = Database.Query<SearchContentModel>(query).ToArray();

        TraceVerbose("{0} product search records found.", searchContent.Length);

        var writer = indexer.GetIndexWriter(true);

        foreach (var searchContentModel in searchContent)
        {
          var document = new Document();

          document.Add(new Field("ID", searchContentModel.ProductID, Field.Store.YES, Field.Index.NOT_ANALYZED));
          document.Add(new Field("productName", searchContentModel.ProductName, Field.Store.YES, Field.Index.NOT_ANALYZED));
          document.Add(new Field("vendorItemNumber", searchContentModel.VendorItemNumber, Field.Store.YES, Field.Index.NOT_ANALYZED));
          document.Add(new Field("customItemNumber", searchContentModel.CustomItemNumber, Field.Store.YES, Field.Index.NOT_ANALYZED));
          document.Add(new Field("searchText", searchContentModel.SearchText, Field.Store.YES, Field.Index.ANALYZED));

          writer.AddDocument(document);
        }

        writer.Commit();
      }
    }
  }
}
