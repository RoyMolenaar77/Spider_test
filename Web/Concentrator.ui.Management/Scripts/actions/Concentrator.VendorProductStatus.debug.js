Concentrator.VendorProductStatus = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {
    var btn = new Concentrator.ui.ToggleMappingsButton(
    {
      roles: ['ShowUnmappedProducts'],
      grid: function () { return grid }, mappingField: 'ConcentratorStatusID'
    });

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor Status',
      pluralObjectName: 'Vendor Statuses',
      url: Concentrator.route('GetList', 'VendorProductStatus'),
      updateUrl: Concentrator.route('Update', 'VendorProductStatus'),
      deleteUrl: Concentrator.route('Delete', 'VendorProductStatus'),
      customButtons: [{ text: 'Show all unmapped', iconCls: 'lightbulb'}],
      primaryKey: ['VendorID', 'VendorStatus', 'ConcentratorStatusID'],
      permissions: {
        update: 'UpdateVendorProductStatus',
        remove: 'UpdateVendorProductStatus',
        list: 'GetVendorProductStatus'
      },
      onGridFilterInitialized: function () {
        btn.toggleClass(true);
      },
      customButtons: [btn],
      sortBy: 'VendorID',
      structure: [
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',          
          renderer: function (val, meta, record) {
            return record.get("Vendor");
          },
          filter: {
            type: 'list',
            labelField: 'VendorName',
            store: Concentrator.stores.vendors
          }
        },
        { dataIndex: 'Vendor', type: 'string' },
        { dataIndex: 'VendorStatus', type: 'string', header: 'Vendor Status' },
        { dataIndex: 'ConcentratorStatusID', type: 'int', header: 'Concentrator Status',
          renderer: function (val, m, re) {
            return re.get('ConcentratorStatus');
          },
          editor: {
            xtype: 'concentratorstatus',
            roles: ['CreateProductStatus']
          },
          filter: {
            type: 'string',
            fieldLabel: 'Status',
            filterField: 'ConcentratorStatus'
          }
        },
        { dataIndex: 'ConcentratorStatus', type: 'string' }
      ]
    });
    return grid;
  }
});