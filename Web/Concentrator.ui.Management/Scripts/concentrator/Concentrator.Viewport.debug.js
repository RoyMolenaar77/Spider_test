Concentrator.Viewport = Ext.extend(Ext.Viewport, {
  navigationPanel: {},
  tabPanel: {},
  headerPanel: {},
  refreshActiveTab: function () {
    var self = this;
    var tab = self.tabPanel.getActiveTab();

    //refresh the tab
    tab.doLayout();
  },
  lastOpened: function () {
    return Concentrator.lastOpened;
  },

  /**
  Opens a new tab or if existing already, sets it as the currently active one.
  id - html id to be used
  params - object/array of objects of the type:
  {
  key : 'some',               ---> the name of the param/dataIndex
  value : 'value',            ---> the value of this filter
  isGridFilter : true/false   ---> whether it is a column in some target grid
  }
  panel -  a pre existing panel. Will be used as the content
  menuItem - the menu item object. See Diract.ui.Navigation.js
    
  */
  open: function (id, params, panel, menuItem) {
    var tab = null;

    //try find tab
    if (this.tabPanel.items) {
      tab = this.tabPanel.items.find(function (i) { return i.id === 'tab-' + id });
    }

    var navPanel = this.navigationPanel.findById('concentrator-navigation-panel');

    var concentratorAction = null;

    if (!menuItem) {
      menuItem = navPanel.getMenuItem(id);

    }
    var dontRefresh = false;
    //if the tab has not been opened -- create it and open it
    if (!tab) {
      //if the panel is not specified -- get one from the action
      if (!panel) {
        dontRefresh = true;
        panel = new menuItem.action({
          id: 'tab-' + id,
          closable: true,
          iconCls: menuItem.iconCls,
          title: menuItem.text,
          params: params,
          autoDestroy: false,
          autoScroll: true,
          params: params,
          layout: 'fit',
          listeners: {
            close: function (p) {
              p.destroy();
            }
          }
        });
      }
      //create the tab together with the panel
      tab = this.tabPanel.add(panel);
    }
    tab.doLayout();

    this.tabPanel.setActiveTab(tab);
    
    if (params && !dontRefresh) {
      tab.refresh(params);
    }

    Concentrator.lastOpened = panel;
  },
  initComponent: function () {
    var config = {
      renderTo: Ext.getBody(),
      layout: 'border',
      items: [
          this.headerPanel,
          this.navigationPanel,
          this.tabPanel],
      layoutConfig: { animate: true }
    };

    // apply config
    Ext.apply(this, Ext.apply(this.initialConfig, config));

    // call parent
    Concentrator.Viewport.superclass.initComponent.apply(this, arguments);
    this.show();
  }
});
    