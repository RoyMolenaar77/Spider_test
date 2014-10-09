using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Stores
{
  using Objects.DataAccess.UnitOfWork;
  using Objects.Models.Vendors;

  public abstract class VendorSettingStoreBase : StoreBase
  {
    public Vendor Vendor
    {
      get;
      private set;
    }

    public VendorSettingStoreBase(Vendor vendor, TraceSource traceSource = null)
      : base(traceSource)
    {
      if (vendor == null)
      {
        throw new ArgumentNullException("vendor");
      }

      Vendor = vendor;
    }

    public override Boolean Load()
    {
      foreach (var property in GetProperties())
      {
        var attribute = property
          .GetCustomAttributes(false)
          .OfType<VendorSettingAttribute>()
          .SingleOrDefault();

        if (attribute != null)
        {
          var vendorSetting = Vendor
            .VendorSettings
            .FirstOrDefault(item => item.SettingKey == attribute.SettingKey);

          if (vendorSetting == null)
          {
            TraceSource.TraceEvent(TraceEventType.Error, 0, "Unable to find vendor setting '{0}' for vendor '{1}'."
              , attribute.SettingKey
              , Vendor.Name);

            return false;
          }

          var typeDescriptor = TypeDescriptor.GetConverter(property.PropertyType);

          if (typeDescriptor == null || !typeDescriptor.CanConvertFrom(typeof(String)))
          {
            TraceSource.TraceEvent(TraceEventType.Error, 0, "Unable to find a type converter for type '{0}' for vendor setting '{1}'."
              , property.PropertyType.FullName
              , attribute.SettingKey);

            return false;
          }

          var value = typeDescriptor.ConvertFrom(vendorSetting.Value);

          property.SetValue(this, value, null);
        }
      }

      return true;
    }
  }
}