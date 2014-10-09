using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Concentrator.Objects;
using Concentrator.Objects.Extensions;
using Concentrator.Objects.Web;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Services.Base;
using Concentrator.Web.Shared.Results;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Environments;

namespace Concentrator.Web.Shared.Controllers
{
  public abstract class BaseController : UnitOfWorkController
  {
    private string _connection;

    protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
    {
      return base.Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
    }

    protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
    {
      return base.Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
    }

    public BaseController()
    {

    }

    /// <summary>
    /// Returns the connection string for direct connection to the database
    /// </summary>
    protected string Connection
    {
      get
      {
        if (string.IsNullOrEmpty(_connection)) _connection = Environments.Current.Connection;

        return _connection;
      }
    }

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (Client.User == null || !Client.User.Identity.IsAuthenticated)
      {
        if (filterContext.HttpContext.Request.IsAjaxRequest())
        {
          var result = new JsonResult
           {
             Data = new
              {
                success = false,
                authorized = false,
                message = "Unauthorized"
              }
           };
          result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

          filterContext.Result = result;
          return;

        }
      }

      base.OnActionExecuting(filterContext);
    }

    public ActionResult Success(string message, string includes = "", bool needsRefresh = false, object data = null, bool isMultipartRequest = false, string contentType = "")
    {
      if (isMultipartRequest)
      {
        return new MultipartJson(new
        {
          success = true,
          message,
          needsRefresh,
          includes = includes,
          data = data
        });
      }
      else
      {
        return Json(new
        {
          success = true,
          message,
          needsRefresh,
          includes = includes,
          data = data
        }, contentType);
      }



    }

    public ActionResult Failure(string message, Exception e = null, bool isMultipartRequest = false, string contentType = "")
    {
      var exceptionMsg = e.Try(c => c.InnerException.Message, string.Empty);
      if (string.IsNullOrEmpty(exceptionMsg)) exceptionMsg = e.Try(c => c.Message, string.Empty);

      if (isMultipartRequest)
      {
        return new MultipartJson(new
        {
          success = false,
          message = string.Format("{0} {1}", message, exceptionMsg)
        });
      }
      else
      {
        return Json(new
        {
          success = false,
          message = string.Format("{0} {1}", message, exceptionMsg)
        }, contentType);
      }
    }

    private enum KnownSqlExceptions
    {
      StatementTerminated = 3621,
      DuplicatePrimaryKey = 2627,
      ForeignKeyConstraintViolation = 547,
      CannotInsertNull = 515
    }


    public NumericFilterObject GetNumericFilter()
    {


      int? gt = null, lt = null, eq = null;

      string gtParam = (string.IsNullOrEmpty(Request["gt"]) ? (string)TempData["gt"] : Request["gt"]);
      string ltParam = (string.IsNullOrEmpty(Request["lt"]) ? (string)TempData["lt"] : Request["lt"]);
      string eqParam = (string.IsNullOrEmpty(Request["eq"]) ? (string)TempData["eq"] : Request["eq"]);

      gt = gtParam.ParseToInt();
      lt = ltParam.ParseToInt();
      eq = eqParam.ParseToInt();

      var f = new NumericFilterObject()
      {
        BiggerThan = gt,
        SmallerThan = lt,
        EqualTo = eq
      };

      return f;
    }

    protected ActionResult HandleSqlException(SqlException ex, bool isMultipartRequest = false)
    {

      string message = string.Empty;

      switch (ex.Number)
      {
        case (int)KnownSqlExceptions.DuplicatePrimaryKey:
          message = "You are trying to insert a duplicate object in the database";
          break;
        case (int)KnownSqlExceptions.ForeignKeyConstraintViolation:
          message = "This object cannot be removed as there are still other objects that depend on it.";
          break;
        case (int)KnownSqlExceptions.CannotInsertNull:
          message = "Tried to insert an empty value into a database column that does not allow empty values. Details: " + ex.Message;
          break;
        case (int)KnownSqlExceptions.StatementTerminated:
          break;
        default:
          message = "An unknown database error occurred";
          break;
      }

      return Failure(message, isMultipartRequest: isMultipartRequest);

    }

