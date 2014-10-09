﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Concentrator.Plugins.Aggregator.JDECrossReference {
    using System.Data;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://localhost/BASConnector/", ConfigurationName="JDECrossReference.JdeAssortmentSoap")]
    public interface JdeAssortmentSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GenerateStockProductList", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GenerateStockProductList(int customerID, int internetAss, int allowDC10O);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GenerateFullProductList", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GenerateFullProductList(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, bool bscstock, bool shopInfo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GenerateFullProductListWithNonStock", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GenerateFullProductListWithNonStock(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, bool bscstock, bool shopInfo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GenerateFullProductListWithRetailStock", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GenerateFullProductListWithRetailStock(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GenerateFullProductListSpecialAssortment", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GenerateFullProductListSpecialAssortment(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, int allowNonStock, string bscStock, string costPriceDC10, string costPriceBSC, bool onlyItemsWithStock);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GetSingeProductInformation", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GetSingeProductInformation(int customerID, string productid);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GetSingleShopProductInformation", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GetSingleShopProductInformation(int customerID, string productid, bool shopInformation);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GenerateFullItemList", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GenerateFullItemList();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://localhost/BASConnector/GetCrossItemNumbers", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GetCrossItemNumbers();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface JdeAssortmentSoapChannel : Concentrator.Plugins.Aggregator.JDECrossReference.JdeAssortmentSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class JdeAssortmentSoapClient : System.ServiceModel.ClientBase<Concentrator.Plugins.Aggregator.JDECrossReference.JdeAssortmentSoap>, Concentrator.Plugins.Aggregator.JDECrossReference.JdeAssortmentSoap {
        
        public JdeAssortmentSoapClient() {
        }
        
        public JdeAssortmentSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public JdeAssortmentSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public JdeAssortmentSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public JdeAssortmentSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Data.DataSet GenerateStockProductList(int customerID, int internetAss, int allowDC10O) {
            return base.Channel.GenerateStockProductList(customerID, internetAss, allowDC10O);
        }
        
        public System.Data.DataSet GenerateFullProductList(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, bool bscstock, bool shopInfo) {
            return base.Channel.GenerateFullProductList(customerID, internetAss, allowDC10O, allowDC10ObyStatus, bscstock, shopInfo);
        }
        
        public System.Data.DataSet GenerateFullProductListWithNonStock(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, bool bscstock, bool shopInfo) {
            return base.Channel.GenerateFullProductListWithNonStock(customerID, internetAss, allowDC10O, allowDC10ObyStatus, bscstock, shopInfo);
        }
        
        public System.Data.DataSet GenerateFullProductListWithRetailStock(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus) {
            return base.Channel.GenerateFullProductListWithRetailStock(customerID, internetAss, allowDC10O, allowDC10ObyStatus);
        }
        
        public System.Data.DataSet GenerateFullProductListSpecialAssortment(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, int allowNonStock, string bscStock, string costPriceDC10, string costPriceBSC, bool onlyItemsWithStock) {
            return base.Channel.GenerateFullProductListSpecialAssortment(customerID, internetAss, allowDC10O, allowDC10ObyStatus, allowNonStock, bscStock, costPriceDC10, costPriceBSC, onlyItemsWithStock);
        }
        
        public System.Data.DataSet GetSingeProductInformation(int customerID, string productid) {
            return base.Channel.GetSingeProductInformation(customerID, productid);
        }
        
        public System.Data.DataSet GetSingleShopProductInformation(int customerID, string productid, bool shopInformation) {
            return base.Channel.GetSingleShopProductInformation(customerID, productid, shopInformation);
        }
        
        public System.Data.DataSet GenerateFullItemList() {
            return base.Channel.GenerateFullItemList();
        }
        
        public System.Data.DataSet GetCrossItemNumbers() {
            return base.Channel.GetCrossItemNumbers();
        }
    }
}