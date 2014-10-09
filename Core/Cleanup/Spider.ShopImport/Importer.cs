using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;

namespace Spider.ShopImport
{
  public class Importer
  {
    static Thread _products;
    static Thread _stock;
    static Thread _productsAndAttributes;
    static System.Timers.Timer _productsTimer;
    static System.Timers.Timer _stockTimer;
    static System.Timers.Timer _productsAndAttributesTimer;
    static volatile bool _running;
    static bool _processingProducts;
    static bool _processingStock;
    static bool _processingProductsAndAttributes;
    static AutoResetEvent _syncProduct = new AutoResetEvent(false);
    static AutoResetEvent _syncStock = new AutoResetEvent(false);
    static AutoResetEvent _syncProductsAndAttributes = new AutoResetEvent(false);
    static ManualResetEvent _waitToEndProduct = new ManualResetEvent(false);
    static ManualResetEvent _waitToEndStock = new ManualResetEvent(false);
    static ManualResetEvent _waitToEndProductsAndAttibutes = new ManualResetEvent(false);
    static bool ExecuteAtStartup = bool.Parse(ConfigurationManager.AppSettings["AttributesAtStartup"].ToString());
    static bool productsprocessing = false;

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WebsiteImportService");

    static Importer()
    {
      // InitializeComponent();
      _productsTimer = new System.Timers.Timer();
      _productsTimer.Elapsed += new System.Timers.ElapsedEventHandler(ProductTimer_Elapsed);

      _productsAndAttributesTimer = new System.Timers.Timer();
      _productsAndAttributesTimer.Elapsed += new System.Timers.ElapsedEventHandler(ProductsAndAttributesTimer_Elapsed);

      _stockTimer = new System.Timers.Timer();
      _stockTimer.Elapsed += new System.Timers.ElapsedEventHandler(StockTimer_Elapsed);
    }

    public static void Start()
    {
      _running = true;

ShopImport import = new ShopImport();
import.ProcessOItems();


//#if DEBUG
//      _productsAndAttributesTimer.AutoReset = false;
//      _productsAndAttributes = new Thread(new ThreadStart(ProductsAndAttributes));
//      _productsAndAttributes.Name = "ProductsAndAttributes";
//      _productsAndAttributes.Start();

//      _stockTimer.AutoReset = false;
//      _stock = new Thread(new ThreadStart(Stock));
//      _stock.Name = "Stock";
//      _stock.Start();


//      _productsTimer.AutoReset = false;
//      _products = new Thread(new ThreadStart(Products));
//      _products.Name = "Products";
//      _products.Start();
//#endif
    }

    public static void Stop()
    {
      _running = false;
      _syncProduct.Set();
      _syncStock.Set();
      _syncProductsAndAttributes.Set();
      _products.Join();
      _stock.Join();
      _productsAndAttributes.Join();

      if (_processingProducts && _processingStock && _processingProductsAndAttributes)
      {
        // wait for work to end
        _waitToEndProduct.WaitOne(TimeSpan.FromMinutes(3), true);
        _waitToEndStock.WaitOne(TimeSpan.FromMinutes(3), true);
        _waitToEndProductsAndAttibutes.WaitOne(TimeSpan.FromMinutes(3), true);
      }
    }

    static void ProductsAndAttributes()
    {
      while (_running)
      {
        DoWorkProductsAndAttribuites();
        _syncProductsAndAttributes.WaitOne();
      }
    }

    static void Products()
    {
      while (_running)
      {
        DoWorkProducts();
        _syncProduct.WaitOne();
      }
    }

    static void Stock()
    {
      while (_running)
      {
        DoWorkStock();
        _syncStock.WaitOne();
      }
    }

    // this runs on a threadpool thread
    static void ProductTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      _productsTimer.Stop();
      _syncProduct.Set();
    }

    static void StockTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      _stockTimer.Stop();
      _syncStock.Set();
    }

    static void ProductsAndAttributesTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      _productsAndAttributesTimer.Stop();
      _syncProductsAndAttributes.Set();
    }

    static void DoWorkProductsAndAttribuites()
    {
      try
      {
        _processingProductsAndAttributes = true;

        if (ExecuteAtStartup && !productsprocessing)
        {
          productsprocessing = true;
          ShopImport import = new ShopImport();
          import.ProcessProductsAndAttributes();

          log.Info("Products and Attributes import Completed On: " + DateTime.Now);
        }
        else
          ExecuteAtStartup = true;

        int minutes = 60;
        int hours = 0;
        int.TryParse(ConfigurationManager.AppSettings["ImportProductAndAttributesIntervalMinutes"], out minutes);
        int.TryParse(ConfigurationManager.AppSettings["ImportProductAndAttributesIntervalHours"], out hours);

        if (minutes > 0 || hours > 0)
        {
          log.InfoFormat("Next products and Attributes run in {0} hours and {1} minutes", hours, minutes);
          _productsAndAttributesTimer.Interval = (new TimeSpan(hours, minutes, 0)).TotalMilliseconds;
          _productsAndAttributesTimer.Start();
        }

        productsprocessing = false;
        _processingProductsAndAttributes = false;
        _waitToEndProductsAndAttibutes.Set();
      }
      catch (Exception ex)
      {
        log.Error("Error import products and Attributes", ex);
      }
    }

    static void DoWorkStock()
    {
      try
      {
        _processingStock = true;
                
        ShopImport import = new ShopImport();
        import.ProcessStock();
        

        Console.WriteLine("Stock import Completed On: " + DateTime.Now);

        int minutes = 60;
        int hours = 0;
        int.TryParse(ConfigurationManager.AppSettings["ImportStockIntervalMinutes"], out minutes);
        int.TryParse(ConfigurationManager.AppSettings["ImportStockIntervalHours"], out hours);

        if (minutes > 0 || hours > 0)
        {
          log.InfoFormat("Next stock run in {0} hours and {1} minutes", hours, minutes);
          _stockTimer.Interval = (new TimeSpan(hours, minutes, 0)).TotalMilliseconds;
          _stockTimer.Start();
        }

        _processingStock = false;
        _waitToEndStock.Set();
      }
      catch (Exception ex)
      {
        log.Error("Error import stock", ex);
      }
    }

    static void DoWorkProducts()
    {
      try
      {
        _processingProducts = true;

        if (!productsprocessing)
        {
          productsprocessing = true;
          ShopImport import = new ShopImport();
          import.ProcessProducts();
        }

        Console.WriteLine("Product import Completed On: " + DateTime.Now);

        int minutes = 60;
        int hours = 0;
        int.TryParse(ConfigurationManager.AppSettings["ImportProductIntervalMinutes"], out minutes);
        int.TryParse(ConfigurationManager.AppSettings["ImportProductIntervalHours"], out hours);

        if (minutes > 0 || hours > 0)
        {
          log.InfoFormat("Next Product run in {0} hours and {1} minutes", hours, minutes);
          _productsTimer.Interval = (new TimeSpan(hours, minutes, 0)).TotalMilliseconds;
          _productsTimer.Start();
        }

        productsprocessing = false;
        _processingProducts = false;
        _waitToEndProduct.Set();
      }
      catch (Exception ex)
      {
        log.Error("Error import products", ex);
      }
    }
  }
}
