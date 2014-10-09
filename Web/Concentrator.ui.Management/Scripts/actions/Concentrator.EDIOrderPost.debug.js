Concentrator.EDIOrderPost = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDI Order Post',
      pluralObjectName: 'EDI Order Posts',
      permissions: {
        list: 'GetEdiOrderPost',
        create: 'CreateEdiOrderPost',
        update: 'UpdateEdiOrderPost',
        remove: 'DeleteEdiOrderPost'
      },
      url: Concentrator.route('GetList', 'EdiOrderPost'),
      primaryKey: 'EdiOrderID',
      sortBy: 'EdiOrderID',
      structure: [
        { dataIndex: 'EdiOrderID', type: 'int', header: 'EDI Order ID' },
        { dataIndex: 'CustomerID', type: 'int', header: 'Customer',
          renderer: function (val, meta, record) {
            return record.get('CustomerName');
          }
        },
        { dataIndex: 'CustomerName', type: 'string' },
        { dataIndex: 'EdiBackendOrderID', type: 'int', header: 'EDI Backend Order ID' },
        { dataIndex: 'CustomerOrderID', type: 'string', header: 'Customer Order ID' },
        { dataIndex: 'Processed', type: 'boolean', header: 'Processed' },
        { dataIndex: 'Type', type: 'string', header: 'Type' },
        { dataIndex: 'PostDocument', type: 'string', header: 'Post Document' },
        { dataIndex: 'PostDocumentUrl', type: 'string', header: 'Post Document Url' },
        { dataIndex: 'PostUrl', type: 'string', header: 'Post Url' },
        { dataIndex: 'Timestamp', type: 'date', header: 'Time stamp' },
        { dataIndex: 'ResponseRemark', type: 'string', header: 'Response Remark' },
        { dataIndex: 'ResponseTime', type: 'int', header: 'Response Time' },
        { dataIndex: 'ProcessedCount', type: 'int', header: 'Processed Count' },
        { dataIndex: 'EdiRequestID', type: 'int', header: 'Edi Request ID' },
        { dataIndex: 'ErrorMessage', type: 'string', header: 'Error Message' },
        { dataIndex: 'BSKIdentifier', type: 'int', header: 'BSK Identifier' },
        { dataIndex: 'DocumentCounter', type: 'int', header: 'Document Counter' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: function (val, meta, record) {
            return record.get('Connector')
          },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'Connector', type: 'string' }
      ]
    });

    return grid;
  }
});