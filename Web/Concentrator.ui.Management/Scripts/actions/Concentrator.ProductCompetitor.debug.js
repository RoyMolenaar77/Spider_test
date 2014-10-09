Concentrator.ProductCompetitor = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var self = this;
    self.grid = new Concentrator.ui.Grid({
      singularObjectName: 'Product Competitor',
      pluralObjectName: 'Product Competitors',
      permissions: {
        list: 'GetProductCompetitorList',
        create: 'CreateProductCompetitor',
        update: 'UpdateProductCompetitor',
        remove: 'DeleteProductCompetitor'
      },
      url: Concentrator.route('GetProductCompetitorList', 'ProductCompetitor'),
      deleteUrl: Concentrator.route('DeleteProductCompetitor', 'ProductCompetitor'),
      updateUrl: Concentrator.route('UpdateProductCompetitor', 'ProductCompetitor'),
      newUrl: Concentrator.route("CreateProductCompetitor", "ProductCompetitor"),
      primaryKey: 'ProductCompetitorID',
      newFormConfig: Concentrator.FormConfigurations.productCompetitor,
      sortBy: 'ProductCompetitorID',
      structure: [
        { dataIndex: 'ProductCompetitorID', type: 'int', header: 'Product Competitor',
          renderer: function (val, meta, record) {
            return record.get('Name');
          },
          editable: true,
          filter: { type: 'string', filterField: 'Name' }
        },
        { dataIndex: 'Name', type: 'string' },
        { dataIndex: 'Reliability', type: 'int', header: 'Reliability', editable: true,
          editor: {
            xtype: 'numberfield',
            allowBlank: false
          }
        },
        { dataIndex: 'DeliveryDate', type: 'date', header: 'Delivery Date', editable: true,
          editor: {
            xtype: 'datefield',
            allowBlank: false
          }
        },
        { dataIndex: 'ShippingCostPerOrder', type: 'float', header: 'Shipping Cost Per Order', editable: true,
          editor: {
            xtype: 'numberfield',
            allowBlank: false
          }
        },
        { dataIndex: 'ShippingCost', type: 'float', header: 'Shipping Cost', editable: true,
          editor: {
            xtype: 'numberfield',
            allowBlank: false
          }
        }
      ]
    });

    return self.grid;

  }

});