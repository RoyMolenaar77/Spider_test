using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using System.Data.Linq;
using ImageService.SpiderService;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Net;

namespace ImageService
{
  public class ImageUtility
  {
    static volatile bool _running;

    static Thread _monitor;
    static System.Timers.Timer _timer;

    static bool _processing;

    static HttpListener _httpListener = new HttpListener();
    static ManualResetEvent _signalStop = new ManualResetEvent(false);

    static AutoResetEvent _sync = new AutoResetEvent(false);
    static AutoResetEvent _shopSync = new AutoResetEvent(false);
    static ManualResetEvent _waitToEnd = new ManualResetEvent(false);
    static ManualResetEvent _shopWaitToEnd = new ManualResetEvent(false);

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ImageService");
    private static int mailCount = 0;

    static ImageUtility()
    {
      _timer = new System.Timers.Timer();
      _timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Exception ex = e.ExceptionObject as Exception;

      if (ex != null)
      {
        if (e.IsTerminating)
          log.Fatal("App Crashed", ex);
        else
          log.Error("App Crashed", ex);
      }
      else
      {
        if (e.IsTerminating)
          log.Fatal("App Crashed with unknown exception");
        else
          log.Error("Unknown error occurred but was not fatal");
      }
    }


    #region Start/Stop

    public static void Start()
    {
      log.Info("App started");

      _timer.AutoReset = false;

      // set service into running state
      _running = true;
      _monitor = new Thread(new ThreadStart(MonitorTimer));
      _monitor.Name = "OrderPoster";
      _monitor.Start();
    }

    public static void Stop()
    {
      log.Info("App exited");

      // signal to stop working 
      _running = false;
      _sync.Set();
      _monitor.Join();

      if (_processing)
      {
        // wait for work to end
        _waitToEnd.WaitOne(TimeSpan.FromMinutes(3), true);
      }


    }

    #endregion

    // this will be running on a background thread
    static void MonitorTimer()
    {
      while (_running)
      {
        Process();
        _sync.WaitOne();
      }
    }

    static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      _timer.Stop();
      _sync.Set();
    }


  }

}
