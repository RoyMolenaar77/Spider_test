/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />


/**
Panel component comprised of a tree structure with a from for editing an item
Needs more abstraction
*/
Diract.ui.MTG = (function () {

  var gr = Ext.extend(Ext.Panel, {
    showLeafs: true,
    treeConfig: {},
    attributeTreeConfig: {},
    border: false,
    layout: 'border',
    defaults: { padding: 5 },

    constructor: function (config) {

      var self = this;

      Ext.apply(this, config);

      this.connectorID = this.treeConfig.baseAttributes.ConnectorID;
      this.rootID = this.treeConfig.rootID || -1;
      this.tabPanelItems = [];
      this.initMasterLayout();
      Diract.ui.MTG.superclass.constructor.call(this, config);

      this.initTreeComponent(this.west);
    },

    /**
    Init layout -> create panels, assign regions and basic settings       
    */
    initMasterLayout: function () {

      //details view
      this.center = new Ext.TabPanel({
        region: 'center',
        margins: '5 0 0 5',
        autoScroll: true,
        autoDestroy: false,
        layoutOnTabChange: true,
        items: this.getTabPanelItems(),
        listeners: {
          'tabchange': (function (tabPanel, tab) {

            if (this.tree.getSelectionModel().selNode) {

              var mgmID = this.tree.getSelectionModel().selNode.attributes.MasterGroupMappingID;
              var nodeText = this.tree.getSelectionModel().selNode.attributes.text;

              tab.refresh(mgmID, nodeText, this.entityFormConfig);
            }

          }).createDelegate(this)
        }
      });

      this.center.setActiveTab(0);

      //tree view
      this.west = new Ext.Panel({
        region: 'west',
        width: 200,
        layout: 'fit',
        autoScroll: true,
        margins: '5 0 0 0'
      });

      //search view
      this.north = new Ext.Panel({
        region: 'north',
        height: 35,
        items: this.getSearchContainer()
      });

      this.items = [this.center, this.west, this.north];
    },
    getTabPanelItems: function () {

      this.mgmPanel = new Concentrator.ui.MasterGroupMapping();

      this.pvPanel = new Concentrator.ui.ProductViewGrid();
      this.pgPanel = new Concentrator.ui.ProductGrid();
      this.vendorProductGroupMapping = new Concentrator.ProductGroupVendors(
        {
          title: 'Vendor View',
          iconCls: 'link',
          layout: 'fit'
        });

      this.pgamPanel = new Concentrator.Referenceview(
        {
          title: 'Reference View',
          iconCls: 'bolt',
          layout: 'fit'
        });

      this.vendorProductGroupMapping.grid.enableDragDrop = true;
      this.vendorProductGroupMapping.grid.ddGroup = 'mastergroupmappingstree';
      //      
      //      this.vendorProductGroupMapping.grid.on('render', function (v) {
      //        this.dragZone = new Ext.dd.DragZone(v.getEl(), {

      //          //      On receipt of a mousedown event, see if it is within a DataView node.
      //          //      Return a drag data object if so.
      //          getDragData: function (e) {

      //            //          Use the DataView's own itemSelector (a mandatory property) to
      //            //          test if the mousedown is within one of the DataView's nodes.
      //            var sourceEl = e.getTarget(v.itemSelector, 10);

      //            //          If the mousedown is within a DataView node, clone the node to produce
      //            //          a ddel element for use by the drag proxy. Also add application data
      //            //          to the returned data object.
      //            if (sourceEl) {
      //              d = sourceEl.cloneNode(true);
      //              d.id = Ext.id();
      //              return {
      //                ddel: d,
      //                sourceEl: sourceEl,
      //                repairXY: Ext.fly(sourceEl).getXY(),
      //                sourceStore: v.store,
      //                draggedRecord: v.getRecord(sourceEl)
      //              }
      //            }
      //          },

      //          //      Provide coordinates for the proxy to slide back to on failed drag.
      //          //      This is the original XY coordinates of the draggable element captured
      //          //      in the getDragData method.
      //          getRepairXY: function () {
      //            return this.dragData.repairXY;
      //          }
      //        });
      //      });

      return [
          this.mgmPanel,
          this.pgPanel,
          this.pvPanel,
          this.vendorProductGroupMapping,
          this.pgamPanel
      ];
    },

    /**
    Adds and initializes the tree panel
    */
    initTreeComponent: function (region) {

      this.treeLoader = new Ext.tree.TreeLoader({
        dataUrl: this.treeConfig.loadDataUrl,
        baseAttrs: this.treeConfig.baseAttributes || {},
        listeners: {
          beforeload: (function (treeloader, node) {
            Ext.apply(treeloader.baseParams, this.getBaseAttributes(node));
            if (this.connectorID) {
              Ext.apply(treeloader.baseParams, { ConnectorID: this.connectorID });
            }
          }).createDelegate(this)
        }
      });

      this.contextMenu = new Ext.menu.Menu({
        items: [
          { id: 'addMGM', text: 'Add', iconCls: 'add', xtype: 'menuitem' },
          { id: 'deleteMGM', text: 'Delete', iconCls: 'delete', xtype: 'menuitem' },
          { id: 'renameMGM', text: 'Rename', iconCls: 'link', xtype: 'menuitem' }
        ],
        listeners: {
          destroy: function () { },
          itemClick: (function (item) {
            var node = this.tree.getSelectionModel().selNode;
            var params = {};

            for (var key in this.treeConfig.baseAttributes) {
              params[key] = node.attributes[key];
            }
            switch (item.id) {
              case 'addMGM':
                this.getAddNodeWindow(params, node);
                break;
              case 'deleteMGM':
                this.deleteNode(params[this.treeConfig.hierarchicalIndexID], node);
                break;
              case 'renameMGM':
                this.renameMasterGroupMapping(params, node);
                break;
            }
          }).createDelegate(this)
        }
      });


      var that = this;
      var saveFunction = function () {
        var processResult = function (result) {
          var ids = that.saveParams.ProductGroupVendorIDs,
            productIDs = that.saveParams.ProductIDs,
            storeID = that.saveParams.storeID,
            nodesToDelete = that.saveParams.nodesToDelete,
            onSuccess = that.saveParams.onSuccess,
            onFailure = that.saveParams.onFailure,
            childStoreID = that.saveParams.childStoreID,
            deleteAfterCreate = that.saveParams.deleteAfterCreate,
            selectedMasterGroupMappingID = that.saveParams.selectedMasterGroupMappingID;

          var move = false;

          if (result) {
            if (result == 'ok') {
              move = true;
            }
          }

          Diract.silent_request({
            url: Concentrator.route('AddNode', 'MasterGroupMapping'),
            params: {
              ProductGroupVendorIDs: ids,
              ProductIDs: productIDs,
              connectorID: that.connectorID,
              parentMasterGroupMappingID: storeID == -1 ? undefined : storeID,
              childMasterGroupMappingID: childStoreID,
              selectedMasterGroupMappingID: selectedMasterGroupMappingID,
              move: move
            },
            success: function (response) {
              if (onSuccess) {
                onSuccess(response.data);
                //onFailure();

                Ext.each(deleteAfterCreate, function (node) {
                  node.remove();
                });
              }
              //sync the folder created with attributeStoreID
              that.saveParams = {};

            },
            failure: function () {
              //reload tree
              if (onFailure) onFailure();
              that.saveParams = {};
            }
          });
          that.center.activeTab.refresh(id, '', this.entityFormConfig);
        }
        if (!that.saveParams.fromTree) {
          Ext.Msg.show({
            title: 'Move or copy?',
            msg: 'Do you want to move this product, or add it to both?',
            buttons: { ok: "Move", cancel: "Add to both" },
            fn: processResult,
            animEl: 'elId',
            icon: Ext.MessageBox.QUESTION
          });
        }
        else {
          processResult(false);
        }
      }


      this.tree = new Ext.tree.TreePanel({
        useArrows: this.treeConfig.useArrows || true,
        autoHeight: true,
        border: false,
        ddGroup: 'mastergroupmappingstree',
        enableDD: true,
        enableDrag: false,
        ddAppendOnly: true,
        trackMouseOver: false,
        animate: this.treeConfig.animate || true,
        containerScroll: true,
        root: {
          text: this.treeConfig.rootText || 'results',
          id: this.connectorID || this.rootID,
          draggable: false,
          leaf: false
        },
        loader: this.treeLoader,
        contextMenu: this.contextMenu,
        listeners: {
          /**
          Load and display form in center if not already displayed  
          */
          'click': (function (node, evt) {

            var id = node.attributes[this.treeConfig.hierarchicalIndexID];
            //if (id != this.rootID) {

            this.center.activeTab.refresh(id, node.text, this.entityFormConfig);

            //}
          }).createDelegate(this),

          'beforedestroy': (function () {
            if (this.contextMenu) {
              this.contextMenu.destroy();
              delete this.contextMenu;
            }
          }).createDelegate(this),

          /**
          drag drop configuration
          */

          // create nodes based on data from grid
          'nodedrop': function (tree, node, dd, e) {
            saveFunction();
          },
          'beforenodedrop': function (e) {
            //prepare data

            var ids = [],
            productIDs = [],
            connectorID = that.connectorID,
            MasterGroupMappingID = (e.target.attributes.MasterGroupMappingID == -1) ? undefined : e.target.attributes.MasterGroupMappingID,
            selectedMasterGroupMappingID = (e.tree.selModel && e.tree.selModel.selNode) ? e.tree.selModel.selNode.attributes.MasterGroupMappingID : undefined,
            nodesToDelete = [],
            deleteAfterCreate = [],
            selections = [],
            self = this,
            tg = that,
            fromTree = !(Boolean(e.data.selections)),
            dragToRoot = (e.target.attributes.MasterGroupMappingID == -1),
            isLeaf = e.data.node ? e.data.node.childNodes.length == 0 : false,
            gridNodes = [],
            childStoreID = undefined;
            //sync the database
            var childMasterGroupMappingID = ((e.data) && (e.data.node) && (e.data.node.attributes)) ? e.data.node.attributes.MasterGroupMappingID : undefined;
            // reset cancel flag
            e.cancel = false;

            // setup dropNode (it can be array of nodes)
            e.dropNode = [];

            //prepare insertion data
            if (fromTree) {
              //if dragging to the root of the tree -> dont unpack selection nodes
              if (dragToRoot) {
                if (isLeaf) {
                  //create folder for this node

                  //temp
                  if (e.data.node.attributes.AttributeID == undefined) {

                  }
                  //temp


                  var f = self.loader.createNode({
                    text: e.data.node.attributes.text,
                    leaf: false,
                    expanded: true,
                    loaded: true,
                    id: e.data.node.attributes.MasterGroupMappingID,
                    draggable: true,
                    attributes: e.data.node.attributes
                  });


                  /*
                  ,
                  children: [self.loader.createNode({
                  text: e.data.node.attributes.text,
                  leaf: true,
                  id: e.data.node.attributes.AttributeID,
                  draggable: true
                  })]
                  */

                  selections.push(f);

                  nodesToDelete.push(e.data.node);
                  if (e.data.node.parentNode && e.data.node.parentNode.attributes.MasterGroupMappingID != -1 && e.data.node.parentNode.childNodes.length == 1) {
                    nodesToDelete.push(e.data.node.parentNode);
                  }
                }
                else {
                  e.cancel = true;
                  return false;
                }
              }
              //unpack nodes
              else {
                if (!isLeaf) {
                  Ext.each(e.data.node.childNodes, function (node) {
                    selections.push(node);
                  });
                  nodesToDelete.push(e.data.node);
                } else {
                  selections.push(e.data.node);

                  //                  if (e.data.node.parentNode && e.data.node.parentNode.attributes.MasterGroupMappingID != -1 && e.data.node.parentNode.childNodes.length == 1) {
                  //                    nodesToDelete.push(e.data.node.parentNode);
                  //                  }
                }
              }

              if (e.data.node.childNodes && e.data.node.childNodes.length > 0) {
                Ext.each(e.data.node.childNodes, function (node) {

                  ids.push(node.attributes.AttributeID);
                  childStoreID = node.attributes.MasterGroupMappingID;
                });

              } else {
                //                if (e.data.node.attributes.AttributeID == undefined)                

                ids.push(e.data.node.attributes.AttributeID);
                childStoreID = e.data.node.attributes.MasterGroupMappingID;
              }

              //check the attributeStoreID 

            }
            else { //from grid

              //push every selection to the selections array
              Ext.each(e.data.selections, function (r) {
                //add grid selections to selection container
                var selectionNode = self.loader.createNode({
                  text: r.get('VendorProductGroupName'),
                  leaf: true,
                  id: r.get('ProductGroupVendorID'),
                  productID: r.get('ProductID'),
                  draggable: true
                })
                gridNodes.push(selectionNode);

                deleteAfterCreate.push(selectionNode);

                ids.push(r.get('ProductGroupVendorID'));
                productIDs.push(r.get('ProductID'));
              });

              if (dragToRoot) {

                var folderNode = self.loader.createNode({
                  text: e.data.selections[0].get('ProductGroupName'),
                  leaf: false,
                  id: e.data.selections[0].get('ProductGroupID'),
                  draggable: true
                  //children: gridNodes
                });

                selections.push(folderNode);

                deleteAfterCreate.push(folderNode);

              } else {
                Ext.each(gridNodes, function (gr) {
                  selections.push(gr);
                });
              }
            }

            e.dropNode = selections;

            //remove folder?
            that.saveParams = {
              ProductGroupVendorIDs: ids,
              ProductIDs: productIDs,
              storeID: MasterGroupMappingID,
              deleteAfterCreate: deleteAfterCreate,
              selectedMasterGroupMappingID: selectedMasterGroupMappingID,
              fromTree: fromTree,
              childStoreID: childStoreID ? childStoreID : Boolean(nodesToDelete[0]) ? nodesToDelete[0].attributes.MasterGroupMappingID : undefined,
              onSuccess: function (data) {
                if (nodesToDelete) {
                  Ext.each(nodesToDelete, function (node) {
                    node.remove();
                  });
                }
                //                Ext.each(selections, function (node, data) {
                //                  node.attributes.MasterGroupMappingID = data.MasterGroupMappingID;
                //                  
                //                  if (node.childNodes && node.childNodes.length > 0) {
                //                    Ext.each(node.childNodes, function (ch) { ch.attributes.MasterGroupMappingID = data.MasterGroupMappingID; });
                //                  }
                //                });

              },
              onFailure: function () {
                self.loader.load(self.root);
                self.root.expand(true);
              }
            };

            return true;
          },


          /**
          end of drag drop listners
          */


          contextmenu: (function (node, evt) {
            node.select();

            var c = node.getOwnerTree().contextMenu;

            if (!c.el)
              c.render();

            c.NodeID = node.id;
            c.attributes = node.attributes;
            c.contextNode = node;

            c.showAt(evt.getXY());

          }).createDelegate(this)

        }
      });

      region.add(this.tree);
    },

    /**
    Initialize filters || search
    */
    getSearchContainer: function () {

      var fieldsToSearchIn = this.searchConfig.fieldsToSearchIn;
      var searchEmptyText = 'Search in ' + fieldsToSearchIn.join(', ') + ' ...';


      //the search field with the handler for reloading
      this.searchField = new Ext.form.TextField({
        emptyText: searchEmptyText,
        width: 600,
        name: 'search'
      });


      this.searchContainer = new Ext.Panel({
        layout: 'column',
        border: false,
        items: [
          this.searchField,
          {
            style: 'margin-left: 3px',
            xtype: 'button',
            iconCls: 'magnify',
            tooltip: 'Search',
            handler: (function () {
              this.search();
            }).createDelegate(this)
          },
           {
             style: 'margin-left: 3px',
             xtype: 'button',
             iconCls: 'delete-btn',
             tooltip: 'clear',
             handler: (function () {
               this.clearSearch();
             }).createDelegate(this)
           }
        ]
      });
      if (this.searchConfig.extraButtons) {
        for (var i = 0; i < this.searchConfig.extraButtons.length; i++) {
          this.searchContainer.add(this.searchConfig.extraButtons[i]);
        }
      }

      return this.searchContainer;
    },

    search: function () {
      var query = this.searchField.getValue();

      var lo = this.tree.getLoader();
      var origUrl = lo.dataUrl;

      lo.on("beforeload", function (treeLoader, node) {
        treeLoader.baseParams.query = query;

        treeLoader.dataUrl = this.searchConfig.searchUrl;
      }, this);


      lo.on("load", function (treeloader, node) {
        treeloader.dataUrl = origUrl;

        /**
        remove all loader listeners so it cleans the configuration for the search  
        */
        treeloader.purgeListeners();

        /** 
        Reset the listener
        */
        lo.on('beforeload', function (treeloader, node) {
          Ext.apply(treeloader.baseParams, this.getBaseAttributes(node));
        }, this);
      }, this);

      lo.load(this.tree.getNodeById(this.rootID || -1));
    },

    /**
    Clear the searched text and reset filters
    */
    clearSearch: function () {
      this.searchField.setValue('');
      this.tree.getLoader().load(this.tree.getNodeById(this.rootID || -1));
    },


    getBaseAttributes: function (node, treeLoader) {
      var params = {};

      if (!treeLoader) {
        for (var key in this.treeConfig.baseAttributes) {
          params[key] = node.attributes[key];
        }
      } else {
        for (var key in treeLoader.baseAttrs) {
          params[key] = node.attributes[key];
        }
      }

      return params;
    },

    /**
    Displays a new form window to add a new node as a child to the selected one. 
    Uses the add new config
    */
    getAddNodeWindow: function (params, node) {
      var items = [];

      //this.newEntityFormConfig.items[0] = this.newEntityFormConfig.ref.createMagentoConnectorPanel(this.newEntityFormConfig.ref.connectorSystem);

      items.push(this.newEntityFormConfig.items);

      for (var key in params) {

        var name = key;
        var value = params[key];

        //the index becomes a parent
        if (name == this.treeConfig.hierarchicalIndexID) {
          name = this.treeConfig.hierarchicalParentIndexID;
        }

        if (name == this.treeConfig.hierarchicalParentIndexID && value == this.rootID) { value = ''; }

        var item = {
          name: name
        };

        if (params[this.treeConfig.hierarchicalIndexID] == this.rootID && name != this.treeConfig.hierarchicalParentIndexID && name == 'ConnectorID') {

          //TAKE out
          item.xtype = 'connector';
          item.fieldLabel = 'Connector ID';

        }
        else {
          item.xtype = 'hidden';
          item.value = value;
        }

        items.push(item);

      }

      var newForm = new Diract.ui.FormWindow({
        url: this.newEntityFormConfig.url,
        autoScroll: true,
        width: this.newEntityFormConfig.width || 400,
        height: this.newEntityFormConfig.height || 600,
        fileUpload: this.newEntityFormConfig.fileUpload,
        items: [
         items
        ],
        success: (function () {
          newForm.destroy();
          this.tree.loader.load(node);

          if (node.isLeaf()) {
            node.leaf = false;
          }

        }).createDelegate(this)
      });

      newForm.show();
    },

    /**
    Delete node
    */
    deleteNode: function (id, node) {
      if (id < 1) {
        Ext.Msg.confirm("Delete " + this.treeConfig.singularObjectName, "Are you sure you want to delete the whole mapping for this connector?",
          (function (button) {
            if (button == "yes") {
              Diract.message("Started remove tree", "This operation will process for a longer period of time and it will finish in about 5 minutes.");

              Diract.mute_request({
                url: this.treeConfig.deleteUrl,
                params: { id: id },
                success: (function () {
                  node.remove();
                }).createDelegate(this)
              });
            }
          }).createDelegate(this));
      } else {
        Ext.Msg.confirm("Delete " + this.treeConfig.singularObjectName, "Are you sure you want to delete this " + this.treeConfig.singularObjectName + " ?",
          (function (button) {
            if (button == "yes") {
              Diract.request({
                url: this.treeConfig.deleteUrl,
                params: { id: id },
                success: (function () {
                  node.remove();
                }).createDelegate(this)
              });
            }
          }).createDelegate(this));
      }
    },

    renameMasterGroupMapping: function (id, node) {

      var productGroupID = id.ProductGroupID;
      var masterGroupMappingID = id.MasterGroupMappingID;

      var grid = new Diract.ui.Grid({
        url: Concentrator.route('GetTranslations', 'ProductGroup'),
        params: {
          'productGroupID': productGroupID
        },
        updateUrl: Concentrator.route('SetTranslation', 'ProductGroup'),
        primaryKey: ['ProductGroupID', 'LanguageID'],
        sortBy: 'ProductGroupID',
        permissions: {
          list: 'GetProductGroup',
          create: 'CreateProductGroup',
          remove: 'DeleteProductGroup',
          update: 'UpdateProductGroup'
        },
        singularObjectName: 'Product group translation',
        pluralObjectName: 'Product group translations',
        structure: [
          { dataIndex: 'ProductGroupID', type: 'int' },
          { dataIndex: 'Language', type: 'string', header: 'Language' },
          { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield'} },
          { dataIndex: 'LanguageID', type: 'int' }
        ]
      });

      var wind = new Ext.Window({
        title: 'Translation management',
        items: grid,
        width: 800,
        height: 400,
        layout: 'fit'
      });

      wind.show();
    }

  });

  return gr;
})();

Concentrator.ui.MasterGroupMapping = Ext.extend(Ext.Panel, {
  title: 'Master group mapping',
  iconCls: 'cube-molecule',
  autoScroll: true,

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    Concentrator.ui.MasterGroupMapping.superclass.constructor.call(this, config);
  },

  refresh: function (mgmID, nodeText, entityFormConfig) {

    var id = mgmID;
    this.entityFormConfig = entityFormConfig;

    if ((!this.infoForm) && this.entityFormConfig) {

      this.infoForm = new Diract.ui.Form({
        url: this.entityFormConfig.url,
        buttonText: this.entityFormConfig.buttonText || 'Save',
        items: this.entityFormConfig.items,
        fileUpload: this.entityFormConfig.fileUpload,
        method: 'get'
      });

      this.add(this.infoForm);
    }

    this.doLayout();

    var params = {
      MasterGroupMappingID: id
    };

    if (this.entityFormConfig) {
      this.infoForm.load({
        url: this.entityFormConfig.loadUrl,
        params: params
      });
    }

    this.doLayout();
  }
});

