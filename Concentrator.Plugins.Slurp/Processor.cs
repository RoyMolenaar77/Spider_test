using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using System.Threading;
using System.Threading.Tasks;
using Concentrator.Plugins.Slurp.Providers;
using System.Data.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Products;
namespace Concentrator.Plugins.Slurp
{
  public class Processor : ConcentratorPlugin
  {
    static readonly object _locker = new object();


    private const string _name = "Slurp Processor Plugin";

    public override string Name
    {
      get { return _name; }
    }

    protected override void Process()
    {
      //  Schedule();

      var baseTime = DateTime.Today.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute / 15);


      Dictionary<int, double> averageExecutionTimes = new Dictionary<int, double>();
      Dictionary<int, int> itemsToProcessPerSource = new Dictionary<int, int>();

      int batchTime = 5; // minutes
      int maxDegree = 1; //per site
      int defaultItemTime = 10; //seconds
      var datetimeValue = DateTime.Now.AddDays(-2);

      using (var unit = GetUnitOfWork())
      {

        var slurpQueueList = (from l in unit.Scope.Repository<SlurpQueue>().GetAll()
                              where l.IsCompleted && l.CompletionTime.Value >= datetimeValue
                              select l).ToList();

        averageExecutionTimes = (from l in slurpQueueList
                                 group l by l.ProductCompareSourceID into grouped
                                 select new
                                 {
                                   ID = grouped.Key,
                                   Duration = grouped.Average(x => (x.CompletionTime.Value - x.StartTime.Value).TotalMilliseconds / 1000.0)
                                 }).ToDictionary(x => x.ID, y => y.Duration);

        itemsToProcessPerSource = (from l in unit.Scope.Repository<SlurpQueue>().GetAll()
                                   where !l.IsCompleted
                                   group l by l.ProductCompareSourceID into grouped
                                   select new
                                   {
                                     ID = grouped.Key,
                                     Count = grouped.Count()
                                   }).ToDictionary(x => x.ID, y => y.Count);

      }

      var remainingHoursToday = 24 - baseTime.Hour;
      var batchesRemainingToday = (remainingHoursToday * 60) / batchTime;



      //Random r = new Random(DateTime.Now.Millisecond);
      var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegree };


      log.DebugFormat("Start Execution");

      using (var unit = GetUnitOfWork())
      {
        //var options = new DataLoadOptions();

        //options.LoadWith<SlurpQueue>(x => x.Product);
        //options.LoadWith<SlurpQueue>(x => x.ProductCompareSource);
        //ctx.LoadOptions = options;

        var slurpQueueRepo = unit.Scope.Repository<SlurpQueue>().Include(c => c.Product, c => c.ProductCompareSource);
        var sources = unit.Scope.Repository<ProductCompareSource>().GetAll(x => x.IsActive == true && x.ProductCompareSourceType != null).ToDictionary(x => x.ProductCompareSourceID, y => y);

        var recordsPerSource = (from q in slurpQueueRepo.GetAll(l => !l.IsCompleted && l.ProductCompareSource.IsActive == true && l.ProductCompareSource.ProductCompareSourceType != null)
                                group q by q.ProductCompareSourceID into grouped
                                select grouped).ToList();

        var siteHandles = new ManualResetEvent[recordsPerSource.Count()];

        for (int idx = 0; idx < recordsPerSource.Count; idx++)
        {
          var source = recordsPerSource[idx];
          var compareSource = sources[source.Key];

          log.DebugFormat("Processing {0}", compareSource.Source);

          siteHandles[idx] = new ManualResetEvent(false);

          List<SlurpItem> sourceQueue = new List<SlurpItem>();

          // calculate items to schedule
          if (!averageExecutionTimes.ContainsKey(source.Key))
            averageExecutionTimes[source.Key] = defaultItemTime;

          var maxItemsToQueue = (int)Math.Floor((60.0 / averageExecutionTimes[source.Key]) * batchTime);

          int itemsInQueue = source.Count();

          var itemsToQueue = (int)(Math.Floor(Math.Max(itemsInQueue / (double)batchesRemainingToday, 0)) + 1);
          itemsToQueue = Math.Min(itemsToQueue, maxItemsToQueue);

          log.DebugFormat("Queueing {0} items of {2} in queue (Maximum : {1})", itemsToQueue, maxItemsToQueue, itemsInQueue);


          var startTime = DateTime.Now;


          var sleepTimes = GetSleepTimes(batchTime * 60, itemsToQueue, false);


          //var delayIdeal = ((batchTime * 60.0) - (averageExecutionTimes[source.Key] * itemsToQueue)) / itemsToQueue;

          int stIndex = -1;
          foreach (var product in source.OrderBy(x => x.CreationTime).Take(itemsToQueue))
          {
            var sleepTime = sleepTimes[++stIndex];

            SlurpItem item = new SlurpItem(Type.GetType(compareSource.ProductCompareSourceType), product.QueueID, sleepTime);
            sourceQueue.Add(item);
          }



          var currentHandle = siteHandles[idx];
          Action wrappedAction = () =>
          {

            try
            {

              Parallel.ForEach(sourceQueue, parallelOptions, x =>
              {
                SlurpResult result = x.Process();

                lock (_locker)
                {
                  //var avg = (from l in slurpQueueRepo.GetAll(c => c.IsCompleted && c.StartTime >= startTime)
                  //           select (l.CompletionTime.Value - l.StartTime.Value).TotalMilliseconds / 1000.0).ToList().Average();
                  //log.InfoFormat("Average execution rate for {1} : {0:f2}s / product", avg, result.SiteName);
                }
              });

            }
            finally { currentHandle.Set(); }


          };

          ThreadPool.QueueUserWorkItem(x => wrappedAction());


        }

        if (siteHandles != null && siteHandles.Length > 0)
          WaitHandle.WaitAll(siteHandles);

        //ctx.SubmitChanges();
      }

    }

    private static int[] GetSleepTimes(double sum, int count, bool exact)
    {
      var r = new Random(DateTime.Now.Millisecond);

      var numbers = new float[count];
      var list = new int[count];
      var total = 0f;

      for (var i = 0; i < count; i++)
      {
        numbers[i] = (float)r.NextDouble();
        total += numbers[i];
      }

      for (var i = 0; i < count; i++)
      {
        numbers[i] /= total;
        list[i] = (int)((numbers[i] * sum) + r.NextDouble());
      }

      if (exact)
      {
        var diff = sum - list.Sum();
        for (var delta = Math.Sign(diff); diff != 0; diff += delta)
        {
          list[r.Next(list.Length)] += delta;
        }
      }

      return list;
    }



  }
}
