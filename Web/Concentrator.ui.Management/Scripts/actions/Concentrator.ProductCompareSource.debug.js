Concentrator.ProductCompareSource = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Product Compare Source',
      pluralObjectName: 'Product Compare Sources',
      permissions: {
        list: 'GetProduct',
        create: 'CreateProduct',
        update: 'UpdateProduct',
        remove: 'DeleteProduct'
      },
      url: Concentrator.route('GetPcSourceList', 'ProductCompetitor'),
      primaryKey: 'ProductCompareSourceID',
      sortBy: 'ProductCompareSourceID',
      structure: [
        { dataIndex: 'ProductCompareSourceID', type: 'int', header: 'Source',
          renderer: function (val, meta, record) {
            return record.get('Source');
          }
        },
        { dataIndex: 'Source', type: 'string' },
        { dataIndex: 'ProductCompareSourceParentID', type: 'int', header: 'Parent Compare Source ID' },
        { dataIndex: 'ProductCompareSourceType', type: 'string', header: 'Product Compare Source Type' },
        { dataIndex: 'IsActive', type: 'boolean', header: 'Is Active' }
      ]
    });

    return grid;
  }
});