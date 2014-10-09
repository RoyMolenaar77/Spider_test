using System;

namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync.Constants
{
  public static class RsoConstants
  {
    public static class Queries
    {
      public static string InsertNewProductDescriptionsQuery = "Concentrator.Tasks.Euretco.Rso.ProductContentSync.Queries.InsertNewProductDescriptions.txt";

      public static string GetProductsThatNeedUpdateDescriptionsQuery = "Concentrator.Tasks.Euretco.Rso.ProductContentSync.Queries.GetProductsThatNeedUpdateDescriptions.txt";

      public static string UpdateProductDescriptionQuery = "Concentrator.Tasks.Euretco.Rso.ProductContentSync.Queries.UpdateProductDescription.txt";
    }

    public static class Language
    {
      public const String Nederlands = "Nederlands";
    }

    public static class Vendor
    {
      public const String ContentVendorName = "IntersportContent";
    }
  }
}