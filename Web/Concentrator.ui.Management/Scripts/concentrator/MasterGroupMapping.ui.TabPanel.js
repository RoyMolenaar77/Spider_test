Ext.ns("MasterGroupMapping.ui");
(function (functionalities) {
  var getDefaultConfig = function() {
    return {
      region: 'center',
      margins: '0 0 0 0',
      autoScroll: true,
      autoDestroy: false,
      layoutOnTabChange: true,
      id: 'MasterGroupMappingCenterPanel',
      listeners: {
        'tabchange': function(tabPanel, tab) {
          if (tab.grid != undefined) {
            if (tab.grid.id == "VendorProductsPanel") {
              if (tab.grid.store.data.getCount() == 0) {
                tab.grid.store.load();
              }
            }
          }
        }
      }
    };
  };

  MasterGroupMapping.ui.TabPanel = Ext.extend(Diract.Ext.TabPanel,
    {
      constructor: function (config, masterGroupMappingTree) {
        var defaults = getDefaultConfig();
        config = config || {};
        config = Ext.apply(defaults, config);
        Ext.apply(this, config);

        config.items = this.getTabPanelItems();

        MasterGroupMapping.ui.TabPanel.superclass.constructor.call(this, config);

        this.setActiveTab(0);

      },
      getTabPanelItems: function () {

        // UnMatched Product Group Tab
        this.unMachtedProductGroup = new Concentrator.UnMatchedProductGroup(
              {
                title: 'Vendor Product Groups',
                iconCls: 'link',
                layout: 'fit',
                requires: functionalities.ViewVendorProductGroups
              });
        this.unMachtedProductGroup.grid.enableDragDrop = true;
        this.unMachtedProductGroup.grid.ddGroup = 'MasterGroupMappingDD';

        // Matched Product Group Tab
        //this.machtedProductGroup = new Concentrator.MatchedProductGroup({
        //  title: 'Matched Product Groups',
        //  iconCls: 'link',
        //  layout: 'fit'
        //});

        // Vendor Products
        this.vendorProductsPanel = new Concentrator.VendorProducts({
          title: 'Vendor Products',
          iconCls: 'link',
          layout: 'fit',
          requires: functionalities.ViewVendorProducts
        });
        this.vendorProductsPanel.grid.enableDragDrop = true;
        this.vendorProductsPanel.grid.ddGroup = 'MasterGroupMappingDD';

        // Vendor Product
        //this.MatchedVendorProductsPanel = new Concentrator.MatchedVendorProducts({
        //  title: 'Matched Vendor Products',
        //  iconCls: 'link',
        //  layout: 'fit'
        //});

        this.ConnectorMapping = new Concentrator.ConnectorMapping({
          title: 'Connector Mapping',
          iconCls: 'link',
          layout: 'fit',
          requires: functionalities.ViewConnectorMapping
        });

        return [
                  this.unMachtedProductGroup,
                  this.vendorProductsPanel,
                  this.ConnectorMapping
        ];
      },
      setProductGroupsVisible: function (visible) {
        this.setTabVisible(this.unMachtedProductGroup, visible);
      },
      setProductsVisible: function (visible) {
        this.setTabVisible(vendorProductsPanel);
      },
      setTabVisible: function (tab, visible) {
        if (visible) {
          this.unhideTabStripItem(tab);
          tab.show();
        }
        else {
          this.hideTabStripItem(tab);
          tab.hide();
        }

        if (!visible && !getActiveTab().isVisible()) {
          var indexOfFirstVisibleTab = this.items.indexOf(Enumerable.From(this.items).FirstOrDefault("$.isVisible()"));
          if (indexOfFirstVisibleTab >= 0) {
            this.setActiveTab(indexOfFirstVisibleTab);
          }
        }
      }
    });

})(Concentrator.MasterGroupMappingFunctionalities);