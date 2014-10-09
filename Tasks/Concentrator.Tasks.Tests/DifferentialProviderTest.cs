using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Concentrator.Tasks.Tests
{
  public class TestModel : IEquatable<TestModel>
  {
    public String K0
    {
      get;
      set;
    }

    public String K1
    {
      get;
      set;
    }

    public Guid Value
    {
      get;
      set;
    }

    public Boolean Equals(TestModel other)
    {
      return K0 == other.K0 && K1 == other.K1;
    }

    public override Int32 GetHashCode()
    {
      return K0.GetHashCode() ^ K1.GetHashCode();
    }

    public override String ToString()
    {
      return String.Format("{0}-{1}: {2}", K0, K1, Value);
    }
  }

  [TestClass]
  public class DifferentialProviderTest
  {
    private IEnumerable<TestModel> GenerateTestModels(Int32 start0, Int32 count0, Int32 start1, Int32 count1)
    {
      for (var index0 = 1; index0 <= count0; index0++)
      {
        for (var index1 = 1; index1 <= count1; index1++)
        {
          yield return new TestModel
          {
            K0 = "A" + (start0 + index0),
            K1 = "B" + (start1 + index1),
            Value = Guid.NewGuid()
          };
        }        
      }
    }

    [TestMethod]
    public void HappyFlow()
    {
      DifferentialProvider.Default.CacheDirectory.Delete(true);

      var models = new List<TestModel>(GenerateTestModels(0, 2, 0, 2));

      var firstResult = DifferentialProvider.Default.GetDifferential(models).ToArray();

      Assert.AreEqual(firstResult.Length, models.Count);

      var secondResult = DifferentialProvider.Default.GetDifferential(models).ToArray();

      Assert.AreEqual(secondResult.Length, 0);

      models.AddRange(GenerateTestModels(2, 1, 2, 1));

      var thirdResult = DifferentialProvider.Default.GetDifferential(models).ToArray();

      Assert.AreEqual(thirdResult.Length, 1);

      var fourthResult = DifferentialProvider.Default.GetDifferential(models).ToArray();

      Assert.AreEqual(fourthResult.Length, 0);

      models.First().Value = Guid.Empty;;

      var fifthResult = DifferentialProvider.Default.GetDifferential(models).ToArray();

      Assert.AreEqual(fifthResult.Length, 1);

      var sixthResult = DifferentialProvider.Default.GetDifferential(models).ToArray();

      Assert.AreEqual(sixthResult.Length, 0);
    }
  }
}
