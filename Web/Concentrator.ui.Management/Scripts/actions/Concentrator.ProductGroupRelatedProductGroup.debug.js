Concentrator.ProductGroupRelatedProductGroup = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Related ProductGroups',
      pluralObjectName: 'Related Productgroups',
      primaryKey: ['ProductGroup'],
      sortBy: 'ProductGroup',
      url: Concentrator.route('GetAll', 'ProductGroupRelatedProductGroup'),
      updateUrl: Concentrator.route('Update', 'ProductGroupRelatedProductGroup'),
      deleteUrl: Concentrator.route('Delete', 'ProductGroupRelatedProductGroup'),
      newUrl: Concentrator.route('Create', 'ProductGroupRelatedProductGroup'),
      permissions: {
        list: 'GetProductGroupRelatedProductGroup',
        create: 'CreateProductGroupRelatedProductGroup',
        remove: 'DeleteProductGroupRelatedProductGroup',
        update: 'UpdateProductGroupRelatedProductGroup'
      },
      structure: [
                    { dataIndex: 'ProductGroup', type: 'string', header: 'Product group', editor: { xtype: 'textfield', allowBlank: false }, editable: false },
                    { dataIndex: 'RelatedProductGroups', type: 'string', header: 'Related product group', editor: { xtype: 'textfield', allowBlank: false } }
      ]

    });
    return grid;
  }

});