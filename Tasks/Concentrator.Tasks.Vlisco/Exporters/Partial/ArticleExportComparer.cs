using Concentrator.Tasks.Vlisco.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace Concentrator.Tasks.Vlisco.Exporters.Partial
{
  public class ArticleExportComparer
  {
    private string _serializationPath;
    private IEnumerable<Article> _currentAssortment;
    private TraceSource _traceSource;

    private Dictionary<string, string> _currentAssortmentSerialized;
    private Dictionary<string, string> _previousAssortmentSerialized;

    /// <summary>
    /// Contains the Articles that need to be processed (changed vs last comparison)
    /// </summary>
    public IEnumerable<Article> ArticlesToProcess;

    public ArticleExportComparer(string serializationPath, IEnumerable<Article> currentAssortment, TraceSource traceSource)
    {
      _serializationPath = serializationPath;
      _currentAssortment = currentAssortment;
      _traceSource = traceSource;

      _currentAssortmentSerialized = SerializeCurrentAssortment();
      _previousAssortmentSerialized = LoadLastExportedAssortment();

      CompareCurrentAssortment();
    }

    /// <summary>
    /// Compares the current assortment with the old assortment and sets the Articles to Process property
    /// </summary>
    private void CompareCurrentAssortment()
    {
      //if no previous assortment - do a full run
      if (_previousAssortmentSerialized == null)
      {
        ArticlesToProcess = _currentAssortment;
        return;
      }

      var newProducts = (from p in _currentAssortmentSerialized
                         where !_previousAssortmentSerialized.ContainsKey(p.Key)
                         select int.Parse(p.Key.Substring(0, p.Key.IndexOf("-")))).ToList(); //todo: improve the parsing of the productid

      var changedProducts = (from p in _currentAssortmentSerialized
                             where _previousAssortmentSerialized.ContainsKey(p.Key)
                             && p.Value != _previousAssortmentSerialized[p.Key]
                             select int.Parse(p.Key.Substring(0, p.Key.IndexOf("-")))).ToList(); //todo: improve the parsing of the productid



      ArticlesToProcess = _currentAssortment.Where(c => newProducts.Union(changedProducts).Contains(c.ProductID)).ToList();

    }

    /// <summary>
    /// Load previsously serialized assortment
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> LoadLastExportedAssortment()
    {
      var serializedFiles = Directory.GetFiles(_serializationPath, "*.txt");

      //no files to process
      if (serializedFiles.Length == 0)
        return null;

      Dictionary<string, string> existingAssortment = new Dictionary<string, string>();

      //Load all txt files from the serialization path for comparison
      foreach (var file in Directory.GetFiles(_serializationPath, "*.txt"))
      {
        string key = Path.GetFileNameWithoutExtension(file);

        //If duplicate productID is found (multiple products with the same productID) warn and continue. 
        //Unlikely because same filename
        if (existingAssortment.ContainsKey(key))
        {
          _traceSource.TraceWarning("Multiple serialized files found for key {0}. Resolve manually by leaving one file. Product will be ignored.", key);
          continue;
        }
        existingAssortment.Add(key, File.ReadAllText(file));
      }

      return existingAssortment;
    }

    /// <summary> 
    /// Serialize currently exported assortment to JSON for comparison
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, string> SerializeCurrentAssortment()
    {
      Dictionary<string, string> serializedAssortment = new Dictionary<string, string>();

      foreach (var product in _currentAssortment)
      {
        string key = string.Format("{0}-{1}-{2}", product.ProductID, product.CountryCode, product.CurrencyCode);

        if (serializedAssortment.ContainsKey(key))
        {
          _traceSource.TraceWarning("Multiple products found with ID {0}. Product will be ignored.", product.ProductID);
          continue;
        }

        serializedAssortment.Add(key, product.ToJson());
      }

      return serializedAssortment;
    }

    /// <summary>
    /// Saves the current assortment for comparison in the future runs
    /// </summary>
    public void SaveCurrentAssortment()
    {
      foreach (var assortomentItem in _currentAssortmentSerialized)
      {
        File.WriteAllText(Path.Combine(_serializationPath, string.Format("{0}.txt", assortomentItem.Key)), assortomentItem.Value);
      }
    }
  }
}
