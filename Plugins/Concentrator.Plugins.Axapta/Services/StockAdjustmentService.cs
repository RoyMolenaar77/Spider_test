using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Plugins.Axapta.Helpers;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Plugins.Axapta.Repositories;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Concentrator.Plugins.Axapta.Services
{
  public class StockAdjustmentService : IAdjustStockService, IExportStockService
  {
    private static string FailedStockFileName
    {
      get
      {
        return String.Format("FailedStock_{0:yyyy-MM-dd_Hmmss_ff}.csv", DateTime.Now);
      }
    }
    private static string TntStockFileName
    {
      get
      {
        return String.Format("TNTStock_{0:yyyy-MM-dd_Hmmss_ff}.txt", DateTime.Now);
      }
    }
    private readonly FtpSetting _ftpSetting = new FtpSetting();

    protected virtual Regex FileNameRegex
    {
      get
      {
        return new Regex(".*\\.csv$", RegexOptions.IgnoreCase);
      }
    }

    const int SapphVendorID = 50;
    const string FtpAddressSettingKey = "FtpAddress";
    const string FtpUsernameSettingKey = "FtpUsername";
    const string FtpPasswordSettingKey = "FtpPassword";
    const string PathSettingKey = "Ax Ftp Dir Stock";

    private readonly ILogger _log;
    private readonly IProductRepository _productRepo;
    private readonly IArchiveService _archiveService;
    private readonly IVendorStockRepository _vendorStockRepo;
    private readonly IVendorSettingRepository _vendorSettingRepo;
    private readonly IVendorAssortmentRepository _vendorAssortmentRepo;

    public StockAdjustmentService(
      ILogger log,
      IArchiveService archiveService,
      IVendorSettingRepository vendorSettingRepo,
      IVendorAssortmentRepository vendorAssortmentRepo,
      IVendorStockRepository vendorStockRepo, 
      IProductRepository productRepo)
    {
      _log = log;
      _archiveService = archiveService;
      _vendorSettingRepo = vendorSettingRepo;
      _vendorStockRepo = vendorStockRepo;
      _productRepo = productRepo;
      _vendorAssortmentRepo = vendorAssortmentRepo;

      FillSettings();
    }

    public void ProcessAdjustmentStock()
    {
      ReadStockFile();
    }

    private void FillSettings()
    {
      _ftpSetting.FtpAddress = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpAddressSettingKey);
      _ftpSetting.FtpUsername = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpUsernameSettingKey);
      _ftpSetting.FtpPassword = _vendorSettingRepo.GetVendorSetting(SapphVendorID, FtpPasswordSettingKey);
      _ftpSetting.Path = _vendorSettingRepo.GetVendorSetting(SapphVendorID, PathSettingKey);
    }
    private void ReadStockFile()
    {
      var ftpManager = new FtpManager(_ftpSetting.FtpUri, null, false, true);
      var regex = FileNameRegex;

      foreach (var fileName in ftpManager.GetFiles())
      {
        if (!regex.IsMatch(fileName)) continue;
        using (var stream = ftpManager.Download(Path.GetFileName(fileName)))
        {
          using (var streamReader = new StreamReader(stream))
          {
            var engine = new FileHelperEngine(typeof(DatColStock));
            var listOfVendorStock = engine.ReadStream(streamReader) as DatColStock[];
            ProcessStock(listOfVendorStock);

            try
            {
              ftpManager.Upload(stream, Path.Combine("History", fileName));
            }
            catch (Exception e) { 
              
            }
          }
        }
        _archiveService.CopyToArchive(ftpManager.BaseUri.AbsoluteUri, SaveTo.StockDirectory, fileName);

        ftpManager.Delete(fileName);
      }
    }
    private void ProcessStock(IEnumerable<DatColStock> listOfLocalVendorStock)
    {
      var corruptStock = new List<DatColStock>();
      var stockToUpdate = new List<VendorStock>();
      var stockToCreate = new List<VendorStock>();

      var currentVendorAssortments = _vendorAssortmentRepo
        .GetListOfVendorAssortmentByVendorID(SapphVendorID)
        .ToLookup(x => Regex.Replace(x.CustomItemNumber, @"\s+", ""))
        .ToDictionary(x => x.Key, x => x.FirstOrDefault());

      var currentVendorStock = _vendorStockRepo
        .GetListOfVendorStockByVendorID(SapphVendorID)
        .ToLookup(x => x.ProductID)
        .ToDictionary(x => x.Key, x => x.FirstOrDefault());

      foreach (var stock in listOfLocalVendorStock)
      {
        if (currentVendorAssortments.ContainsKey(stock.CustomItemNumberWithoutWhiteSpace))
        {
          var productID = currentVendorAssortments[stock.CustomItemNumberWithoutWhiteSpace].ProductID;
          if (currentVendorStock.ContainsKey(productID))
          {
            var vendorStock = currentVendorStock[productID];
            if (vendorStock.QuantityOnHand != stock.Quantity)
            {
              vendorStock.QuantityOnHand = stock.Quantity;
              stockToUpdate.Add(vendorStock);
            }
          }
          else
          {
            var vendorStock = new VendorStock
              {
                ProductID = productID,
                VendorID = SapphVendorID,
                QuantityOnHand = stock.Quantity,
                StockStatus = "ENA",
                ConcentratorStatusID = 4,
                VendorStatus = "ENA",
                VendorStockTypeID = 1
              };
            stockToCreate.Add(vendorStock);
          }
        }
        else
        {
          corruptStock.Add(stock);
          _log.Info(string.Format("Error: CustomItemNumber does not exists. {0}", stock.ModelCode));
        }
      }

      if (stockToCreate.Any())
        _vendorStockRepo.InsertVendorStocks(stockToCreate);

      if (stockToUpdate.Any())
        _vendorStockRepo.UpdateVendorStocksQuantity(stockToUpdate);

      _archiveService.ExportToAxapta(corruptStock, SaveTo.CorruptStockDirectory, FailedStockFileName);
    }

    public void ProcessExportStock(List<DatColStock> listOfStocks)
    {
      var stocksToExport = new List<DatColStockMutation>();

      var stocks = listOfStocks.GroupBy(x => x.CustomItemNumber).Where(x => x.Count() == 2).ToList();
      var corruptStocks = listOfStocks.GroupBy(x => x.CustomItemNumber).Where(x => x.Count() != 2).ToList();

      var currentVendorAssortments = _productRepo
        .GetListOfSkusByVendorID(SapphVendorID)
        .ToLookup(x => Regex.Replace(x.VendorItemNumber, @"\s+", ""))
        .ToDictionary(x => x.Key, x => x.FirstOrDefault());

      foreach (var stock in stocks)
      {
        var stockFrom = stock.FirstOrDefault(x => x.Quantity < 0);
        var stockTo = stock.FirstOrDefault(x => x.Quantity > 0);

        if (stockFrom == null || stockTo == null || (stockFrom.Quantity + stockTo.Quantity != 0))
        {
          corruptStocks.Add(stock);
          continue;
        }

        if (!currentVendorAssortments.ContainsKey(stockFrom.CustomItemNumberWithoutWhiteSpace))
        {
          corruptStocks.Add(stock);
          continue;
        }

        var stockToExport = new DatColStockMutation();
        var product = currentVendorAssortments[stockFrom.CustomItemNumberWithoutWhiteSpace];

        if (!stockToExport.SetVendorItemNumber(product.VendorItemNumber, product.Color, product.Size))
        {
          corruptStocks.Add(stock);
          continue;
        }

        stockToExport.FromStockWarehouse = stockFrom.StockWarehouse;
        stockToExport.ToStockWarehouse = stockTo.StockWarehouse;
        stockToExport.Quantity = stockTo.Quantity;

        if (!stocksToExport.Any())
          stocksToExport.Add(stockToExport); // Axapta kan niet de eerste line lezen. Daarom een extra line

        stocksToExport.Add(stockToExport);
      }

      if (stocksToExport.Any())
        ExportStockMutation(stocksToExport);

      if (corruptStocks.Any())
        ExportToCorruptStockMutation(corruptStocks);
    }
    private void ExportStockMutation(List<DatColStockMutation> stocksToExport)
    {
      var fileName = TntStockFileName;
      _archiveService.ExportToAxapta(stocksToExport, SaveTo.CorrectionStockDirectory, fileName);
      _archiveService.ExportToArchive(stocksToExport, SaveTo.CorrectionStockDirectory, fileName);
    }
    private void ExportToCorruptStockMutation(List<IGrouping<string, DatColStock>> corruptStocksToExpor)
    {
      var fileName = FailedStockFileName;
      _archiveService.ExportToAxapta(corruptStocksToExpor.SelectMany(x => x).ToList(), SaveTo.CorruptCorrectionStockDirectory, fileName);
      _archiveService.ExportToArchive(corruptStocksToExpor.SelectMany(x => x).ToList(), SaveTo.CorruptCorrectionStockDirectory, fileName);
    }
  }
}
