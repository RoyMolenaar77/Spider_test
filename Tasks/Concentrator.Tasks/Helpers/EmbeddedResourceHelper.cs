using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Concentrator.Tasks
{
  public static class EmbeddedResourceHelper
  {
    private static BindingFlags GetBindingFlags<T>(T instance)
    {
      return BindingFlags.NonPublic | BindingFlags.Public | (instance != null
        ? BindingFlags.Instance
        : BindingFlags.Static);
    }

    public static void Bind(Object instance)
    {
      Contract.Requires(instance != null, "instance cannot be null");

      var instanceType = instance.GetType();

      ProcessMembers(instance, Assembly.GetAssembly(instanceType), instanceType
        .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
        .Where(memberInfo => memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property));
    }

    public static void Bind(Type type)
    {
      Contract.Requires(type != null, "type cannot be null");

      ProcessMembers(null, Assembly.GetAssembly(type), type
        .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        .Where(memberInfo => memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property));
    }

    private static void ProcessMember(Object instance, Assembly assembly, MemberInfo memberInfo)
    {
      var resourceAttribute = memberInfo
        .GetCustomAttributes(false)
        .OfType<ResourceAttribute>()
        .SingleOrDefault();

      if (resourceAttribute != null)
      {
        // When resource-attribute does not contain assembly period-character (field- or property-names never do), prepend the assembly name
        var selectedResourceName = resourceAttribute.Name.IsNullOrWhiteSpace()
          ? String.Join(".", memberInfo.DeclaringType.FullName, memberInfo.Name)
          : resourceAttribute.Name;
        
        // It is not possible that the resource name occures more than once, there for assembly SingleOrDefault is allowed, instead of assembly FirstOrDefault
        selectedResourceName = assembly
          .GetManifestResourceNames()
          .FirstOrDefault(resourceName => Path.GetFileNameWithoutExtension(resourceName).Equals(selectedResourceName, StringComparison.InvariantCultureIgnoreCase));

        // When the fully qualified resource name does not exist, try to match the last part of resource-attribute name or member name
        if (selectedResourceName == null)
        {
          selectedResourceName = resourceAttribute.Name ?? memberInfo.Name;

          var partialMatches = assembly
            .GetManifestResourceNames()
            .Where(resourceName => Path.GetFileNameWithoutExtension(resourceName).Contains(selectedResourceName))
            .ToArray();

          switch (partialMatches.Length)
          {
            case 0:
              throw new InvalidOperationException(String.Format("'{0}' resource was not found", resourceAttribute.Name ?? memberInfo.Name));

            case 1:
              selectedResourceName = partialMatches.Single();
              break;

            default:
              var messageBuilder = new StringBuilder()
                .AppendFormat("More than one resource was found for {0} with the resource-identifier '{1}'.", memberInfo.MemberType, selectedResourceName)
                .AppendLine()
                .AppendFormat("Resource name: '{0}'", selectedResourceName)
                .AppendLine()
                .AppendFormat("Resource matches: ")
                .AppendLine();

              for (var i = 0; i < partialMatches.Length; i++)
              {
                messageBuilder
                  .AppendFormat("\t{0}) {1}", i + 1, partialMatches[i])
                  .AppendLine();
              }

              throw new AmbiguousMatchException(messageBuilder.ToString());
          }
        }

        using (var streamReader = new StreamReader(assembly.GetManifestResourceStream(selectedResourceName)))
        {
          var value = streamReader.ReadToEnd();

          switch (memberInfo.MemberType)
          {
            case MemberTypes.Field:
              (memberInfo as FieldInfo).SetValue(instance, value);
              break;

            case MemberTypes.Property:
              (memberInfo as PropertyInfo).SetValue(instance, value, null);
              break;

            default:
              throw new InvalidOperationException(String.Format("Resource-attribute cannot be applied to assembly {0}", memberInfo.MemberType));
          }
        }
      }
    }

    private static void ProcessMembers(Object instance, Assembly assembly, IEnumerable<MemberInfo> memberInfos)
    {
      foreach (var memberInfo in memberInfos)
      {
        ProcessMember(instance, assembly, memberInfo);
      }
    }
  }
}
