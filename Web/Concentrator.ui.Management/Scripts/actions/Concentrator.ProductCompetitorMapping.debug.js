Concentrator.ProductCompetitorMapping = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var self = this;

    this.productSources = Ext.extend(Diract.ui.SearchBox, {
      valueField: 'ProductCompetitorID',
      displayField: 'Name',
      searchUrl: Concentrator.route('GetProductCompetitorStore', 'ProductCompetitor'),
      hiddenName: 'ProductCompetitorID'
    });

    Ext.reg('productCompetitorList', this.productSources);

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Product Competitor Mappings',
      singularObjectName: 'Product Competitor Mapping',
      primaryKey: 'ProductCompetitorMappingID',
      url: Concentrator.route("GetProductCompetitorMapping", "ProductCompetitor"),
      updateUrl: Concentrator.route("UpdateProductCompetitorMapping", "ProductCompetitor"),
      permissions: {
        create: 'CreateProductCompetitor',
        update: 'UpdateProductCompetitor'
      },
      structure: [
                { dataIndex: 'ProductCompetitorMappingID', header: 'Product Competitor Mapping', type: 'int' },
                { dataIndex: 'Competitor', header: 'Competitor', type: 'string' },
                { dataIndex: 'productCompareSourceID', header: 'Product Compare Source', type: 'int',
                  editor: { xtype: 'productSources' },
                  editable: false,
                  renderer: function (val, meta, record) {
                    return record.get('Source');
                  }
                },
                { dataIndex: 'Source', type: 'string' },


                { dataIndex: 'ProductCompetitorName', type: 'string' },
                { dataIndex: 'ProductCompetitorID', type: 'int',
                  header: 'Product Competitor',
                  renderer: function (val, m, rec) { return rec.get('ProductCompetitorName') },
                  filter: { type: 'string', filterField: 'ProductCompetitorName' },
                  editor: { xtype: 'productCompetitorList', allowBlank: true }
                },


                { dataIndex: 'IncludeShippingCost', header: 'Include Shipping Cost', type: 'boolean' },
                { dataIndex: 'InTaxPrice', header: 'In Tax Price', type: 'boolean' }
            ]
    });

    return grid;
  }

});