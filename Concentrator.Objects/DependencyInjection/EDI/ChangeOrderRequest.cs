﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3615
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.3038.
// 

namespace Concentrator.Web.Objects.EDI.ChangeOrder
{
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class ChangeOrderRequest
  {

    private OrderChangeHeader orderChangeHeaderField;

    private OrderChangeDetail[] orderChangeDetailsField;

    private string versionField;

    private ChangeType changeTypeField;

    /// <remarks/>
    public OrderChangeHeader OrderChangeHeader
    {
      get
      {
        return this.orderChangeHeaderField;
      }
      set
      {
        this.orderChangeHeaderField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
    public OrderChangeDetail[] OrderChangeDetails
    {
      get
      {
        return this.orderChangeDetailsField;
      }
      set
      {
        this.orderChangeDetailsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Version
    {
      get
      {
        return this.versionField;
      }
      set
      {
        this.versionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ChangeType ChangeType
    {
      get
      {
        return this.changeTypeField;
      }
      set
      {
        this.changeTypeField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class OrderChangeHeader
  {

    private System.DateTime requestedDateField;

    private bool requestedDateFieldSpecified;

    private int bSKIdentifierField;

    private int orderNumberField;

    private bool orderNumberFieldSpecified;

    private string paymentTermsCodeField;

    private string paymentInstrumentField;

    private bool partialLineShipmentsAllowedField;

    private bool partialLineShipmentsAllowedFieldSpecified;

    private string routeCodeField;

    private string holdCodeField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
    public System.DateTime RequestedDate
    {
      get
      {
        return this.requestedDateField;
      }
      set
      {
        this.requestedDateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool RequestedDateSpecified
    {
      get
      {
        return this.requestedDateFieldSpecified;
      }
      set
      {
        this.requestedDateFieldSpecified = value;
      }
    }

    /// <remarks/>
    public int BSKIdentifier
    {
      get
      {
        return this.bSKIdentifierField;
      }
      set
      {
        this.bSKIdentifierField = value;
      }
    }

    /// <remarks/>
    public int OrderNumber
    {
      get
      {
        return this.orderNumberField;
      }
      set
      {
        this.orderNumberField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OrderNumberSpecified
    {
      get
      {
        return this.orderNumberFieldSpecified;
      }
      set
      {
        this.orderNumberFieldSpecified = value;
      }
    }

    /// <remarks/>
    public string PaymentTermsCode
    {
      get
      {
        return this.paymentTermsCodeField;
      }
      set
      {
        this.paymentTermsCodeField = value;
      }
    }

    /// <remarks/>
    public string PaymentInstrument
    {
      get
      {
        return this.paymentInstrumentField;
      }
      set
      {
        this.paymentInstrumentField = value;
      }
    }

    /// <remarks/>
    public bool PartialLineShipmentsAllowed
    {
      get
      {
        return this.partialLineShipmentsAllowedField;
      }
      set
      {
        this.partialLineShipmentsAllowedField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool PartialLineShipmentsAllowedSpecified
    {
      get
      {
        return this.partialLineShipmentsAllowedFieldSpecified;
      }
      set
      {
        this.partialLineShipmentsAllowedFieldSpecified = value;
      }
    }

    /// <remarks/>
    public string RouteCode
    {
      get
      {
        return this.routeCodeField;
      }
      set
      {
        this.routeCodeField = value;
      }
    }

    /// <remarks/>
    public string HoldCode
    {
      get
      {
        return this.holdCodeField;
      }
      set
      {
        this.holdCodeField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class ProductIdentifier
  {

    private string productNumberField;

    private string eANIdentifierField;

    private string manufacturerItemIDField;

    /// <remarks/>
    public string ProductNumber
    {
      get
      {
        return this.productNumberField;
      }
      set
      {
        this.productNumberField = value;
      }
    }

    /// <remarks/>
    public string EANIdentifier
    {
      get
      {
        return this.eANIdentifierField;
      }
      set
      {
        this.eANIdentifierField = value;
      }
    }

    /// <remarks/>
    public string ManufacturerItemID
    {
      get
      {
        return this.manufacturerItemIDField;
      }
      set
      {
        this.manufacturerItemIDField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class CustomerReference
  {

    private string customerOrderField;

    private string customerOrderLineField;

    /// <remarks/>
    public string CustomerOrder
    {
      get
      {
        return this.customerOrderField;
      }
      set
      {
        this.customerOrderField = value;
      }
    }

    /// <remarks/>
    public string CustomerOrderLine
    {
      get
      {
        return this.customerOrderLineField;
      }
      set
      {
        this.customerOrderLineField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class OrderChangeDetail
  {

    private CustomerReference customerReferenceField;

    private ProductIdentifier productIdentifierField;

    private int quantityField;

    private string vendorItemNumberField;

    private string wareHouseCodeField;

    private ChangeType changeTypeField;

    /// <remarks/>
    public CustomerReference CustomerReference
    {
      get
      {
        return this.customerReferenceField;
      }
      set
      {
        this.customerReferenceField = value;
      }
    }

    /// <remarks/>
    public ProductIdentifier ProductIdentifier
    {
      get
      {
        return this.productIdentifierField;
      }
      set
      {
        this.productIdentifierField = value;
      }
    }

    /// <remarks/>
    public int Quantity
    {
      get
      {
        return this.quantityField;
      }
      set
      {
        this.quantityField = value;
      }
    }

    /// <remarks/>
    public string VendorItemNumber
    {
      get
      {
        return this.vendorItemNumberField;
      }
      set
      {
        this.vendorItemNumberField = value;
      }
    }

    /// <remarks/>
    public string WareHouseCode
    {
      get
      {
        return this.wareHouseCodeField;
      }
      set
      {
        this.wareHouseCodeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ChangeType ChangeType
    {
      get
      {
        return this.changeTypeField;
      }
      set
      {
        this.changeTypeField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  public enum ChangeType
  {

    /// <remarks/>
    Change,

    /// <remarks/>
    Delete,

    /// <remarks/>
    Add,

    Unkown
  }
}