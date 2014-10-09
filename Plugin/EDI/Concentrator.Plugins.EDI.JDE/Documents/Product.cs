﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=2.0.50727.1432.
// 
namespace Concentrator.Plugins.EDI.JDE.Product
{

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class ProductRequest
  {

    private Product[] productsField;

    private string versionField;

    private string bskIdentifierField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
    public Product[] Products
    {
      get
      {
        return this.productsField;
      }
      set
      {
        this.productsField = value;
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
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  public partial class Product
  {

    private string vendorItemNumberField;

    private string modelNumberField;

    private string descriptionField;

    private string description2Field;

    private ProductUnitPrice unitPriceField;

    private decimal unitCostField;

    private string uPCField;

    private string eANField;

    private string parentProductGroupCodeField;

    private string productGroupCodeField;

    private string brandCodeField;

    private int templateItem;

    private string landedCostRuleField;

    private string buyerNumberField;

    private string plannerNumberField;

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
    public string ModelNumber
    {
      get
      {
        return this.modelNumberField;
      }
      set
      {
        this.modelNumberField = value;
      }
    }

    /// <remarks/>
    public string Description
    {
      get
      {
        return this.descriptionField;
      }
      set
      {
        this.descriptionField = value;
      }
    }

    /// <remarks/>
    public string Description2
    {
      get
      {
        return this.description2Field;
      }
      set
      {
        this.description2Field = value;
      }
    }

    /// <remarks/>
    public ProductUnitPrice UnitPrice
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
    public decimal UnitCost
    {
      get
      {
        return this.unitCostField;
      }
      set
      {
        this.unitCostField = value;
      }
    }

    /// <remarks/>
    public string UPC
    {
      get
      {
        return this.uPCField;
      }
      set
      {
        this.uPCField = value;
      }
    }

    /// <remarks/>
    public string EAN
    {
      get
      {
        return this.eANField;
      }
      set
      {
        this.eANField = value;
      }
    }

    /// <remarks/>
    public string ParentProductGroupCode
    {
      get
      {
        return this.parentProductGroupCodeField;
      }
      set
      {
        this.parentProductGroupCodeField = value;
      }
    }

    /// <remarks/>
    public string ProductGroupCode
    {
      get
      {
        return this.productGroupCodeField;
      }
      set
      {
        this.productGroupCodeField = value;
      }
    }

    /// <remarks/>
    public string BrandCode
    {
      get
      {
        return this.brandCodeField;
      }
      set
      {
        this.brandCodeField = value;
      }
    }

    /// <remarks/>
    public int TemplateItem
    {
      get
      {
        return this.templateItem;
      }
      set
      {
        this.templateItem = value;
      }
    }

    /// <remarks/>
    public string LandedCostRule
    {
      get
      {
        return this.landedCostRuleField;
      }
      set
      {
        this.landedCostRuleField = value;
      }
    }

    /// <remarks/>
    public string BuyerNumber
    {
      get
      {
        return this.buyerNumberField;
      }
      set
      {
        this.buyerNumberField = value;
      }
    }

    /// <remarks/>
    public string PlannerNumber
    {
      get
      {
        return this.plannerNumberField;
      }
      set
      {
        this.plannerNumberField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductUnitPrice
  {

    private bool highTaxRateField;

    private bool lowTaxRateField;

    private string[] textField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool HighTaxRate
    {
      get
      {
        return this.highTaxRateField;
      }
      set
      {
        this.highTaxRateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool LowTaxRate
    {
      get
      {
        return this.lowTaxRateField;
      }
      set
      {
        this.lowTaxRateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string[] Text
    {
      get
      {
        return this.textField;
      }
      set
      {
        this.textField = value;
      }
    }
  }
}