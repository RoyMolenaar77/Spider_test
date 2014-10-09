using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ServiceStack.Text;

namespace Concentrator.Tasks
{
  public class DifferentialProvider
  {
    private const Int32 ParallelTurnoverCount = 128;
    private const String StorageFileExtension = ".diff";

    public static DifferentialProvider Default
    {
      get;
      private set;
    }

    static DifferentialProvider()
    {
      Default = new DifferentialProvider("Cache");
    }

    public DirectoryInfo StorageDirectory
    {
      get;
      private set;
    }

    private ConcurrentDictionary<Type, DifferentialStorage> Storages
    {
      get;
      set;
    }

    public DifferentialProvider(String storageDirectory)
    {
      Storages = new ConcurrentDictionary<Type, DifferentialStorage>();
      StorageDirectory = new DirectoryInfo(storageDirectory);
    }

    public IEnumerable<TModel> GetDifferential<TModel>(IEnumerable<TModel> currentCollection, IEqualityComparer<TModel> comparer = null)
    {
      if (currentCollection == null)
      {
        throw new ArgumentNullException("currentCollection");
      }

      if (comparer == null)
      {
        comparer = EqualityComparer<TModel>.Default;
      }

      var storage = Storages.GetOrAdd(typeof(TModel), GetStorageForType);
      var previousLines = storage.ToArray();
      var previousLookup = default(Dictionary<TModel, String>);
      
      try
      {
        previousLookup = previousLines.Length > ParallelTurnoverCount
          ? previousLines.AsParallel().ToDictionary(value => JsonSerializer.DeserializeFromString<TModel>(value), comparer)
          : previousLines.ToDictionary(value => JsonSerializer.DeserializeFromString<TModel>(value), comparer);
      }
      catch (ArgumentException)
      {
        previousLookup = new Dictionary<TModel, String>(comparer);
      }

      var currentLookup = new Dictionary<TModel, String>(previousLookup, comparer);

      foreach (var current in GetDifferentialInternally(currentCollection, previousLookup, comparer))
      {
        currentLookup[current] = JsonSerializer.SerializeToString(current);

        yield return current;
      }
      
      using (var streamWriter = storage.Open())
      {
        foreach (var serialization in currentLookup.Values)
        {
          streamWriter.WriteLine(serialization);
        }
      }
    }

    /// <summary>
    /// Returns all the items in the currentCollection that are either not previousCollection or are different.
    /// </summary>
    /// <param name="currentCollection">
    /// </param>
    /// <param name="previousCollection">
    /// </param>
    /// <param name="comparer">
    /// </param>
    public IEnumerable<TModel> GetDifferential<TModel>(IEnumerable<TModel> currentCollection, IEnumerable<TModel> previousCollection, IEqualityComparer<TModel> comparer = null)
    {
      if (currentCollection == null)
      {
        throw new ArgumentNullException("currentCollection");
      }

      if (previousCollection == null)
      {
        throw new ArgumentNullException("previousCollection");
      }

      if (comparer == null)
      {
        comparer = EqualityComparer<TModel>.Default;
      }

      var previousLookup = previousCollection.ToDictionary(previous => previous, JsonSerializer.SerializeToString, comparer);

      return GetDifferentialInternally(currentCollection, previousLookup, comparer);
    }

    private IEnumerable<TModel> GetDifferentialInternally<TModel>(IEnumerable<TModel> currentCollection, IDictionary<TModel, String> previousLookup, IEqualityComparer<TModel> comparer)
    {
      foreach (var current in currentCollection.Distinct(comparer).ToArray())
      {
        var previousSerialization = previousLookup.GetValueOrDefault(current);

        if (previousSerialization != null)
        {
          var currentSerialization = JsonSerializer.SerializeToString(current);

          if (currentSerialization == previousSerialization)
          {
            continue;
          }
        }

        yield return current;
      }
    }

    private DifferentialStorage GetStorageForType(Type type)
    {
      if (type == null)
      {
        throw new ArgumentNullException("type");
      }

      StorageDirectory.Create();

      return new DifferentialStorage(Path.Combine(StorageDirectory.FullName, type.FullName + StorageFileExtension));
    }
  }
}
