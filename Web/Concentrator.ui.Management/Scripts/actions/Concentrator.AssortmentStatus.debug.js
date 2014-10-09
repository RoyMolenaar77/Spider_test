Concentrator.AssortmentStatus = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {

    var grid = new Diract.ui.Grid({
      primaryKey: 'StatusID',
      singularObjectName: 'Assortment status',
      pluralObjectName: 'Assortment statuses',
      url: Concentrator.route('GetList', 'ProductStatus'),
      newUrl: Concentrator.route('Create', 'ProductStatus'),
      deleteUrl: Concentrator.route('Delete', 'ProductStatus'),
      updateUrl: Concentrator.route('Update', 'ProductStatus'),
      permissions: {
        list: 'GetProductStatus',
        create: 'CreateProductStatus',
        remove: 'DeleteProductStatus',
        update: 'UpdateProductStatus'
      },
structure:
      [
        { dataIndex: 'StatusID', type: 'int' },
        { dataIndex: 'Status', type: 'string', header: 'Assortment status', editor: { xtype: 'textfield', name: 'Status', allowBlank: false} }
      ]
    });
    return grid;
  }

});