using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Concentrator.Tasks
{
  using Objects.DataAccess.Repository;
  using Objects.Models.Vendors;

  /// <summary>
  /// Represents the base for a typical importer task.
  /// </summary>
  public abstract class VendorTaskBase : ContextTaskBase<Vendor>
  {
    /// <summary>
    /// Returns the vendor repository including the vendorsettings
    /// </summary>
    protected override IRepository<Vendor> ContextRepository
    {
      get
      {
        return Unit.Scope.Repository<Vendor>().Include(vendor => vendor.VendorSettings);
      }
    }

    /// <summary>
    /// Apply the vendor setting values to each field or property decorated with the <see cref="Concentrator.Tasks.VendorSettingAttribute"/> using the current vendor.
    /// </summary>
    protected virtual void ApplyVendorSettings()
    {
      var vendorSettingDictionary = Context.VendorSettings.ToDictionary(
        vendorSetting => vendorSetting.SettingKey,
        vendorSetting => vendorSetting.Value);

      foreach (var memberInfo in GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
        {
          var vendorSettingAttribute = memberInfo.GetCustomAttribute<VendorSettingAttribute>(true);

          if (vendorSettingAttribute != null)
          {
            var memberType = default(Type);

            switch (memberInfo.MemberType)
            {
              case MemberTypes.Field:
                memberType = ((FieldInfo)memberInfo).FieldType;
                break;

              case MemberTypes.Property:
                memberType = ((PropertyInfo)memberInfo).PropertyType;
                break;
            }

            var vendorSettingValue = String.Empty;
            var memberValue = vendorSettingAttribute.DefaultValue;

            if (vendorSettingDictionary.TryGetValue(vendorSettingAttribute.SettingKey, out vendorSettingValue))
            {
              var stringToMemberTypeConverter = TypeDescriptor.GetConverter(memberType);

              if (stringToMemberTypeConverter.CanConvertFrom(typeof(String)))
              {
                memberValue = stringToMemberTypeConverter.ConvertFromString(vendorSettingValue);
              }
              else
              {
                TraceError("'{0}' cannot convert '{1}' to '{2}'!"
                  , stringToMemberTypeConverter.GetType().FullName
                  , typeof(String).FullName
                  , memberType.FullName);
              }
            }

            switch (memberInfo.MemberType)
            {
              case MemberTypes.Field:
                ((FieldInfo)memberInfo).SetValue(this, memberValue);
                break;

              case MemberTypes.Property:
                ((PropertyInfo)memberInfo).SetValue(this, memberValue, null);
                break;
            }
          }
        }
      }
    }

    protected override void ExecuteContextTask()
    {
      ApplyVendorSettings();

      ExecuteVendorTask();
    }

    protected abstract void ExecuteVendorTask();

    private Boolean ValidateRequiredVendorSettings()
    {
      var mandatorySettingsPresent = true;

      var vendorSettingDictionary = Context.VendorSettings.ToDictionary(
        vendorSetting => vendorSetting.SettingKey, 
        vendorSetting => vendorSetting.Value, 
        StringComparer.CurrentCultureIgnoreCase);

      foreach (var memberInfo in GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
        {
          var vendorSettingAttribute = memberInfo.GetCustomAttribute<VendorSettingAttribute>(true);

          if (vendorSettingAttribute != null)
          {
            if (vendorSettingAttribute.IsRequired && !vendorSettingDictionary.ContainsKey(vendorSettingAttribute.SettingKey))
            {
              TraceError("The vendor setting '{0}' does not exist for vendor '{1}'.", vendorSettingAttribute.SettingKey, Context.Name);

              mandatorySettingsPresent = false;
            }
          }
        }
      }

      return mandatorySettingsPresent;
    }

    protected override bool ValidateContext()
    {
      return ValidateRequiredVendorSettings() && base.ValidateContext();
    }
  }
}