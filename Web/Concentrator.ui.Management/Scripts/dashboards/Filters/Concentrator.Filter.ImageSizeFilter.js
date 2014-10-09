Ext.ns('Concentrator.Filter');

Concentrator.Filter.ImageSizeFilter = (function () {

  var filter = Ext.extend(Ext.ButtonGroup, {
      columns: 1,
      title: 'Image',
      height: 70,
      portlet : null,
      cls: 'custom-filter',
      defaults: {
        scale: 'small',
        rowspan: '3',        
        iconAlign: 'top'
      },
    constructor: function (config) {
      Ext.apply(this, config);
      if(!this.portlet) throw "Portlet must be passsed in to be bound to this filter";
      this.initFilter();

      Concentrator.Filter.ImageSizeFilter.superclass.constructor.call(this, config);
      
    },

    initFilter: function () { 
        
        var filter = this;

        this.numeric = new Ext.ux.grid.filter.NumericFilter({
            text : 'Image size',
            listeners : {
              'update' : (function(f){                
                this.portlet.filterUpdate(this.portlet, filter);                                            
              }).createDelegate(this)            
            }
        });
        this.checkBoxItem = new Ext.menu.CheckItem({
          text: 'Show images without size',
          checked: false
        });

          this.mbCheckBoxItem = new Ext.menu.CheckItem({
              text : "Megabytes",
              checked: false
            });

        this.kbCheckBoxItem = new Ext.menu.CheckItem({
       
              text : 'Kilobytes',
              checked: true
            
        });
        
        this.items = [
        {        
        iconCls: 'monitor',
        width: 40,
        text : 'Image size filter',
        menu: new Ext.menu.Menu({
          defaults: { width: 175 },
          items:
      [
         this.numeric,
        {
          text : 'Unit of data ',
          
          menu : new Ext.menu.Menu({
            defaults: { width: 175 },  
            items :[
              this.mbCheckBoxItem,
              this.kbCheckBoxItem
            
            ]
          })
        }
      ]
        })
      }];  
    },
     getValue : function(){
        return Ext.apply(this.numeric.getValue(), {
          mb : this.mbCheckBoxItem.checked,
          kb : this.kbCheckBoxItem.checked
        });
     }
  });
  return filter;
})();