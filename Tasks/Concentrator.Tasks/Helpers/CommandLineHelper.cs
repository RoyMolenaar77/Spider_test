using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Concentrator.Tasks
{
  public static class CommandLineHelper
  {
    private static String CommandLine
    {
      get;
      set;
    }

    private static TraceSource FallbackTraceSource
    {
      get;
      set;
    }

    static CommandLineHelper()
    {
      // This regular expression strips the executable file name wrapped in double quotes including the trailing white spaces, for example:
      // "C:\Concentrator\Service\Task.exe" /Parameter1 "Test Test" /Parameter2 123456
      // Becomes:
      // /Parameter1 "Test Test" /Parameter2 123456
      CommandLine = Regex.Replace(Environment.CommandLine, "((?<QUOTE>^\")[^\"]*(?<-QUOTE>\")\\s+)(?(QUOTE)(?!))", String.Empty);
      FallbackTraceSource = new TraceSource("CommandLineHelper", SourceLevels.All);
    }

    /// <summary>
    /// This method will bind the result of the command line switch to the instance fields and/or properties decorated by the <see cref="Concentrator.Tasks.CommandLineParameterAttribute"/>.
    /// </summary>
    public static void Bind(Object instance, TraceSource traceSource = null)
    {
      instance.ThrowIfNull();

      var memberInfos = instance
        .GetType()
        .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Where(memberInfo => memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
        .ToArray();

      Bind(memberInfos, instance, traceSource ?? FallbackTraceSource);
    }

    /// <summary>
    /// This method will bind the result of the command line switch to the static fields and/or properties decorated by the <see cref="Concentrator.Tasks.CommandLineParameterAttribute"/>.
    /// </summary>
    public static void Bind(Type type, TraceSource traceSource = null)
    {
      var memberInfos = type
        .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        .Where(memberInfo => memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
        .ToArray();

      Bind(memberInfos, null, traceSource ?? FallbackTraceSource);
    }

    /// <summary>
    /// This method will bind the result of the command line switch to the static fields and/or properties decorated by the <see cref="Concentrator.Tasks.CommandLineParameterAttribute"/>.
    /// </summary>
    public static void Bind<T>(TraceSource traceSource = null)
    {
      Bind(typeof(T), traceSource);
    }

    private static void Bind(MemberInfo[] memberInfos, Object instance, TraceSource traceSource)
    {
      foreach (var memberInfo in memberInfos)
      {
        var commandLineSwitchAttribute = memberInfo.GetCustomAttribute<CommandLineParameterAttribute>(true);

        if (commandLineSwitchAttribute != null)
        {
          switch (memberInfo.MemberType)
          {
            case MemberTypes.Field:
              Bind(commandLineSwitchAttribute, instance, memberInfo as FieldInfo, traceSource);
              break;

            case MemberTypes.Property:
              Bind(commandLineSwitchAttribute, instance, memberInfo as PropertyInfo, traceSource);
              break;
          }
        }
      }
    }

    private static void Bind(CommandLineParameterAttribute commandLineSwitchAttribute, Object instance, FieldInfo fieldInfo, TraceSource traceSource)
    {
      fieldInfo.SetValue(instance, GetMemberValue(commandLineSwitchAttribute, fieldInfo.FieldType, traceSource));
    }

    private static void Bind(CommandLineParameterAttribute commandLineSwitchAttribute, Object instance, PropertyInfo propertyInfo, TraceSource traceSource)
    {
      propertyInfo.SetValue(instance, GetMemberValue(commandLineSwitchAttribute, propertyInfo.PropertyType, traceSource), null);
    }

    private static readonly Regex MemberValueRegex = new Regex("((?<QUOTE>\")[^\"]*(?<-QUOTE>\"))(?(QUOTE)(?!))|[^\\s\\,]+", RegexOptions.Compiled);

    private static Object GetDefaultValue(Type valueType)
    {
      if (typeof(IEnumerable).IsAssignableFrom(valueType))
      {
        var genericTypes = valueType.IsArray
          ? new[] { valueType.GetElementType() }
          : valueType.GetGenericArguments();

        switch (genericTypes.Length)
        {
          case 0:
            return Array.CreateInstance(typeof(Object), 0);

          case 1:
            return Array.CreateInstance(genericTypes.Single(), 0);
        }
      }
      else if (typeof(Boolean) == valueType)
      {
        return default(Boolean);
      }
      else if (valueType.IsValueType)
      {
        return Activator.CreateInstance(valueType);
      }

      return null;
    }

    private const char ParameterCharacter = '/';

    private static Object GetMemberValue(CommandLineParameterAttribute commandLineSwitchAttribute, Type valueType, TraceSource traceSource)
    {
      valueType.ThrowIfNull();

      foreach (var switchName in commandLineSwitchAttribute.Names
        .Where(item => !item.IsNullOrWhiteSpace())
        .Select(item => item.Trim())
        .Select(item => item.First() != ParameterCharacter
          ? ParameterCharacter + item
          : item))
      {
        var index = CommandLine.IndexOf(switchName, !commandLineSwitchAttribute.IsCaseSensative
          ? StringComparison.CurrentCultureIgnoreCase
          : StringComparison.CurrentCulture);

        if (index > -1)
        {
          index += switchName.Length;

          var nextIndex = CommandLine.IndexOf(ParameterCharacter, index);

          if (nextIndex == -1)
          {
            nextIndex = CommandLine.Length;
          }

          var domain = CommandLine.Substring(index, nextIndex - index).Trim();

          var values = MemberValueRegex
            .Matches(domain)
            .Cast<Match>()
            .Where(match => match.Success)
            .Select(match => match.Value)
            .Select(value => value.Trim('\"', ' '))
            .ToArray();

          // Check if the member accepts multiple arguments separated by spaces.
          if (typeof(IEnumerable).IsAssignableFrom(valueType))
          {
            var enumerableTypes = valueType.IsArray
              ? new[] { valueType.GetElementType() }
              : valueType.GetGenericArguments();

            switch (enumerableTypes.Length)
            {
              case 0:
                return values;

              case 1:
                var enumerableType = enumerableTypes.Single();

                if (enumerableType.IsAssignableFrom(typeof(String)))
                {
                  return values;
                }
                else
                {
                  var typeConverter = TypeDescriptor.GetConverter(enumerableType);

                  if (typeConverter.CanConvertFrom(typeof(String)))
                  {
                    var result = Array.CreateInstance(enumerableType, values.Length);

                    for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
                    {
                      result.SetValue(typeConverter.ConvertFromString(values[valueIndex]), valueIndex);
                    }

                    return result;
                  }
                  else
                  {
                    traceSource.TraceError("Cannot convert '{0}' to '{1}'"
                      , typeof(String).FullName
                      , valueType.GetElementType().FullName);
                  }
                }
                break;

              default:
                traceSource.TraceError("Cannot handle more than 1 generic argument ('{0}')", valueType.FullName);
                break;
            }
          }
          else if (typeof(Boolean) == valueType && !values.Any())
          {
            return true;
          }
          else if (values.Any())
          {
            var value = values.First();
            var typeConverter = TypeDescriptor.GetConverter(valueType);

            if (typeConverter.CanConvertFrom(typeof(String)))
            {
              return typeConverter.ConvertFromString(value);
            }
            else
            {
              traceSource.TraceError("Cannot convert '{0}' to '{1}'", typeof(String).FullName, valueType.FullName);
            }
          }
          else
          {
            traceSource.TraceError("Unable to extract any value of type '{0}' from '{1}'", valueType.FullName, domain);
          }
        }
      }

      return GetDefaultValue(valueType);
    }
  }
}
