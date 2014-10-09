Diract.ui.DateField = Ext.extend(Ext.form.DateField, {
  format: 'd-m-Y',
  initComponent: function () {
    this.triggerConfig = {
      tag: "span", cls: "x-form-twin-triggers", cn: [
                { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger1Class },
                { tag: "img", src: Ext.BLANK_IMAGE_URL, cls: "x-form-trigger " + this.trigger2Class }
            ]
    };
    Diract.ui.DateField.superclass.initComponent.call(this);
  },
  hideTrigger1: true,
  setValue: function (value) {
    // inherit
    Diract.ui.DateField.superclass.setValue.call(this, value);
    // process clear button visibility
    if (Ext.isEmpty(value)) this.triggers[0].hide();
    else this.triggers[0].show();
    // process daterange
    if (this.afterDateField) {
      var date = value ? this.parseDate(value) : null;
      var afterField = Ext.getCmp(this.afterDateField);
      afterField.setMaxValue(date);
      afterField.validate();
    } else if (this.beforeDateField) {
      var date = value ? this.parseDate(value) : null;
      var beforeField = Ext.getCmp(this.beforeDateField);
      beforeField.setMinValue(date);
      beforeField.validate();
    }
  },
  getTrigger: Ext.form.TwinTriggerField.prototype.getTrigger,
  initTrigger: Ext.form.TwinTriggerField.prototype.initTrigger,
  onTrigger1Click: function () { this.setValue(null); this.fireEvent('clear'); },
  onTrigger2Click: Ext.form.DateField.prototype.onTriggerClick,
  trigger1Class: "x-form-clear-trigger",
  trigger2Class: Ext.form.DateField.prototype.triggerClass
});

Ext.reg('wmsdate', Diract.ui.DateField);