using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Tasks.ERB.Common
{
  /// <summary>
  /// </summary>
  public class Constants
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

    public Constants() { }

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
    public static class Vendor
    {
      public const String Coolcat = "Coolcat";

      public static class Setting
      {

      }
    }

    public static class Connector
    {
      public static class Setting
      {
        //CoolCat Nederland B.V.
        public const string NameDebtor = "NameDebtor";
        //Document
        public const string BodyElement = "BodyElement";
        //NL67 INGB 0681 7700 07
        public const string IBANDebtor = "IBANDebtor";

        //INGBNL2A
        public const string BIC = "BIC";

        // https://
        public const string MagentoGetCustomerInfoOnOrderIdUrl = "MagentoGetCustomerInfoOnOrderIdUrl";
      }
    }

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    #endregion
  }
}
