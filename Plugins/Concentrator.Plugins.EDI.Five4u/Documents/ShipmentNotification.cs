﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
public partial class ShippingNotification {
    
    private ShippingNotificationLine[] itemsField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Line", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ShippingNotificationLine[] Items {
        get {
            return this.itemsField;
        }
        set {
            this.itemsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ShippingNotificationLine {
    
    private string orderNumberCustomerField;
    
    private string deliveryDateField;
    
    private string lineNumberCustomerField;
    
    private string productNumberPartnerField;
    
    private string deliveredQuantityField;
    
    private ShippingNotificationLineShipDetails[] shipDetailsField;
    
    private string orderNumberPartnerField;
    
    private string despatchMessageDateField;
    
    private string productNumberCustomerField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string OrderNumberCustomer {
        get {
            return this.orderNumberCustomerField;
        }
        set {
            this.orderNumberCustomerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string DeliveryDate {
        get {
            return this.deliveryDateField;
        }
        set {
            this.deliveryDateField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string LineNumberCustomer {
        get {
            return this.lineNumberCustomerField;
        }
        set {
            this.lineNumberCustomerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string ProductNumberPartner {
        get {
            return this.productNumberPartnerField;
        }
        set {
            this.productNumberPartnerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string DeliveredQuantity {
        get {
            return this.deliveredQuantityField;
        }
        set {
            this.deliveredQuantityField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ShipDetails", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ShippingNotificationLineShipDetails[] ShipDetails {
        get {
            return this.shipDetailsField;
        }
        set {
            this.shipDetailsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string OrderNumberPartner {
        get {
            return this.orderNumberPartnerField;
        }
        set {
            this.orderNumberPartnerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string DespatchMessageDate {
        get {
            return this.despatchMessageDateField;
        }
        set {
            this.despatchMessageDateField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ProductNumberCustomer {
        get {
            return this.productNumberCustomerField;
        }
        set {
            this.productNumberCustomerField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
public partial class ShippingNotificationLineShipDetails {
    
    private string deliveryNumberField;
    
    private string deliveryNumberLineNumberField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string DeliveryNumber {
        get {
            return this.deliveryNumberField;
        }
        set {
            this.deliveryNumberField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string DeliveryNumberLineNumber {
        get {
            return this.deliveryNumberLineNumberField;
        }
        set {
            this.deliveryNumberLineNumberField = value;
        }
    }
}
