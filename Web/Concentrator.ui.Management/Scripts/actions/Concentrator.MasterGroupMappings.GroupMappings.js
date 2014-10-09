/// <reference path="~/Scripts/ext/ext-base-debug.js" />
/// <reference path="~/Scripts/ext/ext-all-debug.js" />
/// <reference path="~/Scripts/linq.js" />

(function () {
  var functionalities = Concentrator.MasterGroupMappingFunctionalities;

  Diract.ui.GroupMappings = (function () {
    var gr = Ext.extend(Ext.Panel, {
      showLeafs: true,
      treeConfig: {},
      attributeTreeConfig: {},
      border: false,
      layout: 'border',
      id: 'masterpanel',
      defaults: { padding: 5 },

      constructor: function (config) {
        Ext.apply(this, config);
        this.rootID = this.treeConfig.rootID || -1;
        this.tabPanelItems = [];
        this.initMasterLayout();
        Diract.ui.GroupMappings.superclass.constructor.call(this, config);
        this.initTreeComponent(this.west);
      },

      /**
      Init layout -> create panels, assign regions and basic settings       
      */
      initMasterLayout: function () {
        //tree view
        this.west = new Ext.Panel({
          region: 'west',
          id: 'masterGroupMappingWestPanel',
          width: 300,
          layout: 'fit',
          autoScroll: true,
          margins: '0 0 0 0',
          split: true,
          collapsible: true,
          collapsed: false
        });
        //details view

        this.center = new MasterGroupMapping.ui.TabPanel();

        this.items = [this.center, this.west];
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
            }).createDelegate(this)
          }
        });

        this.contextMenu = new Diract.Ext.menu.Menu({
          items: [
            { id: 'addMGM', text: 'Add Master Group Mapping', iconCls: 'add', xtype: 'menuitem', requires: functionalities.Add },
            { id: 'deleteMGM', text: 'Delete Master Group Mapping', iconCls: 'delete', xtype: 'menuitem', requires: functionalities.Delete },
            { id: 'findMGM', text: 'Find Master Group Mapping', iconCls: 'view', xtype: 'menuitem', requires: functionalities.update },
            '-',
            { id: 'assignMGM', text: 'Assign User To Master Group Mapping', iconCls: 'icon-assign', xtype: 'menuitem', requires: functionalities.AssignUserTo },
            '-',
            { id: 'renameMGM', text: 'Rename Master Group Mapping', iconCls: 'icon-rename', xtype: 'menuitem', requires: functionalities.Update },
            { id: 'languageWizard', text: 'Language Wizard', iconCls: 'icon-language-wizard', xtype: 'menuitem', requires: functionalities.LanguageWizard },
            //'-',
            //{ id: 'attributeManagement', text: 'Attribute Management', iconCls: 'icon-attribute-manager', xtype: 'menuitem', requires: functionalities.AttributeManagement },
            //{ id: 'crossReferenceManagement', text: 'Cross Reference Management', iconCls: 'icon-cross-reference-management', xtype: 'menuitem', requires: functionalities.CrossReferenceManagement },
            // { id: 'relatedMasterGroupMappingManagement', text: 'Manage Related Master Group Mapping', iconCls: 'icon-cross-reference-management', xtype: 'menuitem', requires: functionalities.RelatedMasterGroupMappingManagement },
            //'-',
            //{ id: 'productControlWizard', text: 'Product Control Wizard', iconCls: 'icon-wizard-hat', xtype: 'menuitem', requires: functionalities.ProductControlWizard },
            //{ id: 'wizardToControlAllProducts', text: 'Wizard to Control All Products', iconCls: 'icon-wizard-hat', xtype: 'menuitem', requires: functionalities.ControlAllProductsWizard },
            //{ id: 'productControleManagement', text: 'Product Control Management', iconCls: 'icon-product-control-management', xtype: 'menuitem', requires: functionalities.ProductControleManagement },
            //{
            //  id: 'AttributeSelectorManagement', text: 'Attribute Selector Management', xtype: 'menuitem', iconCls: 'icon-wizard-hat', requires: functionalities.AttributeSelectorManagement
            //}
          ],
          listeners: {
            destroy: function () { },
            itemClick: (function (item) {
              var node = this.tree.getSelectionModel().lastSelNode;
              var params = {};

              for (var key in this.treeConfig.baseAttributes) {
                params[key] = node.attributes[key];
              }
              switch (item.id) {
                case 'addMGM':
                  var config = {
                    MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                    MasterGroupMappingName: node.attributes.text
                  };
                  Concentrator.MasterGroupMappingFunctions.getAddNewMasterGroupMappingWindow(config);
                  break;
                case 'deleteMGM':
                  this.deleteNode(params[this.treeConfig.hierarchicalIndexID], node);
                  break;
                case 'findMGM':
                  Concentrator.MasterGroupMappingFunctions.findMasterGroupMapping();
                  break;
                case 'renameMGM':
                  //                this.renameMasterGroupMapping(params, node);
                  config = {
                    MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                    MasterGroupMappingName: node.attributes.text
                  };
                  this.renameMasterGroupMappingv2(config);
                  break;
                case 'assignMGM':
                  this.assignMasterGroupMappingTo(params, node);
                  break;
                case 'attributeManagement':
                  this.attributeManagement(params, node);
                  break;
                case 'productControleManagement':
                  Concentrator.MasterGroupMappingFunctions.productControleManagementWindow();
                  break;
                case 'productControlWizard':
                  var parameters = {
                    MasterGroupMappingID: params.MasterGroupMappingID,
                    MasterGroupMappingName: node.attributes.text
                  };
                  Concentrator.MasterGroupMappingFunctions.ProductControleWizard(parameters);
                  break;
                case 'wizardToControlAllProducts':
                  var parameters = {
                    Action: 'all'
                  };
                  Concentrator.MasterGroupMappingFunctions.ProductControleWizard(parameters);
                  break;
                case 'crossReferenceManagement':
                  var parameters = {
                    MasterGroupMappingID: params.MasterGroupMappingID,
                    MasterGroupMappingName: node.attributes.text
                  };
                  Concentrator.MasterGroupMappingFunctions.getCrossReferenceManagementWindow(parameters);
                  break;
                case 'relatedMasterGroupMappingManagement':
                  Concentrator.MasterGroupMappingFunctions.getRelatedMasterGroupMappingsWindow();
                  break;
                case 'languageWizard':
                  this.languageWizard(params, node);
                  break;
                case 'renameMasterGroupMappingName':
                  this.renameMasterGroupMappingName(node);
                  break;
                case 'renameMasterGroupMappingNames':
                  this.renameMasterGroupMappingNames();
                  break;
               
                case 'dashboard':
                  Concentrator.MasterGroupMappingFunctions.dashboardWindow();
                  break;
              }
            }).createDelegate(this)
          }
        });

        var that = this;

        var saveChangsInTree = function () {
          eData = that.saveParams.e;
          actionMenuValue = that.saveParams.fromTreeAction;
          exParentOfCopiedNode = that.saveParams.ExParentOfCopiedNode;

          //        MoveOrCopy = that.saveParams.MoveOrCopy;
          //        MoveOrCopy = that.saveParams.MoveOrCopy;
          if (that.saveParams.fromTree) { // From Tree
            switch (actionMenuValue) {
              case 'move':
                {
                  Diract.silent_request({
                    url: Concentrator.route('MoveTreeNodes', 'MasterGroupMapping'),
                    params: {
                      DDMasterGroupMappingID: eData.data.node.attributes.MasterGroupMappingID,
                      ParentMasterGroupMappingID: eData.data.node.parentNode.attributes.MasterGroupMappingID
                    },
                    success: function (response) {
                      // parameters om de volgorde van de nodes door te geven aan de contorller
                      var MasterGroupMappingJsonObject = { MasterGroupMappingID: 0, ListOfChildrenID: [] };
                      // end parameters

                      MasterGroupMappingJsonObject.MasterGroupMappingID = eData.dropNode.attributes.MasterGroupMappingID;
                      Ext.each(eData.dropNode.parentNode.childNodes, function (r) {
                        MasterGroupMappingJsonObject.ListOfChildrenID.push(r.attributes.MasterGroupMappingID);
                      });

                      var MasterGroupMappingJsonObject = JSON.stringify(MasterGroupMappingJsonObject);
                      Diract.silent_request({
                        url: Concentrator.route('ReorganizeTreeDirectory', 'MasterGroupMapping'),
                        params: {
                          MasterGroupMappingJsonObject: MasterGroupMappingJsonObject
                        },
                        success: function () {
                          var masterGroupMappings = new Array();
                          masterGroupMappings.push(eData.dropNode.attributes.MasterGroupMappingID);
                          masterGroupMappings.push(exParentOfCopiedNode.attributes.MasterGroupMappingID);

                          config = {
                            treePanel: true,
                            MasterGroupMappings: masterGroupMappings
                          };
                          Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                        }
                      });
                    }
                  });
                }
                break;
              case 'copy':
                {
                  CopyAttributes = false;
                  CopyProducts = false;
                  copyCrossReferences = false;

                  if (that.saveParams.fromTreeActionProducts) {
                    CopyProducts = true;
                  }
                  if (that.saveParams.fromTreeActionAttributes) {
                    CopyAttributes = true;
                  }
                  if (that.saveParams.fromTreeActionReferences) {
                    copyCrossReferences = true;
                  }

                  DDMasterGroupMappingID = eData.data.node.attributes.MasterGroupMappingID;
                  ParentMasterGroupMappingID = eData.data.node.parentNode.attributes.MasterGroupMappingID;
                  Diract.silent_request({
                    url: Concentrator.route('CopyTreeNodes', 'MasterGroupMapping'),
                    params: {
                      MasterGroupMappingID: DDMasterGroupMappingID,
                      ParentMasterGroupMappingID: ParentMasterGroupMappingID,
                      CopyProducts: CopyProducts,
                      CopyAttributes: CopyAttributes,
                      copyCrossReferences: copyCrossReferences
                    },
                    success: function (response) {
                      //                    exParentOfCopiedNode.reload();
                      //                    var parentTargetNode = that.tree.getNodeById(eData.dropNode.parentNode.attributes.id);
                      //                    if (!(parentTargetNode == null)) {
                      //                      parentTargetNode.reload();
                      //                    }

                      var masterGroupMappings = new Array();
                      masterGroupMappings.push(eData.dropNode.attributes.MasterGroupMappingID);
                      masterGroupMappings.push(eData.dropNode.parentNode.attributes.MasterGroupMappingID);

                      config = {
                        treePanel: true,
                        MasterGroupMappings: masterGroupMappings
                      };
                      Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                    }
                  });
                }
                break;
              case 'crossReference':
                {
                  CrossReferenceID = eData.dropNode.attributes.MasterGroupMappingID;
                  MasterGroupMappingID = eData.target.attributes.MasterGroupMappingID;

                  Diract.silent_request({
                    url: Concentrator.route('CrossReferenceTreeNode', 'MasterGroupMapping'),
                    params: {
                      MasterGroupMappingID: MasterGroupMappingID,
                      CrossReferenceID: CrossReferenceID
                    },
                    success: function (response) {
                    }
                  });
                }
                break;
              case 'relatedMasterGroupMapping':
                {

                  RelatedMasterGroupMappingID = eData.dropNode.attributes.MasterGroupMappingID;
                  MasterGroupMappingID = eData.target.attributes.MasterGroupMappingID;
                  var MaxRelationQuantity = that.saveParams.maxRelationQuantity;

                  Diract.request({
                    url: Concentrator.route('RelateMasterGroupMappings', 'MasterGroupMapping'),
                    params: {
                      masterGroupMappingID: MasterGroupMappingID,
                      relatedMasterGroupMappingID: RelatedMasterGroupMappingID,
                      maxRelationQuantity: MaxRelationQuantity,
                      relatedTypeID: that.saveParams.relatedTypeID
                    },
                    success: function (response) {
                      var masterGroupMappings = new Array();

                      config = {
                        treePanel: true,
                        MasterGroupMappings: masterGroupMappings
                      };
                      Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                    }
                  });
                }
                break;
            }
          } else { // From Grid                    
            if (that.saveParams.fromGridProductGroups) {
              var parameters = {
                MasterGroupMappingID: eData.target.attributes.MasterGroupMappingID,
                VendorProductGroupRecords: eData.data.selections,
                node: eData.target
              };
              Concentrator.MasterGroupMappingFunctions.getMatchVendorProductGroupWindow(parameters);
            }
            if (that.saveParams.fromGridVendorProducts) {
              var parameters = {
                MasterGroupMappingID: eData.target.attributes.MasterGroupMappingID,
                VendorProductRecords: eData.data.selections,
                node: eData.target
              };
              Concentrator.MasterGroupMappingFunctions.getMatchVendorProductWindow(parameters);
            }
          }
        };


        //var isUserAuthorizedToDropOnConnectorTree = function () {
        //  var required = [functionalities.MoveProductGroupMapping, functionalities.CopyProductGroupMapping];
        //  var hasOneRequiredFunctionality = Enumerable.From(required).Any("Diract.user.hasFunctionality($)");
        //  return hasOneRequiredFunctionality;
        //};

        var validateTreeDropEvent = function (dropEvent) {
          // todo validate if the user may drop on the node
          return true;
          //if (!isUserAuthorizedToDropOnConnectorTree()) {
          //  dropEvent.Cancel = true;
          //  return false;
          //}
          //return true;
        };

        this.tree = new Ext.tree.TreePanel({
          useArrows: true,
          autoHeight: true,
          border: false,
          ddGroup: 'MasterGroupMappingDD',
          enableDD: true,
          ddAppendOnly: false,
          trackMouseOver: true,
          enableDrop: true,
          animate: true,
          selModel: new Ext.tree.MultiSelectionModel(),
          refreshNode: function (node) {
            Concentrator.MasterGroupMappingFunctions.refreshTreeNode(node);
          },
          containerScroll: true,
          id: 'masterGroupMappingTreePanel',
          root: {
            text: this.treeConfig.rootText || 'results',
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

              var id = node.attributes.MasterGroupMappingID;
              //            Ext.getCmp('MatchedProductGroup').store.reload({ params: { masterGroupMappingID: id} });
              //            Ext.getCmp('MatchedProductGroup').excelRequestParams = {
              //              MasterGroupMappingID: id
              //            };

              //            Ext.getCmp('VendorProductPanel').store.reload({ params: { MasterGroupMappingID: id} });
              //            Ext.getCmp('VendorProductPanel').excelRequestParams = {
              //              MasterGroupMappingID: id
              //            };


              // Filter Vendor Product Groups Grid
              var vendorProductGroupsGrid = Ext.getCmp('VendorProductGroupsPanel').ownerCt;
              Ext.each(vendorProductGroupsGrid.filterItems, function (filterItem) {
                if (filterItem.name) {
                  if (filterItem.getName() == 'MasterGroupMappingID') {
                    filterItem.setValue(id);
                  }
                }
              });
              Ext.each(vendorProductGroupsGrid.grid.getTopToolbar().items.items, function (button) {
                if (button.text) {
                  if (button.text == 'Master Group Mapping Filter') {
                    button.setDisabled(false);
                    button.setIconClass('lightbulb-on');
                  }
                }
              });
              vendorProductGroupsGrid.filterGrid();

              // Filter Vendor Products Grid
              var vendorProductsGrid = Ext.getCmp('VendorProductsPanel').ownerCt;
              Ext.each(vendorProductsGrid.filterItems, function (filterItem) {
                if (filterItem.name) {
                  if (filterItem.getName() == 'MasterGroupMappingID') {
                    filterItem.setValue(id);
                  }
                }
              });
              Ext.each(vendorProductsGrid.grid.getTopToolbar().items.items, function (button) {
                if (button.text) {
                  if (button.text == 'Master Group Mapping Filter') {
                    button.setDisabled(false);
                    button.setIconClass('lightbulb-on');
                  }
                }
              });
              vendorProductsGrid.filterGrid();

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
            nodedragover: function (dropEvent) {
              return validateTreeDropEvent(dropEvent);
            },

            'beforenodedrop': function (e) {
              fromTree = !(Boolean(e.data.selections));
              that.saveParams = {
                e: e,
                fromTree: fromTree,
                fromGridVendorProducts: false,
                fromGridProductGroups: false,
                fromTreeActionProducts: false,
                fromTreeActionAttributes: false,
                fromTreeActionReferences: false
              };

              if (fromTree) {
                var treeName = e.source.tree.id;
                if (treeName == "connectorMappingTreePanel") {
                  e.cancel = true;
                } else {
                  var dropedNode = e.dropNode;
                  var exParent = e.dropNode.parentNode;
                  that.saveParams['ExParentOfCopiedNode'] = that.tree.getNodeById(e.dropNode.parentNode.attributes.id);
                  var form = new Diract.Ext.form.FormPanel({
                    baseCls: 'x-plain',
                    labelWidth: 10,
                    defaultType: 'radiogroup',
                    items: [{
                      fieldLabel: '',
                      columns: 1,
                      items: [{
                        boxLabel: 'Move Master Group Mapping',
                        name: 'action',
                        inputValue: 'move',
                        checked: true
                      }, {
                        boxLabel: 'Cross Reference Master Group Mapping',
                        name: 'action',
                        inputValue: 'crossReference'
                      }, {
                        boxLabel: 'Copy Master Group Mapping',
                        name: 'action',
                        inputValue: 'copy',
                        listeners: {
                          'check': function (t, checked) {
                            var fieldset = Ext.getCmp('formfieldset');
                            if (checked) {
                              fieldset.expand(true);
                            } else {
                              fieldset.collapse(true);
                            }
                          }
                        }
                      }, {
                        boxLabel: 'Relate Master Group Mapping',
                        name: 'action',
                        inputValue: 'relatedMasterGroupMapping',
                        listeners: {
                          'check': function (t, checked) {
                            var fieldset = Ext.getCmp('maxRelationQuantityForm');
                            if (checked) {
                              fieldset.expand(true);
                            } else {
                              fieldset.collapse(true);
                            }
                          }
                        }
                      }, { //Copy The Next Item(s) With Master Group Mapping
                        xtype: 'fieldset',
                        title: '',
                        id: 'formfieldset',
                        collapsed: true,
                        autoHeight: true,
                        defaults: {},
                        labelWidth: 5,
                        defaultType: 'checkbox',
                        items: [{
                          boxLabel: 'All Products',
                          name: 'products',
                          checked: true
                        }, {
                          boxLabel: 'All Attributes',
                          name: 'attributes',
                          checked: true
                        }, {
                          boxLabel: 'All Cross References',
                          name: 'references',
                          checked: true
                        }
                        ]
                      }, {
                        xtype: 'fieldset',
                        title: '',
                        id: 'maxRelationQuantityForm',
                        collapsed: true,
                        autoHeight: true,
                        border: false,
                        defaults: {},
                        labelWidth: 120,
                        //defaultType: 'numberfield',
                        items: [
                          {
                            xtype: 'relatedProductType',
                            fieldLabel: 'RelatedType',
                            name: 'relatedTypeID',
                            width: 180
                          },
                          {
                            xtype: 'numberfield',
                            fieldLabel: 'Max Relation Quantity',
                            name: 'maxRelationQuantity',
                            width: 40
                          }
                        ]
                      }
                      ]
                    }],
                    buttons: ["->", {
                      text: 'Save',
                      handler: function () {

                        var formInput = form.getForm().getValues();
                        var isValidForm = true;
                        that.saveParams['fromTreeAction'] = formInput.action;
                        if (formInput.action == 'copy') {
                          if (formInput.products)
                            that.saveParams.fromTreeActionProducts = true;
                          if (formInput.attributes)
                            that.saveParams.fromTreeActionAttributes = true;
                          if (formInput.references)
                            that.saveParams.fromTreeActionReferences = true;
                        }
                        if (formInput.action == 'crossReference') {
                          var panels = {
                            treePanel: true,
                            node: e.dropNode.parentNode
                          };
                          Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                        }
                        if (formInput.action == 'relatedMasterGroupMapping') {
                          if (formInput.maxRelationQuantity) {
                            that.saveParams.maxRelationQuantity = formInput.maxRelationQuantity;
                            that.saveParams.relatedTypeID = formInput.relatedTypeID;
                          }

                          if (formInput.RelatedProductTypeID == null || formInput.RelatedProductTypeID == "") {
                            isValidForm = false;
                            Ext.Msg.alert('RelatedType cannot be empty', 'Please select a related type');
                          } else {
                            that.saveParams.relatedTypeID = formInput.RelatedProductTypeID;
                          }
                        }
                        if (isValidForm) {
                          saveChangsInTree();
                          window.close();
                        }
                      }
                    }, {
                      text: 'Cancel',
                      handler: function () {
                        window.close();
                        var newParent = e.dropNode.parentNode;
                        exParent.reload();
                        newParent.reload();
                      }
                    }]
                  });
                  var window = new Ext.Window({
                    title: 'Action Menu',
                    width: 500,
                    height: 300,
                    minWidth: 300,
                    minHeight: 200,
                    layout: 'fit',
                    plain: false,
                    closable: false,
                    buttonAlign: 'center',
                    items: form
                  });
                  window.show();
                }
              } else {
                if (e.data.grid.id == "VendorProductGroupsPanel") {
                  that.saveParams.fromGridProductGroups = true;
                } else {
                  if (e.data.grid.id == "VendorProductsPanel") {
                    that.saveParams.fromGridVendorProducts = true;
                  }
                }
                saveChangsInTree();
              }
              //return false;
            },
            'nodedrop': function (tree, node, dd, e) {

              return false;
              //                        saveChangsInTree();
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
          },
          tbar: [{
            text: 'Expand All',
            iconCls: 'icon-expand',
            handler: function () {
              that.tree.expandAll();
            }
          }, {
            text: 'Collapse All',
            iconCls: 'icon-collapse',
            handler: function () {
              that.tree.collapseAll();
            }
          }]
        });

        region.add(this.tree);
        this.tree.getRootNode().expand();
        //            this.tree.expandAll();
        //            this.center.setActiveTab(4);
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
        var newForm = new Diract.ui.FormWindow({
          url: Concentrator.route('Create', 'MasterGroupMapping'),
          autoScroll: true,

          labelWidth: 150,

          width: 400,
          height: 320,

          title: 'Add Master Group Mapping to: ' + node.attributes.text,

          items: [{
            xtype: 'productgroup',
            fieldLabel: 'Master Group Mapping',
            hiddenName: 'ProductGroupID',
            allowBlanks: false
          }, {
            xtype: 'hidden',
            name: 'MasterGroupMappingID',
            value: node.attributes.MasterGroupMappingID
          }],

          success: (function () {
            newForm.destroy();
            node.reload();
          }).createDelegate(this)
        });
        newForm.show();
        //            formConfig = {
        //                url     : Concentrator.route('Create', 'MasterGroupMapping'),
        //                title   : 'Add Master Group Mapping to "' + node.attributes.text + '"',
        //                items   : [{
        //                    xtype: 'productgroup',
        //                    fieldLabel: 'Master Group Mapping',
        //                    hiddenName: 'ProductGroupID',
        //                    allowBlanks: false
        //                }, {
        //                    xtype: 'hidden',
        //                    name: 'MasterGroupMappingID',
        //                    value: node.attributes.MasterGroupMappingID
        //                }]
        //            };
        //            Concentrator.MasterGroupMappingComponents.getSimpleFormWindow(formConfig);
      },

      /**
      Delete node
      */
      deleteNode: function (id, node) {
        if (id > -1) {
          var title = 'Delete Master Group Mapping "' + node.attributes.text + '"';
          var msg = 'Are you sure you want to delete Master Group Mapping "' + node.attributes.text + '" in Master Group Mapping "' + node.parentNode.attributes.text + '" ?';
          Ext.Msg.confirm(title, msg,
            (function (button) {
              if (button == "yes") {
                Diract.mute_request({
                  url: this.treeConfig.deleteUrl,
                  params: { id: id },
                  waitMsg: 'Deleting the master group mapping',
                  success: function () {
                    var panels = {
                      allTabPanels: true,
                      treePanel: true,
                      node: node.parentNode
                    };
                    node.remove();
                    Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                  },
                  failure: function () {
                    config = {
                      MasterGroupMappingID: id,
                      MasterGroupMappingName: node.attributes.text
                    };
                    Concentrator.MasterGroupMappingFunctions.getRelatedObjectsToMasterGroupMapping(config, node);
                  }
                });
              }
            }).createDelegate(this));
        }
      },

      /**
      Rename node
      */
      renameMasterGroupMapping: function (id, node) {
        var grid = new Diract.ui.Grid({
          url: Concentrator.route('GetMasterGroupMappingTranslations', 'MasterGroupMapping'),
          updateUrl: Concentrator.route('SetTranslation', 'MasterGroupMapping'),
          params: {
            MasterGroupMappingID: id.MasterGroupMappingID
          },
          saveAndExit: true,
          primaryKey: ['MasterGroupMappingID', 'LanguageID'],
          sortBy: 'MasterGroupMappingID',
          permissions: {
            list: 'DefaultMasterGroupMapping',
            update: 'DefaultMasterGroupMapping'
          },
          structure: [{
            dataIndex: 'MasterGroupMappingID',
            type: 'int'
          }, {
            dataIndex: 'Language',
            type: 'string',
            header: 'Language'
          }, {
            dataIndex: 'Name',
            type: 'string',
            header: 'Name',
            editor: {
              xtype: 'textfield'
            }
          }, {
            dataIndex: 'LanguageID',
            type: 'int'
          }, {
            dataIndex: 'LanguageMustBeFilled',
            type: 'boolean',
            header: 'Language Control',
            editable: true
          }],
          customButtons: ["->", {
            text: 'Exit',
            iconCls: 'exit',
            handler: function () {
              window.close();
            }
          }]
        });
        var window = new Ext.Window({
          title: 'Translation management',
          items: grid,
          width: 800,
          height: 400,
          layout: 'fit',
          renderTo: 'masterpanel'
        });
        window.show();
      },

      /// <summary>
      /// Rename Master Group Mapping
      /// </summary>
      /// <param name="config">MasterGroupMappingID, MasterGroupMappingName, [languageWizardGrid]</param>
      /// <returns>Language Window</returns>
      renameMasterGroupMappingv2: function (config) {
        tempConfig = {
          treePanelToRefresh: 'MasterGroupMappingTreePanel'
        };
        Ext.apply(config, tempConfig);
        Concentrator.MasterGroupMappingFunctions.getRenameMasterGroupMappingWindow(config);
      },

      languageWizard: function (param, node) {
        this.languageGrid = new Diract.ui.FilterGridPanel({
          excelPlease: true,
          exportAll: false,
          pluralObjectName: 'Vendor Products',
          singularObjectName: 'Vendor Product',
          hideGroupedColumn: 'LanguageName',
          url: Concentrator.route("GetListOfMasterGroupMappingsLanguage", "MasterGroupMapping"),
          forceFit: true,
          params: {
            MasterGroupMappingID: param.MasterGroupMappingID,
            showEmptyRecords: true
          },
          permissions: {
            list: 'DefaultMasterGroupMapping',
            create: 'DefaultMasterGroupMapping',
            remove: 'DefaultMasterGroupMapping',
            update: 'DefaultMasterGroupMapping'
          },
          structure: [{
            dataIndex: 'MasterGroupMappingName',
            header: 'MasterGroupMapping Name',
            type: 'string',
            width: 240
          }, {
            dataIndex: 'LanguageName',
            header: 'Language',
            type: 'string',
            width: 240
          }, {
            dataIndex: 'MasterGroupMappingPath',
            header: 'MasterGroupMapping Path',
            type: 'string',
            width: 240
          }, {
            dataIndex: 'ProductGroupID'
          }, {
            header: 'Matched Vendor Product Groups',
            dataIndex: 'Matched',
            type: 'int'
          }, {
            dataIndex: 'MasterGroupMappingID'
          }],
          windowConfig: {
            height: 450,
            width: 500
          },
          listeners: {
            'rowclick': function (t, rowIndex, e) {
            }
          },
          filterPanelConfig: {
            collapsible: false
          },
          filterItems: [
            new Ext.form.Checkbox({
              fieldLabel: 'Empty Records',
              name: 'showEmptyRecords',
              checked: true,
              listeners: {
                'check': (function (ob, checked) {
                  this.languageGrid.grid.store.baseParams.showEmptyRecords = checked;
                  this.languageGrid.grid.store.load();
                }).createDelegate(this)
              }
            }),
            '-',
            new Ext.form.TextField({
              xtype: 'textfield',
              fieldLabel: 'Search Name',
              name: 'filterName'
            }),
            new Diract.ui.Select({
              label: 'Language',
              name: 'languageID',
              allowBlank: true,
              displayField: 'Name',
              valueField: 'ID',
              width: 175,
              value: Concentrator.user.languageID,
              store: Concentrator.stores.languages,
              listeners: {
                'change': (function (ob, newValue, oldValue) {
                  this.languageGrid.grid.store.baseParams.languageID = newValue;
                }).createDelegate(this)
              }
            })
          ],
          rowActions: [{
            iconCls: 'wrench',
            text: 'View Vendor Product Group',
            handler: (function (record) {
              this.ViewVendorProductGroups(record);
            }).createDelegate(this)
          }, {
            iconCls: 'wrench',
            text: 'View Translations',
            handler: (function (record) {
              //this.ViewTranslations(record);
              config = {
                MasterGroupMappingID: record.get('MasterGroupMappingID'),
                MasterGroupMappingName: record.get('MasterGroupMappingName'),
                languageWizardGrid: this.languageGrid
              };
              this.renameMasterGroupMappingv2(config);


            }).createDelegate(this)
          }]
        });

        var AddAttributesToMasterGroupMappingWindow = new Ext.Window({
          title: 'Language Wizard',
          width: 800,
          height: 500,
          modal: true,
          layout: 'fit',
          items: [this.languageGrid]
        });
        AddAttributesToMasterGroupMappingWindow.show();
      },
      ViewTranslations: function (record) {

        var masterGroupMappingID = record.get('MasterGroupMappingID');

        var grid = new Diract.ui.Grid({
          url: Concentrator.route('GetMasterGroupMappingTranslations', 'MasterGroupMapping'),
          updateUrl: Concentrator.route('SetTranslation', 'MasterGroupMapping'),
          params: {
            MasterGroupMappingID: masterGroupMappingID
          },
          primaryKey: ['MasterGroupMappingID', 'LanguageID'],
          sortBy: 'MasterGroupMappingID',
          refreshAfterSave: true,
          permissions: {
            list: 'GetProductGroup',
            create: 'CreateProductGroup',
            remove: 'DeleteProductGroup',
            update: 'UpdateProductGroup'
          },
          structure: [{
            dataIndex: 'MasterGroupMappingID',
            type: 'int'
          }, {
            dataIndex: 'Language',
            type: 'string',
            header: 'Language'
          }, {
            dataIndex: 'Name',
            type: 'string',
            header: 'Name',
            editor: { xtype: 'textfield' }
          }, {
            dataIndex: 'LanguageID',
            type: 'int'
          }, {
            dataIndex: 'LanguageMustBeFilled',
            type: 'boolean',
            header: 'Language Control',
            editable: true
          }]
        });

        var wind = new Ext.Window({
          title: 'Translation management',
          items: grid,
          modal: true,
          width: 800,
          height: 400,
          layout: 'fit'
        });

        wind.show();

      },

      /**
      Assign To node
      */
      assignMasterGroupMappingTo: function (param, node) {
        var grid = new Concentrator.ui.Grid({
          pluralObjectName: 'Users',
          singularObjectName: 'User',
          primaryKey: ['MasterGroupMappingID', 'UserID'],
          permissions: {
            create: 'DefaultMasterGroupMapping',
            remove: 'DefaultMasterGroupMapping',
            list: 'DefaultMasterGroupMapping'
          },
          url: Concentrator.route("GetAssignedUsers", "MasterGroupMapping"),
          newUrl: Concentrator.route("AssignUserTo", "MasterGroupMapping"),
          deleteUrl: Concentrator.route("UnassignedUserFrom", "MasterGroupMapping"),
          params: {
            'MasterGroupMappingID': param[this.treeConfig.hierarchicalIndexID]
          },
          newParams: {
            'MasterGroupMappingID': param[this.treeConfig.hierarchicalIndexID]
          },
          structure: [
                  { dataIndex: 'MasterGroupMappingID', type: 'int' },
                  { dataIndex: 'UserID', type: 'int' },
                  { dataIndex: 'Name', type: 'string', header: 'Name', editable: false, editor: { xtype: 'user', allowBlank: false } }
          ]
        });
        var window = new Ext.Window({
          title: 'User(s) assigned to Master Group Mapping "' + node['text'] + '"',
          width: 600,
          height: 300,
          modal: true,
          layout: 'fit',
          items: [grid]
        });
        window.show();

      },
      ViewVendorProductGroups: function (record) {
        var ViewVendorProductGroups = new Diract.ui.Grid({
          primaryKey: 'ProductGroupVendorID',
          sortBy: 'ProductGroupVendorID',
          forceFit: true,
          permissions: {
            list: 'DefaultMasterGroupMapping',
            update: 'DefaultMasterGroupMapping'
          },
          params: {
            MasterGroupMappingID: record.get('MasterGroupMappingID')
          },
          url: Concentrator.route("GetListOfVendorProductGroupsPerMGM", "MasterGroupMapping"),
          structure:
              [

              {
                dataIndex: 'VendorName',
                header: 'Vendor',
                type: 'string'
              },
              {
                dataIndex: 'VendorCodeDescription',
                header: 'Vendor Code Description',
                type: 'string'
              },
              {
                dataIndex: 'ProductGroupCode',
                header: 'Product Group Code',
                type: 'string'
              }, {
                dataIndex: 'BrandCode',
                type: 'string',
                header: 'Brand Code'
              }
              ],
          customButtons: [
              {
                text: 'Exit',
                iconCls: 'exit',
                alignRight: true,
                handler: function () {
                  viewVendorProductGroupsWindow.close();
                }
              } //Close button 
          ]
        });
        var viewVendorProductGroupsWindow = new Ext.Window({
          modal: true,
          title: 'Matched Vendor Product Groups',
          items: ViewVendorProductGroups,
          layout: 'fit',
          width: 700,
          height: 400
        });
        viewVendorProductGroupsWindow.show();

      },
      attributeManagement: function (param, node) {
        var AddAttributesToMasterGroupMappingGrid = new Diract.ui.Grid({
          pluralObjectName: 'Attributes',
          singularObjectName: 'Attribute',
          primaryKey: 'AttributeID',
          forceFit: true,
          sortBy: 'AttributeGroupName',
          id: 'AssignedAttributesGrid',
          permissions: {
            list: 'DefaultMasterGroupMapping',
            update: 'DefaultMasterGroupMapping'
          },
          params: {
            MasterGroupMappingID: param.MasterGroupMappingID
          },
          url: Concentrator.route("GetListOfMatchedMasterGroupMappingAttributes", "MasterGroupMapping"),
          structure: [
                  {
                    dataIndex: 'MasterGroupMappingID',
                    type: 'int'
                  },
                  {
                    dataIndex: 'AttributeID',
                    type: 'int'
                  },
                  {
                    dataIndex: 'AttributeGroupName',
                    header: 'AttributeGroup Name'
                  },
                  {
                    dataIndex: 'AttributeName',
                    header: 'Attribute Name'
                  },
                  {
                    dataIndex: 'VendorName',
                    header: 'VendorName'
                  }
          ],
          rowActions: [{
            iconCls: 'delete',
            text: 'Delete Attribute',
            handler: function (record) {
              Concentrator.MasterGroupMappingFunctions.deleteAttribute(record);
            }
          }, {
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
          }],
          customButtons: [
                  {
                    text: 'Add Attribute',
                    iconCls: 'save',
                    handler: function (record) {
                      Concentrator.MasterGroupMappingFunctions.addAttribute(param.MasterGroupMappingID, node.attributes.text);
                    }
                  },
                  {
                    iconCls: 'copy',
                    text: 'Copy Attributes',
                    handler: function (record) {
                      Concentrator.MasterGroupMappingFunctions.copyAttributes(param.MasterGroupMappingID, node.attributes.text);
                    }
                  },
                  {
                    iconCls: 'wrench',
                    text: 'Attributes Wizard',
                    handler: function (record) {
                      Concentrator.MasterGroupMappingFunctions.attributesWizard(param.MasterGroupMappingID, node.attributes.text);
                    }
                  },
                  {
                    text: 'Exit',
                    iconCls: 'exit',
                    alignRight: true,
                    handler: function () {
                      AddAttributesToMasterGroupMappingWindow.close();
                    }
                  } //Cancel button 
          ]
        });
        var AddAttributesToMasterGroupMappingWindow = new Ext.Window({
          title: 'Assigned Attributes to Master Group Mapping "' + node.attributes.text + '"',
          width: 800,
          height: 500,
          modal: true,
          layout: 'fit',
          items: [AddAttributesToMasterGroupMappingGrid]
        });
        AddAttributesToMasterGroupMappingWindow.show();
      }
    });
    return gr;
  })();
})();