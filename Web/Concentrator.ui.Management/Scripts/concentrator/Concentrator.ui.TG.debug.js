/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />


/**
Panel component comprised of a tree structure with a from for editing an item
Needs more abstraction
*/
Diract.ui.TG = (function () {

  var gr = Ext.extend(Ext.Panel, {

    /**
    Boolean
    Show leaf elements in the tree view    
    */
    showLeafs: true,

    /**
    Required tree configuration
    */
    treeConfig: {},

    attributeTreeConfig: {},

    border: false,

    layout: 'border',

    defaults: { padding: 5 },

    constructor: function (config) {
      this.clipboardArray = [];

      var connectorField = new Ext.extend(Diract.ui.SearchBox, {
        valueField: 'ConnectorID',
        displayField: 'Name',
        fieldLabel: 'Connector',
        allowBlank: false,
        searchUrl: Concentrator.route('Search', 'Connector')
      });

      var selector = new connectorField();

      var self = this;

      Ext.apply(this, config);

      this.connectorID = this.treeConfig.baseAttributes.ConnectorID;
      this.rootID = this.treeConfig.rootID || -1;
      this.tabPanelItems = [];
      this.initMasterLayout();
      Diract.ui.TG.superclass.constructor.call(this, config);

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
        layoutOnTabChange: true,
        items: this.getTabPanelItems(),
        listeners: {
          'tabchange': (function (tabPanel, tab) {

            if (this.tree.getSelectionModel().selNode) {

              // Retreives the parameters
              var pgmID = this.tree.getSelectionModel().selNode.attributes.ProductGroupMappingID;
              var nodeText = this.tree.getSelectionModel().selNode.attributes.text;

              // Passes Product Group Mapping ID to all tabs, nodeText to Price Rule and entityFormconfig
              // to Product Group Attribute Mapping
              tab.refresh(pgmID, nodeText, this.entityFormConfig);
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

      //this.productGroupMappingPanel = new Ext.Panel({ id: 'ProductGroupMapping', title: 'Product group mapping', iconCls: 'cube-molecule', autoScroll: true });
      this.pgmPanel = new Concentrator.ui.ProductGroupMapping();
      this.pgamPanel = new Concentrator.ui.ProductGroupAttributeMapping();
      this.ppgvPanel = new Concentrator.ui.PreferredProductGroupVendor(this.connectorID);
      this.prPanel = new Concentrator.ui.PriceRule();

      //sets and inits the attributematching tree component
      this.initAttributeTreeComponent(this.pgamPanel.productGroupAttributeEast);

      return [
      //this.productGroupMappingPanel,
          this.pgmPanel,
          this.pgamPanel,
          this.ppgvPanel,
          this.prPanel
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
        //autoDestroy : true,

        items: [
          { id: 'add', text: 'Add to', iconCls: 'add', xtype: 'menuitem' },
          { id: 'delete', text: 'Delete', iconCls: 'delete', xtype: 'menuitem' },
          '-',
          { id: 'publish', text: 'Publish to', iconCls: 'magic-wand', xtype: 'menuitem' },
          { id: 'addProd', text: 'Add product', iconCls: 'add', xtype: 'menuitem' },
          { id: 'scan', text: 'Add slurp schedule', iconCls: 'add', xtype: 'menuitem' },
          { id: 'wizard', text: 'Manage pricing rules', iconCls: 'magic-wand', xtype: 'menuitem' },
          { id: 'seo', text: 'Manage SEO texts', iconCls: 'magic-wand', xtype: 'menuitem' },
          { id: 'translateDescription', text: 'Translate description', iconCls: 'magic-wand', xtype: 'menuitem' },
          { id: 'translateCustomLabels', text: 'Custom labels', iconCls: 'magic-wand', xtype: 'menuitem' },
          '-',
          { id: 'relation', text: 'Add to clipboard', iconCls: 'upload-icon', xtype: 'menuitem' }
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
              case 'add':
                this.getAddNodeWindow(params, node);
                // contextMenu: new Ext.menu.Menu({});
                break;
              case 'delete':
                this.deleteNode(params[this.treeConfig.hierarchicalIndexID], node);
                break;
              case 'publish':
                this.publishNode(params, node);
                break;
              case 'scan':
                this.getCompareWindow(params, node);
                break;
              case 'wizard':
                this.getPriceRuleWizard(params, node);
                break;
              case 'seo':
                this.getSeoWindow(params, node);
                break;
              case 'addProd':
                this.getProductWindow(params, node);
                break;
              case 'relation':
                this.addRelation(params, node);
                break;
              case 'translateDescription':
                this.translateDescription(params, node);
                break;
              case 'translateCustomLabels':
                this.translateCustomLabels(params, node);
                break;

            }
          }).createDelegate(this)
        }
      });

      this.tree = new Ext.tree.TreePanel({
        useArrows: this.treeConfig.useArrows || true,
        autoHeight: true,
        border: false,
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
            if (id != this.rootID) {
              var detailRelationGrid = this.detailRelationGrid(id);
              detailRelationGrid.store.setBaseParam('ProductGroupMappingID', id);
              detailRelationGrid.store.load();
              this.center.activeTab.refresh(id, node.text, this.entityFormConfig, detailRelationGrid);
            }
          }).createDelegate(this),

          'beforedestroy': (function () {
            if (this.contextMenu) {
              this.contextMenu.destroy();
              delete this.contextMenu;
            }
          }).createDelegate(this),

          contextmenu: (function (node, evt) {
            node.select();

            var c = node.getOwnerTree().contextMenu;

            //if(!c.el)
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
    Adds and initializes the >>attribute<< matching tree panel
    */
    initAttributeTreeComponent: function (region) {

      this.contextMenu = new Ext.menu.Menu({
        //autoDestroy : true,

        items: [
          { id: 'add', text: 'Add to', iconCls: 'add', xtype: 'menuitem' },
          { id: 'delete', text: 'Delete', iconCls: 'delete', xtype: 'menuitem' },
          '-',
          { id: 'publish', text: 'Publish to', iconCls: 'magic-wand', xtype: 'menuitem' }
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
              case 'add':
                this.getAddNodeWindow(params, node);
                // contextMenu: new Ext.menu.Menu({});
                break;
              case 'delete':
                this.deleteNode(params[this.treeConfig.hierarchicalIndexID], node);
                break;
              case 'publish':
                this.publishNode(params, node);
                break;
            }
          }).createDelegate(this)
        }
      });

      var that = this;
      var saveFunction = function () {

        var ids = that.saveParams.attributeIDs,
            storeID = that.saveParams.storeID,
            nodesToDelete = that.saveParams.nodesToDelete,
            onSuccess = that.saveParams.onSuccess,
            onFailure = that.saveParams.onFailure,
            oldStoreID = that.saveParams.oldStoreID;

        Diract.silent_request({
          url: Concentrator.route('Add', 'AttributeMatch'),
          params: {
            attributeIDs: ids,
            connectorID: that.connectorID,
            attributeStoreID: storeID == -1 ? undefined : storeID,
            oldAttributeStoreID: oldStoreID
          },
          success: function (response) {
            if (onSuccess) onSuccess(response.data);
            //sync the folder created with attributeStoreID
            that.saveParams = {};

          },
          failure: function () {
            //reload tree
            if (onFailure) onFailure();
            that.saveParams = {};
          }
        });
      }

      this.attributeTree = new Ext.tree.TreePanel({
        useArrows: this.treeConfig.useArrows || true,
        autoHeight: true,
        border: false,
        id: 'matched-attributes-tree',
        ddGroup: 'attributetree',
        animate: this.treeConfig.animate || true,
        containerScroll: true,
        enableDD: true,
        root: {
          text: 'matched attributes',
          id: this.connectorID || this.rootID,
          draggable: false,
          leaf: false
        },

        loader: new Ext.tree.TreeLoader({
          dataUrl: Concentrator.route('GetTreeView', 'AttributeMatch'),
          baseAttrs: { ConnectorID: this.connectorID || Concentrator.user.connectorID, AttributeStoreID: -1 },
          listeners: {
            beforeload: (function (treeloader, node) {
              Ext.apply(treeloader.baseParams, this.getBaseAttributes(node, treeloader));

              if (this.connectorID) {
                Ext.apply(treeloader.baseParams, { ConnectorID: this.connectorID });
              }

            }).createDelegate(this)
          }
        }),

        // context Menu
        contextMenu: new Ext.menu.Menu({
          items: [{ id: 'delete', text: 'Delete', iconCls: 'delete', xtype: 'menuitem' }],
          listeners: {
            'itemClick': (function (item) {

              var node = this.attributeTree.getSelectionModel().selNode;

              switch (item.id) {
                case 'delete':
                  this.deleteAttributeMatchNode(node, Concentrator.route('Delete', 'AttributeMatch'));
                  break;
              }
            }).createDelegate(this)

          }
        }),

        listeners: {
          // create nodes based on data from grid
          'nodedrop': function (tree, node, dd, e) {
            saveFunction();
          },
          'beforenodedrop': function (e) {
            //prepare data
            var ids = [],
            connectorID = that.connectorID,
            attributeStoreID = (e.target.attributes.AttributeStoreID == -1) ? undefined : e.target.attributes.AttributeStoreID,
            nodesToDelete = [],
            selections = [],
            self = this,
            tg = that,
            fromTree = !(Boolean(e.data.selections)),
            dragToRoot = (e.target.attributes.AttributeStoreID == -1),
            isLeaf = e.data.node ? e.data.node.leaf : false,
            gridNodes = [],
            oldStoreID = undefined;
            //sync the database

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
                    id: e.data.node.attributes.AttributeID,
                    draggable: true
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
                  if (e.data.node.parentNode && e.data.node.parentNode.attributes.AttributeStoreID != -1 && e.data.node.parentNode.childNodes.length == 1) {
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

                  if (e.data.node.parentNode && e.data.node.parentNode.attributes.AttributeStoreID != -1 && e.data.node.parentNode.childNodes.length == 1) {
                    nodesToDelete.push(e.data.node.parentNode);
                  }
                }
              }

              if (e.data.node.childNodes && e.data.node.childNodes.length > 0) {
                Ext.each(e.data.node.childNodes, function (node) {

                  ids.push(node.attributes.AttributeID);
                  oldStoreID = node.attributes.AttributeStoreID;
                });

              } else {
                //                if (e.data.node.attributes.AttributeID == undefined)                

                ids.push(e.data.node.attributes.AttributeID);
                oldStoreID = e.data.node.attributes.AttributeStoreID;
              }

              //check the attributeStoreID 

            }
            else { //from grid

              //push every selection to the selections array
              Ext.each(e.data.selections, function (r) {
                //add grid selections to selection container
                gridNodes.push(self.loader.createNode({
                  text: r.get('Attribute'),
                  leaf: true,
                  id: r.get('AttributeID'),
                  draggable: true
                }));


                ids.push(r.get('AttributeID'));
              });

              if (dragToRoot) {

                var folderNode = self.loader.createNode({
                  text: e.data.selections[0].get('Attribute'),
                  leaf: false,
                  id: e.data.selections[0].get('AttributeID'),
                  draggable: true
                  //children: gridNodes
                });

                selections.push(folderNode);
              } else {
                Ext.each(gridNodes, function (gr) {
                  selections.push(gr);
                });
              }
            }

            e.dropNode = selections;

            //remove folder?
            that.saveParams = {
              attributeIDs: ids,
              storeID: attributeStoreID,
              oldStoreID: oldStoreID ? oldStoreID : Boolean(nodesToDelete[0]) ? nodesToDelete[0].attributes.AttributeStoreID : undefined,
              onSuccess: function (data) {
                if (nodesToDelete) {
                  Ext.each(nodesToDelete, function (node) {
                    node.remove();
                  });
                }

                Ext.each(selections, function (node) {
                  node.attributes.AttributeStoreID = data.AttributeStoreID;
                  if (node.childNodes && node.childNodes.length > 0) {
                    Ext.each(node.childNodes, function (ch) { ch.attributes.AttributeStoreID = data.AttributeStoreID; });
                  }
                });

              },
              onFailure: function () {
                self.loader.load(self.root);
              }
            };

            return true;
          },

          /**
          Load and display form in center if not already displayed 
          */
          'click': (function (node, evt) {
            //            var id = node.attributes["AttributeStoreID"];
            //            if (id != this.rootID) {
            //              this.showDetails(id);
            //            }
          }).createDelegate(this),

          'beforedestroy': (function () {
            if (this.contextMenu) {
              this.contextMenu.destroy();
              delete this.contextMenu;
            }
          }).createDelegate(this),

          contextmenu: (function (node, evt) {
            node.select();

            var c = node.getOwnerTree().contextMenu;

            //if(!c.el)
            c.render();

            c.NodeID = node.id;
            c.attributes = node.attributes;
            c.contextNode = node;

            c.showAt(evt.getXY());

          }).createDelegate(this)

        }
      });

      var treeEditor = new Ext.tree.TreeEditor(this.attributeTree, {}, {
        allowBlank: false,
        listeners: {
          'complete': function (treeEditor, n, o) {

            var sel = treeEditor.tree.getSelectionModel().selNode,
                attributeStoreID = sel.attributes.AttributeStoreID;

            if (sel.attributes.leaf) return false; //short circuit for leafs

            Diract.silent_request({
              url: Concentrator.route('Update', 'AttributeMatchStore'),
              params: { AttributeStoreID: attributeStoreID, StoreName: n }
            });
          }
        }
      });


      region.add(this.attributeTree);
    },

    /**
    Displays a new form window to add a new node as a child to the selected one. 
    Uses the add new config
    */
    getAddNodeWindow: function (params, node) {
      var items = [];

      this.newEntityFormConfig.items[0] = this.newEntityFormConfig.ref.createMagentoConnectorPanel(this.newEntityFormConfig.ref.connectorSystem);

      items.push(this.newEntityFormConfig.items);

      for (var key in params) {

        var name = key;
        var value = params[key];

        //the index becomes a parent
        if (name == this.treeConfig.hierarchicalIndexID) {
          name = this.treeConfig.hierarchicalParentIndexID;
        }

        var value = params[key];
        if (name == this.treeConfig.hierarchicalParentIndexID && value == this.rootID) { value = ''; }

        var item = {
          name: name
        };

        item.xtype = 'hidden';
        item.value = value;

        items.push(item);

      }

      var newForm = new Diract.ui.FormWindow({
        url: this.newEntityFormConfig.url,
        autoScroll: true,
        width: 400, //this.newEntityFormConfig.width || 400,
        height: 600, // this.newEntityFormConfig.height || 600,
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

    deleteAttributeMatchNode: function (node, url) {
      Ext.Msg.confirm("Delete attribute match", "Are you sure you want to remove this attribute from the match store?",
          (function (button) {
            if (button == "yes") {
              Diract.request({
                url: url,
                params: {
                  attributeID: node.attributes.AttributeID,
                  attributeStoreID: node.attributes.AttributeStoreID
                },
                success: (function () {
                  var parent = node.parentNode;

                  node.remove();
                  if (parent.childNodes.length == 0) {
                    parent.remove();

                  }


                }).createDelegate(this),
                failure: function () {
                }
              });
            }
          }).createDelegate(this));
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

    publishNode: function (id, node) {

      var ConnectorField = new Diract.ui.Select({
        label: 'To connector',
        valueField: 'ID',
        displayField: 'Name',
        store: Concentrator.stores.connectors,
        searchUrl: Concentrator.route('GetStore', 'Connector'),
        allowBlank: false
      });

      var publishGroup = new Ext.form.CheckboxGroup({
        columns: 2,
        fieldLabel: 'To publish',
        items: [
            { boxLabel: 'Product group mappings', name: 'productGroupMapping', checked: true, disabled: true },
            { boxLabel: 'Attribute groups & attributes', name: 'attributes' },
            { boxLabel: 'Price rules', name: 'contentPrices' },
            { boxLabel: 'Product rules', name: 'contentProducts' },
            { boxLabel: 'Content vendor settings', name: 'contentVendorSettings' },
            { boxLabel: 'Connector publications', name: 'connectorPublications' },
            { boxLabel: 'Connector statuses', name: 'connectorProductStatuses' },
            { boxLabel: 'Preferred content settings', name: 'preferredContentSettings' }
        ]
      });

      var panel = new Ext.form.FormPanel({
        bodyStyle: 'padding: 20px;',
        items: [
        ConnectorField,
        publishGroup
        ]
      });

      var window = new Ext.Window({
        title: 'Publish',
        width: 608,
        height: 230,
        modal: true,
        layout: 'fit',
        buttons: [
                  new Ext.Button({
                    text: 'Publish',
                    handler: function () {
                      var params = {
                        sourceConnectorID: node.attributes.ConnectorID,
                        destinationConnectorID: ConnectorField.getValue(),
                        productGroupMappingID: node.attributes.ProductGroupMappingID
                      };

                      if (!ConnectorField.getValue()) {
                        Ext.Msg.alert('Invalid', 'You have to choose a connector');
                        return;
                      }

                      var publish = publishGroup.getValue();

                      for (var i = 0; i < publish.length; i++) {
                        var s = publish[i];
                        params[s.name] = s.getValue();
                      }

                      Diract.message("Started publishing", "This operation will process for a longer period of time and it will finish in about 5 minutes.");

                      Diract.mute_request({
                        url: Concentrator.route('Publish', 'ProductGroupMapping'),
                        params: params
                      });

                      window.destroy();

                    }
                  })
        ],
        items: [
          panel
        ],
        success: function () {
          window.destroy();
        }
      });

      window.show();

    },
    // Get product window
    getProductWindow: function (id, node) {
      var self = this;

      var brandField = new Ext.form.TextField({
        name: 'BrandID',
        fieldLabel: 'Brand'
      });
      var that = this;


      this.productsGrid = new Diract.ui.Grid({
        primaryKey: ['ProductID'],
        singularObjectName: 'Product',
        pluralObjectName: 'Products',
        sortBy: 'Configurable',
        height: 400,
        autoLoadStore: true,
        width: 600,
        sortInfo: {
          direction: 'DESC',
          field: 'Configurable'
        },
        url: Concentrator.route('GetListByConnector', 'Product'),
        params: this.params,
        rowActions: [],
        permissions: {
          list: 'GetProduct',
          create: 'CreateProduct',
          remove: 'DeleteProduct',
          update: 'UpdateProduct'
        },
        structure: [
        { dataIndex: 'ProductID', type: 'int', header: 'Identifier' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor item number', editor: { xtype: 'textfield' }, editable: false },
        { dataIndex: 'ProductName', type: 'string', header: 'Name', editable: false },
        { dataIndex: 'Configurable', type: 'boolean', header: 'Configurable' },
        { dataIndex: 'ProductID', type: 'string', editor: { xtype: 'productgroup', fieldLabel: 'Product group' } }
        ],

        windowConfig: {
          height: 300

        }
      });

      var form = new Ext.Window({
        width: 700,
        height: 500,
        disableButton: true,
        modal: true,
        title: 'Select product to add',
        formStyle: 'padding: 15px;',
        layout: 'fit',
        items: [
         this.productsGrid
        ],
        buttons: [new Ext.Button({
          text: 'Add',
          handler: (function () {



            var selectedItems = this.productsGrid.selModel.selections.items;

            var myMask = new Ext.LoadMask(form.getEl(), { msg: "Please wait..." });
            myMask.show();
            var selectionLength = selectedItems.length - 1;

            for (var i = 0; i < selectedItems.length; i++) {

              (function (i, myMask) {
                var rec = selectedItems[i];
                var productID = rec.get('ProductID');;

                var params = {
                  url: Concentrator.route("AddByProductGroupMapping", "ContentProductGroupMapping"),
                  flushAt: selectedItems.length,
                  params: {
                    ProductGroupMappingID: id.ProductGroupMappingID,
                    ConnectorID: id.ConnectorID,
                    ProductID: productID
                  },
                  onFlush: function () {
                    var grid = Ext.getCmp('detailRelationGrid');
                    grid.store.load();
                    myMask.hide();
                  }
                };

                Diract.request(params);

              })(i, myMask);
            }
          }).createDelegate(this)
        })]

      });

      form.show();

    },
    // Get pricing rule wizard
    getPriceRuleWizard: function (id, node) {
      var self = this;

      var productGroupMappingID = id.ProductGroupMappingID;
      var connectorID = id.ConnectorID;
      var productGroupID = node.attributes[this.treeConfig.ProductGroupID];
      var brandID = null;

      var vendorField = new Diract.ui.Select({
        label: 'Vendor',
        allowBlank: false,
        displayField: 'VendorName',
        valueField: 'VendorID',
        name: 'VendorID',
        clearable: true,
        width: 175,
        store: Concentrator.stores.vendors
      });

      var form = new Diract.ui.FormWindow({
        width: 400,
        height: 200,
        disableButton: true,
        modal: true,
        title: 'Filter your results',
        formStyle: 'padding: 15px;',
        layout: 'fit',
        items: [
          {
            xtype: 'brand',
            fieldLabel: 'Brand',
            width: 175,
            allowBlank: false,
            listeners: {
              'select': (function (field) {

                brandID = field.getValue();

              }).createDelegate(this)
            }
          },
          vendorField
        ],
        buttons: [
          new Ext.Button({
            text: 'Filter',
            handler: function () {

              var wiz = new Concentrator.ui.PriceRuleWizard({
                self: self,
                connectorID: connectorID,
                productGroupMappingID: productGroupMappingID,
                productGroupID: productGroupID,
                brandID: brandID,
                vendorID: vendorField.getValue()
              });

              wiz.show();

              form.hide();
              form.destroy();
            }
          })
        ]

      });

      form.show();

    },

    translateDescription: function (params, node) {
      var wind = new Ext.Window({
        title: 'Translation management',
        items: [
          new Diract.ui.Grid({
            url: Concentrator.route('GetTranslations', 'ProductGroupMapping'),
            params: {
              ProductGroupMappingID: params.ProductGroupMappingID,
              ConnectorID: params.ConnectorID
            },
            updateUrl: Concentrator.route('SetTranslation', 'ProductGroupMapping'),
            primaryKey: ['ProductGroupMappingID', 'LanguageID'], //based on convention
            sortBy: 'ProductGroupMappingID',
            permissions: { all: 'Default' },
            singularObjectName: 'Product group mapping' + ' translation',
            pluralObjectName: 'Product group mapping' + ' translations',
            structure: [
                        { dataIndex: 'ProductGroupMappingID', type: 'int' },
                        { dataIndex: 'Language', type: 'string', header: 'Language' },
                        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textarea' } },
                        { dataIndex: 'LanguageID', type: 'int' }
            ],
            sortBy: 'ProductGroupMappingID'
          })
        ],
        width: 700,
        height: 500,
        layout: 'fit'
      });

      wind.show();
    },

    translateCustomLabels: function (params, node) {
      var wind = new Ext.Window({
        title: 'Custom label management',
        items: [
          new Diract.ui.Grid({
            url: Concentrator.route('GetCustomLabels', 'ProductGroupMapping'),
            params: {
              ProductGroupMappingID: params.ProductGroupMappingID
            },
            updateUrl: Concentrator.route('SetCustomLabel', 'ProductGroupMapping'),
            primaryKey: ['ProductGroupMappingID', 'LanguageID', 'ConnectorID'],
            sortBy: 'ConnectorID',
            permissions: { all: 'Default' },
            singularObjectName: 'Product group mapping' + ' custom label',
            pluralObjectName: 'Product group mapping' + ' custom label',
            structure: [
                        { dataIndex: 'ProductGroupMappingID', type: 'int' },
                        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', renderer: function (val, m, rec) { return rec.get('ConnectorName') }, editable: false },
                        { dataIndex: 'Language', type: 'string', header: 'Language' },
                        { dataIndex: 'CustomLabel', type: 'string', header: 'Label', editor: { xtype: 'textarea' } },
                        { dataIndex: 'LanguageID', type: 'int' },
                        { dataIndex: 'ConnectorName', type: 'string' }
          ]
          })
        ],
        width: 700,
        height: 500,
        layout: 'fit'
      });

      wind.show();
    },

    getSeoWindow: function (params, node) {
        var wind = new Diract.ui.FormWindow({
            title: 'Seo Text Management',
            url: Concentrator.route('UpdateSeoTexts', 'Seo'),
            loadUrl: Concentrator.route('GetSeoTexts', 'Seo'),
            loadParams: {
                productGroupMappingID: params.ProductGroupMappingID
            },
            params: {
                productGroupMappingID: params.ProductGroupMappingID
            },
            cancelButton: true,
            buttonText: 'Save',
            items: [
            {
                name: 'languageID',
                xtype: 'language',
                valueField: 'ID',
                value: Concentrator.user.languageID,
                fieldLabel: 'Language',
                width: 402,
                listeners: {
                    'select': function (combo, rec, idx) {
                        wind.form.loadForm({ languageID: rec.get('ID') });
                    }
                }
            },
            {
                fieldLabel: 'Description1',
                xtype: 'ckeditor',
                name: 'description1',
                width: 400,
                height: 150,
                CKConfig: {
                    resize_enabled: false,
                    extraAllowedContent: 'iframe[*]',
                    toolbar: [
                ['Bold', 'Italic', 'Underline', 'StrikeThrough', 'Undo', 'Redo', 'Cut', 'Copy', 'Paste'],
                ['NumberedList', 'BulletedList'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                ['Link', 'Source']
                    ]}
            },
            {
                fieldLabel: 'Pagetitle',
                xtype: 'textfield',
                name: 'pageTitle',
                width: 402,
                height: 50
            },
            {
                fieldLabel: 'Metadescription',
                xtype: 'ckeditor',
                name: 'metaDescription',
                width: 400,
                height: 150,
                CKConfig: {
                    resize_enabled: false,
                    extraAllowedContent: 'iframe[*]',
                    toolbar: [
                ['Bold', 'Italic', 'Underline', 'StrikeThrough', 'Undo', 'Redo', 'Cut', 'Copy', 'Paste'],
                ['NumberedList', 'BulletedList'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                ['Link', 'Source']
                    ]
                }
            },
            {
                fieldLabel: 'Description2',
                xtype: 'ckeditor',
                name: 'description2',
                width: 400,
                height: 150,
                CKConfig: {
                    resize_enabled: false,
                    extraAllowedContent: 'iframe[*]',
                    toolbar: [
                ['Bold', 'Italic', 'Underline', 'StrikeThrough', 'Undo', 'Redo', 'Cut', 'Copy', 'Paste'],
                ['NumberedList', 'BulletedList'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                ['Link', 'Source']
                    ]
                }
            },
            {
                fieldLabel: 'Description3',
                xtype: 'ckeditor',
                name: 'description3',
                width: 400,
                height: 150,
                CKConfig: {
                    resize_enabled: false,
                    extraAllowedContent: 'iframe[*]',
                    toolbar: [
                ['Bold', 'Italic', 'Underline', 'StrikeThrough', 'Undo', 'Redo', 'Cut', 'Copy', 'Paste'],
                ['NumberedList', 'BulletedList'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
                ['Link', 'Source']
                    ]
                }
            }
            ],
            width: 600,
            height: 600,
            maximizable: true,
            autoScroll: true,
            layout: 'fit',
            success: (function () {
                this.grid.store.reload();
                wind.destroy();
            }).createDelegate(this)
        });

        wind.show();
    },

    // Add to relation function
    addRelation: function (id, node) {

      //For later : array.push(id.ProductGroupMappingID, node.text);

      this.clipboardArray.push(id.ProductGroupMappingID);
      this.clipBoard(this.clipboardArray);
    },

    // Product Compare Source function
    getCompareWindow: function (id, node) {
      var self = this;

      this.productSourceStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetPcSourceStore", "ProductCompetitor"),
        method: 'GET',
        root: 'results',
        idProperty: 'ProductCompareSourceID',
        fields: ['ProductCompareSourceID', 'Name']
      });

      this.intervalTypeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetIntervalType", "Slurp"),
        method: 'GET',
        root: 'results',
        idProperty: 'IntervalType',
        fields: ['IntervalType', 'IntervalName']
      });

      self.productCompareSource = new Diract.ui.Select({
        label: 'Compare Source',
        hiddenName: 'ProductCompareSourceID',
        allowBlank: true,
        store: this.productSourceStore,
        displayField: 'Name',
        valueField: 'ProductCompareSourceID'
      });

      self.interval = new Ext.form.NumberField({
        fieldLabel: 'interval',
        name: 'Interval'
      });

      self.intervalType = new Diract.ui.Select({
        label: 'Interval Type',
        hiddenName: 'IntervalType',
        allowBlank: false,
        store: this.intervalTypeStore,
        displayField: 'IntervalName',
        valueField: 'IntervalType'
      });

      //      self.intervalType = new Ext.form.NumberField({
      //        fieldLabel: 'Interval Type',
      //        name: 'IntervalType'
      //      });

      var window = new Ext.Window({
        width: 400,
        height: 190,
        modal: true,
        layout: 'fit',
        items: [
        new Ext.form.FormPanel({
          padding: 20,
          items: [
            self.productCompareSource,
            self.interval,
            self.intervalType
          ]
        })
        ],
        buttons: [
          new Ext.Button({
            text: 'Save',
            handler: function () {

              Diract.request({
                url: Concentrator.route("CreateSlurpSchedule", "Slurp"),
                params: {
                  productGroupMappingID: id.ProductGroupMappingID,
                  connectorID: id.ConnectorID,
                  productCompareSourceID: self.productCompareSource.getValue(),
                  interval: self.interval.getValue(),
                  intervalType: self.intervalType.getValue()
                },
                success: function () {
                  window.destroy();
                  self.initialConfig.reload();
                },
                failure: function (form, action) {
                  Ext.MessageBox.show({
                    title: 'No productgroup match for this connector',
                    buttons: Ext.Msg.OK
                  });
                }
              });

            }
          })
        ]
      });

      window.show();
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
    }
  });
  return gr;
})();
