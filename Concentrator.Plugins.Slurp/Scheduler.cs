using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Plugins.Slurp
{
  public class Scheduler : ConcentratorPlugin
  {


    private const string _name = "Slurp Scheduler Plugin";

    public override string Name
    {
      get { return _name; }
    }


    protected override void Process()
    {
      Schedule();
    }
List<Product> products = new List<Product>();
    private void Schedule()
    {
      #region Scheduling

      using (var unit = GetUnitOfWork())
      {

        var schedule = unit.Scope.Repository<SlurpSchedule>().GetAll().ToList();

        foreach (var item in schedule)
        {

          log.DebugFormat("Processing schedule {0}", item.SlurpScheduleID);
          
          if (item.ProductID.HasValue)
            products.Add(item.Product);
          else
          {
            var pgm = unit.Scope.Repository<ProductGroupMapping>().GetSingle(x => x.ProductGroupMappingID == item.ProductGroupMappingID);

           GetProductByProductGroupHierarchy(pgm);

           // var records = unit.Scope.Repository<ContentProductGroup>().GetAll(x => x.ProductGroupMappingID == item.ProductGroupMappingID).OrderByDescending(x => x.ProductID);

            //products.AddRange(records.Select(x => x.Product).Distinct());

          }

          log.DebugFormat("Schedule has {0} products", products.Count);


          foreach (var product in products)
          {
            var openRecord = unit.Scope.Repository<SlurpQueue>().GetSingle(x => !x.IsCompleted && x.ProductID == product.ProductID && x.ProductCompareSourceID == item.ProductCompareSourceID);

            if (openRecord == null) // no open job, so check for interval
            {
              DateTime? lastDate = product.SlurpQueues.Where(x => x.IsCompleted).Max(x => x.CompletionTime);
              if (NeedsScan(item, lastDate))
              {

                openRecord = new SlurpQueue()
                {
                  ProductID = product.ProductID,
                  SlurpScheduleID = item.SlurpScheduleID,
                  ProductCompareSourceID = item.ProductCompareSourceID,
                  IsCompleted = false,
                  CreationTime = DateTime.Now
                };

                unit.Scope.Repository<SlurpQueue>().Add(openRecord);


              }
            }
            unit.Save();
          }
        }

      }


      #endregion

    }

    private void GetProductByProductGroupHierarchy(ProductGroupMapping mapping)
    {
      mapping.ChildMappings.ForEach((map,idx) =>
      {
        GetProductByProductGroupHierarchy(map);
      });

      if (mapping.ChildMappings.Count() == 0)
      {
        products.AddRange(mapping.ContentProductGroups.Select(x => x.Product).ToList());
      }
    }

    private bool NeedsScan(SlurpSchedule item, DateTime? lastDate)
    {

      if (lastDate == null)
        lastDate = DateTime.MinValue;

      IntervalType interval = (IntervalType)item.IntervalType;

      switch (interval)
      {
        case IntervalType.Minutes:
          lastDate.Value.AddMinutes(item.Interval);
          break;
        case IntervalType.Days:
          lastDate.Value.AddDays(item.Interval);
          break;
        case IntervalType.Hours:
          lastDate.Value.AddHours(item.Interval);
          break;
        case IntervalType.Months:
          lastDate.Value.AddMonths(item.Interval);
          break;

        case IntervalType.Years:
          lastDate.Value.AddYears(item.Interval);
          break;
      }
      return DateTime.Now > lastDate.Value;
    }
  }
}
