using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using Concentrator.Configuration;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Concentrator.Objects.Lucene
{
  public class LuceneIndexer : IDisposable
  {
    public Analyzer Analyzer
    {
      get;
      set;
    }

    public Directory Directory
    {
      get;
      set;
    }

    private IndexWriter Writer
    {
      get;
      set;
    }

    public LuceneIndexer(String path = null)
    {
      if (path == null)
      {
        path = ConfigurationManager.AppSettings["SearchIndexFilesPath"];
      }

      Analyzer = new StandardAnalyzer(Version.LUCENE_29);

      var searchFileDirectoryInfo = path == null
        ? new DirectoryInfo(ConcentratorSection.Default.Searching.Directory)
        : new DirectoryInfo(path);

      if (!searchFileDirectoryInfo.Exists)
      {
        searchFileDirectoryInfo.Create();
      }

      Directory = FSDirectory.Open(searchFileDirectoryInfo);
    }

    public void Dispose()
    {
      if (Writer != null)
      {
        Writer.Optimize();
        Writer.Close();
        Writer = null;
      }
    }

    public IndexWriter GetIndexWriter(bool create)
    {
      return Writer ?? (Writer = new IndexWriter(Directory, Analyzer, create, IndexWriter.MaxFieldLength.UNLIMITED));
    }

    public IEnumerable<SearchIndexModel> GetSearchResultsFromIndex(string searchquery, int? numberOfResults = null)
    {
      searchquery = searchquery.IfNullOrEmpty("s").Trim();

      var indexReader = IndexReader.Open(Directory, true);
      var indexSearch = new IndexSearcher(indexReader);
      var queryParser = new QueryParser(Version.LUCENE_29, "searchText", Analyzer);
      var query = queryParser.Parse(searchquery + "*");
      var resultDocs = indexSearch.Search(query, numberOfResults ?? indexReader.MaxDoc());
      var hits = resultDocs.scoreDocs.ToList();

      return hits.Select(hit => indexSearch.Doc(hit.doc)).Select(documentFromSearch => new SearchIndexModel
      {
        ProductID = Int32.Parse(documentFromSearch.Get("ID")),
        ProductName = documentFromSearch.Get("productName"),
        VendorItemNumber = documentFromSearch.Get("vendorItemNumber")
      });
    }

    public void CloseIndexWriter()
    {
      try
      {
        Writer.Optimize();
        Writer.Close();

        var filesToDelete = System.IO.Directory.GetFiles(ConfigurationManager.AppSettings["SearchIndexFilesPath"]);

        foreach (var fileToDelete in filesToDelete)
        {
          if (!fileToDelete.EndsWith(".lock"))
            continue;

          var numberOfAttempts = 0;
          while (IsFileLocked(new System.IO.FileInfo(fileToDelete)) && numberOfAttempts < 20)
          {
            numberOfAttempts++;
            Thread.Sleep(2000);
          }

          if (!IsFileLocked(new System.IO.FileInfo(fileToDelete)))
          {
            File.Delete(fileToDelete);
          }
        }

      }
      catch (Exception e)
      {
        writeException(e);
      }
    }

    private void writeException(Exception e, string additionalMessage = "")
    {
      string fullPath = ConfigurationManager.AppSettings["LuceneErrorLogFile"];

      if (!string.IsNullOrEmpty(fullPath))
      {
        if (!System.IO.File.Exists(fullPath))
        {
          using (System.IO.File.CreateText(fullPath)) { }
        }

        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fullPath, true))
        {
          string message = string.Format("Date: {0} --- Error: {1} --- Stack trace: {2} Addition message: {3}", DateTime.Now.ToString(), e.Message, e.StackTrace, additionalMessage);
          sw.WriteLine(message);
        }
      }
    }

    private static bool IsFileLocked(FileInfo file)
    {
      FileStream stream = null;

      try
      {
        stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
      }
      catch (IOException)
      {
        return true; //If it throws an IOException when trying to open the file the file is locked.
      }
      finally
      {
        if (stream != null)
          stream.Close();
      }
      return false;
    }
  }
}