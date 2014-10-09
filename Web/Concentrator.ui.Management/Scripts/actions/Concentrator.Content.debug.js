Concentrator.Content = Ext.extend(Concentrator.GridAction, {

  needsConnector: true,
  getPanel: function () {

    var grid = new Diract.ui.ExcelGrid({
      primaryKey: 'ConcentratorProductID',
      managePage: 'content-item',
      defaultAction: 'GetList',
      overridePageSize: true,
      controller: 'MissingContent',
      singularObjectName: 'Content',
      pluralObjectName: 'Content',
      customButtons: [
      { xtype: 'tMButton', grid: function () { return grid; }, mappingField: 'hasImage', unmappedValue: false, unmappedText: 'Show content without image' },
      { xtype: 'tMButton', grid: function () { return grid; }, mappingField: 'VendorOverlay', unmappedValue: false, unmappedText: 'Show content Overlay products' },
      { xtype: 'tMButton', grid: function () { return grid; }, mappingField: 'VendorBase', unmappedValue: false, unmappedText: 'Show content Base products' },
      { xtype: 'tMButton', grid: function () { return grid; }, mappingField: 'PreferredContent', unmappedValue: false, unmappedText: 'Show products without preferred content' }
      ],
      sortBy: 'VendorItemNumber',
      autoLoadStore: false,
      chooseColumns: true,
      params: { connectorID: this.connectorID || Concentrator.user.connectorID },
      permissions: {
        list: 'GetContent',
        create: 'CreateContent',
        remove: 'DeleteContent',
        update: 'UpdateContent'
      },
      url: Concentrator.route("GetList", "MissingContent"),
      updateUrl: Concentrator.route("Update", "MissingContent"),
      structure: [
      { dataIndex: 'ConcentratorProductID', type: 'int' },
      { dataIndex: 'Active', type: 'boolean', header: 'Active' },
      { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
      { dataIndex: 'CustomItemNumber', type: 'string' },
      { dataIndex: 'BrandName', type: 'string' },
      { dataIndex: 'ShortDescription', type: 'string', header: 'Short Description' },
      { dataIndex: 'Image', type: 'boolean', header: 'Image' },
      { dataIndex: 'HasDescription', type: 'boolean', header: 'NL Long description' },
      { dataIndex: 'HasFrDescription', type: 'boolean', header: 'FR Long description' },
      { dataIndex: 'Specifications', type: 'boolean' },
      { dataIndex: 'CreationTime', type: 'date' },
      { dataIndex: 'LastModificationTime', type: 'date' },

      { dataIndex: 'IsConfigurable', type: 'boolean', header: 'Is Configurable', editable: false },
      { dataIndex: 'ShopWeek', type: 'string', header: 'Shop week', editable: false },
      { dataIndex: 'QuantityOnHand', type: 'int', header: 'Stock', editable: false },
      { dataIndex: 'SentToWehkamp', type: 'boolean', header: 'Sent To Wehkamp', editable: true },
      { dataIndex: 'ReadyForWehkamp', type: 'boolean', header: 'Ready For Wehkamp', editable: false },
      { dataIndex: 'SentToWehkampAsDummy', type: 'boolean', header: 'Sent dummy product', editable: false },

      { dataIndex: 'WehkampProductNumber', type: 'string', header: 'Wehkamp number', editable: false },
      { dataIndex: 'BtF', type: 'boolean', header: 'Be the first', editable: 'false' }
      ],
      listeners: {
        'celldblclick': (function (grid, rowIndex, columnIndex, evt) {
          var rec = grid.store.getAt(rowIndex);
          var productID = rec.get('ConcentratorProductID');

          var factory = new Concentrator.ProductBrowserFactory({ productID: productID, isSearched: true });

        }).createDelegate(this)
      }
    });

    return grid;
  }
});