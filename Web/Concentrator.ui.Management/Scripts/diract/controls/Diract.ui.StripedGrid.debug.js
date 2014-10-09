Diract.addComponent({ dependencies: ['Diract.ui.Grid'] }, function () {
  Diract.ui.StripedGrid = (function () {

    var stripe = true;
    var lastOne = undefined;
   
    var grid = Ext.extend(Diract.ui.Grid, {
      
      constructor: function (config) {
        Ext.apply(this, constructor);

        Diract.ui.StripedGrid.superclass.constructor.call(this, config);
        this.initStoreListener();
      },

      initStoreListener: function () {

        
        var rowFormat = function (record) {
          if (record.get("ProductID") != lastOne) stripe = !stripe;
          return stripe ? "x-grid3-row-alt" : "";
        };

        rowFormat: rowFormat

        //        var that = this;

        //        //that.view.getRows();

        //        that.store.on('load', function (store) {

        //          var data = [];

        //          var stripedBy = this.stripedBy || this.primaryKey;
        //          var prevValue;
        //          
        //          store.each(function (item) {
        //            
        //            var value = item.get('ProductID');

        //            if (prevValue) {

        //              // Check if it' the same group
        //              if (value != prevValue) {
        //                
        //              }
        //              // Start another group
        //              else {

        //              }

        //            }

        //            prevValue = value;
        //          });

        //        }, this)
      }      

    });
    return grid;

  })();

});