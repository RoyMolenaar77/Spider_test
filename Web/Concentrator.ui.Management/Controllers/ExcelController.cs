using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Excel;
using Concentrator.Objects.Extensions;
using Concentrator.Objects.Web;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ExcelController : UnitOfWorkController
  {
    private string _customerSpecificControllerAssembly = "Concentrator.Web.CustomerSpecific";

    public void ToExcel(string url, string name, string stateName, string columnsOverride, bool? labelColumns, string xlsName = "", string controllerAssembly = "Concentrator.ui.Management.Controllers")
    {
      if (string.IsNullOrEmpty(controllerAssembly)) controllerAssembly = "Concentrator.ui.Management.Controllers";
      var grid = new System.Web.UI.WebControls.GridView();
      var addExtraColumns = labelColumns ?? false;
      if (string.IsNullOrEmpty(xlsName)) xlsName = name;

      var jsonData = (((JsonResult)GetDataSource(url, controllerAssembly)).Data);
      var data = jsonData.GetType().GetProperty("results").GetValue(jsonData, null);

      DatabaseStateColumnProvider provider = new DatabaseStateColumnProvider();
      var columns = provider.GetColumnDefinitions(Client.User.UserID, stateName, GetUnitOfWork());

      if (columnsOverride != null)
      {

        string[] values = columnsOverride.Split(',');

        foreach (var item in values)
        {
          var stateColumns = columns.Where(c => columnsOverride.Contains(c.Header)).ToList();

          columns = stateColumns;
        }
      }
      if (addExtraColumns)
      {
        DefaultColumnDefinition fromDateColumn = new DefaultColumnDefinition() { DataIndex = "fromDate", Header = "FromDate", Hidden = false, Width = 100 };
        columns.Add(fromDateColumn);

        DefaultColumnDefinition toDateColumn = new DefaultColumnDefinition() { DataIndex = "toDate", Header = "toDate", Hidden = false, Width = 100 };
        columns.Add(toDateColumn);

        DefaultColumnDefinition labelColumn = new DefaultColumnDefinition() { DataIndex = "Label", Header = "Label", Hidden = false, Width = 100 };
        columns.Add(labelColumn);

        DefaultColumnDefinition priceColumn = new DefaultColumnDefinition() { DataIndex = "Price", Header = "Price", Hidden = false, Width = 100 };
        columns.Add(priceColumn);

        DefaultColumnDefinition productIDColumn = new DefaultColumnDefinition() { DataIndex = "ProductID", Header = "ProductID", Hidden = false, Width = 100 };
        columns.Add(productIDColumn);

        DefaultColumnDefinition vendorIDColumn = new DefaultColumnDefinition() { DataIndex = "VendorID", Header = "VendorID", Hidden = false, Width = 100 };
        columns.Add(vendorIDColumn);

        DefaultColumnDefinition connectorIDColumn = new DefaultColumnDefinition() { DataIndex = "ConnectorID", Header = "ConnectorID", Hidden = false, Width = 100 };
        columns.Add(connectorIDColumn);
      }
      var excelProvider = new ExcelProvider(columns, data, xlsName);

      var excel = excelProvider.GetExcelDocument();

      Response.Clear();
      Response.ClearContent();

      Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.xlsx", name));
      Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
      Response.BinaryWrite(excel);
      Response.End();
    }


    private object GetDataSource(string url, string controllerAssembly)
    {
      string controller = null;
      string action = null;
      Type controllerType = null;
      string original = url;

      Dictionary<string, string> urlParams = new Dictionary<string, string>();

      // make sure we save url passed params, too
      string[] paramSplit = original.Split('?');
      if (paramSplit.Length == 2) foreach (Match m in Regex.Matches(paramSplit[1], "([a-zA-Z_-]+)=([a-zA-Z0-9%_-]*)&?"))
          urlParams.Add(m.Groups[1].Value, m.Groups[2].Value);


      string[] routeSplit = paramSplit[0].Split('/');
      for (int i = 1; i < routeSplit.Length; i++)
      {
        controller = routeSplit[(routeSplit.Length - i) - 1];

        controllerType = Type.GetType(String.Format("{0}.{1}Controller", controllerAssembly, controller), false);
        if (controllerType == null)
        {
          controllerType = Assembly.Load(_customerSpecificControllerAssembly).GetTypes().FirstOrDefault(c => c.Name == string.Format("{0}Controller", controller));
        }

        controllerType.ThrowIfNull("Controller type can't be resolved");

        if (controllerType != null)
        {
          action = routeSplit[routeSplit.Length - i];
          // and let's not forget the possible third argument in WMS.Routing.Route
          if (i > 1) urlParams.Add("id", routeSplit[routeSplit.Length - i + 1]);
          break;
        }
      }

      // initialise controller type and methodinfo using reflection
      var newController = Activator.CreateInstance(controllerType) as ControllerBase;

      // overload temp data for ceyenneController's List()
      var ceyenneController = newController as BaseController;
      if (ceyenneController != null)
      {
        ceyenneController.TempData.Add("start", 0.ToString());
        ceyenneController.TempData.Add("limit", Int32.MaxValue.ToString());
      }

      MethodInfo method = controllerType.GetMethod(action);

      ControllerContext ctx = new ControllerContext();
      ctx.Controller = newController;
      ctx.HttpContext = HttpContext;
      ctx.RequestContext = ControllerContext.RequestContext;
      newController.ControllerContext = ctx;

      // initialise param vars
      var parameters = method.GetParameters();
      object[] newParams = new object[parameters.Length];
      ParameterInfo param; string paramValue;
      Type paramType; Type underlyingType;


      // for each of the original method's params
      for (int i = 0; i < newParams.Length; i++)
      {
        // get this param info
        param = parameters[i];

        // save value and type
        if (!urlParams.TryGetValue(param.Name, out paramValue)) paramValue = Request[param.Name];
        paramType = param.ParameterType;

        // if it's nullable
        if (paramType.IsNullableValueType())
        {
          // check for null so we can set null
          if (string.IsNullOrEmpty(paramValue) || paramValue == "null") newParams[i] = null;
          // or else get underlying type and save converted param
          else
          {
            underlyingType = new NullableConverter(paramType).UnderlyingType;
            object o = Convert.ChangeType(paramValue, underlyingType);
            newParams[i] = o;
          }
        }
        else if (paramType.GetInterface("ISearchCriteria") != null)
        {
          object complexType = Activator.CreateInstance(paramType);

          var setProperties = (from p in paramType.GetProperties()
                               where p.CanWrite
                               select p);

          var paramValues = (from p in setProperties
                             let val = Request.Params[p.Name]
                             let isEmpty = string.IsNullOrEmpty(val) || val == "null"
                             let isNullable = paramType.IsNullableValueType()
                             let conversionType =
                               isNullable ? new NullableConverter(paramType).UnderlyingType : paramType
                             select new
                             {
                               Property = p,
                               Value = isEmpty ? null : Convert.ChangeType(val, conversionType)
                             }).ToArray();

          foreach (var value in paramValues)
          {
            value.Property.GetSetMethod().Invoke(complexType, new[] { value.Value });
          }

          newParams[i] = complexType;

        }
        // if it's a normal type, save converted param
        else
        {

          if ((!paramType.IsPrimitive) && (paramType != typeof(String)) && !paramType.IsArray)
          {
            ModelBindingContext mbc = new ModelBindingContext();

            mbc.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, paramType);
            mbc.ValueProvider = this.ValueProvider;
            var binder = ModelBinders.Binders.GetBinder(paramType);
            newParams[i] = binder.BindModel(ctx, mbc);
          }
          else if (paramType.IsArray && paramValue != null)
          {
            var baseType = paramType.GetElementType();
            var convertType = (baseType.IsNullableValueType()) ? new NullableConverter(baseType).UnderlyingType : null;
            var sValues = paramValue.Split(',');
            var values = Array.CreateInstance(baseType, sValues.Length);

            for (var j = 0; j < sValues.Length; j++)
            {
              var val = sValues[j];
              if (convertType != null && string.IsNullOrEmpty(val) || val == "null") values.SetValue(null, j);
              else values.SetValue(Convert.ChangeType(val, convertType ?? baseType), j);
            }
            newParams[i] = values;
          }
          else
          {
            object o = Convert.ChangeType(paramValue, paramType);
            // excluding limit and start here!
            if (param.Name == "start")
            {
              o = 0;
            }
            else if (param.Name == "limit")
            {
              o = Int32.MaxValue;
            }
            newParams[i] = o;
          }
        }
      }

      try
      {
        return (JsonResult)method.Invoke(newController, newParams);
      }
      catch
      {
        return null;
      }
    }
  }
}
