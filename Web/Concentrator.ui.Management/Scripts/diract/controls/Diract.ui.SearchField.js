Diract.addComponent({ dependencies: ['Diract.ui.Select'] }, function() {


  Diract.ui.SearchField = (function() {

    var constructor = function(config) {

      Ext.apply(this, config);

      var self = this;

      this.store = this.searchStore || new Ext.data.JsonStore({
        autoDestroy: false,
        url: this.searchUrl,
        method: 'GET',
        root: this.rootProperty || 'results',
        idProperty: this.valueField || 'ID',
        fields: [this.valueField || 'ID', this.displayField || 'Description']
      }); 

      if (this.store.getCount() <= 0) {
        this.search('');
      }

      this.hiddenName = this.hiddenName || this.valueField;

      this.task = new Ext.util.DelayedTask(function() {
        doSearch.call(self);
      });
      Diract.ui.SearchField.superclass.constructor.call(this, config);
    };


    var doSearch = function() {

      this.expand();
      var val = this.el.dom.value;
      if (val && val.length >= this.minQueryLength) {
        this.search(this.el. dom.value);
      }
    };
    var onBlur = function(field, e) {
      this.search('');
    }
    var onKeyPress = function(field, e) {
      var key = e.getKey();

      if (e.isNavKeyPress()) {
        return;
      }

      var delay = 200;

      if (this.delaySearchPeriod)
        delay = this.delaySearchPeriod;

      this.task.delay(delay);

      if (key === e.ENTER) {
        doSearch.call(this);
      }
    };

    var SearchField = Ext.extend(Ext.form.ComboBox, {
      constructor: constructor,
      minChars: 10000,
      listeners: {
        'keypress': onKeyPress,
        'blur': onBlur
      },
      typeAhead: false,
      enableKeyEvents: true,
      editable: true,
      minQueryLength: 0,
      autoSelect: false,
      clearable: true,
      triggerAction: 'all',
      trigger1Class: 'x-form-clear-trigger',
      initComponent: function() {
        Ext.form.TwinTriggerField.superclass.initComponent.call(this);

        if (this.enableCreateItem) {
          this.trigger3Class = 'x-form-new-trigger'

          this.triggerConfig = {
            tag: 'span', cls: 'x-form-twin-triggers', cn: [
            { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger3Class },
            { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger1Class },
            { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger2Class }
        ]
          };

        } else {
          this.triggerConfig = {
            tag: 'span', cls: 'x-form-twin-triggers', cn: [
            { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger1Class },
            { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger2Class }
            ]
          };
        }
      },
      initTrigger: function() {
        var ts = this.trigger.select('.x-form-trigger', true);
        var triggerField = this;
        ts.each(function(t, all, index) {
          var triggerIndex = 'Trigger' + (index + 1);
          t.hide = function() {
            var w = triggerField.wrap.getWidth();
            this.dom.style.display = 'none';
            triggerField.el.setWidth(w - triggerField.trigger.getWidth());
            this['hidden' + triggerIndex] = true;
          };
          t.show = function() {
            var w = triggerField.wrap.getWidth();
            this.dom.style.display = '';
            triggerField.el.setWidth(w - triggerField.trigger.getWidth());
            this['hidden' + triggerIndex] = false;
          };

          if (this['hide' + triggerIndex]) {
            t.dom.style.display = 'none';
            this['hidden' + triggerIndex] = true;
          }
          this.mon(t, 'click', this['on' + triggerIndex + 'Click'], this, { preventDefault: true });
          t.addClassOnOver('x-form-trigger-over');
          t.addClassOnClick('x-form-trigger-click');
        }, this);
        this.triggers = ts.elements;
      },
      getTriggerWidth: function() {
        var tw = 0;
        Ext.each(this.triggers, function(t, index) {
          var triggerIndex = 'Trigger' + (index + 1),
                w = t.getWidth();
          if (w === 0 && !this['hidden' + triggerIndex]) {
            tw += this.defaultTriggerWidth;
          } else {
            tw += w;
          }
        }, this);
        return tw;
      },
      search: function(query) {
        this.store.reload(
        {
          params: { query: query }
        });
      },
      doExpand: function() {
        this.el.dom.focus();
        this.expand();
      },
      // private
      onDestroy: function() {
        Ext.destroy(this.triggers);
        Ext.form.TwinTriggerField.superclass.onDestroy.call(this);
      },
      onTriggerClick: function() {
        
      },
      onTrigger1Click: function() {
        if (this.enableCreateItem) {
          if (this.addItemDelegate) {
            this.addItemDelegate();
          }
        } else {
          this.setValue(null);
        }
      },
      onTrigger2Click: function() {
        if (this.enableCreateItem) {
          this.setValue(null);
        } else {
          this.search('');
          this.doExpand();
        }
      },
      onTrigger3Click: function() {
        this.doExpand();
      },
      enableCreateItem: false
    });

    return SearchField;

  })();

  Ext.reg('search', Diract.ui.SearchField);

});