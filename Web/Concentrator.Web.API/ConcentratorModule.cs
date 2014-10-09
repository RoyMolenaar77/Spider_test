using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

using Nancy;
using Nancy.Security;

namespace Concentrator.Web.API
{
  public class ConcentratorModule : NancyModule
  {
    #region Nested Types
    
    public enum HttpRequestMethod
    {
      Delete,
      Get,
      Head,
      Options,
      Patch,
      Post,
      Put
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class QueryAttribute : Attribute
    {
      public String Parameter
      {
        get;
        private set;
      }

      public QueryAttribute()
        : this(String.Empty)
      {
      }

      public QueryAttribute(String parameter)
      {
        Parameter = parameter;
      }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RegisterAttribute : Attribute
    {
      public HttpRequestMethod Method
      {
        get;
        private set;
      }

      public String Path
      {
        get;
        private set;
      }

      public RegisterAttribute(params String[] pathSegments)
        : this(HttpRequestMethod.Get, pathSegments)
      {
      }

      public RegisterAttribute(HttpRequestMethod method, params String[] pathSegments)
      {
        Method = method;
        Path = String.Join("/", pathSegments);
      }
    }

    #endregion

    protected String PathBase
    {
      get;
      private set;
    }

    [Query("from")]
    protected DateTime? FromTime
    {
      get;
      private set;
    }

    [Query("till")]
    protected DateTime? TillTime
    {
      get;
      private set;
    }

    [Query("skip")]
    protected Int32? SkipCount
    {
      get;
      private set;
    }

    [Query("take")]
    protected Int32? TakeCount
    {
      get;
      private set;
    }

    public ConcentratorModule()
      : this(null)
    {
    }

    public ConcentratorModule(String pathBase, Boolean requiresAuthentication = true)
    {
      PathBase = pathBase ?? String.Empty;

      Register();

      if (requiresAuthentication)
      {
        this.RequiresAuthentication();
      }
    }

    private static readonly BindingFlags Bindings = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    protected virtual void ParseQuery()
    {
      var query = new Dictionary<String, String>(Request.Query as IDictionary<String, String>, StringComparer.CurrentCultureIgnoreCase);

      foreach (var propertyInfo in GetType().GetProperties(Bindings | BindingFlags.SetProperty))
      {
        var queryAttribute = propertyInfo
          .GetCustomAttributes(true)
          .OfType<QueryAttribute>()
          .SingleOrDefault();

        if (queryAttribute != null)
        {
          var parameter = queryAttribute.Parameter ?? propertyInfo.Name;
          var parameterValue = TypeConverterService.ConvertFromString(propertyInfo.PropertyType, query.GetValueOrDefault(parameter, String.Empty));

          propertyInfo.SetValue(this, parameterValue, null);
        }
      }
    }

    protected virtual void Register()
    {
      foreach (var methodInfo in GetType().GetMethods(Bindings))
      {
        var registerAttribute = methodInfo
          .GetCustomAttributes(true)
          .OfType<RegisterAttribute>()
          .SingleOrDefault();

        if (registerAttribute != null && methodInfo.GetParameters().Length == 1 && methodInfo.ReturnType != typeof(void))
        {
          var path = '/' + String.Join("/", PathBase, registerAttribute.Path).Trim('/');
          var @delegate = methodInfo.CreateDelegate(typeof(Func<dynamic, dynamic>), this) as Func<dynamic, dynamic>;
          
          switch (registerAttribute.Method)
          {
            case HttpRequestMethod.Delete:
              Delete[path] = @delegate;
              break;

            case HttpRequestMethod.Get:
              Get[path] = @delegate;
              break;

            case HttpRequestMethod.Options:
              Options[path] = @delegate;
              break;

            case HttpRequestMethod.Patch:
              Patch[path] = @delegate;
              break;

            case HttpRequestMethod.Post:
              Post[path] = @delegate;
              break;

            case HttpRequestMethod.Put:
              Put[path] = @delegate;
              break;
          }
        }
      }
    }
  }
}