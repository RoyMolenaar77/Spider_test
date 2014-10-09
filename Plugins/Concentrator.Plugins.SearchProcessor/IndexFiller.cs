using System;
using System.Collections.Generic;
using Concentrator.Objects.ConcentratorService;
using Lucene.Net.Documents;

using PetaPoco;

namespace Concentrator.Plugins.SearchProcessor
{
  using Objects.Lucene;

  public class IndexFiller : ConcentratorPlugin
  {
    LuceneIndexer indexer;

    public override string Name
    {
      get
      {
        return "Concentrator Search Index maker Plugin";
      }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      indexer = new LuceneIndexer();
      IndexContent();
    }


    private void IndexContent()
    {
      log.AuditInfo("Start index search plugin");

      var writer = indexer.GetIndexWriter(true);

      using (var database = new Database(Connection, Database.MsSqlClientProvider)
      {
        CommandTimeout = 122333
      })
      {
        var content = database.Fetch<dynamic>(@"
          SELECT DISTINCT
            SC.ProductID, 
            ISNULL(SC.ProductName, SC.VendorItemNumber) AS ProductName, 
            SC.VendorItemNumber, 
            ISNULL(SC.CustomItemNumber, SC.vendoritemnumber) AS CustomItemNumber,
            ISNULL(SC.SearchText, SC.VendorItemNumber) AS SearchText
          FROM [dbo].[SearchContent] AS SC 
          INNER JOIN [dbo].[Product] AS P ON P.ProductID = SC.ProductID
          WHERE P.IsConfigurable = 1
          ORDER BY SC.VendorItemNumber");

        try
        {
          foreach (var c in content)
          {
            var doc = new Document();

            doc.Add(new Field("ID", c.ProductID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("productName", c.ProductName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("vendorItemNumber", c.VendorItemNumber, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("customItemNumber", c.CustomItemNumber, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("searchText", c.SearchText, Field.Store.YES, Field.Index.ANALYZED));

            writer.AddDocument(doc);
          }
        }
        catch (Exception e)
        {
          log.Debug(e.InnerException == null ? e.Message : e.InnerException.Message);
        }
        finally
        {
          indexer.CloseIndexWriter(); // removes the write.lock file
        }
      }

      log.AuditSuccess("Finished search index plugin");
    }
  }
}