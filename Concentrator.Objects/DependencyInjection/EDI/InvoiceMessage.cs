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
namespace Concentrator.Web.Objects.EDI
{

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class InvoiceMessage
  {

    private InvoiceLine[] invoiceLinesField;

    private string versionField;

    private string invoiceNumberField;

    private System.DateTime invoiceDateField;

    private string supplierNumberField;

    private string currencyField;

    private string paymentInstrumentField;

    private string paymentTypeField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
    public InvoiceLine[] InvoiceLines
    {
      get
      {
        return this.invoiceLinesField;
      }
      set
      {
        this.invoiceLinesField = value;
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
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
    public string InvoiceNumber
    {
      get
      {
        return this.invoiceNumberField;
      }
      set
      {
        this.invoiceNumberField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public System.DateTime InvoiceDate
    {
      get
      {
        return this.invoiceDateField;
      }
      set
      {
        this.invoiceDateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
    public string SupplierNumber
    {
      get
      {
        return this.supplierNumberField;
      }
      set
      {
        this.supplierNumberField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Currency
    {
      get
      {
        return this.currencyField;
      }
      set
      {
        this.currencyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string PaymentType
    {
      get
      {
        return this.paymentTypeField;
      }
      set
      {
        this.paymentTypeField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class InvoiceLine
  {

    private string itemNumberField;

    private string vendorItemNumberField;

    private decimal unitPriceField;

    private decimal extendedPriceField;

    private decimal amountOpenField;

    private int quantityOpenField;

    private string purchaseOrderNumberField;

    private string bskIdentifierField;

    private bool additionalLineField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "boolean")]
    public bool AdditionalLine
    {
      get
      {
        return this.additionalLineField;
      }
      set
      {
        this.additionalLineField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
    public string bskIdentifier
    {
      get
      {
        return this.bskIdentifierField;
      }
      set
      {
        this.bskIdentifierField = value;
      }
    }

    /// <remarks/>
    public string ItemNumber
    {
      get
      {
        return this.itemNumberField;
      }
      set
      {
        this.itemNumberField = value;
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
    public decimal UnitPrice
    {
      get
      {
        return this.unitPriceField;
      }
      set
      {
        this.unitPriceField = value;
      }
    }

    /// <remarks/>
    public decimal ExtendedPrice
    {
      get
      {
        return this.extendedPriceField;
      }
      set
      {
        this.extendedPriceField = value;
      }
    }

    /// <remarks/>
    public decimal AmountOpen
    {
      get
      {
        return this.amountOpenField;
      }
      set
      {
        this.amountOpenField = value;
      }
    }

    /// <remarks/>
    public int QuantityOpen
    {
      get
      {
        return this.quantityOpenField;
      }
      set
      {
        this.quantityOpenField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
    public string PurchaseOrderNumber
    {
      get
      {
        return this.purchaseOrderNumberField;
      }
      set
      {
        this.purchaseOrderNumberField = value;
      }
    }
  }
}