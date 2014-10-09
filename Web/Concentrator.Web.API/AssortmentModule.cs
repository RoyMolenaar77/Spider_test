using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;

using Nancy;
using Nancy.Security;

namespace Concentrator.Web.API
{
  using Core;
  using Core.Services;

  public class AssortmentModule : ConcentratorModule
  {
    [Flags]
    public enum AssortmentDetail
    {
      Attribute,
      Category,
      Localization,
      Media,
      Pricing,
      Stock
    }

    [Query]
    public String Connector
    {
      get;
      set;
    }

    [Query]
    public AssortmentDetail Details
    {
      get;
      set;
    }

    [Query]
    public String Language
    {
      get;
      set;
    }

    public AssortmentModule() : base("Assortment")
    {
    }

    [Register]
    private dynamic GetAssortment(dynamic parameters)
    {
      var productService = new Products();

      return null;
    }
  }
}