    protected PagingParameters GetPagingParams()
    {
      return new PagingParameters
      {
        Dir = Request["dir"] ?? "ASC", 
        Skip = (Request["start"] ?? TempData["start"] as String).ToInt().GetValueOrDefault(),
        Sort = Request["sort"], 
        Take = (Request["limit"] ?? TempData["limit"] as String).ToInt().GetValueOrDefault(500)
      };
    }

    //TODO: add to regular method
    protected List<T> GetPagedResultWithCount<T>(IQueryable<T> query, out int count)
    {
      var result = query.Filter(Request).ToList();

      count = result.Count;

      var pagingParameters = GetPagingParams();

      return result
        .AsQueryable()
        .Sort(pagingParameters.Sort, pagingParameters.Dir)
        .Skip(pagingParameters.Skip)
        .Take(pagingParameters.Take)
        .ToList();
    }

    protected List<T> GetPagedResult<T>(IQueryable<T> query)
    {
      var pagingParams = GetPagingParams();

      var result = query.Filter(Request);

      var sorted = result.AsQueryable().Sort(pagingParams.Sort, pagingParams.Dir);
      var skipped = sorted.Skip(pagingParams.Skip);
      var taken = skipped.Take(pagingParams.Take);
      var res = taken.ToList();

      //add localization  here
      foreach (var resultItem in res)
      {
        var dateTimeProperties = resultItem.GetType().GetProperties().Where(c => c.CanWrite && (c.PropertyType == typeof(DateTime) || c.PropertyType == typeof(DateTime?))).ToList();
        foreach (var prop in dateTimeProperties)
        {
          var value = prop.GetValue(resultItem, null);
          if (value != null) //not nullable
          {
            var dateTime = Convert.ToDateTime(value);
            prop.SetValue(resultItem, dateTime.ToLocalTime(), null);
          }
        }
      }
      return res;
    }

    protected ActionResult Search<TModel>(Func<IServiceUnitOfWork, IQueryable<TModel>> query)
    {
      int limit;
      int.TryParse(Request["limit"], out limit);

      using (var unit = GetUnitOfWork())
      {


        var exp = query(unit);

        if (limit != 0)
          exp = exp.Take(limit);

        return Json(new { results = exp.ToList() });
      }
    }

    protected ActionResult SimpleList<T>(Func<IServiceUnitOfWork, IQueryable<T>> query) where T : class
    {
      using (var unit = GetUnitOfWork())
      {
        var exp = query(unit);

        var result = exp.ToList();

        var total = result.Count;

        return Json(new
        {
          total,
          results = result
        }
      );
      }
    }

    protected ActionResult SimpleList<T>(IEnumerable<T> result) where T : class
    {
      var resultArray = result.ToArray();

      using (var unit = GetUnitOfWork())
      {
        var total = resultArray.Count();

        return Json(new
        {
          total,
          results = resultArray
        }
      );
      }
    }

    protected IEnumerable<object> SimpleList<TModel>(Expression<Func<TModel, object>> selector) where TModel : class, new()
    {
      using (var unit = GetUnitOfWork())
      {
        return unit.Service<TModel>().GetAll().Select(selector).ToList();
      }
    }

    protected TModel GetObject<TModel>(Expression<Func<TModel, bool>> predicate)
      where TModel : class, new()
    {
      using (var unit = GetUnitOfWork())
      {
        return unit.Service<TModel>().Get(predicate);
      }
    }

