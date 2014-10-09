Concentrator.Brands = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Brands',
      singularObjectName: 'Brand',
      primaryKey: 'BrandID',
      url: Concentrator.route("GetList", "Brand"),
      newUrl: Concentrator.route("Create", "Brand"),
      updateUrl: Concentrator.route("Update", "Brand"),
      deleteUrl: Concentrator.route("Delete", "Brand"),
      permissions: {
        list: 'GetBrand',
        create: 'CreateBrand',
        remove: 'DeleteBrand',
        update: 'UpdateBrand'
      },
      structure: [
        { dataIndex: 'BrandID', type: 'int', header: 'Brand ID' },
        { dataIndex: 'Name', type: 'string', header: 'Brand Name', editor: { xtype: 'textfield', allowBlank: false} }
      ],
      rowActions: [
          {
            text: 'View Media',
            iconCls: 'merge',
            predicate: function (record) { return (record.get('BrandID') > 0); },
            handler: function (record) {
              var view = new Concentrator.ui.BrandMediaViewer({
                brandID: record.get('BrandID')
              });
              view.show();
            }
          }]
    });

    return grid;
  }

});

