Concentrator.EDIFieldMappings = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDI Field Mapping',
      pluralObjectName: 'EDI Field Mapping',
      permissions: {
        list: 'GetEdiFieldMapping',
        create: 'CreateEdiFieldMapping',
        update: 'UpdateEdiFieldMapping',
        remove: 'DeleteEdiFieldMapping'
      },
      url: Concentrator.route('GetList', 'EdiFieldMapping'),
      primaryKey: 'EdiMappingID',
      sortBy: 'EdiMappingID',
      structure: [
        { dataIndex: 'EdiMappingID', type: 'int' },
        { dataIndex: 'TableName', type: 'string', header: 'Table Name' },
        { dataIndex: 'FieldName', type: 'string', header: 'Field Name' },
        { dataIndex: 'EdiVendorID', type: 'int', header: 'Edi Vendor',
          renderer: function (val, meta, record) {
            return record.get('EdiVendorName');
          }
        },
        { dataIndex: 'EdiVendorName', type: 'string' },
        { dataIndex: 'VendorFieldName', type: 'string', header: 'Vendor Field Name' },
        { dataIndex: 'VendorTableName', type: 'string', header: 'Vendor Table Name' },
        { dataIndex: 'VendorFieldLength', type: 'string', header: 'Vendor Table Name' },
        { dataIndex: 'VendorDefaultValue', type: 'string', header: 'Vendor Default Value' },
        { dataIndex: 'EdiType', type: 'int', header: 'Edi Type' }
      ]
    });

    return grid;
  }
});