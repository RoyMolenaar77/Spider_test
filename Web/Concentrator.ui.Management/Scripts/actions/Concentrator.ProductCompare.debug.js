Concentrator.ProductCompare = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {
  
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Product Compare',
      pluralObjectName: 'Product Comparings',
      permissions: {
        list: 'GetProduct',
        create: 'CreateProduct',
        update: 'UpdateProduct',
        remove: 'DeleteProduct'
      },
      url: Concentrator.route('GetProductCompareList', 'ProductCompetitor'),
      primaryKey: 'CompareProductID',
      sortBy: 'CompareProductID',
      structure: [
        { dataIndex: 'CompareProductID', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int', 
            header: 'Connector',
            renderer: function (val, m, rec) { return rec.get('ConnectorName') }
        },
        { dataIndex: 'ConnectorName', type: 'string' },
        { dataIndex: 'ConnectorCustomItemNumber', type: 'string', header: 'Connector Custom Item Number' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
        { dataIndex: 'MinPrice', type: 'float', header: 'Min Price' },
        { dataIndex: 'MaxPrice', type: 'float', header: 'Max Price' },
        { dataIndex: 'HotSeller', type: 'boolean', header: 'Hot Seller' },
        { dataIndex: 'PriceIndex', type: 'float', header: 'Price Index' },
        { dataIndex: 'UPID', type: 'string', header: 'UPID' },
        { dataIndex: 'EAN', type: 'string', header: 'EAN' },
        { dataIndex: 'SourceProductID', type: 'string', header: 'Source Product ID' },
        { dataIndex: 'AveragePrice', type: 'float', header: 'Average Price' },
        { dataIndex: 'TotalStock', type: 'int', header: 'Total Stock' },
        { dataIndex: 'MinStock', type: 'int', header: 'Min Stock' },
        { dataIndex: 'MaxStock', type: 'int', header: 'Max Stock' },
        { dataIndex: 'PriceGroup1Percentage', type: 'float', header: 'Price Group 1 Percentage' },
        { dataIndex: 'PriceGroup2Percentage', type: 'float', header: 'Price Group 2 Percentage' },
        { dataIndex: 'PriceGroup3Percentage', type: 'float', header: 'Price Group 3 Percentage' },
        { dataIndex: 'PriceGroup4Percentage', type: 'float', header: 'Price Group 4 Percentage' },
        { dataIndex: 'PriceGroup5Percentage', type: 'float', header: 'Price Group 5 Percentage' },
        { dataIndex: 'TotalSales', type: 'float', header: 'Total Sales' },
        { dataIndex: 'Popularity', type: 'float', header: 'Popularity' },
        { dataIndex: 'Price', type: 'float', header: 'Price' },
        { dataIndex: 'LastImport', type: 'date', header: 'Last Import' }
      ]
    });

    return grid;
  }
});