Concentrator.ui.ProductViewGrid = Ext.extend(Ext.Panel, {
  title: 'Product View',
  iconCls: 'link',
  autoScroll: true,
  isScrollable: true,
  defaults: {
    collapsible: true,
    split: true,
    bodyStyle: 'padding:15px'
  },
  layout: 'fit',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    this.initPanelItems();

    Concentrator.ui.ProductViewGrid.superclass.constructor.call(this, config);
  },

  initPanelItems: function () {

    this.gridPanel = new Ext.Panel({
      title: 'Product Grid',
      border: true,
      padding: 0,
      height: 500,
      layout: 'fit'
    });

    this.items = [this.gridPanel];
  },

  refresh: function (mgmID, nodeText) {

    this.productGrid = new Diract.ui.Grid({
      pluralObjectName: 'Products',
      singularObjectName: 'Product',
      ddGroup: 'mastergroupmappingstree',
      enableDD: true,
      enableDragDrop: true,
      url: Concentrator.route('GetProduct', 'MasterGroupMapping'),
      params: {
        masterGroupMappingID: mgmID
      },
      primaryKey: 'ProductID',
      permissions: {
        all: 'Default'
      },
      customButtons: [
        {
          text: 'Mapped Products wizard',
          iconCls: 'magic-wand',
          that: this,
          handler: function (scope, record, event) {
            var mappedProductsWizard = new Concentrator.ui.MappedProductsWizard({
              selections: this.that.gridPanel.items.first().selModel.selections
            });

            mappedProductsWizard.show();
          }
        }
      ],
      structure: [
        { dataIndex: 'ProductID', type: 'int', header: 'Image', renderer: function (value, metadata, record) {
          var mediaUrl = record.get('MediaUrl');

          return '<div ><img width="100" src="' + mediaUrl + '"></img></div>';
        }, filterable: false, width: 100
        },
        { dataIndex: 'ShortContentDescription', type: 'string', header: 'Short Content Description' },
        { dataIndex: 'LongContentDescription', type: 'string', header: 'Long Content Description' },
        { dataIndex: 'Barcode', type: 'string', header: 'Barcode' },
        { dataIndex: 'BrandName', type: 'string', header: 'Brand Name' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
        { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number' },
        { dataIndex: 'MediaUrl', type: 'string' },
        { dataIndex: 'MediaPath', type: 'string' }
      ]
    });
    this.gridPanel.items.clear();
    this.gridPanel.add(this.productGrid);

    this.doLayout();
  }

});

Concentrator.ui.ProductViewPanel = Ext.extend(Ext.Panel, {
  title: 'Product View',
  iconCls: 'link',
  autoScroll: true,
  ddGroup: 'mastergroupmappingstree',
  enableDD: true,
  enableDragDrop: true,
  isScrollable: true,
  defaults: {
    collapsible: true,
    split: true,
    bodyStyle: 'padding:15px'
  },
  layout: 'fit',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    this.initPanelItems();

    Concentrator.ui.ProductView.superclass.constructor.call(this, config);
  },

  initPanelItems: function () {

    this.viewPanel = new Ext.Panel({
      title: 'Product View',
      border: true,
//      region: 'center',
      padding: 0,
      autoScroll: true,
      height: 500
    });

//    this.gridPanel = new Ext.Panel({
//      title: 'Product Grid',
//      border: true,
//      region: 'south',
//      padding: 0,
//      height: 500,
//      layout: 'fit'
//    });

    this.items = [this.viewPanel];
  },

  refresh: function (mgmID, nodeText) {

    this.productView = this.getProductView();

//    this.productGrid = new Diract.ui.Grid({
//      pluralObjectName: 'Products',
//      singularObjectName: 'Product',
//      url: Concentrator.route('GetProduct', 'MasterGroupMapping'),
//      params: {
//        masterGroupMappingID: mgmID
//      },
//      primaryKey: 'ProductID',
//      permissions: {
//        all: 'Default'
//      },
//      structure: [
//        { dataIndex: 'ProductID', type: 'int' },
//        { dataIndex: 'ShortContentDescription', type: 'string', header: 'Short Content Description' },
//        { dataIndex: 'LongContentDescription', type: 'string', header: 'Long Content Description' }
//      ]
//    });

    this.viewPanel.items.clear();
    this.viewPanel.add(this.productView);
//    this.gridPanel.add(this.productGrid);

    this.doLayout();
  },

  getProductView: function () {

    //todo: load create jsonstore

    var store = new Ext.data.ArrayStore({
      proxy: new Ext.data.MemoryProxy(),
      fields: ['hasEmail', 'hasCamera', 'id', 'name', 'price', 'screen', 'camera', 'color', 'type', 'reviews', 'screen-size'],
      sortInfo: {
        field: 'name',
        direction: 'ASC'
      }
    });

    store.loadData([
        [true, false, 1, "LG KS360", 70, "240 x 320 pixels", 2, "Pink", "Slider", 359, 2.400000],
        [true, true, 2, "Sony Ericsson C510a Cyber-shot", 180, "320 x 240 pixels", 3.2, "Future black", "Candy bar", 11, 0.000000],
        [true, true, 3, "LG PRADA KE850", 155, "240 x 400 pixels", 2, "Black", "Candy bar", 113, 0.000000],
        [true, true, 4, "Nokia N900 Smartphone 32 GB", 499, "800 x 480 pixels", 5, "Black", "Slider", 320, 3.500000],
        [true, false, 5, "Motorola RAZR V3", 65, "96 x 80 pixels", 0.3, "Silver", "Folder type phone", 5, 2.200000],
        [true, true, 6, "LG KC910 Renoir", 180, "240 x 400 pixels", 8, "Black", "Candy bar", 79, 0.000000],
        [true, true, 7, "BlackBerry Curve 8520 BlackBerry", 135, "320 x 240 pixels", 2, "Frost", "Candy bar", 320, 2.640000],
        [true, true, 8, "Sony Ericsson W580i Walkman", 70, "240 x 320 pixels", 2, "Urban gray", "Slider", 1, 0.000000],
        [true, true, 9, "Nokia E63 Smartphone 110 MB", 170, "320 x 240 pixels", 2, "Ultramarine blue", "Candy bar", 319, 2.360000],
        [true, true, 10, "Sony Ericsson W705a Walkman", 274, "320 x 240 pixels", 3.2, "Luxury silver", "Slider", 5, 0.000000],
        [false, false, 11, "Nokia 5310 XpressMusic", 170, "320 x 240 pixels", 2, "Blue", "Candy bar", 344, 2.000000],
        [false, true, 12, "Motorola SLVR L6i", 50, "128 x 160 pixels", 2, "Black", "Candy bar", 38, 0.000000],
        [false, true, 13, "T-Mobile Sidekick 3 Smartphone 64 MB", 170, "240 x 160 pixels", 1.3, "Green", "Sidekick", 115, 0.000000],
        [false, true, 14, "Audiovox CDM8600", 50, "240 x 160 pixels", 2, "Blue", "Folder type phone", 1, 0.000000],
        [false, true, 15, "Nokia N85", 70, "320 x 240 pixels", 5, "Copper", "Dual slider", 143, 2.600000],
        [false, true, 16, "Sony Ericsson XPERIA X1", 180, "800 x 480 pixels", 3.2, "Solid black", "Slider", 14, 0.000000],
        [false, true, 17, "Motorola W377", 135, "128 x 160 pixels", 0.3, "Black", "Folder type phone", 35, 0.000000],
        [true, true, 18, "LG Xenon GR500", 50, "240 x 400 pixels", 2, "Red", "Slider", 658, 2.800000],
        [true, false, 19, "BlackBerry Curve 8900 BlackBerry", 135, "480 x 360 pixels", 3.2, "Silver", "Candy bar", 21, 2.440000],
        [true, false, 20, "Samsung SGH U600 Ultra Edition 10.9", 135, "240 x 320 pixels", 3.2, "Rainbow", "Slider", 169, 2.200000]
    ]);

    var tpl = new Ext.XTemplate(
        '<ul>',
            '<tpl for=".">',
                '<li>',
                    '<img width="64" height="64" src="{[this.getThumbSrc(values.Type, values.MediaUrl,values.MediaPath, true)]}" />',
                    '<strong>{name}</strong>',
                    '<span>{price:usMoney} ({camera} MP)</span>',
                '</li>',
            '</tpl>',
        '</ul>',
        {
          getThumbSrc: function (type, src, path, restrict) {
            switch (type) {
              case 'Image':
                if (path)
                  if (restrict) {
                    return Concentrator.GetImageUrl(path, 64, 64);
                  } else {
                    return Concentrator.GetImageUrl(path);
                  }
                else
                  return Concentrator.ResizeImageFromUrl(src, 64, 64);
                break;
              case 'Video':
                return Concentrator.content('Content/images/Icons/Video.png');
                break;
              case 'Audio':
                return Concentrator.content('Content/images/Icons/Audio.png');
                break;
              default:
                return Concentrator.GetImageUrl(path, 64, 64, null, src);
                break;
            }
          }
        }
    );

    tpl.compile();

    var dataview = new Ext.DataView({
      store: store,
      tpl: tpl,
      plugins: [
        new Ext.ux.DataViewTransition({
          duration: 550,
          idProperty: 'id'
        })
      ],
      id: 'phones',
      itemSelector: 'li.phone',
      overClass: 'phone-hover',
      singleSelect: true,
      multiSelect: true,
      autoScroll: true
    });

    return dataview;
  }
});

