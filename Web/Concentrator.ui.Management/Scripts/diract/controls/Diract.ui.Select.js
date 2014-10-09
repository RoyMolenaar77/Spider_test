//Diract.components.push(function() {
Diract.addComponent({ dependencies: [] }, function() {

  Diract.ui.Select = (function() {

    var singleDefaults = {
      typeAhead: false,
      triggerAction: 'all',
      selectOnFocus: false,
      allowBlank: true,
      editable: false,
      delay: 700
    };

    var multiDefaults = {
      typeAhead: false,
      triggerAction: 'all',
      selectOnFocus: false,
      allowBlank: true,
      editable: false,
      delay: 700
    };

    var determineFields = function(store) {

      if (store.fields.length == 2) { // Most likely a store that contains an 'id' field and a 'display' field
        var keys = store.fields.keys;
        if (keys[0].indexOf("ID") > 0 || keys[1].indexOf("ID") < 0) {
          return { valueField: keys[0], displayField: keys[1] };
        } else {
          return { valueField: keys[1], displayField: keys[0] };
        }

      } else {
        var valueField;
        var displayField;
        Ext.each(store.fields.keys, function(field) {

          if (field.indexOf("ID") > 0) {
            if (valueField) { // More than one field that has an "ID", can't reliably say which is the value field
              return;
            } else {
              valueField = field;
            }
          } else if (field.toLowerCase().indexOf("description")) {
            displayField = field;
          }
        });
        return { valueField: valueField };
      }
    };

    var determineDefaults = function(config, that) {
      var fields;
      if (!config.valueField || !config.displayField) {
        fields = determineFields(config.store);
      }

      config.valueField = config.valueField || fields.valueField;
      config.hiddenName = config.hiddenName || config.name || config.valueField || fields.valueField;
      config.displayField = config.displayField || fields.displayField;
      config.fieldLabel = config.label;
      config.mode = config.mode || config.store.getCount() > 0 ? "local" : "remote";
      config.multiSelect = config.multi || config.multiSelect;
      config.name = config.name || config.hiddenName || fields.valueField;

      if (!config.listeners && config.callback) {

        var callbackTask = new Ext.util.DelayedTask(config.callback, that);

        config.listeners = {
          'select': function() { callbackTask.delay(config.delay); },
          'deselect': function() { callbackTask.delay(config.delay); },
          'clear': function() { callbackTask.delay(0); }
        };
      }
    };

    var constructor = function(config) {
      if (config && (config.multi || config.clearable)) {
        config = Ext.apply(this, config, multiDefaults);
        determineDefaults(config, this);
        Ext.applyIf(this, Ext.ux.Andrie.Select.prototype);
        Ext.ux.Andrie.Select.createDelegate(this)(config);
      } else {
        config = Ext.apply(this, config, singleDefaults);
        determineDefaults(config, this);
        Ext.applyIf(this, Ext.form.ComboBox.prototype);
        Ext.form.ComboBox.createDelegate(this)(config);
      }
    };

    return function(config) {
      this.constructor = constructor;
      this.constructor(config);
    };

  })();

  Diract.ui.MultiSelect = Ext.extend(Diract.ui.Select, {
    multi: true,
    valueAsArray: true,
    constructor: function(config) {
      config = Ext.apply({}, config, this);
      config.multi = config.single ? false : config.multi;
      config.valueAsArray = !config.single || config.valueAsArray;
      config.value = Ext.isArray(config.value) ? (!config.single ? config.value : undefined) : config.value;

      Diract.ui.MultiSelect.superclass.constructor.call(this, config);
    }
  });

  Ext.reg('select', Diract.ui.Select);
  Ext.reg('multiselect', Diract.ui.MultiSelect);

});