﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Concentrator.Payment.Providers.AfterPayMerchant {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CreateInvoice", Namespace="http://afterpay.nl/services")]
    [System.SerializableAttribute()]
    public partial class CreateInvoice : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string UserIdField;
        
        private string PasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int PortfolioField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string OrderNumberField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string InvoiceNumberField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime InvoiceDateField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Concentrator.Payment.Providers.AfterPayMerchant.InvoiceLine[] InvoiceLinesField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public string UserId {
            get {
                return this.UserIdField;
            }
            set {
                if ((object.ReferenceEquals(this.UserIdField, value) != true)) {
                    this.UserIdField = value;
                    this.RaisePropertyChanged("UserId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=1)]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=2)]
        public int Portfolio {
            get {
                return this.PortfolioField;
            }
            set {
                if ((this.PortfolioField.Equals(value) != true)) {
                    this.PortfolioField = value;
                    this.RaisePropertyChanged("Portfolio");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=3)]
        public string OrderNumber {
            get {
                return this.OrderNumberField;
            }
            set {
                if ((object.ReferenceEquals(this.OrderNumberField, value) != true)) {
                    this.OrderNumberField = value;
                    this.RaisePropertyChanged("OrderNumber");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=4)]
        public string InvoiceNumber {
            get {
                return this.InvoiceNumberField;
            }
            set {
                if ((object.ReferenceEquals(this.InvoiceNumberField, value) != true)) {
                    this.InvoiceNumberField = value;
                    this.RaisePropertyChanged("InvoiceNumber");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=5)]
        public System.DateTime InvoiceDate {
            get {
                return this.InvoiceDateField;
            }
            set {
                if ((this.InvoiceDateField.Equals(value) != true)) {
                    this.InvoiceDateField = value;
                    this.RaisePropertyChanged("InvoiceDate");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=6)]
        public Concentrator.Payment.Providers.AfterPayMerchant.InvoiceLine[] InvoiceLines {
            get {
                return this.InvoiceLinesField;
            }
            set {
                if ((object.ReferenceEquals(this.InvoiceLinesField, value) != true)) {
                    this.InvoiceLinesField = value;
                    this.RaisePropertyChanged("InvoiceLines");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="InvoiceLine", Namespace="http://afterpay.nl/services")]
    [System.SerializableAttribute()]
    public partial class InvoiceLine : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<int> LineNumberField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private decimal UnitPriceInVatField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int QuantityField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> LineNumber {
            get {
                return this.LineNumberField;
            }
            set {
                if ((this.LineNumberField.Equals(value) != true)) {
                    this.LineNumberField = value;
                    this.RaisePropertyChanged("LineNumber");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public decimal UnitPriceInVat {
            get {
                return this.UnitPriceInVatField;
            }
            set {
                if ((this.UnitPriceInVatField.Equals(value) != true)) {
                    this.UnitPriceInVatField = value;
                    this.RaisePropertyChanged("UnitPriceInVat");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=2)]
        public int Quantity {
            get {
                return this.QuantityField;
            }
            set {
                if ((this.QuantityField.Equals(value) != true)) {
                    this.QuantityField = value;
                    this.RaisePropertyChanged("Quantity");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CreateInvoiceResponse", Namespace="http://afterpay.nl/services")]
    [System.SerializableAttribute()]
    public partial class CreateInvoiceResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool SuccessField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorMessageField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool Success {
            get {
                return this.SuccessField;
            }
            set {
                if ((this.SuccessField.Equals(value) != true)) {
                    this.SuccessField = value;
                    this.RaisePropertyChanged("Success");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(Order=1)]
        public string ErrorMessage {
            get {
                return this.ErrorMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.ErrorMessageField, value) != true)) {
                    this.ErrorMessageField = value;
                    this.RaisePropertyChanged("ErrorMessage");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="AfterPayMerchant.IMerchant")]
    public interface IMerchant {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMerchant/CreateInvoice", ReplyAction="http://tempuri.org/IMerchant/CreateInvoiceResponse")]
        Concentrator.Payment.Providers.AfterPayMerchant.CreateInvoiceResponse CreateInvoice(Concentrator.Payment.Providers.AfterPayMerchant.CreateInvoice request);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMerchantChannel : Concentrator.Payment.Providers.AfterPayMerchant.IMerchant, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MerchantClient : System.ServiceModel.ClientBase<Concentrator.Payment.Providers.AfterPayMerchant.IMerchant>, Concentrator.Payment.Providers.AfterPayMerchant.IMerchant {
        
        public MerchantClient() {
        }
        
        public MerchantClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MerchantClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MerchantClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MerchantClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Concentrator.Payment.Providers.AfterPayMerchant.CreateInvoiceResponse CreateInvoice(Concentrator.Payment.Providers.AfterPayMerchant.CreateInvoice request) {
            return base.Channel.CreateInvoice(request);
        }
    }
}
