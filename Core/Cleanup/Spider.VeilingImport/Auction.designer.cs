﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Spider.VeilingImport
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[System.Data.Linq.Mapping.DatabaseAttribute(Name="Bram_Dev_Test")]
	public partial class AuctionDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertAuctionProduct(AuctionProduct instance);
    partial void UpdateAuctionProduct(AuctionProduct instance);
    partial void DeleteAuctionProduct(AuctionProduct instance);
    partial void InsertProduct(Product instance);
    partial void UpdateProduct(Product instance);
    partial void DeleteProduct(Product instance);
    partial void InsertBrand(Brand instance);
    partial void UpdateBrand(Brand instance);
    partial void DeleteBrand(Brand instance);
    partial void InsertTaxRate(TaxRate instance);
    partial void UpdateTaxRate(TaxRate instance);
    partial void DeleteTaxRate(TaxRate instance);
    #endregion
		
		public AuctionDataContext() : 
				base(global::Spider.VeilingImport.Properties.Settings.Default.Bram_Dev_TestConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public AuctionDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AuctionDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AuctionDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public AuctionDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<AuctionProduct> AuctionProducts
		{
			get
			{
				return this.GetTable<AuctionProduct>();
			}
		}
		
		public System.Data.Linq.Table<Product> Products
		{
			get
			{
				return this.GetTable<Product>();
			}
		}
		
		public System.Data.Linq.Table<Brand> Brands
		{
			get
			{
				return this.GetTable<Brand>();
			}
		}
		
		public System.Data.Linq.Table<TaxRate> TaxRates
		{
			get
			{
				return this.GetTable<TaxRate>();
			}
		}
	}
	
	[Table(Name="dbo.AuctionProducts")]
	public partial class AuctionProduct : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _AuctionProductID;
		
		private int _AuctionBSCStock;
		
		private int _AuctionOEMStock;
		
		private int _AuctionDC10Stock;
		
		private string _StockStatus;
		
		private System.Nullable<int> _QuantityToReceive;
		
		private decimal _DC10CostPrice;
		
		private decimal _BSCCostPrice;
		
		private EntityRef<Product> _Product;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnAuctionProductIDChanging(int value);
    partial void OnAuctionProductIDChanged();
    partial void OnAuctionBSCStockChanging(int value);
    partial void OnAuctionBSCStockChanged();
    partial void OnAuctionOEMStockChanging(int value);
    partial void OnAuctionOEMStockChanged();
    partial void OnAuctionDC10StockChanging(int value);
    partial void OnAuctionDC10StockChanged();
    partial void OnStockStatusChanging(string value);
    partial void OnStockStatusChanged();
    partial void OnQuantityToReceiveChanging(System.Nullable<int> value);
    partial void OnQuantityToReceiveChanged();
    partial void OnDC10CostPriceChanging(decimal value);
    partial void OnDC10CostPriceChanged();
    partial void OnBSCCostPriceChanging(decimal value);
    partial void OnBSCCostPriceChanged();
    #endregion
		
		public AuctionProduct()
		{
			this._Product = default(EntityRef<Product>);
			OnCreated();
		}
		
		[Column(Storage="_AuctionProductID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int AuctionProductID
		{
			get
			{
				return this._AuctionProductID;
			}
			set
			{
				if ((this._AuctionProductID != value))
				{
					if (this._Product.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnAuctionProductIDChanging(value);
					this.SendPropertyChanging();
					this._AuctionProductID = value;
					this.SendPropertyChanged("AuctionProductID");
					this.OnAuctionProductIDChanged();
				}
			}
		}
		
		[Column(Storage="_AuctionBSCStock", DbType="Int NOT NULL")]
		public int AuctionBSCStock
		{
			get
			{
				return this._AuctionBSCStock;
			}
			set
			{
				if ((this._AuctionBSCStock != value))
				{
					this.OnAuctionBSCStockChanging(value);
					this.SendPropertyChanging();
					this._AuctionBSCStock = value;
					this.SendPropertyChanged("AuctionBSCStock");
					this.OnAuctionBSCStockChanged();
				}
			}
		}
		
		[Column(Storage="_AuctionOEMStock", DbType="Int NOT NULL")]
		public int AuctionOEMStock
		{
			get
			{
				return this._AuctionOEMStock;
			}
			set
			{
				if ((this._AuctionOEMStock != value))
				{
					this.OnAuctionOEMStockChanging(value);
					this.SendPropertyChanging();
					this._AuctionOEMStock = value;
					this.SendPropertyChanged("AuctionOEMStock");
					this.OnAuctionOEMStockChanged();
				}
			}
		}
		
		[Column(Storage="_AuctionDC10Stock", DbType="Int NOT NULL")]
		public int AuctionDC10Stock
		{
			get
			{
				return this._AuctionDC10Stock;
			}
			set
			{
				if ((this._AuctionDC10Stock != value))
				{
					this.OnAuctionDC10StockChanging(value);
					this.SendPropertyChanging();
					this._AuctionDC10Stock = value;
					this.SendPropertyChanged("AuctionDC10Stock");
					this.OnAuctionDC10StockChanged();
				}
			}
		}
		
		[Column(Storage="_StockStatus", DbType="NVarChar(2) NOT NULL", CanBeNull=false)]
		public string StockStatus
		{
			get
			{
				return this._StockStatus;
			}
			set
			{
				if ((this._StockStatus != value))
				{
					this.OnStockStatusChanging(value);
					this.SendPropertyChanging();
					this._StockStatus = value;
					this.SendPropertyChanged("StockStatus");
					this.OnStockStatusChanged();
				}
			}
		}
		
		[Column(Storage="_QuantityToReceive", DbType="Int")]
		public System.Nullable<int> QuantityToReceive
		{
			get
			{
				return this._QuantityToReceive;
			}
			set
			{
				if ((this._QuantityToReceive != value))
				{
					this.OnQuantityToReceiveChanging(value);
					this.SendPropertyChanging();
					this._QuantityToReceive = value;
					this.SendPropertyChanged("QuantityToReceive");
					this.OnQuantityToReceiveChanged();
				}
			}
		}
		
		[Column(Storage="_DC10CostPrice", DbType="Decimal(18,4) NOT NULL")]
		public decimal DC10CostPrice
		{
			get
			{
				return this._DC10CostPrice;
			}
			set
			{
				if ((this._DC10CostPrice != value))
				{
					this.OnDC10CostPriceChanging(value);
					this.SendPropertyChanging();
					this._DC10CostPrice = value;
					this.SendPropertyChanged("DC10CostPrice");
					this.OnDC10CostPriceChanged();
				}
			}
		}
		
		[Column(Storage="_BSCCostPrice", DbType="Decimal(18,4) NOT NULL")]
		public decimal BSCCostPrice
		{
			get
			{
				return this._BSCCostPrice;
			}
			set
			{
				if ((this._BSCCostPrice != value))
				{
					this.OnBSCCostPriceChanging(value);
					this.SendPropertyChanging();
					this._BSCCostPrice = value;
					this.SendPropertyChanged("BSCCostPrice");
					this.OnBSCCostPriceChanged();
				}
			}
		}
		
		[Association(Name="Product_AuctionProduct", Storage="_Product", ThisKey="AuctionProductID", OtherKey="ProductID", IsForeignKey=true)]
		public Product Product
		{
			get
			{
				return this._Product.Entity;
			}
			set
			{
				Product previousValue = this._Product.Entity;
				if (((previousValue != value) 
							|| (this._Product.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Product.Entity = null;
						previousValue.AuctionProduct = null;
					}
					this._Product.Entity = value;
					if ((value != null))
					{
						value.AuctionProduct = this;
						this._AuctionProductID = value.ProductID;
					}
					else
					{
						this._AuctionProductID = default(int);
					}
					this.SendPropertyChanged("Product");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="dbo.Products")]
	public partial class Product : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _ProductID;
		
		private string _ShortDescription;
		
		private string _LongDescription;
		
		private string _ManufacturerID;
		
		private int _BrandID;
		
		private int _TaxRateID;
		
		private System.Nullable<decimal> _UnitPrice;
		
		private System.Nullable<decimal> _UnitCost;
		
		private string _LedgerClass;
		
		private bool _IsCustom;
		
		private string _LineType;
		
		private string _ProductStatus;
		
		private bool _IsVisible;
		
		private bool _ExtendedCatalog;
		
		private System.Nullable<System.DateTime> _PromisedDeliveryDate;
		
		private bool _CanModifyPrice;
		
		private System.Nullable<int> _CreatedBy;
		
		private System.DateTime _CreationTime;
		
		private System.Nullable<int> _LastModifiedBy;
		
		private System.DateTime _LastModificationTime;
		
		private string _ProductDesk;
		
		private System.Nullable<bool> _PromisedDeliveryDateOverride;
		
		private string _BackEndDescription;
		
		private EntityRef<AuctionProduct> _AuctionProduct;
		
		private EntityRef<Brand> _Brand;
		
		private EntityRef<TaxRate> _TaxRate;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnProductIDChanging(int value);
    partial void OnProductIDChanged();
    partial void OnShortDescriptionChanging(string value);
    partial void OnShortDescriptionChanged();
    partial void OnLongDescriptionChanging(string value);
    partial void OnLongDescriptionChanged();
    partial void OnManufacturerIDChanging(string value);
    partial void OnManufacturerIDChanged();
    partial void OnBrandIDChanging(int value);
    partial void OnBrandIDChanged();
    partial void OnTaxRateIDChanging(int value);
    partial void OnTaxRateIDChanged();
    partial void OnUnitPriceChanging(System.Nullable<decimal> value);
    partial void OnUnitPriceChanged();
    partial void OnUnitCostChanging(System.Nullable<decimal> value);
    partial void OnUnitCostChanged();
    partial void OnLedgerClassChanging(string value);
    partial void OnLedgerClassChanged();
    partial void OnIsCustomChanging(bool value);
    partial void OnIsCustomChanged();
    partial void OnLineTypeChanging(string value);
    partial void OnLineTypeChanged();
    partial void OnProductStatusChanging(string value);
    partial void OnProductStatusChanged();
    partial void OnIsVisibleChanging(bool value);
    partial void OnIsVisibleChanged();
    partial void OnExtendedCatalogChanging(bool value);
    partial void OnExtendedCatalogChanged();
    partial void OnPromisedDeliveryDateChanging(System.Nullable<System.DateTime> value);
    partial void OnPromisedDeliveryDateChanged();
    partial void OnCanModifyPriceChanging(bool value);
    partial void OnCanModifyPriceChanged();
    partial void OnCreatedByChanging(System.Nullable<int> value);
    partial void OnCreatedByChanged();
    partial void OnCreationTimeChanging(System.DateTime value);
    partial void OnCreationTimeChanged();
    partial void OnLastModifiedByChanging(System.Nullable<int> value);
    partial void OnLastModifiedByChanged();
    partial void OnLastModificationTimeChanging(System.DateTime value);
    partial void OnLastModificationTimeChanged();
    partial void OnProductDeskChanging(string value);
    partial void OnProductDeskChanged();
    partial void OnPromisedDeliveryDateOverrideChanging(System.Nullable<bool> value);
    partial void OnPromisedDeliveryDateOverrideChanged();
    partial void OnBackEndDescriptionChanging(string value);
    partial void OnBackEndDescriptionChanged();
    #endregion
		
		public Product()
		{
			this._AuctionProduct = default(EntityRef<AuctionProduct>);
			this._Brand = default(EntityRef<Brand>);
			this._TaxRate = default(EntityRef<TaxRate>);
			OnCreated();
		}
		
		[Column(Storage="_ProductID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ProductID
		{
			get
			{
				return this._ProductID;
			}
			set
			{
				if ((this._ProductID != value))
				{
					this.OnProductIDChanging(value);
					this.SendPropertyChanging();
					this._ProductID = value;
					this.SendPropertyChanged("ProductID");
					this.OnProductIDChanged();
				}
			}
		}
		
		[Column(Storage="_ShortDescription", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string ShortDescription
		{
			get
			{
				return this._ShortDescription;
			}
			set
			{
				if ((this._ShortDescription != value))
				{
					this.OnShortDescriptionChanging(value);
					this.SendPropertyChanging();
					this._ShortDescription = value;
					this.SendPropertyChanged("ShortDescription");
					this.OnShortDescriptionChanged();
				}
			}
		}
		
		[Column(Storage="_LongDescription", DbType="NVarChar(MAX) NOT NULL", CanBeNull=false)]
		public string LongDescription
		{
			get
			{
				return this._LongDescription;
			}
			set
			{
				if ((this._LongDescription != value))
				{
					this.OnLongDescriptionChanging(value);
					this.SendPropertyChanging();
					this._LongDescription = value;
					this.SendPropertyChanged("LongDescription");
					this.OnLongDescriptionChanged();
				}
			}
		}
		
		[Column(Storage="_ManufacturerID", DbType="VarChar(100)")]
		public string ManufacturerID
		{
			get
			{
				return this._ManufacturerID;
			}
			set
			{
				if ((this._ManufacturerID != value))
				{
					this.OnManufacturerIDChanging(value);
					this.SendPropertyChanging();
					this._ManufacturerID = value;
					this.SendPropertyChanged("ManufacturerID");
					this.OnManufacturerIDChanged();
				}
			}
		}
		
		[Column(Storage="_BrandID", DbType="Int NOT NULL")]
		public int BrandID
		{
			get
			{
				return this._BrandID;
			}
			set
			{
				if ((this._BrandID != value))
				{
					if (this._Brand.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnBrandIDChanging(value);
					this.SendPropertyChanging();
					this._BrandID = value;
					this.SendPropertyChanged("BrandID");
					this.OnBrandIDChanged();
				}
			}
		}
		
		[Column(Storage="_TaxRateID", DbType="Int NOT NULL")]
		public int TaxRateID
		{
			get
			{
				return this._TaxRateID;
			}
			set
			{
				if ((this._TaxRateID != value))
				{
					if (this._TaxRate.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnTaxRateIDChanging(value);
					this.SendPropertyChanging();
					this._TaxRateID = value;
					this.SendPropertyChanged("TaxRateID");
					this.OnTaxRateIDChanged();
				}
			}
		}
		
		[Column(Storage="_UnitPrice", DbType="Decimal(18,4)")]
		public System.Nullable<decimal> UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				if ((this._UnitPrice != value))
				{
					this.OnUnitPriceChanging(value);
					this.SendPropertyChanging();
					this._UnitPrice = value;
					this.SendPropertyChanged("UnitPrice");
					this.OnUnitPriceChanged();
				}
			}
		}
		
		[Column(Storage="_UnitCost", DbType="Decimal(18,4)")]
		public System.Nullable<decimal> UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				if ((this._UnitCost != value))
				{
					this.OnUnitCostChanging(value);
					this.SendPropertyChanging();
					this._UnitCost = value;
					this.SendPropertyChanged("UnitCost");
					this.OnUnitCostChanged();
				}
			}
		}
		
		[Column(Storage="_LedgerClass", DbType="VarChar(50)")]
		public string LedgerClass
		{
			get
			{
				return this._LedgerClass;
			}
			set
			{
				if ((this._LedgerClass != value))
				{
					this.OnLedgerClassChanging(value);
					this.SendPropertyChanging();
					this._LedgerClass = value;
					this.SendPropertyChanged("LedgerClass");
					this.OnLedgerClassChanged();
				}
			}
		}
		
		[Column(Storage="_IsCustom", DbType="Bit NOT NULL")]
		public bool IsCustom
		{
			get
			{
				return this._IsCustom;
			}
			set
			{
				if ((this._IsCustom != value))
				{
					this.OnIsCustomChanging(value);
					this.SendPropertyChanging();
					this._IsCustom = value;
					this.SendPropertyChanged("IsCustom");
					this.OnIsCustomChanged();
				}
			}
		}
		
		[Column(Storage="_LineType", DbType="VarChar(4)")]
		public string LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				if ((this._LineType != value))
				{
					this.OnLineTypeChanging(value);
					this.SendPropertyChanging();
					this._LineType = value;
					this.SendPropertyChanged("LineType");
					this.OnLineTypeChanged();
				}
			}
		}
		
		[Column(Storage="_ProductStatus", DbType="VarChar(2)")]
		public string ProductStatus
		{
			get
			{
				return this._ProductStatus;
			}
			set
			{
				if ((this._ProductStatus != value))
				{
					this.OnProductStatusChanging(value);
					this.SendPropertyChanging();
					this._ProductStatus = value;
					this.SendPropertyChanged("ProductStatus");
					this.OnProductStatusChanged();
				}
			}
		}
		
		[Column(Storage="_IsVisible", DbType="Bit NOT NULL")]
		public bool IsVisible
		{
			get
			{
				return this._IsVisible;
			}
			set
			{
				if ((this._IsVisible != value))
				{
					this.OnIsVisibleChanging(value);
					this.SendPropertyChanging();
					this._IsVisible = value;
					this.SendPropertyChanged("IsVisible");
					this.OnIsVisibleChanged();
				}
			}
		}
		
		[Column(Storage="_ExtendedCatalog", DbType="Bit NOT NULL")]
		public bool ExtendedCatalog
		{
			get
			{
				return this._ExtendedCatalog;
			}
			set
			{
				if ((this._ExtendedCatalog != value))
				{
					this.OnExtendedCatalogChanging(value);
					this.SendPropertyChanging();
					this._ExtendedCatalog = value;
					this.SendPropertyChanged("ExtendedCatalog");
					this.OnExtendedCatalogChanged();
				}
			}
		}
		
		[Column(Storage="_PromisedDeliveryDate", DbType="DateTime")]
		public System.Nullable<System.DateTime> PromisedDeliveryDate
		{
			get
			{
				return this._PromisedDeliveryDate;
			}
			set
			{
				if ((this._PromisedDeliveryDate != value))
				{
					this.OnPromisedDeliveryDateChanging(value);
					this.SendPropertyChanging();
					this._PromisedDeliveryDate = value;
					this.SendPropertyChanged("PromisedDeliveryDate");
					this.OnPromisedDeliveryDateChanged();
				}
			}
		}
		
		[Column(Storage="_CanModifyPrice", DbType="Bit NOT NULL")]
		public bool CanModifyPrice
		{
			get
			{
				return this._CanModifyPrice;
			}
			set
			{
				if ((this._CanModifyPrice != value))
				{
					this.OnCanModifyPriceChanging(value);
					this.SendPropertyChanging();
					this._CanModifyPrice = value;
					this.SendPropertyChanged("CanModifyPrice");
					this.OnCanModifyPriceChanged();
				}
			}
		}
		
		[Column(Storage="_CreatedBy", DbType="Int")]
		public System.Nullable<int> CreatedBy
		{
			get
			{
				return this._CreatedBy;
			}
			set
			{
				if ((this._CreatedBy != value))
				{
					this.OnCreatedByChanging(value);
					this.SendPropertyChanging();
					this._CreatedBy = value;
					this.SendPropertyChanged("CreatedBy");
					this.OnCreatedByChanged();
				}
			}
		}
		
		[Column(Storage="_CreationTime", DbType="DateTime NOT NULL")]
		public System.DateTime CreationTime
		{
			get
			{
				return this._CreationTime;
			}
			set
			{
				if ((this._CreationTime != value))
				{
					this.OnCreationTimeChanging(value);
					this.SendPropertyChanging();
					this._CreationTime = value;
					this.SendPropertyChanged("CreationTime");
					this.OnCreationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_LastModifiedBy", DbType="Int")]
		public System.Nullable<int> LastModifiedBy
		{
			get
			{
				return this._LastModifiedBy;
			}
			set
			{
				if ((this._LastModifiedBy != value))
				{
					this.OnLastModifiedByChanging(value);
					this.SendPropertyChanging();
					this._LastModifiedBy = value;
					this.SendPropertyChanged("LastModifiedBy");
					this.OnLastModifiedByChanged();
				}
			}
		}
		
		[Column(Storage="_LastModificationTime", DbType="DateTime NOT NULL")]
		public System.DateTime LastModificationTime
		{
			get
			{
				return this._LastModificationTime;
			}
			set
			{
				if ((this._LastModificationTime != value))
				{
					this.OnLastModificationTimeChanging(value);
					this.SendPropertyChanging();
					this._LastModificationTime = value;
					this.SendPropertyChanged("LastModificationTime");
					this.OnLastModificationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_ProductDesk", DbType="VarChar(50)")]
		public string ProductDesk
		{
			get
			{
				return this._ProductDesk;
			}
			set
			{
				if ((this._ProductDesk != value))
				{
					this.OnProductDeskChanging(value);
					this.SendPropertyChanging();
					this._ProductDesk = value;
					this.SendPropertyChanged("ProductDesk");
					this.OnProductDeskChanged();
				}
			}
		}
		
		[Column(Storage="_PromisedDeliveryDateOverride", DbType="Bit")]
		public System.Nullable<bool> PromisedDeliveryDateOverride
		{
			get
			{
				return this._PromisedDeliveryDateOverride;
			}
			set
			{
				if ((this._PromisedDeliveryDateOverride != value))
				{
					this.OnPromisedDeliveryDateOverrideChanging(value);
					this.SendPropertyChanging();
					this._PromisedDeliveryDateOverride = value;
					this.SendPropertyChanged("PromisedDeliveryDateOverride");
					this.OnPromisedDeliveryDateOverrideChanged();
				}
			}
		}
		
		[Column(Storage="_BackEndDescription", DbType="NVarChar(50)")]
		public string BackEndDescription
		{
			get
			{
				return this._BackEndDescription;
			}
			set
			{
				if ((this._BackEndDescription != value))
				{
					this.OnBackEndDescriptionChanging(value);
					this.SendPropertyChanging();
					this._BackEndDescription = value;
					this.SendPropertyChanged("BackEndDescription");
					this.OnBackEndDescriptionChanged();
				}
			}
		}
		
		[Association(Name="Product_AuctionProduct", Storage="_AuctionProduct", ThisKey="ProductID", OtherKey="AuctionProductID", IsUnique=true, IsForeignKey=false)]
		public AuctionProduct AuctionProduct
		{
			get
			{
				return this._AuctionProduct.Entity;
			}
			set
			{
				AuctionProduct previousValue = this._AuctionProduct.Entity;
				if (((previousValue != value) 
							|| (this._AuctionProduct.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._AuctionProduct.Entity = null;
						previousValue.Product = null;
					}
					this._AuctionProduct.Entity = value;
					if ((value != null))
					{
						value.Product = this;
					}
					this.SendPropertyChanged("AuctionProduct");
				}
			}
		}
		
		[Association(Name="Brand_Product", Storage="_Brand", ThisKey="BrandID", OtherKey="BrandID", IsForeignKey=true)]
		public Brand Brand
		{
			get
			{
				return this._Brand.Entity;
			}
			set
			{
				Brand previousValue = this._Brand.Entity;
				if (((previousValue != value) 
							|| (this._Brand.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._Brand.Entity = null;
						previousValue.Products.Remove(this);
					}
					this._Brand.Entity = value;
					if ((value != null))
					{
						value.Products.Add(this);
						this._BrandID = value.BrandID;
					}
					else
					{
						this._BrandID = default(int);
					}
					this.SendPropertyChanged("Brand");
				}
			}
		}
		
		[Association(Name="TaxRate_Product", Storage="_TaxRate", ThisKey="TaxRateID", OtherKey="TaxRateID", IsForeignKey=true)]
		public TaxRate TaxRate
		{
			get
			{
				return this._TaxRate.Entity;
			}
			set
			{
				TaxRate previousValue = this._TaxRate.Entity;
				if (((previousValue != value) 
							|| (this._TaxRate.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._TaxRate.Entity = null;
						previousValue.Products.Remove(this);
					}
					this._TaxRate.Entity = value;
					if ((value != null))
					{
						value.Products.Add(this);
						this._TaxRateID = value.TaxRateID;
					}
					else
					{
						this._TaxRateID = default(int);
					}
					this.SendPropertyChanged("TaxRate");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="dbo.Brands")]
	public partial class Brand : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _BrandID;
		
		private string _BrandCode;
		
		private string _BrandName;
		
		private string _BrandWebsite;
		
		private System.Nullable<int> _CreatedBy;
		
		private System.Nullable<System.DateTime> _CreationTime;
		
		private System.Nullable<int> _LastModifiedBy;
		
		private System.Nullable<System.DateTime> _LastModificationTime;
		
		private bool _IsActive;
		
		private EntitySet<Product> _Products;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnBrandIDChanging(int value);
    partial void OnBrandIDChanged();
    partial void OnBrandCodeChanging(string value);
    partial void OnBrandCodeChanged();
    partial void OnBrandNameChanging(string value);
    partial void OnBrandNameChanged();
    partial void OnBrandWebsiteChanging(string value);
    partial void OnBrandWebsiteChanged();
    partial void OnCreatedByChanging(System.Nullable<int> value);
    partial void OnCreatedByChanged();
    partial void OnCreationTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnCreationTimeChanged();
    partial void OnLastModifiedByChanging(System.Nullable<int> value);
    partial void OnLastModifiedByChanged();
    partial void OnLastModificationTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnLastModificationTimeChanged();
    partial void OnIsActiveChanging(bool value);
    partial void OnIsActiveChanged();
    #endregion
		
		public Brand()
		{
			this._Products = new EntitySet<Product>(new Action<Product>(this.attach_Products), new Action<Product>(this.detach_Products));
			OnCreated();
		}
		
		[Column(Storage="_BrandID", AutoSync=AutoSync.OnInsert, DbType="Int NOT NULL IDENTITY", IsPrimaryKey=true, IsDbGenerated=true)]
		public int BrandID
		{
			get
			{
				return this._BrandID;
			}
			set
			{
				if ((this._BrandID != value))
				{
					this.OnBrandIDChanging(value);
					this.SendPropertyChanging();
					this._BrandID = value;
					this.SendPropertyChanged("BrandID");
					this.OnBrandIDChanged();
				}
			}
		}
		
		[Column(Storage="_BrandCode", DbType="VarChar(10) NOT NULL", CanBeNull=false)]
		public string BrandCode
		{
			get
			{
				return this._BrandCode;
			}
			set
			{
				if ((this._BrandCode != value))
				{
					this.OnBrandCodeChanging(value);
					this.SendPropertyChanging();
					this._BrandCode = value;
					this.SendPropertyChanged("BrandCode");
					this.OnBrandCodeChanged();
				}
			}
		}
		
		[Column(Storage="_BrandName", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string BrandName
		{
			get
			{
				return this._BrandName;
			}
			set
			{
				if ((this._BrandName != value))
				{
					this.OnBrandNameChanging(value);
					this.SendPropertyChanging();
					this._BrandName = value;
					this.SendPropertyChanged("BrandName");
					this.OnBrandNameChanged();
				}
			}
		}
		
		[Column(Storage="_BrandWebsite", DbType="VarChar(100)")]
		public string BrandWebsite
		{
			get
			{
				return this._BrandWebsite;
			}
			set
			{
				if ((this._BrandWebsite != value))
				{
					this.OnBrandWebsiteChanging(value);
					this.SendPropertyChanging();
					this._BrandWebsite = value;
					this.SendPropertyChanged("BrandWebsite");
					this.OnBrandWebsiteChanged();
				}
			}
		}
		
		[Column(Storage="_CreatedBy", DbType="Int")]
		public System.Nullable<int> CreatedBy
		{
			get
			{
				return this._CreatedBy;
			}
			set
			{
				if ((this._CreatedBy != value))
				{
					this.OnCreatedByChanging(value);
					this.SendPropertyChanging();
					this._CreatedBy = value;
					this.SendPropertyChanged("CreatedBy");
					this.OnCreatedByChanged();
				}
			}
		}
		
		[Column(Storage="_CreationTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> CreationTime
		{
			get
			{
				return this._CreationTime;
			}
			set
			{
				if ((this._CreationTime != value))
				{
					this.OnCreationTimeChanging(value);
					this.SendPropertyChanging();
					this._CreationTime = value;
					this.SendPropertyChanged("CreationTime");
					this.OnCreationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_LastModifiedBy", DbType="Int")]
		public System.Nullable<int> LastModifiedBy
		{
			get
			{
				return this._LastModifiedBy;
			}
			set
			{
				if ((this._LastModifiedBy != value))
				{
					this.OnLastModifiedByChanging(value);
					this.SendPropertyChanging();
					this._LastModifiedBy = value;
					this.SendPropertyChanged("LastModifiedBy");
					this.OnLastModifiedByChanged();
				}
			}
		}
		
		[Column(Storage="_LastModificationTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> LastModificationTime
		{
			get
			{
				return this._LastModificationTime;
			}
			set
			{
				if ((this._LastModificationTime != value))
				{
					this.OnLastModificationTimeChanging(value);
					this.SendPropertyChanging();
					this._LastModificationTime = value;
					this.SendPropertyChanged("LastModificationTime");
					this.OnLastModificationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_IsActive", DbType="Bit NOT NULL")]
		public bool IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				if ((this._IsActive != value))
				{
					this.OnIsActiveChanging(value);
					this.SendPropertyChanging();
					this._IsActive = value;
					this.SendPropertyChanged("IsActive");
					this.OnIsActiveChanged();
				}
			}
		}
		
		[Association(Name="Brand_Product", Storage="_Products", ThisKey="BrandID", OtherKey="BrandID")]
		public EntitySet<Product> Products
		{
			get
			{
				return this._Products;
			}
			set
			{
				this._Products.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Products(Product entity)
		{
			this.SendPropertyChanging();
			entity.Brand = this;
		}
		
		private void detach_Products(Product entity)
		{
			this.SendPropertyChanging();
			entity.Brand = null;
		}
	}
	
	[Table(Name="dbo.TaxRates")]
	public partial class TaxRate : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private int _TaxRateID;
		
		private System.Nullable<decimal> _TaxRate1;
		
		private string _Description;
		
		private string _LedgerClass;
		
		private System.Nullable<int> _CreatedBy;
		
		private System.Nullable<System.DateTime> _CreationTime;
		
		private System.Nullable<int> _LastModifiedBy;
		
		private System.Nullable<System.DateTime> _LastModificationTime;
		
		private EntitySet<Product> _Products;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnTaxRateIDChanging(int value);
    partial void OnTaxRateIDChanged();
    partial void OnTaxRate1Changing(System.Nullable<decimal> value);
    partial void OnTaxRate1Changed();
    partial void OnDescriptionChanging(string value);
    partial void OnDescriptionChanged();
    partial void OnLedgerClassChanging(string value);
    partial void OnLedgerClassChanged();
    partial void OnCreatedByChanging(System.Nullable<int> value);
    partial void OnCreatedByChanged();
    partial void OnCreationTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnCreationTimeChanged();
    partial void OnLastModifiedByChanging(System.Nullable<int> value);
    partial void OnLastModifiedByChanged();
    partial void OnLastModificationTimeChanging(System.Nullable<System.DateTime> value);
    partial void OnLastModificationTimeChanged();
    #endregion
		
		public TaxRate()
		{
			this._Products = new EntitySet<Product>(new Action<Product>(this.attach_Products), new Action<Product>(this.detach_Products));
			OnCreated();
		}
		
		[Column(Storage="_TaxRateID", DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int TaxRateID
		{
			get
			{
				return this._TaxRateID;
			}
			set
			{
				if ((this._TaxRateID != value))
				{
					this.OnTaxRateIDChanging(value);
					this.SendPropertyChanging();
					this._TaxRateID = value;
					this.SendPropertyChanged("TaxRateID");
					this.OnTaxRateIDChanged();
				}
			}
		}
		
		[Column(Name="TaxRate", Storage="_TaxRate1", DbType="Decimal(18,4)")]
		public System.Nullable<decimal> TaxRate1
		{
			get
			{
				return this._TaxRate1;
			}
			set
			{
				if ((this._TaxRate1 != value))
				{
					this.OnTaxRate1Changing(value);
					this.SendPropertyChanging();
					this._TaxRate1 = value;
					this.SendPropertyChanged("TaxRate1");
					this.OnTaxRate1Changed();
				}
			}
		}
		
		[Column(Storage="_Description", DbType="NVarChar(50)")]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._Description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}
			}
		}
		
		[Column(Storage="_LedgerClass", DbType="VarChar(10)")]
		public string LedgerClass
		{
			get
			{
				return this._LedgerClass;
			}
			set
			{
				if ((this._LedgerClass != value))
				{
					this.OnLedgerClassChanging(value);
					this.SendPropertyChanging();
					this._LedgerClass = value;
					this.SendPropertyChanged("LedgerClass");
					this.OnLedgerClassChanged();
				}
			}
		}
		
		[Column(Storage="_CreatedBy", DbType="Int")]
		public System.Nullable<int> CreatedBy
		{
			get
			{
				return this._CreatedBy;
			}
			set
			{
				if ((this._CreatedBy != value))
				{
					this.OnCreatedByChanging(value);
					this.SendPropertyChanging();
					this._CreatedBy = value;
					this.SendPropertyChanged("CreatedBy");
					this.OnCreatedByChanged();
				}
			}
		}
		
		[Column(Storage="_CreationTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> CreationTime
		{
			get
			{
				return this._CreationTime;
			}
			set
			{
				if ((this._CreationTime != value))
				{
					this.OnCreationTimeChanging(value);
					this.SendPropertyChanging();
					this._CreationTime = value;
					this.SendPropertyChanged("CreationTime");
					this.OnCreationTimeChanged();
				}
			}
		}
		
		[Column(Storage="_LastModifiedBy", DbType="Int")]
		public System.Nullable<int> LastModifiedBy
		{
			get
			{
				return this._LastModifiedBy;
			}
			set
			{
				if ((this._LastModifiedBy != value))
				{
					this.OnLastModifiedByChanging(value);
					this.SendPropertyChanging();
					this._LastModifiedBy = value;
					this.SendPropertyChanged("LastModifiedBy");
					this.OnLastModifiedByChanged();
				}
			}
		}
		
		[Column(Storage="_LastModificationTime", DbType="DateTime")]
		public System.Nullable<System.DateTime> LastModificationTime
		{
			get
			{
				return this._LastModificationTime;
			}
			set
			{
				if ((this._LastModificationTime != value))
				{
					this.OnLastModificationTimeChanging(value);
					this.SendPropertyChanging();
					this._LastModificationTime = value;
					this.SendPropertyChanged("LastModificationTime");
					this.OnLastModificationTimeChanged();
				}
			}
		}
		
		[Association(Name="TaxRate_Product", Storage="_Products", ThisKey="TaxRateID", OtherKey="TaxRateID")]
		public EntitySet<Product> Products
		{
			get
			{
				return this._Products;
			}
			set
			{
				this._Products.Assign(value);
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_Products(Product entity)
		{
			this.SendPropertyChanging();
			entity.TaxRate = this;
		}
		
		private void detach_Products(Product entity)
		{
			this.SendPropertyChanging();
			entity.TaxRate = null;
		}
	}
}
#pragma warning restore 1591
