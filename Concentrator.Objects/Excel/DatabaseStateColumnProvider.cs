using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

using Microsoft.Practices.ServiceLocation;
using Ninject;

namespace Concentrator.Objects.Excel
{
  using DataAccess.Repository;
  using DataAccess.UnitOfWork;
  using Models;

  public class DatabaseStateColumnProvider
  {
    protected IKernel Kernel
    {
      get;
      set;
    }

    public class ColumnDefinitionWrapper
    {
      public List<DefaultColumnDefinition> Columns
      {
        get;
        set;
      }
    }

    public List<DefaultColumnDefinition> GetColumnDefinitions(int userID, string name, IServiceUnitOfWork unit)
    {
      var serializer = new JavaScriptSerializer();

      var state = unit.Service<UserState>().Get(c => c.UserID == userID && c.EntityName == name);
      var stateObject = state != null
        ? state.SavedState
        : String.Empty;
      
      var columnWrapper = serializer.Deserialize<ColumnDefinitionWrapper>(stateObject);

      return columnWrapper != null && columnWrapper.Columns != null
        ? columnWrapper.Columns
        : new List<DefaultColumnDefinition>();
    }
  }
}
