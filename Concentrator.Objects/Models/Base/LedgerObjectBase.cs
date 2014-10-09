using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.ComponentModel;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Data.Objects;
using System.Data.Common;

namespace Concentrator.Objects.Models.Base
{
  public abstract class LedgerObjectBase : ILedgerObject
  {
    #region Ledger callbacks

    /// <summary>
    /// Is called when the object is being deleted from the database. (base.LedgerOnDelete() will return false)
    /// </summary>
    /// <param name="ledgerEntry">The ledger object that will be saved</param>
    /// <returns>Return true if you want the ledger entry to be written to the database. Returning false would prevent this from happening.</returns>
    internal virtual bool LedgerOnDelete(ContentLedger ledgerEntry, ConcentratorDataContext dataContext, LedgerObjectBase originalObject)
    {
      return false;
    }

    /// <summary>
    /// Is called when the object is being inserted into the database. (base.LedgerOnInsert() will return false)
    /// </summary>
    /// <param name="ledgerEntry">The ledger object that will be saved</param>
    /// <returns>Return true if you want the ledger entry to be written to the database. Returning false would prevent this from happening.</returns>
    internal virtual bool LedgerOnInsert(ContentLedger ledgerEntry, ConcentratorDataContext dataContext)
    {
      return false;
    }

    /// <summary>
    /// Is called when the object is being updated in the database
    /// </summary>
    /// <param name="ledgerEntry">The ledger object that will be saved</param>
    /// <returns>Return true if you want the ledger entry to be written to the database. Returning false would prevent this from happening. (base.LedgerOnUpdate() will return false)</returns>
    internal virtual bool LedgerOnUpdate(ContentLedger ledgerEntry, CurrentValueRecord currentValues, DbDataRecord originalValues, ConcentratorDataContext dataContext)
    {
      return false;
    }


    /// <summary>
    ///  Gets called before any call to the LedgerCallback methods (OnUpdate/OnInsert/OnDelete).
    /// <para>Use this method to setup the ProductLedger entry to your needs</para>
    /// </summary>
    /// <param name="ledgerEntry">The newly constructed ProductLedger entry</param>
    /// <param name="action">The action the is performed on the object</param>
    /// <returns>Default returns true</returns>
    internal virtual bool Ledger(ContentLedger ledgerEntry, ChangeAction action)
    {
      return true;
    }
    /// <summary>
    ///  Gets called before after call to the LedgerCallback methods (OnUpdate/OnInsert/OnDelete).
    /// <para>Use this method to setup the ProductLedger entry to your needs</para>
    /// </summary>
    /// <param name="ledgerEntry">The newly constructed ProductLedger entry</param>
    /// <param name="action">The action the is performed on the object</param>
    /// <returns>Default returns true</returns>
    internal virtual bool AfterLedger(ContentLedger ledgerEntry, ChangeAction action)
    {
      return true;
    }

    void ILedgerObject.LedgerCallback(CurrentValueRecord currentValues, DbDataRecord originalValues, ChangeAction changeAction, DataAccess.EntityFramework.ConcentratorDataContext dataContext, string name) 
    {
      ContentLedger ledgerEntry = new ContentLedger();
      ledgerEntry.LedgerObject = name;

      if (OnBeforeLedger != null)
        OnBeforeLedger(ledgerEntry);

      bool insertLedgerEntry = false;
      Ledger(ledgerEntry, changeAction);

      switch (changeAction)
      {
        case ChangeAction.Delete:
          //insertLedgerEntry = LedgerOnDelete(ledgerEntry, dataContext, (LedgerObjectBase)originalObject);
          break;
        case ChangeAction.Insert:
          insertLedgerEntry = LedgerOnInsert(ledgerEntry, dataContext);
          break;
        case ChangeAction.Update:
          insertLedgerEntry = LedgerOnUpdate(ledgerEntry, currentValues, originalValues, dataContext);
          break;
        default:
          throw new ApplicationException(String.Format("Got ChangeAction: {0}. This not a supported action for ledgering", changeAction.ToString()));
      }

      AfterLedger(ledgerEntry, changeAction);

      // Ad hoc issue
      if (OnLedger != null)
        OnLedger(ledgerEntry);

      if (insertLedgerEntry)
      {
        ledgerEntry.LedgerDate = DateTime.Now;
        dataContext.CreateObjectSet<ContentLedger>().AddObject(ledgerEntry);        
      }
    }


    internal Action<ContentLedger> OnLedger { get; set; }
    internal Action<ContentLedger> OnBeforeLedger { get; set; }

    #endregion
  }
}