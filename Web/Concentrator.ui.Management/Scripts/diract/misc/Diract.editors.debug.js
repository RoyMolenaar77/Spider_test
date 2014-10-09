DefaultEditorRepository = function() {

  this.Int = Ext.extend(Ext.form.NumberField, {
    allowDecimals: false,
    allowNegative: false
  });

  this.Decimal = Ext.extend(Ext.form.NumberField, {
    allowDecimals: true,
    allowNegative: false,
    decimalSeparator: ','
  });

  this.Int_Signed = Ext.extend(Ext.form.NumberField, {
    allowDecimals: false,
    allowNegative: false
  });

  this.Decimal_Signed = Ext.extend(Ext.form.NumberField, {
    allowDecimals: true,
    allowNegative: false,
    decimalSeparator: ','
  });

  Ext.reg('int', this.Int);
  Ext.reg('decimal', this.Decimal);
  Ext.reg('int_signed', this.Int_Signed);
  Ext.reg('decimal_signed', this.Decimal_Signed);

};
