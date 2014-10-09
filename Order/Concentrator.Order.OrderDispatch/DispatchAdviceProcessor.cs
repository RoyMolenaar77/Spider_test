using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ordering.Dispatch;
using System.Text.RegularExpressions;
using Concentrator.Plugins.Monitoring;


namespace Concentrator.Order.OrderDispatch
{
  public class DispatchAdviceProcessor : ConcentratorPlugin
  {
    private const string vendorSettingType = "DispatcherType";

    public override string Name
    {
      get { return "Order Dispatch Advice Processor"; }
    }

    private readonly Monitoring _monitoring = new Monitoring();

    protected override void Process()
    {
      log.Info("Starting check dispatch advice process");
      try
      {
        CheckDispatchAdvice();
        log.AuditSuccess("Available dispatch advices processed");
      }
      catch (Exception e)
      {
        log.AuditError("Sending dispatch advice to EDI failed", e, "Order Dispatching");
      }
      log.Info("Finish check dispatch advice process");
    }

    private void CheckDispatchAdvice()
    {
      _monitoring.Notify(Name, 0);
      using (var unit = GetUnitOfWork())
      {
        //TODO : FIX PARENT/CHILD CHECK
        var dispatchers = unit.Scope
          .Repository<Vendor>()
          .GetAll(c => (c.ParentVendorID == null || !string.IsNullOrEmpty(c.OrderDispatcherType))  && c.IsActive)
          .ToList();


        string logPath = Path.Combine(GetConfiguration().AppSettings.Settings["XMLlogReceive"].Value, DateTime.Now.ToString("dd-MM-yyyy"));

#if DEBUG
        logPath = @"C:\Concentrator\Log";
#endif

        if (!Directory.Exists(logPath))
        {
          Directory.CreateDirectory(logPath);
        }

        foreach (var vendor in dispatchers.Where(x => ((VendorType)x.VendorType).Has(VendorType.Assortment)))
        {
#if DEBUG
          if (vendor.VendorID != 50) continue;
#endif
          log.Info("Dispatch advice for vendor " + vendor.VendorID);
          try
          {
            var types = AppDomain.CurrentDomain
              .GetAssemblies()
              .SelectMany(assembly => assembly.GetTypes())
              .Where(type => type.FullName == vendor.OrderDispatcherType && type.GetInterfaces().Contains(typeof(IDispatchable)))
              .ToArray();

            if (types.Length == 0)
            {
              throw new Exception(String.Format("There is no type with the name '{0}'.", vendor.OrderDispatcherType));
            }

            if (types.Length > 1)
            {
              throw new Exception(String.Format("There are too many types with the name '{0}'.", vendor.OrderDispatcherType));
            }

            var dispatcher = (IDispatchable)Activator.CreateInstance(types.Single());

            dispatcher.GetAvailableDispatchAdvices(vendor, log, logPath, unit);
          }
          catch (Exception ex)
          {
            log.AuditError("Error process dispatch for vendor " + vendor.Name, ex);
            _monitoring.Notify(Name, -1);
          }
        }

      }
      _monitoring.Notify(Name, 1);
    }
  }
}
