#region Usings

using System;
using System.IO;
using System.Reflection;
using System.Text;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Constants;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Models;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync.Synchronizer
{
  [Task("Product Description Synchronizer")]
  public class ProductDescriptionSync : ConnectorTaskBase
  {
    public int IntersportContentVendorID { get; set; }
    public int RsoLanguageID { get; set; }

    [ConnectorSetting("ContentSourceDatabaseName")]
    public string SourceDatabaseName { get; set; }

    private Boolean Init()
    {
      var language = Unit.Scope.Repository<Language>().GetSingle(x => x.Name == RsoConstants.Language.Nederlands);
      if (language == null)
      {
        TraceError("Could not find language 'Nederlands'.");
        return false;
      }
      
      RsoLanguageID = language.LanguageID;

      var contentVendor = Unit.Scope.Repository<Vendor>().GetSingle(x => x.Name == RsoConstants.Vendor.ContentVendorName);
      if (contentVendor == null)
      {
        TraceError("Could not find the Intersport content vendor.");
        return false;
      }
      
      IntersportContentVendorID = contentVendor.VendorID;

      return true;
    }

    private void InsertNewProductDescriptions()
    {
      var insertNewProductDescriptionsQuery = GetQueryFromResource(RsoConstants.Queries.InsertNewProductDescriptionsQuery);
      if (!string.IsNullOrEmpty(insertNewProductDescriptionsQuery))
      {
        Database.CommandTimeout = 300;
        
        var query = string.Format(insertNewProductDescriptionsQuery, SourceDatabaseName, IntersportContentVendorID, RsoLanguageID);
        
        Database.Execute(query);
      }
    }

    private void UpdateProductDescriptions()
    {
      var productsThatNeedUpdateQuery = GetQueryFromResource(RsoConstants.Queries.GetProductsThatNeedUpdateDescriptionsQuery);

      var query = string.Format(productsThatNeedUpdateQuery, SourceDatabaseName, IntersportContentVendorID, RsoLanguageID);

      var productsForUpdate = Database.Fetch<ProductDescriptionDiff>(query);

      var updateProductDescriptionBaseQuery = GetQueryFromResource(RsoConstants.Queries.UpdateProductDescriptionQuery);
      foreach (var productForUpdate in productsForUpdate)
      {
        var updateSetStatement = new StringBuilder();

        if (CanInsert(productForUpdate.RsoLongContentDescription, productForUpdate.LongContentDescription))
          updateSetStatement.Append(string.Format("LongContentDescription = '{0}' ", productForUpdate.LongContentDescription));

        if (CanInsert(productForUpdate.RsoShortContentDescription, productForUpdate.ShortContentDescription))
          updateSetStatement.Append(string.Format("ShortContentDescription = '{0}' ", productForUpdate.ShortContentDescription));

        if (CanInsert(productForUpdate.RsoLongSummaryDescription, productForUpdate.LongSummaryDescription))
          updateSetStatement.Append(string.Format("LongSummaryDescription = '{0}' ", productForUpdate.LongSummaryDescription));

        if (CanInsert(productForUpdate.RsoShortSummaryDescription, productForUpdate.ShortSummaryDescription))
          updateSetStatement.Append(string.Format("ShortSummaryDescription = '{0}' ", productForUpdate.ShortSummaryDescription));

        if (CanInsert(productForUpdate.RsoProductName, productForUpdate.ProductName))
          updateSetStatement.Append(string.Format("ProductName = '{0}'", productForUpdate.ProductName));

        if (updateSetStatement.Length > 0)
        {
          var updateProductDescriptionQuery = string.Format(updateProductDescriptionBaseQuery
                                                            , productForUpdate.RsoProductID
                                                            , updateSetStatement
                                                            , IntersportContentVendorID
                                                            , RsoLanguageID);

          Database.Execute(updateProductDescriptionQuery);
        }
      }
    }

    private Boolean CanInsert(string sourceDescription, string targetDescription)
    {
      return string.IsNullOrEmpty(sourceDescription) && !string.IsNullOrEmpty(targetDescription);
    }

    private string GetQueryFromResource(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }

    protected override void ExecuteConnectorTask()
    {
      if (Init())
      {
        TraceInformation("Start inserting new ProductDescriptions");
        InsertNewProductDescriptions();

        TraceInformation("Start updating ProductDescriptions");
        UpdateProductDescriptions();
      }

      TraceInformation("Finished!");
    }
  }
}