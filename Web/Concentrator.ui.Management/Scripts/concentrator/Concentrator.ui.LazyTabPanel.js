Concentrator.ui.LazyTabPanel = Ext.extend(Ext.TabPanel, {
  region: 'center',
  xtype: 'tabpanel',
  enableTabScroll: true,
  activeTab: 0,
  border: true,
  deferredRender: false,
  layoutOnTabChange: true,

  constructor: function (config) {

    Concentrator.ui.LazyTabPanel.superclass.constructor.call(this, config);

    this.lazyLoadingTabs = [];

    for (var i = 0; i < config.tabs.length; i++) {
      var c = config.tabs[i];

      if (c.lazyLoad !== false && i > 0) {
        var tab = {
          title: c.title,
          tabCls: c.tabCls,
          border: false,
          frame: false,
          layout: 'fit',
          disabled: c.disabled
        };
        this.add(tab);
        this.lazyLoadingTabs[c.title] = c;
      } else {
        var tab = typeof c.items == "function" ? (c.scope ? c.items.call(c.scope, c.args) : c.items(c.args)) : c.items;
        if (tab != null) {
          tab.title = c.title;
          tab.tabCls = c.tabCls;
          this.add(tab);
        }
      }
    }
  },

  listeners: {
    'tabchange': function (panel, newTab) {

      var c = this.lazyLoadingTabs[newTab.title];
      if (c) {
        var content = typeof c.items == "function" ? (c.scope ? c.items.call(c.scope, c.args) : c.items(c.args)) : c.items;
        content.border = false;
        newTab.add(content);
        panel.doLayout();
        this.lazyLoadingTabs[newTab.title] = null;
      }
    }
  }

});

Concentrator.LazyTabWindow = Ext.extend(Ext.Window, {
  layout: 'fit',
  width: 600,
  height: 400,
  modal: true,
  constructor: function (config) {
    config = Ext.apply({}, config);

    config.items = new Concentrator.ui.LazyTabPanel({ tabs: config.items });

    Concentrator.ui.superclass.constructor.call(this, config);
  }

});
