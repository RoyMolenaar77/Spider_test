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

namespace Concentrator.Objects.Ordering.XmlClasses
{

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class XML_order
  {

    private orderheader orderheaderField;

    private orderline[] orderlineField;

    private string documentsourceField;

    private string external_document_idField;

    private string supplierField;

    private string warehouse_locationField;

    /// <remarks/>
    public orderheader orderheader
    {
      get
      {
        return this.orderheaderField;
      }
      set
      {
        this.orderheaderField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("orderline")]
    public orderline[] orderline
    {
      get
      {
        return this.orderlineField;
      }
      set
      {
        this.orderlineField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string documentsource
    {
      get
      {
        return this.documentsourceField;
      }
      set
      {
        this.documentsourceField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string external_document_id
    {
      get
      {
        return this.external_document_idField;
      }
      set
      {
        this.external_document_idField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string supplier
    {
      get
      {
        return this.supplierField;
      }
      set
      {
        this.supplierField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string warehouse_location
    {
      get
      {
        return this.warehouse_locationField;
      }
      set
      {
        this.warehouse_locationField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class orderheader
  {

    private Customer customerField;

    private ShipTo shipToField;

    private string transportCodeField;

    private notification notificationField;

    private License_data license_dataField;

    private ordertext[] ordertextField;

    private string sender_idField;

    private string ordertypeField;

    private string customer_ordernumberField;

    private string testflagField;

    private string orderdateField;

    private string completedeliveryField;

    private string requested_deliverydateField;

    private string recipientsreferenceField;

    /// <remarks/>
    public Customer Customer
    {
      get
      {
        return this.customerField;
      }
      set
      {
        this.customerField = value;
      }
    }

    /// <remarks/>
    public ShipTo ShipTo
    {
      get
      {
        return this.shipToField;
      }
      set
      {
        this.shipToField = value;
      }
    }

    /// <remarks/>
    public string TransportCode
    {
      get
      {
        return this.transportCodeField;
      }
      set
      {
        this.transportCodeField = value;
      }
    }

    /// <remarks/>
    public notification notification
    {
      get
      {
        return this.notificationField;
      }
      set
      {
        this.notificationField = value;
      }
    }

    /// <remarks/>
    public License_data License_data
    {
      get
      {
        return this.license_dataField;
      }
      set
      {
        this.license_dataField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ordertext")]
    public ordertext[] ordertext
    {
      get
      {
        return this.ordertextField;
      }
      set
      {
        this.ordertextField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string sender_id
    {
      get
      {
        return this.sender_idField;
      }
      set
      {
        this.sender_idField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ordertype
    {
      get
      {
        return this.ordertypeField;
      }
      set
      {
        this.ordertypeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string customer_ordernumber
    {
      get
      {
        return this.customer_ordernumberField;
      }
      set
      {
        this.customer_ordernumberField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string testflag
    {
      get
      {
        return this.testflagField;
      }
      set
      {
        this.testflagField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string orderdate
    {
      get
      {
        return this.orderdateField;
      }
      set
      {
        this.orderdateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string completedelivery
    {
      get
      {
        return this.completedeliveryField;
      }
      set
      {
        this.completedeliveryField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string requested_deliverydate
    {
      get
      {
        return this.requested_deliverydateField;
      }
      set
      {
        this.requested_deliverydateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string recipientsreference
    {
      get
      {
        return this.recipientsreferenceField;
      }
      set
      {
        this.recipientsreferenceField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class Customer
  {

    private string customeridField;

    private customercontact customercontactField;

    /// <remarks/>
    public string customerid
    {
      get
      {
        return this.customeridField;
      }
      set
      {
        this.customeridField = value;
      }
    }

    /// <remarks/>
    public customercontact customercontact
    {
      get
      {
        return this.customercontactField;
      }
      set
      {
        this.customercontactField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class customercontact
  {

    private string emailField;

    private string telephoneField;

    private string faxField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("e-mail")]
    public string email
    {
      get
      {
        return this.emailField;
      }
      set
      {
        this.emailField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string telephone
    {
      get
      {
        return this.telephoneField;
      }
      set
      {
        this.telephoneField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string fax
    {
      get
      {
        return this.faxField;
      }
      set
      {
        this.faxField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class ShipTo
  {

    private object[] itemsField;

    private ItemsChoiceType[] itemsElementNameField;

    private string cOD_amountField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("addresscode", typeof(string))]
    [System.Xml.Serialization.XmlElementAttribute("addresstype", typeof(string))]
    [System.Xml.Serialization.XmlElementAttribute("adress", typeof(adress))]
    [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
    public object[] Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public ItemsChoiceType[] ItemsElementName
    {
      get
      {
        return this.itemsElementNameField;
      }
      set
      {
        this.itemsElementNameField = value;
      }
    }

    /// <remarks/>
    public string COD_amount
    {
      get
      {
        return this.cOD_amountField;
      }
      set
      {
        this.cOD_amountField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class adress
  {

    private string name1Field;

    private string name2Field;

    private string name3Field;

    private string name4Field;

    private string streetField;

    private string postalcodeField;

    private string cityField;

    private string countryField;

    private contact contactField;

    /// <remarks/>
    public string name1
    {
      get
      {
        return this.name1Field;
      }
      set
      {
        this.name1Field = value;
      }
    }

    /// <remarks/>
    public string name2
    {
      get
      {
        return this.name2Field;
      }
      set
      {
        this.name2Field = value;
      }
    }

    /// <remarks/>
    public string name3
    {
      get
      {
        return this.name3Field;
      }
      set
      {
        this.name3Field = value;
      }
    }

    /// <remarks/>
    public string name4
    {
      get
      {
        return this.name4Field;
      }
      set
      {
        this.name4Field = value;
      }
    }

    /// <remarks/>
    public string street
    {
      get
      {
        return this.streetField;
      }
      set
      {
        this.streetField = value;
      }
    }

    /// <remarks/>
    public string postalcode
    {
      get
      {
        return this.postalcodeField;
      }
      set
      {
        this.postalcodeField = value;
      }
    }

    /// <remarks/>
    public string city
    {
      get
      {
        return this.cityField;
      }
      set
      {
        this.cityField = value;
      }
    }

    /// <remarks/>
    public string country
    {
      get
      {
        return this.countryField;
      }
      set
      {
        this.countryField = value;
      }
    }

    /// <remarks/>
    public contact contact
    {
      get
      {
        return this.contactField;
      }
      set
      {
        this.contactField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class contact
  {

    private string emailField;

    private string telephoneField;

    private string faxField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("e-mail")]
    public string email
    {
      get
      {
        return this.emailField;
      }
      set
      {
        this.emailField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string telephone
    {
      get
      {
        return this.telephoneField;
      }
      set
      {
        this.telephoneField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string fax
    {
      get
      {
        return this.faxField;
      }
      set
      {
        this.faxField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "", IncludeInSchema = false)]
  public enum ItemsChoiceType
  {

    /// <remarks/>
    addresscode,

    /// <remarks/>
    addresstype,

    /// <remarks/>
    adress,
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class notification
  {

    private string typeField;

    private string miscField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
      get
      {
        return this.typeField;
      }
      set
      {
        this.typeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string misc
    {
      get
      {
        return this.miscField;
      }
      set
      {
        this.miscField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class License_data
  {

    private string end_user_company_nameField;

    private end_user_contact end_user_contactField;

    private string end_user_nameField;

    private string end_user_addressField;

    private string end_user_postalcodeField;

    private string end_user_cityField;

    private string end_user_countrycodeField;

    private string end_user_vatnrField;

    private contract_type contract_typeField;

    /// <remarks/>
    public string end_user_company_name
    {
      get
      {
        return this.end_user_company_nameField;
      }
      set
      {
        this.end_user_company_nameField = value;
      }
    }

    /// <remarks/>
    public end_user_contact end_user_contact
    {
      get
      {
        return this.end_user_contactField;
      }
      set
      {
        this.end_user_contactField = value;
      }
    }

    /// <remarks/>
    public string end_user_name
    {
      get
      {
        return this.end_user_nameField;
      }
      set
      {
        this.end_user_nameField = value;
      }
    }

    /// <remarks/>
    public string end_user_address
    {
      get
      {
        return this.end_user_addressField;
      }
      set
      {
        this.end_user_addressField = value;
      }
    }

    /// <remarks/>
    public string end_user_postalcode
    {
      get
      {
        return this.end_user_postalcodeField;
      }
      set
      {
        this.end_user_postalcodeField = value;
      }
    }

    /// <remarks/>
    public string end_user_city
    {
      get
      {
        return this.end_user_cityField;
      }
      set
      {
        this.end_user_cityField = value;
      }
    }

    /// <remarks/>
    public string end_user_countrycode
    {
      get
      {
        return this.end_user_countrycodeField;
      }
      set
      {
        this.end_user_countrycodeField = value;
      }
    }

    /// <remarks/>
    public string end_user_vatnr
    {
      get
      {
        return this.end_user_vatnrField;
      }
      set
      {
        this.end_user_vatnrField = value;
      }
    }

    /// <remarks/>
    public contract_type contract_type
    {
      get
      {
        return this.contract_typeField;
      }
      set
      {
        this.contract_typeField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class end_user_contact
  {

    private string faxField;

    private string telField;

    private string emailField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string fax
    {
      get
      {
        return this.faxField;
      }
      set
      {
        this.faxField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string tel
    {
      get
      {
        return this.telField;
      }
      set
      {
        this.telField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string email
    {
      get
      {
        return this.emailField;
      }
      set
      {
        this.emailField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class contract_type
  {

    private string typeField;

    private string misc1Field;

    private string misc2Field;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string type
    {
      get
      {
        return this.typeField;
      }
      set
      {
        this.typeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string misc1
    {
      get
      {
        return this.misc1Field;
      }
      set
      {
        this.misc1Field = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string misc2
    {
      get
      {
        return this.misc2Field;
      }
      set
      {
        this.misc2Field = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class ordertext
  {

    private string textqualifierField;

    private string textField;

    /// <remarks/>
    public string textqualifier
    {
      get
      {
        return this.textqualifierField;
      }
      set
      {
        this.textqualifierField = value;
      }
    }

    /// <remarks/>
    public string text
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

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class orderline
  {

    private string linenumberField;

    private item_id[] item_idField;

    private quantity quantityField;

    private string deliverydateField;

    private price priceField;

    private string item_descriptionField;

    private orderlinetext[] orderlinetextField;

    /// <remarks/>
    public string linenumber
    {
      get
      {
        return this.linenumberField;
      }
      set
      {
        this.linenumberField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("item_id")]
    public item_id[] item_id
    {
      get
      {
        return this.item_idField;
      }
      set
      {
        this.item_idField = value;
      }
    }

    /// <remarks/>
    public quantity quantity
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
    public string deliverydate
    {
      get
      {
        return this.deliverydateField;
      }
      set
      {
        this.deliverydateField = value;
      }
    }

    /// <remarks/>
    public price price
    {
      get
      {
        return this.priceField;
      }
      set
      {
        this.priceField = value;
      }
    }

    /// <remarks/>
    public string item_description
    {
      get
      {
        return this.item_descriptionField;
      }
      set
      {
        this.item_descriptionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("orderlinetext")]
    public orderlinetext[] orderlinetext
    {
      get
      {
        return this.orderlinetextField;
      }
      set
      {
        this.orderlinetextField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class item_id
  {

    private string tagField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string tag
    {
      get
      {
        return this.tagField;
      }
      set
      {
        this.tagField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class quantity
  {

    private string unitField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string unit
    {
      get
      {
        return this.unitField;
      }
      set
      {
        this.unitField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class price
  {

    private string currencyField;

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string currency
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
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
      get
      {
        return this.valueField;
      }
      set
      {
        this.valueField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class orderlinetext
  {

    private string textqualifierField;

    private string textField;

    /// <remarks/>
    public string textqualifier
    {
      get
      {
        return this.textqualifierField;
      }
      set
      {
        this.textqualifierField = value;
      }
    }

    /// <remarks/>
    public string text
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