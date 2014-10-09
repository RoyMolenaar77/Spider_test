Concentrator.EDIValidate = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDI Validation',
      pluralObjectName: 'EDI Validations',
      permissions: {
        list: 'GetEdiValidation',
        create: 'CreateEdiValidation',
        update: 'UpdateEdiValidation',
        remove: 'DeleteEdiValidation'
      },
      url: Concentrator.route('GetList', 'EDIValidate'),
      primaryKey: 'EdiValidateID',
      sortBy: 'EdiValidateID',
      structure: [
        { dataIndex: 'EdiValidateID', type: 'int' },
        { dataIndex: 'TableName', type: 'string', header: 'Table name' },
        { dataIndex: 'FieldName', type: 'string', header: 'Field Name' },
        { dataIndex: 'EdiVendorID', type: 'int', header: 'Edi Vendor',
          renderer: function (val, meta, record) {
            return record.get('EdiVendorName');
          }
        },
        { dataIndex: 'EdiVendorName', type: 'string' },
        { dataIndex: 'MaxLength', type: 'string', header: 'Max Length' },
        { dataIndex: 'Type', type: 'string', header: 'Type' },
        { dataIndex: 'Value', type: 'string', header: 'Value' },
        { dataIndex: 'IsActive', type: 'boolean', header: 'Is Active' },
        { dataIndex: 'EdiType', type: 'string', header: 'Edi Type' },
        { dataIndex: 'EdiValidationType', type: 'string', header: 'Edi Validation Type' },
        { dataIndex: 'EdiConnectionType', type: 'int', header: 'Edi Connection Type' },
        { dataIndex: 'Connection', type: 'string', header: 'Connection' }
      ]
    });

    return grid;
  }
});