Concentrator.ProductCompetitorPrice = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Product Competitor Price',
      pluralObjectName: 'Product Competitors Prices',
      permissions: {
        list: 'GetProductCompetitor',
        update: 'UpdateProductCompetitor'
      },
      url: Concentrator.route('GetProductCompetitorPriceList', 'ProductCompetitor'),
      primaryKey: 'ProductCompetitorPriceID',
      sortBy: 'ProductCompetitorPriceID',
      structure: [
        { dataIndex: 'ProductCompetitorPriceID', type: 'int' },
        { dataIndex: 'ProductCompetitorMappingID', type: 'int', header: 'Product Competitor Mapping' },
        { dataIndex: 'CompareProductID', type: 'int', header: 'Compare Product' },
        { dataIndex: 'ProductID', type: 'int', header: 'Product' },
        { dataIndex: 'Price', type: 'float', header: 'Price' },
        { dataIndex: 'Stock', type: 'string', header: 'Stock' },
        { dataIndex: 'LastImport', type: 'date', header: 'Last Import' }
      ]
    });

    return grid;
  }
});