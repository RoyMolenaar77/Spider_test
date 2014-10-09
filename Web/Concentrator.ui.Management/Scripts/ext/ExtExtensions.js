Ext.apply(Ext.menu.Menu.prototype,
{
  removeAllCheckItems: function () {
    var toRemove = this.findBy(function (it) {
      if (it instanceof Ext.menu.CheckItem && !it.suppressRemove) {
        //uncheck
        return true;
      }
    }, this);

    Ext.each(toRemove, function (it) {
      this.remove(it);
    }, this);

  },

  checkAll: function (suppressEvent) {
    this.findBy(function (it) {
      if (it instanceof Ext.menu.CheckItem) {
        //uncheck
        it.setChecked(true, suppressEvent);
        return true;
      }
    }, this);
  },

  uncheckAll: function (suppressEvent) {
    this.findBy(function (it) {
      if (it instanceof Ext.menu.CheckItem) {
        //uncheck
        it.setChecked(false, suppressEvent);
        return true;
      }
    }, this);
  }
});