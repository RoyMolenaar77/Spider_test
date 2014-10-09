/// <reference path="~/Content/js/ext/ext-base-debug.js" /> 
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.Select'] }, function () {
  Diract.ui.SearchBox = (function () {
    var constructor = function (config) {
      this.attributeOptions = false;
      Ext.apply(this, config);

      this.store = this.searchStore || new Ext.data.JsonStore({
        autoDestroy: false,
        url: this.searchUrl,
        autoLoad: this.autoLoad || false,
        method: 'GET',
        root: this.hasOwnProperty('rootProperty') ? this.rootProperty : 'results',
        idProperty: this.valueField || 'ID',
        fields: [this.valueField || 'ID', this.displayField || 'Description']
      });

      this.hiddenName = this.hiddenName || this.valueField;
      this.gridField = this.gridField || this.displayField;
      this.extraParams = config.extraParams;
      var self = this;

      this.task = new Ext.util.DelayedTask(function () {
        doSearch.call(self);
      });


      Diract.ui.SearchBox.superclass.constructor.call(this, config);
    };

    var doSearch = function (scope) {
      this.expand();

      var val = this.el.dom.value;
      if (val && val.length >= this.minQueryLength) {
        this.search(this.el.dom.value);
      }
    };

    var autoCompleteCallback = function (store) {
      store.un('load', autoCompleteCallback, this);

      var value = this.autoCompleteParams[0];
      var field = this.autoCompleteParams[1];
      var index = store.findExact(field, value);

      if (index == -1) {
        index = store.find(field, value, 0, true, false);
      }

      if (index != -1) {
        this.setValue(store.getAt(index).data[this.valueField || this.displayField]);
      }
      else {
        this.clearValue();
        this.selectedIndex = -1;
      }

      this.on('expand', onExpandAfterAutoComplete);
    };

    var autoComplete = function (value, field) {
      var autoCompleteDelegate = function (value, field) {
        if (!field) {
          field = this.displayField;
        }

        if (!value) {
          value = field == this.displayField
            ? this.getRawValue()
            : this.getValue();
        }

        // First try to find an exact match
        var index = this.store.findExact(field, value);

        // If that fails
        if (index == -1) {
          // Find the partial match
          index = this.store.find(field, value, 0, true, false);
        }

        if (index == -1) {
          this.autoCompleteParams = arguments;
          this.store.on('load', autoCompleteCallback, this);
          this.search(field == this.displayField ? value : null);
        } else {
          this.setValue(this.store.getAt(index).data[this.valueField || this.displayField]);
          this.selectedIndex = index;
        }
      }.createDelegate(this);
      // SUSPICIOUS: somehow the creation of the delegate fixes a synchronisation problem with selecting a value when the store is being loaded
      // this doesnt work as I expected but did solve a problem I encountered, might need more investigation/work

      autoCompleteDelegate(value, field);
    };

    var onExpandAfterAutoComplete = function (sender) {
      this.un('expand', onExpandAfterAutoComplete);

      sender.restrictHeight();
    };

    var onBlur = function (sender) {
      var text = sender.getRawValue();
      var value = sender.getValue();

      if (!(sender.allowBlank && text.trim() == "") && text != value) {
        this.autoComplete(text);
      }
    };

    var onKeyPress = function (field, e) {
      var key = e.getKey();

      if (e.isNavKeyPress()) {
        return;
      }

      var delay = 1000;

      if (this.delaySearchPeriod)
        delay = this.delaySearchPeriod;

      this.task.delay(delay);

      if (key === e.ENTER) {
        doSearch.call(this);
      }
    };

    var OnSpecialKey = function (field, evt) {
      if (evt.getKey() == evt.ENTER) {

        if (field.getRawValue() === 'liger' || field.getRawValue() === 'lijger') {
          var window = new Ext.Window({
            height: 500,
            title: 'The concentrator liger. TIP: have you tried searching for "liger hd"',
            width: 866,
            items: [{
              html: '<img src="' + Concentrator.content('Content/images/foto.PNG') + '" />'
            }]
          });
          window.show();
        }
        else if (field.getRawValue() === 'liger hd' || field.getRawValue() === 'lijger hd') {
          var window = new Ext.Window({
            height: 500,
            title: 'The concentrator liger: HD bootleg edition',
            width: 866,
            items: [{
              html: '<img src="' + Concentrator.content('Content/images/liger_hd.PNG') + '" />'
            }]
          });
          window.show();
        }
      }

    };

    var onFocus = function (field) {
      var editor = field.gridEditor;
      var recFromServer = editor != null ? field.gridEditor.record.json : null;
      var recordValue = recFromServer != null ? recFromServer.MaxInputLength : null;
      if (this.maxInputLength || recordValue) {
        var maxLength = this.maxInputLength || recordValue;
        field.getEl().set({ maxlength: maxLength });
      }
    };

    var searchBox = Ext.extend(Ext.form.ComboBox, {
      constructor: constructor,
      autoComplete: autoComplete,
      minChars: 10000,
      minHeight: 150,
      isValid: function () {
        if (this.forceSelection && this.allowBlank && this.selectedIndex == -1 && this.getRawValue() != "")
          return false;
        if (this.forceSelection && !this.allowBlank && this.selectedIndex == -1)
          return false;
        if (!this.forceSelection && !this.allowBlank && this.selectedIndex == -1 && this.getRawValue() == "")
          return false;
        return true;
      },
      listeners: {
        'blur': onBlur,
        'keypress': onKeyPress,
        'specialkey': OnSpecialKey,
        'keyup': onKeyPress,
        'focus': onFocus
      },
      typeAhead: false,
      maxLength: this.maxLength || Number.MAX_VALUE,
      limit: 500,
      enableKeyEvents: true,
      editable: true,
      minQueryLength: 0,
      autoSelect: false,
      clearable: true,
      triggerAction: 'all',
      trigger1Class: 'x-form-clear-trigger',
      trigger2Class: 'x-form-expand-trigger',
      initComponent: function () {
        this.initTripleTrigger();
        Diract.ui.SearchBox.superclass.initComponent.call(this);
      },
      //Init triggers
      initTrigger: function () {
        var ts = this.trigger.select('.x-form-trigger', true);
        var triggerField = this;
        ts.each(function (t, all, index) {
          var triggerIndex = 'Trigger' + (index + 1);
          t.hide = function () {
            var w = triggerField.wrap.getWidth();
            this.dom.style.display = 'none';
            triggerField.el.setWidth(w - triggerField.trigger.getWidth());
            this['hidden' + triggerIndex] = true;
          };
          t.show = function () {
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
      getTriggerWidth: function () {
        var tw = 0;
        Ext.each(this.triggers, function (t, index) {
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

      onTrigger1Click: function () {
        var self = this;
        if (self.enableCreateItem) {
          self.getCreateItemForm();
        } else {
          self.setValue(null);
          this.selectedIndex = -1;
          this.store.reload({ params: '' });
        }
      },
      onTrigger2Click: function () {
        if (this.enableCreateItem) {
          this.setValue(null);
          this.selectedIndex = -1;
        } else {
          this.doChangeState();
        }
      },
      onTrigger3Click: function () {
        var self = this;
        self.doChangeState();
      },

      doChangeState: function () {
        if (this.isExpanded())
          this.doCollapse();
        else {
          if (this.clearOnExpand) {
            this.store.removeAll();
            this.lastQuery = null;
          }
          this.doExpand();
        }
      },

      doExpand: function () {
        if (this.store.getCount() <= 1) {
          this.search();
        }
        if (!this.hasFocus)
          this.focus();
        this.expand();
      },
      doCollapse: function () {
        this.collapse();
      },
      onDestroy: function () {
        Ext.destroy(this.triggers);
        Ext.form.TwinTriggerField.superclass.onDestroy.call(this);
      },

      getCreateItemForm: function () {
        var self = this;
        var config = {
          url: self.createUrl,
          title: self.singularObjectName,
          buttonText: 'Create ' + (self.singularObjectName || ''),
          cancelButton: true,
          bodyStyle: 'z-index: 12000',
          width: this.formPanelWidth || 300,
          height: this.formPanelHeight || 300,
          items: self.formItems,
          success: function () {
            form.destroy();
            self.store.load();
          }
        };

        if (this.createFormConfig) {
          Ext.apply(config, this.createFormConfig);
        }

        var form = new Diract.ui.FormWindow(config);

        form.show();
      },

      search: function (query) {
        var params = { query: query, limit: this.limit };

        if (this.searchInID) {
          params.searchID = this.searchInID;
        }

        for (var key in this.extraParams) {
          params[key] = this.extraParams[key];
        }

        if (this.searchParams) {
          Ext.apply(params, this.searchParams);
        }


        this.store.reload({ params: params });
      },

      enableCreateItem: false,
      beforeBlur: Ext.emptyFn,

      setValue: function (v) {
        var text = v;
        if (text != "") this.value = text;

        if (this.notGridContext && v) {

          var id = Number(text);
          if (this.searchInID && id) {
            //load the store
            this.search(id);
            text = "";
            this.store.on('load', (function (store) {
              if (this.searchInID) {
                var Name = store.getById(id).get(this.displayField);
                this.searchInID = false;
                this.setValue(v);
              }
            }).createDelegate(this));
          }

          /** if there is an onEnterAction */
          if (typeof (text) != 'string' && text != '') {
            if (this.onEnterAction) {

              var val = v;
              v = '';
              this.clearValue();
              this.onEnterAction(val, Ext.util.Format.ellipsis(this.store.getById(val).get(this.displayField), 20));


            }
          }
        }

        if (this.noSyncValue) {
          Diract.ui.SearchBox.superclass.setValue.call(this, text);
          return this;
        }

        if (this.updateObject) {
          if (!this.attributeOptions || (typeof (v) == 'string' && (v.trim() == ''))) {
            if (typeof (v) == 'string') { return; } //short circuit if selection is just text
          }
          if (v == null) {
            text = null;
          }
          else {
            text = this.syncCurrentValue(v);
          }
          this.updateObject.record.set(this.displayField, text);

          if (this.updateObject.record) {
            text = this.updateObject.record.get(this.displayField);
          }
        }

        // Ext.form.ComboBox
        Diract.ui.SearchBox.superclass.setValue.call(this, text);
        //if (this.updateObject) {
        this.value = v;


        var key = this.getValue();
        var index = this.store.find(this.valueField || this.displayField, key);
        this.selectedIndex = index;

        //}
        return this;
      },

      syncCurrentValue: function (value) {
        var displayValue = value;

        if (!this.updateObject.record.get(this.displayField)) //no display field in grid --> a renderer is used
        {
          //use the renderer to get to the store
          displayValue = this.updateObject.grid.colModel.getRenderer(this.updateObject.column)(value, undefined, this.updateObject.record);
        } else { //display field --> use it instead of a renderer
          displayValue = this.updateObject.record.get(this.displayField);
        }

        var r = this.findRecord(this.valueField, value);
        if (!r) {
          var recObj = new Object();
          recObj[this.displayField] = displayValue;
          recObj[this.valueField] = this.updateObject.record.get(this.valueField);
          r = new this.store.recordType(recObj);
          this.store.insert(0, r);
        }
        return r.data[this.displayField] || displayValue;
      },

      initTripleTrigger: function () {
        var isPermitted = true;

        if (this.roles) {
          isPermitted = false;
          Ext.each(this.roles, function (item) {
            if (Diract.user.isInRole(item)) {
              isPermitted = true;
            }
          }, this);
        }

        if (this.enableCreateItem && this.enableCreateItem == true && isPermitted) {
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
      }
    });

    return searchBox;

  })();

  Ext.reg('searchBox', Diract.ui.SearchBox);
});