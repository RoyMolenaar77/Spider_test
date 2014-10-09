Concentrator.ui.ProductGroupMapping = Ext.extend(Ext.Panel, {
  title: 'Product group mapping',
  iconCls: 'cube-molecule',
  autoScroll: true,

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    Concentrator.ui.ProductGroupMapping.superclass.constructor.call(this, config);
  },

  refresh: function (pgmID, nodeText, entityFormConfig, detailRelationGrid) {
    var id = pgmID;
    this.entityFormConfig = entityFormConfig;
    this.detailRelationGrid = detailRelationGrid;

    if (!this.infoForm) {

      this.infoForm = new Diract.ui.Form({
        url: this.entityFormConfig.url,
        buttonText: this.entityFormConfig.buttonText || 'Save',
        items: this.entityFormConfig.items,
        fileUpload: this.entityFormConfig.fileUpload,        
        method: 'get'
      });

      this.add(this.infoForm);
    } else {
      this.infoForm.getForm().reset();
    }


    this.doLayout();

    var params = {
      ProductGroupMappingID: id
    };

    this.infoForm.load({
      url: this.entityFormConfig.loadUrl,
      params: params
    });

    if (this.detailRelationGrid) {
      if (!this.detailsGrid) {
        
        this.detailsGrid = this.detailRelationGrid;
        this.detailsGrid.params.ProductGroupMappingID = id;

        var p = new Ext.Panel({
          height: 400,
          layout: 'fit',
          region: 'south',
          items: this.detailsGrid,
          border: false
        });

        this.add(p);
      }
      else {
        var params = {
          ProductGroupMappingID: id
        };

        Ext.apply(this.detailsGrid.baseParams, params);
        this.detailsGrid.store.load({ params: params });
      }
    }

    this.doLayout();
  }

});