Concentrator.ui.ProductGrid = Ext.extend(Ext.Panel, {
  title: 'Product Grid',
  iconCls: 'link',
  autoScroll: true,
  isScrollable: true,
  defaults: {
    collapsible: true,
    split: true,
    bodyStyle: 'padding:15px'
  },
  layout: 'fit',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    this.initPanelItems();

    Concentrator.ui.ProductGrid.superclass.constructor.call(this, config);
  },

  initPanelItems: function () {

    this.gridPanel = new Ext.Panel({
      title: 'Product Grid',
      border: true,
      padding: 0,
      height: 500,
      layout: 'fit'
    });

    this.items = [this.gridPanel];
  },

  refresh: function (mgmID, nodeText) {

    this.productGrid = new Diract.ui.Grid({
      pluralObjectName: 'Products',
      singularObjectName: 'Product',
      ddGroup: 'mastergroupmappingstree',
      enableDD: true,
      enableDragDrop: true,
      url: Concentrator.route('GetProduct', 'MasterGroupMapping'),
      params: {
        masterGroupMappingID: mgmID
      },
      primaryKey: 'ProductID',
      permissions: {
        all: 'Default'
      },
      customButtons: [
        {
          text: 'Mapped Products wizard',
          iconCls: 'magic-wand',
          that: this,
          handler: function (scope, record, event) {
            var mappedProductsWizard = new Concentrator.ui.MappedProductsWizard({
              selections: this.that.gridPanel.items.first().selModel.selections
            });

            mappedProductsWizard.show();
          }
        }
      ],
      structure: [
        { dataIndex: 'ProductID', type: 'int' },
        { dataIndex: 'ShortContentDescription', type: 'string', header: 'Short Content Description' },
        { dataIndex: 'LongContentDescription', type: 'string', header: 'Long Content Description' },
        { dataIndex: 'Barcode', type: 'string', header: 'Barcode' },
        { dataIndex: 'BrandName', type: 'string', header: 'Brand Name' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
        { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number' }
      ]
    });
    this.gridPanel.items.clear();
    this.gridPanel.add(this.productGrid);

    this.doLayout();
  }

});

Concentrator.ui.MasterGroupAttributeMapping = Ext.extend(Ext.Panel, {
  title: 'Product group attribute mapping',
  iconCls: 'transform',
  layout: 'border',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    this.getPanelItems();

    Concentrator.ui.ProductGroupAttributeMapping.superclass.constructor.call(this, config);
  },

  refresh: function (pgmID) {
    var self = this;
    self.pgmID = pgmID;

    if (!this.productGroupAttributeGrid) {

      this.productGroupAttributeGrid = new Concentrator.ui.Grid({
        singularObjectName: 'Product group attribute',
        pluralObjectName: 'Product group attributes',
        primaryKey: ['AttributeID'],
        sortBy: 'AttributeGroupID',
        permissions: {
          list: 'GetProductGroupAttributeMapping',
          create: 'CreateProductGroupAttributeMapping',
          update: 'UpdateProductGroupAttributeMapping',
          remove: 'DeleteProductGroupAttributeMapping'
        },
        params: {
          productGroupMappingID: self.pgmID
        },
        newParams: {
          productGroupMappingID: self.pgmID
        },
        ddGroup: 'attributetree',
        enableDragDrop: true,
        url: Concentrator.route('GetList', 'ProductGroupAttributeMapping'),
        updateUrl: Concentrator.route('Update', 'ProductGroupAttributeMapping'),
        newUrl: Concentrator.route('Create', 'ProductGroupAttributeMapping'),
        deleteUrl: Concentrator.route('Delete', 'ProductGroupAttributeMapping'),
        groupField: 'AttributeGroupID',
        newFormConfig: Ext.apply(Concentrator.FormConfigurations.existingAttributeValue, { suppressSuccessMsg: true, suppressFailureMsg: true }),
        structure: [
          { dataIndex: 'AttributeID', type: 'int' },
          { dataIndex: 'Attribute', header: 'Attribute', type: 'string' },
          { dataIndex: 'AttributeGroupID', type: 'int', header: 'Group', type: 'int', editor: { xtype: 'productattributegroup', hiddenName: 'ProductAttributeGroupID' },
            sortBy: 'AttributeGroup',
            renderer: function (val, m, rec) {
              return rec.get('AttributeGroup');
            }
          },
          { dataIndex: 'AttributeGroup', type: 'string' },
          { dataIndex: 'Sign', header: 'Sign', type: 'string', editor: { xtype: 'textfield', allowBlank: false} },
          { dataIndex: 'Index', header: 'Attribute Index', type: 'int', editor: { xtype: 'numberfield', allowBlank: false} },
          { dataIndex: 'IsVisible', header: 'Visible in filter', type: 'boolean', editor: { xtype: 'checkbox', allowBlank: false} },
          { dataIndex: 'IsSearchable', header: 'Searchable', type: 'boolean', editor: { xtype: 'checkbox', allowBlank: false} },
          { dataIndex: 'ProductGroupMappingID', type: 'int' }
        ],
        rowActions: [
          {
            text: 'View associated products',
            iconCls: 'cubes',
            handler: function (row) {
              var productGroupMappingID = self.pgmID;

              self.GetAssociatedProductsWindow(row.get('AttributeID'), productGroupMappingID);
            }
          },
          {
            text: 'Attribute options',
            iconCls: 'merge',
            handler: function (record) {

              attributeOptionsGrid = new Concentrator.ui.Grid({
                pluralObjectName: 'Attribute options',
                singularObjectName: 'Attribute option',
                primaryKey: ['AttributeID', 'AttributeOptionID'],
                permissions: {
                  list: 'GetProductAttribute',
                  create: 'UpdateProductAttribute',
                  remove: 'UpdateProductAttribute',
                  update: 'UpdateProductAttribute'
                },
                newFormConfig: Concentrator.FormConfigurations.newAttributeImage,
                formConfig: {
                  fileUpload: true
                },
                translationsUrl: Concentrator.route('GetOptionsTranslations', 'ProductAttribute'),
                translationsUrlUpdate: Concentrator.route('SetOptionsTranslation', 'ProductAttribute'),
                translationsGridStructure: [
                  { dataIndex: 'AttributeOptionID', type: 'int' },
                  { dataIndex: 'Language', type: 'string', header: 'Language' },
                  { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'language' } },
                  { dataIndex: 'LanguageID', type: 'int' }
                ],
                params: { AttributeID: record.get('AttributeID') },
                newParams: { AttributeID: record.get('AttributeID') },
                url: Concentrator.route("GetAttributeOptions", "ProductAttribute"),
                updateUrl: Concentrator.route('UpdateAttributeOption', 'ProductAttribute'),
                deleteUrl: Concentrator.route('DeleteAttributeOption', 'ProductAttribute'),
                newUrl: Concentrator.route('CreateAttributeOption', 'ProductAttribute'),
                structure:
                  [
                    { dataIndex: 'AttributeID', type: 'int' },
                    { dataIndex: 'AttributeOptionID', type: 'int' },
                    { dataIndex: 'AttributeOptionCode', type: 'string', header: 'Attribute Option Code', editor: { xtype: 'textfield', allowBlank: false } },
                    { dataIndex: 'AttributeOptionLanguage', type: 'string', header: 'Attribute Option Name' }
                  ]
              });

              var window = new Ext.Window({
                title: 'Attribute options for: ' + record.get('AttributeName'),
                width: 600,
                height: 400,
                modal: true,
                layout: 'fit',
                items: [
                         attributeOptionsGrid
                ]
              });

              window.show();

            }
          }
        ]
        });
      this.productGroupAttributeCenter.clear();
      this.productGroupAttributeCenter.add(this.productGroupAttributeGrid);
      this.doLayout();
    }

    this.productGroupAttributeGrid.store.reload({
      params: {
        productGroupMappingID: self.pgmID
      }
    });

  },

  getPanelItems: function () {

    this.productGroupAttributeCenter = new Ext.Panel({
      region: 'center',
      border: false,
      margins: '0 5 0 0',
      layout: 'fit'
    });

    this.productGroupAttributeEast = new Ext.Panel({
      region: 'east',
      ddGroup: 'tree',
      width: 200
    });

    this.items = [this.productGroupAttributeCenter, this.productGroupAttributeEast];

    //    this.productGroupAttributeMappingPanel = new Ext.Panel({
    //      id: 'ProductGroupAttributeMapping',
    //      title: 'Product group attribute mapping',
    //      iconCls: 'transform',
    //      layout: 'border',
    //      items: []
    //    });
  },

  GetAssociatedProductsWindow: function (attributeid, productGroupMappingID) {

    this.attributesgrid = new Diract.ui.ExcelGrid({
      singularObjectName: 'Product',
      pluralObjectName: 'Products',
      primaryKey: ['AttributeValueID'],
      chooseColumns: true,
      permissions: {
        list: 'GetProduct',
        create: 'CreateProduct',
        update: 'UpdateProduct',
        remove: 'DeleteProduct'
      },
      customButtons: [
        {
          text: 'Show Unmapped Records',
          iconCls: 'lightbulb-off',
          alwaysEnabled: true,
          enableToggle: true,
          listeners: {
            'toggle': (function (bt, pressed) {

              if (pressed == true) {
                bt.setIconClass('lightbulb-on');

                var params = {
                  showUnmappedValues: true
                };

                Ext.apply(this.attributesgrid.baseParams, params);
                this.attributesgrid.store.load({ params: params });
              }
              else {
                bt.setIconClass('lightbulb-off');

                var params = {
                  showUnmappedValues: false
                };

                Ext.apply(this.attributesgrid.baseParams, params);
                this.attributesgrid.store.load({ params: params });
              }

            }).createDelegate(this)
          }
        }
      ],
      params: {
        AttributeID: attributeid,
        ProductGroupMappingID: productGroupMappingID
      },
      excelRequestParams: {
        AttributeID: attributeid,
        ProductGroupMappingID: productGroupMappingID
      },
      url: Concentrator.route('GetList', 'ProductAttributeValue'),
      updateUrl: Concentrator.route('Update', 'ProductAttributeValue'),
      sortBy: 'ProductID',
      listeners: {
        'celldblclick': (function (grid, rowIndex, columnIndex, evt) {
          var rec = grid.store.getAt(rowIndex);
          var productID = rec.get('ProductID');

          var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
        }).createDelegate(this)
      },
      structure: [
        { dataIndex: 'AttributeValueID', type: 'int' },
        { dataIndex: 'ProductID', header: 'Identifier', type: 'int', editable: false },
        { dataIndex: 'BrandID', header: 'Brand', type: 'int', editable: false,
          sortBy: 'BrandName',
          renderer: function (val, met, record) {
            return record.get("BrandName");
          }
        },
        { dataIndex: 'BrandName', type: 'string' },
        { dataIndex: 'VendorItemNumber', header: 'Vendor item number', type: 'string', editable: false },
        { dataIndex: 'Value', header: 'Attribute value', type: 'string', editor: { xtype: 'textfield'} }
      ]
    });

    var window = new Ext.Window({
      title: 'Associated Products',
      width: 830,
      height: 450,
      modal: true,
      layout: 'fit',
      items: [
        this.attributesgrid
      ]
    });

    window.show();
  }

});