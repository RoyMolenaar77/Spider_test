using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Vlisco
{
  using Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders;
  using Exporters;
  using Importers;

  internal class Program
  {
    [CommandLineParameter("/ExportArticles", "/EA")]
    public static Boolean ExportArticles
    {
      get;
      set;
    }

    [CommandLineParameter("/ExportTransactions", "/ET")]
    public static Boolean ExportTransactions
    {
      get;
      set;
    }

    [CommandLineParameter("/ImportArticles", "/IA")]
    public static Boolean ImportArticles
    {
      get;
      set;
    }

    [CommandLineParameter("/ImportTransactions", "/IT")]
    public static Boolean ImportTransactions
    {
      get;
      set;
    }

    [CommandLineParameter("/QueryTransactions", "/QT")]
    public static Boolean QueryTransactions
    {
      get;
      set;
    }

    [STAThread]
    public static void Main(String[] arguments)
    {
      CommandLineHelper.Bind<Program>();

      if (ImportArticles)
      {
        TaskBase.Execute<ArticleImporterTask>();
      }

      if (ExportArticles)
      {
        TaskBase.Execute<ArticleExporterTask>();
      }

      if (QueryTransactions)
      {
        TaskBase.Execute<CustomerDataReader>();
        TaskBase.Execute<ItemDataReader>();
        TaskBase.Execute<MovementDataReader>();
        TaskBase.Execute<OrderDataReader>();
        TaskBase.Execute<StatisticDataReader>();
        TaskBase.Execute<StockDataReader>();
      }

      if (ImportTransactions)
      {
        TaskBase.Execute<TransactionImporterTask>();
        TaskBase.Execute<CustomerImporterTask>();
        TaskBase.Execute<OrderImporterTask>();
        TaskBase.Execute<StockImporterTask>();
      }

      if (ExportTransactions)
      {
        TaskBase.Execute<TransactionExporterTask>();
      }

      if (TaskBase.ShowConsole)
      {
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
      }
    }
  }
}
