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
namespace Concentrator.Plugins.EDI.JDE.Purchase
{

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class PurchaseAcknowledgement
  {

    private ConfirmationLine[] purchaseAcknowledgementLineField;

    private string versionField;

    private string bskIdentifierField;

    private string purchaseOrderNumberField;

    private string referenceField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
    public ConfirmationLine[] PurchaseAcknowledgementLine
    {
      get
      {
        return this.purchaseAcknowledgementLineField;
      }
      set
      {
        this.purchaseAcknowledgementLineField = value;
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
    public string Reference
    {
      get
      {
        return this.referenceField;
      }
      set
      {
        this.referenceField = value;
      }
    }
  }
}