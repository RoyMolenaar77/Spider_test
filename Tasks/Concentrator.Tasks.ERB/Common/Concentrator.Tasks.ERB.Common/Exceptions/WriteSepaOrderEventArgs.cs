using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Tasks.ERB.Common.Models;

namespace Concentrator.Tasks.ERB.Common.Exceptions
{
  /// <summary>
  /// 
  /// </summary>
  public class WriteSepaOrderEventArgs : EventArgs
  {
    //=========================================================================
    // Class variables
    //=========================================================================

    #region Class variables

    #endregion

    //=========================================================================
    // Class constructors, Load/ Shown events
    //=========================================================================

    #region Class constructors

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public WriteSepaOrderEventArgs(SepaRowElement order)
    {
      // Do additional work here otherwise you can leave it empty
    }

    #endregion

    //=========================================================================
    // Class or Control events
    //=========================================================================

    #region Class events

    #endregion

    //=========================================================================
    // Class members (properties & methods)
    //=========================================================================

    #region Class members

    /// <summary>
    /// 
    /// </summary>
    private SepaRowElement order { get; set; }

    /// <summary>
    /// This method return the order which has failed to wrtie to a Sepa document. 
    /// </summary>
    /// <returns></returns>
    public SepaRowElement GetOrder()
    {
      return order;
    }

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    #endregion
  }
}
