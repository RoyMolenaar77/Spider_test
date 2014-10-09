Concentrator.createItemDelegates = {

  brand: function(grid) {

    var form = new Diract.ui.FormWindow({
      url: Concentrator.route('Create', 'Brand'),
      buttonText: "Create",
      cancelButton: true,
      forceFit: false,
      width: 370,
      height: 134,
      title: 'New Brand',
      items: [
                        new Ext.form.TextField({
                          fieldLabel: "Brand name",
                          name: 'Name',
                          allowBlank: false,
                          inputType: 'text'
                        })],
      success: function() {
        form.destroy();
        if (grid)
          grid.getStore().reload();
      }
    });

    return form;

  },
  productGroup: function(grid) {
    var form = new Diract.ui.FormWindow({
      url: Concentrator.route('Create', 'ProductGroup'),
      title: 'New Product Group',
      buttonText: "Create",
      cancelButton: true,
      forceFit: false,
      width: 370,
      height: 230,
      items: [
                        new Ext.form.TextField({
                          fieldLabel: "English Name",
                          name: 'nameEnglish',
                          allowBlank: false,
                          inputType: 'text'
                        }),
                        new Ext.form.TextField({
                          fieldLabel: "Dutch Name",
                          name: 'nameDutch',
                          allowBlank: false,
                          inputType: 'text'
                        }),
                        new Ext.form.TextField({
                          fieldLabel: 'Score',
                          name: 'score',
                          allowBlank: false,
                          inputType: 'text'
                        })
                        ],
      success: function() {
        form.destroy();
        if (grid)
          grid.getStore().reload();
      }
    });
    return form;
  }
};