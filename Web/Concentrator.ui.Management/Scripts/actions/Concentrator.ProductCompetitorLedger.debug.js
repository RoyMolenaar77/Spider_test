Concentrator.ProductCompetitorLedger = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {

    var grid = new Diract.ui.Grid({
      singularObjectName: 'Product Competitor Ledger',
      pluralObjectName: 'Product Competitor Ledgers',
      url: Concentrator.route('GetProductLedgerList', 'ProductCompetitor'),
      newUrl: Concentrator.route('CreateProductCompetitorLedger', 'ProductCompetitor'),
      updateUrl: Concentrator.route('UpdateProductCompetitorLedger', 'ProductCompetitor'),
      deleteUrl: Concentrator.route('DeleteProductCompetitorLedger', 'ProductCompetitor'),
      newFormConfig: Concentrator.FormConfigurations.newProductCompetitorLedger,
      primaryKey: 'ProductCompetitorLedgerID',
      permissions: {
        list: 'GetProductCompetitor',
        create: 'CreateBrand',
        update: 'UpdateProductCompetitor',
        remove: 'DeleteProductCompetitor'
      },
      sortBy: 'ProductCompetitorLedgerID',
      structure: [
        { dataIndex: 'ProductCompetitorLedgerID', type: 'int' },
        { dataIndex: 'ProductCompetitorPriceID', type: 'int' },
        { dataIndex: 'ProductName', type: 'string', header: 'Product', editable: false },
      //        { dataIndex: 'ProductCompetitorID', type: 'int', header: 'CompetitorName',
      //          //              filter: {
      //          //                type: 'string',
      //          //                //filterField: 'BrandName'
      //          //              },

      //          editor: { xtype: 'productCompetitor', hiddenName: 'ProductCompetitorID' },
      //          renderer: function (val, metadata, record) {
      //            return record.get('Name');
      //          } 
      //        },
        {dataIndex: 'CompetitorName', type: 'string', header: 'Competitor', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'Stock', type: 'string', header: 'Stock', editable: true, editor: { xtype: 'deliveryStatus'} },
        { dataIndex: 'Price', type: 'float', header: 'Price', editable: true, editable: true, editor: { xtype: 'numberfield'} },
        { dataIndex: 'CreationTime', type: 'Date', header: 'Creation time', editable: false },
        { dataIndex: 'LastModificationTime', type: 'Date', header: 'Last modification time', editable: false }
      ]
    });

    return grid;
  }
});