Concentrator.ProductGroupMappings = Ext.extend(Concentrator.BaseAction, {
  needsConnector: true,
  listeners: {
    'beforeclose': function () {
      Concentrator.navigationPanel.expand(true);
    }
  },
  getPanel: function () {
    Concentrator.navigationPanel.collapse(true);

    this.connectorSystem = null;

    this.magentoConnectorPanel = this.createMagentoConnectorPanel(this.connectorSystem);

    this.productGroupConnectorPanel = this.createProductGroupConnectorPanel();

    this.relationField = new Ext.form.TextField({
      fieldLabel: 'Related To',
      name: 'Relation'
    });

    Diract.silent_request({
      url: Concentrator.route("GetConnectorSystem", "Connector"),
      params: {
        connectorID: this.connectorID
      },
      success: (function (result) {
        this.connectorSystem = result.ConnectorSystem;

        if (this.connectorSystem == "Magento") {
          this.magentoConnectorPanel.hidden = false;
        }
      }).createDelegate(this)
    });

    var ProductGroupMappingGrid = new Diract.ui.TG({
      treeConfig: {
        loadDataUrl: Concentrator.route('GetTreeView', 'ProductGroupMapping'),
        rootText: 'product group mappings',
        deleteUrl: Concentrator.route('Delete', 'ProductGroupMapping'),
        singularObjectName: 'Product group mapping',
        hierarchicalIndexID: 'ProductGroupMappingID',
        ProductGroupID: 'ProductGroupID',
        hierarchicalParentIndexID: 'ParentProductGroupMappingID',
        baseAttributes: { ProductGroupMappingID: -1, ConnectorID: this.connectorID }
      },
      searchConfig: {
        fieldsToSearchIn: ['product', 'product group', 'product group mapping'],
        searchUrl: Concentrator.route('SearchTree', 'ProductGroupMapping'),
        extraButtons: [
          {
            text: 'Generate Brand Mapping',
            iconCls: 'merge',
            xtype: 'button',
            style: 'margin-left: 3px',
            tooltip: 'Generate Brand Mapping',
            handler: (function () {
              this.generateBrandMapping();
            }).createDelegate(this)
          },

        //          configurableProductsButton,

          {
            text: 'Copy mappings',
            iconCls: 'add',
            xtype: 'button',
            style: 'margin-left: 3px',
            tooltip: 'Copy Mapping',
            handler: (function () {
              this.getCopyMappingsWindow();
            }).createDelegate(this)
          }
        ]
      },
      reload: function () {

      },
      detailRelationGrid: function (productGroupMappingID) {
        var that = this;
        if (!this.relationGrid) {
          var configurableProductsButton = new Concentrator.ui.ToggleMappingsButton({
            grid: function () {
              return that.relationGrid;
            },
            mappingField: 'IsConfigurable',
            unmappedValue: true,
            unmappedText: 'Show configurable products'
          });

          this.relationGrid = new Diract.ui.Grid({
            singularObjectName: 'Product in mapping',
            pluralObjectName: 'Product in mapping',
            params: { ProductGroupMappingID: productGroupMappingID },
            updateParams: { ProductGroupMappingID: productGroupMappingID },
            autoLoad: false,
            customButtons: [configurableProductsButton],
            url: Concentrator.route('GetByProductGroupMapping', 'ContentProductGroupMapping'),
            deleteUrl: Concentrator.route('RemoveFromProductGroupMapping', 'Product'),
            updateUrl: Concentrator.route('Update', 'ContentProductGroupMapping'),
            primaryKey: ['ProductID', 'ConnectorID'],
            permissions: {
              list: 'GetProductGroupMapping',
              create: 'CreateProductGroupMapping',
              remove: 'DeleteProductGroupMapping',
              update: 'DeleteProductGroupMapping'
            },
            sortBy: 'ProductID',
            id: 'detailRelationGrid',
            structure: [
            { dataIndex: 'ProductID', type: 'int' },
            {
              dataIndex: 'ProductName', type: 'string', header: 'Product'
            },
            { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor item number' },
            {
              dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
              editor: { xtype: 'connector', allowBlank: false },
              renderer: Concentrator.renderers.field('connectors', 'Name'),
              filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
            },
            { dataIndex: 'ShortDescription', type: 'string', header: 'Short desc', editable: false },
            { dataIndex: 'VendorItemNumber', type: 'string' },
            { dataIndex: 'LongDescription', type: 'string', header: 'Long desc', editable: false },
            { dataIndex: 'IsConfigurable', type: 'boolean', header: 'Configurable' },
            { dataIndex: 'IsExported', type: 'boolean', header: 'Will be exported', editor: { xtype: 'checkbox' } }
            ],
            listeners: {
              'celldblclick': (function (grid, rowIndex, columnIndex, evt) {
                var rec = grid.store.getAt(rowIndex);
                var productID = rec.get('ProductID');

                var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
              }).createDelegate(this)
            }
          });

        }
        return this.relationGrid;
      },

      newEntityFormConfig: {
        height: 320,
        fileUpload: true,
        ref: this, //lame fix
        url: Concentrator.route('Create', 'ProductGroupMapping'),
        items: [
          this.magentoConnectorPanel,
          {
            xtype: 'productgroup',
            fieldLabel: 'Product group',
            hiddenName: 'ProductGroupID',
            allowBlank: false
          },
            {
              xtype: 'fileuploadfield',
              emptyText: 'Select file ...',
              name: 'MappingThumbnailImagePath',
              hiddenName: 'MappingThumbnailImagePath',
              fieldLabel: 'Thumbnail',
              buttonText: '',
              width: 154,
              buttonCfg: { iconCls: 'upload-icon' }
            },

          {
            xtype: 'fileuploadfield',
            emptyText: 'Select file ...',
            name: 'ProductGroupMappingPath',
            hiddenName: 'ProductGroupMappingPath',
            fieldLabel: 'Image',
            buttonText: '',
            width: 154,
            buttonCfg: { iconCls: 'upload-icon' }
          },
          {
            xtype: 'checkbox',
            name: 'FilterByParentGroup',
            fieldLabel: 'Filter by parent'
          },
          {
            xtype: 'checkbox',
            name: 'FlattenHierarchy',
            fieldLabel: 'Flatten hierarchy'
          },
          {
            xtype: 'numberfield',
            name: 'Score',
            width: 200,
            fieldLabel: 'Score'
          },
          {
            xtype: 'textfield',
            name: 'CustomProductGroupLabel',
            width: 200,
            fieldLabel: 'Custom label'
          }
        ]
      },

      entityFormConfig: {
        url: Concentrator.route('Update', 'ProductGroupMapping'),
        fileUpload: true,
        ref: this,
        loadUrl: Concentrator.route('Get', 'ProductGroupMapping'),
        items: [
            this.magentoConnectorPanel,
            this.productGroupConnectorPanel,
            {
              xtype: 'hidden',
              name: 'ProductGroupMappingID'
            },
            {
              xtype: 'hidden',
              name: 'ProductGroupID'
            },
            {
              xtype: 'textfield',
              name: 'ProductGroupName',
              readOnly: true,
              fieldLabel: 'Product group'
            },
            {
              xtype: 'fileuploadfield',
              emptyText: 'Select file ...',
              name: 'MappingThumbnailImagePath',
              hiddenName: 'MappingThumbnailImagePath',
              fieldLabel: 'Thumbnail',
              buttonText: '',
              width: 154,
              buttonCfg: { iconCls: 'upload-icon' }
            },
            {
              xtype: 'fileuploadfield',
              emptyText: 'Select Image...',
              name: 'ProductGroupMappingPath',
              hiddenName: 'ProductGroupMappingPath',
              fieldLabel: 'Image',
              buttonText: '',
              width: 154,
              buttonCfg: { iconCls: 'upload-icon' }
            },
            {
              xtype: 'textfield',
              name: 'ConnectorName',
              readOnly: true,
              fieldLabel: 'Connector'
            },
            {
              xtype: 'checkbox',
              name: 'FilterByParentGroup',
              fieldLabel: 'Filter by parent'
            },
            {
              xtype: 'checkbox',
              name: 'FlattenHierarchy',
              fieldLabel: 'Flatten hierarchy'
            },
            {
              xtype: 'numberfield',
              name: 'Score',
              fieldLabel: 'Score'
            },
            {
              xtype: 'textfield',
              name: 'CustomProductGroupLabel',
              fieldLabel: 'Custom label'
            },


            this.relationField
        ]
      },

      clipBoard: (function (data) {

        this.getClipBoard(data);

      }).createDelegate(this)

    });

    return ProductGroupMappingGrid;
  },

  getClipBoard: function (data) {

    this.relationField.setValue(data);
  },

  createMagentoConnectorPanel: function (connectorSystem) {

    var magentoConnectorPanel = new Ext.Panel({
      title: 'Magento Connector Settings',
      border: true,
      region: 'north',
      layout: 'form',
      hidden: connectorSystem == "Magento" ? false : true,
      autoDestory: true,
      bodyStyle: 'padding: 5px; margin-bottom: 15px;',
      width: 300,
      items: [
        {
          xtype: 'checkbox',
          name: 'ShowInMenu',
          fieldLabel: 'Hide in menu'
        },
        {
          xtype: 'checkbox',
          name: 'DisabledMenu',
          fieldLabel: 'Disable menu'
        },
        {
          xtype: 'checkbox',
          name: 'IsAnchor',
          fieldLabel: 'Is Anchor'
        },
        {
          xtype: 'select',
          name: 'LayoutID',
          fieldLabel: 'Page Layout',
          allowBlank: true,
          valueField: 'LayoutID',
          store: Concentrator.stores.pageLayouts,
          label: 'Page Layout',
          displayField: 'LayoutName'
        }
      ]
    });


    return magentoConnectorPanel;
  },

  createProductGroupConnectorPanel: function () {

    var connectorsForUser = Concentrator.stores.connectorsForLoggedInUser.data.items;

    var productGroupMappingConnecorItems = [];

    for (var i = 0; i < connectorsForUser.length; i++) {

      var newProductGroupMappingConnecorItem = {
        xtype: 'checkbox',
        name: 'ConnectorID_' + connectorsForUser[i].data.ConnectorID,
        fieldLabel: connectorsForUser[i].data.ConnectorName
      }

      productGroupMappingConnecorItems.push(newProductGroupMappingConnecorItem);
    }

    var productGroupConnectorPanel = new Ext.Panel({
      title: 'Active/Inactive groups per connector',
      border: true,
      region: 'center',
      layout: 'form',
      autoDestory: true,
      bodyStyle: 'padding: 5px; margin-bottom: 15px;',
      width: 300,
      items: productGroupMappingConnecorItems
    });

    return productGroupConnectorPanel;
  },


  generateBrandMapping: function () {

    var panel = new Diract.ui.FormWindow({
      width: 400,
      buttonText: 'Generate',
      url: Concentrator.route('GenerateBrandMapping', 'ProductGroupMapping'),
      items: [
        {
          xtype: 'connector',
          fieldLabel: 'Connector',
          width: 175
        },
        {
          xtype: 'numberfield',
          name: 'Score',
          fieldLabel: 'Score',
          width: 175
        }

      ],
      success: function () {
        panel.destroy();
      }
    });

    panel.show();
  },

  getCopyMappingsWindow: function () {
    var wi = new Diract.ui.FormWindow({
      url: Concentrator.route('Copy', 'ContentProductGroupMapping'),
      width: 400,
      height: 200,
      items: [
        {
          xtype: 'connector',
          fieldLabel: 'From Connector',
          hiddenName: 'sourceConnectorID'
        },
        {
          xtype: 'connector',
          fieldLabel: 'To Connector',
          hiddenName: 'destinationConnectorID'
        }
      ],
      success: function () {
        wi.destroy();
      }
    });

    wi.show();
  }

});