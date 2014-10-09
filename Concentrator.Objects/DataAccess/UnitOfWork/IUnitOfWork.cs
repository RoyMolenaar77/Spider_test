using System;
using System.Data.Objects;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Linq;

namespace Concentrator.Objects.DataAccess.UnitOfWork
{
  public interface IUnitOfWork : IDisposable
  {
    /// <summary>
    /// Saves all the changes made to this unit of work
    /// </summary>
    void Save();

    /// <summary>
    /// Returns a new scope
    /// </summary>
    IScope Scope { get; }

    /// <summary>
    /// Quick fix
    /// </summary>
    ConcentratorDataContext Context { get; }

    /// <summary>
    /// Executes a command against the storage
    /// </summary>
    /// <param name="command">Command to execute</param>
    /// <param name="parameters">Will be passed in together with the command</param>
    void ExecuteStoreCommand(string command, params object[] parameters);

    /// <summary>
    /// Executes a query against the storage and returns the results
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="query">Query</param>
    /// <returns></returns>
    IQueryable<TModel> ExecuteStoreQuery<TModel>(string query);
  }
}
