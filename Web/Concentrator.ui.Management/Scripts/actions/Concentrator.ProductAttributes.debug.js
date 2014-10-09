Concentrator.ProductAttributes = Ext.extend(Concentrator.BaseAction, {

	getPanel: function () {
		var self = this;

		this.grid = new Diract.ui.TranslationGrid({
			singularObjectName: 'Product attribute',
			pluralObjectName: 'Product attributes',
			permissions: {
				list: 'GetProductAttribute',
				create: 'CreateProductAttribute',
				remove: 'DeleteProductAttribute',
				update: 'UpdateProductAttribute'
			},
			getParams: function (rec) {
				return {
					attributeID: rec.get('AttributeID')
				};
			},
			translationsUrl: Concentrator.route('GetTranslations', 'ProductAttribute'),
			translationsUrlUpdate: Concentrator.route('SetTranslation', 'ProductAttribute'),
			translationsGridStructure: [
        { dataIndex: 'AttributeID', type: 'int' },
        { dataIndex: 'Language', type: 'string', header: 'Language' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield'} },
        { dataIndex: 'LanguageID', type: 'int' }
      ],
			primaryKey: 'AttributeID',
			url: Concentrator.route('GetList', 'ProductAttribute'),
			updateUrl: Concentrator.route('Update', 'ProductAttribute'),
			deleteUrl: Concentrator.route('Delete', 'ProductAttribute'),
			newUrl: Concentrator.route('Create', 'ProductAttribute'),
			sortBy: 'ProductAttributeGroupID',
			groupField: 'ProductAttributeGroupID',
			newFormConfig: Concentrator.FormConfigurations.newProductAttribute,
			structure: [
        { dataIndex: 'AttributeID', type: 'int' },
        { dataIndex: 'AttributeName', type: 'string', header: 'Attribute', editable: false },
        {
          dataIndex: 'ProductAttributeGroupID',
          type: 'int',
          header: 'Group',
          filter:
            {
              type: 'string',
              filterField: 'AttributeGroupName'
            },
          editable: true,
          renderer: function (val, metadata, record)
          {
        		return record.get('AttributeGroupName');
        	},
          editor:
          {
        		xtype: 'productattributegroup'
        	}
        },
        { dataIndex: 'AttributeGroupName', type: 'string' },
        { dataIndex: 'FormatString', type: 'string', header: 'Format',
        	filter: {
        		type: 'string',
        		filterField: 'FormatString'
        	},
        	renderer: function (val, metadata, record) {
        		return record.get('FormatString');
        	},
        	editor: {
        		xtype: 'textfield'
        	},
        	editable: true
        },
        { dataIndex: 'Sign', type: 'string', header: 'Sign',
        	filter: {
        		type: 'string',
        		filterField: 'Sign'
        	},
        	renderer: function (val, metadata, record) {
        		return record.get('Sign');
        	},
        	editor: {
        		xtype: 'textfield'
        	},
        	editable: true
        },
        { dataIndex: 'DataType', type: 'string', header: 'DataType',
        	filter: {
        		type: 'string',
        		filterField: 'DataType'
        	},
        	renderer: function (val, metadata, record) {
        		return record.get('DataType');
        	},
        	editor: {
        		xtype: 'textfield'
        	},
        	editable: true
        },
        { dataIndex: 'NeedsUpdate', type: 'boolean', header: 'Allow System Override', editable: true, editor: { xtype: 'boolean' } },
        { dataIndex: 'Index', type: 'int', header: 'Index', editable: true, editor: { xtype: 'numberfield'} },
        { dataIndex: 'IsConfigurable', type: 'boolean', header: 'IsConfigurable', editable: true, editor: { xtype: 'boolean'} },
        { dataIndex: 'IsSearchable', type: 'boolean', header: 'IsSearchable', editable: true, editor: { xtype: 'boolean'} },
        { dataIndex: 'IsVisible', type: 'boolean', header: 'IsVisible', editable: true, editor: { xtype: 'boolean'} },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',	filter: {
        		type: 'string',
        		filterField: 'VendorName'
        	},
        	renderer: function (val, metadata, record) {
        		return record.get('VendorName');
        	},
        	editor: {
        		xtype: 'vendor'
        	}
        },
        { dataIndex: 'VendorName', type: 'string' },
        { dataIndex: 'Mandatory', type: 'boolean', header: 'Mandatory', editable: true, editor: { xtype: 'boolean'} },
        { dataIndex: 'DefaultValue', type: 'string', header: 'Default Value', editor: { xtype: 'textfield'} },
        { dataIndex: 'AttributePath', type: 'string', header: 'Image',	editable: false },
				{ dataIndex: 'ConfigurablePosition', type: 'int', header: 'Position in configurable product', editable: true, editor: { xtype: 'numberfield'} }
      ],
			windowConfig: {
				height: 550
			},
			formConfig: {
				fileUpload: true
			},
			rowActions: [
        {
        	text: 'Image options',
        	iconCls: 'upload-icon',
        	handler: (function (record) {

        		var box = new Ext.BoxComponent(
              { autoEl: {
              	tag: 'img',
              	src: Concentrator.GetImageUrl(record.get('AttributePath'), 50, 50)
              }
              });

        		var closeButton = new Ext.Button({
        			text: 'Close',
        			handler: function () {
        				window.destroy();
        			}
        		});

        		var uploadButton = new Ext.Button({
        			text: 'Upload new logo',
        			handler: function () {
        				self.uploadImage(record);
        			}
        		});

        		form = new Ext.form.FormPanel({
        			padding: 10,
        			items: [
                box
              ],
        			buttons: [
                uploadButton,
                closeButton
              ]
        		});

        		this.uploadWindow = new Ext.Window({
        			title: 'Logo',
        			height: 200,
        			width: 350,
        			modal: true,
        			layout: 'fit',
        			items: [
                form
              ]
        		});

        		this.uploadWindow.show();
        	}).createDelegate(this)
        },
        {
        	iconCls: 'cubes',
        	text: 'View associated products',
        	handler: function (record) {
        		self.getProducts(record);
        	}
        }
      ]
		});

		return this.grid;
	},

	uploadImage: function (record) {
		var attributeID = record.get('AttributeID');
		var that = this;
		var window = new Diract.ui.FormWindow({
			url: Concentrator.route('Update', 'ProductAttribute'),
			title: 'Product Attribute Image',
			buttonText: 'Save',
			labelWidth: 200,
			fileUpload: true,
			loadUrl: Concentrator.route('GetImage', 'ProductAttribute'),
			loadParams: { id: attributeID },
			params: {
				id: attributeID
			},
			cancelButton: true,
			items: [
        {
        	xtype: 'fileuploadfield',
        	emptyText: 'Select Image...',
        	name: 'AttributePath',
        	hiddenName: 'AttributePath',
        	fieldLabel: 'Image',
        	buttonText: '',
        	width: 154,
        	buttonCfg: { iconCls: 'upload-icon' }
        }
      ],
			layout: 'fit',
			height: 200,
			width: 350,
			success: (function () {
				// Reload the grid to display the new image url   
				this.grid.store.reload();

				window.destroy();

				this.uploadWindow.destroy();
			}).createDelegate(this)
		});

		window.show();
	},

	getProducts: function (record) {
		var grid = new Concentrator.ui.Grid({
			singularObjectName: 'Product',
			pluralObjectName: 'Products',
			permissions: {
				list: 'GetProductAttribute',
				create: 'CreateProductAttribute',
				remove: 'DeleteProductAttribute',
				update: 'UpdateProductAttribute'
			},
			url: Concentrator.route('GetAttributeProductValues', 'ProductAttribute'),
			//url: Diract.user.hasFunctionality("Default") ? Concentrator.route('GetAttributeProductValues', 'ProductAttribute') : null,
			primaryKey: 'ProductID',
			params: {
				attributeID: record.get('AttributeID'),
				attributeValueGroupID: record.get('ProductAttributeGroupID')
			},
			structure: [
          { dataIndex: 'ProductID', type: 'int', header: 'Identifier' },
          { dataIndex: 'Description', type: 'string', header: 'Description' },
          { dataIndex: 'Value', type: 'string', header: 'Attribute value' }
        ],
			listeners: {
				'rowdblclick': (function (grid, rowIndex, columnIndex, evt) {
					var rec = grid.store.getAt(rowIndex);
					var productID = rec.get('ProductID');

					var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
				}).createDelegate(this)
			}
		});

		var wind = new Ext.Window({
			modal: true,
			width: 600,
			height: 400,
			items: grid,
			layout: 'fit'
		});
		wind.show();
	}
});