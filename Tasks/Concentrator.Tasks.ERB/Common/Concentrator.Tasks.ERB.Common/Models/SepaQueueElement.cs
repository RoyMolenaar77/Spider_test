using System;
using System.Collections.Generic;
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
  public class RefundQueueElement
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

    /// <summary>
    /// 
    /// </summary>
    public RefundQueueElement()
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
    [Range(0, Int32.MaxValue, ErrorMessage = "ConnectorID should be greater than {1} and less or equal than {2}")]
    [Required(ErrorMessage = "OrderID is required")]
    public Int32 ConnectorID { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Range(0, Int32.MaxValue, ErrorMessage = "OrderID should be greater than {1} and less or equal than {2}")]
    [Required(ErrorMessage = "OrderID is required")]
    public Int32 OrderID { get; set; }

    /// <summary>
    /// 
    /// </summary>  
    [Required(ErrorMessage = "OrderResponseID is required")]
    public String OrderResponseID { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [StringLength(255, ErrorMessage = "The OrderDescription value cannot exceed {1} characters. ")]
    public String OrderDescription { get; set; }

    /// <summary>
    /// This boolean is set to false in case one or more fields of this instance of <see cref="RefundQueueElement"/> are invalid.
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
    /// This method validates the <see cref="RefundQueueElement"/> content and set the <see cref="IsValid"/> field to false if one ore more fields are invalid.
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
    //  return String.Format("Sepa Order {0}: from connector {1}", OrderID, ConnectorID);
    //}

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    #endregion
  }
}
