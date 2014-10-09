Concentrator.AdvancedPricesGrid = Ext.extend(Concentrator.BaseAction, {
  needsConnector: true,

  getPanel: function () {

    var grid = new Diract.ui.ExcelGrid({
      primaryKey: 'ProductID',
      managePage: 'advanced-prices-grid',
      controller: 'AdvancedPrices',
      defaultAction: 'GetList',
      singularObjectName: 'Advanced price',
      pluralObjectName: 'Advanced prices',
      priceLabelColumns: true,
      customButtons: [{ xtype: 'tMButton', grid: function () { return grid; }, mappingField: 'Image', unmappedValue: false, unmappedText: 'Export from template'}],
      sortBy: 'CustomItemNumber',
      autoLoadStore: true,
      chooseColumns: true,
      permissions: {
        list: 'GetContent',
        create: 'CreateContent',
        remove: 'DeleteContent',
        update: 'UpdateContent'
      },
      url: Concentrator.route("GetList", "AdvancedPricing"),
      updateUrl: Concentrator.route("Update", "AdvancedPricing"),
      structure: [
            { dataIndex: 'ProductID', type: 'int' },
        { dataIndex: 'CustomItemNumber', type: 'int' },
        { dataIndex: 'ProductTitel', type: 'string', header: 'Title' },
        { dataIndex: 'CurrentPrice', type: 'float', header: 'Current price' },
        { dataIndex: 'CurrentCostPrice', type: 'float', header: 'Current costprice' },
        { dataIndex: 'CurrentMarge', type: 'float', header: 'Current marge' },
        { dataIndex: 'CurrentRank', type: 'int', header: 'Current rank', editor: {
          xtype: 'numberfield'
        }, editable: true
        },
        { dataIndex: 'ScanDate', type: 'date', header: 'Scandate' },
        { dataIndex: 'NewPrice', type: 'float', header: 'New price', editor: {
          xtype: 'numberfield'
        }, editable: true
        },
        { dataIndex: 'NewMarge', type: 'float', header: 'New marge', editor: {
          xtype: 'numberfield'
        }, editable: true
        },
        { dataIndex: 'minPriceInc', type: 'float', header: 'Min price (inc)' },
        { dataIndex: 'maxPriceInc', type: 'float', header: 'Max price (inc)' },
        { dataIndex: 'PromisedDeliveryDate', type: 'string', header: 'Promised Delivery Date' },
        { dataIndex: 'PriceLabel', type: 'string', header: 'Price label', editor: {
          xtype: 'textfield'
        }, editable: true
      },
        { dataIndex: 'ComparePricePosition', type: 'int', header: 'Compare Price Position' },
         { dataIndex: 'ProductCompareSourceID', header: 'Product Compare Source', type: 'int',
           editor: { xtype: 'productSources' },
           editable: false,
           renderer: function (val, meta, record) {
             return record.get('Source');
           }
                }

      ],
      rowFormat: function (row, index) {
        var data = row.data,
        cls = '';
        if (data.PriceLabel) {
          cls = 'grid-row-red';
        }
        return cls;
      }

    });

    return grid;
  }
});