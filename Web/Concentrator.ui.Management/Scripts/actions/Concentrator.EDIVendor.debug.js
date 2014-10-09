Concentrator.EDIVendors = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDI Vendor',
      pluralObjectName: 'EDI Vendors',
      permissions: {
        list: 'GetEdiVendor',
        create: 'CreateEdiVendor',
        update: 'UpdateEdiVendor',
        remove: 'DeleteEdiVendor'
      },
      url: Concentrator.route('GetList', 'EDIVendor'),
      primaryKey: 'EdiVendorID',
      sortBy: 'EdiVendorID',
      structure: [
        { dataIndex: 'EdiVendorID', type: 'int', header: 'EDI Vendor',
          renderer: function (val, meta, record) {
            return record.get('Name');
          }
        },
        { dataIndex: 'Name', type: 'string' },
        { dataIndex: 'EdiVendorType', type: 'string', header: 'Edi Vendor Type' },
        { dataIndex: 'CompanyCode', type: 'string', header: 'Company Code' },
        { dataIndex: 'DefaultDocumentType', type: 'string', header: 'Default Document Type' },
        { dataIndex: 'OrderBy', type: 'string', header: 'Order By' }
      ]
    });

    return grid;
  }
});