﻿Ext.grid.CheckIconColumn = function (config) {
  Ext.apply(this, config);
  if (!this.id) {
    this.id = Ext.id();
  }
  this.renderer = this.renderer.createDelegate(this);
};

Ext.grid.CheckIconColumn.prototype = {
  init: function (grid) {
    this.grid = grid;
    this.grid.on('render', function () {
      var view = this.grid.getView();
      if (!this.readonly) {
        view.mainBody.on('mousedown', this.onMouseDown, this);
      }
    }, this);
  },

  onMouseDown: function (e, t) {
    if (t.className && t.className.indexOf('x-grid3-cc-' + this.id) != -1) {
      e.stopEvent();
      var index = this.grid.getView().findRowIndex(t);
      var record = this.grid.store.getAt(index);
      record.set(this.dataIndex, !record.data[this.dataIndex]);
    }
  },

  renderer: function (v, p, record) {
    p.css += ' x-grid3-check-col-td';
    return '<div class="' + (v ? 'rowAcceptIcon' : 'rowCancelIcon') + ' x-grid3-cc-' + this.id + '">&#160;</div>';
  }
};