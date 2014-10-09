
/**
Component for price rule management.
The component needs to be initialized with a ConnectorID, ProductGroupLabel and a ProductGroupMappingID 
*/
Concentrator.ui.PricingManager = (function () {


  var pan = Ext.extend(Ext.Panel,
  {
    /**
    Product group mapping for which the price rules are defined. 
    If not overriden it will remain null which is not allowed.
    */
    productGroupMappingID: null,


    /**
    Connector for which the price rules are defined. 
    If not overriden it will remain null which is not allowed.    
    */
    connectorID: null,

    /**
    Product group label for the current level
    */
    productGroupLabel: null,

    /**
    Stores references to all rendered components
    */
    componentMap: {},

    border: false,

    /**
    Initializes the component
    */
    constructor: function (config) {
      Ext.apply(this, config);

      this.initGeneralLayout();

      Concentrator.ui.PricingManager.superclass.constructor.call(this, config);
    },

    /**
    Initiliazes the main layout for the price manager.
    The layout of the component is a border one. It displays:
    --> Hierarchy of Brands and products found in this level (west)
    --> All rules for the current item in the above hierarchy (if none is displayed, show all for this level) (center)
    --> Posibility to create new rules (west)
    */
    initGeneralLayout: function () {
      this.layout = 'border';
      var west = this.getWestComponent(),
          center = this.getCenterComponent();


      this.items = [west, center];


      Ext.apply(this.componentMap, {
        west: west,
        center: center
      });
    },

    /**
    Initializes the component that is in the west region    
    */
    getWestComponent: function () {
      var brandProductMenu = this.getBrandProductHierarchy();

      var west = new Ext.Panel({
        region: 'west',
        width: 175,
        margins: '0 5 0 0 ',
        title: 'Level to set price rule on',
        collapsible: true,
        items: brandProductMenu
      });

      this.componentMap.brandProductMenu = brandProductMenu;
      return west;
    },

    /**
    Initializes and returns the central component  
    */
    getCenterComponent: function () {
      var center = new Ext.Panel({
        region: 'center',
        title: 'Existing rules',
        layout: 'fit'
      });

      return center;
    },


    /**
    Initializes the brand -> product hierarchy on which product rules can be applied
    */
    getBrandProductHierarchy: function () {

      //init tree
      var brandProductMenu = new Ext.tree.TreePanel({
        useArrows: true,
        autoHeight: true,
        border: false,
        id: 'matched-attributes-tree',
        animate: true,
        containerScroll: true,
        root: {
          text: this.productGroupLabel,
          id: -1,
          leaf: false
        },
        //tree loader
        loader: {
          dataUrl: Concentrator.route('GetBrandProductHierarchy', 'ProductGroupMapping'),
          baseAttrs:
          {
            connectorID: this.connectorID,
            productGroupMappingID: this.productGroupMappingID
          },
          listeners:
          {
            'beforeload': (function (treeLoader, node) {
              //collect here additional information -- brandid || productid
              var attributes = node.attributes,
                  productID = attributes['productID'],
                  brandID = attributes['brandID'],
                  productGroupMappingID = attributes["productGroupMappingID"],
                  connectorID = attributes["connectorID"];
              
              Ext.apply(treeLoader.baseParams, {
                productID: productID,
                brandID: brandID,
                productGroupMappingID: productGroupMappingID,
                connectorID: connectorID
              });

            }).createDelegate(this)
          }//eof listeners
        }, //eof tree loader
        //context menu
        contextMenu: new Ext.menu.Menu({
          items: [{ id: 'winning', text: 'Add price rule', iconCls: 'add', xtype: 'menuitem'}],
          listeners: {
            'itemClick': (function (item) {

              var node = this.attributeTree.getSelectionModel().selNode;

              switch (item.id) {
                case 'winning':
                  //launch rule wizard
                  alert('wizard coming up');
                  break;
              }
            }).createDelegate(this)
          }
        })//eof context menu
      }); //eof tree

      return brandProductMenu;
    }
  });

  return pan;
})();