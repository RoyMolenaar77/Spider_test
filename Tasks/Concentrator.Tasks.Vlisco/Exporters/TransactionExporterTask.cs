using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Exporters
{
  //using Objects.Models.Attributes;
  using Objects.Models.Connectors;
  //using Objects.Models.Contents;
  //using Objects.Models.Products;
  //using Objects.Models.Orders;
  using Objects.Monitoring;
  //using Objects.Vendors.Bulk;

  using Models;

  [Task(Constants.Vendor.Vlisco + " Transaction Exporter Task")]
  public class TransactionExporterTask : ConnectorTaskBase
  {
    private static readonly FeeblMonitoring Monitoring = new FeeblMonitoring();

    //[Resource]
    //private static readonly String SelectCustomers = null;

    //[Resource]
    //private static readonly String SelectOrders = null;

    [ConnectorSetting(Constants.Connector.Setting.Destination)]
    private String Destination
    {
      get;
      set;
    }

    protected override void ExecuteConnectorTask()
    {
      Monitoring.Notify(Name, 0);

      var destination = new DirectoryInfo(Destination);

      if (!destination.Exists)
      {
        TraceError("{0}: The directory '{1}' does not exists!", Context.Name, destination.FullName);
      }
      else
      {
        var consolidateConnectors = Unit.Scope
          .Repository<ConnectorSystem>()
          .Include(connectorSystem => connectorSystem.Connectors)
          .GetAll(connectorSystem => connectorSystem.Name == Constants.Connector.System.Magento /* || connectorSystem.Name == Constants.Connector.System.MultiMag */)
          .SelectMany(connectorSystem => connectorSystem.Connectors)
          .ToArray();

        var repositoryDirectory = new DirectoryInfo(Constants.Directories.Repository);

        if (!repositoryDirectory.Exists)
        {
          repositoryDirectory.Create();
        }

        var differentialProvider = new DifferentialProvider(Path.Combine(Constants.Directories.History, Context.Name));

        foreach (var fileGrouping in repositoryDirectory
          .GetFiles("??_???_*" + Constants.Extensions.CSV)
          .OrderByDescending(fileInfo => fileInfo.LastWriteTime)
          .GroupBy(fileInfo => fileInfo.Name.Substring(0, 2).ToUpper()))
        {
          var destinationFileName = String.Format("{0}_{1:yyyyMMdd_HHmmss}" + Constants.Extensions.CSV, fileGrouping.Key, DateTime.Now);
          var destinationFileInfo = new FileInfo(Path.Combine(destination.FullName, destinationFileName));

          try
          {
            switch (fileGrouping.Key)
            {
              case Constants.Prefixes.Customer:
                var customers = fileGrouping
                  .SelectMany(Import<Customer, CustomerMapping>)
                  .ToArray();

                customers = GetCustomers().Concat(customers).Distinct().ToArray();
                customers = differentialProvider.GetDifferential(customers).ToArray();

                Export<Customer, CustomerMapping>(destinationFileInfo, customers);
                break;

              case Constants.Prefixes.Items:
                var items = fileGrouping
                  .SelectMany(Import<Item, ItemMapping>)
                  .Distinct()
                  .ToArray();

                items = differentialProvider
                  .GetDifferential(items)
                  .ToArray();

                Export<Item, ItemMapping>(destinationFileInfo, items);
                break;

              case Constants.Prefixes.Movements:
                var movements = fileGrouping
                  .SelectMany(Import<Movement, MovementMapping>)
                  .Distinct()
                  .ToArray();

                movements = differentialProvider
                  .GetDifferential(movements)
                  .ToArray();

                Export<Movement, MovementMapping>(destinationFileInfo, movements);
                break;

              case Constants.Prefixes.Statistics:
                var statistics = fileGrouping
                  .SelectMany(Import<Statistic, StatisticMapping>)
                  .Distinct()
                  .ToArray();

                statistics = differentialProvider
                  .GetDifferential(statistics)
                  .ToArray();

                Export<Statistic, StatisticMapping>(destinationFileInfo, statistics);
                break;

              case Constants.Prefixes.Stock:
                var stock = fileGrouping
                  .SelectMany(Import<Stock, StockMapping>)
                  .Distinct()
                  .ToArray();

                stock = differentialProvider
                  .GetDifferential(stock)
                  .ToArray();

                Export<Stock, StockMapping>(destinationFileInfo, stock);
                break;

              case Constants.Prefixes.Transaction:
                var orders = fileGrouping
                  .SelectMany(Import<Order, OrderMapping>)
                  .ToArray();

                orders = GetOrders(consolidateConnectors)
                  .Concat(orders)
                  .Distinct()
                  .ToArray();
                orders = differentialProvider
                  .GetDifferential(orders)
                  .ToArray();

                Export<Order, OrderMapping>(destinationFileInfo, orders);
                break;

              default:
                break;
            }
          }
          catch (Exception exception)
          {
            TraceCritical(exception);
          }
        }
      }

      Monitoring.Notify(Name, 1);
    }

    private void Export<TModel, TMapping>(FileInfo fileInfo, IEnumerable<TModel> models) where TMapping : CsvClassMap<TModel>
    {
      if (models.Any())
      {
        using (var streamWriter = new StreamWriter(fileInfo.FullName))
        using (var csvWriter = new CsvWriter(streamWriter))
        {
          csvWriter.Configuration.Delimiter = ";";
          csvWriter.Configuration.RegisterClassMap<TMapping>();
          csvWriter.WriteHeader<TModel>();
          csvWriter.WriteRecords(models);

          TraceInformation("{0}: {1} records written to file '{2}'!", Context.Name, models.Count(), fileInfo.FullName);
        }
      }
    }

    private IEnumerable<TModel> Import<TModel, TMapping>(FileInfo fileInfo) where TMapping : CsvClassMap<TModel>
    {
      var models = new List<TModel>();

      try
      {
        using (var streamReader = new StreamReader(fileInfo.FullName))
        using (var csvReader = new CsvReader(streamReader))
        {
          csvReader.Configuration.Delimiter = ";";
          csvReader.Configuration.RegisterClassMap<TMapping>();

          while (csvReader.Read())
          {
            try
            {
              models.Add(csvReader.GetRecord<TModel>());
            }
            catch (CsvHelperException exception)
            {
              var stringBuilder = new StringBuilder();

              stringBuilder.AppendFormat("{0}: Unable to read the row {1} in '{2}'.", Context.Name, csvReader.Row, fileInfo.Name);

              if (exception.Data.Contains("CsvHelper"))
              {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(exception.Data["CsvHelper"].ToString());
              }

              TraceWarning(stringBuilder.ToString());
            }
          }
        }
      }
      catch (Exception exception)
      {
        TraceCritical(exception);
      }

      return models;
    }

    private IEnumerable<Customer> GetCustomers()
    {
      return Enumerable.Empty<Customer>();
      //return Database.Query<Customer>(SelectCustomers).ToArray();
    }

    private IEnumerable<Order> GetOrders(IEnumerable<Connector> connectors)
    {
      return Enumerable.Empty<Order>();

      //var relatedProductTypeID = Unit.Scope
      //  .Repository<Objects.Models.Products.RelatedProductType>()
      //  .GetSingle(relatedProductType => relatedProductType.Type == Constants.Relation.Style && relatedProductType.IsConfigured)
      //  .RelatedProductTypeID;

      //return Database
      //  .Query<Order>(SelectOrders, new
      //  {
      //    ColorAttributeCode = Constants.Attribute.ColorCode,
      //    SizeAttributeCode = Constants.Attribute.SizeCode,
      //    RelatedProductTypeID = relatedProductTypeID
      //  })
      //  .Select(order =>
      //  {
      //    if (order.ColorCode == null)
      //    {
      //      order.ColorCode = Constants.IgnoreCode;
      //    }

      //    if (order.SizeCode == null)
      //    {
      //      order.SizeCode = Constants.IgnoreCode;
      //    }

      //    return order;
      //  })
      //  .ToArray();
    }

    protected override Boolean ValidateContext()
    {
      return Context.ConnectorSystemID.HasValue && Constants.Connector.System.BusinessIntelligence.Equals(Context.ConnectorSystem.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}
