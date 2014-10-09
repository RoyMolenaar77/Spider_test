/// <reference path="~/Scripts/ext/ext-base-debug.js" />
/// <reference path="~/Scripts/ext/ext-all-debug.js" />

Concentrator.LanguageManager = (function () {

  var languageManager = Ext.extend(Ext.Panel, {
    border: false,
    layout: 'form',
    height: 150,

    updateField: function (rec, name, value) {
      
      if(rec){
        name = rec.get('name');
        value = rec.get('value');
      }
      
      var field = this.findBy(function (field) {
        return field.name == 'LN#' + name;
      });

      if (!field || field.length == 0) throw "Hidden field storing language information could not be found";
      field[0].setValue(value);

    },

//    getHiddens : function(){
//      var vals = {};
//      
//      for(var i = 1; i < this.items.items.length; i ++){
//        vals[this.items.items[i].name] = this.items.items[i].value;
//      }

//      return vals;
//    },


    constructor: function (config) {
      Ext.apply(this, config);

      this.languageValues = new Object();
      var it = this.initPropertyGrid();

      var items = this.items || it;

      this.items = items;

      Concentrator.LanguageManager.superclass.constructor.call(this, config);
    },

    /**
    Initializes the grid responsible for presenting the languages to the user
    */
    initPropertyGrid: function () {
      var that = this,
          source = new Object(),
          items = [],
          hiddenItems = [];

      Concentrator.stores.languages.each(function (rec) {
        var name = rec.get('Name');

        source[name] = '';

        hiddenItems.push({
          xtype: 'hidden', name: 'LN#' + rec.get('Name')
        });
      }, this);

      source["All languages"] = '';
      
      this.propertyGrid = new Diract.ui.PropertyGrid({
        source: source,
        listeners: {
          'afteredit': function (eventObject) {
            var rec = eventObject.record,
                grid = eventObject.grid,
                records = grid.store.data.items;
            
            if(rec.id == 'All languages')
            {
              for(var key in source){
                source[key] = rec.get('value');
              }  
              
              grid.setSource(source);
                            
              for(var key in source){
                if(key != 'All languages'){
                  that.updateField(undefined, key, source[key]);  
                }
              }
            }
            else
            {
              that.updateField(rec);
            }
          }          
        }
      });



      items.push(this.propertyGrid);
      
      Ext.each(hiddenItems, function (item) { items.push(item) });

      return items;
    }
  });

  return languageManager;
})();

Ext.reg('languageManager', Concentrator.LanguageManager);