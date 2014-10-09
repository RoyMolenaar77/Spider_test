using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Concentrator.Tasks.ERB.Common.Models
{
  /// <summary>
  /// This class acts as a [presenter] that acts upon:
  /// using <see cref=""/> 
  /// using <see cref=""/>
  /// using <see cref=""/>
  /// using <see cref=""/>
  /// </summary>
  public class SepaRowElement
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

    public SepaRowElement() { }

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
    public Int32 OrderID { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string IBAN { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String AccountName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String OrderDescription { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String BIC { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String Country { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String Address { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String Email { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double RefundAmount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Int32 ConnectorID { get; set; }

    /// <summary>
    /// 
    /// </summary>  
    public String OrderResponseID { get; set; }

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    #endregion
  }
}
