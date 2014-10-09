Diract.addComponent({ dependencies: [] }, function() {

  Ext.apply(Ext.form.VTypes, {
    password: function(val, field) {
      if (field.initialPassField) {
        var pwd = Ext.getCmp(field.initialPassField);
        return (val == pwd.getValue());
      }
      return true;
    },
    passwordText: 'Passwords do not match'
  });
});