    protected ActionResult List<TModel>(Func<IServiceUnitOfWork, IQueryable<TModel>> query) where TModel : class
    {
      using (var unit = GetUnitOfWork())
      {
        var result = query(unit).Filter(Request);
        int total = result.Count();

        return Json(new 
        { 
          results = GetPagedResult(result), 
          total, 
          JsonRequestBehavior.AllowGet 
        });
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query">Query which gets excecuted on the database</param>
    /// <param name="inMemoryProjection">Additional project which will only excecute in memory</param>
    /// <returns></returns>
    protected ActionResult List<TModel>(IQueryable<TModel> query, Func<IEnumerable<TModel>, IEnumerable<dynamic>> inMemoryProjection = null)
              where TModel : class
    {
      var result = query.Filter(Request);


      int total = result.Count();
      var paged = GetPagedResult(result);
      var projected = inMemoryProjection == null ? paged : inMemoryProjection(paged);

      return Json(new { results = projected, total, JsonRequestBehavior.AllowGet });
    }

    /// <summary>
    /// Creates and populates the model
    /// </summary>
    /// <typeparam name="TModel">The type of the model to use</typeparam>
    /// <param name="CreationAction">Will be called with default created unit and a populated instance of the model from the request</param>
    /// <returns></returns>
    protected ActionResult Create<TModel>(Action<IServiceUnitOfWork, TModel> CreationAction = null, bool isMultipartRequest = false, Action<IServiceUnitOfWork, TModel> onCreatingAction = null,
      Action<IServiceUnitOfWork, TModel> onCreatedAction = null, List<string> includesInResult = null, bool needsRefresh = false, int? timeout = null, string contentType = "")
      where TModel : class, new()
    {
      try
      {
        TModel model = new TModel();
        using (var unit = GetUnitOfWork())
        {
          if (timeout.HasValue)
            unit.Context.CommandTimeout = timeout.Value;

          TryUpdateModel(model);
          SetSpecialPropertyValues<TModel>(model);

          //convert to UTC time
          UpdateModelDateTimeToUTC(model);

          if (onCreatingAction != null) onCreatingAction(unit, model);

          if (CreationAction != null)
            CreationAction(unit, model);
          else
          {
            unit.Service<TModel>().Create(model);
          }
          if (onCreatedAction != null) onCreatedAction(unit, model);
          unit.Save();
        }
        if (includesInResult == null)
          return Success("Successfully created object", isMultipartRequest: isMultipartRequest, needsRefresh: needsRefresh, contentType: contentType);
        else
        {
          StringBuilder builder = new StringBuilder();
          builder.Append("{");
          for (int i = 0; i < includesInResult.Count; i++)
          {

            var propName = includesInResult[i];
            builder.Append("\"" + propName + "\":\"" + model.GetType().GetProperty(propName).GetValue(model, null) + "\"");
            if (i < includesInResult.Count - 1) builder.Append(",");

          }
          builder.Append("}");

          return Success("Successfully created object", includes: builder.ToString(), isMultipartRequest: isMultipartRequest, needsRefresh: needsRefresh, contentType: contentType);
        }
      }
      catch (SqlException e)
      {
        return HandleSqlException(e);
      }
      catch (Exception e)
      {
        if (e.InnerException != null)
          if (e.InnerException is SqlException) return HandleSqlException((SqlException)e.InnerException);

        return Failure("Failed to create object ", e, isMultipartRequest: isMultipartRequest, contentType: contentType);
      }
    }

    protected ActionResult Delete<TModel>(Expression<Func<TModel, bool>> predicate, Action<IServiceUnitOfWork, TModel> actionBeforeDelete = null, Action<IServiceUnitOfWork, TModel> action = null, bool needsRefresh = false) where TModel : class, new()
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var model = unit.Service<TModel>().Get(predicate);

          model.ThrowIfNull();

          if (actionBeforeDelete != null) actionBeforeDelete(unit, model);

          unit.Service<TModel>().Delete(predicate);

          if (action != null) action(unit, model);

          unit.Save();
          return Success("Deleted object", needsRefresh: needsRefresh);
        }
      }
      catch (SqlException e)
      {
        return HandleSqlException(e);
      }
      catch (Exception e)
      {
        return Failure("Failed to delete object. ", e.InnerException);
      }
    }

    public List<int> GetAllUnderlyingVendorAssortmentsByVendorAssortmentID(int vendorAssortID)
    {
      List<int> VendorAssortIDs = new List<int>();

      List<int> ProductIDs = GetAllUnderlyingProdsByVendorAssortmentID(vendorAssortID);
      using (var unit = GetUnitOfWork())
      {
        VendorAssortIDs = unit.Service<VendorAssortment>().GetAll(x => ProductIDs.Contains(x.ProductID)).Select(x => x.VendorAssortmentID).ToList();
      }

      return VendorAssortIDs;
    }

    public List<int> GetAllUnderlyingProdsByVendorAssortmentID(int vendorAssortID)
    {
      int productID;
      using (var unit = GetUnitOfWork())
      {
        productID = Convert.ToInt32(unit.Service<VendorAssortment>().Get(x => x.VendorAssortmentID == vendorAssortID).ProductID);
      }
      return GetAllUnderlyingProds(productID);
    }

    public List<int> GetAllUnderlyingProds(int prodID)
    {
      List<int> allProductIDsThatChange;

      using (var unit = GetUnitOfWork())
      {

        var levelObject = unit.Service<ContentProductMatch>().Get(c => c.ProductID == prodID && c.IsLeading == true);

        if (levelObject == null) return new List<int> { prodID }; //child levels not found; return only productid of selected product.


        int level = Convert.ToInt16(levelObject.Index);

        if (level == 1)//change it for all the underlying products
        {
          allProductIDsThatChange = unit.Service<RelatedProduct>().GetAll(c => c.ProductID == prodID && c.IsConfigured).Select(c => c.RelatedProductID).ToList();
          allProductIDsThatChange.Add(prodID);
        }
        else
        {
          //get the storeID 
          var storeId = unit.Service<ContentProductMatch>().Get(c => c.ProductID == prodID && c.Index == 0).StoreID;
          allProductIDsThatChange = unit.Service<ContentProductMatch>().GetAll(c => c.StoreID == storeId).Select(c => c.ProductID).ToList();
        }
      }
      return allProductIDsThatChange;
    }


    protected ActionResult Update<TModel>(Expression<Func<TModel, bool>> predicate, Action<IServiceUnitOfWork, TModel> action = null, bool updateModel = true, Expression<Func<IServiceUnitOfWork, TModel, bool>> updatePredicate = null, string updatePredicateErrorMessage = null, bool isMultipartRequest = false, bool needsRefresh = false, string[] properties = null)
  where TModel : class, new()
    {
      try
      {

        using (var unit = GetUnitOfWork())
        {
          var u = unit.Service<TModel>().GetAll(predicate);

          u.ForEach((entity, index) =>
          {
            if (updateModel)
            {
              if (updatePredicate != null)
              {
                if (!updatePredicate.Compile().Invoke(unit, entity)) throw new Exception(updatePredicateErrorMessage);
              }
              if (properties != null && properties.Length > 0)
                TryUpdateModel<TModel>(entity, properties);

              else TryUpdateModel<TModel>(entity);

              //convert to UTC time
              UpdateModelDateTimeToUTC(entity, properties);
            }
            if (action != null) action(unit, entity);
          });


          unit.Save();

          return Success("Updated object ", isMultipartRequest: isMultipartRequest, needsRefresh: needsRefresh);
        }
      }
      catch (SqlException e)
      {
        return HandleSqlException(e);
      }
      catch (Exception e)
      {
        return Failure("Failed to update object. ", e, isMultipartRequest: isMultipartRequest);
      }
    }


    private void UpdateModelDateTimeToUTC<TModel>(TModel entity, string[] whitelistProperties = null)
    {
      var dateTimeProperties = entity.GetType().GetProperties().Where(c => c.PropertyType == typeof(DateTime) || c.PropertyType == typeof(DateTime?)).ToList();
      foreach (var prop in dateTimeProperties)
      {
        if (whitelistProperties != null && whitelistProperties.Contains(prop.Name)) continue;

        var value = prop.GetValue(entity, null);
        if (value != null) //not nullable
        {
          var dateTime = Convert.ToDateTime(value);
          prop.SetValue(entity, dateTime.ToUniversalTime(), null);
        }
      }
    }


    protected ActionResult UpdateForAllProducts<TModel>(Expression<Func<TModel, bool>> predicate, Action<IServiceUnitOfWork, TModel> action = null, bool updateModel = true, Expression<Func<IServiceUnitOfWork, TModel, bool>> updatePredicate = null, string updatePredicateErrorMessage = null, bool isMultipartRequest = false, bool needsRefresh = false, params string[] properties)
      where TModel : class, new()
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var updates = unit.Service<TModel>().GetAll(predicate);
          foreach (var update in updates)
          {
            if (updateModel)
            {

              if (updatePredicate != null)
              {
                if (!updatePredicate.Compile().Invoke(unit, update)) throw new Exception(updatePredicateErrorMessage);
              }
              if (properties != null && properties.Length > 0)
                TryUpdateModel<TModel>(update, properties);

              else TryUpdateModel<TModel>(update);

              //convert to UTC time
              UpdateModelDateTimeToUTC(update, properties);
            }

            if (action != null) action(unit, update);

            else
            {
              unit.Save();
            }

          }
          unit.Save();
          return Success("Updated" + updates.Count() + " objects ", isMultipartRequest: isMultipartRequest, needsRefresh: needsRefresh);
        }
      }
      catch (SqlException e)
      {
        return HandleSqlException(e);
      }
      catch (Exception e)
      {
        return Failure("Failed to update object. ", e, isMultipartRequest: isMultipartRequest);
      }
    }

    //  protected ActionResult UpdateForAllProductAttribute<TModel>(Expression<Func<TModel, bool>> predicate, Action<IServiceUnitOfWork, TModel> action = null, bool updateModel = true, Expression<Func<IServiceUnitOfWork, TModel, bool>> updatePredicate = null, string updatePredicateErrorMessage = null, bool isMultipartRequest = false, bool needsRefresh = false, params string[] properties)
    //where TModel : class, new()
    //  {
    //    try
    //    {
    //      using (var unit = GetUnitOfWork())
    //      {
    //        var updates = unit.Service<ProductAttributeValue>().GetAll();
    //        foreach (var update in updates)
    //        {
    //          if (updateModel)
    //          {

    //            if (updatePredicate != null)
    //            {
    //              if (!updatePredicate.Compile().Invoke(unit, update)) throw new Exception(updatePredicateErrorMessage);
    //            }
    //            if (properties != null && properties.Length > 0)
    //              TryUpdateModel<TModel>(update, properties);

    //            else TryUpdateModel<TModel>(update);
    //          }

    //          if (action != null) action(unit, update);

    //          else
    //          {
    //            unit.Save();
    //          }
    //          //unit.Save();

    //        }
    //        unit.Save();
    //        return Success("Updated" + updates.Count() + " objects ", isMultipartRequest: isMultipartRequest, needsRefresh: needsRefresh);
    //      }
    //    }
    //    catch (SqlException e)
    //    {
    //      return HandleSqlException(e);
    //    }
    //    catch (Exception e)
    //    {
    //      return Failure("Failed to update object. ", e, isMultipartRequest: isMultipartRequest);
    //    }
    //  }

    #region Utility
    protected void MergeSession(object data, string key)
    {
      if (Session[key] == null)
        Session.Add(key, data);

      else
        Session[key] = data;
    }

    #endregion

    #region WidgetErrors
    public ActionResult AnychartError()
    {
      return View("Error/AnychartError");
    }

    public ActionResult HtmlError()
    {
      return View("Error/HtmlError");
    }

    #endregion

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="query"></param>
    ///// <param name="options"></param>
    ///// <param name="renderers">A list of server side renderers for excel exports, etc. Properties will be matched with renderers on index.</param>
    ///// <returns></returns>
    //protected ActionResult List<T>(Func<TDataContext, IQueryable<T>> query, DataLoadOptions options = null, IEnumerable<object> renderers = null)
    //{
    //  using (var context = new TDataContext())
    //  {
    //    if (options != null)
    //    {
    //      context.LoadOptions = options;
    //    }
    //    PagingParameters pagingParams = GetPagingParams();

    //    var result = query(context).Filter(Request);


    //    int total = result.Count();

    //    return Json(new
    //    {
    //      total,
    //      results = string.IsNullOrEmpty(pagingParams.Sort) ? result.Skip(pagingParams.Skip).Take(pagingParams.Take).ToList() : result.Sort(pagingParams.Sort, pagingParams.Dir).Skip(pagingParams.Skip).Take(pagingParams.Take).ToList(),
    //      renderers = renderers
    //    });

    //  }
    //}

    //protected IEnumerable<object> JsonStore<T>(Expression<Func<T, object>> selector) where T : class
    //{
    //  return SimpleList(selector);
    //}

    //protected IEnumerable<object> SimpleList<T>(Expression<Func<T, object>> selector) where T : class
    //{
    //  using (var context = new TDataContext())
    //  {
    //    return context.GetTable<T>().Select(selector).ToList();
    //  }
    //}

    //protected ActionResult SimpleList<T>(Func<TDataContext, IQueryable<T>> query) where T : class
    //{
    //  using (var context = new TDataContext())
    //  {
    //    var exp = query(context);

    //    var total = exp.Count();

    //    return Json(new
    //                    {
    //                      total,
    //                      results = exp.ToList()
    //                    }
    //                    );

    //  }
    //}

    //protected ActionResult Search<T>(Func<TDataContext, IQueryable<T>> query) where T : class
    //{
    //  int limit;
    //  int.TryParse(Request["limit"], out limit);

    //  using (var context = new TDataContext())
    //  {
    //    var exp = query(context).Take(limit);
    //    return Json(new { results = exp.ToList() });

    //  }
    //}

    /// <summary>
    /// Sets properties that you can't be reliably bound by TryUpdateModel. 
    /// </summary>
    protected void SetSpecialPropertyValues<T>(T model, params string[] whitelist)
    {
      var writableProperties = from p in typeof(T).GetProperties()
                               where p.CanWrite && ((whitelist == null || whitelist.Length == 0) || whitelist.Contains(p.Name))
                               select p;

      foreach (var property in writableProperties)
      {
        string value = Request.Params[property.Name];
        // Automatically set a boolean property to null if there's no request parameter for it
        if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
        {
          bool empty = string.IsNullOrEmpty(value);
          bool val;

          // If request doesn't contain any param, bool is false
          if (empty)
          {
            val = false;
          }
          else
          {
            // If the request param doesn't parse as a boolean, set the boolean to true (values like 'on')
            if (!bool.TryParse(value, out val))
            {
              val = true;
            }
          }

          property.GetSetMethod().Invoke(model, new object[] { val });


        }
        // If a request parameter is an empty string or equal to the string "null" and the value can be set to null, do so. 
        //TODO uncomment property.PropertyType.IsNullableValueType()
        else if ((!property.PropertyType.IsValueType) //|| property.PropertyType.IsNullableValueType()
        && (value == string.Empty || value == "null"))
        {
          property.GetSetMethod().Invoke(model, new object[] { null });
        }
      }
    }

    ///// <summary>
    ///// Creates an item by updating the model from the request data
    ///// </summary>
    ///// <typeparam name="T">Model type</typeparam>
    ///// <param name="action">Extra action to be executed once the model has been updated</param>
    ///// <param name="message">Custom message</param>
    ///// <param name="updateModel">Indicates if the model should be updated from the request</param>
    ///// <param name="useJsonResult">If the request has been made from a multipart form this should be true</param>
    ///// <param name="includeInResponse">Includes to be serialized in the response</param>
    ///// <param name="modelPredicate">A predicate to be executed against the model when it is updated. If false, returns an error</param>
    ///// <param name="properties"></param>
    ///// <returns></returns>
    //protected ActionResult Create<T>(Action<T, TDataContext> action = null, string message = "Created item", bool updateModel = true, bool? useJsonResult = null, List<string> includeInResponse = null, Expression<Func<T, TDataContext, bool>> modelPredicate = null, string modelPredicateErrorMessage = "Error creating item", Action<TDataContext> predicateAction = null, params string[] properties) where T : class, new()
    //{
    //  try
    //  {
    //    var model = new T();
    //    if (updateModel)
    //    {
    //      if (properties != null && properties.Length > 0)
    //      {
    //        TryUpdateModel(model, properties);
    //      }
    //      else
    //      {
    //        TryUpdateModel(model);
    //      }

    //      SetSpecialPropertyValues(model);
    //    }

    //    using (var context = new TDataContext())
    //    {
    //      if (modelPredicate != null)
    //      {
    //        var success = modelPredicate.Compile().Invoke(model, context);
    //        if (!success)
    //        {
    //          if (predicateAction != null)
    //          {
    //            predicateAction(context);
    //          }
    //          return Failure(modelPredicateErrorMessage);
    //        }
    //      }

    //      if (action != null) action(model, context);

    //      context.GetTable<T>().InsertOnSubmit(model);

    //      context.SubmitChanges();

    //      if (useJsonResult.HasValue && useJsonResult.Value)
    //      {
    //        return Json(new { success = true, message });
    //      }
    //      else
    //      {

    //        StringBuilder builder = new StringBuilder();
    //        if (includeInResponse != null)
    //        {
    //          builder.Append("{");
    //          for (int i = 0; i < includeInResponse.Count; i++)
    //          {
    //            var propName = includeInResponse[i];
    //            builder.Append(propName + ":" + model.GetType().GetProperty(propName).GetValue(model, null));
    //            if (i < includeInResponse.Count - 1) builder.Append(",");
    //          }
    //          builder.Append("}");
    //        }
    //        return Success(message, includes: builder.ToString());
    //      }
    //    }
    //  }
    //  catch (SqlException ex)
    //  {
    //    return HandleSqlException(ex);
    //  }
    //  catch (Exception ex)
    //  {
    //    return Failure("Something went wrong: " + ex.Message);
    //  }
    //}

    ///// <summary>
    ///// Fetches a model by using the supplied query function and updates it according to request parameters
    ///// </summary>
    ///// <param name="query">The query that returns an object. Will throw an exception if the query returns more than one item.</param>
    ///// <param name="action">An additional function that is executed after the model has been fetched and updated.</param>
    ///// <param name="properties">Whitelist of properties to bind</param>
    ///// <returns></returns>
    //protected ActionResult Update<T>(Expression<Func<T, bool>> predicate, Action<T, TDataContext> action = null, bool updateModel = true, Expression<Func<T, TDataContext, bool>> modelPredicate = null, string modelPredicateErrorMessage = "Error updating item", params string[] properties) where T : class
    //{
    //  using (var context = new TDataContext())
    //  {
    //    try
    //    {
    //      var model = context.GetTable<T>().Where(predicate);

    //      if (updateModel)
    //      {
    //        if (properties != null && properties.Length > 0)
    //        {
    //          model.ForEach((m, id) =>
    //          {
    //            TryUpdateModel(m, properties);
    //          });
    //        }
    //        else
    //        {
    //          model.ForEach((m, id) =>
    //          {
    //            TryUpdateModel(m, properties);
    //          });
    //        }
    //        SetSpecialPropertyValues(model);
    //        if (modelPredicate != null)
    //        {
    //          var success = modelPredicate.Compile();
    //          bool successful = true;

    //          model.ForEach((m, id) =>
    //          {
    //            successful = success.Invoke(m, context);
    //          });

    //          if (!successful) return Failure(modelPredicateErrorMessage);
    //        }
    //      }

    //      if (action != null)
    //        model.ForEach((m, id) =>
    //        {
    //          action(m, context);
    //        });

    //      context.SubmitChanges();

    //      return Success("Edited item ");

    //    }
    //    catch (SqlException ex)
    //    {
    //      return HandleSqlException(ex);
    //    }
    //    catch (Exception ex)
    //    {
    //      return Failure("Something went wrong: " + ex.Message);
    //    }

    //  }
    //}

    //protected ActionResult Merge<T>(Expression<Func<T, bool>> predicate, Action<TDataContext, T> action = null, Action<T> populatePrimaryKeysAction = null) where T : class, new()
    //{
    //  using (var context = new TDataContext())
    //  {
    //    try
    //    {
    //      var model = context.GetTable<T>().Where(predicate).FirstOrDefault();

    //      if (model == null)
    //      {
    //        model = new T();

    //        if (populatePrimaryKeysAction != null)
    //          populatePrimaryKeysAction(model);

    //        context.GetTable<T>().InsertOnSubmit(model);
    //      }

    //      TryUpdateModel(model);


    //      if (action != null) action(context, model);

    //      context.SubmitChanges();

    //      return Success("Edited item ");

    //    }
    //    catch (SqlException ex)
    //    {
    //      return HandleSqlException(ex);
    //    }
    //    catch (Exception ex)
    //    {
    //      return Failure("Something went wrong: " + ex.Message);
    //    }

    //  }
    //}


    //protected ActionResult Delete<T>(Expression<Func<T, bool>> predicate) where T : class
    //{
    //  return Delete(predicate, null, null);
    //}


    //protected ActionResult Delete<T>(Expression<Func<T, bool>> predicate, Action<T, TDataContext> action) where T : class
    //{
    //  return Delete(predicate, null, action);
    //}
    ///// <summary>
    ///// Deletes a model by using the query passed in.
    ///// </summary>
    ///// <param name="query">The query that returns an object. Will throw an exception if the query returns more than one item.</param>
    ///// <returns></returns>
    //protected ActionResult Delete<T>(Expression<Func<T, bool>> predicate, string message, Action<T, TDataContext> action = null) where T : class
    //{
    //  using (var context = new TDataContext())
    //  {
    //    try
    //    {
    //      var table = context.GetTable<T>();
    //      var model = table.Where(predicate);

    //      table.DeleteAllOnSubmit(model);


    //      if (action != null)
    //      {

    //        model.ForEach((c, id) =>
    //        {
    //          action(c, context);
    //        });
    //      }
    //      context.SubmitChanges();

    //      return Success(message ?? "Deleted item");

    //    }
    //    catch (SqlException ex)
    //    {
    //      return HandleSqlException(ex);
    //    }
    //    catch (Exception ex)
    //    {
    //      return Failure("Something went wrong: " + ex.Message);
    //    }
    //  }
    //}


    //protected T GetObject<T>(Expression<Func<T, bool>> predicate) where T : class
    //{
    //  using (TDataContext context = new TDataContext())
    //  {
    //    return context.GetTable<T>().Where(predicate).FirstOrDefault();
    //  }
    //}

    //protected T GetObject<T>(Expression<Func<T, bool>> predicate, DataLoadOptions options) where T : class
    //{
    //  using (TDataContext context = new TDataContext())
    //  {
    //    if (options != null)
    //      context.LoadOptions = options;

    //    return context.GetTable<T>().Where(predicate).FirstOrDefault();
    //  }
    //}

    //protected TDataContext GetContext()
    //{
    //  return new TDataContext();
    //}

    //protected void MergeLanguageSpecificData<T>(TDataContext context, Action<T> createActionAddKey = null, Expression<Func<T, bool>> updateActionPredicate = null) where T : class, new()
    //{
    //  foreach (var l in GetPostedLanguages())
    //  {

    //    var languageEntity = context.GetTable<Language>().FirstOrDefault(c => c.Name == l.Key);

    //    var languageValueTable = context.GetTable<T>();

    //    var languageValueEntities = languageValueTable.Where(c => c.GetType().GetProperties().FirstOrDefault(le => le.Name == "LanguageID").GetValue(c, null) == l.Key);

    //    T ent = null;

    //    if (updateActionPredicate != null)
    //    {
    //      ent = languageValueEntities.FirstOrDefault(updateActionPredicate);
    //    }

    //    var languageSpecificEntity = (ILanguageSpecificEntity)ent;

    //    if (ent == null)
    //    {
    //      ent = new T();

    //      Type t = ent.GetType();

    //      if (createActionAddKey != null) { createActionAddKey(ent); }

    //      t.GetProperty("LanguageID").SetValue(ent, languageEntity.LanguageID, null);
    //      t.GetProperty("Name").SetValue(ent, l.Value, null);

    //      languageValueTable.InsertOnSubmit(ent);
    //    }


    //  }
    //}


    ///// <summary>
    ///// Extracts the languages posted by a language manager to the server
    ///// The format expected is --> 1 = Value in english
    ///// </summary>
    ///// <returns></returns>
    protected Dictionary<string, string> GetPostedLanguages()
    {
      Dictionary<string, string> languageInformation = new Dictionary<string, string>();

      var keys = Request.Form.AllKeys.Where(c => c.StartsWith("LN#")).ToList();

      foreach (var key in keys)
      {
        var val = Request.Form[key];
        if (!string.IsNullOrEmpty(val))
          languageInformation.Add(key.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries)[1], Request.Form[key]);
      }
      return languageInformation;
    }

    /// <summary>
    /// <remarks>Integrate with GetPosteLanguages</remarks>
    /// </summary>
    /// <returns></returns>
    protected Dictionary<string, string[]> GetPostedLanguagesMultiple()
    {
      Dictionary<string, string[]> languageInformation = new Dictionary<string, string[]>();

      var keys = Request.Form.AllKeys.Where(c => c.StartsWith("LN#")).ToList();
      foreach (var key in keys)
      {
        string[] values = Request.Form.GetValues(key);
        languageInformation.Add(key.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries)[1], values);
      }

      return languageInformation;
    }
  }



  public class PagingParameters
  {
    public int Skip { get; set; }

    public int Take { get; set; }

    public string Sort { get; set; }

    public string Dir { get; set; }
  }

  public class TranslationResultObject
  {
    public int LanguageID { get; set; }
    public string Language { get; set; }
    public int EntityID { get; set; }
    public string EntityName { get; set; }
  }

  public class NumericFilterObject
  {
    public int? BiggerThan { get; set; }
    public int? SmallerThan { get; set; }
    public int? EqualTo { get; set; }
  }
}
