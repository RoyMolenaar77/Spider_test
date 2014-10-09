using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Concentrator.Tasks.ERB.Common.Models
{
  /// <summary>
  /// 
  /// </summary>
  public class CustomerInfo
  {
    //=========================================================================
    // Class variables
    //=========================================================================

    #region Class variables

    /// <summary>
    /// 
    /// </summary>
    private List<ValidationResult> m_ValidationResults = new List<ValidationResult>();

    #endregion

    //=========================================================================
    // Class constructors, Load/ Shown events
    //=========================================================================

    #region Class constructors

    public CustomerInfo()
    {
      TypeDescriptor.AddProviderTransparent(new AssociatedMetadataTypeTypeDescriptionProvider(this.GetType()), this.GetType());
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
    [Range(0, Int32.MaxValue, ErrorMessage = "OrderID should be greater than {1} and less or equal than {2}")]
    [Required(ErrorMessage = "OrderID is required")]
    public Int32 OrderID { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [RegularExpression(@"[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}")]
    [Required(ErrorMessage = "IBAN is required")]
    public string IBAN { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required(ErrorMessage = "AccountName is required")]
    public String AccountName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public String BIC { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required(ErrorMessage = "CountryCode is required")]
    [StringLength(3, ErrorMessage = "The Country value cannot exceed {1} characters. ")]
    public String CountryCode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required(ErrorMessage = "Address is required")]
    [StringLength(100, ErrorMessage = "The Address value cannot exceed {1} characters. ")]
    public String Address { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(@"^[a-zA-Z\.\-_]+@([a-zA-Z\.\-_]+\.)+[a-zA-Z]{2,4}$")]
    public String Email { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Refund amount should be greater than {1} and less or equal than {2}")]
    [Required(ErrorMessage = "RefundAmount is required")]
    public double RefundAmount { get; set; }

    /// <summary>
    /// This boolean is set to false in case one or more fields of this instance of <see cref="CustomerInfo"/> are invalid.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// This collection contains all the invalid validation results.
    /// </summary>
    public List<ValidationResult> ValidationFailures
    {
      get { return m_ValidationResults; }
      set { m_ValidationResults = value; }
    }

    /// <summary>
    /// This method validates the <see cref="CustomerInfo"/> content and set the <see cref="Valid"/> field to false if one ore more fields are invalid.
    /// </summary>
    public bool Validate()
    {
      return IsValid = Validator.TryValidateObject(this, new ValidationContext(this, null, null), ValidationFailures, true);
    }

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <returns></returns>
    //public override String ToString()
    //{
    //  return String.Format("Customerinfo Order {0}: Accountname {1} with account {2}", OrderID, AccountName, IBAN);
    //}

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    #endregion
  }
}
