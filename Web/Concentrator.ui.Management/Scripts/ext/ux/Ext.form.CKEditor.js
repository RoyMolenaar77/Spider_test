var CKEDITOR_BASEPATH = document.location.href + '/Scripts/diract/misc/CKEditor/';

Ext.form.CKEditor = function (config) {
  this.config = config;
  Ext.form.CKEditor.superclass.constructor.call(this, config);
};

Ext.extend(Ext.form.CKEditor, Ext.form.TextArea, {
  hideLabel: false,
  constructor: function (config) {
    this.config = config = config || {};
    this.config.listeners = config.listeners || {};
    Ext.applyIf(this.config.listeners, {
      beforedestroy: this.onBeforeDestroy
                              .createDelegate(this),
      scope: this
    });
    Ext.form.CKEditor.superclass.constructor.call(this, config);
  },
  onBeforeDestroy: function () {
    this.ckEditor.destroy();
  },
  onRender: function (ct, position) {
    if (!this.el) {
      this.defaultAutoCreate = {
        tag: "textarea",
        autocomplete: "off"
      };
    }
    Ext.form.TextArea.superclass.onRender.call(this, ct, position);
    this.ckEditor = CKEDITOR.replace(this.id, Ext.apply({
      width: this.config.width,
      height: this.config.height,
      baseFloatZIndex: 100001,
      protectedSource: [/\r|\n/g]
    }, this.config.CKConfig));
        
  },

  setValue: function (value) {
    if (this.ckEditor) {
      if (Ext.isEmpty(value)) {
        value = "";
      }
      Ext.form.TextArea.superclass.setValue.apply(this, [value]);

      this.ckEditor.setData(value);
    }
  },

  getValue: function () {
    if (this.ckEditor) {
      this.ckEditor.updateElement();
      this.value = this.ckEditor.getData();
      return Ext.form.TextArea.superclass.getValue.apply(this);
    }
  },

  getRawValue: function () {
    if (this.ckEditor) {
      this.ckEditor.updateElement();
      this.value = this.ckEditor.getData();
      return Ext.form.TextArea.superclass.getRawValue.apply(this);
    }
  }
});
Ext.reg('ckeditor', Ext.form.CKEditor);