/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ui.ToggleMappingsButton = (function () {

  var constructor = function (config) {
    Ext.apply(this, config);
    this.unmappedValue = false; //|| -1;
    Concentrator.ui.ToggleMappingsButton.superclass.constructor.call(this, config);
  };

  var onToggle = function (bt, pressed) {
    if (pressed) {
      bt.setIconClass('lightbulb-on');
      bt.reloadGridStore(pressed);
    } else {
      bt.setIconClass('lightbulb-off');
      bt.reloadGridStore();
    }
  };
  var button = Ext.extend(Ext.Button, {
    text: 'Show unmapped ',
    enableToggle: true,
    iconCls: this.toggleOffCls || 'lightbulb-off',
    listeners: {
      'toggle': onToggle
    },
    toggleClass: function (pressed) {
      if (pressed) {
        this.setIconClass('lightbulb-on');
      } else {
        this.setIconClass('lightbulb-off');
      }
    },
    initComponent: function () {
      this.text = this.unmappedText || this.text + (this.pluralObjectName || 'records');
      Concentrator.ui.ToggleMappingsButton.superclass.initComponent.call(this);
    },
    reloadGridStore: function (pressed) {

      if (this.grid !== undefined) {
        if (typeof this.grid == 'function') {
          this.grid = this.grid();
        }
        var params = {};
        if (pressed) {
          this.syncStoreParams(true);
        } else {
          this.syncStoreParams();
        }

        this.grid.store.reload();
      }
    },
    syncStoreParams: function (pressed) {
      if (pressed) {
        var params = {};
        if (this.unmappedValue != null)
          this.unmappedValue = this.unmappedValue;
        else
          this.unmappedValue = -1;
        params[this.mappingField] = this.unmappedValue;
        Ext.apply(this.grid.store.baseParams, params);
      } else {
        delete this.grid.store.baseParams[this.mappingField];
      }
    },

    handleParameter: function (identification) {
//      if (typeof this.grid == 'function') {
//        this.grid = this.grid();
//      }

//      this.grid.params.Identification = identification;
    }

  });
  return button;
})();

Ext.reg('tMButton', Concentrator.ui.ToggleMappingsButton);
