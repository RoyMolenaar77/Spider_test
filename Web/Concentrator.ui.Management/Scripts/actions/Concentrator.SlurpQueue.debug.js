Concentrator.SlurpQueue = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {

    this.productSources = Ext.extend(Diract.ui.SearchBox, {
      valueField: 'ProductCompareSourceID',
      displayField: 'Name',
      searchUrl: Concentrator.route('GetPcSourceStore', 'ProductCompetitor'),
      allowBlank: false
    });

    Ext.reg('productsources', this.productSources);

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Slurp Queue',
      pluralObjectName: 'Slurp Queue',
      permissions: {
        list: 'GetSlurp',
        create: 'CreateSlurp',
        update: 'UpdateSlurp',
        remove: 'DeleteSlurp'
      },
      url: Concentrator.route('GetSlurpQueueList', 'Slurp'),
      deleteUrl: Concentrator.route('DeleteSlurpQueue', 'Slurp'),
      updateUrl: Concentrator.route('UpdateSlurpQueue', 'Slurp'),
      newUrl: Concentrator.route("CreateSlurpQueue", "Slurp"),
      primaryKey: 'QueueID',
      sortBy: 'QueueID',
      structure: [
        { dataIndex: 'QueueID', type: 'int' },
        { dataIndex: 'ProductID', type: 'int', header: 'Product',
          renderer: function (val, meta, record) {
            return record.get('ProductDescription');
          },
          editor: {
            xtype: 'product',
            allowBlank: false
          },
          editable: true
        },
        { dataIndex: 'ProductDescription', type: 'string' },
        { dataIndex: 'ProductCompareSourceID', type: 'int', header: 'Product Compare Source',
          editor: {
            xtype: 'productsources',
            allowBlank: false
          },
          editable: true,
          renderer: function (val, meta, record) {
            return record.get('Source');
          }
        },
        { dataIndex: 'Source', type: 'string' },
        { dataIndex: 'SlurpScheduleID', type: 'int', header: 'Slurp Schedule', editable: false },
        { dataIndex: 'CreationTime', type: 'date' },
        { dataIndex: 'CompletionTime', type: 'date' },
        { dataIndex: 'IsCompleted', type: 'boolean', header: 'Is Completed', editable: false }
      ]
    });

    return grid;
  }
});