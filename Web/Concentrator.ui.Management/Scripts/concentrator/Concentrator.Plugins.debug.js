// create namespace for plugins
Ext.namespace('Ext.ux.plugins');

Ext.ux.plugins.Counter = function (config) {

  Ext.apply(this, config);
};

Ext.extend(Ext.ux.plugins.Counter, Ext.util.Observable, {
  init: function (field) {


    field.on({
      scope: field,
      keyup: this.onKeyUp,
      focus: this.onFocus
    });
    Ext.apply(field, {
      onRender: field.onRender.createSequence(function () {

        this.wrap = this.el.wrap({
          tag: 'div',
          id: this.id + '-wrapper'
        }, true);
        this.enableKeyEvents = true;
      }),

      afterRender: field.afterRender.createSequence(function () {

        this.counter = Ext.DomHelper.append(Ext.get(this.wrap).id, {
          tag: 'span',
          style: 'color:#C0C0C0;padding-left:5px;',
          html: ''
        });

      }),
    });
  },
  onKeyUp: function (textField, eventObj) {

    var textFieldLength = textField.getValue().length;

    Ext.get(this.counter).update('Characters: ' + textFieldLength);

    if (textFieldLength > textField.plugins.maximumCharacters) {
      this.counter.style = 'color:red;padding-left:5px;';
    }

    if (textFieldLength <= textField.plugins.maximumCharacters) {
      this.counter.style = 'color:#C0C0C0;padding-left:5px;';
    }
  },
  onFocus: function (textField) {

    var textFieldLength = textField.getValue().length;

    Ext.get(this.counter).update('Characters: ' + textFieldLength);

    if (textFieldLength > textField.plugins.maximumCharacters) {
      this.counter.style = 'color:red;padding-left:5px;';
    }

    if (textFieldLength <= textField.plugins.maximumCharacters) {
      this.counter.style = 'color:#C0C0C0;padding-left:5px;';
    }
  }
});
