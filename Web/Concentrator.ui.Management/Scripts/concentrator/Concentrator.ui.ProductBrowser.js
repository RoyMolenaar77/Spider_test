/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.Grid'] }, function () {
  Concentrator.ui.ProductBrowser = (function () {
    var productBrowser = Ext.extend(Ext.Panel, {

      layout: 'border',
      border: false,
      constructor: function (config) {
        Ext.apply(this, config);
        this.initLayout();
        Concentrator.ui.ProductBrowser.superclass.constructor.call(this, config);
      },
      initLayout: function () {
        this.items = [this.getHeader(), this.getContent()];
      },
      getHeader: function () {
        headerPanel = new Ext.Panel({
          height: 125,
          layout: 'border',
          region: 'north',
          border: true,
          baseCls: 'product-browser-header',
          padding: 10,
          items: [this.getHeaderContent(), this.getHeaderContentText()]
        });

        return headerPanel;
      },
      getHeaderContent: function () {
        return [
          {
            width: 150,
            border: true,
            region: 'west',
            baseCls: 'product-browser-header',
            height: 125,
            html: '<img src="' + Concentrator.GetImageUrl(this.product.MediaPath, 95, 95, true) + '" />',
            padding: 10
          }
        ];
      },
      getHeaderContentText: function () {
        var panel = new Ext.Panel({
          layout: 'border',
          border: false,
          region: 'center',
          items: [
          {
            region: 'west',
            border: false,
            width: 175,
            padding: 5,
            baseCls: 'product-browser-header',
            html: '<BR><BR>Vendor Item Number:' + '<BR>Concentrator Product ID:' + '<BR>Product Name:' + '<BR><div style="margin-top:5px">Description:</div>'
          },
          {
            border: false,
            width: 200,
            baseCls: 'product-browser-header',
            html: '<BR><BR><div style="color:green">' + this.product.VendorItemNumber + '</div><div style="color:red">' + this.product.ProductID + '</div><h1>' + this.product.ProductName + '</h1><div style="margin-top:5px"><p>' + Ext.util.Format.ellipsis(this.product.ShortContentDesc, 55) + '</p></div>',
            region: 'center',
            padding: 5
          },
          {
            border: false,
            baseCls: 'product-browser-header',
            height: 100,
            region: 'east',
            width: 100,
            padding: 5
          }]
        });

        return panel;
      },
      getContent: function () {
        var contentPanel = new Concentrator.ui.LazyTabPanel({
          border: false,
          margins: '5 0 0 0',
          //style: 'margin-top:3px',
          activeTab: 0,
          region: 'center',
          tabs: [
            {
              title: 'General',
              items: this.getGeneralTab,
              scope: this
            },
            {
              title: "Commercial",
              items: this.getCommercialContent,
              scope: this
            },
            {
              title: 'Specifications',
              items: this.getSpecs,
              scope: this
            },
            {
              title: 'Media',
              layout: 'border',
              cls: 'plain-border',
              items: this.getMediaContent,
              scope: this
            },
            {
              title: 'Product groups',
              layout: 'border',
              cls: 'plain-border',
              items: this.getProductGroups,
              scope: this
            },
            {
              title: 'Related Products',
              items: this.getRelatedProducts,
              scope: this
            },
            {
              title: 'Vendor assortment',
              items: this.getVendorAssortment,
              scope: this
            },
            {
              title: 'Barcodes',
              items: this.getBrandcodes,
              scope: this
            },
            {
              title: 'Brand',
              items: this.getBrand,
              scope: this
            },
            {
              title: 'Export',
              items: this.getExportGrid,
              scope: this
            }
          ]
        });

        return contentPanel;
      },
      getGeneralTab: function () {
        return {
          title: 'General',
          layout: 'fit',
          items: [this.getGeneralInfo()]
        };
      },
      getGeneralInfo: function () {
        return new Concentrator.ui.ProductInfoViewer({ productID: this.productID, isSearched: this.isSearched });
      },
      getCommercialContent: function () {
        var items = [];
        var title = '';
        
        if (this.product.IsConfigurable) {
          items.push(this.getCommercialOverview());
          title = 'Commercial';
        }
        else {
          items.push(this.getCommercialPanels());
          title = Concentrator.Commercial.DisplayAveragePrices
            ? 'Stock and Pricing. Average price: <span ' + (Concentrator.Commercial.DisplayPriceColors ?
            'style="color: magenta"' : '') + '>' + Concentrator.Commercial.CurrencySymbol + this.product.AveragePrice + '</span>. Average cost price: <span ' + (Concentrator.Commercial.DisplayPriceColors ?
            'style="color: magenta"' : '') + '>' + ((this.product.AverageCostPrice == null) ? 'n/a' : (Concentrator.Commercial.CurrencySymbol + this.product.AverageCostPrice)) + '</span>'
            : 'Stock and Pricing';
        }

        return {
          collapsible: false,
          title: title,
          layout: 'fit',
          items: items
        };
      },

      /**
      Retrieves the panels containing the commercial information
      */
      getCommercialPanels: function () {
        return new Ext.Panel({
          layout: 'border',
          border: false,
          autoScroll: true,
          items: [
            this.getPricingContainer(),
            this.getStockContainer()
          ]
        });
      },

      /**
      Retrieves the pricing container for the commercial panel
      */
      getPricingContainer: function () {
        var that = this;

        that.vendorStore = new Ext.data.JsonStore({
          autoDestroy: false,
          url: Concentrator.route('GetVendorStore', 'VendorAssortment'),
          baseParams: { productID: this.productID, isSearched: this.isSearched },
          root: 'results',
          idProperty: 'VendorID',
          fields: ['VendorID', 'Vendor']
        });

        this.VendorSelect = Ext.extend(Diract.ui.Select, {
          label: 'Vendor',
          allowBlank: false,
          store: that.vendorStore,
          displayField: 'Vendor',
          valueField: 'VendorID'
        });

        Ext.reg('vendorAssortmentSelect', this.VendorSelect);

        if (!this.pricingGrid) {
          var pricingRenderer = function (val, m, rec) {
            var currencySymbol = Concentrator.Commercial.DisplayCurrencySymbol
              ? Concentrator.Commercial.CurrencySymbol/*'&#8364 '*/
              : '';

            var style = Concentrator.Commercial.DisplayAveragePrices
              ? that.product.AveragePrice > val
                ? ' style="color:green"'
                : ' style="color:red"'
              : '';

            val = currencySymbol + Ext.util.Format.round(val, 2);
            
            return '<span ' + (Concentrator.Commercial.DisplayPriceColors ? style : '') + '>' + val + '</span>';
          };

          this.pricingGrid = new Diract.ui.Grid({
            singularObjectName: 'price',
            pluralObjectName: 'prices',
            height: 400,
            permissions: { all: 'GetVendorAssortment' },
            primaryKey: ['VendorAssortmentID', 'MinimumQuantity'],
            sortBy: 'Vendor',
            url: Concentrator.route('GetPricesByProduct', 'VendorPrice'),
            newUrl: Concentrator.route('CreateByVendorAssortment', 'VendorPrice'),
            updateUrl: Concentrator.route("Update", "VendorPrice"),
            deleteUrl: Concentrator.route("DeleteByAssortmentID", "VendorPrice"),
            params: { productID: this.productID, isSearched: this.isSearched },
            newParams: { productID: this.productID, isSearched: this.isSearched },
            windowConfig: {
              width: 450,
              height: 500
            },
            structure: [
              { dataIndex: 'VendorAssortmentID', type: 'int' },
              {
                dataIndex: 'MinimumQuantity',
                type: 'int',
                editable: false,
                editor:
                {
                  allowBlank: false,
                  xtype: 'numberfield'
                }
              },
              { dataIndex: 'Vendor', type: 'string' },
              {
                dataIndex: 'VendorID',
                header: 'Vendor',
                type: 'int',
                editor:
                {
                  xtype: 'vendorAssortmentSelect',
                  allowBlank: false,
                  labelField: 'Vendor',
                  method: 'GET'
                },
                editable: false,
                renderer: function (val, m, rec) {
                  return rec.get('Vendor')
                },
                width: 120
              },
              {
                dataIndex: 'CustomItemNumber',
                header: Concentrator.CustomItemNumberField,
                type: 'string',
                width: 150
              },
              { dataIndex: 'CommercialStatus', header: 'Commercial Status', type: 'string', width: 120 },
              {
                dataIndex: 'CostPrice',
                type: 'float',
                header: Concentrator.CostPriceField,
                renderer: pricingRenderer,
                editor:
                {
                  allowBlank: true,
                  xtype: 'numberfield'
                },
                width: 100
              },
              {
                dataIndex: 'Price',
                type: 'float',
                header: Concentrator.UnitPriceField,
                renderer: pricingRenderer,
                editor:
                {
                  allowBlank: true,
                  xtype: 'numberfield'
                },
                width: 100
              },
              {
                dataIndex: 'SpecialPrice',
                header: Concentrator.SpecialPriceField,
                editor:
                {
                  allowBlank: true,
                  xtype: 'numberfield'
                },
                renderer: pricingRenderer,
                type: 'float',
                width: 100
              },
              {
                dataIndex: 'TaxRate',
                header: Concentrator.TaxRateField,
                editor:
                {
                  allowBlank: true,
                  xtype: 'numberfield'
                },
                renderer: Concentrator.renderers.percentage(),
                type: 'float',
                width: 100
              }
            ]
          });
        }

        return new Ext.Panel({
          region: 'west',
          layout: 'fit',
          width: 600,
          margins: '5 5 0 0',
          border: false,
          items: this.pricingGrid
        });
      },

      getStockContainer: function () {
        var that = this;

        if (!this.stockGrid) {
          this.stockGrid = new Diract.ui.Grid({
            singularObjectName: 'stock',
            pluralObjectName: 'stocks',
            height: 400,
            permissions: { all: 'GetVendorAssortment' },
            primaryKey: ['ProductID', 'VendorID', 'VendorStockTypeID'],
            sortBy: 'Vendor',
            url: Concentrator.route('GetStockByProduct', 'VendorStock'),
            newUrl: Concentrator.route('Create', 'VendorStock'),
            updateUrl: Concentrator.route("Update", "VendorStock"),
            deleteUrl: Concentrator.route("DeleteByVendorAssortment", "VendorStock"),
            params: { productID: this.productID, isSearched: this.isSearched },
            newParams: { productID: this.productID },
            windowConfig: {
              width: 450,
              height: 500
            },
            structure:
            [
              { dataIndex: 'isSearched', type: 'boolean' },
              { dataIndex: 'ProductID', type: 'int' },
              {
                dataIndex: 'VendorID', type: 'int', header: 'Vendor', renderer: function (val, m, rec) { return rec.get('Vendor') },
                editable: false,
                editor:
                {
                  xtype: 'vendorAssortmentSelect',
                  allowBlank: false,
                  labelField: 'Vendor',
                  method: 'GET'
                }
              },
              { dataIndex: 'Vendor', type: 'string' },
              { dataIndex: 'QuantityOnHand', type: 'string', header: 'Quantity on hand', editor: { xtype: 'numberfield', value: 0 } },
              { dataIndex: 'PromisedDeliveryDate', type: 'date', header: 'Delivery date', renderer: Ext.util.Format.dateRenderer('d-m-Y') },
              { dataIndex: 'QuantityToReceive', type: 'int', header: 'Quantity to receive', editor: { xtype: 'numberfield', allowBlank: true } },
              { dataIndex: 'UnitCost', type: 'float', header: 'Unit cost', editor: { xtype: 'numberfield', allowBlank: true } },
              { dataIndex: 'StockStatus', type: 'string', header: 'Stock Status', editor: { fieldLabel: 'Stock Status', xtype: 'stockStatus' } },
              {
                dataIndex: 'VendorStockTypeID', type: 'int',
                editable: false,
                editor: { fieldLabel: 'Vendor Stock Type', xtype: 'vendorStockType', allowBlank: false },
                header: 'Stock type',
                renderer: function (val, m, rec) {
                  return rec.get('StockType');
                }
              },
              { dataIndex: 'StockType', type: 'string' }
            ]
          });
        }

        return new Ext.Panel({
          region: 'center',
          layout: 'fit',
          items: this.stockGrid,
          margins: '5 0 0 0',
          border: false
        });
      },

      getSpecs: function () {
        return {
          collapsible: true,
          title: 'Specificaties',
          layout: 'fit',
          items: [
            this.getSpecsGrid()
          ]
        };
      },

      getSpecsGrid: function (record) {
        if (!this.attributesGrid) {
          this.attirbutesGrid = new Concentrator.ui.Grid({
            singularObjectName: 'Product attribute',
            pluralObjectName: 'Product attributes',
            permissions: {
              list: 'GetProductAttribute',
              create: 'CreateProductAttribute',
              update: 'UpdateProductAttribute',
              remove: 'DeleteProductAttribute'
            },
            height: 400,
            params: { productID: this.productID, isSearched: this.isSearched },
            newParams: { productID: this.productID, isSearched: this.isSearched },
            updateParams: { productID: this.productID, isSearched: this.isSearched },
            deleteParams: { productID: this.productID, isSearched: this.isSearched },
            primaryKey: 'AttributeValueID',
            url: Concentrator.route('GetListForProduct', 'ProductAttribute'),
            newUrl: Concentrator.route('Create', 'ProductAttributeValue'),
            updateUrl: Concentrator.route('Update', 'ProductAttributeValue'),
            deleteUrl: Concentrator.route('Delete', 'ProductAttributeValue'),
            newFormConfig: Concentrator.FormConfigurations.existingAttributeValue,
            groupField: 'ProductAttributeGroupID',
            structure: [
              { dataIndex: 'isSearched', type: 'boolean' },
              { dataIndex: 'AttributeValueID', type: 'int' },
              { dataIndex: 'AttributeID', type: 'int' },
              { dataIndex: 'AttributeName', type: 'string', header: 'Attribute' },
              { dataIndex: 'AttributeGroupName', type: 'string' },
              { dataIndex: 'ProductAttributeGroupID', type: 'int', header: 'Group', renderer: function (val, moveBy, rec) { return rec.get('AttributeGroupName') } },
              {
                dataIndex: 'LanguageID', header: 'Language', renderer: Concentrator.renderers.field('languages', 'Name'), type: 'int',
                editor: {
                  xtype: 'language',
                  enableCreateItem: false,
                  allowBlank: true
                },
                filter: {
                  type: 'list',
                  labelField: 'Name',
                  store: Concentrator.stores.languages
                }
              },
              { dataIndex: 'Value', header: 'Value', type: 'string', editor: { xtype: 'textfield', fieldLabel: 'Value', allowBlank: false } }
            ],
            windowConfig: {
              height: 325
            }
          });
        }

        return this.attirbutesGrid;
      },

      getBrand: function () {
        return {
          title: 'Brand',
          layout: 'fit',
          items: [this.getBrandGrid()]
        };
      },

      getCommercialOverview: function () {
        var vendorStore = new Ext.data.JsonStore({
          url: Concentrator.route('GetByProduct', 'Vendor'),
          baseParams: { productID: this.productID, includeConcentratorVendor: true },
          root: 'results',
          idProperty: 'VendorID',
          fields: ['VendorName', 'VendorID'],
          listeners:
          {
            'load': function (store, records) {
              if (vendorSelector.selectedIndex == -1 && records.length > 0) {
                vendorSelector.setValue(records[0].get('VendorID'));
                commercialOverviewReload();
              }
            }
          }
        });

        var vendorSelector = new Ext.form.ComboBox(
        {
          displayField: 'VendorName',
          valueField: 'VendorID',
          store: vendorStore,
          triggerAction: 'all',
          editable: false,
          typeAhead: false,
          forceSelection: false,
          listeners:
          {
            'select': function () {
              commercialOverviewReload();
            }
          }
        });

        vendorStore.load();

        var setPricingWizard = function (records) {
          this.records = records;

          var setPricesForm = new Diract.ui.Form({
            autoWidth: true,
            noButton: true,
            labelWidth: 50,
            items: [
              { xtype: 'numberfield', name: 'CostPrice', fieldLabel: 'Cost' },
              { xtype: 'numberfield', name: 'UnitPrice', fieldLabel: 'Unit' },
              { xtype: 'numberfield', name: 'SpecialPrice', fieldLabel: 'Special' },
              { xtype: 'numberfield', name: 'TaxRate', fieldLabel: 'Tax' }]
          });

          var setPricing = function () {
            var form = setPricesForm.getForm().getFieldValues(true);

            Ext.each(records, function (record) {
              record.beginEdit();

              if (!!form.CostPrice) record.set('CostPrice', form.CostPrice);
              if (!!form.UnitPrice) record.set('UnitPrice', form.UnitPrice);
              if (!!form.SpecialPrice) record.set('SpecialPrice', form.SpecialPrice);
              if (!!form.TaxRate) record.set('TaxRate', form.TaxRate);

              record.endEdit();
            });

            //commercialOverviewGrid.getView().refresh();

            setPricesWindow.close();
          };

          var setPricesWindow = new Ext.Window({
            closable: false,
            items: [setPricesForm],
            layout: 'fit',
            tbar: {
              items: [
              {
                text: 'Apply',
                iconCls: 'save',
                handler: setPricing,
                scope: this
              },
              '->',
              {
                text: 'Exit',
                iconCls: 'exit',
                handler: function () {
                  setPricesWindow.close();
                },
                scope: this
              }]
            },
            title: 'Pricing Wizard',
            height: 180,
            width: 270
          });

          setPricesWindow.show(this, null, this);
        };

        var commercialOverviewReload = function () {
          var parameters = { vendorID: vendorSelector.getValue(), relatedProductTypeID: 1 };

          if (!!parameters.vendorID && !!parameters.relatedProductTypeID) {
            commercialOverviewGrid.store.baseParams = Ext.apply(parameters, commercialOverviewGrid.store.baseParams);
            commercialOverviewGrid.store.reload();
          }
        };

        var commercialOverviewGrid = new Diract.ui.Grid({
          autoLoadStore: false,
          height: 400,
          primaryKey: ['VendorAssortmentID', 'MinimumQuantity'],
          permissions: {
            list: 'GetVendorPrice',
            update: 'UpdateVendorPrice'
          },
          url: Concentrator.route("GetCommercialOverview", "Product"),
          updateUrl: Concentrator.route("UpdateCommercialOverview", "Product"),
          params: { productID: this.productID },
          rowActions: [
            {
              allowMultipleSelect: true,
              text: 'Set prices...',
              iconCls: 'wrench',
              handler: setPricingWizard
            }],
          structure: [
            { dataIndex: 'VendorAssortmentID', type: 'int' },
            {
              dataIndex: 'VendorID',
              type: 'int',
              header: 'Vendor',
              renderer: Concentrator.renderers.field('vendors', 'VendorName'),
              filter: {
                type: 'list',
                labelField: 'VendorName',
                store: Concentrator.stores.vendors
              }
            },
            {
              dataIndex: 'CustomItemNumber',
              header: Concentrator.CustomItemNumberField,
              type: 'string',
              width: 150
            },
            { dataIndex: 'MinimumQuantity', type: 'int', header: 'Minimum Quantity' },
            { dataIndex: 'CommercialStatus', header: 'Commercial Status', type: 'string', width: 120 },
            {
              dataIndex: 'CostPrice',
              header: Concentrator.CostPriceField,
              editor:
              {
                allowBlank: true,
                xtype: 'numberfield'
              },
              type: 'float',
              width: 100
            },
            {
              dataIndex: 'UnitPrice',
              header: Concentrator.UnitPriceField,
              editor:
              {
                allowBlank: true,
                xtype: 'numberfield'
              },
              type: 'float',
              width: 100
            },
            {
              dataIndex: 'SpecialPrice',
              header: Concentrator.SpecialPriceField,
              editor:
              {
                allowBlank: true,
                xtype: 'numberfield'
              },
              type: 'float',
              width: 100
            },
            {
              dataIndex: 'TaxRate',
              header: Concentrator.TaxRateField,
              editor:
              {
                allowBlank: true,
                xtype: 'numberfield'
              },
              renderer: Concentrator.renderers.percentage(),
              type: 'float',
              width: 100
            }
          ]
        });

        return new Ext.Panel({
          border: false,
          layout: 'fit',
          tbar: { items: ['Vendor', vendorSelector] },
          items: [commercialOverviewGrid]
        });
      },

      // Tab Export
      getExport: function () {
        return {
          collapsible: true,
          title: 'Export',
          items: [this.getExportGrid()]
        };
      },

      getBrandGrid: function () {
        var that = this;
        var grid = new Diract.ui.Grid({
          height: 400,
          pluralObjectName: 'Brands',
          singularObjectName: 'Brand',
          primaryKey: 'ProductID',
          permissions: {
            list: 'GetBrand',
            create: 'CreateBrand',
            update: 'UpdateBrand'
          },
          url: Concentrator.route("GetBrandPerProduct", "Brand"),
          updateUrl: Concentrator.route("UpdateBrandperVendor", "Brand"),
          params: { productID: this.productID, isSearched: this.isSearched },
          structure: [
            { dataIndex: 'isSearched', type: 'boolean' },
            {
              dataIndex: 'ProductID', type: 'int', header: 'Product',
              renderer: function (val, m, rec) {
                return rec.get('ProductName');
              }
            },
            { dataIndex: 'ProductName', type: 'string' },
            {
              dataIndex: 'BrandID', type: 'int', header: 'Brand',
              editor: { xtype: 'brand', hiddenName: 'BrandID' },
              renderer: function (val, metadata, record) {
                return record.get('BrandName');
              },
              width: 150
            },
            { dataIndex: 'BrandName', type: 'string', header: 'BrandName' }
          ]
        });

        return grid;
      },

      // Grid Export
      getExportGrid: function () {
        var grid = new Diract.ui.Grid({
          height: 400,
          pluralObjectName: 'Exports',
          singularObjectName: 'Export',
          primaryKey: 'ProductID',
          permissions: {
            list: 'GetVendorAssortment',
            create: 'CreateVendorAssortment',
            update: 'UpdateVendorAssortment',
            remove: 'DeleteVendorAssortment'
          },
          url: Concentrator.route("GetCalculatedPrice", "Product"),
          params: {
            productID: this.productID
          },
          callback: function () {
            that.grid.store.reload();
            that.grid.doLayout();
          },
          structure: [

            { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number' },
            { dataIndex: 'CostPrice', type: 'string', header: 'Retail Price' },
            { dataIndex: 'CalculatedPrice', type: 'string', header: 'Calculated Price' },
            { dataIndex: 'Marge', type: 'string', header: 'Marge' },
            { dataIndex: 'ProductStatus', type: 'string', header: 'Product Status' },
            { dataIndex: 'ShortDescription', type: 'string', header: 'Short Descriptions ' },
            { dataIndex: 'LongDescription', type: 'string', header: 'Long Descriptions' },
            { dataIndex: 'PriceRuleID', type: 'int' },
            { dataIndex: 'ConnectorDescription', type: 'string', header: 'Connector' }
          ],
          listeners: {
            'rowdblclick': function (gr, rowIndex, evt) {
              var rec = grid.store.getAt(rowIndex);
              var priceruleid = rec.get('PriceRuleID');
              Concentrator.ViewInstance.open('content-prices-item', { initialUnmapped: true });
            }.createDelegate(this)
          }
        });

        return grid;

      },
      getFaqGrid: function () {
        var that = this;
        var grid = new Diract.ui.Grid({
          height: 400,
          pluralObjectName: 'Faqs',
          singularObjectName: 'Faq',
          primaryKey: 'FaqID',
          permissions: {
            all: 'Default'
          },
          url: Concentrator.route("GetListByProduct", "Faq"),
          updateUrl: Concentrator.route("UpdateForProduct", "Faq"),
          deleteUrl: Concentrator.route("DeleteForProduct", "Faq"),
          params: {
            productID: this.productID
          },
          structure: [
             { dataIndex: 'FaqID', type: 'int', header: 'Faq ID', width: 20 },
        { dataIndex: 'Question', type: 'string', header: 'Question', editor: { xtype: 'textfield', allowBlank: false } },
         { dataIndex: 'Answer', type: 'string', header: 'Answer', editor: { xtype: 'textfield', allowBlank: false } }
          //           { dataIndex: 'LanguageID', type: 'int', header: 'Language', editor: { xtype: 'language', allowBlank: false }, renderer: Concentrator.renderers.field('languages', 'Name') },
          //          { dataIndex: 'Mandatory', type: 'boolean', header: 'Mandatory', editor: { xtype: 'checkbox'} }
          ]
        });

        var btn = new Ext.Button({
          text: 'Add new Faq',
          iconCls: 'add',
          handler: function () {

            var faqGrid = new Diract.ui.Grid({
              pluralObjectName: 'Faqs',
              height: 490,
              singularObjectName: 'Faq',
              primaryKey: 'FaqID',
              permissions: {
                list: 'GetFaq',
                create: 'CreateFaq',
                update: 'UpdateFaq',
                remove: 'DeleteFaq'
              },
              params: {
                productID: that.productID
              },
              url: Concentrator.route("GetList", "Faq"),
              structure: [
             { dataIndex: 'FaqID', type: 'int', header: 'Faq ID', width: 20 },
            { dataIndex: 'Question', type: 'string', header: 'Question' },
                     { dataIndex: 'LanguageID', type: 'int', header: 'Language', renderer: Concentrator.renderers.field('languages', 'Name') }
              //          { dataIndex: 'Mandatory', type: 'boolean', header: 'Mandatory', editor: { xtype: 'checkbox'} }
              ]
            });

            var window = new Ext.Window({
              width: 800,
              height: 500,
              floating: true,
              layout: 'fit',
              //style : 'margin-top: 5px',
              padding: 10,
              modal: true,
              title: 'Select Faq',
              margins: '5 0 0 3',
              items: [faqGrid],
              buttons: [new Ext.Button({
                text: 'Add',
                handler: (function () {

                  var rec = faqGrid.selModel.selections.items[0];
                  var faqID = rec.get('FaqID');

                  var myMask = new Ext.LoadMask(faqGrid.getEl(), { msg: "Please wait..." });


                  Diract.request({
                    url: Concentrator.route("AddForProduct", "Faq"),
                    params: {
                      productID: that.productID,
                      faqID: faqID
                    },
                    success: function () {
                      window.hide();
                      grid.refresh();
                    },
                    failure: function (form, action) {
                      Ext.MessageBox.show({
                        title: 'Failed to add faq',
                        buttons: Ext.Msg.OK
                      });
                      myMask.hide();
                    }
                  });
                  myMask.show();

                }).createDelegate(this)
              })]
            });

            window.show();
          }
        });

        grid.getTopToolbar().addButton(btn);
        return grid;

      },
      getMediaContent: function () {
        this.eastern = new Ext.Panel({
          region: 'center',
          width: 400,
          collapsible: true,
          padding: 10,
          title: 'Media',
          autoHeight: true,

          cmargins: '5 0 0 3',
          items: [{ border: false, html: 'Please select media', padding: 20 }]
        });

        this.western = new Ext.Panel({
          region: 'east',
          width: 400,
          collapsible: true,
          padding: 10,
          title: 'Image',
          autoHeight: true,
          cmargins: '5 0 0 3',
          items: [{ border: false, html: 'Please select image', padding: 20 }]
        });

        this.wrapper = new Ext.Panel({
          border: false,
          width: 850,
          layout: 'column',
          autoHeight: true,
          items: [
            this.eastern,
            this.western
          ]
        });
        var container = new Ext.Panel({
          items: [this.getMediaView(this.wrapper), this.wrapper],
          height: 1000

        });

        return container;
      },

      getMediaView: function (west) {
        if (!this.mediaViewer) {
          this.mediaViewer = new Concentrator.ui.ProductMediaViewer({
            region: 'center',
            style: 'margin-top: 5px',
            autoHeight: true,
            productID: this.productID,
            isSearched: this.isSearched,
            masterPanel: west
          });
        }

        return this.mediaViewer;
      },
      getProductGroups: function () {
        return [
            {
              region: 'center',
              title: 'Vendor product groups',
              border: false,

              layout: 'fit',
              style: 'margin-top: 5px',


              items: [
                  new Diract.ui.Grid({
                    pluralObjectName: 'Product Group Vendors',
                    singularObjectName: 'Product Group Vendor',
                    primaryKey: ['ProductGroupVendorID', 'VendorID', 'VendorAssortmentID'],
                    sortBy: 'ProductGroupID',
                    height: 400,
                    groupField: 'ProductGroupID',
                    params: { productID: this.productID },
                    // newParams: { productID: this.productID },
                    newUrl: Concentrator.route("CreateProductGroupVendor", "Product"),
                    url: Concentrator.route("GetListByProduct", "ProductGroupVendor"),
                    deleteUrl: Concentrator.route("DeleteProductGroupVendor", "Product"),
                    deleteParams: {
                      productID: this.productID
                    },
                    //                        customButtons: [{ xtype: 'tMButton', grid: function() { return grid; }, mappingField: 'ProductGroupID', pluralObjectName: 'vendor product groups'}],
                    newParams: { productID: this.productID },
                    permissions: {
                      list: 'GetProductGroupVendor',
                      create: 'CreateProductGroupVendor',
                      update: 'UpdateProductGroupVendor',
                      remove: 'DeleteProductGroupVendor'
                    },
                    hasSearchBox: true,
                    updateUrl: Concentrator.route("Update", "ProductGroupVendor"),
                    structure: [
                      { dataIndex: 'VendorAssortmentID', type: 'int' },
                      { dataIndex: 'ProductGroupVendorID', type: 'int' },
                      {
                        dataIndex: 'ProductGroupID', type: 'int', header: "Product Group", width: 200,
                        renderer:
                        function (val, m, record) {
                          return record.get('ProductGroupName');
                        },
                        filter: {
                          type: 'string',
                          filterField: 'ProductGroupName'
                        },
                        editor: {
                          xtype: 'productgroup'
                        }
                      },
                      { dataIndex: 'ProductGroupName', type: 'string' },
                      {
                        dataIndex: 'VendorID', type: 'int', header: "Vendor", width: 45, renderer: Concentrator.renderers.field('vendors', 'VendorName'), width: 100,
                        filter: {
                          type: 'list',
                          labelField: 'VendorName',
                          store: Concentrator.stores.vendors
                        },
                        editable: false
                      },
                      {
                        dataIndex: 'VendorProductGroupCode1', type: 'string', header: 'Product Group Code 1', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      },
                      {
                        dataIndex: 'VendorProductGroupCode2', type: 'string', header: 'Product Group Code 2', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      },
                      {
                        dataIndex: 'VendorProductGroupCode3', type: 'string', header: 'Product Group Code 3', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      },
                      {
                        dataIndex: 'VendorProductGroupCode4', type: 'string', header: 'Product Group Code 4', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      },
                      {
                        dataIndex: 'VendorProductGroupCode5', type: 'string', header: 'Product Group Code 5', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      },

                      {
                        dataIndex: 'VendorProductGroupCode6', type: 'string', header: 'Product Group Code 6', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        },
                        editable: true
                      },
                      {
                        dataIndex: 'VendorProductGroupCode7', type: 'string', header: 'Product Group Code 7', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        },
                        editable: true
                      },

                      {
                        dataIndex: 'VendorProductGroupCode8', type: 'string', header: 'Product Group Code 8', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        },
                        editable: true
                      },
                      {
                        dataIndex: 'VendorProductGroupCode9', type: 'string', header: 'Product Group Code 9', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        },
                        editable: true
                      },
                      {
                        dataIndex: 'VendorProductGroupCode10', type: 'string', header: 'Product Group Code 10', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        },
                        editable: true
                      },

                      {
                        dataIndex: 'BrandCode', type: 'string', header: 'Brand Code', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      },
                      {
                        dataIndex: 'VendorName', type: 'string', header: 'Vendor Name', editable: false,
                        editor: {
                          xtype: 'textfield',
                          allowBlank: true
                        }
                      }
                    ],
                    windowConfig: {
                      height: 550

                    }
                  })
              ]
            }
        ];

      },
      getVendorAssortment: function () {
        return {
          collapsible: true,
          autoHeight: true,
          title: 'Vendor assortment',
          layout: 'fit',
          style: 'margin-top: 5px',
          items: [
            this.getVendorAssortmentGrid()
          ]
        };
      },

      getVendorAssortmentGrid: function () {
        var grid = new Concentrator.ui.Grid({
          singularObjectName: 'Vendor Assortment',
          pluralObjectName: 'Vendor Assortments',
          primaryKey: 'VendorAssortmentID',
          sortBy: 'VendorAssortmentID',
          height: 400,
          permissions: {
            list: 'GetVendorAssortment',
            create: 'CreateVendorAssortment',
            update: 'UpdateVendorAssortment',
            remove: 'DeleteVendorAssortment'
          },
          url: Concentrator.route('GetList', 'VendorAssortment'),
          params: { productID: this.productID },
          updateUrl: Concentrator.route('SetActive', 'VendorAssortment'),
          structure: [
            { dataIndex: 'VendorAssortmentID', type: 'int' },
            { dataIndex: 'ProductID', type: 'int', header: 'Concentrator identifier' },
            { dataIndex: 'VendorItemNumber', type: 'string', header: Concentrator.VendorItemNumberField },
          //{ dataIndex: 'ProductName', type: 'string', header: 'Product' },
            { dataIndex: 'CustomItemNumber', type: 'string', header: Concentrator.CustomItemNumberField },
            {
              dataIndex: 'VendorID',
              header: 'Vendor',
              renderer: Concentrator.renderers.field('vendors', 'VendorName'),
              filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName' },
              type: 'int'
            },
            { dataIndex: 'ShortDescription', type: 'string', header: 'Description' },
            { dataIndex: 'LineType', type: 'string', header: 'Line Type' },
            { dataIndex: 'QuantityOnHand', type: 'string', header: 'Quantity on hand' },
            { dataIndex: 'PromisedDeliveryDate', type: 'date', header: 'Delivery date', renderer: Ext.util.Format.dateRenderer('d-m-Y') },
            { dataIndex: 'QuantityToReceive', type: 'int', header: 'Quantity to receive' },
          //{ dataIndex: 'VendorStatus', type: 'string', header: 'Vendor Status' },
            { dataIndex: 'UnitCost', type: 'float', header: 'Unit cost' },
            { dataIndex: 'IsActive', type: 'boolean', header: 'Active', editor: { xtype: 'checkbox', allowBlank: false } }
          ]
        });

        return grid;
      },
      getBrandcodes: function () {
        return {
          autoHeight: true,
          title: 'Barcodes',
          layout: 'fit',
          items: [
            this.getProductBarcodesGrid()
          ]
        };

      },

      getRelatedProducts: function () {
        return {
          title: 'Related products',
          layout: 'fit',
          items: [this.getRelatedProductsGrid()]
        };
      },
      getRelatedProductsGrid: function () {
        var that = this;

        if (!this.relatedProductsGrid) {
          this.relatedProductsGrid = new Diract.ui.Grid({
            singularObjectName: 'Related product',
            pluralObjectName: 'Related products',
            url: Concentrator.route('GetRelatedProductsByProductID', 'RelatedProduct'),
            permissions: {
              list: 'GetProduct',
              create: 'CreateProduct',
              update: 'UpdateProduct',
              remove: 'DeleteProduct'
            },
            primaryKey: ['ProductID', 'RelatedProductID'],
            sortInfo: { field: 'Index', direction: 'DESC' },
            newUrl: Concentrator.route('Create', 'RelatedProduct'),
            updateUrl: Concentrator.route('Update', 'RelatedProduct'),
            deleteUrl: Concentrator.route('Delete', 'RelatedProduct'),
            params: { productID: this.productID },
            newFormConfig: Concentrator.FormConfigurations.newRelatedProduct,
            newParams: { ProductID: this.productID },
            height: 300,
            structure: [
              { dataIndex: 'ProductID', type: 'int', editor: { xtype: 'hidden', value: this.productID } },
              {
                dataIndex: 'RelatedProductID', type: 'int', editable: false, header: 'Related product', renderer: function (val, m, rec) { return rec.get('RelatedProductDescription') },
                editor: {
                  xtype: 'product',
                  hiddenName: 'RelatedProductID'
                }
              },
              { dataIndex: 'IsConfigured', type: 'boolean', header: 'Configured' },
              { dataIndex: 'IsActive', type: 'boolean', header: 'Active', editable: true, editor: { xtype: 'checkbox', allowBlank: false } },
              { dataIndex: 'Index', type: 'int', header: 'Score/Index', editable: true, editor: { xtype: 'textfield' } },
              { dataIndex: 'VendorItemNumber', header: 'Item number', type: 'string' },
              { dataIndex: 'RelatedProductDescription', type: 'string' },
              {
                dataIndex: 'VendorID', type: 'int', header: 'Vendor',
                editable: true,
                editor: { xtype: 'vendor', allowBlank: false },
                renderer: Concentrator.renderers.field('vendors', 'VendorName'), //
                filter: {
                  type: 'list',
                  labelField: 'VendorName',
                  store: Concentrator.stores.vendors
                }
              },
              {
                dataIndex: 'RelatedProductTypeID', type: 'int', header: 'Related Product Type',
                renderer: Concentrator.renderers.field('relatedProductTypes', 'Type'),
                filter: {
                  type: 'list',
                  labelField: 'Type',
                  store: Concentrator.stores.relatedProductTypes
                },
                editor: {
                  xtype: 'relatedProductType',
                  hiddenName: 'RelatedProductTypeID'
                }
              },
              { dataIndex: 'RelatedProdctTypeName', type: 'string' }
            ],
            rowActions: [
             {
               text: 'View related product',
               iconCls: 'box-view',
               handler: function (record) {

                 var productID = record.get('RelatedProductID');

                 var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
               }
             }
            ]
            //,
            //listeners: {
            //  'celldblclick': (function (grid, rowIndex, columnIndex, evt) {
            //    var rec = grid.store.getAt(rowIndex);
            //    var productID = rec.get('RelatedProductID');

            //    var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
            //  }).createDelegate(this)
            //}
          });
        }

        return that.relatedProductsGrid;

      },

      getProductBarcodes: function () {
        return {
          title: 'Related',
          items: [this.getProductBarcodesGrid()]
        };
      },
      getProductBarcodesGrid: function () {
        var that = this;
        if (!this.productBarcodesGrid) {
          this.productBarcodesGrid = new Diract.ui.Grid({
            singularObjectName: 'Product Barcode',
            pluralObjectName: 'Product Barcodes',
            url: Concentrator.route('GetList', 'ProductBarcode'),
            permissions: {
              list: 'GetProductBarCode',
              create: 'CreateProductBarCode',
              update: 'UpdateProductBarCode',
              remove: 'DeleteProductBarCode'
            },
            primaryKey: ['ProductID', 'Barcode'],
            sortBy: 'ProductID',
            height: 400,
            params: { productID: this.productID },
            newUrl: Concentrator.route('Create', 'ProductBarcode'),
            updateUrl: Concentrator.route('Update', 'ProductBarcode'),
            deleteUrl: Concentrator.route('Delete', 'ProductBarcode'),
            structure: [
              { dataIndex: 'ProductID', type: 'int', editor: { xtype: 'hidden', value: this.productID } },
              { dataIndex: 'Barcode', type: 'string', header: 'Barcode', editor: { xtype: 'textfield', allowBlank: false } },
              {
                dataIndex: 'BarcodeType', type: 'int', header: 'Barcode Type',
                renderer: Concentrator.renderers.field('productBarcodeTypes', 'Name'),
                editor: {
                  xtype: 'productBarcodetypes',
                  allowBlank: true
                }, filter: {
                  type: 'list',
                  store: Concentrator.stores.productBarcodeTypes,
                  labelField: 'Name'
                }
              }
            ]
          });
        }

        return that.productBarcodesGrid;

      }
    });

    return productBrowser;
  })();

  Ext.reg('productbrowser', Concentrator.ui.ProductBrowser);
});