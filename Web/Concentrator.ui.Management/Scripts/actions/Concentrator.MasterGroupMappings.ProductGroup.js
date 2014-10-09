(function () {
  var functionalities = Concentrator.MasterGroupMappingFunctionalities;

  Concentrator.MasterGroupMappingComponents = {
    // config: configName   : type (default value) or (*) => required field

    //  form
    //  labelWidth      : int (140)
    //  url             : string (*) => example. Concentrator.route('', '') 
    //  width           : int (400)
    //  height          : int (100)
    //  frame           : bool (true)
    //  items           : [{item1},{item2},..] (*)
    //  btnCancelText   : string ('Cancel')
    //  btnCancelIcon   : string (empty)
    //  btnSaveText     : string ('Save')
    //  btnSaveIcon     : string (empty)

    //  window
    //  modal           : bool (true)
    //  resizable       : bool (false)
    //  closable        : bool (false)
    //  plain           : bool (false)
    //  title           : string (empty)

    getSimpleForm: function (config) {
      var formPanel = new Ext.FormPanel({
        url: config.url,
        items: config.items,
        labelWidth: config.labelWidth || 140,
        width: config.width || 400,
        height: config.height || 100,
        frame: config.frame || true,
        buttons: [{
          text: 'Save',
          iconCls: '',
          handler: function () {
            formPanel.getForm().submit({
              clientValidation: true,
              success: function (form, action) {
                Ext.Msg.alert('Success', action.result.msg);
              },
              failure: function (form, action) {
                switch (action.failureType) {
                  case Ext.form.Action.CLIENT_INVALID:
                    Ext.Msg.alert('Failure', 'Form fields may not be submitted with invalid values');
                    break;
                  case Ext.form.Action.CONNECT_FAILURE:
                    Ext.Msg.alert('Failure', 'Ajax communication failed');
                    break;
                  case Ext.form.Action.SERVER_INVALID:
                    Ext.Msg.alert('Failure', action.result.msg);
                }
              }
            });
          }
        }, {
          text: config.btnCancelText || 'Cancel',
          iconCls: config.btnCancelIcon || '',
          handler: function () {
            window.close();
          }
        }]
      });
      var window = new Ext.Window({
        modal: config.modal || true,
        plain: config.plain || false,
        resizable: config.resizable || false,
        closable: config.closable || false,
        title: config.title || '',
        items: formPanel,
        layout: 'fit'
      });
      window.show();
    },
    getSimpleFormWindow: Ext.extend(Ext.Window, {
      width: 300,
      height: 200,
      modal: true,
      buttonText: "Submit",
      formStyle: 'padding: 8px;',
      submit: function (config) {
        var that = this;
        this.form.submitForm({
          params: config.params,
          suppressSuccessMsg: config.suppressSuccessMsg,
          success: config.success || that.success
        });
      },
      constructor: function (config) {
        var that = this;

        config = Ext.apply({}, config, this);
        config.layout = 'fit';

        if (config.form) {
          this.form = config.form;
        } else if (config.items) {
          var buttons = [];

          this.button = new Ext.Button({
            text: config.buttonText,
            formBind: true,
            handler: config.buttonHandler || function () {
              if (that.fireEvent('beforesubmit', that, that.form)) {
                that.submit(config);
                that.fireEvent('aftersubmit', that, that.form);
              }
            }
          });

          buttons.push(this.button);

          if (config.cancelButton) {
            var cancelButton = new Ext.Button({
              text: "Cancel",
              handler: function () {
                that.destroy();
              }
            });
            buttons.push(cancelButton);
          }

          if (config.disableButton !== undefined) {

            this.form = new Diract.ui.Form({
              border: config.border,
              url: config.url,
              disableButton: true,
              fileUpload: config.fileUpload || false,
              items: config.items,
              loadUrl: config.loadUrl,
              loadParams: config.loadParams,
              bodyStyle: config.formStyle,
              autoScroll: config.autoScroll || false
            });
          }
          else {

            this.form = new Diract.ui.Form({
              border: config.border,
              url: config.url,
              buttons: buttons,
              fileUpload: config.fileUpload || false,
              items: config.items,
              loadUrl: config.loadUrl,
              loadParams: config.loadParams,
              bodyStyle: config.formStyle,
              autoScroll: config.autoScroll || false
            });

          }
        }
        config.items = [this.form];

        Concentrator.MasterGroupMappingComponents.getSimpleFormWindow2.superclass.constructor.call(this, config);
      }
    })

  };

  Concentrator.MasterGroupMappingFunctions = {
    viewVendorAssortments: function (record) {
      var windowTitle = 'Vendor Assortments of Product "' + record.get('ConcentratorNumber');
      if (record.get('ProductName') != null && record.get('ProductName') != "") {
        windowTitle += ', ' + record.get('ProductName');
      }
      windowTitle += '"';

      var viewVendorAssortmentsGrid = new Diract.ui.Grid({
        pluralObjectName: 'Vendor Assortments',
        singularObjectName: 'Vendor Assortment',
        primaryKey: 'VendorAssortmentID',
        forceFit: true,
        id: 'ViewVendorAssortmentsGrid',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          ProductID: record.get('ConcentratorNumber')
        },
        url: Concentrator.route("GetListOfVendorAssortmentsByProductID", "MasterGroupMapping"),
        structure:
							[
							{
							  dataIndex: 'VendorAssortmentID',
							  type: 'int'

							},
							{
							  dataIndex: 'VendorName',
							  header: 'Vendor Name'
							},
							{
							  dataIndex: 'DistriItemNumber',
							  header: 'Distri Item Number'
							},
							{
							  dataIndex: 'ProductName',
							  header: 'Product Name'
							}
							],
        customButtons: [
							{
							  text: 'Exit',
							  iconCls: 'exit',
							  alignRight: true,
							  handler: function () {
							    viewVendorAssortmentsWindow.close();
							  }
							} //Close button 
        ]
      });
      var viewVendorAssortmentsWindow = new Ext.Window({
        modal: true,
        title: windowTitle,
        items: viewVendorAssortmentsGrid,
        layout: 'fit',
        width: 800,
        height: 500,
        renderTo: 'masterpanel'
      });
      viewVendorAssortmentsWindow.show();
    },
    getAssortementsOfVendorProductGroupWindow: function (record) {
      var vendorProductGourpName = this.getVendorProductGroupName(record);
      if (record.get('IsBlocked')) {
        btnBlockText = 'UnBlock Vendor Product Group';
        btnBlockIcon = 'unchecked';
      } else {
        btnBlockText = 'Block Vendor Product Group';
        btnBlockIcon = 'checked';
      }
      var gridTitle = 'Vendor Assortments of Vendor Product Group "' + vendorProductGourpName + '"';
      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Vendor Products',
        singularObjectName: 'Vendor Product',
        id: 'VendorAssortmentPanel',
        sortBy: 'VendorAssortmentID',
        params: {
          ProductGroupVendorID: record.get('ProductGroupVendorID')
        },
        url: Concentrator.route("GetListOfVendorAssortment", "MasterGroupMapping"),
        forceFit: false,
        permissions: {
          list: 'GetProductGroupVendor',
          create: 'CreateProductGroupVendor',
          remove: 'DeleteProductGroupVendor',
          update: 'UpdateProductGroupVendor'
        },
        structure: [{
          dataIndex: 'VendorAssortmentID',
          type: 'int'
        }, {
          dataIndex: 'ConcentratorNumber',
          header: 'Concentrator #',
          type: 'int'
        }, {
          dataIndex: 'DistriItemNumber',
          header: 'Distri Item Number',
          type: 'string'
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name',
          type: 'string',
          width: 240
        }, {
          dataIndex: 'Brand',
          header: 'Brand',
          type: 'string'
        }, {
          dataIndex: 'ProductCode',
          header: 'Product Code',
          type: 'string'
        }, {
          dataIndex: 'Image',
          header: 'Image',
          type: 'boolean'
        }, {
          dataIndex: 'PossibleMatch',
          header: 'Possible Match',
          type: 'int',
          renderer: function (val, m, record) {
            return record.get('PossibleMatch') + " %";
          }
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: gridTitle,
        items: grid,
        layout: 'fit',
        width: 900,
        height: 500,
        renderTo: 'masterpanel',
        tbar: [{
          text: btnBlockText,
          iconCls: btnBlockIcon,
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.setBlockValueForVendorProductGroup(record);
            window.close();
          }
        }, {
          text: 'Show Product Details',
          iconCls: 'view',
          handler: function () {
            productGridSelection = grid.getSelectionModel();
            if (productGridSelection.getCount() > 0) {
              record = productGridSelection.getSelected();
              Concentrator.MasterGroupMappingFunctions.getEditProductWindow(record.get('ConcentratorNumber'));
            } else {
              Ext.Msg.alert('', 'Please select one vendor product to show details"');
            }
          }
        }, "->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      window.show();
    },
    setBlockValueForVendorProductGroup: function (record) {
      var vendorProductGroupName = this.getVendorProductGroupName(record);
      if (record.get('IsBlocked')) {
        Diract.request({
          url: Concentrator.route('UpdateProductGroupVendors', 'MasterGroupMapping'),
          params: {
            _ProductGroupVendorID: record.get('ProductGroupVendorID'),
            IsBlocked: false
          },
          callback: function () {
            var panels = {
              vendorProductGroupsTabPanel: true
            };
            Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
          }
        });
      } else {
        Diract.silent_request({
          url: Concentrator.route('GetListOfMatchedMasterGroupMappings', 'MasterGroupMapping'),
          params: {
            ProductGroupVendorID: record.get('ProductGroupVendorID')
          },
          waitMsg: 'Please wait..',
          success: function (response) {
            var masterGroupMappings = new Array();
            if (response.results.length > 0) {
              if (response.results.length > 1) {
                var masterGroupMappingPads = "<br> Vendor Product Group is matched in the next Master Group Mappings:";
              } else {
                var masterGroupMappingPads = "<br> Vendor Product Group is matched in the next Master Group Mapping:";
              }
              Ext.each(response.results, function (r) {
                masterGroupMappingPads += "<br>" + r.MasterGroupMappingPad;
                masterGroupMappings.push(r.MasterGroupMappingID);
              });
            } else {
              masterGroupMappingPads = "<br> Product Group Vendor is not matched";
            }

            Ext.Msg.show({
              title: 'Vendor Product Group',
              msg: 'Would you like to block the Vendor Product Group"' + vendorProductGroupName + '"?<br>' + masterGroupMappingPads,
              buttons: { ok: "Yes", cancel: "No" },
              fn: function (result) {
                if (result == 'ok') {
                  Diract.request({
                    url: Concentrator.route('UpdateProductGroupVendors', 'MasterGroupMapping'),
                    params: {
                      _ProductGroupVendorID: record.get('ProductGroupVendorID'),
                      IsBlocked: true
                    },
                    waitMsg: 'Blocking product group please wait..',
                    callback: function () {
                      var panels = {
                        vendorProductGroupsTabPanel: true
                      };
                      Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                    }
                  });
                } else {
                };
              },
              animEl: 'elId',
              icon: Ext.MessageBox.QUESTION
            });
          }
        });
      }
    },
    getMatchVendorProductGroupWindow: function (params) {
      var masterGroupMappingGridTitle = '';
      if (params.VendorProductGroupRecords.length > 1) {
        masterGroupMappingGridTitle = 'Match Master Group Mappings to ' + params.VendorProductGroupRecords.length + ' Vendor Product Groups';
      } else {
        record = params.VendorProductGroupRecords[0];
        masterGroupMappingGridTitle = 'Match Master Group Mappings to Vendor Product Group "' + this.getVendorProductGroupName(record) + '"';
      }

      var productGroupVendorGrid = new Diract.ui.Grid({
        pluralObjectName: 'Master Group Mapping Nodes',
        singularObjectName: 'Master Group Mapping Node',
        primaryKey: 'MasterGroupMappingID',
        forceFit: false,
        id: 'MatchProductGroupVendors',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfMatchedMasterGroupMappingNames", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'Check',
          header: 'Check To Match',
          type: 'boolean',
          editable: true,
          width: 50
        }, {
          dataIndex: 'MasterGroupMappingPad',
          header: 'MasterGroupMapping path',
          type: 'string',
          width: 650
        }],
        customButtons: [{
          text: 'Map Master Group Mappings',
          iconCls: 'save',
          alignRight: false,
          handler: function () {
            var ListOfProductGroupIDs = { MasterGroupMappingIDs: [], ProductGroupVendorIDs: [] };
            listOfProductGroupVendors = Ext.getCmp('MatchProductGroupVendors').store.data.items;
            Ext.each(listOfProductGroupVendors, function (r) {
              if (r.data.Check) {
                ListOfProductGroupIDs.MasterGroupMappingIDs.push(r.data.MasterGroupMappingID);
              }
            });
            Ext.each(params.VendorProductGroupRecords, function (r) {
              ListOfProductGroupIDs.ProductGroupVendorIDs.push(r.get('ProductGroupVendorID'));
            });
            var ListOfProductGroupIDsJson = JSON.stringify(ListOfProductGroupIDs);
            productGroupVendorWindow.close();

            var vendorProductGridTitle = '';
            if (ListOfProductGroupIDs.MasterGroupMappingIDs.length > 1) {
              vendorProductGridTitle = 'Map Vendor Products to ' + ListOfProductGroupIDs.MasterGroupMappingIDs.length + ' Master Group Mappings';
            } else {
              vendorProductGridTitle = 'Map Vendor Products to Master Group Mappings "' + listOfProductGroupVendors[0].get('MasterGroupMappingPad') + '"';
            }

            var grid = new Diract.ui.Grid({
              pluralObjectName: 'Vendor Products',
              singularObjectName: 'Vendor Product',
              primaryKey: 'VendorAssortmentID',
              forceFit: true,
              id: 'MatchVendorProductsGrid',
              permissions: {
                list: 'DefaultMasterGroupMapping',
                update: 'DefaultMasterGroupMapping'
              },
              pageSize: 0,
              params: {
                ListOfProductGroupIDsJson: ListOfProductGroupIDsJson,
                limit: 99999
              },
              groupField: 'productGroupVendorName',
              hideGroupedColumn: 'productGroupVendorName',
              url: Concentrator.route("GetListOfVendorProductsGroup", "MasterGroupMapping"),
              structure: [{
                dataIndex: 'VendorAssortmentID',
                type: 'int'
              }, {
                dataIndex: 'ProductID',
                type: 'int'
              }, {
                dataIndex: 'check',
                header: 'Check To Map',
                type: 'boolean',
                editable: true,
                width: 100
              }, {
                dataIndex: 'ShortDescription',
                header: 'Short Description',
                type: 'string',
                width: 250
              }, {
                dataIndex: 'CustomItemNumber',
                header: 'Custom Item Number',
                type: 'string',
                width: 100
              }, {
                dataIndex: 'VendorItemNumber',
                header: 'Vendor Part Number',
                type: 'string',
                width: 100
              }, {
                dataIndex: 'productGroupVendorName',
                header: 'Vendor Product Group',
                type: 'string'
              }],
              customButtons: [{
                text: 'Map Vendor Products',
                iconCls: 'save',
                alignRight: false,
                handler: function () {
                  var listOfMgmAndVpIDs = { MasterGroupMappingIDs: [], ProductIDs: [] }; // list of master group mapping and vendor products
                  listOfMgmAndVpIDs.MasterGroupMappingIDs = ListOfProductGroupIDs.MasterGroupMappingIDs;
                  listOfVendorProducts = Ext.getCmp('MatchVendorProductsGrid').store.data.items;
                  Ext.each(listOfVendorProducts, function (r) {
                    if (r.get('check') == true) {
                      listOfMgmAndVpIDs.ProductIDs.push(r.get('ProductID'));
                    }
                  });
                  var listOfMgmAndVpIDsJson = JSON.stringify(listOfMgmAndVpIDs);
                  Diract.request({
                    url: Concentrator.route('MatchProductGroupVendor', 'MasterGroupMapping'),
                    waitMsg: 'Saving Mapped Vendor Product Groups',
                    params: {
                      ListOfProductGroupIDsJson: ListOfProductGroupIDsJson
                    },
                    callback: function () {
                      Diract.request({
                        url: Concentrator.route('MatchVendorProductGroups', 'MasterGroupMapping'),
                        waitMsg: 'Saving Mapped Vendor Products',
                        params: {
                          listOfMgmAndVpIDsJson: listOfMgmAndVpIDsJson
                        },
                        callback: function () {
                          var panels = {
                            allTabPanels: true,
                            treePanel: true,
                            MasterGroupMappings: listOfMgmAndVpIDs.MasterGroupMappingIDs
                          };
                          Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                          window.close();
                        }
                      });
                    }
                  });
                }
              }, "->", {
                text: 'Select All',
                iconCls: 'checked',
                handler: function () {
                  var MatchGrid = Ext.getCmp('MatchVendorProductsGrid');
                  MatchGrid.store.each(function (rec) { rec.set('check', true) });
                }
              }, {
                text: 'Deselect All',
                iconCls: 'unchecked',
                handler: function () {
                  var MatchGrid = Ext.getCmp('MatchVendorProductsGrid');
                  MatchGrid.store.each(function (rec) { rec.set('check', false) });
                }
              }, {
                text: 'Show Product Details',
                iconCls: 'view',
                handler: function () {
                  productGridSelection = grid.getSelectionModel();
                  if (productGridSelection.getCount() > 0) {
                    record = productGridSelection.getSelected();
                    Concentrator.MasterGroupMappingFunctions.getEditProductWindow(record.get('ProductID'));
                  } else {
                    Ext.Msg.alert('', 'Please select one vendor product to show details"');
                  }
                }
              }, {
                text: 'Exit without save matches',
                iconCls: 'exit',
                handler: function () {
                  window.close();
                }
              }]
            });
            var window = new Ext.Window({
              modal: true,
              title: vendorProductGridTitle,
              items: grid,
              layout: 'fit',
              width: 800,
              height: 500,
              renderTo: 'MasterGroupMappingCenterPanel'
            });
            window.show();
          }
        }, "->", {
          text: 'Select All',
          iconCls: 'checked',
          handler: function () {
            var unMatchGrid = Ext.getCmp('MatchProductGroupVendors');
            unMatchGrid.store.each(function (rec) { if (!(rec.get('Check') == 3) && !(rec.get('Check') == 2)) rec.set('Check', true) });
          }
        }, {
          text: 'Deselect All',
          iconCls: 'unchecked',
          handler: function () {

            var unMatchGrid = Ext.getCmp('MatchProductGroupVendors');
            unMatchGrid.store.each(function (rec) { if (!(rec.get('Check') == 3) && !(rec.get('Check') == 2)) rec.set('Check', false) });
          }
        }, {
          text: 'Exit without save matches',
          iconCls: 'exit',
          handler: function () {
            productGroupVendorWindow.close();
          }
        }]
      });
      var productGroupVendorWindow = new Ext.Window({
        modal: true,
        title: masterGroupMappingGridTitle,
        items: productGroupVendorGrid,
        layout: 'fit',
        width: 800,
        height: 500,
        renderTo: 'MasterGroupMappingCenterPanel'
      });
      productGroupVendorWindow.show();
    },
    getMatchVendorProductWindow: function (parameters) {
      var params = {
        MasterGroupMappingID: 0,
        VendorProductRecords: [],
        node: null
      };
      Ext.apply(params, parameters);

      var masterGroupMappingGridTitle = '';
      var productIDs = "";
      if (params.VendorProductRecords.length > 1) {
        masterGroupMappingGridTitle = 'Map Master Group Mappings to ' + params.VendorProductRecords.length + ' Vendor Products';
        for (var i = 0; i < params.VendorProductRecords.length; i++) {
          productIDs += params.VendorProductRecords[i].get('ConcentratorNumber');
          if (i != params.VendorProductRecords.length - 1) productIDs += ",";
        }
      } else {
        record = params.VendorProductRecords[0];
        productIDs = record.get('ConcentratorNumber');
        masterGroupMappingGridTitle = 'Map Master Group Mappings to Vendor Product "' + record.get('ConcentratorNumber') + ', ' + record.get('ProductName') + '"';
      }

      var hasAllProductsMasterGroupMappingID = true;
      var listOfRecords = [];
      Ext.each(params.VendorProductRecords, function (record) {
        var MasterGroupMappingID = record.get('MasterGroupMappingID');
        var ProductID = record.get('ConcentratorNumber');

        var item = {
          MasterGroupMappingID: MasterGroupMappingID,
          ProductID: ProductID
        };

        listOfRecords.push(item);

        if (MasterGroupMappingID == 0)
          hasAllProductsMasterGroupMappingID = false;
      });
      if (hasAllProductsMasterGroupMappingID) {
        Ext.Msg.show({
          title: 'Copy or Move the products?',
          msg: 'Would you like to Copy or Move the products?',
          buttons: { yes: "Copy", no: 'Move', cancel: "Cancel" },
          fn: function (result) {
            var listOfIDs = {
              CopyProducts: false,
              MasterGroupMappingID: params.MasterGroupMappingID,
              ListOfRecords: listOfRecords
            };
            var msg = 'Moving products';
            switch (result) {
              case 'yes':
                listOfIDs.CopyProducts = true;
                var msg = 'Coping products';
              case 'no':
                var listOfIDsJson = JSON.stringify(listOfIDs);
                Diract.request({
                  url: Concentrator.route('CopyOrMoveMatchedProducts', 'MasterGroupMapping'),
                  waitMsg: msg,
                  params: {
                    listOfIDsJson: listOfIDsJson
                  },
                  success: function () {
                    connectorTree = Ext.getCmp('masterGroupMappingTreePanel');
                    config = {
                      MasterGroupMappingIDs: [],
                      RootNode: connectorTree.getRootNode()
                    };

                    Ext.each(listOfRecords, function (record) {
                      if (config.MasterGroupMappingIDs.indexOf(record.MasterGroupMappingID) == -1) {
                        config.MasterGroupMappingIDs.push(record.MasterGroupMappingID);
                      }
                    });
                    config.MasterGroupMappingIDs.push(listOfIDs.MasterGroupMappingID);
                    Concentrator.MasterGroupMappingFunctions.refreshTree(config);
                    Concentrator.MasterGroupMappingFunctions.refreshVendorProductsGrid(listOfIDs.MasterGroupMappingID);
                  },
                  failure: function () {
                    connectorTree = Ext.getCmp('masterGroupMappingTreePanel');
                    config = {
                      MasterGroupMappingIDs: [],
                      RootNode: connectorTree.getRootNode()
                    };
                    config.MasterGroupMappingIDs.push(listOfIDs.MasterGroupMappingID);
                    Concentrator.MasterGroupMappingFunctions.refreshTree(config);
                    Concentrator.MasterGroupMappingFunctions.refreshVendorProductsGrid(listOfIDs.MasterGroupMappingID);
                  }
                });
                break;
              case 'cancel':
                break;
            }
          },
          animEl: 'elId',
          icon: Ext.MessageBox.QUESTION
        });
      } else {
        var MatchVendorProductGrid = new Diract.ui.Grid({
          pluralObjectName: 'Master Group Mapping Nodes',
          singularObjectName: 'Master Group Mapping Node',
          primaryKey: 'MasterGroupMappingID',
          forceFit: false,
          id: 'VendorProductsPanelMatchVendorProductGrid',
          permissions: {
            list: 'DefaultMasterGroupMapping',
            update: 'DefaultMasterGroupMapping'
          },
          params: {
            productIDs: productIDs,
            MasterGroupMappingID: params.MasterGroupMappingID
          },
          url: Concentrator.route("GetListOfMatchedMasterGroupMappingNames", "MasterGroupMapping"),
          structure: [{
            dataIndex: 'MasterGroupMappingID',
            type: 'int'
          }, {
            dataIndex: 'Check',
            header: 'Check To Map',
            type: 'boolean',
            editable: true,
            width: 50
          }, {
            dataIndex: 'MasterGroupMappingPad',
            header: 'MasterGroupMapping path',
            type: 'string',
            width: 650
          }],
          customButtons: [{
            text: 'Map Vendor Products',
            iconCls: 'save',
            alignRight: false,
            handler: function () {
              var ListOfToMatchIDs = { MasterGroupMappingIDs: [], ProductIDs: [] };
              listOfPMasterGroupMappings = Ext.getCmp('VendorProductsPanelMatchVendorProductGrid').store.data.items;
              Ext.each(listOfPMasterGroupMappings, function (r) {
                if (r.data.Check) {
                  ListOfToMatchIDs.MasterGroupMappingIDs.push(r.data.MasterGroupMappingID);
                }
              });
              Ext.each(params.VendorProductRecords, function (r) {
                ListOfToMatchIDs.ProductIDs.push(r.get('ConcentratorNumber'));
              });
              var ListOfToMatchIDsJson = JSON.stringify(ListOfToMatchIDs);
              Diract.request({
                url: Concentrator.route('MatchVendorProduct', 'MasterGroupMapping'),
                params: {
                  ListOfToMatchIDsJson: ListOfToMatchIDsJson
                },
                callback: function () {
                  var panels = {
                    vendorProductsTabPanel: true,
                    matchedVendorProductsTabPanel: true,
                    treePanel: true,
                    node: params.node
                  };
                  Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                  MatchVendorProductWindow.close();
                }
              });
            }
          }, "->", {
            text: 'Select All',
            iconCls: 'checked',
            handler: function () {
              var MatchGrid = Ext.getCmp('VendorProductsPanelMatchVendorProductGrid');
              MatchGrid.store.each(function (rec) { if (!(rec.get('Check') == 3) && !(rec.get('Check') == 2)) rec.set('Check', true) });
            }
          }, {
            text: 'Deselect All',
            iconCls: 'unchecked',
            handler: function () {
              var MatchGrid = Ext.getCmp('VendorProductsPanelMatchVendorProductGrid');
              MatchGrid.store.each(function (rec) { if (!(rec.get('Check') == 3) && !(rec.get('Check') == 2)) rec.set('Check', false) });
            }
          }, {
            text: 'Exit without save matches',
            iconCls: 'exit',
            handler: function () {
              MatchVendorProductWindow.close();
            }
          }]
        });
        var MatchVendorProductWindow = new Ext.Window({
          modal: true,
          title: masterGroupMappingGridTitle,
          items: MatchVendorProductGrid,
          layout: 'fit',
          width: 800,
          height: 500,
          renderTo: 'MasterGroupMappingCenterPanel'
        });
        MatchVendorProductWindow.show();
      }
    },
    getChangedAndBooleanFields: function (record) {
      var changes = record.getChanges();
      Ext.each(record.fields.items, function (field) {
        if (field.type === "boolean") {
          changes[field.name] = record.get(field.name);
        }
      });
      return changes;
    },
    countChangedFields: function (edits) {
      var totalCellsChanged = 0;

      for (var i = 0, len = edits.length; i < len; i++) {

        var changes = edits[i].getChanges();

        for (var change in changes) {
          if (change) {
            totalCellsChanged++;
          }
        }
      }

      return totalCellsChanged;

    },
    // Parameters
    // Record: Grid Store record 
    getUnMatchVendorProductGroupWindow: function (record, connectorID) {
      var records = record.length ? record : [record];

      var params = {};
      var title = "";
      var url = "";

      if (records.length > 1) {
        //loop 
        var data = { GridMasterGroupMappingID: record[0].get('MasterGroupMappingID'), ProductGroupVendorIDs: [] };
        for (var i = 0; i < records.length; i++) {
          var record = records[i];
          data.ProductGroupVendorIDs.push(record.get('ProductGroupVendorID'));
        }
        params = { records: JSON.stringify(data), ConnectorID: connectorID };
        title = 'UnMatch ' + data.ProductGroupVendorIDs.length + ' Vendor Product Groups';
        url = Concentrator.route("GetListOfMultipleMatchedMasterGroupMappings", "MasterGroupMapping");
      } else {
        var record = records[0];
        var vendorProductGroupName = this.getVendorProductGroupName(record);
        params = {
          ProductGroupVendorID: record.get('ProductGroupVendorID'),
          GridMasterGroupMappingID: record.get('MasterGroupMappingID'),
          ConnectorID: connectorID
        };
        title = 'Unmap Vendor Product Group "' + vendorProductGroupName + '"';
        url = Concentrator.route("GetListOfMatchedMasterGroupMappings", "MasterGroupMapping");
      }


      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Master Group Mapping Nodes',
        singularObjectName: 'Master Group Mapping Node',
        primaryKey: ['ProductGroupVendorID', 'MasterGroupMappingID'],
        forceFit: true,
        id: 'UnMatchProductGroupVendors',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: params,
        url: url,
        groupField: records.length > 1 ? 'ProductGroupVendorName' : "",
        structure: [{
          dataIndex: 'ProductGroupVendorID',
          type: 'int'
        }, {
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'Check',
          header: 'Check To UnMatch',
          type: 'boolean',
          editable: true,
          width: 50
        }, {
          dataIndex: 'MasterGroupMappingPad',
          header: 'MasterGroupMapping path',
          type: 'string',
          width: 650
        }, {
          dataIndex: 'ProductGroupVendorName',
          type: 'string',
          hidden: true,
          header: 'ProductGroupVendor Name'
        }],
        customButtons: [{
          text: 'Unmap Vendor Product Group',
          iconCls: 'save',
          alignRight: false,
          handler: function () {
            var ListOfVpgAndMgmIDs;
            var unMatchUrl = "";
            var data = [];
            var store = Ext.getCmp('UnMatchProductGroupVendors').store;
            var ListOfMgmIDs = store.data.items;
            var title = "";

            if (records.length > 1) {
              title = 'UnMatch ' + records.length + ' Vendor Product Groups';
              var currentMasterGroupMappingID;
              var MasterGroupMappingGroups = store.collect('MasterGroupMappingID');
              for (var i = 0; i < MasterGroupMappingGroups.length; i++) {
                var listOfProductGroupIDValuesByMGM = store.query('MasterGroupMappingID', MasterGroupMappingGroups[i]);
                var MGMVPGModel = { MasterGroupMappingID: MasterGroupMappingGroups[i], ListOfVendorProductGroupIDs: [], ListOfVendorProductIDs: [] };
                Ext.each(listOfProductGroupIDValuesByMGM.items, function (r) {
                  if (r.data.Check) {
                    MGMVPGModel.ListOfVendorProductGroupIDs.push(r.data.ProductGroupVendorID);
                  }
                });
                if (MGMVPGModel.ListOfVendorProductGroupIDs.length > 0)
                  data.push(MGMVPGModel);
              }
              ListOfVpgAndMgmIDs = JSON.stringify(data);
              unMatchUrl = Concentrator.route("GetListVendorProductsByMasterGroupMappingAndMultipleVendorProductGroup", "MasterGroupMapping");
            } else {
              title = 'Unmap Vendor Product Group "' + vendorProductGroupName + '"';
              listOfVpgAndMgmAndVpIDs = { VendorProductGroupID: 0, ListOfMasterGroupMappingIDs: [], ListOfVendorProductIDs: [] };
              listOfVpgAndMgmAndVpIDs.VendorProductGroupID = record.get('ProductGroupVendorID');
              Ext.each(ListOfMgmIDs, function (r) {
                if (r.data.Check) {
                  listOfVpgAndMgmAndVpIDs.ListOfMasterGroupMappingIDs.push(r.data.MasterGroupMappingID);
                }
              });
              ListOfVpgAndMgmIDs = JSON.stringify(listOfVpgAndMgmAndVpIDs);
              unMatchUrl = Concentrator.route("GetListVendorProductsByMasterGroupMappingAndVendorProductGroup", "MasterGroupMapping");
            }
            window.close();

            var productGrid = new Diract.ui.Grid({
              pluralObjectName: 'Vendor Products',
              singularObjectName: 'Vendor Product',
              primaryKey: 'VendorAssortmentID',
              forceFit: true,
              id: 'UnMatchVendorProductsGrid',
              permissions: {
                list: 'DefaultMasterGroupMapping',
                update: 'DefaultMasterGroupMapping'
              },
              pageSize: 0,
              params: {
                ListOfIDsJson: ListOfVpgAndMgmIDs,
                limit: 99999
              },
              groupField: 'productGroupVendorName',
              hideGroupedColumn: 'productGroupVendorName',
              url: unMatchUrl,
              structure: [{
                dataIndex: 'VendorAssortmentID',
                type: 'int'
              }, {
                dataIndex: 'ProductID',
                type: 'int'
              }, {
                dataIndex: 'Check',
                header: 'Check To Map',
                type: 'boolean',
                editable: true,
                width: 50
              }, {
                dataIndex: 'ShortDescription',
                header: 'Short Description',
                type: 'string',
                width: 650
              }, {
                dataIndex: 'productGroupVendorName',
                header: 'Vendor Product Group',
                type: 'string'
              }, {
                dataIndex: 'CustomItemNumber',
                header: 'Custom Item Number'
              }, {
                dataIndex: 'VendorItemNumber',
                header: 'Vendor Item Number'
              }, {
                dataIndex: 'MasterGroupMappingID',
                type: 'int'
              },
							{
							  dataIndex: 'ProductGroupVendorID',
							  type: 'int'
							}],
              customButtons: [{
                text: 'UnMatch Vendor Products',
                iconCls: 'save',
                alignRight: false,
                handler: function () {
                  var listOfVpgAndMgmAndVpIDsJson;
                  var productsStore = Ext.getCmp('UnMatchVendorProductsGrid').store;
                  var masterGroupMappingIDs = [];
                  var listOfVendorProducts = productsStore.data.items;
                  var url = "";
                  var params;
                  if (records.length > 1) {
                    url = Concentrator.route('UnMatchMultipleVendorProductGroup', 'MasterGroupMapping');
                    var productgroupvendors = store.collect('ProductGroupVendorID');
                    var unmatchData = [];
                    for (var i = 0; i < productgroupvendors.length; i++) {
                      var checkedItemsPerProductGroupVendor = { VendorProductGroupID: productgroupvendors[i], ListOfVendorProductIDs: [] };
                      var listOfVendorProductIDsByVendorProductGroup = productsStore.query('ProductGroupVendorID', productgroupvendors[i]);
                      Ext.each(listOfVendorProductIDsByVendorProductGroup.items, function (r) {
                        if (r.get('Check') == true) {
                          checkedItemsPerProductGroupVendor.ListOfVendorProductIDs.push(r.get('ProductID'));
                        }
                      });
                      unmatchData.push(checkedItemsPerProductGroupVendor);
                    }
                    masterGroupMappingIDs = [];
                    Ext.each(data, function (r) {
                      masterGroupMappingIDs.push(r.MasterGroupMappingID);

                    });
                    params = { ListOfIDsJson: JSON.stringify(unmatchData), MasterGroupMappingIDs: JSON.stringify(masterGroupMappingIDs) };

                  } else {
                    masterGroupMappingIDs = listOfVpgAndMgmAndVpIDs.ListOfMasterGroupMappingIDs;
                    url = Concentrator.route('UnMatchVendorProductGroup', 'MasterGroupMapping');
                    Ext.each(listOfVendorProducts, function (r) {
                      if (r.get('Check') == true) {
                        listOfVpgAndMgmAndVpIDs.ListOfVendorProductIDs.push(r.get('ProductID'));
                      }
                    });
                    params = { ListOfIDsJson: JSON.stringify(listOfVpgAndMgmAndVpIDs) };
                  }

                  Diract.request({
                    url: url,
                    waitMsg: 'UnMatching Vendor product Group(s) and Vendor Products',
                    params: params,
                    callback: function () {
                      var panels = {
                        allTabPanels: true,
                        treePanel: true,
                        MasterGroupMappings: masterGroupMappingIDs
                      };
                      Concentrator.MasterGroupMappingFunctions.refreshAction(panels);

                      productWindow.close();
                    }
                  });
                }
              }, "->", {
                text: 'Select All',
                iconCls: 'checked',
                handler: function () {
                  productGrid.getStore().each(function (rec) { rec.set('Check', true) });
                }
              }, {
                text: 'Deselect All',
                iconCls: 'unchecked',
                handler: function () {
                  productGrid.getStore().each(function (rec) { rec.set('Check', false) });
                }
              }, {
                text: 'Show Product Details',
                iconCls: 'view',
                handler: function () {
                  productGridSelection = productGrid.getSelectionModel();
                  if (productGridSelection.getCount() > 0) {
                    record = productGridSelection.getSelected();
                    Concentrator.MasterGroupMappingFunctions.getEditProductWindow(record.get('ProductID'));
                  } else {
                    Ext.Msg.alert('', 'Please select one vendor product to show details"');
                  }
                }
              }, {
                text: 'Exit without save matches',
                iconCls: 'exit',
                handler: function () {
                  productWindow.close();
                }
              }]
            });
            var productWindow = new Ext.Window({
              modal: true,
              title: title,
              items: productGrid,
              layout: 'fit',
              width: 800,
              height: 500,
              renderTo: 'MasterGroupMappingCenterPanel'
            });
            productWindow.show();
          }
        }, "->", {
          text: 'Select All',
          iconCls: 'checked',
          handler: function () {
            var unMatchGrid = Ext.getCmp('UnMatchProductGroupVendors');
            unMatchGrid.store.each(function (rec) { rec.set('Check', true) });
          }
        }, {
          text: 'Deselect All',
          iconCls: 'unchecked',
          handler: function () {
            var unMatchGrid = Ext.getCmp('UnMatchProductGroupVendors');
            unMatchGrid.store.each(function (rec) { rec.set('Check', false) });
          }
        }, {
          text: 'Exit without save unmatches',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      var window = new Ext.Window({
        modal: false,
        title: title,
        items: grid,
        layout: 'fit',
        width: 800,
        height: 500,
        renderTo: 'masterpanel'
      });
      window.show();

      grid.getStore().addListener('load', function () {
        if (grid.getStore().getCount() < 1) {
          Ext.Msg.show({
            title: 'Matched Vendor Product Group',
            msg: 'Vendor Product Group"' + vendorProductGroupName + '" is not matched.',
            buttons: Ext.Msg.OK,
            fn: function (result) {
              window.close();
            },
            animEl: 'elId',
            icon: Ext.MessageBox.QUESTION
          });
        }
      });

    },
    getUnMatchVendorProductWindow: function (recs, onSuccess, connectorID) {

      var records = recs.length ? recs : [recs];

      var title = "";
      var url = "";
      var unMatchUrl = "";
      var params = {};

      if (records.length > 1) {
        url = Concentrator.route("GetListOfMultipleMatchedMasterGroupMappingsAndVendorProducts", "MasterGroupMapping");
        title = 'UnMatch ' + records.length + ' products';
        unMatchUrl = "UnMatchMultipleVendorProductByMasterGroupMapping";
        var data = [];
        for (var idx = 0; idx < records.length; idx++) {
          var record = records[idx];
          data.push({ productID: record.get('ConcentratorNumber'), MasterGroupMappingID: record.get('MasterGroupMappingID') });
        }
        params = { data: JSON.stringify(data) };
      } else {
        var record = records[0];
        if (record.get && record.get('ProductControl') == 'NotMatched') {
          Ext.Msg.alert('Product', 'Product is not matched.');
          return;
        }
        params = {
          ProductID: record.get('ConcentratorNumber'),
          GridMasterGroupMappingID: record.get('MasterGroupMappingID'),
          ConnectorID: connectorID
        };
        unMatchUrl = "UnMatchVendorProductAndMasterGroupMapping";
        url = Concentrator.route("GetListOfMatchedMasterGroupMappingAndVendorProduct", "MasterGroupMapping");
        title = 'UnMatch Vendor Product "' + record.get('ConcentratorNumber') + ', ' + record.get('ProductName') + '"';
      }


      var unMatchVendorProductGrid = new Diract.ui.Grid({
        pluralObjectName: 'Master Group Mapping Nodes',
        singularObjectName: 'Master Group Mapping Node',
        primaryKey: ['MasterGroupMappingID', 'ConcentratorID'],
        forceFit: true,
        id: 'UnMatchVendorProductGrid',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: params,
        groupField: records.length > 1 ? 'MasterGroupMappingPad' : "",
        url: url,
        structure: [{
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'Check',
          header: 'Check To UnMatch',
          type: 'boolean',
          editable: true,
          width: 50
        }, {
          dataIndex: 'MasterGroupMappingPad',
          header: 'MasterGroupMapping path',
          type: 'string',
          width: 450
        }, {
          dataIndex: 'ConcentratorID',
          header: 'Concentrator #',
          type: 'int',
          hidden: records.length > 1 ? false : true,
          width: 200
        }],
        customButtons: [{
          text: records.length > 1 ? 'UnMatch Vendor Products' : 'UnMatch Vendor Product',
          iconCls: 'save',
          alignRight: false,
          handler: function () {

            var ListToUnMatch = [];
            var ListOfMasterGroupMappingIDs;

            if (records.length > 1) {
              Ext.each(unMatchVendorProductWindow.records, function (record) {
                var ListOfMasterGroupMappingIDs = { ProductID: 0, MasterGroupMappingIDs: [] };
                ListOfMasterGroupMappingIDs.ProductID = record.get('ConcentratorNumber');

                listOfMasterGroupMappings = Ext.getCmp('UnMatchVendorProductGrid').store.data.items;
                Ext.each(listOfMasterGroupMappings, function (r) {
                  if (r.data.Check) {
                    ListOfMasterGroupMappingIDs.MasterGroupMappingIDs.push(r.data.MasterGroupMappingID);
                  }
                });
                ListToUnMatch.push(ListOfMasterGroupMappingIDs);
              });
              var ListOfMasterGroupMappingIDs = JSON.stringify(ListToUnMatch);
            } else {
              var ListOfMasterGroupMappingIDs = { ProductID: 0, MasterGroupMappingIDs: [] };
              ListOfMasterGroupMappingIDs.ProductID = record.get('ConcentratorNumber');

              listOfMasterGroupMappings = Ext.getCmp('UnMatchVendorProductGrid').store.data.items;
              Ext.each(listOfMasterGroupMappings, function (r) {
                if (r.data.Check) {
                  ListOfMasterGroupMappingIDs.MasterGroupMappingIDs.push(r.data.MasterGroupMappingID);
                }
              });
              var ListOfMasterGroupMappingIDs = JSON.stringify(ListOfMasterGroupMappingIDs);
            }


            Diract.request({
              url: Concentrator.route(unMatchUrl, 'MasterGroupMapping'),
              params: {
                ListOfMasterGroupMappingIDsJson: ListOfMasterGroupMappingIDs
              },
              callback: function () {
                unMatchVendorProductWindow.close();
                var panels = {
                  vendorProductsTabPanel: true,
                  matchedVendorProductsTabPanel: true,
                  treePanel: true
                };
                Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                if (onSuccess) onSuccess();
              }
            });
          }
        }, "->", {
          text: 'Select All',
          iconCls: 'checked',
          handler: function () {
            var unMatchGrid = Ext.getCmp('UnMatchMoreVendorProductGrid');
            unMatchGrid.store.each(function (rec) { rec.set('Check', true) });
          }
        }, {
          text: 'Deselect All',
          iconCls: 'unchecked',
          handler: function () {
            var unMatchGrid = Ext.getCmp('UnMatchMoreVendorProductGrid');
            unMatchGrid.store.each(function (rec) { rec.set('Check', false) });
          }
        }, {
          text: 'Exit without save unmatches',
          iconCls: 'exit',
          handler: function () {
            unMatchVendorProductWindow.close();
          }
        }]
      });
      var unMatchVendorProductWindow = new Ext.Window({
        modal: false,
        records: records,
        title: title,
        items: unMatchVendorProductGrid,
        layout: 'fit',
        width: 800,
        height: 500,
        renderTo: 'masterpanel'
      });
      unMatchVendorProductWindow.show();

    },
    getVendorProductGroupName: function (record) {
      var vendorProductGroupName = '';

      if (record.json.VendorName) {
        vendorProductGroupName += record.json.VendorName;
      }

      if (record.get('VendorProductGroupName')) {
        if (vendorProductGroupName) {
          vendorProductGroupName += ', ';
        }
        vendorProductGroupName += record.get('VendorProductGroupName');
      }

      if (record.get('BrandCode')) {
        if (vendorProductGroupName) {
          vendorProductGroupName += ', ';
        }
        vendorProductGroupName += record.get('BrandCode');
      }

      var productCode = '';
      if (record.get('VendorProductGroupCode1')) {
        productCode = record.get('VendorProductGroupCode1');
      } else if (record.get('VendorProductGroupCode2')) {
        productCode = record.get('VendorProductGroupCode2');
      } else if (record.get('VendorProductGroupCode3')) {
        productCode = record.get('VendorProductGroupCode3');
      } else if (record.get('VendorProductGroupCode4')) {
        productCode = record.get('VendorProductGroupCode4');
      } else if (record.get('VendorProductGroupCode5')) {
        productCode = record.get('VendorProductGroupCode5');
      } else if (record.get('VendorProductGroupCode6')) {
        productCode = record.get('VendorProductGroupCode6');
      } else if (record.get('VendorProductGroupCode7')) {
        productCode = record.get('VendorProductGroupCode7');
      } else if (record.get('VendorProductGroupCode8')) {
        productCode = record.get('VendorProductGroupCode8');
      } else if (record.get('VendorProductGroupCode9')) {
        productCode = record.get('VendorProductGroupCode9');
      } else if (record.get('VendorProductGroupCode10')) {
        productCode = record.get('VendorProductGroupCode10');
      }

      if (productCode) {
        if (vendorProductGroupName) {
          vendorProductGroupName += ', ';
        }
        vendorProductGroupName += productCode;
      }

      return vendorProductGroupName;
    },
    getPossibleMatchesTabItems: function () {

      var store = Ext.getCmp('PossibleMatchesGrid').getStore();
      store.addListener('load', function () {

        var tabs = Ext.getCmp('PossibleMatchesTabs');
        tabs.removeAll();

        Ext.QuickTips.init();
        Ext.apply(Ext.QuickTips.getQuickTip(), {
          maxWidth: 10000
        });
        Ext.each(store.reader.jsonData.results, function (gridRecord) {
          // create the data store
          var gridStore = new Ext.data.JsonStore({
            // store configs
            autoDestroy: true,
            idProperty: 'AttributeName',
            fields: ['AttributeName', 'AttributeValue']
          });

          // manually load local data
          gridStore.loadData(gridRecord.Attributes);

          var listView = new Ext.list.ListView({
            store: gridStore,
            region: 'south',
            height: 250,
            width: 400,
            reserveScrollOffset: true,
            columns: [{
              header: 'Attribute Name',
              dataIndex: 'AttributeName'
            }, {
              header: 'Attribute Value',
              dataIndex: 'AttributeValue'
            }]
          });

          var title = (gridRecord.Title != null) ? gridRecord.Title : "";
          var description = (gridRecord.Description != null) ? gridRecord.Description : "";
          var productDescription = '<b>Title</b>: ' + title + ' <br>' + '<b>Short Description</b>: ' + description;
          var id = Ext.id();
          var tabPanel = new Ext.Panel({
            layout: 'border',
            width: '100%',
            height: 500,
            items: [{
              region: 'north',
              width: 200,
              height: 200,
              html: '<img id=' + id + ' src="' + gridRecord.Image + '"  height="200" width="200" />',
              padding: 3,
              listeners: {
                afterrender: function (c) {
                  var html = '<img class="ImgCenterBig" src="' + gridRecord.Image + '" />';
                  //c.update(html);
                  var divObj = Ext.get(id);
                  Ext.QuickTips.register({
                    target: divObj,
                    baseCls: 'noBackGround',
                    text: html
                  });
                }
              }
            }, {
              region: 'center',
              //width: 200,
              height: 50,
              html: productDescription,
              padding: 3
            },
						listView
            ]
          });
          matchedItem = {
            title: gridRecord.ProductID,
            items: tabPanel
          };
          tabs.add(matchedItem);
        });
        tabs.setActiveTab(0);
      });
    },
    getPossibleMatchesGrid: function (productID) {
      var functions = {
        getDifferencesBetweenAttributesWindow: function (productID) {
          Diract.request({
            url: Concentrator.route('GetAllAttributesForAllMatchingProducts', 'MasterGroupMapping'),
            params: { productID: productID },
            success: (function (response) {

              if (response.results.length > 0) {

                var structure = [{
                  dataIndex: 'AttributeName',
                  type: 'string',
                  header: 'Attribute Name'
                },
								{
								  dataIndex: 'AttributeID',
								  type: 'int'
								}];

                Ext.each(response.results, function (result) {
                  var struc = {
                    dataIndex: result.ProductID,
                    header: result.VendorItemNumber + ' (' + result.ProductID + ')' + result.Primary + (result.VendorName != "" ? (' - ' + result.VendorName) : ''),
                    type: 'string',
                    filterable: false,
                    editor: { xtype: 'textfield', allowBlank: true }

                  }
                  structure.push(struc);
                });

                var attributeGrid = new Diract.ui.Grid({
                  ddGroup: 'attibuteGridDD',
                  enableDragDrop: true,
                  params: { productIDs: response.productIDs, productID: productID },
                  region: 'center',
                  url: Concentrator.route("GetAllAttributesInfoForAllMatchingProducts", "MasterGroupMapping"),
                  permissions: {
                    list: 'DefaultMasterGroupMapping',
                    update: 'DefaultMasterGroupMapping'
                  },
                  structure: structure,
                  customButtons: [
										{
										  text: 'Save Changes',
										  iconCls: 'save',
										  id: 'attributeGridSaveButton',
										  handler: function () {

										    var edits = attributeGrid.store.getModifiedRecords();
										    var countCellsUpdated = 0;
										    var totalCellsUpdatedByUser = Concentrator.MasterGroupMappingFunctions.countChangedFields(edits);

										    for (var i = 0, len = edits.length; i < len; i++) {

										      var changedFields = Concentrator.MasterGroupMappingFunctions.getChangedAndBooleanFields(edits[i]);
										      var attributeID = edits[i].data.AttributeID;

										      for (var propertyName in changedFields) {

										        var propName = propertyName;
										        if (propName) {
										          countCellsUpdated++;

										          var productID = propName;
										          var attributeValue = changedFields[propName];

										          var paramsToSend = {
										            productID: productID,
										            attributeID: attributeID,
										            attributeValue: attributeValue
										          }

										          Diract.request({
										            url: Concentrator.route("UpdateAttributesForMatchingProducts", "MasterGroupMapping"),
										            params: paramsToSend,
										            flushAt: totalCellsUpdatedByUser,
										            onFlush: function () {

										              if (countCellsUpdated == totalCellsUpdatedByUser) {
										                attributeGrid.store.commitChanges();
										                attributeGrid.store.reload();
										              }
										            }
										          });
										        }
										      }
										    }
										  }
										},
										{
										  text: 'Discard changes',
										  iconCls: "cancel",
										  handler: function () {
										    var edits = attributeGrid.store.getModifiedRecords();
										    Ext.MessageBox.confirm(Diract.text.confirmTitle, Diract.text.discardBtnConfirmation, function (btn) {
										      if (btn == "yes") {
										        attributeGrid.store.rejectChanges();
										      }
										    }, this);
										  }
										}
                  ],

                  listeners: {
                    render: function () {

                      var ddrow = new Ext.dd.DropTarget(attributeGrid.container, {
                        ddGroup: 'attibuteGridDD',
                        copy: false,
                        notifyDrop: function (dd, e, data) {

                          var attributeGridStore = attributeGrid.store;

                          var selectionModel = attributeGrid.getSelectionModel();
                          var selectedRows = selectionModel.getSelections();

                          if (dd.getDragData(e)) {

                            var dragIndex = dd.getDragData(e).rowIndex;
                            if (typeof (dragIndex) != "undefined") {

                              for (i = 0; i < selectedRows.length; i++) {
                                attributeGridStore.remove(attributeGridStore.getById(selectedRows[i].id));
                              }

                              attributeGridStore.insert(dragIndex, data.selections);
                              selectionModel.clearSelections();
                            }
                          }

                        }
                      })
                    }
                  }
                });
                var attributeWindow = new Ext.Window({
                  title: 'Attribute information for all matched products',
                  width: 1000,
                  height: 600,
                  layout: 'fit',
                  autoScroll: true,
                  modal: true,
                  items: [attributeGrid],
                  tbar: ["->", {
                    text: 'Exit',
                    iconCls: 'exit',
                    handler: function () {
                      attributeWindow.close();
                    }
                  }]
                });
                attributeWindow.show();
              } else {
                Ext.Msg.alert('No Attributes!', 'No Attributes for this product selection!');
              }
            }).createDelegate(this)
          });
        }
      };
      var productmatchesGridMenu = new Ext.menu.Menu({
        items: [{
          id: 'saveChanges',
          text: 'Save Changes',
          iconCls: 'menuItem-Approve'
        }, {
          id: 'addProductToMatch',
          text: 'Add Product To Match',
          iconCls: 'add'
        }, {
          id: 'viewAttributes',
          text: 'Differences Between Attributes',
          iconCls: 'view'
        }],
        listeners: {
          itemclick: function (item) {
            if (item.id) {
              switch (item.id) {
                case 'saveChanges': {
                  var store = Ext.getCmp('PossibleMatchesGrid').getStore();
                  Ext.each(store.getModifiedRecords(), function (r) {
                    Diract.request({
                      url: Concentrator.route('UpdateMatchProductWizard', 'MasterGroupMapping'),
                      params: {
                        ProductID: r.data.ProductID,
                        IsMatch: r.data.Check
                      },
                      callback: function () {
                        store.commitChanges();
                      }
                    });
                  });
                }
                  break;
                case 'addProductToMatch':
                  var thisGrid = Ext.getCmp('PossibleMatchesGrid');
                  var record = thisGrid.getSelectionModel().getSelected();
                  Concentrator.MasterGroupMappingFunctions.getProductWindow(record.get('ProductID'));
                  break;
                case 'viewAttributes':
                  var thisGrid = Ext.getCmp('PossibleMatchesGrid');
                  var record = thisGrid.getSelectionModel().getSelected();
                  functions.getDifferencesBetweenAttributesWindow(record.get('ProductID'));
                  break;
              }
            }
          }
        }
      });
      var possibleMatchesGrid = new Diract.ui.Grid({
        pluralObjectName: 'Possible Matches',
        singularObjectName: 'Possible Match',
        primaryKey: ['ProductID', 'VendorName'],
        forceFit: true,
        layout: 'fit',
        id: 'PossibleMatchesGrid',
        region: 'center',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          ProductID: productID
        },
        url: Concentrator.route("GetListOfMatchedProducts", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'Check',
          type: 'boolean',
          header: '#',
          editable: true,
          width: 20
        }, {
          dataIndex: 'MatchStatus',
          header: 'Match Status',
          type: 'string',
          renderer: function (val, metadata, record) {
            switch (val) {
              case "New":
                break;
              case "Declined":
                return '<div class="rowCancelIcon centerIcon"/>';
                break;
              case "Accepted":
                return '<div class="rowAcceptIcon centerIcon"/>';
                break;
            }
          }
        }, {
          dataIndex: 'ProductID',
          type: 'int',
          header: 'Product ID'
        }, {
          dataIndex: 'VendorItemNumber',
          header: 'Vendor Item Number'
        }, {
          dataIndex: 'Brand',
          header: 'Brand'
        }, {
          dataIndex: 'Barcode',
          header: 'Barcode'
        }, {
          dataIndex: 'VendorName',
          header: 'Vendor'
        },
				{
				  dataIndex: 'VendorDescription',
				  header: 'Vendor Description'
				},
				{
				  dataIndex: 'MatchPercentage',
				  type: 'string',
				  header: 'Match Percentage'
				},
				{
				  dataIndex: 'Primary',
				  type: 'boolean',
				  header: 'Primary'
				}],
        rowActions: [
          {
            text: 'Create new Product Match with selected products',
            iconCls: 'add',
            alwaysEnabled: true,
            handler: function (record) {
              if (record.length > 0) {
                var productIds = '';
                for (var i = 0; i < record.length; i++) {
                  productIds += record[i].get('ProductID') + ',';
                }
                productIds = productIds.substring(0, productIds.length - 1)

                Diract.request({
                  url: Concentrator.route('CreateNewProductMatch', 'ProductMatch'),
                  params: { productIDs: productIds },
                  success: (function (options, success, repsonse) {
                    possibleMatchesGrid.store.reload();
                  }).createDelegate(productgroupthis),
                  failure: (function (response) {
                  }).createDelegate(this)
                });
              } else if (record && record.get('ProductID')) {
                Diract.request({
                  url: Concentrator.route('CreateNewProductMatch', 'ProductMatch'),
                  params: { productIDs: record.get('ProductID') },
                  success: (function (options, success, repsonse) {
                    possibleMatchesGrid.store.reload();
                  }).createDelegate(this),
                  failure: (function (response) {
                  }).createDelegate(this)
                });
              }
            }
          },
					{
					  text: 'Set Primary',
					  iconCls: 'wrench',
					  handler: function (record) {

					    Diract.request({
					      url: Concentrator.route('SetProductMatchAsPrimary', 'MasterGroupMapping'),
					      params: {
					        productID: record.get('ProductID')
					      },
					      callback: function () {
					        possibleMatchesGrid.store.reload();

					        var compareAttPanel = Ext.getCmp('compareAttributesPanel');
					        var attributeGrid = compareAttPanel.items.items[0];
					        if (attributeGrid) {
					          compareAttPanel.removeAll();
					          Concentrator.MasterGroupMappingFunctions.initCompareAttributesGrid(productID);
					        }
					      }
					    });
					  }
					}

        ],
        listeners: {
          rowclick: function (t, rowIndex, e) {
            Ext.getCmp('PossibleMatchesTabs').setActiveTab(rowIndex);
          },
          rowcontextmenu: function (thisGrid, index, event) {
            event.stopEvent();
            productmatchesGridMenu.showAt(event.xy);
          },
          beforedestroy: function () {
            if (productmatchesGridMenu) {
              productmatchesGridMenu.destroy();
              delete productmatchesGridMenu;
            }
          }
        }
      });
      return possibleMatchesGrid;
    },
    getPossibleMatchesTabs: function () {
      var possibleMatchesTabs = new Ext.TabPanel({
        region: 'east',
        id: 'PossibleMatchesTabs',
        hideLabel: true,
        deferredRender: false,
        defaults: { autoScroll: true },
        width: 250,
        enableTabScroll: true,
        items: this.getPossibleMatchesTabItems(),
        listeners: {
          'tabchange': function (tabPanel, tab) {
            var activeTab = tabPanel.getActiveTab();
            var grid = Ext.getCmp('PossibleMatchesGrid');

            if (activeTab) {
              var activeTabIndex = tabPanel.items.findIndex('id', activeTab.id);
              grid.getSelectionModel().selectRow(activeTabIndex);
            } else {
              grid.getSelectionModel().selectRow(0);
              tabPanel.setActiveTab(0);
            }
          }
        }
      });
      return possibleMatchesTabs;
    },
    // Possible Matches Window
    possibleMatches: function (record) {
      var PossibleMatchesGrid = this.getPossibleMatchesGrid(record.get('ConcentratorNumber'));
      var PossibleMatchesTabs = this.getPossibleMatchesTabs();
      var PossibleMatchesWindow = new Ext.Window({
        modal: true,
        title: 'Wizard to Matching Products',
        items: [PossibleMatchesGrid, PossibleMatchesTabs],
        plain: true,
        //layout: 'border',
        width: 1068,
        resizable: false,
        height: 591,
        layout: 'fit',
        renderTo: 'masterpanel',
        //renderTo: 'MasterGroupMappingCenterPanel',
        tbar: [{
          text: 'Add Product To Match',
          iconCls: 'add',
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.getProductWindow(record.get('ConcentratorNumber'));
            PossibleMatchesWindow.close();
          }
        }, {
          text: 'Save Matches',
          iconCls: 'save',
          handler: function () {
            var store = Ext.getCmp('PossibleMatchesGrid').getStore();
            this.setDisabled(true);

            Ext.each(store.getModifiedRecords(), function (r) {
              Diract.request({
                url: Concentrator.route('UpdateMatchProductWizard', 'MasterGroupMapping'),
                params: {
                  ProductID: r.data.ProductID,
                  IsMatch: r.data.Check
                },
                callback: function () {
                  PossibleMatchesWindow.close();
                }
              });
            });

            PossibleMatchesGrid.getStore().reload();
            this.setDisabled(false);
          }
        }, "->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            PossibleMatchesWindow.close();
          }
        }],
        listener: {
          'afterlayout': function (t, layout) {
            Ext.getCmp('PossibleMatchesTabs').doLayout();
          }
        }
      });
      PossibleMatchesWindow.show();
    },
    // Attribute Management Window
    addAttribute: function (masterGroupMappingID, masterGroupMappingName) {
      var window = new Diract.ui.FormWindow({
        url: Concentrator.route('AddAttributeToMasterGroupMapping', 'MasterGroupMapping'),
        title: 'Add Attribute to Master Group Mapping "' + masterGroupMappingName + '"',
        buttonText: 'Save',
        labelWidth: 200,
        cancelButton: true,
        items: [{
          name: 'AttributeID',
          xtype: 'attribute',
          fieldLabel: 'Attribute',
          allowBlank: false
        }, {
          name: 'MasterGroupMappingID',
          xtype: 'hidden',
          value: masterGroupMappingID
        }],
        layout: 'fit',
        height: 200,
        width: 400,
        success: (function () {
          Ext.getCmp('AssignedAttributesGrid').getStore().reload();
          window.close();
        })
      });
      window.show();
    },
    deleteAttribute: function (record) {
      Ext.Msg.confirm("Delete " + record.data.AttributeName, "Are you sure you want to delete this " + record.data.AttributeName + " ?",
					(function (button) {
					  if (button == "yes") {
					    Diract.request({
					      url: Concentrator.route('DeleteAttributeFromMasterGroupMapping', 'MasterGroupMapping'),
					      params: {
					        AttributeID: record.data.AttributeID,
					        MasterGroupMappingID: record.data.MasterGroupMappingID
					      },
					      success: function () {
					        Ext.getCmp('AssignedAttributesGrid').getStore().reload();
					      }
					    });
					  }
					}));
    },
    copyAttributes: function (masterGroupMappingID, masterGroupMappingName) {
      var CopyAttributesGrid = new Diract.ui.Grid({
        pluralObjectName: 'MasterGroupMappings',
        singularObjectName: 'MasterGroupMapping',
        primaryKey: 'MasterGroupMappingID',
        id: 'CopyAttributesGrid',
        forceFit: true,
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          MasterGroupMappingID: masterGroupMappingID
        },
        url: Concentrator.route("GetListOfAllMasterGroupMappingNames", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'Check',
          header: 'Check To Copy',
          type: 'boolean',
          editable: true,
          width: 50
        }, {
          dataIndex: 'MasterGroupMappingPad',
          header: 'MasterGroupMapping path',
          type: 'string',
          width: 650
        }],
        customButtons: [{
          iconCls: 'save',
          text: 'Copy to selected Master Group Mapping',
          handler: function (record) {
            var ListOfMasterGroupMappingIDs = { CopyToMasterGroupMappingIDs: [], CopyFromMasterGroupMappingID: masterGroupMappingID };

            store = Ext.getCmp('CopyAttributesGrid').getStore();
            Ext.each(store.getModifiedRecords(), function (r) {
              if (r.data.Check) {
                ListOfMasterGroupMappingIDs.CopyToMasterGroupMappingIDs.push(r.data.MasterGroupMappingID);
              }
            });
            var ListOfMasterGroupMappingIDsJson = JSON.stringify(ListOfMasterGroupMappingIDs);
            Diract.request({
              url: Concentrator.route('CopyAttributeFromMasterGroupMapping', 'MasterGroupMapping'),
              params: {
                ListOfMasterGroupMappingIDsJson: ListOfMasterGroupMappingIDsJson
              },
              callback: function () {
                CopyAttributesWindow.close();
              }
            });

          }
        }, {
          text: 'Exit',
          iconCls: 'exit',
          alignRight: true,
          handler: function () {
            CopyAttributesWindow.close();
          }
        }]
      });
      var CopyAttributesWindow = new Ext.Window({
        title: 'Copy Attributes from Master Group Mapping "' + masterGroupMappingName + '"',
        width: 800,
        height: 500,
        modal: true,
        layout: 'fit',
        items: [CopyAttributesGrid]
      });
      CopyAttributesWindow.show();
    },
    attributesWizard: function (masterGroupMappingID, masterGroupMappingName) {

      var productsGrid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        id: 'AttributeWizardProductsGrid',
        primaryKey: ['AttributeValueID', 'ConcentratorNumber', 'AttributeID'],
        region: 'center',
        autoLoadStore: false,
        refreshAfterSave: true,
        url: Concentrator.route("GetListOfMatchedProductsPerAttribtue", "MasterGroupMapping"),
        updateUrl: Concentrator.route("UpdateMatchedProductsPerAttribtue", "MasterGroupMapping"),
        forceFit: true,
        params: {
          MasterGroupMappingID: masterGroupMappingID,
          AttributeID: 0
        },
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [
									{
									  dataIndex: 'AttributeValueID',
									  type: 'int'
									},
									{
									  dataIndex: 'AttributeID',
									  type: 'int'
									},
									{
									  dataIndex: 'ConcentratorNumber',
									  header: 'Concentrator #',
									  type: 'int'
									},
									{
									  dataIndex: 'ProductName',
									  header: 'Product Name',
									  type: 'string'
									},
									{
									  dataIndex: 'Brand',
									  header: 'Brand',
									  type: 'string'
									},
									{
									  dataIndex: 'VendorItemNumber',
									  header: 'Vendor Item Number',
									  type: 'string'
									},
                  {
                    dataIndex: 'LanguageCode',
                    header: 'Language',
                    type: 'string'
                  },
									{
									  dataIndex: 'AttributeValue',
									  header: 'Attribute Value',
									  editor: {
									    xtype: 'textfield',
									    allowBlank: false
									  }
									}
        ],
        rowActions: [
									{
									  iconCls: 'search',
									  text: 'View Associated Vendor Assortments',
									  handler: function (record) {
									    Concentrator.MasterGroupMappingFunctions.viewVendorAssortments(record);
									  }
									}
        ],
        customButtons: ['->', {
          iconCls: 'add',
          text: 'Fill Default Value',
          align: 'right',
          handler: function () {
            var functions = {
              fillupRows: function () {
                if (txtValue.getValue() && txtValue.getValue().replace(/^\s\s*/, '').replace(/\s\s*$/, '') != '') {
                  if (productsGrid.getStore().getCount() > 0) {
                    Ext.each(productsGrid.getStore().getRange(), function (record) {
                      if (!record.get('AttributeValue') || record.get('AttributeValue').replace(/^\s\s*/, '').replace(/\s\s*$/, '') == '') {
                        record.set('AttributeValue', txtValue.getValue());
                      }
                    });
                  }
                }
              },
              getAttributeDefaultValue: function () {
                if (attributesGrid.getSelectionModel().getSelected().get('DefaultValue')) {
                  return attributesGrid.getSelectionModel().getSelected().get('DefaultValue');
                } else {
                  return '';
                }
              }
            };
            var buttons = {
              createCancelButton: function () {
                return new Ext.Toolbar.Button({
                  text: 'Cancel',
                  iconCls: '',
                  handler: function () {
                    window.close();
                  }
                });
              },
              createFillButton: function () {
                return new Ext.Toolbar.Button({
                  text: 'Fill Rows',
                  iconCls: '',
                  handler: function () {
                    if (txtValue.getValue() && txtValue.getValue().replace(/^\s\s*/, '').replace(/\s\s*$/, '') != '') {
                      functions.fillupRows();
                      window.close();
                    }
                  }
                });
              },
              getButtons: function () {
                var listOfButtons = [];
                listOfButtons.push(btnFill);
                listOfButtons.push(btnCancel);
                return listOfButtons;
              }
            };
            var formFields = {
              getTextFieldValue: function () {
                return new Ext.form.TextField({
                  name: 'value',
                  allowBlank: false,
                  width: 100,
                  fieldLabel: 'Value'
                });
              }
            };
            var forms = {
              getValueForm: function () {
                return new Ext.FormPanel({
                  frame: true,
                  //width: 685,
                  items: [
                    {
                      layout: 'column',
                      items: [
                      {
                        //columnWidth: .71,
                        layout: 'form',
                        //labelAlign: 'right',
                        //labelWidth: 200,
                        items: [
                          txtValue
                        ]
                      }]
                    }
                  ]
                });
              }
            };

            var btnCancel = buttons.createCancelButton();
            var btnFill = buttons.createFillButton();
            var txtValue = formFields.getTextFieldValue();

            if (functions.getAttributeDefaultValue()) {
              txtValue.setValue(functions.getAttributeDefaultValue());
            }
            var window = new Ext.Window({
              modal: true,
              title: 'Fill Value For All Rows',
              plain: true,
              layout: 'fit',
              resizable: false,
              width: 250,
              height: 150,
              items: forms.getValueForm(),
              buttons: buttons.getButtons()
            });
            window.show();
          }
        }]
      });
      var attributesGrid = new Diract.ui.Grid({
        pluralObjectName: 'Attributes',
        singularObjectName: 'Attribute',
        primaryKey: 'AttributeID',
        id: 'AttributeWizardAttributesGrid',
        title: 'Attributes in Master Group Mapping "' + masterGroupMappingName + '"',
        forceFit: true,
        disabled: true,
        width: 400,
        region: 'west',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          MasterGroupMappingID: masterGroupMappingID
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
          },
          {
            dataIndex: 'DefaultValue'
          }
        ],
        hideBottomBar: true
      });
      var attributesWizardWindow = new Ext.Window({
        modal: true,
        title: 'Product Attribute Wizard',
        items: [attributesGrid, productsGrid],
        plain: true,
        layout: 'border',
        width: 1068,
        resizable: false,
        height: 591,
        tbar: [
									"->",
									{
									  text: 'Exit',
									  iconCls: 'exit',
									  handler: function () {
									    attributesWizardWindow.close();
									  }
									} //Cancel button 
        ],
        buttons: [
									{
									  text: 'Back',
									  iconCls: '',
									  handler: function () {
									    Concentrator.MasterGroupMappingFunctions.attributesWizardAttributeGrid('back');
									  }
									}, //Back button 
									{
									  text: 'Next',
									  iconCls: '',
									  handler: function () {
									    Concentrator.MasterGroupMappingFunctions.attributesWizardAttributeGrid('next');
									  }
									} //Next button 
        ]
      });
      attributesWizardWindow.show();
      var store = attributesGrid.getStore();
      store.addListener('load', function () {
        Concentrator.MasterGroupMappingFunctions.attributesWizardAttributeGrid('next');
      });
    },
    attributesWizardAttributeGrid: function (move) {
      var attributeGrid = Ext.getCmp('AttributeWizardAttributesGrid').getSelectionModel();
      var productGrid = Ext.getCmp('AttributeWizardProductsGrid');
      if (productGrid.store.getModifiedRecords().length > 0) {
        Ext.Msg.confirm("The changes are not saved!", "Do you want to go unsaved to the next attribute?",
									(function (button) {
									  if (button == "yes") {
									    if (move == 'next') {
									      if (attributeGrid.selectNext()) {
									      } else {
									        attributeGrid.selectFirstRow();
									      }
									    }
									    if (move == 'back') {
									      if (attributeGrid.selectPrevious()) {
									      } else {
									        attributeGrid.selectLastRow();
									      }
									      ;
									    }
									    productGrid.store.reload({
									      params: {
									        MasterGroupMappingID: attributeGrid.getSelected().get('MasterGroupMappingID'),
									        AttributeID: attributeGrid.getSelected().get('AttributeID')
									      }
									    });
									  }
									}));
      } else {
        if (move == 'next') {
          if (attributeGrid.selectNext()) {
          } else {
            attributeGrid.selectFirstRow();
          }
        }
        if (move == 'back') {
          if (attributeGrid.selectPrevious()) {
          } else {
            attributeGrid.selectLastRow();
          }
          ;
        }

        productGrid.getStore().setBaseParam('MasterGroupMappingID', attributeGrid.getSelected().get('MasterGroupMappingID'));
        productGrid.getStore().setBaseParam('AttributeID', attributeGrid.getSelected().get('AttributeID'));
        productGrid.getStore().reload();

      }
    },
    productControleManagementWindow: function () {
      var ProductControleManagementGrid = new Diract.ui.Grid({
        pluralObjectName: 'ProductControles',
        singularObjectName: 'ProductControle',
        primaryKey: 'ProductControlID',
        forceFit: true,
        id: 'productControleManagementGrid',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        url: Concentrator.route("GetListOfProductControle", "MasterGroupMapping"),
        updateUrl: Concentrator.route("UpdateProductControle", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'ProductControlID',
          type: 'int'
        }, {
          dataIndex: 'ProductControlName',
          header: 'ProductControl Name'
        }, {
          dataIndex: 'IsActive',
          type: 'boolean',
          header: 'Active',
          editor: {
            xtype: 'boolean',
            allowBlank: false
          }
        }],
        customButtons: [{
          text: 'Exit',
          iconCls: 'exit',
          alignRight: true,
          handler: function () {
            ProductControleManagementWindow.close();
          }
        }]
      });
      var ProductControleManagementWindow = new Ext.Window({
        title: 'Product Controle Management',
        width: 800,
        height: 500,
        modal: true,
        layout: 'fit',
        items: [ProductControleManagementGrid]
      });
      ProductControleManagementWindow.show();
    },
    // Product Control wizard Window
    ProductControleWizard: function (params) {
      if (params.Action) {
        url = Concentrator.route("GetListOfAllMatchedProductsForControl", "MasterGroupMapping");
        windowTitle = 'Wizard to Control All Matched Products';
      } else {
        url = Concentrator.route("GetListOfMatchedProductsForProductControleWizard", "MasterGroupMapping");
        windowTitle = 'Wizard to Control All Products in Master Group Mapping "' + params.MasterGroupMappingName + '"';
      }
      var ProductControleTabs = new Ext.TabPanel({
        id: 'ProductControleWizardTabs',
        border: false,
        autoScroll: true,
        region: 'center',
        tabs: []
      });
      var ProductGrid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        primaryKey: ['ProductID', 'VendorName'],
        title: ' ',
        id: 'ProductControleWizardProductGrid',
        //forceFit: true,
        disabled: true,
        width: 350,
        region: 'west',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: url,
        hideBottomBar: true,
        structure: [{
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'ConcentratorNumber',
          header: '#',
          width: 60,
          type: 'int'
        }, {
          dataIndex: 'DistriItemNumber',
          header: 'Distri Item'
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name'
        }, {
          dataIndex: 'ProductApproved',
          header: 'Approved',
          width: 30,
          renderer: function (val, metadata, record) {
            if (val) {
              return '<div class="rowAcceptIcon"/>';
            } else {
              return '<div class="rowCancelIcon"/>';
            }
          }
        }]
      });
      var ProductControleWizardWindow = new Ext.Window({
        modal: true,
        title: windowTitle,
        items: [ProductGrid, ProductControleTabs],
        plain: true,
        layout: 'border',
        id: 'ProductControlWizardWindow',
        width: 1068,
        height: 591,
        maximizable: true,
        resizable: true,
        tbar: [{
          text: 'Update Products',
          iconCls: 'reset',
          handler: function () {
            productGrid = Ext.getCmp('ProductControleWizardProductGrid');
            store = productGrid.getStore();
            store.reload();
            var tabs = Ext.getCmp('ProductControleWizardTabs');
            tabs.removeAll();
          }
        }, "->", {
          text: 'Edit Product',
          iconCls: 'copy',
          handler: function () {
            productGrid = Ext.getCmp('ProductControleWizardProductGrid');
            productGridSelection = productGrid.getSelectionModel();
            if (productGridSelection.getCount() > 0) {
              record = productGridSelection.getSelected();
              Concentrator.MasterGroupMappingFunctions.getEditProductWindow(record.get('ConcentratorNumber'));
            } else {
              Ext.Msg.alert('', 'Select a Product. Click on "Next"');
            }
          }
        }, {
          text: 'Edit Attributes',
          iconCls: 'wrench',
          handler: function () {
            productGrid = Ext.getCmp('ProductControleWizardProductGrid');
            productGridSelection = productGrid.getSelectionModel();
            if (productGridSelection.getCount() > 0) {
              record = productGridSelection.getSelected();
              var parameters = {
                ProductID: record.get('ConcentratorNumber'),
                MasterGroupMappingID: record.get('MasterGroupMappingID'),
                MasterGroupMappingName: params.MasterGroupMappingName
              };
              Concentrator.MasterGroupMappingFunctions.getEditAttributePerProductWindow(parameters);
            } else {
              Ext.Msg.alert('', 'Select a Product. Click on "Next"');
            }
          }
        }, {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            var config = {
              treePanel: true
            };
            Concentrator.MasterGroupMappingFunctions.refreshAction(config);

            ProductControleWizardWindow.close();
          }
        }],
        buttons: [{
          text: 'Un Map',
          iconCls: '',
          handler: function () {
            var gr = Ext.getCmp('ProductControleWizardProductGrid');
            var selection = gr.getSelectionModel();
            if (productGridSelection.getCount() > 0) {
              record = selection.getSelected();
              Concentrator.MasterGroupMappingFunctions.getUnMatchVendorProductWindow(record, function () { gr.store.load() });
            }
          }
        }, {
          text: 'Block',
          iconCls: '',
          handler: function () {
            var gr = Ext.getCmp('ProductControleWizardProductGrid');
            var selection = gr.getSelectionModel();
            if (productGridSelection.getCount() > 0) {
              record = selection.getSelected();
              Concentrator.MGMVendorProductFunctions.BlockProduct(record, function () { gr.store.load() });
            }
          }
        }, {
          text: 'Reject',
          iconCls: '',
          handler: function () {
            var gr = Ext.getCmp('ProductControleWizardProductGrid');
            var selection = gr.getSelectionModel();
            if (productGridSelection.getCount() > 0) {
              record = selection.getSelected();
              Concentrator.MGMVendorProductFunctions.RejectProduct(record, function () { gr.store.load() });
            }

          }
        }, '-', '-', {
          text: 'Back',
          iconCls: '',
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('back');
          }
        }, {
          text: 'Accept and Next',
          iconCls: 'save',
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('acceptAndNext');
          }
        }, {
          text: 'Next',
          iconCls: '',
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('next');
          }
        }]
      });
      ProductControleWizardWindow.show();
      var store = ProductGrid.getStore();
      store.addListener('load', function () {
        if (ProductGrid.getStore().getCount() > 0) {
          var record = ProductGrid.store.data.items[0];
          if (record.json.countProductsToControl > 1) {
            ProductGrid.setTitle(record.json.countProductsToControl + ' Products to Control');
          } else {
            ProductGrid.setTitle(record.json.countProductsToControl + ' Product to Control');
          }
        }
        Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('next');
      });
    },
    ProductControleWizardNaviBtns: function (move) {
      productGrid = Ext.getCmp('ProductControleWizardProductGrid');
      productGridSelection = productGrid.getSelectionModel();
      if (productGrid.getStore().getCount() > 0) {
        var tabs = Ext.getCmp('ProductControleWizardTabs');


        if (move == 'next' || move == 'back') {

          var activeTabIndex = 0;

          var activeTab = tabs.getActiveTab();
          if (activeTab) {
            var index = tabs.items.findIndex('id', activeTab.id);
            activeTabIndex = index;
          }

          var refresh = true;
          tabs.removeAll();
          if (move == 'back') {
            if (productGridSelection.selectPrevious()) {
            } else {
              productGridSelection.selectLastRow();
            }
          }
          if (move == 'next') {
            if (productGridSelection.selectNext()) {
              //Diract.silent_request({
              //  url: Concentrator.route("GetListOfMatchedProducts", "MasterGroupMapping"),
              //  params: {
              //    ProductID: productGridSelection.getSelected().json.ConcentratorNumber
              //  },
              //  success: function (response) {
              //    var skipRecord = false;
              //    for (record in response.results) {
              //      if (response.results[record].MatchStatus && response.results[record].MatchStatus == 'Accepted') {
              //        skipRecord = true;
              //      }
              //    }
              //    if (skipRecord) {
              //      Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('next');
              //      return;
              //    }
              //  }
              //});
            } else {
              if (productGridSelection.getCount() > 0) {
                refresh = false;
                productGrid.getStore().reload();
              } else {
                productGridSelection.selectFirstRow();
              }
            }
          }

          if (refresh) {
            record = productGridSelection.getSelected();
            Concentrator.MasterGroupMappingFunctions.getPCWizardTabs(record.json, activeTabIndex);
          }
        }
        if (move == 'acceptAndNext') {
          if (productGridSelection.getCount() > 0) {
            productControle = true;
            Diract.request({
              url: Concentrator.route('UpdateVendorProduct', 'MasterGroupMapping'),
              params: {
                ConcentratorNumber: record.get('ConcentratorNumber'),
                MasterGroupMappingID: record.get('MasterGroupMappingID'),
                ProductControleVariable: productControle
              },
              success: function () {
                var selectedRecord = productGridSelection.getSelected();
                selectedRecord.set('ProductApproved', true);
                Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('next');

                var config = {
                  treePanel: false,
                  MasterGroupMappingID: selectedRecord.get('MasterGroupMappingID'),
                  ProductGrid: true
                };
                Concentrator.MasterGroupMappingFunctions.refreshAction(config);
              },
              failure: function () {
              }
            });
          } else {
            Concentrator.MasterGroupMappingFunctions.ProductControleWizardNaviBtns('next');
          }
        }
      } else {
        //2nd round
        Diract.request({
          url: Concentrator.route("CheckFinished", "MasterGroupMapping"),
          params: {
            MasterGroupMappingID: productGrid.store.baseParams.MasterGroupMappingID
          },
          success: function (resp) {
            //Ext.getCmp('ProductControleWizardProductGrid').store.reload();
            var productGrid = Ext.getCmp('ProductControleWizardProductGrid');
            var productGridSelection = productGrid.getSelectionModel();

            Ext.Msg.show({
              title: 'Compare attributes',
              msg: resp.message,
              buttons: Ext.Msg.OK,
              fn: function (btn) {
                //var window = Ext.getCmp('ProductControlWizardWindow');
                //window.close();
              },
              animEl: 'elId',
              icon: Ext.MessageBox.QUESTION
            });

            if (resp.message != 'Check complete') {

              var productGrid = Ext.getCmp('ProductControleWizardProductGrid');
              store = productGrid.getStore();
              store.reload();
              var tabs = Ext.getCmp('ProductControleWizardTabs');
              tabs.removeAll();
            }

            var config = {
              allTabPanels: true,
              MasterGroupMappingID: productGrid.store.baseParams.MasterGroupMappingID
            };
            Concentrator.MasterGroupMappingFunctions.refreshAction(config);

          },
          failure: function () {
            Ext.Msg.show({
              title: 'Product table',
              msg: resp.message,
              buttons: Ext.Msg.OK,
              fn: function (btn) {
                var config = {
                  treePanel: true
                };
                Concentrator.MasterGroupMappingFunctions.refreshAction(config);

                var window = Ext.getCmp('ProductControlWizardWindow');
                window.close();
              },
              animEl: 'elId',
              icon: Ext.MessageBox.QUESTION
            });
            productControleWindow.close();
          }
        });
      }
    },
    getPCWizardTabs: function (productGridRecord, activeTabIndex) {
      var tabs = Ext.getCmp('ProductControleWizardTabs');

      productInfoTab = {
        title: 'Product Info',
        items: Concentrator.MasterGroupMappingFunctions.getPCWizardProductInfo(productGridRecord)
      };
      tabs.add(productInfoTab);

      possibleProductMatchItem = {
        title: 'Product Matches',
        items: Concentrator.MasterGroupMappingFunctions.getPossibleProductMatchesPanel(productGridRecord.ConcentratorNumber)
      };
      tabs.add(possibleProductMatchItem);

      compareAttributesTab = {
        title: 'Attributes',
        items: Concentrator.MasterGroupMappingFunctions.getCompareAttributesGrid(productGridRecord.ConcentratorNumber)
      };
      tabs.add(compareAttributesTab);

      tabs.setActiveTab(activeTabIndex);


    },

    initCompareAttributesGrid: function (productID) {
      Diract.silent_request({
        url: Concentrator.route('GetAllAttributesForAllMatchingProducts', 'MasterGroupMapping'),
        params: { productID: productID },
        success: (function (response) {

          window.attributeOptionEditorArray = {};

          var structure = [{
            dataIndex: 'AttributeName',
            type: 'string',
            header: 'Attribute Name'
          },
					{
					  dataIndex: 'AttributeID',
					  type: 'int'
					},
          {
            dataIndex: 'AttributeValueID',
            type: 'int'
          },
					{
					  dataIndex: 'Matched',
					  type: 'string',
					  header: 'Matched',
					  hidden: true
					}];

          Ext.each(response.results, function (result) {

            if (!window.attributeOptionEditorArray[result.ProductID]) {
              window.attributeOptionEditorArray[result.ProductID] = new Diract.ui.SearchBox
              ({
                valueField: 'AttributeOptionID',
                params: { productID: result.ProductID },
                searchParams: { productID: result.ProductID },
                displayField: 'Value',
                clearOnExpand: true,
                searchUrl: Concentrator.route('SearchAttributeOption', 'ProductAttribute'),
                enableCreateItem: false,
                noSyncValue: true,
                searchInID: true,
                attributeOptions: true,
                allowBlank: false,
                listeners: {
                  'select': function (cmb, record, index) {
                    if (this.params.productID != null) {
                      this.gridEditor.record.set("" + this.params.productID + "Value", record.get('Value'));
                    }
                  }
                }
              });
            }

            var struc = {
              dataIndex: result.ProductID,
              header: result.VendorItemNumber + ' (' + result.ProductID + ')' + result.Primary + (result.VendorName != "" ? (' - ' + result.VendorName) : ''),
              type: 'string',
              filterable: false,
              editor: window.attributeOptionEditorArray[result.ProductID],
              renderer: function (val, m, rec) {
                return rec.get(this.dataIndex + "Value") || rec.get(this.dataIndex);
              }
            }
            structure.push(struc);
          });

          var attributeGrid = new Diract.ui.Grid({
            ddGroup: 'attibuteGridDD',
            sortBy: 'Matched',
            groupField: 'Matched',
            enableDragDrop: true,
            height: 470,
            params: { productIDs: response.productIDs, productID: productID },
            region: 'center',
            url: Concentrator.route("GetAllAttributesInfoForAllMatchingProducts", "MasterGroupMapping"),
            permissions: {
              list: 'DefaultMasterGroupMapping',
              update: 'DefaultMasterGroupMapping'
            },
            structure: structure,
            customButtons: [
							{
							  text: 'Save Changes',
							  iconCls: 'save',
							  id: 'attributeGridSaveButton',
							  handler: function () {

							    var edits = attributeGrid.store.getModifiedRecords();
							    var editsToSend = [];

							    for (var i = 0, len = edits.length; i < len; i++) {
							      delete edits[i].modified.Value;
							    }

							    for (var i = 0, len = edits.length; i < len; i++) {

							      var changedFields = Concentrator.MasterGroupMappingFunctions.getChangedAndBooleanFields(edits[i]);
							      var attributeID = edits[i].data.AttributeID;

							      for (var propertyName in changedFields) {
							        var propName = propertyName;
							        if (propName) {

							          //is value field
							          if (propName.indexOf('Value') == -1) {
							            var productID = propName;
							            var attributeValue = changedFields[propName + "Value"];
							            if (!attributeValue)
							              attributeValue = changedFields[propName];

							            var paramsToSend = {
							              productID: productID,
							              attributeID: attributeID,
							              attributeValue: attributeValue
							            }
							            editsToSend.push(paramsToSend);
							          }
							        }
							      }
							    }
							    //send edits
							    var countCellsUpdated = 0;
							    for (var i = 0; i < editsToSend.length ; i++) {
							      var paramsToSend = editsToSend[i]
							      countCellsUpdated++;
							      Diract.request({
							        url: Concentrator.route("UpdateAttributesForMatchingProducts", "MasterGroupMapping"),
							        params: paramsToSend,
							        flushAt: editsToSend.length,
							        onFlush: function () {
							          if (countCellsUpdated == editsToSend.length) {
							            attributeGrid.store.commitChanges();
							            attributeGrid.store.reload();
							          }
							        }
							      });
							    }
							  }
							},
							{
							  text: 'Discard changes',
							  iconCls: "cancel",
							  handler: function () {
							    var edits = attributeGrid.store.getModifiedRecords();
							    Ext.MessageBox.confirm(Diract.text.confirmTitle, Diract.text.discardBtnConfirmation, function (btn) {
							      if (btn == "yes") {
							        attributeGrid.store.rejectChanges();
							      }
							    }, this);
							  }
							}
            ],
            rowActions: [
              {
                text: 'Edit Attribute options',
                iconCls: 'wrench',
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
                    newFormConfig: Concentrator.FormConfigurations.newAttributeOption,
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
            ],
            listeners: {
              render: function () {
                var ddrow = new Ext.dd.DropTarget(attributeGrid.container, {
                  ddGroup: 'attibuteGridDD',
                  copy: false,
                  notifyDrop: function (dd, e, data) {

                    var attributeGridStore = attributeGrid.store;

                    var selectionModel = attributeGrid.getSelectionModel();
                    var selectedRows = selectionModel.getSelections();

                    if (dd.getDragData(e)) {

                      var dragIndex = dd.getDragData(e).rowIndex;
                      if (typeof (dragIndex) != "undefined") {

                        for (i = 0; i < selectedRows.length; i++) {
                          attributeGridStore.remove(attributeGridStore.getById(selectedRows[i].id));
                        }

                        attributeGridStore.insert(dragIndex, data.selections);
                        selectionModel.clearSelections();
                      }
                    }

                  }
                })
              },
              cellclick: function (grid, rowIndex, cellIndex) {
                //var record = grid.getStore().getAt(rowIndex);
                //var cell = record.store.getAt(cellIndex);
                //for (key in window.attributeOptionEditorArray) {
                //  var editor = window.attributeOptionEditorArray[key];
                //  editor.searchParams = { AttributeValueID: record.get('AttributeValueID'), AttributeID: record.get('AttributeID') };
                //  editor.store.searchParams = editor.searchParams;
                //  editor.store.load({ params: editor.searchParams });
                //}
                //var tempProductid = record.get('ProductID');
                //return true;
              }
            },
            editPredicate: function (record) {
              //var record = grid.getStore().getAt(rowIndex);
              //var cell = record.store.getAt(cellIndex);
              for (key in window.attributeOptionEditorArray) {
                var editor = window.attributeOptionEditorArray[key];
                if (editor.searchParams && editor.searchParams.AttributeValueID != record.get('AttributeValueID')) {
                  editor.searchParams = { AttributeValueID: record.get('AttributeValueID'), AttributeID: record.get('AttributeID') };
                  editor.store.searchParams = editor.searchParams;
                  editor.store.load({ params: editor.searchParams });
                }
              }
              //var tempProductid = record.get('ProductID');
              return true;
            }
          });
          Ext.getCmp('compareAttributesPanel').add(attributeGrid);
          Ext.getCmp('compareAttributesPanel').doLayout();
        }).createDelegate(this)
      });
    },

    getCompareAttributesGrid: function (productID) {
      var compareAttributesPanel = new Ext.Panel({
        id: 'compareAttributesPanel',
        height: 470,
        region: 'center',
        items: []
      });

      this.initCompareAttributesGrid(productID);

      return compareAttributesPanel;
    },

    getPossibleProductMatchesPanel: function (productID) {

      var PossibleMatchesGrid = this.getPossibleMatchesGrid(productID);

      var PossibleMatchesTabs = this.getPossibleMatchesTabs();

      return new Ext.Panel({
        layout: 'border',
        height: 470,
        margins: '0 0 0 0',
        border: false,
        items: [PossibleMatchesGrid, PossibleMatchesTabs],
        tbar: [{
          text: 'Add Product To Match',
          iconCls: 'add',
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.getProductWindow(record.get('ConcentratorNumber'));
          }
        }, {
          text: 'Save Matches',
          iconCls: 'save',
          handler: function () {
            var store = Ext.getCmp('PossibleMatchesGrid').getStore();
            this.setDisabled(true);

            var data = [];
            for (var idx = 0; idx < store.data.length; idx++) {
              var record = store.data.items[idx].data;
              data.push({ productID: record.ProductID, IsMatch: record.Check });
            }
            var params = { data: JSON.stringify(data) };
            //Products: store.getRange(0, store.getCount())

            //Ext.each(store.getRange(0,store.getCount()), function (r) {
            Diract.request({
              url: Concentrator.route('UpdateMatchProductsWizard', 'MasterGroupMapping'),
              params: {
                //ProductID: r.data.ProductID,
                //IsMatch: r.data.Check,
                Products: JSON.stringify(data)
              },
              callback: function () {
                PossibleMatchesGrid.getStore().reload();
                var productGrid = Ext.getCmp('ProductControleWizardProductGrid');
                var productGridSelection = productGrid.getSelectionModel();
                var selectedRecord = productGridSelection.getSelected();

                var config = {
                  treePanel: true,
                  MasterGroupMappingID: selectedRecord.get('MasterGroupMappingID')
                };
                Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                //var panels = {
                //  allTabPanels: true
                //};
                //Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                // PossibleMatchesWindow.close();
              }
            });
            //});

            //PossibleMatchesGrid.getStore().reload();
            this.setDisabled(false);
          }
        }]
      });

    },
    getPCWizardProductInfo: function (productGridRecord) {
      var prodcutDescriptionPanel = new Ext.Panel({
        region: 'center',
        layout: 'fit',
        height: 210,
        border: false,
        items: [{
          autoScroll: true,
          padding: 3,
          html: '<b>' + productGridRecord.BrandName + ' ' + productGridRecord.ShortDescription + '</b>' + '<br>' + '<br>' +
																		'Title: ' + ((productGridRecord.Title != null) ? productGridRecord.Title : '') + '<br>' +
																		'ProductID: ' + productGridRecord.CustomItemNumber + '<br>' +
																		'Model: ' + productGridRecord.VendorItemNumber + '<br>' +
																		'Barcode: ' + productGridRecord.Barcode + '<br>' +
																		'Merk: ' + productGridRecord.BrandName + '<br>' +
																		'Extra beschrijving: ' + ((productGridRecord.LongDescription != null) ? productGridRecord.LongDescription : '') + '<br>' +
																		'<b>Check step: ' + ((productGridRecord.Step != null) ? productGridRecord.Step : 'Product Check') + '</b>'
        }]
      });

      Ext.QuickTips.init();

      Ext.apply(Ext.QuickTips.getQuickTip(), {
        maxWidth: 10000
      });

      var prodcutImagesTabs = new Ext.TabPanel({
        region: 'east',
        border: false,
        enableTabScroll: true,
        height: 210,
        width: 210,
        tabs: []
      });
      var countImage = 0;
      Ext.each(productGridRecord.Images, function (imageRec) {
        var id = Ext.id();

        var img = new Image();
        img.src = imageRec.isUrl ? imageRec.Image : Concentrator.GetImageUrl(imageRec.Image, 400, 400, true, null);

        tabItem = {
          title: ++countImage,
          padding: 3,
          image: img,
          imageID: id,
          listeners: {
            afterrender: function (c) {
              var html = '<img class="ImgCenterSmall" id=' + c.imageID + ' src="' + c.image.src + '" />';
              c.update(html);
              var divObj = Ext.get(c.imageID);
              Ext.QuickTips.register({
                target: divObj,
                baseCls: 'noBackGround',
                text: '<img class="ImgCenterBig" src="' + c.image.src + '" />'
              });
            }
          }
        };

        prodcutImagesTabs.add(tabItem);
      });
      prodcutImagesTabs.setActiveTab(0);
      var attributeGrid = new Diract.ui.Grid({
        region: 'center',
        height: 260,
        pluralObjectName: 'Attributes',
        singularObjectName: 'Attribute',
        sortBy: 'AttributeName',
        autoLoadStore: false,
        store: new Ext.data.GroupingStore({
          reader: new Ext.data.JsonReader({
            idProperty: 'AttributeID',
            fields: ['AttributeGroup', 'AttributeName', 'AttributeValue', 'VendorName']
          }),
          data: productGridRecord.Attributes,
          sortInfo: { field: 'AttributeName', direction: "ASC" },
          groupField: 'AttributeGroup'
        }),
        forceFit: true,
        permissions: {},
        groupField: 'AttributeGroup',
        hideGroupedColumn: 'AttributeGroup',
        structure: [{
          dataIndex: 'AttributeGroup',
          header: 'Group'
        }, {
          header: 'Name',
          dataIndex: 'AttributeName'
        }, {
          header: 'Value',
          dataIndex: 'AttributeValue'
        }, {
          dataIndex: 'VendorName',
          header: 'Vendor'
        }]
      });
      var productControlGrid = new Diract.ui.Grid({
        region: 'east',
        width: 210,
        pluralObjectName: '',
        singularObjectName: '',
        id: 'WizardProductControlPanelProductControlGrid',
        primaryKey: 'ProductControlID',
        forceFit: true,
        permissions: {
          list: 'DefaultMasterGroupMapping'
        },
        params: {
          ProductID: productGridRecord.ConcentratorNumber,
          MasterGroupMappingID: productGridRecord.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfProductControleOfProduct", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'ProductControlID',
          type: 'int'
        }, {
          dataIndex: 'ProductControlName',
          header: 'Required Conditions',
          width: 30
        }, {
          dataIndex: 'ProductControlApproved',
          header: 'Approved',
          width: 20,
          renderer: function (val, metadata, record) {
            if (val) {
              return '<div class="rowAcceptIcon"/>';
            } else {
              return '<div class="rowCancelIcon"/>';
            }
          }
        }]
      });
      var southPanel = new Ext.Panel({
        region: 'south',
        layout: 'border',
        width: 650,
        height: 260,
        margins: '0 0 0 0',
        border: false,
        items: [attributeGrid, productControlGrid]
      });
      return new Ext.Panel({
        layout: 'border',
        region: 'center',
        height: 470,
        border: false,
        items: [prodcutDescriptionPanel, southPanel, prodcutImagesTabs]
      });
    },
    productControleWindow: function (record) {
      Diract.silent_request({
        url: Concentrator.route('GetListOfMatchedProductForProductControle', 'MasterGroupMapping'),
        waitMsg: 'Getting Product Info',
        params: {
          ProductID: record.get('ConcentratorNumber'),
          MasterGroupMappingID: record.get('MasterGroupMappingID')
        },
        success: function (result) {
          var windowTitle = 'Control Product "' + record.get('ConcentratorNumber') + ', ' + record.get('ProductName');
          if (record.get('Brand') != null && record.get('Brand') != "") {
            windowTitle += ', ' + record.get('Brand');
          }
          windowTitle += '" in Master Group Mapping "' + record.get('MasterGroupMappingPad') + '"';
          var ProductControleTabs = new Ext.TabPanel({
            id: 'ProductControleWizardTabs',
            border: false,
            autoScroll: true,
            region: 'center',
            tabs: []
          });
          Concentrator.MasterGroupMappingFunctions.getPCWizardTabs(result.results[0]);
          var productControleWindow = new Ext.Window({
            modal: true,
            title: windowTitle,
            items: [ProductControleTabs],
            plain: true,
            layout: 'border',
            width: 665,
            height: 558,
            maximizable: true,
            resizable: false,
            tbar: [{
              text: 'Product Approved',
              iconCls: 'save',
              handler: function () {
                productControle = true;
                Diract.request({
                  url: Concentrator.route('UpdateVendorProduct', 'MasterGroupMapping'),
                  waitMsg: 'Updating Product Status To Approval',
                  params: {
                    ConcentratorNumber: record.get('ConcentratorNumber'),
                    MasterGroupMappingID: record.get('MasterGroupMappingID'),
                    ProductControleVariable: productControle
                  },
                  success: function () {
                    productControleWindow.close();
                    config = {
                      treePanel: true,
                      vendorProductsTabPanel: true,
                      MasterGroupMappingID: record.get('MasterGroupMappingID')
                    };
                    Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                  },
                  failure: function (form) {
                  }
                });
              }
            }, "->", {
              text: 'Edit Product',
              iconCls: 'copy',
              handler: function () {
                Concentrator.MasterGroupMappingFunctions.getEditProductWindow(result.results[0].ConcentratorNumber);
              }
            }, {
              text: 'Edit Attributes',
              iconCls: 'wrench',
              handler: function () {
                var parameters = {
                  ProductID: result.results[0].ConcentratorNumber,
                  MasterGroupMappingID: result.results[0].MasterGroupMappingID,
                  MasterGroupMappingName: record.get('MasterGroupMappingPad')
                };
                Concentrator.MasterGroupMappingFunctions.getEditAttributePerProductWindow(parameters);
              }
            }, {
              text: 'Exit',
              iconCls: 'exit',
              handler: function () {
                productControleWindow.close();
              }
            }]
          });
          productControleWindow.show();
        }
      });
    },
    getEditProductWindow: function (productID) {
      Diract.request({
        url: Concentrator.route('GetProductDetails', 'Product'),
        waitMsg: 'Getting Product Details',
        params: {
          productID: productID
        },
        success: (function (data) {
          var productPanel = new Concentrator.ui.ProductBrowser({
            productID: productID,
            product: data.product
          });
          var editProductWindow = new Ext.Window({
            modal: true,
            title: 'Product Editor',
            plain: true,
            layout: 'fit',
            width: 1200,
            height: 600,
            items: productPanel,
            tbar: ["->", {
              text: 'Exit',
              iconCls: 'exit',
              handler: function () {
                editProductWindow.close();
                var productControlGrid = Ext.getCmp('WizardProductControlPanelProductControlGrid');
                productControlGrid.getStore().reload();
              }
            }]
          });
          editProductWindow.show();
        })
      });
    },
    getEditAttributePerProductWindow: function (params) {

      var attributeOptionEditor = new Diract.ui.SearchBox
        ({
          valueField: 'Value',
          params: { productID: this.productID },
          displayField: 'Value',
          searchUrl: Concentrator.route('SearchAttributeOption', 'ProductAttribute'),
          enableCreateItem: false,
          searchInID: true,
          fieldLabel: 'Value',
          allowBlank: true,
          searchParams: -1,
          createFormConfig: Concentrator.FormConfigurations.newAttributeOption,
          singularObjectName: 'Product Attribute Option',
          attributeOptions: true,
          formItems: [
            Concentrator.FormConfigurations.newAttributeOption
          ],
          createUrl: Concentrator.route('CreateAttributeOption', 'ProductAttribute')
        });

      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Attributes',
        singularObjectName: 'Attribute',
        primaryKey: ['AttributeValueID', 'ProductID', 'AttributeID'],
        forceFit: true,
        width: 600,
        permissions: {
          list: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          ProductID: params.ProductID,
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        editPredicate: function (record) {
          attributeOptionEditor.searchParams = { AttributeValueID: record.get('AttributeValueID'), AttributeID: record.get('AttributeID') };
          attributeOptionEditor.store.searchParams = attributeOptionEditor.searchParams;
          attributeOptionEditor.store.load({ params: attributeOptionEditor.searchParams });
          return true;
        },
        url: Concentrator.route("GetListOfAttributeValues", "MasterGroupMapping"),
        updateUrl: Concentrator.route("UpdateAttributeValues", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'AttributeID',
          type: 'int'
        }, {
          dataIndex: 'AttributeValueID',
          type: 'int'
        }, {
          dataIndex: 'ProductID',
          type: 'int'
        }, {
          dataIndex: 'AttributeGroupName',
          header: 'Attribute Group'
        }, {
          dataIndex: 'AttributeName',
          header: 'Attribute'
        }, {
          dataIndex: 'VendorName',
          header: 'Vendor'
        }, {
          dataIndex: 'AttributeValue',
          header: 'Value',
          editor: attributeOptionEditor
        }],
        customButtons: ["->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
            var productControlGrid = Ext.getCmp('WizardProductControlPanelProductControlGrid');
            productControlGrid.getStore().reload();
          }
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Required Attributes of Master Group Mapping "' + params.MasterGroupMappingName + '"',
        plain: true,
        layout: 'fit',
        width: 500,
        height: 500,
        items: grid
      });
      window.show();
    },
    getProductWindow: function (productID) {
      var productGrid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        sortBy: 'ConcentratorNumber',
        ddGroup: 'ProductGridDDGroup',
        enableDragDrop: true,
        autoLoadStore: true,
        title: 'List of All Products',
        params: {
          ProductID: productID,
          ForGrid: 'product'
        },
        url: Concentrator.route("GetListOfProductsForAddProductToMatch", "MasterGroupMapping"),
        region: 'west',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [{
          dataIndex: 'ProductMatchID',
          type: 'int'
        }, {
          dataIndex: 'ConcentratorNumber',
          header: 'Concentrator #',
          type: 'int',
          width: 125
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name',
          type: 'string',
          width: 240
        }, {
          dataIndex: 'Brand',
          header: 'Brand',
          type: 'string'
        }, {
          dataIndex: 'ProductCode',
          header: 'Product Code',
          type: 'string'
        }],
        listeners: {
          'rowclick': function (t, rowIndex, e) {
          }
        },
        width: 500,
        height: 500
      });
      var matchGrid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        id: 'ProductMatchGrid',
        sortBy: 'ConcentratorNumber',
        ddGroup: 'MatchGridDDGroup',
        title: 'List of Matched Products',
        enableDragDrop: true,
        autoLoadStore: true,
        params: {
          ProductID: productID,
          ForGrid: 'match'
        },
        url: Concentrator.route("GetListOfProductsForAddProductToMatch", "MasterGroupMapping"),
        region: 'center',
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [{
          dataIndex: 'ProductMatchID',
          type: 'int'
        }, {
          dataIndex: 'ConcentratorNumber',
          header: 'Concentrator #',
          type: 'int',
          width: 125
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name',
          type: 'string',
          width: 240
        }, {
          dataIndex: 'Brand',
          header: 'Brand',
          type: 'string'
        }, {
          dataIndex: 'ProductCode',
          header: 'Product Code',
          type: 'string'
        }],
        width: 400,
        height: 500
      });
      var window = new Ext.Window({
        modal: false,
        title: 'Wizard to Match Products',
        plain: true,
        items: [productGrid, matchGrid],
        layout: 'border',
        resizable: false,
        maximizable: true,
        width: 900,
        height: 500,
        renderTo: 'MasterGroupMappingCenterPanel',
        tbar: ["->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
            var matchesGrid = Ext.getCmp('PossibleMatchesGrid');
            if (matchesGrid)
              matchesGrid.store.reload();

            //var productGrid = Ext.getCmp('ProductControleWizardProductGrid');
            //if (productGrid) {
            //  store = productGrid.getStore();
            //  store.reload();
            //  var tabs = Ext.getCmp('ProductControleWizardTabs');
            //  tabs.removeAll();
            //}

            var compareAttPanel = Ext.getCmp('compareAttributesPanel');
            var attributeGrid = compareAttPanel.items.items[0];
            if (attributeGrid) {
              compareAttPanel.removeAll();
              Concentrator.MasterGroupMappingFunctions.initCompareAttributesGrid(productID);
            }
          }
        }]
      });
      window.show();


      var productGridDropTargetEl = productGrid.getView().scroller.dom;
      var productGridDropTarget = new Ext.dd.DropTarget(productGridDropTargetEl, {
        ddGroup: 'MatchGridDDGroup',
        notifyDrop: function (ddSource, e, data) {
          var records = ddSource.dragData.selections;
          var store = matchGrid.getStore();
          var firstRecord = store.getRange(0, 0);
          var productID = firstRecord[0].get('ConcentratorNumber');
          if (records.length > 0 && productID > 0) {
            var ListOfIDs = { ProductID: 0, NewMatchProductIDs: [], Action: 'unmatch' };
            ListOfIDs.ProductID = productID;
            Ext.each(records, function (record) {
              ListOfIDs.NewMatchProductIDs.push(record.get('ConcentratorNumber'));
            });
            var ListOfIDs = JSON.stringify(ListOfIDs);
            Diract.request({
              url: Concentrator.route('UpdateProductsForAddProductToMatch', 'MasterGroupMapping'),
              params: {
                ListOfIDs: ListOfIDs
              },
              success: function () {
                productGrid.getStore().reload();
                Ext.each(records, function (record) {
                  matchGrid.getStore().remove(record);
                });
              }
            });
          }
        }
      });

      var matchGridDropTargetEl = matchGrid.getView().scroller.dom;
      var matchGridDropTarget = new Ext.dd.DropTarget(matchGridDropTargetEl, {
        ddGroup: 'ProductGridDDGroup',
        notifyDrop: function (ddSource, e, data) {
          var records = ddSource.dragData.selections;
          var store = matchGrid.getStore();
          var firstRecord = store.getRange(0, 0);
          var productID = firstRecord[0].get('ConcentratorNumber');
          if (records.length > 0 && productID > 0) {
            var ListOfIDs = { ProductID: 0, NewMatchProductIDs: [], Action: 'match' };
            ListOfIDs.ProductID = productID;
            Ext.each(records, function (record) {
              ListOfIDs.NewMatchProductIDs.push(record.get('ConcentratorNumber'));
            });
            var ListOfIDs = JSON.stringify(ListOfIDs);
            Diract.request({
              url: Concentrator.route('UpdateProductsForAddProductToMatch', 'MasterGroupMapping'),
              params: {
                ListOfIDs: ListOfIDs
              },
              success: function () {
                matchGrid.getStore().reload();
                Ext.each(records, function (record) {
                  productGrid.getStore().remove(record);
                });
              }
            });
          }
        }
      });


    },
    // Cross Reference Management
    // params:
    // MasterGroupMappingID
    // MasterGroupMappingName
    getCrossReferenceManagementWindow: function (params) {
      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Cross References',
        singularObjectName: 'Cross Reference',
        sortBy: 'CrossReferenceID',
        autoLoadStore: true,
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfCrossReferences", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [{
          dataIndex: 'CrossReferenceID',
          type: 'int'
        }, {
          dataIndex: 'CrossReferenceName',
          header: 'Cross Reference Name'
        }, {
          dataIndex: 'CrossReferencePath',
          header: 'Cross Reference Path'
        }, {
          dataIndex: 'CountProducts',
          header: '# Products',
          type: 'int'
        }],
        rowActions: [{
          text: 'UnCross Reference',
          iconCls: 'delete',
          handler: function (record) {
            Ext.Msg.show({
              title: 'UnCross Reference',
              msg: 'Would you like to UnCross Reference "' + record.get('CrossReferenceName') + '" from Master Group Mapping "' + params.MasterGroupMappingName + '" ?',
              buttons: { ok: "Yes", cancel: "No" },
              fn: function (result) {
                if (result == 'ok') {
                  var MgmIDAndCrID = { MasterGroupMappingID: 0, CrossReferenceID: 0 };
                  MgmIDAndCrID.MasterGroupMappingID = params.MasterGroupMappingID;
                  MgmIDAndCrID.CrossReferenceID = record.get('CrossReferenceID');
                  var MgmIDAndCrIDJson = JSON.stringify(MgmIDAndCrID);

                  Diract.request({
                    url: Concentrator.route('UnCrossReference', 'MasterGroupMapping'),
                    params: {
                      MgmIDAndCrIDJson: MgmIDAndCrIDJson
                    },
                    callback: function () {
                      grid.getStore().reload();
                    }
                  });
                } else {
                };
              },
              animEl: 'elId',
              icon: Ext.MessageBox.QUESTION
            });
          }
        }, {
          text: 'View Products',
          iconCls: 'wrench',
          handler: function (record) {
            var parameters = {
              MasterGroupMappingID: record.get('CrossReferenceID'),
              MasterGroupMappingName: record.get('CrossReferenceName')
            };
            Concentrator.MasterGroupMappingFunctions.getViewProductsWindow(parameters);
          }
        }],
        customButtons: [{
          text: 'Mapping Related Attribute',
          iconCls: 'wrench',
          handler: function () {
            Concentrator.MasterGroupMappingFunctions.getMappingRelatedAttributeWindow(params);
          }
        }, "->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Cross References of Master Group Mapping"' + params.MasterGroupMappingName + '"',
        plain: true,
        items: grid,
        layout: 'fit',
        resizable: false,
        width: 600,
        height: 400,
        renderTo: 'MasterGroupMappingCenterPanel'
      });
      window.show();
    },
    // params:
    // MasterGroupMappingID
    // MasterGroupMappingName
    getMappingRelatedAttributeWindow: function (params) {

      var mgmAttributesGrid = new Diract.ui.Grid({
        pluralObjectName: 'Attributes',
        singularObjectName: 'Attribute',
        sortBy: 'AttributeGroupName',
        title: 'Product Attributes in Master Group Mapping',
        id: 'MasterGroupMappingAttributeViewGrid',
        autoLoadStore: true,
        enableDragDrop: true,
        ddGroup: 'relatedAttributeDDGroup',
        region: 'west',
        width: 400,
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfMasterGroupMappingAttributes", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        groupField: 'AttributeGroupName',
        hideGroupedColumn: 'AttributeGroupName',
        structure: [{
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'AttributeID',
          type: 'int'
        }, {
          dataIndex: 'AttributeGroupName',
          header: 'Group Name'
        }, {
          dataIndex: 'AttributeName',
          header: 'Attribute Name'
        }, {
          dataIndex: 'FormatString',
          header: 'Format'
        }, {
          dataIndex: 'IsVisible',
          header: 'IsVisible',
          type: 'boolean'
        }, {
          dataIndex: 'IsSearchable',
          header: 'IsSearchable',
          type: 'boolean'
        }],
        listeners: {
          'rowclick': function (t, rowIndex, e) {
            var record = t.getSelectionModel().getSelected();
          }
        }
      });

      var eastPanelCrossReferenceProductAttributeNamesGrid = new Diract.ui.Grid({
        region: 'north',
        pluralObjectName: 'Attributes',
        singularObjectName: 'Attribute',
        hideBottomBar: true,
        autoLoadStore: true,
        forceFit: false,
        width: 430,
        height: 110,
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfCrossReferences", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [{
          dataIndex: 'CrossReferenceID',
          type: 'int'
        }, {
          dataIndex: 'CrossReferenceName',
          header: 'Cross Reference Name'
        }, {
          dataIndex: 'CrossReferencePath',
          header: 'Cross Reference Path'
        }],
        listeners: {
          rowclick: function (t, rowIndex, e) {
            var record = eastPanelCrossReferenceProductAttributeNamesGrid.getSelectionModel().getSelected();
            var CrossReferenceID = record.get('CrossReferenceID');

            eastPanelCrossReferenceProductAttributesGrid.getStore().reload({ params: { CrossReferenceID: CrossReferenceID } });
          }
        }
      });
      var eastPanelCrossReferenceProductAttributesGrid = new Diract.ui.Grid({
        region: 'center',
        pluralObjectName: 'Attributes',
        singularObjectName: 'Attribute',
        sortBy: 'AttributeGroupName',
        id: 'CrossRederenceAttributeViewGrid',
        autoLoadStore: true,
        enableDragDrop: true,
        ddGroup: 'relatedAttributeDDGroup',
        width: 430,
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfCrossReferenceAttributes", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        groupField: 'AttributeGroupName',
        hideGroupedColumn: 'AttributeGroupName',
        structure: [{
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'AttributeID',
          type: 'int'
        }, {
          dataIndex: 'AttributeGroupName',
          header: 'Group Name'
        }, {
          dataIndex: 'AttributeName',
          header: 'Attribute Name'
        }, {
          dataIndex: 'FormatString',
          header: 'Format'
        }, {
          dataIndex: 'IsVisible',
          header: 'IsVisible',
          type: 'boolean'
        }, {
          dataIndex: 'IsSearchable',
          header: 'IsSearchable',
          type: 'boolean'
        }],
        listeners: {
          'rowclick': function (t, rowIndex, e) {
            var record = t.getSelectionModel().getSelected();
          }
        }
      });

      var eastPanel = new Ext.Panel({
        layout: 'border',
        region: 'east',
        title: 'Product Attributes in Cross Reference',
        width: 400,
        items: [eastPanelCrossReferenceProductAttributeNamesGrid, eastPanelCrossReferenceProductAttributesGrid]
      });

      var centerPanelRelatedAttributesGrid = new Diract.ui.Grid({
        region: 'center',
        id: 'RelatedAttributeRelationGrid',
        width: 200,
        title: 'Attribute Relation Mapping',
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        hideBottomBar: true,
        url: Concentrator.route("GetListOfRelatedAttributes", "MasterGroupMapping"),
        structure: [{
          dataIndex: 'CrossReferenceAttributeName',
          pluginType: 'rowexpander',
          tpl: new Ext.XTemplate(
						'<tpl for="CrossReferenceAttributeName">',
							' <p>  - {.} </p>',
						'</tpl>'
					)
        }, {
          dataIndex: 'RelatedAttributeID',
          type: 'int'
        }, {
          dataIndex: 'AttributeID',
          type: 'int'
        }, {
          dataIndex: 'AttributeName',
          header: 'Attribute Name'
        }, {
          dataIndex: 'AttributeValue',
          header: 'Value'
        }],
        rowActions: [{
          text: 'Delete Relation',
          iconCls: 'delete',
          handler: function () {
            var record = centerPanelRelatedAttributesGrid.getSelectionModel().getSelected();
            params = {
              MasterGroupMappingID: record.store.baseParams.MasterGroupMappingID,
              AttributeID: record.get('AttributeID'),
              AttributeName: record.get('AttributeName'),
              AttributeValue: record.get('AttributeValue'),
              RelatedAttributeID: record.get('RelatedAttributeID')
            };
            Concentrator.MasterGroupMappingFunctions.getViewRelatedAttributesWindow(params);
          }
        }]
      });

      // Setup the form panel
      var dropPanel = new Ext.form.FormPanel({
        region: 'south',
        id: 'southPanelDropPanel',
        title: 'Drop Zone: Drop Dragged Attribute Here!',
        bodyStyle: 'padding: 10px; background-color: #DFE8F6',
        labelWidth: 200,
        width: 200,
        height: 150,
        items: [{
          xtype: 'hidden',
          name: 'MasterGroupMappingID',
          readOnly: true
        }, {
          xtype: 'hidden',
          name: 'CrossReferenceID',
          readOnly: true
        }, {
          xtype: 'hidden',
          name: 'MasterGroupMappingAttributeID',
          readOnly: true
        }, {
          xtype: 'hidden',
          name: 'CrossReferenceAttributeID',
          readOnly: true
        }, {
          xtype: 'displayfield',
          fieldLabel: 'Master Group Mapping Attribute'
        }, {
          xtype: 'textfield',
          fieldLabel: '',
          name: 'MasterGroupMappingAttributeName',
          readOnly: true,
          hideLabel: true
        }, {
          xtype: 'displayfield',
          fieldLabel: 'Cross Reference Attribute'
        }, {
          xtype: 'textfield',
          fieldLabel: '',
          name: 'CrossReferenceAttributeName',
          readOnly: true,
          hideLabel: true
        }]
      });

      var centerPanel = new Ext.Panel({
        layout: 'border',
        region: 'center',
        width: 300,
        items: [centerPanelRelatedAttributesGrid, dropPanel],
        bbar: ['->', {
          text: 'Reset',
          iconCls: 'reset',
          handler: function () {
            dropPanel.getForm().reset();
          }
        }, {
          text: 'Create Relation',
          iconCls: 'add',
          handler: function () {

            var MasterGroupMappingID = dropPanel.getForm().getFieldValues().MasterGroupMappingID;
            var MasterGroupMappingAttributeID = dropPanel.getForm().getFieldValues().MasterGroupMappingAttributeID;
            var MasterGroupMappingAttributeName = dropPanel.getForm().getFieldValues().MasterGroupMappingAttributeName;
            var CrossReferenceID = dropPanel.getForm().getFieldValues().CrossReferenceID;
            var CrossReferenceAttributeID = dropPanel.getForm().getFieldValues().CrossReferenceAttributeID;
            var CrossReferenceAttributeName = dropPanel.getForm().getFieldValues().CrossReferenceAttributeName;

            if (MasterGroupMappingID == "" || MasterGroupMappingAttributeID == "") {
              Ext.Msg.alert('Missing Master Group Mapping Attribute item', 'Drop one attribute from Master Group Mapping Attribute Grid to make a relation.');
            } else if (CrossReferenceAttributeID == "" || CrossReferenceID == "") {
              Ext.Msg.alert('Missing Cross Reference Attribute item', 'Drop one attribute from Cross Reference Attribute Grid to make a relation.');
            } else {
              var ListOfIDs = {
                MasterGroupMappingID: MasterGroupMappingID,
                MasterGroupMappingAttributeID: MasterGroupMappingAttributeID,
                MasterGroupMappingAttributeName: MasterGroupMappingAttributeName,
                CrossReferenceID: CrossReferenceID,
                CrossReferenceAttributeID: CrossReferenceAttributeID,
                CrossReferenceAttributeName: CrossReferenceAttributeName
              };

              Concentrator.MasterGroupMappingFunctions.getCreateRelationWindow(ListOfIDs);
            }
          }
        }]
      });

      var window = new Ext.Window({
        modal: true,
        title: 'Mapping Related Attributes of Master Group Mapping "' + params.MasterGroupMappingName + '"',
        plain: true,
        layout: 'border',
        resizable: false,
        width: 1100,
        height: 650,
        renderTo: 'MasterGroupMappingCenterPanel',
        items: [mgmAttributesGrid, eastPanel, centerPanel],
        tools: [{
          id: 'help',
          qtip: 'Get Help',
          handler: function (event, toolEl, panel) {

          }
        }],
        tbar: ["->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      window.show();

      /****
			* Setup Drop Targets
			***/
      // This will make sure we only drop to the view container
      var dropPanelDropTargetEl = dropPanel.body.dom;
      var dropPanelDropTarget = new Ext.dd.DropTarget(dropPanelDropTargetEl, {
        ddGroup: 'relatedAttributeDDGroup',
        notifyEnter: function (ddSource, e, data) {

          //Add some flare to invite drop.
          dropPanel.body.stopFx();
          dropPanel.body.highlight();
        },
        notifyDrop: function (ddSource, e, data) {
          if (data.selections.length == 1) {

            var droppedRecord = ddSource.dragData.selections[0];
            var form = dropPanel.getForm();

            switch (ddSource.grid.id) {
              case 'MasterGroupMappingAttributeViewGrid':
                {
                  form.setValues({
                    MasterGroupMappingID: droppedRecord.get('MasterGroupMappingID'),
                    MasterGroupMappingAttributeID: droppedRecord.get('AttributeID'),
                    MasterGroupMappingAttributeName: droppedRecord.get('AttributeName')
                  });
                }
                break;
              case 'CrossRederenceAttributeViewGrid':
                {
                  form.setValues({
                    CrossReferenceID: droppedRecord.get('MasterGroupMappingID'),
                    CrossReferenceAttributeID: droppedRecord.get('AttributeID'),
                    CrossReferenceAttributeName: droppedRecord.get('AttributeName')
                  });
                }
                break;
              default:
            }
          } else {
            Ext.Msg.alert('To Match Attributes', 'Drag one attribute to drop in drop zone');
          }
          return (true);
        }
      });
    },
    // params:
    // MasterGroupMappingID
    // MasterGroupMappingName
    getViewProductsWindow: function (params) {
      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        sortBy: 'VendorAssortmentID',
        autoLoadStore: true,
        url: Concentrator.route("GetListOfMatchedVendorProducts", "MasterGroupMapping"),
        forceFit: false,
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        structure: [{
          dataIndex: 'ConcentratorNumber',
          header: 'Concentrator #',
          type: 'int'
        }, {
          dataIndex: 'DistriItemNumber',
          header: 'Distri Item Number'
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name',
          type: 'string',
          width: 240
        }, {
          dataIndex: 'Brand',
          header: 'Brand',
          type: 'string'
        }, {
          dataIndex: 'ProductCode',
          header: 'Product Code',
          type: 'string'
        }, {
          dataIndex: 'Image',
          header: 'Image',
          type: 'boolean'
        }, {
          dataIndex: 'PossibleMatch',
          header: 'Possible Match',
          type: 'int',
          renderer: function (val, m, record) {
            return record.get('PossibleMatch') + " %";
          }
        }, {
          dataIndex: 'MasterGroupMappingPad',
          header: 'Master Group Mapping'
        }, {
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'ProductControle',
          header: 'Product Controle',
          type: 'boolean'
        }, {
          dataIndex: 'countVendorAssortment',
          header: '# Vendor Assortments',
          type: 'int'
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Cross References of Master Group Mapping"' + params.MasterGroupMappingName + '"',
        plain: true,
        items: grid,
        layout: 'fit',
        resizable: true,
        width: 1000,
        height: 500,
        renderTo: 'MasterGroupMappingCenterPanel',
        tbar: ["->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      window.show();
    },
    // params:
    // MasterGroupMappingID
    // AttributeID
    // AttributeName
    // AttributeValue
    // RelatedAttributeID
    getViewRelatedAttributesWindow: function (params) {
      var grid = new Diract.ui.Grid({
        singularObjectName: 'Related Attribute',
        pluralObjectName: 'Related Attributes',
        autoLoadStore: true,
        primaryKey: 'RelatedAttributeID',
        url: Concentrator.route("GetListOfCrossReferenceRelatedAttributes", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          RelatedAttributeID: params.RelatedAttributeID
        },
        structure: [{
          dataIndex: 'RelatedAttributeID',
          type: 'int'
        }, {
          dataIndex: 'AttributeName',
          header: 'Attribute Name'
        }, {
          dataIndex: 'AttributeValue',
          header: 'Attribute Value'
        }],
        customButtons: [{
          text: 'Delete All Related Attributes',
          iconCls: 'delete',
          handler: function () {
            var title = 'Delete Attribute Related Mapping "' + params.AttributeName + '"';
            var msg = 'Are you sure you want to delete Attribute Related Mapping "' + params.AttributeName + '" With Value (' + params.AttributeValue + ') ?';
            Ext.Msg.confirm(title, msg, function (button) {
              if (button == "yes") {
                Diract.request({
                  url: Concentrator.route('DeleteRelatedAttributeMapping', 'MasterGroupMapping'),
                  waitMsg: 'Deleting the attribute related mapping',
                  params: {
                    RelatedAttributeID: params.RelatedAttributeID
                  },
                  success: function () {
                    Ext.getCmp('RelatedAttributeRelationGrid').getStore().reload();
                    window.close();
                  }
                });
              }
            });
          }
        }, "->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }],
        rowActions: [{
          text: 'Delete Related Attribute',
          iconCls: 'delete',
          alignRight: false,
          handler: function (record) {
            var title = 'Delete Attribute Related "' + record.get('AttributeName') + '"';
            var msg = 'Are you sure you want to delete Attribute Related "' + record.get('AttributeName') + '" With Value (' + record.get('AttributeValue') + ') ?';
            Ext.Msg.confirm(title, msg, function (button) {
              if (button == "yes") {
                Diract.request({
                  url: Concentrator.route('DeleteCrossReferenceRelatedAttribute', 'MasterGroupMapping'),
                  waitMsg: 'Deleting the attribute related mapping',
                  params: {
                    id: record.get('RelatedAttributeID')
                  },
                  success: function () {
                    grid.getStore().remove(record);
                    Ext.getCmp('RelatedAttributeRelationGrid').getStore().reload();
                    if (grid.getStore().getCount() < 1) {
                      window.close();
                    }
                  }
                });
              }
            });
          }
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Related Attributes of Attribute "' + params.AttributeName + '" With Attribute Value (' + params.AttributeValue + ')',
        plain: true,
        items: grid,
        layout: 'fit',
        resizable: true,
        width: 500,
        height: 300,
        renderTo: 'MasterGroupMappingCenterPanel'
      });
      window.show();
    },
    // params:
    // MasterGroupMappingID
    // MasterGroupMappingAttributeID
    // MasterGroupMappingAttributeName
    // CrossReferenceID
    // CrossReferenceAttributeID
    // CrossReferenceAttributeName
    getCreateRelationWindow: function (params) {

      var listOfIDs = {
        MasterGroupMappingID: params.MasterGroupMappingID,
        AttributeID: params.MasterGroupMappingAttributeID,
        CrossReferenceAttributeID: params.CrossReferenceAttributeID,
        IsForMasterGroupMappingGrid: true,
        CrossReferenceID: params.CrossReferenceID
      };
      var listOfIDsJson = JSON.stringify(listOfIDs);
      var masterGroupMappingAttributeGrid = new Diract.ui.Grid({
        singularObjectName: 'Related Attribute',
        pluralObjectName: 'Related Attributes',
        region: 'center',
        title: 'Attribute "' + params.MasterGroupMappingAttributeName + '"',
        autoLoadStore: true,
        forceFit: true,
        hideBottomBar: true,
        url: Concentrator.route("GetListOfRelatedAttributeValues", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          listOfIDsJson: listOfIDsJson
        },
        listeners: {
          'rowclick': function (grid, index) {
            //clear all
            crossReferenceAttributeGrid.selModel.clearSelections();
            var rec = grid.store.getAt(index);
            var value = rec.json.mappedToValue;
            if (value != null) {
              //select value from crosslist
              for (var i = 0; i < value.length; i++) {
                var attVal = value[i].AttributeValue;
                crossReferenceAttributeGrid.selModel.selectRow(crossReferenceAttributeGrid.store.find('Value', attVal), true);
              }
            }
          }
        },
        structure: [{
          dataIndex: 'AttributeValueID',
          type: 'int'
        }, {
          dataIndex: 'Selected',
          header: ' ',
          type: 'boolean',
          editable: true
        }, {
          dataIndex: 'Value',
          header: 'Attribute Value'
        }, {
          dataIndex: 'IsAttributeMapped',
          header: 'Is Mapped',
          renderer: function (val, metadata, record) {
            if (val) {
              return '<div class="rowAcceptIcon"/>';
            } else {
              return '';
            }
          }
        }]
      });

      var listOfIDs = {
        MasterGroupMappingID: params.MasterGroupMappingID,
        CrossReferenceID: params.CrossReferenceID,
        MasterGroupMappingAttributeID: params.MasterGroupMappingAttributeID,
        AttributeID: params.CrossReferenceAttributeID,
        IsForCrossReferenceGrid: true
      };
      var listOfIDsJson = JSON.stringify(listOfIDs);
      var crossReferenceAttributeGrid = new Diract.ui.Grid({
        singularObjectName: 'Related Attribute',
        pluralObjectName: 'Related Attributes',
        region: 'center',
        title: 'Attribute "' + params.CrossReferenceAttributeName + '"',
        autoLoadStore: true,
        hideBottomBar: true,
        url: Concentrator.route("GetListOfRelatedAttributeValues", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        params: {
          listOfIDsJson: listOfIDsJson
        },
        structure: [{
          dataIndex: 'AttributeValueID',
          type: 'int'
        }, {
          dataIndex: 'Selected',
          header: ' ',
          type: 'boolean',
          editable: true
        }, {
          dataIndex: 'Value',
          header: 'Attribute Value'
        }, {
          dataIndex: 'IsAttributeMapped',
          header: 'Is Mapped',
          renderer: function (val, metadata, record) {
            if (val) {
              return '<div class="rowAcceptIcon"/>';
            } else {
              return '';
            }
          }
        }]
      });

      var addAttributeToGrid = function (textField, grid) {
        if (textField.getValue() && textField.getValue().trim().length > 0) {
          var isAttriubteValueExists = false;
          var rowIndex = 0;
          var store = grid.getStore();
          var attributeValue = textField.getValue();
          grid.getSelectionModel().clearSelections();
          Ext.each(store.data.items, function (storeRecord) {
            if (attributeValue == storeRecord.get('Value')) {
              isAttriubteValueExists = true;
              rowIndex = store.indexOf(storeRecord);
              grid.getSelectionModel().selectRow(rowIndex, true);
            }
          });

          if (!isAttriubteValueExists) {
            var fields = store.fields.items;
            var attribute = Ext.data.Record.create([fields]);
            var record = new attribute({
              AttributeValueID: 1,
              Selected: true,
              Value: attributeValue
            });
            store.insert(0, record);
            textField.reset();
          }
          grid.getView().getRow(rowIndex).scrollIntoView();
        }
      };
      var saveAttributes = function (closeWindow) {
        var ListOfAttributeValues = {
          MasterGroupMappingID: params.MasterGroupMappingID,
          MasterGroupMappingAttributeID: params.MasterGroupMappingAttributeID,
          MasterGroupMappingAttributeValues: [],
          CrossReferenceID: params.CrossReferenceID,
          CrossReferenceAttributeID: params.CrossReferenceAttributeID,
          CrossReferenceAttributeValues: []
        };
        Ext.each(masterGroupMappingAttributeGrid.getStore().getModifiedRecords(), function (record) {
          if (record.get('Selected')) {
            ListOfAttributeValues.MasterGroupMappingAttributeValues.push(record.get('Value'));
          }
        });
        Ext.each(crossReferenceAttributeGrid.getStore().getModifiedRecords(), function (record) {
          if (record.get('Selected')) {
            ListOfAttributeValues.CrossReferenceAttributeValues.push(record.get('Value'));
          }
        });
        if (ListOfAttributeValues.MasterGroupMappingAttributeValues.length > 0 && ListOfAttributeValues.CrossReferenceAttributeValues.length > 0) {
          var ListOfAttributeValuesJson = JSON.stringify(ListOfAttributeValues);

          Diract.request({
            url: Concentrator.route('CreatRelatedAttribute', 'MasterGroupMapping'),
            waitMsg: 'Waiting',
            params: {
              ListOfAttributeValuesJson: ListOfAttributeValuesJson
            },
            success: function () {
              Ext.getCmp('RelatedAttributeRelationGrid').getStore().reload();
              Ext.getCmp('southPanelDropPanel').getForm().reset();
              if (closeWindow) {
                window.close();
              } else {
                Ext.each(masterGroupMappingAttributeGrid.getStore().getModifiedRecords(), function (record) {
                  record.set('Selected', false);
                });
                Ext.each(crossReferenceAttributeGrid.getStore().getModifiedRecords(), function (record) {
                  record.set('Selected', false);
                });
              }
            }
          });
        } else {
          Ext.Msg.alert('Fail to create relation', 'Choose two attributes');
        }
      };

      var masterGroupMappingAttributeTextField = new Ext.form.TextField({
        fieldLabel: 'Value',
        emptyText: 'Master Group Mapping Attribute Value',
        region: 'center',
        name: 'MasterGroupMappingAttributeValue',
        margins: '0 1 0 0',
        height: 25,
        enableKeyEvents: true,
        listeners: {
          keypress: function (t, e) {
            if (e.getKey() == 13) {
              addAttributeToGrid(this, masterGroupMappingAttributeGrid);
            }
          }
        }
      });
      var crossReferenceAttributeTextField = new Ext.form.TextField({
        fieldLabel: 'Value',
        emptyText: 'Cross Reference Attribute Value',
        region: 'center',
        name: 'CrossReferenceAttributeValue',
        margins: '0 1 0 0',
        height: 25,
        enableKeyEvents: true,
        listeners: {
          keypress: function (t, e) {
            if (e.getKey() == 13) {
              addAttributeToGrid(this, crossReferenceAttributeGrid);
            }
          }
        }
      });

      var btnAddAttributeValueToMGMGrid = new Ext.Button({
        region: 'east',
        text: '',
        iconCls: 'add',
        handler: function () {
          addAttributeToGrid(masterGroupMappingAttributeTextField, masterGroupMappingAttributeGrid);
        }
      });
      var btnAddAttributeValueToCRGrid = new Ext.Button({
        region: 'east',
        text: '',
        iconCls: 'add',
        handler: function () {
          addAttributeToGrid(crossReferenceAttributeTextField, crossReferenceAttributeGrid);
        }
      });

      var southPanelOfWestPanel = new Ext.Panel({
        region: 'south',
        layout: 'border',
        margins: '1 1 1 1',
        width: 300,
        height: 27,
        items: [masterGroupMappingAttributeTextField, btnAddAttributeValueToMGMGrid]
      });
      var southPanelOfCenterPanel = new Ext.Panel({
        region: 'south',
        layout: 'border',
        margins: '1 1 1 1',
        width: 300,
        height: 27,
        items: [crossReferenceAttributeTextField, btnAddAttributeValueToCRGrid]
      });

      var westPanel = new Ext.Panel({
        region: 'west',
        layout: 'border',
        margins: '4 4 4 4',
        width: 300,
        items: [masterGroupMappingAttributeGrid, southPanelOfWestPanel]
      });
      var centerPanel = new Ext.Panel({
        region: 'center',
        layout: 'fit',
        plain: true,
        border: false
      });
      var eastPanel = new Ext.Panel({
        region: 'east',
        layout: 'border',
        margins: '4 4 4 4',
        width: 300,
        items: [crossReferenceAttributeGrid, southPanelOfCenterPanel]
      });

      var window = new Ext.Window({
        title: 'Choose Attribute Value',
        modal: true,
        plain: true,
        resizable: true,
        layout: 'border',
        width: 630,
        height: 400,
        items: [westPanel, centerPanel, eastPanel],
        tbar: ["->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }],
        buttons: [{
          text: 'Save and Next',
          iconCls: 'save',
          handler: function () {
            saveAttributes(false);
          }
        }, {
          text: 'Save and Exit',
          iconCls: 'save',
          handler: function () {
            saveAttributes(true);
          }
        }]
      });
      window.show();
    },
    // params
    // MasterGroupMappingID
    // MasterGroupMappingName
    getRelatedObjectsToMasterGroupMapping: function (params, node) {
      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Related Objects',
        singularObjectName: 'Related Object',
        autoLoadStore: true,
        params: {
          MasterGroupMappingID: params.MasterGroupMappingID
        },
        url: Concentrator.route("GetListOfRelatedObjectsToMasterGroupMapping", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [{
          dataIndex: 'Info',
          pluginType: 'rowexpander',
          tpl: new Ext.XTemplate(
						'<tpl for="Info">',
							'<p>{#}. {.}</p>',
						'</tpl>'
					)
        }, {
          dataIndex: 'ErrorMessage',
          header: 'Name'
        }]
      });
      var window = new Ext.Window({
        title: 'Related Objects to Master Group Mapping "' + params.MasterGroupMappingName + '"',
        modal: true,
        plain: true,
        resizable: true,
        layout: 'fit',
        width: 600,
        height: 400,
        renderTo: 'MasterGroupMappingCenterPanel',
        items: grid,
        tbar: [{
          text: 'Delete All And Continue',
          iconCls: 'delete',
          handler: function () {
            Ext.Msg.show({
              title: 'UnCross Reference',
              waitMsg: 'Deleting All Related To ' + params.MasterGroupMappingName + ' ..Please Wait..',
              msg: 'Would you like to delete all references from Master Group Mapping "' + params.MasterGroupMappingName + '" ?',
              buttons: { ok: "Yes", cancel: "No" },
              fn: function (result) {
                if (result == 'ok') {
                  Diract.request({
                    url: Concentrator.route('UnCrossReferenceAllOfMasterGroupMapping', 'MasterGroupMapping'),
                    params: {
                      MasterGroupMappingID: params.MasterGroupMappingID,
                      Delete: true
                    },
                    success: function () {
                      var panels = {
                        allTabPanels: true,
                        treePanel: true,
                        node: node.parentNode
                      };
                      node.remove();
                      Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                      window.close();
                    }
                  });
                } else {
                };
              },
              animEl: 'elId',
              icon: Ext.MessageBox.QUESTION
            });
          }
        }, "->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      window.show();
    },
    getRelatedMasterGroupMappingsWindow: function (params) {
      var self = this;

      var relatedMasterGroupMappingContextMenu = new Ext.menu.Menu({
        items: [{
          identity: 'FindMasterGroupMapping',
          text: 'Find Master Group Mapping',
          iconCls: 'view'
        }, {
          identity: 'FindRelatedMasterGroupMapping',
          text: 'Find Related Master Group Mapping',
          iconCls: 'view'
        }],
        listeners: {
          itemclick: function (item) {

            var selections = grid.getSelectionModel().getSelections();
            var selectedRecord = grid.getSelectionModel().getSelected();

            if (selectedRecord) {
              switch (item.identity) {
                case 'FindMasterGroupMapping':
                  var ID = selectedRecord.get('MasterGroupMappingID');
                  if (ID) Concentrator.MasterGroupMappingFunctions.showTreeNodeByMasterGroupMappingID(ID);
                  break;
                case 'FindRelatedMasterGroupMapping':
                  var ID = selectedRecord.get('RelatedMasterGroupMappingID');
                  if (ID) Concentrator.MasterGroupMappingFunctions.showTreeNodeByMasterGroupMappingID(ID);
                  break;
              }
            }
          }
        }
      });

      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Related Master Group Mappings',
        singularObjectName: 'Related Master Group Mapping',
        sortBy: 'RelatedID',
        autoLoadStore: true,
        groupField: 'MasterGroupMappingName',
        hideGroupedColumn: 'MasterGroupMappingName',
        useHeaderFilters: true,
        sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
        url: Concentrator.route("GetListOfRelatedMasterGroupMappings", "MasterGroupMapping"),
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [
          {
            dataIndex: 'RelatedID',
            type: 'int'
          }, {
            dataIndex: 'MasterGroupMappingID',
            type: 'int'
          }, {
            dataIndex: 'RelatedMasterGroupMappingID',
            type: 'int'
          },
        {
          dataIndex: 'MasterGroupMappingName',
          type: 'string',
          header: 'Master Group Mapping Name'
        },
        {
          dataIndex: 'RelatedMasterGroupMappingName',
          type: 'string',
          header: 'Related Master Group Mapping Name'
        },
        {
          dataIndex: 'QuantityLevel',
          type: 'string',
          header: 'Quantity Level'
        },
        {
          dataIndex: 'RelatedType',
          type: 'string',
          header: 'Related Type'
        },
        {
          dataIndex: 'action',
          type: 'string',
          header: 'Action',
          // fixed: true,
          filterable: false,
          width: 45,
          renderer: function (a, b, c) {
            return '<div   ext:qtip="Delete" class="x-btn-text remove" ></div> <div ext:qtip="Find Master Group Mappings" class="x-btn-text view-icon mm" ></div>';
          }
        }],
        listeners: {
          rowcontextmenu: function (grid, index, event) {

            grid.getSelectionModel().clearSelections();
            var selections = grid.getSelectionModel().getSelections().length;
            if (selections >= 1) {
              grid.getSelectionModel().selectRows(selections);
            } else {
              var rec = grid.store.getAt(index);
              grid.getSelectionModel().selectRecords([rec]);
            }
            var record = grid.getSelectionModel().getSelected();

            event.stopEvent();
            relatedMasterGroupMappingContextMenu.showAt(event.xy);
          },
          'rowclick': function (t, rowIndex, e) {
            var record = t.getSelectionModel().getSelected();
            if (e.target.className == "x-btn-text remove") {
              Ext.Msg.show({
                title: 'Delete Related Master Group Mapping',
                msg: "Are you sure you want to delete this mapping?",
                buttons: Ext.Msg.OKCANCEL,
                fn: function (result) {
                  if (result == 'ok') {
                    Diract.request({
                      url: Concentrator.route('DeleteRelatedMasterGroupMapping', 'MasterGroupMapping'),
                      waitMsg: "Please wait..",
                      params: {
                        relatedID: record.get('RelatedID')
                      },
                      success: function () {
                        grid.getStore().load();
                      }
                    });
                  }
                },
                animEl: 'elId',
                icon: Ext.MessageBox.QUESTION
              });
            }
            if (e.target.className == "x-btn-text view-icon mm") {
              var ids = [];

              var MasterGroupMappingID = record.get('MasterGroupMappingID');
              if (MasterGroupMappingID)
                ids.push(MasterGroupMappingID);

              var RelatedMasterGroupMappingID = record.get('RelatedMasterGroupMappingID');
              if (RelatedMasterGroupMappingID)
                ids.push(RelatedMasterGroupMappingID);

              Concentrator.MasterGroupMappingFunctions.showTreeNodesByMasterGroupMappingID(ids);
            }
            if (e.target.className == "x-btn-text view-icon rmm") {
              var ID = record.get('RelatedMasterGroupMappingID');
              if (ID) Concentrator.MasterGroupMappingFunctions.showTreeNodeByMasterGroupMappingID(ID);
            }
          }
        }
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Related Master Group Mappings',
        plain: true,
        items: grid,
        layout: 'fit',
        resizable: false,
        width: 600,
        height: 400,
        renderTo: 'MasterGroupMappingCenterPanel'
      });
      window.show();


      Ext.QuickTips.interceptTitles = true;
      Ext.QuickTips.init();
    },

    refreshTreeNode: function (node) {
      var hasParent = true;
      var tempNode = node;
      var listOfIDs = [];
      do {
        if (tempNode.parentNode) {
          listOfIDs.push(tempNode.attributes.MasterGroupMappingID);
          tempNode = tempNode.parentNode;
        } else {
          tempNode.reload();
          hasParent = false;
        }
      } while (hasParent);

      listOfIDs.reverse();
      var counter = 0;
      var path = listOfIDs;
      var timer = setInterval(function () {
        if (path[counter]) {
          if (tempNode.isExpanded()) {
            tempNode = tempNode.findChild('MasterGroupMappingID', path[counter]);
            counter++;
          } else {
            tempNode.expand();
          }
        } else {
          clearInterval(timer);
        }
      }, 250);
    },
    showTreeNodeByMasterGroupMappingID: function (masterGroupMappingID) {
      Diract.silent_request({
        url: Concentrator.route('GetTreeNodePathByMasterGroupMappingID', 'MasterGroupMapping'),
        params: {
          MasterGroupMappingID: masterGroupMappingID
        },
        success: function (result) {
          var tree = Ext.getCmp('masterGroupMappingTreePanel');
          var rootNode = tree.getRootNode();

          var counter = 0;
          var path = result.results;
          var tempNode = rootNode;
          var timer = setInterval(function () {
            if (path[counter]) {
              if (tempNode.isExpanded()) {
                tempNode = tempNode.findChild('MasterGroupMappingID', path[counter]);
                counter++;
              } else {
                tempNode.expand();
                tempNode.select();
              }
            } else {
              tempNode.select();
              clearInterval(timer);
            }
          }, 250);
        }
      });
    },
    showTreeNodesByMasterGroupMappingID: function (masterGroupMappingIDs) {
      if (masterGroupMappingIDs && masterGroupMappingIDs.length > 0) {
        var ListOfMasterGroupMappingIDs = { MasterGroupMappingIDs: masterGroupMappingIDs };
        var ListOfMasterGroupMappingIDs = JSON.stringify(ListOfMasterGroupMappingIDs);
        Diract.silent_request({
          url: Concentrator.route('GetTreeNodesPathByMasterGroupMappingID', 'MasterGroupMapping'),
          params: {
            MasterGroupMappingIDsJson: ListOfMasterGroupMappingIDs
          },
          success: function (result) {

            var pathes = result.results;
            var counter = 0;
            var valueCounter = 0;
            var getNextPath = true;
            var path;
            var tree = Ext.getCmp('masterGroupMappingTreePanel');
            var tempNode;

            if (pathes.length > 0) {
              var timer = setInterval(function () {
                if (pathes.length == counter) {
                  clearInterval(timer);
                } else {
                  if (getNextPath) {
                    path = pathes[counter];
                    getNextPath = false;
                    tempNode = tree.getRootNode();
                  } else {
                    if (tempNode) {
                      if (path.Value.length == valueCounter) {
                        valueCounter = 0;
                        counter++;
                        getNextPath = true;
                        // todo: testen
                        tempNode.expand();
                        // end todo
                        tree.getSelectionModel().select(tempNode, false, true);
                      } else {
                        if (tempNode.isExpanded()) {
                          tempNode = tempNode.findChild('MasterGroupMappingID', path.Value[valueCounter]);
                          valueCounter++;
                        } else {
                          tempNode.expand();
                        }
                      }
                    } else {
                      clearInterval(timer);
                    }
                  }
                }
              }, 125);
            } else {
            }
          }
        });

      }
    },
    refreshTreeNodeByMasterGroupMappingID: function (masterGroupMappingID) {
      Diract.silent_request({
        url: Concentrator.route('GetTreeNodePathByMasterGroupMappingID', 'MasterGroupMapping'),
        params: {
          MasterGroupMappingID: masterGroupMappingID
        },
        success: function (result) {

          var tree = Ext.getCmp('masterGroupMappingTreePanel');
          rootNode = tree.getRootNode();
          rootNode.reload();

          var counter = 0;
          var path = result.results;
          tempNode = rootNode;
          var timer = setInterval(function () {
            if (path[counter]) {
              if (tempNode) {
                if (tempNode.isExpanded()) {
                  tempNode = tempNode.findChild('MasterGroupMappingID', path[counter]);
                  counter++;
                } else {
                  tempNode.expand();
                }
              } else {
                clearInterval(timer);
              }
            } else {
              tempNode.select();
              tempNode.expand();
              clearInterval(timer);
            }
          }, 250);
        }
      });
    },
    // Params
    // treePanel = true -> Refresh Tree
    //    node -> Refresh a tree node
    //    MasterGroupMappingID -> Refresh a tree node by master group mapping id
    //    MasterGroupMappings -> Refresh more than one tree node by master group mapping id
    // vendorProductGroupsTabPanel = true -> Refresh Vendor Product Group Grid
    // vendorProductsTabPanel = true -> Refresh Vendor Products Grid
    // allTabPanels = True 
    // ProductGrid
    // connectorTreePanel
    refreshAction: function (refresh) {
      if (refresh.treePanel && refresh.treePanel == true) {
        if (refresh.node) {
          Ext.getCmp('masterGroupMappingTreePanel').refreshNode(refresh.node);
        } else {
          if (refresh.MasterGroupMappingID) {
            Concentrator.MasterGroupMappingFunctions.refreshTreeNodeByMasterGroupMappingID(refresh.MasterGroupMappingID);
          } else {
            Ext.getCmp('masterGroupMappingTreePanel').getRootNode().reload();
            Concentrator.MasterGroupMappingFunctions.showTreeNodesByMasterGroupMappingID(refresh.MasterGroupMappings);
          }
        }
      }

      if (refresh.connectorTreePanel && refresh.connectorTreePanel == true) {
        Ext.getCmp('connectorMappingTreePanel').getRootNode().reload();
      }

      if (refresh.ProductGrid && refresh.ProductGrid == true) {
        var grid = Ext.getCmp('VendorProductsPanel');
        grid.ownerCt.filterGrid();
      }

      if (refresh.vendorProductGroupsTabPanel && refresh.vendorProductGroupsTabPanel == true) {
        Ext.getCmp('VendorProductGroupsPanel').store.reload();
      }

      if (refresh.matchedVendorProductGroupsTabPanel && refresh.matchedVendorProductGroupsTabPanel == true) {
        //Ext.getCmp('MatchedProductGroup').store.reload();
      }

      if (refresh.vendorProductsTabPanel && refresh.vendorProductsTabPanel == true) {
        Ext.getCmp('VendorProductsPanel').store.reload();
      }

      if (refresh.matchedVendorProductsTabPanel && refresh.matchedVendorProductsTabPanel == true) {
        //Ext.getCmp('VendorProductPanel').store.reload();
      }

      if (refresh.allTabPanels && refresh.allTabPanels == true) {
        Ext.getCmp('VendorProductGroupsPanel').store.reload();
        //Ext.getCmp('MatchedProductGroup').store.reload();
        Ext.getCmp('VendorProductsPanel').store.reload();
        //Ext.getCmp('VendorProductPanel').store.reload();
      }
    },

    /// <summary>
    /// Refresh Connector Mapping Tree
    /// </summary>
    /// <param> MasterGroupMappingIDs, RootNode </param>
    /// <returns>  </returns>
    refreshTree: function (obj) {
      config = {
        MasterGroupMappingIDs: [],
        RootNode: null
      };
      Ext.apply(config, obj);

      if (config.MasterGroupMappingIDs.length > 0) {
        if (config.MasterGroupMappingIDs.length > 1) {
          Concentrator.MasterGroupMappingFunctions.refreshTreeByMasterGroupMappingIDs(config.MasterGroupMappingIDs, config.RootNode);
        } else {
          Concentrator.MasterGroupMappingFunctions.refreshTreeByMasterGroupMappingID(config.MasterGroupMappingIDs[0], config.RootNode);
        }
      }
    },
    refreshTreeByMasterGroupMappingID: function (MasterGroupMappingID, RootNode) {
      RootNode.reload();
      var timer = setInterval(function () {
        if (RootNode.isExpanded()) {
          clearInterval(timer);
          Diract.silent_request({
            url: Concentrator.route('GetTreeNodePathByMasterGroupMappingID', 'MasterGroupMapping'),
            params: {
              MasterGroupMappingID: MasterGroupMappingID
            },
            success: function (result) {
              var counter = 0;
              var path = result.results;
              tempNode = RootNode;
              var timer = setInterval(function () {
                if (tempNode) {
                  if (path[counter]) {
                    if (tempNode.isExpanded()) {
                      tempNode = tempNode.findChild('MasterGroupMappingID', path[counter]);
                      counter++;
                    } else {
                      tempNode.expand();
                      tempNode.select();
                    }
                  } else {
                    tempNode.select();
                    clearInterval(timer);
                  }
                } else {
                  clearInterval(timer);
                }
              }, 125);
            }
          });
        }
      }, 125);
    },
    refreshTreeByMasterGroupMappingIDs: function (MasterGroupMappingIDs, RootNode) {
      RootNode.reload();
      var timer = setInterval(function () {
        if (RootNode.isExpanded()) {
          clearInterval(timer);

          var ListOfMasterGroupMappingIDs = { MasterGroupMappingIDs: MasterGroupMappingIDs };
          var ListOfMasterGroupMappingIDs = JSON.stringify(ListOfMasterGroupMappingIDs);
          Diract.silent_request({
            url: Concentrator.route('GetTreeNodesPathByMasterGroupMappingID', 'MasterGroupMapping'),
            params: {
              MasterGroupMappingIDsJson: ListOfMasterGroupMappingIDs
            },
            success: function (result) {

              var pathes = result.results;
              var pathCounter = 0;
              var valueCounter = 0;
              var path;
              var tempNode = RootNode;

              var timer = setInterval(function () {
                if (tempNode) {
                  if (path) {
                    if (tempNode.isExpanded()) {
                      if (valueCounter >= path.Value.length) {
                        path = null;
                        RootNode.ownerTree.getSelectionModel().select(tempNode, false, true);
                        tempNode = RootNode;
                      } else {
                        tempNode = tempNode.findChild('MasterGroupMappingID', path.Value[valueCounter]);
                        valueCounter++;
                      }
                    } else {
                      tempNode.expand();
                    }
                  } else {
                    if (pathCounter >= pathes.length) {
                      clearInterval(timer);
                    } else {
                      path = pathes[pathCounter];
                      pathCounter++;
                      valueCounter = 0;
                    }
                  }
                } else {
                  clearInterval(timer);
                }
              }, 125);
            }
          });
        }
      }, 125);
    },
    refreshVendorProductsGrid: function (MasterGroupMappingIDs) {
      var vendorProductsGrid = Ext.getCmp('VendorProductsPanel').ownerCt;
      if (MasterGroupMappingIDs) {
        Ext.each(vendorProductsGrid.filterItems, function (filterItem) {
          if (filterItem.name) {
            if (filterItem.getName() == 'MasterGroupMappingID') {
              filterItem.setValue(MasterGroupMappingIDs);
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
      }
      vendorProductsGrid.filterGrid();
    },


    /// <summary>
    /// Add Master Group Mapping/Connector Mapping
    /// </summary>
    /// <param> MasterGroupMappingID, MasterGroupMappingName, [ConnectorID] </param>
    /// <returns> Window </returns>
    getAddNewMasterGroupMappingWindow: function (config) {
      var functions = {
        getTranslation: function (translationComponent, record) {
          switch (translationComponent) {
            case 'mainWindow':
              if (config.ConnectorID) {
                return 'Add New Connector Mapping to Connector Mapping "' + config.MasterGroupMappingName + '"';
              } else {
                return 'Add New Master Group Mapping to Master Group Mapping "' + config.MasterGroupMappingName + '"';
              }
              break;
            case 'addNewMappingButton':
              if (config.ConnectorID) {
                return 'Add New Connector Mapping';
              } else {
                return 'Add New Master Group Mapping';
              }
              break;
            case 'addNewMappingAndExitButton':
              if (config.ConnectorID) {
                return 'Add New Connector Mapping and Back to Connector Mapping';
              } else {
                return 'Add New Master Group Mapping and Back to Master Group Mapping';
              }
              break;
            case 'addSelectedMappingButton':
              if (config.ConnectorID) {
                return 'Add Selected Name To Connector Mapping';
              } else {
                return 'Add Selected Name To Master Group Mapping';
              }
              break;
            case 'addNewMappingWindow':
              if (config.ConnectorID) {
                return 'Add New Connector Mapping to Connector Mapping "' + config.MasterGroupMappingName + '"';
              } else {
                return 'Add New Master Group Mapping to Master Group Mapping "' + config.MasterGroupMappingName + '"';
              }
              break;
            case 'confirmMessageBoxMsg':
              if (config.ConnectorID) {
                return 'Are you sure you want to add Translation "' + record.get('GroupName') + '" to Connector Mapping "' + config.MasterGroupMappingName + '"?';
              } else {
                return 'Are you sure you want to add Translation "' + record.get('GroupName') + '" to Master Group Mapping "' + config.MasterGroupMappingName + '"?';
              }
              break;
            case 'confirmMessageBoxWaitMsg':
              if (config.ConnectorID) {
                return 'Adding Selected Record To Connector Mapping';
              } else {
                return 'Adding Selected Record To Master Group Mapping';
              }
              break;
          };
        },
        addNewMasterGroupMappingWindow: function () {
          var addGrid = new Diract.ui.Grid({
            url: Concentrator.route('GetList', 'Language'),
            primaryKey: 'ID',
            permissions: {
              list: 'GetLanguage',
              create: 'DefaultMasterGroupMapping',
              remove: 'DefaultMasterGroupMapping',
              update: 'DefaultMasterGroupMapping'
            },
            structure: [{
              dataIndex: 'ID',
              type: 'int'
            }, {
              dataIndex: 'Name',
              header: 'Language'
            }, {
              dataIndex: 'LanguageValue',
              header: 'Name',
              editor: {
                xtype: 'textfield'
              }
            }, {
              dataIndex: 'LanguageMustBeFilled',
              type: 'boolean',
              editable: true,
              header: 'Translation Correct?'
            }],
            customButtons: [{
              text: functions.getTranslation('addNewMappingButton'),
              iconCls: 'add',
              handler: function () {
                functions.saveGridSelections(addGrid, window);
              }
            }, {
              text: functions.getTranslation('addNewMappingAndExitButton'),
              iconCls: 'add',
              handler: function () {
                functions.saveGridSelections(addGrid, window, true);
              }
            }, "->", {
              text: 'Exit',
              iconCls: 'exit',
              handler: function () {
                window.close();
              }
            }]
          });

          addGrid.getStore().addListener('load', function (thisStore) {
            var data = { Name: 'All Languages' };

            var record = new thisStore.recordType(data, -1);
            thisStore.insert(0, record);

          });

          addGrid.getStore().addListener('update', function (thisStore, rec) {
            if (rec.get('Name') == 'All Languages') {
              thisStore.each(function (langRec) {
                langRec.set('LanguageValue', rec.get('LanguageValue'));
              });
            }
          });

          var window = new Ext.Window({
            modal: true,
            title: functions.getTranslation('addNewMappingWindow'),
            plain: true,
            layout: 'fit',
            resizable: true,
            width: 700,
            height: 500,
            closable: false,
            items: addGrid
          });
          window.show();
        },
        deletGridSelections: function () {
          var ListOfProductGroups = { ProductGroupIDs: [] };
          var records = grid.grid.getSelectionModel().getSelections();
          if (records.length > 0) {
            Ext.each(records, function (record) {
              if (!record.get('IsMasterGroupMapping')) {
                ListOfProductGroups.ProductGroupIDs.push(record.get('ID'));
              }
            });
            Ext.each(ListOfProductGroups.ProductGroupIDs, function (ProductGroupID) {
              Diract.request({
                url: Concentrator.route('Delete', 'ProductGroup'),
                waitMsg: 'Deleting Product Group',
                params: {
                  id: ProductGroupID
                },
                success: function () {
                  grid.grid.getStore().reload();
                }
              });
            });
          } else {
            Ext.Msg.alert('Warning', 'Select At Least One Record In Grid To Delete.');
          }
        },
        saveGridSelections: function (thisGrid, thisWindow, backToMasterGroupMapping) {
          var ListOfLanguageIDsAndValues = {
            LanguageIDs: [],
            ParentMasterGroupMappingID: config.MasterGroupMappingID,
            ConnectorID: config.ConnectorID ? config.ConnectorID : 0
          };
          var records = thisGrid.getStore().getModifiedRecords();
          Ext.each(records, function (record) {
            if (record.get('ID') > -1) {
              var value = {};
              value.LanguageID = record.get('ID');
              value.Value = record.get('LanguageValue');
              value.LanguageMustBeFilled = record.get('LanguageMustBeFilled') ? true : false;
              ListOfLanguageIDsAndValues.LanguageIDs.push(value);
            }
          });
          var ListOfLanguageIDsAndValues = JSON.stringify(ListOfLanguageIDsAndValues);
          Diract.request({
            url: Concentrator.route('CreateMasterGroupMapping', 'MasterGroupMapping'),
            waitMsg: 'Saving Master Group Mapping',
            params: {
              ListOfLanguageIDsAndValuesJson: ListOfLanguageIDsAndValues
            },
            success: function () {
              thisWindow.close();
              if (backToMasterGroupMapping) {
                var addMasterGroupMappingWindow = Ext.getCmp('addMasterGroupMappingWindow');
                addMasterGroupMappingWindow.close();
              }
              var panels;
              if (config.ConnectorID) {
                panels = {
                  connectorTreePanel: true
                };
              } else {
                panels = {
                  treePanel: true,
                  MasterGroupMappingID: config.MasterGroupMappingID
                };
              }

              Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
            }
          });
        },
        addSelectedRecordToMasterGroupMapping: function (record) {
          Ext.Msg.show({
            title: 'Add Mapping?',
            msg: functions.getTranslation('confirmMessageBoxMsg', record),
            buttons: Ext.Msg.OKCANCEL,
            fn: function (result) {
              if (result == 'ok') {
                var recordModel = {
                  ID: record.get('ID'),
                  IsMasterGroupMapping: record.get('IsMasterGroupMapping'),
                  ParentMasterGroupMappingID: config.MasterGroupMappingID,
                  ConnectorID: config.ConnectorID ? config.ConnectorID : 0
                };
                var recordJson = JSON.stringify(recordModel);
                Diract.request({
                  url: Concentrator.route('AddSelectedRecordToMasterGroupMapping', 'MasterGroupMapping'),
                  waitMsg: functions.getTranslation('confirmMessageBoxWaitMsg'),
                  params: {
                    RecordJson: recordJson
                  },
                  success: function () {
                    var panels;
                    if (config.ConnectorID) {
                      panels = {
                        connectorTreePanel: true
                      };
                    } else {
                      panels = {
                        treePanel: true,
                        MasterGroupMappingID: config.MasterGroupMappingID
                      };
                    }
                    Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
                  }
                });
              }
            },
            animEl: 'elId',
            icon: Ext.MessageBox.QUESTION
          });
        }
      }
      var grid = new Diract.ui.FilterGridPanel({
        pluralObjectName: 'Group Names',
        singularObjectName: 'Group Name',
        url: Concentrator.route("GetListOfGroupNames", "MasterGroupMapping"),
        autoLoadStore: true,
        forceFit: true,
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        filterPanelConfig: {
          collapsible: false
        },
        filterItems: [new Ext.form.TextField({
          xtype: 'textfield',
          fieldLabel: 'Search Group Name',
          name: 'filterGroupName'
        })],
        structure: [{
          dataIndex: 'LanguageTranslation',
          pluginType: 'rowexpander',
          tpl: new Ext.XTemplate(
						'<tpl for="LanguageTranslation">',
							' <p>  - {.} </p>',
						'</tpl>'
					)
        }, {
          dataIndex: 'ID',
          type: 'int'
        }, {
          dataIndex: 'IsMasterGroupMapping',
          type: 'bool'
        }, {
          dataIndex: 'GroupName',
          header: 'Group Name'
        }],
        customButtons: [{
          text: functions.getTranslation('addNewMappingButton'),
          iconCls: 'add',
          handler: function () {
            functions.addNewMasterGroupMappingWindow();
          }
        }, {
          text: 'Delete Product Group',
          iconCls: 'delete',
          handler: function () {
            functions.deletGridSelections();
          }
        }],
        rowActions: ['->', {
          text: functions.getTranslation('addSelectedMappingButton'),
          iconCls: 'add',
          handler: function (record) {
            functions.addSelectedRecordToMasterGroupMapping(record);
          }
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: functions.getTranslation('mainWindow'),
        id: 'addMasterGroupMappingWindow',
        plain: true,
        layout: 'fit',
        resizable: true,
        width: 700,
        height: 500,
        closable: false,
        items: grid,
        tbar: ["->", {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      window.show();
    },
    /// <summary>
    /// Rename Master Group Mapping/Connector Mapping
    /// </summary>
    /// <param> >MasterGroupMappingID, MasterGroupMappingName, treePanelToRefresh, [languageWizardGrid] </param>
    /// <returns> Window </returns>
    getRenameMasterGroupMappingWindow: function (config) {
      var functions = {
        createSaveButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Save changes',
            iconCls: "save",
            disabled: true,
            handler: function () {
              functions.saveChanges();
            }
          });
        },
        createSaveAndExitButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Save changes and Exit',
            iconCls: "save",
            disabled: true,
            handler: function () {
              functions.saveChanges(true);
            }
          });
        },
        createDiscardButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Discard changes',
            iconCls: "delete",
            disabled: true,
            handler: function () {
              grid.getStore().rejectChanges();
              functions.setToolbarButtons(true);
            }
          });
        },
        createExitButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Exit',
            iconCls: "exit",
            handler: function () {
              window.close();
            }
          });
        },
        getButtons: function () {
          listOfButtons = [];
          listOfButtons.push(btnSave);
          listOfButtons.push(btnSaveAndExit);
          listOfButtons.push(btnDiscard);
          listOfButtons.push("->");
          listOfButtons.push(btnExit);
          return listOfButtons;
        },
        setToolbarButtons: function (disabled) {
          btnSave.setDisabled(disabled)
          btnSaveAndExit.setDisabled(disabled)
          btnDiscard.setDisabled(disabled)
        },
        saveChanges: function (backToMasterGroupMapping) {
          var ListOfLanguageIDsAndValues = { LanguageIDs: [], MasterGroupMappingID: config.MasterGroupMappingID };
          var records = grid.getStore().getModifiedRecords();
          Ext.each(records, function (record) {
            var value = {};
            value.LanguageID = record.get('LanguageID');
            value.Value = record.get('LanguageValue');
            value.LanguageMustBeFilled = record.get('LanguageMustBeFilled') ? true : false;
            ListOfLanguageIDsAndValues.LanguageIDs.push(value);
          });
          var ListOfLanguageIDsAndValues = JSON.stringify(ListOfLanguageIDsAndValues);

          Diract.request({
            url: Concentrator.route('UpdateMasterGroupMappingLanguage', 'MasterGroupMapping'),
            waitMsg: 'Updating Master Group Mapping Name',
            params: {
              ListOfLanguageIDsAndValuesJson: ListOfLanguageIDsAndValues
            },
            success: function () {
              if (backToMasterGroupMapping) {
                window.close();
              } else {
                grid.getStore().commitChanges();
                functions.setToolbarButtons(true);
              }
              var panels;
              if (config.treePanelToRefresh) {
                if (config.treePanelToRefresh == 'MasterGroupMappingTreePanel') {
                  panels = {
                    treePanel: true,
                    MasterGroupMappingID: config.MasterGroupMappingID
                  };
                } else {
                  panels = {
                    connectorTreePanel: true
                  };
                }
                Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
              }

              if (config.languageWizardGrid) {
                config.languageWizardGrid.grid.getStore().reload();
              }
            }
          });
        }
      };

      btnSave = functions.createSaveButton();
      btnSaveAndExit = functions.createSaveAndExitButton();
      btnDiscard = functions.createDiscardButton();
      btnExit = functions.createExitButton();

      var grid = new Diract.ui.Grid({
        url: Concentrator.route('GetMasterGroupMappingLanguages', 'MasterGroupMapping'),
        primaryKey: 'LanguageID',
        params: {
          masterGroupMappingID: config.MasterGroupMappingID
        },
        permissions: {
          list: 'GetLanguage',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [{
          dataIndex: 'LanguageID',
          type: 'int'
        }, {
          dataIndex: 'LanguageName',
          header: 'Language'
        }, {
          dataIndex: 'LanguageValue',
          header: 'Master Group Mapping Name',
          editor: {
            xtype: 'textfield'
          }
        }, {
          dataIndex: 'LanguageMustBeFilled',
          type: 'boolean',
          editable: true,
          header: 'Translation Correct?'
        }],
        customButtons: [functions.getButtons()]
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Rename Master Group Mapping "' + config.MasterGroupMappingName + '"',
        plain: true,
        layout: 'fit',
        resizable: true,
        width: 700,
        height: 500,
        closable: false,
        items: grid
      });
      window.show();
      grid.getStore().on("update", function () {
        functions.setToolbarButtons(false);
      });
    },
    dashboardWindow: function () {

      var anychartMatchedProducts = new AnyChart(Concentrator.anychartSource);
      anychartMatchedProducts.width = '100%';
      anychartMatchedProducts.height = (500 - 30) + 'px';
      anychartMatchedProducts.wMode = 'opaque';
      anychartMatchedProducts.setXMLFile(Concentrator.route('GetDashboard', 'MasterGroupMapping'));

      var dashboardPanel = new Ext.Panel({
        id: 'dashboardPanel',
        listeners: {
          'afterrender': function (component) {
            anychartMatchedProducts.write('dashboardPanel');
          }
        }

      })

      var dashboardWindow = new Ext.Window({
        title: 'Master group mapping statistics',
        width: 800,
        height: 500,
        modal: true,
        layout: 'fit',
        items: [dashboardPanel]
      });
      dashboardWindow.show();
    },

    findMasterGroupMapping: function (connectorID) {

      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Names',
        singularObjectName: 'Name',
        url: Concentrator.route("GetListOfMasterGroupMappingByConnectorID", "MasterGroupMapping"),
        autoLoadStore: true,
        forceFit: true,
        useHeaderFilters: true,
        params: {
          connectorID: connectorID
        },
        permissions: {
          list: 'DefaultMasterGroupMapping'
        },
        structure: [
        {
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'MasterGroupMappingName',
          header: 'Name',
          width: 75
        }, {
          dataIndex: 'MasterGroupMappingPath',
          header: 'Path'
        }],
        rowActions: [{
          text: 'Find Node in Tree',
          iconCls: 'view',
          buttonAlign: 'left',
          handler: function (record) {
            if (record.get('MasterGroupMappingID')) {
              if (connectorID) {
                var connectorTree = Ext.getCmp('connectorMappingTreePanel');
                var config = {
                  MasterGroupMappingIDs: [],
                  RootNode: connectorTree.getRootNode()
                };
                config.MasterGroupMappingIDs.push(record.get('MasterGroupMappingID'));
                Concentrator.MasterGroupMappingFunctions.refreshTree(config);
              } else {
                Concentrator.MasterGroupMappingFunctions.showTreeNodeByMasterGroupMappingID(record.get('MasterGroupMappingID'));
              }
            }
          }
        }],
        customButtons: ['->', {
          text: 'Exit',
          iconCls: 'exit',
          handler: function () {
            window.close();
          }
        }]
      });
      var window = new Ext.Window({
        modal: false,
        title: 'Find Master Group Mapping',
        plain: true,
        layout: 'fit',
        resizable: true,
        width: 500,
        height: 400,
        items: grid
      });
      window.show();

    }
  };

  Concentrator.UnMatchedProductGroup = Ext.extend(Concentrator.BaseAction, {
    requires: functionalities.VendorProductGroupsManagement,
    getPanel: function () {
      var grid = new Diract.ui.FilterGridPanel({
        pluralObjectName: 'Product Group Vendors',
        singularObjectName: 'Product Group Vendor',
        excelPlease: true,
        exportAll: true,
        primaryKey: ['ProductGroupVendorID'],
        id: 'VendorProductGroupsPanel',
        sortBy: 'ProductGroupVendorID',
        useHeaderFilters: true,
        autoLoadStore: true,
        url: Concentrator.route("GetListOfProductGroupVendors", "MasterGroupMapping"),
        forceFit: false,
        permissions: {
          list: 'GetProductGroupVendor',
          create: 'CreateProductGroupVendor',
          remove: 'DeleteProductGroupVendor',
          update: 'UpdateProductGroupVendor'
        },
        structure: [
									{
									  dataIndex: 'VendorID',
									  type: 'int'
									},
									{
									  dataIndex: 'VendorName',
									  type: 'string',
									  header: "Vendor",
									  width: 130,
									  editable: false

									}, // Vendor name
									{
									  dataIndex: 'VendorProductGroupName',
									  header: 'Vendor Code Description',
									  width: 270,
									  editable: false
									},
									{
									  dataIndex: 'ProductGroupVendorID',
									  type: 'int'
									}, // ProductGroup ID
									{
									  dataIndex: 'VendorProductGroupCode1',
									  header: 'Product Group Code 1',
									  width: 130,
									  editable: false
									}, // Product Group 1
									{
									  dataIndex: 'VendorProductGroupCode2',
									  header: 'Product Group Code 2',
									  width: 130,
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 2
									{
									  dataIndex: 'VendorProductGroupCode3',
									  header: 'Product Group Code 3',
									  width: 130,
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 3
									{
									  dataIndex: 'VendorProductGroupCode4',
									  header: 'Product Group Code 4',
									  width: 130,
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 4
									{
									  dataIndex: 'BrandCode',
									  header: 'Brand Code',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Brand code
									{
									  dataIndex: 'IsBlocked',
									  header: 'IsBlocked',
									  type: 'boolean',
									  editable: false
									},  // Blocked 
									{
									  dataIndex: 'VendorProductGroupCode5',
									  header: 'Product Group Code 5',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 5
									{
									  dataIndex: 'VendorProductGroupCode6',
									  header: 'Product Group Code 6',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 6
									{
									  dataIndex: 'VendorProductGroupCode7',
									  header: 'Product Group Code 7',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 7
									{
									  dataIndex: 'VendorProductGroupCode8',
									  header: 'Product Group Code 8',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 8
									{
									  dataIndex: 'VendorProductGroupCode9',
									  header: 'Product Group Code 9',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 9
									{
									  dataIndex: 'VendorProductGroupCode10',
									  header: 'Product Group Code 10',
									  editable: false,
									  editor: { xtype: 'textfield', allowBlank: true }
									}, // Product Group 10
        ],
        windowConfig: {
          height: 450,
          width: 500
        },
        listeners: {
          'rowclick': function (t, rowIndex, e) {
            var record = t.getSelectionModel().getSelected();
            var isBlocked = record.data.IsBlocked;
            var toolBarButtons = Ext.getCmp('VendorProductGroupsPanel').toolbarButtons;
            Ext.each(toolBarButtons, function (button) {
              if (button.text == 'Block Vendor Product Group' || button.text == 'UnBlock Vendor Product Group') {
                if (isBlocked) {
                  button.setText('UnBlock Vendor Product Group');
                  button.setIconClass("unchecked");
                } else {
                  button.setText('Block Vendor Product Group');
                  button.setIconClass("checked");
                }
              }
            });
          }
        },
        filterPanelConfig: {
          collapsible: false
        },
        filterItems: [
									new Ext.form.ComboBox({
									  typeAhead: false,
									  triggerAction: 'all',
									  editable: false,
									  mode: 'local',
									  name: 'IsBlocked',
									  fieldLabel: 'Blocked Product Group',
									  value: 1,
									  store: new Ext.data.ArrayStore({
									    id: 0,
									    fields: [
															'id',
															'text'
									    ],
									    data: [
															[1, 'All Product Group Vendors'],
															[2, 'Blocked Product Group Vendors'],
															[3, 'Unblocked Product Group Vendors']
									    ]
									  }),
									  valueField: 'id',
									  displayField: 'text',
									  listeners: {
									    'select': function (combo, record, index) {
									      grid.filterGrid();
									    }
									  }
									}),
									new Ext.form.ComboBox({
									  typeAhead: false,
									  triggerAction: 'all',
									  editable: false,
									  mode: 'local',
									  name: 'ShowID',
									  fieldLabel: 'Show Product Group',
									  value: 1,
									  store: new Ext.data.ArrayStore({
									    id: 0,
									    fields: [
															'id',
															'text'
									    ],
									    data: [
															[1, 'All Product Group Vendors'],
															[2, 'Mapped Product Group Vendors'],
															[3, 'Unmapped Product Group Vendors']
									    ]
									  }),
									  valueField: 'id',
									  displayField: 'text',
									  listeners: {
									    'select': function (combo, record, index) {
									      grid.filterGrid();
									    }
									  }
									}),
									"-",
									new Diract.ui.Select({
									  label: "Vendor",
									  valueField: "VendorID",
									  displayField: "VendorName",
									  store: new Ext.data.JsonStore({
									    autoDestroy: false,
									    url: Concentrator.route("GetListOfVendorNames", "MasterGroupMapping"),
									    method: 'GET',
									    root: 'results',
									    idProperty: 'VendorID',
									    fields: ['VendorID', 'VendorName']
									  }),
									  callback: function () {
									    grid.filterGrid();
									  },
									  clearable: false,
									  forceDynamicPageSize: false
									}),
									new Ext.form.TextField({
									  name: 'ProductGroupBrandCode',
									  fieldLabel: 'Product Group + Brand Code'
									}),
									new Ext.form.Hidden({
									  name: 'MasterGroupMappingID'
									})
        ],
        rowActions: [{
          iconCls: 'icon-view-vendor-assortments',
          text: 'View Vendor Assortments',
          handler: function (record) {
            Concentrator.MasterGroupMappingFunctions.getAssortementsOfVendorProductGroupWindow(record);
          }
        }, {
          iconCls: 'checked',
          text: 'Block Vendor Product Group',
          handler: function (record) {
            Concentrator.MasterGroupMappingFunctions.setBlockValueForVendorProductGroup(record);
          }
        }, {
          allowMultipleSelect: true,
          iconCls: 'icon-unmatch-vendor-product-group',
          text: 'UnMap Vendor Product Group',
          handler: function (record) {
            Concentrator.MasterGroupMappingFunctions.getUnMatchVendorProductGroupWindow(record);
          }
        }],
        customButtons: [{
          text: 'Master Group Mapping Filter',
          iconCls: 'lightbulb-off',
          tooltip: 'Click on button to clear Master Group Mapping Filter',
          disabled: true,
          handler: function () {
            this.setIconClass('lightbulb-off');
            this.setDisabled(true);

            Ext.each(grid.filterItems, function (filterItem) {
              if (filterItem.name) {
                if (filterItem.getName() == 'MasterGroupMappingID') {
                  filterItem.setValue(filterItem.defaultValue);
                  grid.filterGrid();
                }
              }
            });
          }
        }]
      });
      this.grid = grid.grid;
      return grid;
    }
  });

  Concentrator.MGMVendorProductFunctions = {
    BlockProduct: function (record, onSuccess) {
      var MasterGroupMappingID = record.get('MasterGroupMappingID');
      var ProductID = record.get('ConcentratorNumber');

      Diract.silent_request({
        url: Concentrator.route('GetListOfMachtedMasterGroupMappingsForProduct', 'MasterGroupMapping'),
        params: {
          ProductID: ProductID,
          MasterGroupMappingID: MasterGroupMappingID
        },
        success: function (response) {
          var masterGroupMappings = new Array();
          if (response.results.length > 0) {
            if (response.results.length > 1) {
              var masterGroupMappingPads = "<br> Product is matched in the next Master Group Mappings:";
            } else {
              var masterGroupMappingPads = "<br> Product is matched in the next Master Group Mapping:";
            }
            Ext.each(response.results, function (r) {
              masterGroupMappingPads += "<br>" + r.MasterGroupMappingPad;
              masterGroupMappings.push(r.MasterGroupMappingID);
            });
          } else {
            masterGroupMappingPads = "<br> Product is not matched";
          }

          Ext.Msg.show({
            title: 'Update Product Status',
            msg: 'Would you like to block Product "' + ProductID + '"?<br>' + masterGroupMappingPads,
            buttons: { ok: "Yes", cancel: "No" },
            fn: function (result) {
              if (result == 'ok') {
                Concentrator.MGMVendorProductFunctions.UpdateProductBlockSatus(ProductID, true, MasterGroupMappingID, onSuccess);
              } else {
              };
            },
            animEl: 'elId',
            icon: Ext.MessageBox.QUESTION
          });
        }
      });
    },
    UpdateProductBlockSatus: function (productID, IsBlocked, MasterGroupMappingID, onSuccess) {
      Diract.request({
        url: Concentrator.route('UpdateProduct', 'MasterGroupMapping'),
        waitMsg: 'Updating product status',
        params: {
          productID: productID,
          IsBlocked: IsBlocked
        },
        success: function () {

          if (MasterGroupMappingID) {
            var panels = {
              treePanel: true,
              ProductGrid: true,
              MasterGroupMappingID: MasterGroupMappingID
            };
          } else {
            var panels = {
              treePanel: true,
              ProductGrid: true
            };
          }
          Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
          if (onSuccess) onSuccess();
        },
        failure: function () { }
      });
    },
    RejectProduct: function (record, onSuccess) {

      Ext.Msg.show({
        title: 'Update Product Status',
        msg: 'Are you sure you would like to reject product:  "' + record.get('ConcentratorNumber') + '"?<br>',
        buttons: { ok: "Yes", cancel: "No" },
        fn: function (result) {
          if (result == 'ok') {
            Diract.request({
              url: Concentrator.route('UpdateVendorProduct', 'MasterGroupMapping'),
              waitMsg: 'Updating Product Status To Rejected',
              params: {
                ConcentratorNumber: record.get('ConcentratorNumber'),
                MasterGroupMappingID: record.get('MasterGroupMappingID'),
                ProductControleVariable: false
              },
              callback: function () {
                var config = {
                  connectorTreePanel: true
                };
                Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                if (onSuccess) onSuccess();
              }
            });
          } else {
          };
        },
        animEl: 'elId',
        icon: Ext.MessageBox.QUESTION
      });
    }
  };

  Concentrator.VendorProducts = Ext.extend(Concentrator.BaseAction, {
    requires: functionalities.VendorProductsManagement,
    getPanel: function () {
      var functions = {
        BlockProduct: function (productID) {
          var MasterGroupMappingID;
          Ext.each(grid.filterItems, function (filterItem) {
            if (filterItem.name) {
              if (filterItem.getName() == 'MasterGroupMappingID') {
                MasterGroupMappingID = filterItem.getValue();
              }
            }
          });

          Diract.silent_request({
            url: Concentrator.route('GetListOfMachtedMasterGroupMappingsForProduct', 'MasterGroupMapping'),
            params: {
              ProductID: productID,
              MasterGroupMappingID: MasterGroupMappingID
            },
            success: function (response) {
              var masterGroupMappings = new Array();
              if (response.results.length > 0) {
                if (response.results.length > 1) {
                  var masterGroupMappingPads = "<br> Product is matched in the next Master Group Mappings:";
                } else {
                  var masterGroupMappingPads = "<br> Product is matched in the next Master Group Mapping:";
                }
                Ext.each(response.results, function (r) {
                  masterGroupMappingPads += "<br>" + r.MasterGroupMappingPad;
                  masterGroupMappings.push(r.MasterGroupMappingID);
                });
              } else {
                masterGroupMappingPads = "<br> Product is not matched";
              }

              Ext.Msg.show({
                title: 'Update Product Status',
                msg: 'Would you like to block Product "' + productID + '"?<br>' + masterGroupMappingPads,
                buttons: { ok: "Yes", cancel: "No" },
                fn: function (result) {
                  if (result == 'ok') {
                    functions.UpdateProductBlockSatus(productID, true);
                  } else {
                  };
                },
                animEl: 'elId',
                icon: Ext.MessageBox.QUESTION
              });
            }
          });
        },
        UnBlockProduct: function (productID) {
          Ext.Msg.show({
            title: 'Update Product Status',
            msg: 'Would you like to Unblock Product "' + productID + '"',
            buttons: { ok: "Yes", cancel: "No" },
            fn: function (result) {
              if (result == 'ok') {
                functions.UpdateProductBlockSatus(productID, false);
              } else {
              };
            },
            animEl: 'elId',
            icon: Ext.MessageBox.QUESTION
          });
        },
        UpdateProductBlockSatus: function (productID, IsBlocked) {
          Diract.request({
            url: Concentrator.route('UpdateProduct', 'MasterGroupMapping'),
            waitMsg: 'Updating product status',
            params: {
              productID: productID,
              IsBlocked: IsBlocked
            },
            success: function () {
              var MasterGroupMappingID;
              Ext.each(grid.filterItems, function (filterItem) {
                if (filterItem.name) {
                  if (filterItem.getName() == 'MasterGroupMappingID') {
                    MasterGroupMappingID = filterItem.getValue();
                  }
                }
              });

              if (MasterGroupMappingID) {
                var panels = {
                  treePanel: true,
                  ProductGrid: true,
                  MasterGroupMappingID: MasterGroupMappingID
                };
              } else {
                var panels = {
                  treePanel: true,
                  ProductGrid: true
                };
              }
              Concentrator.MasterGroupMappingFunctions.refreshAction(panels);
            },
            failure: function () { }
          });
        }
      };

      var vendorProductGridMenu = new Ext.menu.Menu({
        items: [{
          id: 'PossibleMatches',
          text: 'Possible Matches',
          iconCls: 'wrench'
        }, {
          id: 'ViewProductDetails',
          text: 'View Product Details',
          iconCls: 'view'
        }, {
          id: 'ViewVendorAssortments',
          text: 'View Vendor Assortments',
          iconCls: 'view'
        }, {
          id: 'UnMatchVendorProduct',
          text: 'Unmap Product',
          iconCls: 'wrench',
          disabled: true
        }, {
          id: 'ApprovalProduct',
          text: 'Approval Product',
          iconCls: 'menuItem-Approve',
          disabled: true
        }, {
          id: 'RejectProduct',
          text: 'Reject Product',
          iconCls: 'menuItem-Reject',
          disabled: true
        }, {
          id: 'BlockProduct',
          text: 'Block Product',
          iconCls: 'menuItem-Block',
          disabled: false
        }, {
          id: 'UnBlockProduct',
          text: 'UnBlock Product',
          iconCls: 'menuItem-UnBlock',
          disabled: true
        }],
        listeners: {
          itemclick: function (item) {

            var selections = grid.grid.getSelectionModel().getSelections();
            var selectedRecord = grid.grid.getSelectionModel().getSelected();

            if (selectedRecord) {
              switch (item.id) {
                case 'PossibleMatches':
                  Concentrator.MasterGroupMappingFunctions.possibleMatches(selectedRecord);
                  break;
                case 'ViewProductDetails':
                  var factory = new Concentrator.ProductBrowserFactory({ productID: selectedRecord.get('ConcentratorNumber') });
                  break;
                case 'ViewVendorAssortments':
                  Concentrator.MasterGroupMappingFunctions.viewVendorAssortments(selectedRecord);
                  break;
                case 'UnMatchVendorProduct':
                  Concentrator.MasterGroupMappingFunctions.getUnMatchVendorProductWindow(selections);
                  break;
                case 'ApprovalProduct':
                  Concentrator.MasterGroupMappingFunctions.productControleWindow(selectedRecord);
                  break;
                case 'RejectProduct':
                  Diract.request({
                    url: Concentrator.route('UpdateVendorProduct', 'MasterGroupMapping'),
                    waitMsg: 'Updating Product Status To Rejected',
                    params: {
                      ConcentratorNumber: selectedRecord.get('ConcentratorNumber'),
                      MasterGroupMappingID: selectedRecord.get('MasterGroupMappingID'),
                      ProductControleVariable: false
                    },
                    callback: function () {
                      grid.filterGrid();
                      config = {
                        treePanel: true,
                        MasterGroupMappingID: record.get('MasterGroupMappingID')
                      };
                      Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                    }
                  });
                  break;
                case 'BlockProduct':
                  functions.BlockProduct(selectedRecord.get('ConcentratorNumber'));
                  break;
                case 'UnBlockProduct':
                  functions.UnBlockProduct(selectedRecord.get('ConcentratorNumber'));
                  break;
              }
            }
          }
        }
      });
      var grid = new Diract.ui.FilterGridPanel({
        excelPlease: true,
        exportAll: true,
        pluralObjectName: 'Vendor Products',
        singularObjectName: 'Vendor Product',
        id: 'VendorProductsPanel',
        sortBy: 'ConcentratorNumber',
        autoLoadStore: false,
        refreshAfterSave: true,
        useHeaderFilters: true,
        url: Concentrator.route("GetListOfVendorProducts", "MasterGroupMapping"),
        forceFit: true,
        permissions: {
          list: 'DefaultMasterGroupMapping',
          create: 'DefaultMasterGroupMapping',
          remove: 'DefaultMasterGroupMapping',
          update: 'DefaultMasterGroupMapping'
        },
        structure: [
        {
          dataIndex: 'IsBlocked',
          header: 'Is Product Blocked',
          type: 'boolean'
        }, {
          dataIndex: 'ConcentratorNumber',
          header: 'Concentrator #',
          type: 'int',
          width: 125
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name',
          type: 'string',
          width: 240
        }, {
          dataIndex: 'Brand',
          header: 'Brand',
          type: 'string'
        }, {
          dataIndex: 'ProductCode',
          header: 'Product Code',
          type: 'string'
        }, {
          dataIndex: 'Image',
          header: 'Image',
          type: 'boolean'
        }, {
          dataIndex: 'PossibleMatch',
          header: 'Possible Match',
          type: 'int',
          renderer: function (val, m, record) {
            return record.get('PossibleMatch') + " %";
          }
        }, {
          dataIndex: 'CountMatches',
          header: '# Possible Matches',
          type: 'int'
        }, {
          dataIndex: 'CountVendorAssortment',
          header: '# Vendor Assortments',
          type: 'int'
        }, {
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'ShortContentDescription',
          type: 'string',
          hidden: true,
          isAdditional: true,
          hideable: false,
          header: 'Short Content Description'
        },
        {
          dataIndex: 'LongContentDescription',
          type: 'string',
          hidden: true,
          isAdditional: true,
          hideable: false,
          header: 'Long Content Description'
        }, {
          dataIndex: 'IsConfigurable',
          header: 'Is Configurable',
          type: 'boolean'
        }],
        windowConfig: {
          height: 450,
          width: 500
        },
        listeners: {
          rowclick: function (thisGrid, rowIndex, e) {
            var record = thisGrid.getSelectionModel().getSelected();
            Ext.each(thisGrid.getTopToolbar().items.items, function (button) {
              if (button.text == 'Product Control' || button.text == 'Approval Product' || button.text == 'Reject Product') {
                switch (record.get('ProductControl')) {
                  case "NotMatched":
                    button.setText('Product Control');
                    button.setIconClass('');
                    break;
                  case "Approved":
                    button.setText('Reject Product');
                    button.setIconClass('cancel');
                    break;
                  case "Rejected":
                    button.setText('Approval Product');
                    button.setIconClass('save');
                    break;
                }
              }
            });
          },
          rowcontextmenu: function (thisGrid, index, event) {
            var selections = thisGrid.getSelectionModel().getSelections().length;
            if (selections == 1) {
              thisGrid.getSelectionModel().selectRow(index);
            }
            var record = thisGrid.getSelectionModel().getSelected();
            var approvalMenuBtn = vendorProductGridMenu.get('ApprovalProduct');
            var rejectMenuBtn = vendorProductGridMenu.get('RejectProduct');
            var unMatchedMenuBtn = vendorProductGridMenu.get('UnMatchVendorProduct');
            var blockProductMenuBtn = vendorProductGridMenu.get('BlockProduct');
            var unBlockProductMenuBtn = vendorProductGridMenu.get('UnBlockProduct');

            if (selections == 1) {
              switch (record.get('ProductControl')) {
                case "NotMatched":
                  approvalMenuBtn.setDisabled(true);
                  rejectMenuBtn.setDisabled(true);
                  unMatchedMenuBtn.setDisabled(true);
                  break;
                case "Approved":
                  approvalMenuBtn.setDisabled(true);
                  rejectMenuBtn.setDisabled(false);
                  unMatchedMenuBtn.setDisabled(false);
                  break;
                case "Rejected":
                  approvalMenuBtn.setDisabled(false);
                  rejectMenuBtn.setDisabled(true);
                  unMatchedMenuBtn.setDisabled(false);
                  break;
              };

              switch (record.get('IsBlocked')) {
                case false:
                  blockProductMenuBtn.setDisabled(false);
                  unBlockProductMenuBtn.setDisabled(true)
                  break;
                case true:
                  blockProductMenuBtn.setDisabled(true);
                  unBlockProductMenuBtn.setDisabled(false)
                  break;
              }
            } else {
              //disable all with multiple records except unmatch
              blockProductMenuBtn.setDisabled(true);
              approvalMenuBtn.setDisabled(true);
              rejectMenuBtn.setDisabled(true);
              unMatchedMenuBtn.setDisabled(false);
              unBlockProductMenuBtn.setDisabled(true)
            }

            event.stopEvent();
            vendorProductGridMenu.showAt(event.xy);
          }
        },
        filterPanelConfig: {
          collapsible: false
        },
        filterItems: [
					new Ext.form.ComboBox({
					  typeAhead: false,
					  triggerAction: 'all',
					  editable: false,
					  mode: 'local',
					  name: 'MatchedUnMatchedVendorProducts',
					  fieldLabel: 'Vendor Products',
					  value: 1,
					  store: new Ext.data.ArrayStore({
					    id: 0,
					    fields: ['id', 'text'],
					    data: [[1, 'All Vendor Products'], [2, 'Matched Vendor Products'], [3, 'UnMatched Vendor Products']]
					  }),
					  valueField: 'id',
					  displayField: 'text',
					  listeners: {
					    'select': function (combo, record, index) {
					      grid.filterGrid();
					    }
					  }
					}),
					new Ext.form.ComboBox({
					  typeAhead: false,
					  triggerAction: 'all',
					  editable: false,
					  mode: 'local',
					  name: 'ImageVendorProducts',
					  fieldLabel: 'Image',
					  value: 1,
					  store: new Ext.data.ArrayStore({
					    id: 0,
					    fields: ['id', 'text'],
					    data: [[1, 'All Vendor Products'], [2, 'Vendor Products With Image'], [3, 'Vendor Products Without Images']]
					  }),
					  valueField: 'id',
					  displayField: 'text',
					  listeners: {
					    'select': function (combo, record, index) {
					      grid.filterGrid();
					    }
					  }
					}), "-",
					new Ext.form.ComboBox({
					  typeAhead: false,
					  triggerAction: 'all',
					  editable: false,
					  mode: 'local',
					  name: 'ProductBlockStatus',
					  fieldLabel: 'Product Status',
					  value: 1,
					  store: new Ext.data.ArrayStore({
					    id: 0,
					    fields: ['id', 'text'],
					    data: [[1, 'All Products'], [2, 'Blocked Products'], [3, 'UnBlocked Products']]
					  }),
					  valueField: 'id',
					  displayField: 'text',
					  listeners: {
					    'select': function (combo, record, index) {
					      grid.filterGrid();
					    }
					  }
					}),
					new Ext.form.TextField({
					  name: 'ProductID',
					  fieldLabel: 'Concentrator Number'
					}),
					new Ext.form.TextField({
					  name: 'UniversalSearchTerm',
					  fieldLabel: 'Search Term',
					  listeners: {
					    afterrender: function (c) {
					      Ext.QuickTips.register({
					        target: c.getEl(),
					        text: 'Type in any search term you would like to search for'
					      });
					    }
					  }
					}),
					new Ext.form.Hidden({
					  name: 'MasterGroupMappingID'
					})
        ],
        rowActions: [{
          iconCls: 'wrench',
          text: 'Possible Matches',
          handler: function (record) {
            Concentrator.MasterGroupMappingFunctions.possibleMatches(record);
          }
        }, {
          iconCls: 'wrench',
          text: 'View Product Details',
          handler: function (record) {
            var factory = new Concentrator.ProductBrowserFactory({ productID: record.get('ConcentratorNumber') });
          }
        }, {
          iconCls: 'wrench',
          text: 'View Associated Vendor Assortments',
          handler: function (record) {
            Concentrator.MasterGroupMappingFunctions.viewVendorAssortments(record);
          }
        }, {
          iconCls: 'wrench',
          allowMultipleSelect: true,
          text: 'Unmap',
          handler: function (record) {
            Concentrator.MasterGroupMappingFunctions.getUnMatchVendorProductWindow(record);
          }
        }],
        customButtons: [{
          text: 'Master Group Mapping Filter',
          iconCls: 'lightbulb-off',
          tooltip: 'Click on button to clear Master Group Mapping Filter',
          disabled: true,
          handler: function () {
            this.setIconClass('lightbulb-off');
            this.setDisabled(true);

            Ext.each(grid.filterItems, function (filterItem) {
              if (filterItem.name) {
                if (filterItem.getName() == 'MasterGroupMappingID') {
                  filterItem.setValue(filterItem.defaultValue);
                  grid.filterGrid();
                }
              }
            });
          }
        }]
      });
      this.grid = grid.grid;
      return grid;
    }
  });


  Concentrator.ConnectorMapping = Ext.extend(Concentrator.BaseAction, {
    needsConnector: true,
    requires: 'ConnectorMappingManagement',
    // functions
    getBaseAttributes: function (node, treeLoader) {
      var params = {};

      if (!treeLoader) {
        for (var key in { ConnectorID: -1, MasterGroupMappingID: -1 }) {
          params[key] = node.attributes[key];
        }
      } else {
        for (var key in treeLoader.baseAttrs) {
          params[key] = node.attributes[key];
        }
      }

      return params;
    },
    getTabPanelItems: function (connectorID) {
      this.tabProduct = new Concentrator.ConnectorMappingProducts({
        title: 'Products',
        iconCls: 'link',
        layout: 'fit',
        requires: functionalities.ViewProducts
      });
      this.tabConnectorPublicationRule = new Concentrator.ConnectorMappingConnectorPublicationRules({
        title: 'Connector Publication Rule',
        iconCls: 'link',
        layout: 'fit',
        ConnectorID: connectorID,
        requires: functionalities.ViewConnectorPublicationRule
      });
      return [
				this.tabProduct,
				this.tabConnectorPublicationRule
      ];
    },
    reloadAllConnectorMappingTabs: function () {
      //  var connectorPublicationRuleGrid = Ext.getCmp('ConnectorMappingConnectorPublicationRuleGrid');
      //  var groupAttributeMappingGrid = Ext.getCmp('connectorProductGroupAttributeGrid');
      //  var priceRuleGrid = Ext.getCmp('connectorMappingPriceRuleGrid');
      //  var priceTagMappingGrid = Ext.getCmp('MasterGroupPriceTagGrid');

      //  connectorPublicationRuleGrid.getStore().reload();
      //  //groupAttributeMappingGrid.getStore().reload();
      //  priceRuleGrid.getStore().reload();
      //  priceTagMappingGrid.getStore().reload();

      var reload = function (id) {
        var cmp = Ext.getCmp(id);
        if (!!cmp) {
          cmp.getStore().reload();
        }
      };

      reload('ConnectorMappingConnectorPublicationRuleGrid');
      reload('connectorProductGroupAttributeGrid');
      reload('connectorMappingPriceRuleGrid');
      reload('MasterGroupPriceTagGrid');
    },

    // Menu Items
    menuItemAddNewConnectorMapping: function (node, thisConnectorID) {
      config = {
        MasterGroupMappingID: node.attributes.MasterGroupMappingID,
        MasterGroupMappingName: node.attributes.text,
        ConnectorID: thisConnectorID
      };
      Concentrator.MasterGroupMappingFunctions.getAddNewMasterGroupMappingWindow(config);
    },
    menuItemDeleteConnectorMapping: function (node) {
      var id = node.attributes.MasterGroupMappingID;
      if (id > -1) {
        var title = 'Delete Connector Mapping "' + node.attributes.text + '"';
        var msg = 'Are you sure you want to delete Connector Mapping "' + node.attributes.text + '" in Connector "' + node.parentNode.attributes.text + '" ?';
        Ext.Msg.confirm(title, msg, function (button) {
          if (button == "yes") {
            Diract.mute_request({
              url: Concentrator.route("Delete", "MasterGroupMapping"),
              params: { id: id },
              waitMsg: 'Deleting the connector mapping',
              success: function () {
                var panels = {
                  connectorTreePanel: true
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
        });
      } else {

      }
    },
    menuItemFindConnectorMapping: function (connectorID) {
      if (connectorID > -1) {
        Concentrator.MasterGroupMappingFunctions.findMasterGroupMapping(connectorID);
      } else {
      }
    },
    menuItemSettingConnectorMapping: function (node) {

      var formFields = {
        createFilterByParentCheckbox: function () {
          return new Ext.form.Checkbox({
            fieldLabel: 'Filter By Parent',
            name: 'filterByParent'
          });
        },
        createFlattenHierachyCheckbox: function () {
          return new Ext.form.Checkbox({
            fieldLabel: 'Flatten Hierachy',
            name: 'flattenHierachy'
          });
        },
        createExportIDNumberField: function () {
          return new Ext.form.NumberField({
            fieldLabel: 'ExportID',
            name: 'exportID'
          });
        },
        createFollowSourceMasterGroupMappingNameCheckbox: function () {
          return new Ext.form.Checkbox({
            fieldLabel: 'Follow Source Master Group Mapping Name',
            name: 'followSourceMasterGroupMappingName'
          });
        },
        createDisableInNaviagtionCheckbox: function () {
          return new Ext.form.Checkbox({
            fieldLabel: 'Hide in menu',
            name: 'disableInNaviagtion'
          });
        },
        createWposMasterGroupMappingSettingCheckboxes: function (settings) {

          var items = [];

          for (var i = 0; i < settings.length; i++) {

            var checkbox = new Ext.form.Checkbox({
              fieldLabel: settings[i].code,
              name: settings[i].code,
              value: settings[i].settingValue,
              checked: settings[i].settingValue,
              listeners: {
                'check': function (checkbox, checked) {

                  for (var j = 0; j < wposSettings.length; j++) {

                    if (wposSettings[j].code == checkbox.name) {
                      wposSettings[j].settingValue = checked;
                      break;
                    }
                  }
                }
              }
            });
            items.push(checkbox);
          }
          return items;
        },
        createDisableInProductGroupCheckbox: function () {
          return new Ext.form.Checkbox({
            fieldLabel: 'Disable menu',
            name: 'disableInProductGroup'
          });
        },
        createListViewCheckbox: function () {
          return new Ext.form.Checkbox({
            fieldLabel: 'Is anchor',
            name: 'listView'
          });
        },
        createPageLayoutSelect: function () {
          return new Diract.ui.Select({
            name: 'LayoutID',
            fieldLabel: 'Page Layout',
            allowBlank: true,
            width: 100,
            valueField: 'LayoutID',
            store: Concentrator.stores.pageLayouts,
            label: 'Page Layout',
            displayField: 'LayoutName'
          });
        },
        createProductGroupConnectorItems: function () {
          var connectorsForUser = Concentrator.stores.connectorsForLoggedInUser.data.items;

          var productGroupMappingConnectorItems = [];

          for (var i = 0; i < connectorsForUser.length; i++) {

            var newProductGroupMappingConnectorItem = new Ext.form.Checkbox({
              name: 'ConnectorID_' + connectorsForUser[i].data.ConnectorID,
              connectorID: connectorsForUser[i].data.ConnectorID,
              fieldLabel: connectorsForUser[i].data.ConnectorName,
              value: true,
              checked: true
            })

            productGroupMappingConnectorItems.push(newProductGroupMappingConnectorItem);
          }

          return productGroupMappingConnectorItems;
        }
      };
      var tabPanels = {
        getMasterGroupMappingSettingConfig: function () {
          return {
            title: 'General Settings',
            layout: 'form',
            bodyStyle: 'padding:10px',
            defaults: { width: 70 },
            items: [checkboxFilterByParent, checkboxFlattenHierachy, numberFieldExportID]
          };
        },

        getMasterGroupMappingMagentoSettingConfig: function () {
          return {
            title: 'Magento Setting',
            bodyStyle: 'padding:10px',
            layout: 'form',
            items: [checkboxDisableInNaviagtion, checkboxDisableInProductGroup, checkboxListView, pageLayoutSelect]
          }
        },

        getMasterGroupMappingActiveInactiveGroupsSettingConfig: function () {

          return {
            title: 'Active/Inactive groups per connector',
            layout: 'form',
            bodyStyle: 'padding:10px',
            defaults: { width: 10 },
            items: [productGroupMappingConnectorItems]
          }
        },

        getMasterGroupMappingWposSettingConfig: function () {

          var wposCheckBoxes = formFields.createWposMasterGroupMappingSettingCheckboxes(wposSettings);

          return {
            title: 'Wpos Setting',
            layout: 'form',
            bodyStyle: 'padding:10px',
            defaults: { width: 10 },
            items: wposCheckBoxes
          }
        },

        getMasterGroupMappingFollowSystemConfig: function () {
          return {
            title: 'Follow  System Settings',
            layout: 'form',
            bodyStyle: 'padding:10px',
            defaults: { width: 10 },
            items: [checkboxFollowSourceMasterGroupMappingName]
          }
        },

        getMasterGroupMappingUploadPictureConfig: function () {
          var self = this;

          if (tabPanels.dataViewData != null) {

            var data = JSON.stringify(tabPanels.dataViewData);
            if (data != "") {
              dataView.store.loadData(data);
            }
          }

          var imageHolder = new Ext.Panel({
            id: 'imageHolder',
            layout: 'column',
            region: 'center',

            width: 500,
            // columnWidth: .8,
            autoScroll: true,
            height: 300,
            border: false,
            items: self.generateImageViews(currentMediaType, imageConnectors, node.attributes.MasterGroupMappingID)
          });


          var west = new Ext.Panel({
            id: 'imagesmenu',
            region: 'west',
            width: 200,
            height: 300,
            // columnWidth: .2,
            margins: '0 0 0 0',
            padding: 10,
            collapseMode: 'mini',
            split: true,
            border: false
          });

          west.add([
           {
             xtype: 'menuitem',
             text: 'Images',
             overCls: 'menu-item-over',
             activeClass: 'x-menu-item-active',
             listeners: {
               'click': function () {
                 currentMediaType = "Image";

                 var borderPanel = settingFormPanel.getComponent('tabs').getComponent('image_holder').getComponent('borderPanel');
                 borderPanel.setTitle('Images');
                 borderPanel.getComponent('imageHolder').items.each(function (comp) {

                   var dataView = comp.items.get(0);

                   var dropZone = Ext.getCmp('mgmImageDropZone-' + comp.id);
                   if (dropZone) {
                     dropZone.setConfig('Url', Concentrator.route('UploadImage', 'MasterGroupMapping', {
                       MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                       type: 'Image',
                       connectorID: dataView.getStore().baseParams.connectorID,
                       languageID: dataView.getStore().baseParams.languageID
                     }));
                   }

                   if (dataView != null && dataView.id.indexOf('mgmDataView') != -1) {

                     dataView.getStore().load();
                   }
                 });
               }
             }
           },
            {
              xtype: 'menuitem',
              text: 'Thumbnails',
              overCls: 'menu-item-over',
              activeClass: 'x-menu-item-active',
              listeners: {
                'click': function () {
                  currentMediaType = "Thumbnail";
                  var borderPanel = settingFormPanel.getComponent('tabs').getComponent('image_holder').getComponent('borderPanel');
                  borderPanel.setTitle('Thumbnails');
                  borderPanel.getComponent('imageHolder').items.each(function (comp) {

                    var dataView = comp.items.get(0);

                    var dropZone = Ext.getCmp('mgmImageDropZone-' + comp.id);
                    if (dropZone) {
                      dropZone.setConfig('Url', Concentrator.route('UploadImage', 'MasterGroupMapping', {
                        MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                        type: 'Thumbnail',
                        connectorID: dataView.getStore().baseParams.connectorID,
                        languageID: dataView.getStore().baseParams.languageID
                      }));
                    }

                    if (dataView != null && dataView.id.indexOf('mgmDataView') != -1) {

                      dataView.getStore().load();
                    }
                  });
                }
              }
            },
             {
               xtype: 'menuitem',
               text: 'Menu Icons',
               overCls: 'menu-item-over',
               activeClass: 'x-menu-item-active',
               listeners: {
                 'click': function () {
                   currentMediaType = "MenuIcon";
                   var borderPanel = settingFormPanel.getComponent('tabs').getComponent('image_holder').getComponent('borderPanel');
                   borderPanel.setTitle('Menu Icons');
                   borderPanel.getComponent('imageHolder').items.each(function (comp) {

                     var dataView = comp.items.get(0);

                     var dropZone = Ext.getCmp('mgmImageDropZone-' + comp.id);
                     if (dropZone) {
                       dropZone.setConfig('Url', Concentrator.route('UploadImage', 'MasterGroupMapping', {
                         MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                         type: 'MenuIcon',
                         connectorID: dataView.getStore().baseParams.connectorID,
                         languageID: dataView.getStore().baseParams.languageID
                       }));
                     }

                     if (dataView != null && dataView.id.indexOf('mgmDataView') != -1) {

                       dataView.getStore().load();
                     }
                   });
                 }
               }
             }
          ]);

          var borderHolder = new Ext.Panel({
            title: 'Images',
            id: 'borderPanel',
            layout: 'border',
            width: 500,
            height: 400,
            items: [west, imageHolder],
            border: false
          });

          var mainPanel = new Ext.Panel({
            title: 'Image Setting',
            id: 'image_holder',
            layout: 'fit',
            items: [borderHolder]
          });

          return mainPanel;
        },

        generateImageViews: function (currentMediaType, imageConnectors, masterGroupMappingID) {

          var self = this;

          var imageViews = [];

          for (var i = 0; i < imageConnectors.length; i++) {
            var connector = imageConnectors[i];
            if (connector.Languages != null && connector.Languages.length > 0) {
              for (var c = 0; c < connector.Languages.length; c++) {
                var language = connector.Languages[c];
                var image = new Ext.Panel({
                  id: 'mgm_imageView_' + i + '_' + c,
                  title: imageConnectors[i].ConnectorName + "/" + language.Name,
                  columnWidth: .5,
                  cls: 'mgm_imageView',
                  height: 200,
                  width: 250,
                  items: self.generateDataView(currentMediaType, masterGroupMappingID, i, imageConnectors[i].ConnectorID, language.LanguageID)
                });
                imageViews.push(image);
              }
            } else {
              var image = new Ext.Panel({
                id: 'mgm_imageView_' + i,
                title: imageConnectors[i].ConnectorName,
                columnWidth: .5,
                cls: 'mgm_imageView',
                height: 200,
                width: 250,
                items: self.generateDataView(currentMediaType, masterGroupMappingID, i, imageConnectors[i].ConnectorID, null)
              });
              imageViews.push(image);
            }
          }

          return imageViews;

        },

        generateDataView: function (type, masterGroupMappingID, index, connectorID, languageID) {

          var tp = new Ext.XTemplate(
         '<tpl for=".">',
           '<div class="thumb-wrap" mediaPath="{values.ImagePath}" data-qtip="test">',
               '<div class="thumbMM">',
                 '<img src="{[this.getThumbSrc(values.ImagePath, true)]}"/>',
                 '<div class="media-toolbox">',
                 '<button class="media-thumb-toolbox delete x-btn" mediaid="{values.ImagePath}"></button>',
                 '</div>',
               '</div>',
             '</div>',
           '</tpl>',
         '<div class="x-clear"></div>'
         , {
           getThumbSrc: function (path, restrict) {
             if (restrict) {
               return Concentrator.GetImageUrl(path, 180, 180, 'Test', null, true);
             } else {
               return Concentrator.GetImageUrl(path, null, null, null, null, true);
             }
           }
         }
       );

          tp.compile();

          // Set up images view
          var imageStore = new Ext.data.JsonStore({
            // autoLoad: false,
            url: Concentrator.route('GetMasterGroupMappingImage', 'MasterGroupMapping'),
            baseParams: { masterGroupMappingID: masterGroupMappingID, type: type, connectorID: connectorID, languageID: languageID },
            root: 'results',
            idProperty: ['ImagePath'],
            fields: ['ImagePath'],
            listeners: {
              'beforeload': function (a, b, c) {
                this.baseParams.type = currentMediaType;
              }
            }
          });

          var dataView = new Ext.DataView({
            store: imageStore,
            tpl: tp,
            height: 180,
            deferEmptyText: true,
            columnWidth: .5,
            emptyText: 'Drop an image here...',
            id: 'mgmDataView_' + connectorID + '_' + languageID,
            listeners: {
              'click': (function (view, index, item, e) {

                var el = new Ext.Element(e.target);
                var mediaID = el.getAttribute('mediaid');
                if (mediaID) {
                  //remove was clicked
                  Ext.Msg.confirm("Delete Master Group Mapping Image", "Are you sure you want to delete this Master Group Mapping image?", function (answer) {
                    if (answer === "yes") {
                      Diract.request({
                        url: Concentrator.route('DeleteMasterGroupMappingImage', 'MasterGroupMapping'),
                        waitMsg: 'Deleting MasterGroupMapping Image',
                        params: {
                          MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                          type: currentMediaType,
                          connectorID: dataView.getStore().baseParams.connectorID,
                          languageID: dataView.getStore().baseParams.languageID
                        },
                        success: function (record) {
                          imageStore.load();
                        },
                        failure: function () { }
                      });

                    } else {
                      return;
                    }
                  }, this);
                }

              }).createDelegate(this),
              'mouseenter': (function (view, index, node, e) {
                var el = new Ext.Element(node);
                el.child('.media-toolbox').setStyle('display', 'inline');
              }).createDelegate(this),
              'mouseleave': (function (view, index, node, e) {
                var el = new Ext.Element(node);
                el.child('.media-toolbox').setStyle('display', 'none');
              }).createDelegate(this),
              'afterrender': (function (view) {



              }).createDelegate(this)


            },
            overClass: 'x-view-over',

            itemSelector: 'div.thumb-wrap',
            singleSelect: true,
            autoScroll: true
          });

          return dataView;
        },

        getSettingTabPanels: function (showMagentoSetting, genericSettingsPanels) {
          var tabItems = [];

          tabItems.push(tabPanels.getMasterGroupMappingSettingConfig());
          tabItems.push(tabPanels.getMasterGroupMappingUploadPictureConfig());
          //tabItems.push(tabPanels.getMasterGroupMappingFollowSystemConfig());
          if (showMagentoSetting) {
            tabItems.push(tabPanels.getMasterGroupMappingMagentoSettingConfig());
          }
          if (showWposSetting) {
            tabItems.push(tabPanels.getMasterGroupMappingWposSettingConfig(wposSettings));
          }

          if (showConnectorGroupSettings) {
            tabItems.push(tabPanels.getMasterGroupMappingActiveInactiveGroupsSettingConfig());
          }

          for (var i = 0; i < genericSettingsPanels.length; i++) {
            var foundMatch = false;
            for (var j = 0; j < tabItems.length; j++) {
              if (genericSettingsPanels[i].title == tabItems[j].title) {
                tabItems[j].items.push(genericSettingsPanels[i].items);
                foundMatch = true;
                break;
              }
            }
            if (!foundMatch)
              tabItems.push(genericSettingsPanels[i]);
          }

          return tabItems;
        },

        panelExists: function (genericSettingsPanels, groupName) {
          for (var i = 0; i < genericSettingsPanels.length; i++) {
            if (genericSettingsPanels[i].title == groupName)
              return true;
          }
          return false;
        },

        createSettingsPanel: function (groupName) {
          return {
            title: groupName,
            layout: 'form',
            bodyStyle: 'padding:10px',
            items: []
          }
        },

        createSettingElement: function (setting) {
          if (setting.Type == 'string') {
            return new Ext.form.TextField({
              fieldLabel: setting.Name,
              name: setting.Name,
              width: '100px',
              value: setting.Value,
              settingID: setting.SettingID
            });
          } else if (setting.Type == 'boolean') {
            return new Ext.form.Checkbox({
              fieldLabel: setting.Name,
              name: setting.Name,
              width: '100px',
              checked: setting.Value == 'True',
              settingID: setting.SettingID
            });
          } else if (setting.Type == 'option') {

            var store = new Ext.data.JsonStore({
              autoDestroy: false,
              url: Concentrator.route("GetOptions", "MasterGroupMappingSetting"),
              method: 'GET',
              baseParams: { settingID: setting.SettingID },
              root: 'options',
              idProperty: 'ID',
              fields: ['ID', 'Value']
            });
            store.load();

            return {
              name: setting.Name,
              xtype: 'masterGroupMappingOptionSelect',
              store: store,//stores.getMasterGroupMappingSettingOptionStore(setting.SettingID),
              label: setting.Name,
              value: setting.Value,
              settingID: setting.SettingID,
              allowBlank: true,
              valueField: 'ID',
              displayField: 'Value',
              width: 150,
              clearable: true
            };
          }
        }
      };

      var stores = {
        getImageStore: function () {
          return new Ext.data.ArrayStore({
            fields: ['FileName']
          });
        },

        getMasterGroupMappingSettingOptionStore: function (settingID) {
          var store = new Ext.data.JsonStore({
            autoDestroy: false,
            url: Concentrator.route("GetOptions", "MasterGroupMappingSetting"),
            method: 'GET',
            baseParams: { settingID: settingID },
            root: 'options',
            idProperty: 'ID',
            fields: ['ID', 'Value']
          });
          store.load();

          return store;
        }
      };

      var dataviewData = null;

      var panels = {
        createSettingFormPanel: function () {
          return new Ext.FormPanel({
            labelWidth: 250,
            border: false,
            width: 500,
            items: {
              xtype: 'tabpanel',
              activeTab: 0,
              defaults: { autoHeight: true },
              items: [tabPanels.getMasterGroupMappingSettingConfig()]
            },
            buttons: [{
              text: 'Save',
              handler: function () {
                var SaveParams = {
                  MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                  FilterByParentGroup: checkboxFilterByParent.getValue(),
                  FlattenHierarchy: checkboxFlattenHierachy.getValue(),
                  ExportID: numberFieldExportID.getValue()
                };
                var SaveParamsJson = JSON.stringify(SaveParams);
                Diract.request({
                  url: Concentrator.route('UpdateConnectorMappingSettings', 'MasterGroupMapping'),
                  waitMsg: 'Updating Connector Mapping Settings',
                  params: {
                    SaveParamsJson: SaveParamsJson
                  },
                  success: function (record) {
                    window.close();
                  },
                  failure: function () { }
                });
              }
            }, {
              text: 'Cancel',
              handler: function () {
                window.close();
              }
            }],

            listeners: {
              afterrender: function (thisForm) {

              }
            }
          });
        }
      };
      var windows = {
        getWindow: function () {
          
          settingFormPanel = new Ext.FormPanel({
            labelWidth: 250,
            border: false,
            width: 900,
            height: 400,
            id: 'settingsFormPanel',
            items: {
              xtype: 'tabpanel',
              activeTab: 0,
              id: 'tabs',
              defaults: { autoHeight: true },
              items: tabPanels.getSettingTabPanels(showMagentoSettings, genericSettingsPanels),
              listeners: {
                'tabchange': function (t, tab) {
                  if (tab.id == "image_holder") {

                    window.setHeight(490);
                    window.setWidth(790);

                    tab.getComponent('borderPanel').getComponent('imageHolder').items.each(function (comp) {

                      var dataView = comp.items.get(0);
                      if (dataView != null && dataView.id.indexOf('mgmDataView') != -1) {
                        dataView.getStore().load();
                      }

                      var dropZone = new Diract.DragDropUpload({
                        ElementId: comp.id,
                        id: 'mgmImageDropZone-' + comp.id,
                        acceptedTypes: ["image/"],
                        Url: Concentrator.route('UploadImage', 'MasterGroupMapping', {
                          MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                          type: dataView.store.baseParams.type,
                          connectorID: dataView.getStore().baseParams.connectorID,
                          languageID: dataView.getStore().baseParams.languageID
                        }),
                        onDragEnter: function (e) {
                          e.preventDefault();

                          var images = Ext.getCmp(this.id);
                          images.body.stopFx();
                          images.body.highlight();
                          return false;
                        },
                        onDragLeave: function (e) {
                          e.preventDefault();
                          return false;
                        },
                        onLoad: function (e) {
                          e.preventDefault();
                          var comp = Ext.getCmp(this.ElementId);
                          var dataView = comp.items.get(0);
                          if (dataView != null && dataView.id.indexOf('mgmDataView') != -1) {
                            dataView.getStore().load();
                          }
                          return false;
                        }
                      });

                    });

                  } else {
                    window.setHeight(300);
                    window.setWidth(550);
                  }
                }
              }
            },
            buttons: [{
              text: 'Save',
              handler: function () {

                var connectorGroupSettings = [];
                Ext.each(productGroupMappingConnectorItems, function (connector) {
                  connectorGroupSettings.push({ ConnectorID: connector.connectorID, IsActive: connector.getValue() });
                });

                var genericSettingParams = [];
                var tabPanels = settingFormPanel.items.items[0].items.items;
                for (var i = 0; i < tabPanels.length; i++) {
                  var formFields = tabPanels[i].items.items;

                  for (var j = 0; j < formFields.length; j++) {
                    if (formFields[j].settingID) {
                      genericSettingParams.push({ SettingID: formFields[j].settingID, Value: formFields[j].getValue() });
                    }
                  }
                }

                var SaveParams = {
                  MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                  FilterByParentGroup: checkboxFilterByParent.getValue(),
                  FlattenHierachy: checkboxFlattenHierachy.getValue(),
                  ExportID: numberFieldExportID.getValue(),
                  FollowSystemSettings: checkboxFollowSourceMasterGroupMappingName.getValue(),
                  DisableInNavigation: checkboxDisableInNaviagtion.getValue(),
                  DisableInProductGroup: checkboxDisableInProductGroup.getValue(),
                  ListView: checkboxListView.getValue(),
                  WposConnectorMappingSettings: wposSettings,
                  ConnectorGroupSettings: connectorGroupSettings,
                  PageLayoutID: pageLayoutSelect.getValue(),
                  GenericSettings: genericSettingParams
                };

                var SaveParamsJson = JSON.stringify(SaveParams);
                Diract.request({
                  url: Concentrator.route('UpdateConnectorMappingSettings', 'MasterGroupMapping'),
                  waitMsg: 'Updating Connector Mapping Settings',
                  params: {
                    SaveParamsJson: SaveParamsJson
                  },
                  success: function (record) {
                    window.close();
                  },
                  failure: function () { }
                });
              }
            }, {
              text: 'Cancel',
              handler: function () {
                window.close();
              }
            }],
            listeners: {
              afterrender: function (thisForm) {

              }
            }
          });
          var window = new Ext.Window({
            modal: true,
            title: 'Connector Mapping "' + node.attributes.text + '" Settings',
            plain: true,
            layout: 'fit',
            resizable: true,
            width: 1500,
            height: 500,
            closable: true,
            items: settingFormPanel
          });
          window.show();
        }
      };

      checkboxFilterByParent = formFields.createFilterByParentCheckbox();
      checkboxFlattenHierachy = formFields.createFlattenHierachyCheckbox();
      numberFieldExportID = formFields.createExportIDNumberField();
      checkboxFollowSourceMasterGroupMappingName = formFields.createFollowSourceMasterGroupMappingNameCheckbox();
      checkboxDisableInNaviagtion = formFields.createDisableInNaviagtionCheckbox();
      checkboxDisableInProductGroup = formFields.createDisableInProductGroupCheckbox();
      checkboxListView = formFields.createListViewCheckbox();
      pageLayoutSelect = formFields.createPageLayoutSelect();
      showConnectorGroupSettings = true;
      showMagentoSettings = false;
      showWposSetting = false;
      productGroupMappingConnectorItems = formFields.createProductGroupConnectorItems();

      var genericSettingsPanels = [];

      var wposSettings = [];

      settingFormPanel = panels.createSettingFormPanel();

      var imageConnectors = [];

      var currentMediaType = "Image";
      var settingFormPanel;

      Diract.request({
        url: Concentrator.route('GetConnectorMappingSettings', 'MasterGroupMapping'),
        waitMsg: 'Getting Connector Mapping Settings',
        params: {
          MasterGroupMappingID: node.attributes.MasterGroupMappingID
        },
        success: function (record) {

          Ext.each(record.data, function (setting) {
            if (setting.Name == 'MasterGroupMappingSettings') {
              var ExportID = setting.ExportID || '';
              var FilterByParentGroup = setting.FilterByParentGroup || false;
              var FlattenHierarchy = setting.FlattenHierarchy || false;

              //set settings
              checkboxFlattenHierachy.setValue(FlattenHierarchy);
              checkboxFilterByParent.setValue(FilterByParentGroup);
              numberFieldExportID.setValue(ExportID);
            } else
              if (setting.Name == "MagentoSettings") {
                showMagentoSettings = true;
                //get settings
                var DisableInNaviagtion = setting.DisableInNaviagtion || false;
                var DisableInProductGroup = setting.DisableInProductGroup || false;
                var ListView = setting.ListView || false;
                var PageLayoutID = setting.PageLayoutID;

                //set settings
                checkboxDisableInNaviagtion.setValue(DisableInNaviagtion);
                checkboxDisableInProductGroup.setValue(DisableInProductGroup);
                checkboxListView.setValue(ListView);
                if (PageLayoutID) {
                  pageLayoutSelect.setValue(PageLayoutID)
                }
              } else
                if (setting.Name == "ConnectorGroupSetting") {

                  for (var i = 0; i < productGroupMappingConnectorItems.length; i++) {
                    var connectorSetting = productGroupMappingConnectorItems[i];
                    if (connectorSetting.connectorID == setting.ConnectorID) {
                      connectorSetting.setValue(false);
                    }
                  }
                } else
                  if (setting.Name == "ImageConnector") {
                    imageConnectors.push({
                      ConnectorID: setting.ConnectorID,
                      ConnectorName: setting.ConnectorName,
                      Languages: setting.Languages
                    });
                  } else
                    if (setting.Name == 'WposSettings') {

                      showWposSetting = true;

                      var wposSetting = {
                        code: setting.Code,
                        settingValue: setting.SettingValue
                      };

                      wposSettings.push(wposSetting);
                    } else
                      if (setting.Name == "ImageSettings") {
                        tabPanels.dataViewData = setting.Images;

                      } else {
                        //The generic stuff

                        
                        if (!tabPanels.panelExists(genericSettingsPanels, setting.Group)) {
                          genericSettingsPanels.push(tabPanels.createSettingsPanel(setting.Group));
                        }

                        for (var i = 0; i < genericSettingsPanels.length; i++) {
                          if (genericSettingsPanels[i].title == setting.Group) {
                            genericSettingsPanels[i].items.push(tabPanels.createSettingElement(setting));
                          }
                        }
                      }
          });

          windows.getWindow();
        },
        failure: function () { }
      });
    },
    menuItemRenameConnectorMapping: function (node) {
      config = {
        MasterGroupMappingID: node.attributes.MasterGroupMappingID,
        MasterGroupMappingName: node.attributes.text,
        treePanelToRefresh: 'ConnectorMappingTreePanel'
      };

      Concentrator.MasterGroupMappingFunctions.getRenameMasterGroupMappingWindow(config);
    },
    menuItemAddProductToConnectorMapping: function (node, connectorID) {
      // if node.attributes.mastergroupmappingid = -1 return to tabpanel

      var buttons = {
        createExitButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Exit',
            iconCls: "exit",
            handler: function () {
              window.close();
            }
          });
        },
        GetExpandAllTreeNodes: function () {
          return new Ext.Button({
            text: 'Expand All',
            iconCls: 'icon-expand',
            handler: function () {
              connectorMappingTree.expandAll();
            }
          });
        },
        GetCollapseAllTreeNodes: function () {
          return new Ext.Button({
            text: 'Collapse All',
            iconCls: 'icon-collapse',
            handler: function () {
              connectorMappingTree.collapseAll();
            }
          });
        },

        GetTreeButtons: function () {
          listOfButtons = [];
          listOfButtons.push(btnExpandAllTreeNodes);
          listOfButtons.push(btnCollapseAllTreeNodes);
          return listOfButtons;
        },
        getWindowButtons: function () {
          listOfButtons = [];
          listOfButtons.push('->');
          listOfButtons.push(btnExitWindow);
          return listOfButtons;
        }
      };
      var trees = {
        getSelectedNode: function () {
          var selectedNode = connectorMappingTree.getSelectionModel().selNode;
          return selectedNode;
        },
        GetBaseAttributes: function (node, treeLoader) {
          var params = {};

          if (!treeLoader) {
            for (var key in { ConnectorID: -1, MasterGroupMappingID: -1 }) {
              params[key] = node.attributes[key];
            }
          } else {
            for (var key in treeLoader.baseAttrs) {
              params[key] = node.attributes[key];
            }
          }

          return params;
        },
        GetTreeLoader: function () {
          return new Ext.tree.TreeLoader({
            dataUrl: Concentrator.route('GetTreeView', 'MasterGroupMapping'),
            baseAttrs: { ConnectorID: connectorID, MasterGroupMappingID: node.attributes.MasterGroupMappingID },
            listeners: {
              beforeload: (function (treeloader, node) {
                Ext.apply(treeloader.baseParams, trees.GetBaseAttributes(node));
              })
            }
          });
        },
        GetTree: function () {
          return new Ext.tree.TreePanel({
            useArrows: true,
            autoHeight: true,
            border: false,
            enableDrop: true,
            ddGroup: 'AddProductToConnectorMappingDD',
            height: 500,
            width: 300,
            root: {
              text: node.attributes.text,
              MasterGroupMappingID: node.attributes.MasterGroupMappingID,
              draggable: false,
              leaf: false
            },
            loader: trees.GetTreeLoader(),
            listeners: {
              click: function (node, dropEvent) {
                var MasterGroupMappingID = node.attributes.MasterGroupMappingID;
                gridCustomProducts.getStore().reload({ params: { MasterGroupMappingID: MasterGroupMappingID } });
              },
              beforenodedrop: function (dropEvent) {
                Ext.Msg.show({
                  title: 'Add Custom Product',
                  msg: 'Do you want to map this product to Product Group "' + dropEvent.target.attributes.text + '" ?',
                  buttons: { ok: "Yes", cancel: "No" },
                  fn: function (result) {
                    if (result == 'ok') {
                      var SaveParams = {
                        MasterGroupMappingID: dropEvent.target.attributes.MasterGroupMappingID,
                        ConnectorID: connectorID,
                        ProductIDs: []
                      };
                      Ext.each(dropEvent.data.selections, function (droppedRecord) {
                        SaveParams.ProductIDs.push(droppedRecord.get('ProductID'));
                      });

                      var SaveParamsJson = JSON.stringify(SaveParams);

                      Diract.request({
                        url: Concentrator.route('AddProductsToConnectorMapping', 'MasterGroupMapping'),
                        waitMsg: 'Mapping Product To Product Group "' + node.attributes.text + '"',
                        params: {
                          SaveParamsJson: SaveParamsJson
                        },
                        success: function () {

                        },
                        failure: function () { }
                      });

                    }
                  },
                  animEl: 'elId',
                  icon: Ext.MessageBox.QUESTION
                });
              }
            },
            tbar: [{
              text: 'Expand All',
              iconCls: 'icon-expand',
              handler: function () {
                connectorMappingTree.expandAll();
              }
            }, {
              text: 'Collapse All',
              iconCls: 'icon-collapse',
              handler: function () {
                connectorMappingTree.collapseAll();
              }
            }]
          });
        }
      }
      var panels = {
        GetGeneralPanel: function () {
          return new Ext.Panel({
            layout: 'border',
            tbar: [buttons.getWindowButtons()],
            items: [panels.GetTreePanel(), panels.GetTabPanel()]
          });
        },

        GetTreePanel: function () {
          return new Ext.Panel({
            region: 'west',
            width: 300,
            layout: 'fit',
            autoScroll: true,
            margins: '0 0 0 0',
            split: true,
            items: connectorMappingTree
          });
        },
        GetTabPanel: function () {
          return new Diract.Ext.TabPanel({
            region: 'center',
            deferredRender: false,
            defaults: { autoScroll: true },
            width: 500,
            enableTabScroll: true,
            activeTab: 0,
            items: [panels.GetTabPanelItems()],
            listeners: {
              tabchange: function (t, tab) {
                var grid = tab.getComponent('customProductsGrid');
                if (grid != null) {
                  grid.getStore().reload();
                }
              }
            }
          });
        },

        GetTabPanelItems: function () {
          listOfTabs = [];
          listOfTabs.push(panels.GetTabSourceMasterGroupMappingProducts());
          listOfTabs.push(panels.GetTabProducts());
          listOfTabs.push(panels.GetTabCustomProducts());
          return listOfTabs;
        },
        GetTabSourceMasterGroupMappingProducts: function () {
          return new Ext.Panel({
            title: 'Source Master Group Mapping Products',
            layout: 'fit',
            items: [grids.getSourceMasterGroupMappingProductsGrid()]
          });
        },
        GetTabProducts: function () {
          return new Ext.Panel({
            title: 'Products',
            layout: 'fit',
            items: [grids.getProductsGrid()]
          });
        },
        GetTabCustomProducts: function () {
          return new Ext.Panel({
            title: 'Custom Products',
            layout: 'fit',
            items: [gridCustomProducts]
          });
        }
      }
      var grids = {
        getSourceMasterGroupMappingProductsGrid: function () {
          var gridMenu = new Ext.menu.Menu({
            items: [{
              id: 'ViewProductDetailsInSourceProductGrid',
              text: 'View Product Details',
              iconCls: 'view'
            }],
            listeners: {
              itemclick: function (item) {
                var selectedRecord = grid.getSelectionModel().getSelected();

                if (selectedRecord) {
                  switch (item.id) {
                    case 'ViewProductDetailsInSourceProductGrid':
                      var factory = new Concentrator.ProductBrowserFactory({ productID: selectedRecord.get('ProductID') });
                      break;
                  }
                }
              }
            }
          });
          var grid = new Diract.ui.Grid({
            pluralObjectName: 'Products',
            singularObjectName: 'Product',
            primaryKey: 'ProductID',
            sortBy: 'ProductID',
            ddGroup: 'AddProductToConnectorMappingDD',
            enableDragDrop: true,
            params: {
              MasterGroupMappingID: node.attributes.MasterGroupMappingID
            },
            url: Concentrator.route("GetListOfSourceMasterGroupMappingProducts", "MasterGroupMapping"),
            forceFit: true,
            permissions: {
              list: 'GetProductGroupVendor',
              create: 'CreateProductGroupVendor',
              remove: 'DeleteProductGroupVendor',
              update: 'UpdateProductGroupVendor'
            },
            structure: [{
              dataIndex: 'ProductID',
              type: 'int',
              header: 'Product ID'
            }, {
              dataIndex: 'ProductName',
              header: 'Product Name'
            }, {
              dataIndex: 'VendorItemNumber',
              header: 'Vendor Item Number'
            }, {
              dataIndex: 'ShortDescription',
              header: 'Short Description',
              renderer: function (val, metadata, record) {
                return '\'' + val + '\'';
              }
            }, {
              dataIndex: 'LongDescription',
              header: 'Long Description',
              renderer: function (val, metadata, record) {
                return '\'' + val + '\'';
              }
            }, {
              dataIndex: 'IsConfigurable',
              header: 'Is Configurable',
              type: 'boolean'
            }],
            listeners: {
              beforedestroy: function () {
                if (gridMenu) {
                  gridMenu.destroy();
                  delete gridMenu;
                }
              },
              rowcontextmenu: function (thisGrid, index, event) {
                thisGrid.getSelectionModel().selectRow(index);
                event.stopEvent();
                gridMenu.showAt(event.xy);
              }
            }
          });
          return grid;
        },
        getProductsGrid: function () {
          var gridMenu = new Ext.menu.Menu({
            items: [{
              id: 'ViewProductDetailsInProductGrid',
              text: 'View Product Details',
              iconCls: 'view'
            }],
            listeners: {
              itemclick: function (item) {
                var selectedRecord = grid.getSelectionModel().getSelected();

                if (selectedRecord) {
                  switch (item.id) {
                    case 'ViewProductDetailsInProductGrid':
                      var factory = new Concentrator.ProductBrowserFactory({ productID: selectedRecord.get('ProductID') });
                      break;
                  }
                }
              }
            }
          });

          var grid = new Diract.ui.Grid({
            pluralObjectName: 'Products',
            singularObjectName: 'Product',
            primaryKey: 'ProductID',
            sortBy: 'ProductID',
            ddGroup: 'AddProductToConnectorMappingDD',
            enableDragDrop: true,
            params: {
              ConnectorID: connectorID
            },
            url: Concentrator.route("GetListOfProducts", "MasterGroupMapping"),
            forceFit: true,
            permissions: {
              list: 'GetProductGroupVendor',
              create: 'CreateProductGroupVendor',
              remove: 'DeleteProductGroupVendor',
              update: 'UpdateProductGroupVendor'
            },
            structure: [{
              dataIndex: 'ProductID',
              type: 'int',
              header: 'Product ID'
            }, {
              dataIndex: 'ProductName',
              header: 'Product Name'
            }, {
              dataIndex: 'VendorItemNumber',
              header: 'Vendor Item Number'
            }, {
              dataIndex: 'ShortDescription',
              header: 'Short Description',
              renderer: function (val, metadata, record) {
                return '\'' + val + '\'';
              }
            }, {
              dataIndex: 'LongDescription',
              header: 'Long Description',
              renderer: function (val, metadata, record) {
                return '\'' + val + '\'';
              }
            }, {
              dataIndex: 'IsConfigurable',
              header: 'Is Configurable',
              type: 'boolean'
            }],
            listeners: {
              beforedestroy: function () {
                if (gridMenu) {
                  gridMenu.destroy();
                  delete gridMenu;
                }
              },
              rowcontextmenu: function (thisGrid, index, event) {
                thisGrid.getSelectionModel().selectRow(index);
                event.stopEvent();
                gridMenu.showAt(event.xy);
              }
            }
          });
          return grid;
        },
        getCustomProductsGrid: function () {
          var menuCustomProductGrid = new Ext.menu.Menu({
            items: [{
              id: 'removeCustomProduct',
              text: 'Remove Custom Product',
              iconCls: 'delete'
            }, {
              id: 'ViewProductDetails',
              text: 'View Product Details',
              iconCls: 'view'
            }],
            listeners: {
              itemclick: function (item) {
                var selectedRecord = grid.getSelectionModel().getSelected();

                if (selectedRecord) {
                  switch (item.id) {
                    case 'removeCustomProduct':
                      productID = selectedRecord.get('ProductID');
                      functions.removeCustomProducts(productID);
                      break;
                    case 'ViewProductDetails':
                      var factory = new Concentrator.ProductBrowserFactory({ productID: selectedRecord.get('ProductID') });
                      break;
                  }
                }
              }
            }
          });
          var grid = new Diract.ui.Grid({
            pluralObjectName: 'Products',
            singularObjectName: 'Product',
            primaryKey: 'ProductID',
            sortBy: 'ProductID',
            autoLoadStore: false,
            id: 'customProductsGrid',
            params: {
              MasterGroupMappingID: node.attributes.MasterGroupMappingID
            },
            url: Concentrator.route("GetListOfCustomProducts", "MasterGroupMapping"),
            forceFit: true,
            permissions: {
              list: 'GetProductGroupVendor',
              create: 'CreateProductGroupVendor',
              remove: 'DeleteProductGroupVendor',
              update: 'UpdateProductGroupVendor'
            },
            structure: [{
              dataIndex: 'IsExported',
              header: 'Will be exported',
              renderer: function (val, metadata, record) {
                if (val) {
                  return '<div class="rowAcceptIcon"/>';
                } else {
                  return '<div class="rowCancelIcon"/>';
                }
              }
            }, {
              dataIndex: 'ProductID',
              type: 'int',
              header: 'Product ID'
            }, {
              dataIndex: 'ProductName',
              header: 'Product Name'
            }, {
              dataIndex: 'VendorItemNumber',
              header: 'Vendor Item Number'
            }, {
              dataIndex: 'ShortDescription',
              header: 'Short Description',
              renderer: function (val, metadata, record) {
                return '\'' + val + '\'';
              }
            }, {
              dataIndex: 'LongDescription',
              header: 'Long Description',
              renderer: function (val, metadata, record) {
                return '\'' + val + '\'';
              }
            },
             {
               dataIndex: 'IsConfigurable',
               header: 'Is Configurable',
               type: 'boolean'
             }],
            listeners: {
              beforedestroy: function () {
                if (menuCustomProductGrid) {
                  menuCustomProductGrid.destroy();
                  delete menuCustomProductGrid;
                }
              },
              rowcontextmenu: function (thisGrid, index, event) {
                thisGrid.getSelectionModel().selectRow(index);
                event.stopEvent();
                menuCustomProductGrid.showAt(event.xy);
              }
            }
          });
          return grid;
        }
      }
      var functions = {
        removeCustomProducts: function (productID) {
          var selectedNode = trees.getSelectedNode();
          if (selectedNode) {
            Ext.Msg.show({
              title: 'Remove Custom Product',
              msg: 'Do you want to remove this product from product group "' + selectedNode.attributes.text + '" ?',
              buttons: { ok: "Yes", cancel: "No" },
              fn: function (result) {
                if (result == 'ok') {
                  Diract.request({
                    url: Concentrator.route('RemoveCustomProduct', 'MasterGroupMapping'),
                    waitMsg: 'Removing Custom Product from Product Group "' + selectedNode.attributes.text + '"',
                    params: {
                      MasterGroupMappingID: selectedNode.attributes.MasterGroupMappingID,
                      ProductID: productID
                    },
                    success: function () {
                      gridCustomProducts.getStore().reload();
                    },
                    failure: function () { }
                  });

                }
              },
              animEl: 'elId',
              icon: Ext.MessageBox.QUESTION
            });
          } else {
            console.log(selectedNode);
          }
        }
      }

      var connectorMappingTree = trees.GetTree();
      var btnExpandAllTreeNodes = buttons.GetExpandAllTreeNodes();
      var btnCollapseAllTreeNodes = buttons.GetCollapseAllTreeNodes();
      var btnExitWindow = buttons.createExitButton();

      var gridCustomProducts = grids.getCustomProductsGrid();

      var window = new Ext.Window({
        width: 800,
        height: 500,
        title: 'Add Product To Product Group "' + node.attributes.text + '"',
        modal: true,
        maximizable: true,
        resize: true,
        items: panels.GetGeneralPanel(),
        layout: 'fit'
      });
      window.show();
    },
    menuItemChooseSourceMasterGroupMapping: function (node) {
      var masterGroupMappingTree = Ext.getCmp('masterGroupMappingTreePanel');
      var selectedNode = masterGroupMappingTree.getSelectionModel().lastSelNode;

      if (node.attributes.MasterGroupMappingID > -1) {
        if (selectedNode) {
          var title = 'Set Source Master Group Mapping';
          var msg = 'Are you sure you want to set Master Group Mapping "' + selectedNode.attributes.text + '" as Source Master Group Mapping for Product Group Mapping "' + node.attributes.text + '"?';
          Ext.Msg.confirm(title, msg, function (button) {
            if (button == "yes") {
              Diract.mute_request({
                url: Concentrator.route("SetSourceMasterGroupMapping", "MasterGroupMapping"),
                params: {
                  MasterGroupMappingID: selectedNode.attributes.MasterGroupMappingID,
                  ProductGroupMappingID: node.attributes.MasterGroupMappingID
                },
                waitMsg: 'Setting the source master group mapping',
                success: function () {
                  connectorTree = Ext.getCmp('connectorMappingTreePanel');
                  config = {
                    MasterGroupMappingIDs: [],
                    RootNode: connectorTree.getRootNode()
                  };
                  config.MasterGroupMappingIDs.push(node.attributes.MasterGroupMappingID);
                  Concentrator.MasterGroupMappingFunctions.refreshTree(config);
                },
                failure: function () {
                }
              });
            }
          });
        } else {
          Ext.Msg.alert(
            'Select one Master Group Mapping',
            'Please select  one master group mapping to set as Source Master Group Mapping for product group mapping "' + node.attributes.text + '".');
        }
      }
    },
    menuItemSetMasterGroupMappingDescription: function (node) {
      var wind = new Ext.Window({
        title: 'Translation management for ' + node.attributes.MasterGroupMappingName,
        items: [
          new Diract.ui.Grid({
            url: Concentrator.route('GetMasterGroupMappingDescriptions', 'MasterGroupMapping'),
            params: {
              MasterGroupMappingID: node.attributes.MasterGroupMappingID
            },
            updateUrl: Concentrator.route('SetMasterGroupMappingDescriptions', 'MasterGroupMapping'),
            primaryKey: ['MasterGroupMappingID', 'LanguageID'],
            sortBy: 'MasterGroupMappingID',
            permissions: { all: 'Default' },
            singularObjectName: 'Master group mapping' + ' translation',
            pluralObjectName: 'Master group mapping' + ' translations',
            structure: [
                        { dataIndex: 'MasterGroupMappingID', type: 'int' },
                        { dataIndex: 'Language', type: 'string', header: 'Language' },
                        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textarea' } },
                        { dataIndex: 'LanguageID', type: 'int' }
            ]
          })
        ],
        width: 700,
        height: 500,
        layout: 'fit'
      });

      wind.show();
    },
    menuItemSeoTexts: function (node) {

      var wind = new Diract.ui.FormWindow({
        title: 'Seo Text Management',
        url: Concentrator.route('UpdateSeoTexts', 'MasterGroupMapping'),
        loadUrl: Concentrator.route('GetSeoTexts', 'MasterGroupMapping'),
        loadParams: {
          masterGroupMappingID: node.attributes.MasterGroupMappingID,
          languageID: Concentrator.user.languageID,
          ConnectorID: Concentrator.user.connectorID
        },
        params: {
          masterGroupMappingID: node.attributes.MasterGroupMappingID
        },
        cancelButton: true,
        buttonText: 'Save',
        items: [
        {
          name: 'ConnectorID',
          xtype: 'connectorWithFilter',
          valueField: 'ID',
          value: Concentrator.user.connectorID,
          fieldLabel: 'Connector',
          width: 402,
          listeners: {
            'select': function (combo, rec, idx) {
              wind.loadParams.ConnectorID = rec.get('ID');
              wind.form.loadForm(wind.loadParams);
            }
          }
        }, {
          name: 'LanguageID',
          xtype: 'language',
          valueField: 'ID',
          value: Concentrator.user.languageID,
          fieldLabel: 'Language',
          width: 402,
          listeners: {
            'select': function (combo, rec, idx) {
              wind.loadParams.languageID = rec.get('ID');
              wind.form.loadForm(wind.loadParams);
            }
          }
        },
        {
          fieldLabel: 'Description1',
          xtype: 'ckeditor',
          name: 'Description',
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
          fieldLabel: 'Pagetitle',
          xtype: 'textfield',
          name: 'Meta_title',
          width: 402,
          height: 50
        },
        {
          fieldLabel: 'Metadescription',
          xtype: 'ckeditor',
          name: 'Meta_description',
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
          name: 'Description2',
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
          name: 'Description3',
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
    menuItemCustomLabels: function (node) {

      var grid = new Diract.ui.Grid({
        url: Concentrator.route('GetCustomLabels', 'MasterGroupMapping'),
        params: {
          MasterGroupMappingID: node.attributes.MasterGroupMappingID
        },
        updateUrl: Concentrator.route('SetCustomLabel', 'MasterGroupMapping'),
        primaryKey: ['MasterGroupMappingID', 'LanguageID', 'ConnectorID'],
        sortBy: 'ConnectorID',
        permissions: { all: 'Default' },
        singularObjectName: 'Master Group Mapping custom label',
        pluralObjectName: 'Master Group Mapping custom label',
        structure: [
                    { dataIndex: 'MasterGroupMappingID', type: 'int' },
                    { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', renderer: function (val, m, rec) { return rec.get('ConnectorName') }, editable: false },
                    { dataIndex: 'Language', type: 'string', header: 'Language' },
                    { dataIndex: 'CustomLabel', type: 'string', header: 'Label', editor: { xtype: 'textarea' } },
                    { dataIndex: 'LanguageID', type: 'int' },
                    { dataIndex: 'ConnectorName', type: 'string' }
        ]
      });


      grid.getStore().addListener('load', function (thisStore) {
        var data = { Language: 'All Languages', _MasterGroupMappingID: -1, _ConnectorID: -1, _LanguageID: -1 };

        var record = new thisStore.recordType(data, -1);
        thisStore.insert(0, record);

      });

      grid.getStore().addListener('update', function (thisStore, rec) {
        if (rec.get('Language') == 'All Languages') {
          thisStore.each(function (langRec) {
            if (langRec.get('ConnectorID') == -1)
              langRec.set('CustomLabel', rec.get('CustomLabel'));
          });
        }
      });

      var wind = new Ext.Window({
        title: 'Custom label management',
        items: [grid],
        width: 700,
        height: 500,
        layout: 'fit',
        listeners: {
          'close': function (window) {
            if (config.treePanelToRefresh == 'MasterGroupMappingTreePanel') {
              panels = {
                treePanel: true,
                MasterGroupMappingID: config.MasterGroupMappingID
              };
            } else {
              panels = {
                connectorTreePanel: true
              };
            }
            Concentrator.MasterGroupMappingFunctions.refreshAction(panels);

          }
        }
      });

      wind.show();
    },
    menuItemFindSourceMasterGroupMapping: function (node) {
      var SourceMasterGroupMappingID = node.attributes.SourceMasterGroupMappingID;
      if (SourceMasterGroupMappingID) {
        Concentrator.MasterGroupMappingFunctions.showTreeNodeByMasterGroupMappingID(SourceMasterGroupMappingID);
      } else {
        Ext.Msg.alert('Finding Source Master Group Mapping', 'Connector Mapping "' + node.attributes.text + '" has not Source Master Group Mapping. This Connector Mapping added by Connector User.');
      }
    },
    menuItemImportProductGroupMappings: function (connectorID, connectorName, root) {

      var functions = {
        createToRootRadioField: function () {
          return new Ext.form.Radio({
            checked: true,
            fieldLabel: 'Import Product Group Mappings To',
            boxLabel: 'Connector Mapping Root',
            name: 'ImportProductGroup',
            inputValue: 'toRoot',
            listeners: {
              check: function (thisEvent, checked) {
                functions.setVisibleTextFieldNewProductGroup(!checked);
              }
            }
          });
        },
        createToNewProductGroupRadioField: function () {
          return new Ext.form.Radio({
            checked: false,
            fieldLabel: '',
            boxLabel: 'New Product Group',
            name: 'ImportProductGroup',
            inputValue: 'toNewProductGroup'
          });
        },
        createNewProductGroupNameTextField: function () {
          return new Ext.form.TextField({
            name: 'newProductGroup',
            hidden: true
          });
        },

        setVisibleTextFieldNewProductGroup: function (visible) {
          textFieldNewProductGroup.setVisible(visible);
        },
        getFormFields: function () {
          listOfFields = new Array();

          listOfFields.push(radioFieldToRoot);
          listOfFields.push(radioFieldToNewProductGroup);
          listOfFields.push(textFieldNewProductGroup);

          return listOfFields;
        }
      };

      radioFieldToRoot = functions.createToRootRadioField();
      radioFieldToNewProductGroup = functions.createToNewProductGroupRadioField();
      textFieldNewProductGroup = functions.createNewProductGroupNameTextField();

      var importFormPanel = new Ext.FormPanel({
        labelWidth: 200,
        border: false,
        width: 350,
        items: {
          xtype: 'fieldset',
          title: 'Import Options:',
          items: [functions.getFormFields()]
        },
        buttons: [{
          text: 'Import Product Groups',
          handler: function () {

            var SaveParams = {
              connectorID: connectorID,
              importTo: '',
              newProductGroupName: ''
            };

            if (radioFieldToRoot.getValue()) {
              SaveParams.importTo = 'root';
            }

            if (radioFieldToNewProductGroup.getValue()) {
              SaveParams.importTo = 'new';
              SaveParams.newProductGroupName = textFieldNewProductGroup.getValue();
            }
            var SaveParamsJson = JSON.stringify(SaveParams);
            Diract.request({
              url: Concentrator.route('ImportProductGroupMappings', 'MasterGroupMapping'),
              waitMsg: 'Importing product group mapping from connector',
              params: {
                SaveParamsJson: SaveParamsJson
              },
              success: function (record) {
                root.reload();
                window.close();
              },
              failure: function () { }
            });
          }
        }, {
          text: 'Cancel',
          handler: function () {
            window.close();
          }
        }]
      });
      var window = new Ext.Window({
        modal: true,
        title: 'Import Product Group Mappings From Connector "' + connectorName + '"',
        plain: true,
        layout: 'fit',
        resizable: true,
        width: 450,
        height: 187,
        closable: false,
        items: importFormPanel
      });
      window.show();


    },

    // Main Panel                   
    getPanel: function () {
      var connectorID = 0;
      var connectorName = '';
      var thisCmp = this;
      if (Concentrator.user.connectorID) {
        connectorID = Concentrator.user.connectorID;
      } else {
        connectorID = this.connectorID;
      }

      var treeLoader = new Ext.tree.TreeLoader({
        dataUrl: Concentrator.route('GetTreeView', 'MasterGroupMapping'),
        baseAttrs: { ConnectorID: connectorID, MasterGroupMappingID: -1 },
        listeners: {
          beforeload: (function (treeloader, node) {
            Ext.apply(treeloader.baseParams, this.getBaseAttributes(node));
          }).createDelegate(this)
        }
      });
      var contextMenu = new Diract.Ext.menu.Menu({
        items: [
        {
          id: 'menuItemAddNewConnectorMapping',
          text: 'Add New Product Group',
          iconCls: 'add',
          xtype: 'menuitem',
          requires: functionalities.AddConnectorMapping
        }, {
          id: 'menuItemDeleteConnectorMapping',
          text: 'Delete Product Group',
          iconCls: 'delete',
          xtype: 'menuitem',
          requires: functionalities.DeleteConnectorMapping
        }, {
          id: 'menuItemFindConnectorMapping',
          text: 'Find Product Group',
          iconCls: 'view',
          xtype: 'menuitem',
          requires: functionalities.FindSource
        }, '-', {
          id: 'menuItemAddProductToConnectorMapping',
          text: 'Add Product To Product Group',
          iconCls: 'add',
          xtype: 'menuitem',
          requires: functionalities.AddConnectorMapping
        }, '-', {
          id: 'menuItemSettingConnectorMapping',
          text: 'Product Group Setting',
          iconCls: 'menuItem-setting',
          xtype: 'menuitem',
          requires: functionalities.ProductGroupSettings
        }, {
          id: 'menuItemRenameConnectorMapping',
          text: 'Rename Product Group',
          iconCls: 'menuItem-Rename',
          xtype: 'menuitem',
          requires: functionalities.RenameConnectorMapping
        }, '-',
        {
          id: 'menuItemSetMasterGroupMappingDescription',
          text: 'Translate Description',
          iconCls: 'magic-wand',
          xtype: 'menuitem',
          requires: functionalities.SetMasterGroupMappingDescription
        }, {
          id: 'menuItemSeoTexts',
          text: 'Manage SEO Texts',
          iconCls: 'magic-wand',
          xtype: 'menuitem',
          requires: functionalities.ManageSeoTexts
        }, {
          id: 'menuItemCustomLabels',
          text: 'Custom Labels',
          iconCls: 'magic-wand',
          xtype: 'menuitem',
          requires: functionalities.ManageSeoTexts
        }
        , '-',
        {
          id: 'menuItemChooseSourceMasterGroupMapping',
          text: 'Choose Source Master Group Mapping',
          iconCls: 'menuItem-folderEdit',
          xtype: 'menuitem',
          requires: functionalities.ChooseSource
        }, {
          id: 'menuItemFindSourceMasterGroupMapping',
          text: 'Find Source Master Group Mapping',
          iconCls: 'view',
          xtype: 'menuitem',
          requires: functionalities.FindSource
        }],
        listeners: {
          destroy: function () { },
          itemClick: (function (item) {
            var node = connectorTree.getSelectionModel().lastSelNode;
            switch (item.id) {
              case 'menuItemAddNewConnectorMapping':
                this.menuItemAddNewConnectorMapping(node, connectorID);
                break;
              case 'menuItemDeleteConnectorMapping':
                this.menuItemDeleteConnectorMapping(node);
                break;
              case 'menuItemFindConnectorMapping':
                this.menuItemFindConnectorMapping(connectorID);
                break;
              case 'menuItemSettingConnectorMapping':
                this.menuItemSettingConnectorMapping(node);
                break;
              case 'menuItemRenameConnectorMapping':
                this.menuItemRenameConnectorMapping(node);
                break;
              case 'menuItemAddProductToConnectorMapping':
                this.menuItemAddProductToConnectorMapping(node, connectorID);
                break;
              case 'menuItemFindSourceMasterGroupMapping':
                this.menuItemFindSourceMasterGroupMapping(node);
                break;
              case 'menuItemImportProductGroupMappings':
                this.menuItemImportProductGroupMappings(connectorID, connectorName, connectorTree.getRootNode());
                break;
              case 'menuItemChooseSourceMasterGroupMapping':
                this.menuItemChooseSourceMasterGroupMapping(node);
                break;
              case 'menuItemSetMasterGroupMappingDescription':
                this.menuItemSetMasterGroupMappingDescription(node);
                break;
              case 'menuItemSeoTexts':
                this.menuItemSeoTexts(node);
                break;
              case 'menuItemCustomLabels':
                this.menuItemCustomLabels(node);
                break;
              case 'menuItemViewProductGroupDetails':
                this.menuItemViewProductGroupDetails();
                break;
            }
          }).createDelegate(this)
        }
      });

      var isUserAuthorizedToDropOnConnectorTree = function () {
        var required = [functionalities.MoveProductGroupMapping, functionalities.CopyProductGroupMapping];
        var hasOneRequiredFunctionality = Enumerable.From(required).Any("Diract.user.hasFunctionality($)");
        return hasOneRequiredFunctionality;
      };

      var validateConnectorTreeDropEvent = function (dropEvent) {
        if (!isUserAuthorizedToDropOnConnectorTree()) {
          dropEvent.Cancel = true;
          return false;
        }
        return true;
      };
      var that = this;
      var connectorTree = new Ext.tree.TreePanel({
        useArrows: true,
        autoHeight: true,
        border: false,
        ddGroup: 'MasterGroupMappingDD',
        id: 'connectorMappingTreePanel',
        selModel: new Ext.tree.MultiSelectionModel(),
        width: 300,
        enableDD: true,
        ddAppendOnly: false,
        trackMouseOver: true,
        animate: true,
        containerScroll: true,
        root: {
          text: 'Product Group',
          draggable: false,
          leaf: false
        },
        loader: treeLoader,
        contextMenu: contextMenu,
        listeners: {
          click: function (node, dropEvent) {
            var MasterGroupMappingID = node.attributes.MasterGroupMappingID;

            var connectorMappingGrid = Ext.getCmp('ConnectorMappingProducts');
            var connectorGroupAttributeGrid = Ext.getCmp('connectorProductGroupAttributeGrid');
            var masterGroupMappingPriceTagPanel = Ext.getCmp('MasterGroupPriceTagPanel');

            if (!!masterGroupMappingPriceTagPanel)
              masterGroupMappingPriceTagPanel.loadPanel(MasterGroupMappingID);
            if (!!connectorGroupAttributeGrid)
              connectorGroupAttributeGrid.store.reload({ params: { MasterGroupMappingID: MasterGroupMappingID } });
            if (!!connectorMappingGrid) {
              connectorMappingGrid.store.baseParams.MasterGroupMappingID = MasterGroupMappingID;
              connectorMappingGrid.store.baseParams.ConnectorID = connectorID;

              connectorMappingGrid.store.reload();
              connectorMappingGrid.setDisabled(false);
            }
          },
          nodedragover: function (dropEvent) {
            return validateConnectorTreeDropEvent(dropEvent);
          },
          beforenodedrop: function (dropEvent) {
            if (dropEvent.source.tree != undefined) {
              var treeName = dropEvent.source.tree.id;

              if (treeName == "masterGroupMappingTreePanel") {
                var node = dropEvent.dropNode;
                var targetNode = dropEvent.target;

                Ext.Msg.show({
                  title: 'Copy Master Group Mapping To Connector Mapping',
                  msg: 'Do you also want to copy the sub Master Group Mappings of Master Group Mapping"' + node.attributes.text + '" to Connector Mapping "' + targetNode.attributes.text + '"?',
                  buttons: { ok: "Yes", cancel: "No" },
                  fn: function (result) {
                    if (result == 'ok') {
                      var copyChildren = true;
                    } else {
                      var copyChildren = false;
                    }

                    var SaveParams = {
                      ConnectorID: connectorID,
                      MasterGroupMappingID: node.attributes.MasterGroupMappingID,
                      ParentConnectorMappingID: targetNode.attributes.MasterGroupMappingID,
                      copyChildren: copyChildren
                    };
                    var SaveParamsJson = JSON.stringify(SaveParams);

                    Diract.silent_request({
                      url: Concentrator.route('AddMasterGroupMappingToConnectorMapping', 'MasterGroupMapping'),
                      waitMsg: 'Saving',
                      params: {
                        SaveParamsJson: SaveParamsJson
                      },
                      success: function (record) {
                        targetNode.reload();
                      }
                    });
                  },
                  animEl: 'elId',
                  icon: Ext.MessageBox.QUESTION
                });
                dropEvent.cancel = true;
              }
            }
          },
          nodedrop: function (dropEvent) {
            if (dropEvent.tree != undefined) {
              var treeName = dropEvent.tree.id;
              if (treeName == 'connectorMappingTreePanel') {
                var dropNode = dropEvent.dropNode;
                var parameters = {
                  DroppedMasterGroupMappingID: dropNode.attributes.MasterGroupMappingID,
                  ParentMasterGroupMappingID: dropNode.parentNode.attributes.MasterGroupMappingID,
                  childrenOfParenMasterGroupMappingID: []
                };

                var form = new Diract.Ext.form.FormPanel({
                  baseCls: 'x-plain',
                  labelWidth: 10,
                  defaultType: 'radiogroup',
                  items: [{
                    fieldLabel: '',
                    columns: 1,
                    items: [{
                      boxLabel: 'Move Product Group',
                      name: 'action',
                      inputValue: 'move',
                      checked: true,
                      requires: functionalities.MoveProductGroupMapping
                    }, {
                      boxLabel: 'Copy Product Group',
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
                      },
                      requires: functionalities.CopyProductGroupMapping
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
                      }]
                    }]
                  }],
                  buttons: ["->", {
                    text: 'Save',
                    handler: function () {
                      var formInput = form.getForm().getValues();
                      switch (formInput.action) {
                        case 'move':
                          {
                            Ext.each(dropNode.parentNode.childNodes, function (node) {
                              parameters.childrenOfParenMasterGroupMappingID.push(node.attributes.MasterGroupMappingID);
                            });
                            var parametersJson = JSON.stringify(parameters);
                            Diract.silent_request({
                              url: Concentrator.route('UpdateMasterGroupMapping', 'MasterGroupMapping'),
                              params: {
                                SaveParamsJson: parametersJson
                              },
                              success: function (response) {
                                window.close();
                                config = {
                                  MasterGroupMappingIDs: [],
                                  RootNode: connectorTree.getRootNode()
                                };
                                config.MasterGroupMappingIDs.push(parameters.DroppedMasterGroupMappingID);
                                Concentrator.MasterGroupMappingFunctions.refreshTree(config);
                              }
                            });
                          }
                          break;
                        case 'copy':
                          {
                            CopyAttributes = false;
                            CopyProducts = false;
                            copyCrossReferences = false;

                            if (formInput.products) {
                              CopyProducts = true;
                            }

                            DDMasterGroupMappingID = parameters.DroppedMasterGroupMappingID;
                            ParentMasterGroupMappingID = parameters.ParentMasterGroupMappingID;

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
                                window.close();

                                config = {
                                  MasterGroupMappingIDs: [],
                                  RootNode: connectorTree.getRootNode()
                                };
                                config.MasterGroupMappingIDs.push(parameters.DroppedMasterGroupMappingID);
                                config.MasterGroupMappingIDs.push(parameters.ParentMasterGroupMappingID);

                                Concentrator.MasterGroupMappingFunctions.refreshTree(config);
                              }
                            });
                          }
                          break;
                      }
                    }
                  }, {
                    text: 'Cancel',
                    handler: function () {
                      window.close();

                      config = {
                        MasterGroupMappingIDs: [],
                        RootNode: connectorTree.getRootNode()
                      };
                      config.MasterGroupMappingIDs.push(parameters.DroppedMasterGroupMappingID);
                      config.MasterGroupMappingIDs.push(parameters.ParentMasterGroupMappingID);

                      Concentrator.MasterGroupMappingFunctions.refreshTree(config);
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
            }
          },

          beforedestroy: (function () {
            if (contextMenu) {
              contextMenu.destroy();
              delete contextMenu;
            }
          }).createDelegate(this),
          contextmenu: (function (node, evt) {
            node.select();
            var c = node.getOwnerTree().contextMenu;
            if (!c.el)
              c.render();

            c.NodeID = node.id;
            c.attributes = node.attributes;
            c.contextNode = node;

            c.showAt(evt.getXY());

          }).createDelegate(this),

          afterrender: function (thisTree) {
            Diract.silent_request({
              url: Concentrator.route('GetConnector', 'MasterGroupMapping'),
              params: {
                connectorID: connectorID
              },
              success: function (record) {
                if (record.data.ParentConnectorID) {
                  connectorTree.getRootNode().setText(record.data.Name + ' (Parent Connector: ' + record.data.ParentConnectorName + ')');
                } else {
                  connectorTree.getRootNode().setText(record.data.Name);
                }
                connectorTree.getRootNode().expand();
                connectorName = record.data.Name;
                thisCmp.reloadAllConnectorMappingTabs();
              }
            });
          }
        },
        tbar: [{
          text: 'Expand All',
          iconCls: 'icon-expand',
          handler: function () {
            connectorTree.expandAll();
          }
        }, {
          text: 'Collapse All',
          iconCls: 'icon-collapse',
          handler: function () {
            connectorTree.collapseAll();
          }
        }]
      });

      var treePanel = new Ext.Panel({
        region: 'west',
        width: 300,
        layout: 'fit',
        autoScroll: true,
        margins: '0 0 0 0',
        split: true,
        collapsible: true,
        collapsed: false,
        items: connectorTree
      });

      var tabPanels = new Diract.Ext.TabPanel({
        region: 'center',
        deferredRender: false,
        defaults: { autoScroll: true },
        width: 400,
        enableTabScroll: true,
        items: this.getTabPanelItems(connectorID),
        listeners: {
          tabchange: function (t, tab) {
          }
        }
      });

      var panel = new Ext.Panel({
        layout: 'border',
        items: [treePanel, tabPanels]
      });
      tabPanels.setActiveTab(0);
      return panel;
    }
  });

  Concentrator.ConnectorMappingProducts = Ext.extend(Concentrator.BaseAction, {
    loadLazy: true,
    getPanel: function () {
      var vendorProductGridMenu = new Ext.menu.Menu({
        items: [{
          id: 'ViewProductDetails',
          text: 'View Product Details',
          iconCls: 'view'
        }, '-', {
          id: 'Enable',
          text: 'Enable for export',
          iconCls: 'menuItem-Approve',
          disabled: true
        }, {
          id: 'Disable',
          text: 'Disable for export',
          iconCls: 'menuItem-Reject',
          disabled: true
        }],
        listeners: {
          itemclick: function (item) {
            var selectedRecord = grid.getSelectionModel().getSelected();

            if (selectedRecord) {
              switch (item.id) {
                case 'ViewProductDetails':
                  var factory = new Concentrator.ProductBrowserFactory({ productID: selectedRecord.get('ProductID') });
                  break;
                case 'Enable':
                  Diract.request({
                    url: Concentrator.route('SetProductExportFlag', 'MasterGroupMapping'),
                    waitMsg: 'Updating product to be exported',
                    params: {
                      ContentProductGroupID: selectedRecord.get('ContentProductGroupID'),
                      IsExported: true
                    },
                    callback: function () {
                      //config = {
                      //  connectorTreePanel: true
                      //};
                      grid.store.reload({ params: { MasterGroupMappingID: selectedRecord.get('MasterGroupMappingID') } });
                      //Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                    }
                  });
                  break;
                case 'Disable':
                  Diract.request({
                    url: Concentrator.route('SetProductExportFlag', 'MasterGroupMapping'),
                    waitMsg: 'Updating product to not be exported',
                    params: {
                      ContentProductGroupID: selectedRecord.get('ContentProductGroupID'),
                      IsExported: false
                    },
                    callback: function () {
                      grid.store.reload({ params: { MasterGroupMappingID: selectedRecord.get('MasterGroupMappingID') } });
                      //config = {
                      //  connectorTreePanel: true
                      //};
                      //Concentrator.MasterGroupMappingFunctions.refreshAction(config);
                    }
                  });
                  break;
              }
            }
          }
        }
      });
      var grid = new Diract.ui.ExcelGrid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        primaryKey: 'ContentProductGroupID',
        sortBy: 'ProductID',
        disabled: true,
        id: 'ConnectorMappingProducts',
        autoLoadStore: false,
        url: Concentrator.route("GetListOfConnectorMappingProducts", "MasterGroupMapping"),
        updateUrl: Concentrator.route("UpdateConnectorMappingProducts", "MasterGroupMapping"),
        forceFit: true,
        permissions: {
          list: 'GetProductGroupVendor',
          create: 'CreateProductGroupVendor',
          remove: 'DeleteProductGroupVendor',
          update: 'UpdateProductGroupVendor'
        },
        structure: [{
          dataIndex: 'ContentProductGroupID',
          type: 'int'
        }, {
          dataIndex: 'IsExported',
          header: 'Will Be Exported',
          type: 'boolean',
          editable: true,
          useIcon: true,

        }, {
          dataIndex: 'ProductID',
          type: 'int',
          header: 'Product ID'
        }, {
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'ProductName',
          header: 'Product Name'
        }, {
          dataIndex: 'VendorItemNumber',
          header: 'Vendor Item Number'
        }, {
          dataIndex: 'ShortDescription',
          header: 'Short Description',
          renderer: function (val, metadata, record) {
            return '\'' + val + '\'';
          }
        }, {
          dataIndex: 'LongDescription',
          header: 'Long Description',
          renderer: function (val, metadata, record) {
            return '\'' + val + '\'';
          }
        },
        {
          dataIndex: 'IsConfigurable',
          header: 'Is Configurable',
          type: 'boolean'
        }],
        listeners: {
          rowcontextmenu: function (thisGrid, index, event) {
            thisGrid.getSelectionModel().selectRow(index);
            var record = thisGrid.getSelectionModel().getSelected();
            var approvalMenuBtn = vendorProductGridMenu.get('Enable');
            var rejectMenuBtn = vendorProductGridMenu.get('Disable');

            if (record.get('WillBeExported')) {
              approvalMenuBtn.setDisabled(true);
              rejectMenuBtn.setDisabled(false);
            } else {
              approvalMenuBtn.setDisabled(false);
              rejectMenuBtn.setDisabled(true);
            }

            event.stopEvent();
            vendorProductGridMenu.showAt(event.xy);
          }
        }
      });
      this.grid = grid.grid;
      return grid;
    }
  });
  Concentrator.ConnectorMappingConnectorPublicationRules = Ext.extend(Concentrator.BaseAction, {
    loadLazy: true,
    getPanel: function () {
      var connectorID = this.ConnectorID;
      var highestPublicationIndex = 100;
      var ConnectorMappingConnectorPublicationRules = this;

      var functions = {
        resortPublicationIndex: function () {
          var records = grid.getStore().getRange();
          var publicationIndex = highestPublicationIndex;
          Ext.each(records, function (record) {
            record.set('PublicationIndex', publicationIndex);
            publicationIndex--;
          });
          functions.setSaveAndDiscardChangeButtons(false);
          functions.setDeleteButton(true);
        },
        saveConnectorPublicationIndex: function () {
          var ListOfConnectorPublicationRuleIDs = { listOfPublicationIndex: [] };
          var records = grid.getStore().getModifiedRecords();
          Ext.each(records, function (record) {
            var connectorPublicationRuleID = record.get('ConnectorPublicationRuleID');
            var connectorPublicationRuleIndex = record.get('PublicationIndex');
            item = {
              ConnectorPublicationRuleID: connectorPublicationRuleID,
              ConnectorPublicationRuleIndex: connectorPublicationRuleIndex
            };
            ListOfConnectorPublicationRuleIDs.listOfPublicationIndex.push(item);
          });
          var ListOfConnectorPublicationRuleIDsJson = JSON.stringify(ListOfConnectorPublicationRuleIDs);
          Diract.request({
            url: Concentrator.route("UpdatePublicationRuleIndex", "ConnectorPublicationRule"),
            waitMsg: 'Updating Publication Rule Index',
            params: {
              ListOfConnectorPublicationRuleIDsJson: ListOfConnectorPublicationRuleIDsJson
            },
            success: function (result) {
              functions.setSaveAndDiscardChangeButtons(true);
              functions.setDeleteButton(true);
              grid.getStore().reload();
            }
          });

        },

        createAddConnectorPublicationRuleButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Add New Connector Publication Rule',
            iconCls: "add",
            handler: function () {
              ConnectorMappingConnectorPublicationRules.getWindowForCreateConnectorPublicationRule(connectorID, grid);
            }
          });
        },
        createSaveChangesButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Save Publication Index',
            iconCls: "save",
            disabled: true,
            handler: function () {
              functions.saveConnectorPublicationIndex();
            }
          });
        },
        createDiscardButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Discard changes',
            iconCls: "cancel",
            disabled: true,
            handler: function () {
              grid.getStore().reload();
              functions.setSaveAndDiscardChangeButtons(true);
            }
          });
        },
        createDeleteConnectorPublicationRuleButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Delete Connector Publication Rule',
            iconCls: "delete",
            disabled: true,
            handler: function () {
              var records = grid.getSelectionModel().getSelections();
              if (records.length == 1) {
                var record = grid.getSelectionModel().getSelected();
                var title = 'Delete Connector Publication Rule.';
                var msg = 'Are you sure you want to delete Connector Publication Rule?';
                Ext.Msg.confirm(title, msg, function (button) {
                  if (button == "yes") {
                    Diract.request({
                      url: Concentrator.route("Delete", "ConnectorPublicationRule"),
                      waitMsg: 'Deleting Publication Rule Index',
                      params: {
                        id: record.get('ConnectorPublicationRuleID')
                      },
                      success: function (result) {
                        functions.setSaveAndDiscardChangeButtons(true);
                        functions.setDeleteButton(true);
                        grid.getStore().reload();
                      }
                    });
                  }
                });
              }
            }
          });
        },
        createModifyButton: function () {
          return new Ext.Toolbar.Button({
            text: 'Edit Connector Publication Rule',
            iconCls: "modify",
            disabled: true,
            handler: function () {
              var records = grid.getSelectionModel().getSelections();
              if (records.length == 1) {
                var record = grid.getSelectionModel().getSelected();
                ConnectorMappingConnectorPublicationRules.getWindowForModifyConnectorPublicationRule(record.get('ConnectorPublicationRuleID'), grid);
              }
            }
          });
        },

        getToolbarButtons: function () {
          listOfButtons = [];
          listOfButtons.push(btnAddNewConnectorPublicationRule);
          listOfButtons.push(btnSaveChanges);
          listOfButtons.push(btnDiscardChanges);
          listOfButtons.push(btnDeleteConnectorPublicationRule);
          listOfButtons.push(btnModifyConnectorPublicationRule);
          return listOfButtons;
        },
        setDeleteButton: function (disabled) {
          btnDeleteConnectorPublicationRule.setDisabled(disabled);
          btnModifyConnectorPublicationRule.setDisabled(disabled);
        },
        setSaveAndDiscardChangeButtons: function (disabled) {
          btnSaveChanges.setDisabled(disabled);
          btnDiscardChanges.setDisabled(disabled);
        }
      };

      btnAddNewConnectorPublicationRule = functions.createAddConnectorPublicationRuleButton();
      btnSaveChanges = functions.createSaveChangesButton();
      btnDiscardChanges = functions.createDiscardButton();
      btnDeleteConnectorPublicationRule = functions.createDeleteConnectorPublicationRuleButton();
      btnModifyConnectorPublicationRule = functions.createModifyButton();

      var grid = new Concentrator.ui.Grid({
        singularObjectName: 'Connector Publication',
        pluralObjectName: 'Connector Publications',
        permissions: {
          list: 'GetConnectorPublication',
          create: 'CreateConnectorPublication',
          update: 'UpdateConnectorPublication',
          remove: 'DeleteConnectorPublication'
        },
        url: Concentrator.route('GetListOfConnectorPublictionRulesByConnector', 'ConnectorPublicationRule'),
        primaryKey: 'ConnectorPublicationRuleID',
        sortBy: 'PublicationIndex',
        id: 'ConnectorMappingConnectorPublicationRuleGrid',
        // enable drag and drop of grid rows
        loadMask: true,
        ddGroup: 'connectorPublicationRuleGridDD',
        enableDragDrop: true,
        // enable select single row
        sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
        autoLoadStore: false,
        param: {
          publish: 'PublicationType'
        },
        params: {
          ConnectorID: connectorID
        },
        structure: [{
          dataIndex: 'ConnectorPublicationRuleID',
          type: 'int'
        }, {
          dataIndex: 'ConnectorID',
          type: 'int',
          header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          sortable: false
        }, {
          dataIndex: 'VendorID',
          type: 'int',
          header: 'Vendor',
          renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          sortable: false
        }, {
          dataIndex: 'BrandID',
          type: 'int',
          header: 'Brand',
          renderer: function (val, metadata, record) {
            return record.get('BrandName');
          },
          sortable: false
        }, {
          dataIndex: 'BrandName',
          type: 'string',
          sortable: false
        }, {
          dataIndex: 'MasterGroupMappingID',
          type: 'int'
        }, {
          dataIndex: 'MasterGroupMappingName',
          header: 'MasterGroupMapping'
        }, {
          dataIndex: 'ProductID',
          type: 'int',
          sortable: false
        }, {
          dataIndex: 'ConnectorRelationID',
          type: 'string',
          header: 'CustomerID',
          sortable: false
        }, {
          dataIndex: 'ProductDescription',
          type: 'string',
          header: 'Product',
          sortable: false
        }, {
          dataIndex: 'PublicationType',
          type: 'boolean',
          header: 'Publish',
          sortable: false
        }, {
          dataIndex: 'PublishOnlyStock',
          type: 'boolean',
          header: 'Publish Only Stock',
          sortable: false
        }, {
          dataIndex: 'PublicationIndex',
          type: 'int',
          header: 'Product Content Index',
          sortable: false
        }, {
          dataIndex: 'StatusID',
          type: 'int',
          header: 'Concentrator Status',
          renderer: function (val, m, re) {
            return re.get('ConcentratorStatus');
          },
          sortable: false
        }, {
          dataIndex: 'ConcentratorStatus',
          type: 'string',
          sortable: false
        }, {
          dataIndex: 'FromDate',
          type: 'date',
          header: 'From Date',
          renderer: Ext.util.Format.dateRenderer('d-m-Y'),
          sortable: false
        }, {
          dataIndex: 'ToDate',
          type: 'date',
          header: 'To Date',
          renderer: Ext.util.Format.dateRenderer('d-m-Y'),
          sortable: false
        }, {
          dataIndex: 'FromPrice',
          type: 'float',
          header: 'From price',
          sortable: false
        }, {
          dataIndex: 'ToPrice',
          type: 'float',
          header: 'To price',
          sortable: false
        }, {
          dataIndex: 'OnlyApprovedProducts',
          header: 'Only Approved Products',
          type: 'boolean'
        }, {
          dataIndex: 'IsActive',
          type: 'boolean',
          header: 'IsActive',
          sortable: false
        },
         {
           dataIndex: 'AttributeID',
           type: 'int'
         },
         {
           dataIndex: 'AttributeName',
           type: 'string',
           header: 'Attribute Name',
           sortable: false
         },
        {
          dataIndex: 'AttributeValue',
          type: 'string',
          header: 'Attribute Value',
          sortable: false
        }],
        listeners: {
          render: {
            scope: this,
            fn: function (grid) {
              var ddrow = new Ext.dd.DropTarget(grid.container, {
                ddGroup: 'connectorPublicationRuleGridDD',
                copy: false,
                notifyDrop: function (dd, e, data) {
                  var ds = grid.store;
                  var sm = grid.getSelectionModel();
                  var rows = sm.getSelections();
                  if (dd.getDragData(e)) {
                    var cindex = dd.getDragData(e).rowIndex;
                    if (typeof (cindex) != "undefined") {
                      for (i = 0; i < rows.length; i++) {
                        ds.remove(ds.getById(rows[i].id));
                      }
                      ds.insert(cindex, data.selections);
                      sm.clearSelections();
                      functions.resortPublicationIndex();
                    }
                  }
                }
              })
            }
          },
          rowclick: function (thisGrid, rowIndex, e) {
            functions.setDeleteButton(false);
          }
        },
        customButtons: [functions.getToolbarButtons()]
      });
      return grid;
    },
    getFunctions: {
      getConnectorPublicationRuleParameters: function () {
        var ruleParameters = [
            ['Connector', 'Connector'],
            ['Vendor', 'Vendor'],
            ['MasterGroupMapping', 'Master Group Mapping'],
            ['Brand', 'Brand'],
            ['Product', 'Product'],
            ['ConcentratorStatus', 'Concentrator Status'],
            [],
            ['FromDate', 'From Date'],
            ['ToDate', 'To Date'],
            ['FromPrice', 'From Price'],
            ['ToPrice', 'To Price'],
            [],
            ['Publication', 'Publish', false, 'No'],
            ['PublishOnlyStock', 'Publish Only Stock', false, 'No'],
            ['OnlyApprovedProducts', 'Only Approved Products', true, 'Yes'],
            ['IsActive', 'Is Connector Pubblication Rule Active', true, 'Yes'],
            [],
            ['Attribute', 'Attribute Name'],
            ['AttributeValue', 'Attribute Value']
        ];
        return ruleParameters;
      },
      getConnectorPublicationRuleStore: function () {
        var store = new Ext.data.ArrayStore({
          fields: [
              { name: 'ParameterID' },
              { name: 'ParameterName' },
              { name: 'ParameterValue' },
              { name: 'ParameterValueName' }
          ]
        });
        store.loadData(this.getConnectorPublicationRuleParameters());
        return store;
      },
      getConnectorPublicationRuleGrid: function () {
        var thisFunction = this;
        var grid = new Ext.grid.GridPanel({
          ddGroup: 'NewConnectorPublicationRuleDD',
          enableDragDrop: true,
          store: thisFunction.getConnectorPublicationRuleStore(),
          columns: [{
            id: 'ParameterID',
            dataIndex: 'ParameterID',
            hidden: true
          }, {
            header: 'Name',
            width: 160,
            sortable: true,
            dataIndex: 'ParameterName'
          }, {
            header: 'Value',
            sortable: true,
            width: 70,
            dataIndex: 'ParameterValueName'
          }, {
            dataIndex: 'ParameterValue',
            type: 'int',
            hidden: true
          }],
          stripeRows: false,
          listeners: {
            render: {
              scope: this,
              fn: function (grid) {
                var ddrow = new Ext.dd.DropTarget(grid.container, {
                  ddGroup: 'SourceConnectorPublicationRuleDD',
                  copy: false,
                  notifyDrop: function (dd, e, data) {
                    if (dd.grid.id) {
                      var gridID = dd.grid.id;
                      if (data.selections.length > 0) {
                        var record = data.selections[0];
                        if (gridID == 'SettingCPRGrid') {
                          var ParameterID = record.get('ParameterID');
                          var ParameterValueName = record.get('ParameterValueName');
                          var ParameterValue = record.get('ParameterValue');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'DateCPRGrid') {
                          // controle uit voeren
                          if (record.get('ParameterFromValue')) {
                            var ParameterID = 'FromDate';
                            var ParameterValueName = record.get('ParameterFromValue').dateFormat('d M Y');
                            var ParameterValue = record.get('ParameterFromValue');
                            thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                          }
                          if (record.get('ParameterToValue')) {
                            var ParameterID = 'ToDate';
                            var ParameterValueName = record.get('ParameterToValue').dateFormat('d M Y');
                            var ParameterValue = record.get('ParameterToValue');
                            thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                          }
                        }
                        if (gridID == 'PriceCPRGrid') {
                          if (record.get('ParameterFromValue')) {
                            if (record.get('ParameterToValue') && (record.get('ParameterFromValue') > record.get('ParameterToValue'))) {
                              Ext.Msg.alert('Error', 'The value FromPrice "' + record.get('ParameterFromValue') + '" must be higher than the value ToPrice "' + record.get('ParameterToValue') + '"');
                            } else {
                              var ParameterID = 'FromPrice';
                              var ParameterValueName = record.get('ParameterFromValue');
                              var ParameterValue = record.get('ParameterFromValue');
                              thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                            }
                          }
                          if (record.get('ParameterToValue')) {
                            var ParameterID = 'ToPrice';
                            var ParameterValueName = record.get('ParameterToValue');
                            var ParameterValue = record.get('ParameterToValue');
                            thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                          }
                        }
                        if (gridID == 'VendorCPRGrid') {
                          var ParameterID = 'Vendor';
                          var ParameterValueName = record.json.VendorName;
                          var ParameterValue = record.get('VendorID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'ConnectorCPRGrid') {
                          var ParameterID = 'Connector';
                          var ParameterValueName = record.json.Name;
                          var ParameterValue = record.get('ConnectorID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'MasterGroupMappingCPRGrid') {
                          var ParameterID = 'MasterGroupMapping';
                          var ParameterValueName = record.get('MasterGroupMappingName');
                          var ParameterValue = record.get('MasterGroupMappingID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'BrandCPRGrid') {
                          var ParameterID = 'Brand';
                          var ParameterValueName = record.get('Name');
                          var ParameterValue = record.get('BrandID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'ProductCPRGrid') {
                          var ParameterID = 'Product';
                          var ParameterValueName = record.get('ProductID');
                          var ParameterValue = record.get('ProductID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'ConcentratorStatusCPRGrid') {
                          var ParameterID = 'ConcentratorStatus';
                          var ParameterValueName = record.get('Status');
                          var ParameterValue = record.get('StatusID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'CustomerCPRGrid') {
                          var ParameterID = 'ConnectorRelation';
                          var ParameterValueName = record.get('CustomerID');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);
                        }
                        if (gridID == 'AttributeCPRGrid') {

                          //attribute id
                          var ParameterID = 'Attribute';
                          var ParameterValueName = record.get('AttributeName');
                          var ParameterValue = record.get('AttributeID');

                          if (record.data.UseForClear) //clear value
                            thisFunction.processValueToGrid(ParameterID, null, null, grid);
                          else
                            thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);

                          //attribute value
                          var ParameterID = 'AttributeValue';
                          var ParameterValueName = record.get('AttributeValue');
                          var ParameterValue = record.get('AttributeValue');
                          thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);

                          if (record.data.UseForClear) //clear value
                            thisFunction.processValueToGrid(ParameterID, null, null, grid);
                          else
                            thisFunction.processValueToGrid(ParameterID, ParameterValueName, ParameterValue, grid);

                        }
                      }
                    }
                  }
                });
              }
            }
          }
        });
        return grid;
      },

      fillConnectorPublicationRuleGrid: function (record, grid) {
        this.processValueToGrid('Connector', record.Connector, record.ConnectorID, grid);
        this.processValueToGrid('Vendor', record.VendorName, record.VendorID, grid);
        this.processValueToGrid('MasterGroupMapping', record.MasterGroupMappingName, record.MasterGroupMappingID, grid);
        this.processValueToGrid('Brand', record.BrandName, record.BrandID, grid);
        this.processValueToGrid('Product', record.ProductDescription, record.ProductGroupID, grid);
        this.processValueToGrid('ConcentratorStatus', record.ConcentratorStatus, record.ConcentratorStatusID, grid);

        if (record.FromDate) {
          var fromDate = new Date(parseInt((record.FromDate).substr(6)));
          this.processValueToGrid('FromDate', fromDate.dateFormat('d M Y'), fromDate, grid);
        }
        if (record.ToDate) {
          var toDate = new Date(parseInt((record.ToDate).substr(6)));
          this.processValueToGrid('ToDate', toDate.dateFormat('d M Y'), toDate, grid);
        }

        this.processValueToGrid('FromPrice', record.FromPrice, record.FromPrice, grid);
        this.processValueToGrid('ToPrice', record.ToPrice, record.ToPrice, grid);

        this.processValueToGrid('Publication', record.PublicationType ? 'Yes' : 'No', record.PublicationType, grid);
        this.processValueToGrid('PublishOnlyStock', record.PublishOnlyStock ? 'Yes' : 'No', record.PublishOnlyStock, grid);
        this.processValueToGrid('OnlyApprovedProducts', record.OnlyApprovedProducts ? 'Yes' : 'No', record.OnlyApprovedProducts, grid);
        this.processValueToGrid('IsActive', record.IsActive ? 'Yes' : 'No', record.IsActive, grid);
        this.processValueToGrid('Attribute', record.AttributeName, record.AttributeID, grid);
        this.processValueToGrid('ConnectorRelationCustomerID', record.ConnectorRelationCustomerID, record.ConnectorRelationCustomerID, grid);
        this.processValueToGrid('Attribute', record.AttributeName, record.AttributeID, grid);
        this.processValueToGrid('AttributeValue', record.AttributeValue, record.AttributeValue, grid);

        grid.getStore().commitChanges();
      },

      getSettingGrid: function () {
        var ruleParameters = [
            ['Publication', 'Publish', 'Yes', true],
            ['Publication', 'Publish', 'No', false],
            ['PublishOnlyStock', 'Publish Only Stock', 'Yes', true],
            ['PublishOnlyStock', 'Publish Only Stock', 'No', false],
            ['OnlyApprovedProducts', 'Only Approved Products', 'Yes', true],
            ['OnlyApprovedProducts', 'Only Approved Products', 'No', false],
            ['IsActive', 'Connector Pubblication Rule Active', 'Yes', true],
            ['IsActive', 'Connector Pubblication Rule Active', 'No', false]
        ];

        var store = new Ext.data.ArrayStore({
          fields: [
              { name: 'ParameterID' },
              { name: 'ParameterName' },
              { name: 'ParameterValueName' },
              { name: 'ParameterValue' }
          ]
        });

        store.loadData(ruleParameters);

        var grid = new Ext.grid.GridPanel({
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'SettingCPRGrid',
          store: store,
          columns: [{
            id: 'ParameterID',
            dataIndex: 'ParameterID',
            hidden: true
          }, {
            header: 'Name',
            width: 160,
            sortable: true,
            dataIndex: 'ParameterName'
          }, {
            header: 'Value',
            sortable: true,
            width: 70,
            dataIndex: 'ParameterValueName'
          }, {
            dataIndex: 'ParameterValue',
            type: 'int',
            hidden: true
          }],
          stripeRows: true
        });
        return grid;

      },
      getAttributesGrid: function () {

        var grid = new Diract.ui.Grid({
          primaryKey: ['AttributeID'],
          singularObjectName: 'Attributes',
          pluralObjectName: 'Attribute',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'AttributeCPRGrid',
          autoLoad: false,
          allowEdit: true,
          url: Concentrator.route('GetList', 'ProductAttribute'),
          permissions: {
            list: 'GetProductAttribute',
            update: 'GetProductAttribute'
          },
          structure: [{
            dataIndex: 'AttributeID',
            type: 'int'
          }, {
            dataIndex: 'AttributeName',
            type: 'string',
            header: 'AttributeName'
          }, {
            dataIndex: 'AttributeValue',
            type: 'string',
            header: 'Value',
            editable: true,
            renderer: function (data, ob, rec) {
              var val = rec.get('AttributeValue');
              if (rec.data.UseForClear && (val != null && val.length > 0)) {
                rec.set('AttributeValue', '');
                return "";
              }
              return data;
            },
            editor: {
              xtype: 'textfield'
            }
          }]
        });

        grid.store.addListener('load', function (store, records) {
          store.insert(0, new store.recordType({
            AttributeID: null,
            AttributeName: '<i>Clear Attribute Name/Value</i>',
            AttributeValue: null,
            UseForClear: true
          }));
        });

        return grid;
      },
      getDateGrid: function () {
        function formatDate(value) {
          return value ? value.dateFormat('M d, Y') : '';
        }

        var ruleParameters = [
            ['Date', 'Choose Date']
        ];

        var store = new Ext.data.ArrayStore({
          fields: [
              { name: 'ParameterID' },
              { name: 'ParameterName' },
              { name: 'ParameterFromValue', type: 'date', dateFormat: 'd/m/Y' },
              { name: 'ParameterToValue', type: 'date', dateFormat: 'd/m/Y' }
          ]
        });

        store.loadData(ruleParameters);

        var grid = new Ext.grid.EditorGridPanel({
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'DateCPRGrid',
          clicksToEdit: 1,
          store: store,
          columns: [{
            id: 'ParameterID',
            dataIndex: 'ParameterID',
            hidden: true
          }, {
            header: 'Name',
            width: 160,
            sortable: true,
            dataIndex: 'ParameterName'
          }, {
            header: 'From Date',
            width: 95,
            dataIndex: 'ParameterFromValue',
            renderer: formatDate,
            editor: new Ext.form.DateField({
              format: 'd/m/y',
              minValue: '01/01/13',
              disabledDays: [0, 6],
              disabledDaysText: 'Plants are not available on the weekends'
            })
          }, {
            header: 'To Date',
            width: 95,
            dataIndex: 'ParameterToValue',
            renderer: formatDate,
            editor: new Ext.form.DateField({
              format: 'd/m/y',
              minValue: '01/01/13',
              disabledDays: [0, 6],
              disabledDaysText: 'Plants are not available on the weekends'
            })
          }],
          stripeRows: true
        });
        return grid;

      },
      getPriceGrid: function () {
        euMoney = function (v) {
          v = (Math.round((v - 0) * 100)) / 100;
          v = (v == Math.floor(v)) ? v + ".00" : ((v * 10 == Math.floor(v * 10)) ? v + "0" : v);
          v = String(v);
          var ps = v.split('.'),
                  whole = ps[0],
                  sub = ps[1] ? '.' + ps[1] : '.00',
                  r = /(\d+)(\d{3})/;
          while (r.test(whole)) {
            whole = whole.replace(r, 'â‚¬1' + ',' + 'â‚¬2');
          }
          v = whole + sub;
          if (v.charAt(0) == '-') {
            return '-â‚¬' + v.substr(1);
          }
          return "â‚¬ " + v;
        };
        var ruleParameters = [
            ['Price', 'Price']
        ];

        var store = new Ext.data.ArrayStore({
          fields: [
              { name: 'ParameterID' },
              { name: 'ParameterName' },
              { name: 'ParameterFromValue', type: 'date', dateFormat: 'd/m/Y' },
              { name: 'ParameterToValue', type: 'date', dateFormat: 'd/m/Y' }
          ]
        });

        store.loadData(ruleParameters);

        var grid = new Ext.grid.EditorGridPanel({
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'PriceCPRGrid',
          //          clicksToEdit: 1,
          store: store,
          columns: [{
            id: 'ParameterID',
            dataIndex: 'ParameterID',
            hidden: true
          }, {
            header: 'Name',
            width: 160,
            sortable: true,
            dataIndex: 'ParameterName'
          }, {
            header: 'From Price',
            dataIndex: 'ParameterFromValue',
            width: 70,
            align: 'right',
            renderer: euMoney,
            editor: new Ext.form.NumberField({
              allowBlank: false,
              allowNegative: false,
              maxValue: 100000
            })
          }, {
            header: 'To Price',
            dataIndex: 'ParameterToValue',
            width: 70,
            align: 'right',
            renderer: euMoney,
            editor: new Ext.form.NumberField({
              allowBlank: false,
              allowNegative: false,
              maxValue: 100000
            })
          }],
          stripeRows: true
        });
        return grid;

      },
      getVendorGrid: function () {
        var grid = new Concentrator.ui.Grid({
          singularObjectName: 'Vendor',
          pluralObjectName: 'Vendors',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'VendorCPRGrid',
          permissions: {
            list: 'GetVendor'
          },
          url: Concentrator.route('GetList', 'Vendor'),
          primaryKey: 'VendorID',
          structure: [{
            dataIndex: 'VendorID',
            type: 'int',
            header: 'Vendor',
            sortBy: 'VendorName',
            renderer: Concentrator.renderers.field('allVendors', 'VendorName'),
            filter: {
              type: 'string',
              store: Concentrator.stores.vendors,
              filterField: 'VendorName'
            }
          }, {
            dataIndex: 'ParentVendorID',
            type: 'int',
            header: 'Parent Vendor',
            renderer: Concentrator.renderers.getName('allVendors', 'VendorName'),
            filter: {
              type: 'string',
              store: Concentrator.stores.vendors,
              filterField: 'VendorName'
            }
          }, {
            dataIndex: 'BackendVendorCode',
            type: 'string',
            header: 'Backend Vendor Code'
          }, {
            dataIndex: 'IsActive',
            type: 'boolean',
            header: 'Is Active'
          }]
        });
        return grid;
      },
      getConnectorGrid: function () {
        var grid = new Concentrator.ui.Grid({
          singularObjectName: 'Connector',
          pluralObjectName: 'Connectors',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'ConnectorCPRGrid',
          params: {
            ConnectorID: Concentrator.user.connectorID
          },
          permissions: {
            list: 'GetConnector'
          },
          url: Concentrator.route('GetListByConnectorID', 'Connector'),
          primaryKey: 'ConnectorID',
          structure: [{
            dataIndex: 'ConnectorID',
            type: 'int',
            header: 'Connector',
            sortBy: 'Name',
            renderer: Concentrator.renderers.field('connectors', 'Name'),
            filter: {
              type: 'string',
              store: Concentrator.stores.connectors,
              labelField: 'Name'
            }
          }, {
            dataIndex: 'ParentConnectorID',
            type: 'int',
            header: 'Parent Connector',
            renderer: Concentrator.renderers.field('connectors', 'Name'),
            filter: {
              type: 'string',
              store: Concentrator.stores.connectors,
              filterField: 'Name'
            }
          }, {
            dataIndex: 'IsActive',
            type: 'boolean',
            header: 'Is Active'
          }]
        });
        return grid;
      },
      getMasterGroupMappingGrid: function () {
        var grid = new Diract.ui.Grid({
          pluralObjectName: 'MasterGroupMappings',
          singularObjectName: 'MasterGroupMapping',
          primaryKey: 'MasterGroupMappingID',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'MasterGroupMappingCPRGrid',
          forceFit: true,
          permissions: {
            list: 'DefaultMasterGroupMapping'
          },
          url: Concentrator.route("GetListOfMasterGroupMappingByConnectorID", "MasterGroupMapping"),
          structure: [{
            dataIndex: 'MasterGroupMappingID',
            type: 'int'
          }, {
            dataIndex: 'MasterGroupMappingName',
            header: 'Name',
            type: 'string',
            width: 100
          }, {
            dataIndex: 'MasterGroupMappingPath',
            header: 'MasterGroupMapping path',
            type: 'string'
          }]
        });
        return grid;
      },
      getBrandGrid: function () {
        var grid = new Concentrator.ui.Grid({
          pluralObjectName: 'Brands',
          singularObjectName: 'Brand',
          primaryKey: 'BrandID',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'BrandCPRGrid',
          url: Concentrator.route("GetList", "Brand"),
          permissions: {
            list: 'GetBrand'
          },
          structure: [{
            dataIndex: 'BrandID',
            type: 'int'
          }, {
            dataIndex: 'Name',
            type: 'string',
            header: 'Brand Name'
          }]
        });

        return grid;
      },
      getProductGrid: function () {
        var grid = new Diract.ui.Grid({
          primaryKey: ['ProductID'],
          singularObjectName: 'Product',
          pluralObjectName: 'Products',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'ProductCPRGrid',
          url: Concentrator.route('GetList', 'Product'),
          permissions: {
            list: 'GetProduct'
          },
          structure: [{
            dataIndex: 'ProductID',
            type: 'int',
            header: 'Identifier'
          }, {
            dataIndex: 'VendorItemNumber',
            type: 'string',
            header: 'Vendor item number'
          }, {
            dataIndex: 'CustomItemNumber',
            type: 'string',
            header: 'Custom Item Number'
          }, {
            dataIndex: 'ProductDescription',
            type: 'string',
            header: 'Description'
          }, {
            dataIndex: 'BrandID',
            type: 'int',
            header: 'Brand',
            filter: {
              type: 'string',
              filterField: 'BrandName'
            },
            sortBy: 'BrandName',
            renderer: function (val, metadata, record) {
              return record.get('BrandName');
            }
          }, {
            dataIndex: 'BrandName',
            type: 'string'
          }, {
            dataIndex: 'VendorID',
            type: 'int',
            header: 'Source vendor',
            sortBy: 'VendorName',
            renderer: Concentrator.renderers.field('vendors', 'VendorName'),
            filter: {
              type: 'list',
              store: Concentrator.stores.vendors,
              labelField: 'VendorName'
            }
          }]
        });
        return grid;
      },
      getConcentratorStatusGrid: function () {
        var grid = new Diract.ui.Grid({
          primaryKey: ['StatusID'],
          singularObjectName: 'Status',
          pluralObjectName: 'Statuses',
          ddGroup: 'SourceConnectorPublicationRuleDD',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          id: 'ConcentratorStatusCPRGrid',
          autoLoad: false,
          url: Concentrator.route('GetList', 'ProductStatus'),
          permissions: {
            list: 'GetProductStatus'
          },
          structure: [{
            dataIndex: 'StatusID',
            type: 'int'
          }, {
            dataIndex: 'Status',
            type: 'string',
            header: 'Status'
          }]
        });
        return grid;
      },
      getCustomerGrid: function () {

        var config = {
          id: 'CustomerCPRGrid',
          enableDragDrop: true,
          sm: new Ext.grid.RowSelectionModel({ singleSelect: true }),
          ddGroup: 'SourceConnectorPublicationRuleDD'
        };
        var grid = new Concentrator.ConnectorRelations(config);
        //grid.id = 'productAttributeCPRGrid';
        //grid.enableDragDrop = true;
        //grid.sm = new Ext.grid.RowSelectionModel({ singleSelect: true });
        //grid.ddGroup = 'SourceConnectorPublicationRuleDD';
        return grid;
      },

      processValueToGrid: function (ParameterID, ParameterValueName, ParameterValue, newCPRGrid) {
        var rows = newCPRGrid.getStore().getRange();
        Ext.each(rows, function (row) {
          if (row.get('ParameterID') == ParameterID) {
            row.set('ParameterValueName', ParameterValueName);
            row.set('ParameterValue', ParameterValue);
          }
        });
      }
    },
    getWindowForCreateConnectorPublicationRule: function (ConnectorID, connectorPublictationRuleGird) {
      var functions = {
        getTabPanelItems: function () {

          tabSettingGrid.title = 'Setting';
          tabSettingGrid.layout = 'fit';

          tabDateGrid.title = 'Date';
          tabDateGrid.layout = 'fit';

          tabPriceGrid.title = 'Price';
          tabPriceGrid.layout = 'fit';

          tabVendorGrid.title = 'Vendors';
          tabVendorGrid.layout = 'fit';

          tabMasterGroupMappingGrid.title = 'Master Group Mapping';
          tabMasterGroupMappingGrid.layout = 'fit';

          tabBrandGrid.title = 'Brand';
          tabBrandGrid.layout = 'fit';

          tabProductGrid.title = 'Product';
          tabProductGrid.layout = 'fit';

          tabConcentratorStatusGrid.title = 'Concentrator Status';
          tabConcentratorStatusGrid.layout = 'fit';

          tabCustomerGrid.title = 'Customer';
          tabCustomerGrid.layout = 'fit';

          tabConnectorGrid.title = 'Connectors';
          tabConnectorGrid.layout = 'fit';

          tabAttributes.title = 'Attribute';
          tabAttributes.layout = 'fit';

          return [
            tabConnectorGrid,
            tabVendorGrid,
            tabMasterGroupMappingGrid,
            tabBrandGrid,
            tabProductGrid,
            tabConcentratorStatusGrid,
            tabSettingGrid,
            tabDateGrid,
            tabPriceGrid,
            tabCustomerGrid,
            tabAttributes
          ];
        },

        saveConnectorPublicationRule: function () {
          var listOfParameters = {
            ConnectorID: ConnectorID,
            Publication: false
          };
          var records = gridConnectorPublicationRule.getStore().getRange();

          Ext.each(records, function (record) {

            if (record.get('ParameterID') && record.get('ParameterValue')) {
              if (record.get('ParameterID') == 'Connector') {
                listOfParameters.ConnectorID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'Vendor') {
                listOfParameters.VendorID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'MasterGroupMapping') {
                listOfParameters.MasterGroupMappingID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'Brand') {
                listOfParameters.BrandID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'Product') {
                listOfParameters.ProductID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'ConcentratorStatus') {
                listOfParameters.StatusID = record.get('ParameterValue');
              }

              if (record.get('ParameterID') == 'FromDate') {
                listOfParameters.FromDate = record.get('ParameterValue').dateFormat('Y-m-d');
              }
              if (record.get('ParameterID') == 'ToDate') {
                listOfParameters.ToDate = record.get('ParameterValue').dateFormat('Y-m-d');
              }
              if (record.get('ParameterID') == 'FromPrice') {
                listOfParameters.FromPrice = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'ToPrice') {
                listOfParameters.ToPrice = record.get('ParameterValue');
              }

              if (record.get('ParameterID') == 'Publication') {
                listOfParameters.Publication = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'PublishOnlyStock') {
                listOfParameters.PublishOnlyStock = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'OnlyApprovedProducts') {
                listOfParameters.OnlyApprovedProducts = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'IsActive') {
                listOfParameters.IsActive = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'ConnectorRelationCustomerID') {
                listOfParameters.CustomerID = record.get('ConnectorRelationCustomerID');
              }
              if (record.get('ParameterID') == 'Attribute') {
                listOfParameters.AttributeID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'AttributeValue') {
                listOfParameters.AttributeValue = record.get('ParameterValue');
              }
            }
          });

          if (listOfParameters.VendorID && listOfParameters.ConnectorID) {
            var saveConnectorPublicationRule = true;
            var msg = '';

            var fromDate;
            var toDate;
            var fromPrice;
            var toPrice;

            var isFromDateSameAsCPRGridFromDate = false;
            var isToDateSameAsCPRGridToDate = false;

            var isFromPriceSameAsCPRGridFromPrice = false;
            var isToPriceSameAsCPRGridToPrice = false;

            var cprRecords = gridConnectorPublicationRule.getStore().getRange();
            var dateRecords = tabDateGrid.getStore().getRange();
            var dateRecord = dateRecords[0];
            var priceRecords = tabPriceGrid.getStore().getRange();
            var priceRecord = priceRecords[0];

            if (dateRecord.get('ParameterFromValue')) {
              fromDate = dateRecord.get('ParameterFromValue');
            }
            if (dateRecord.get('ParameterToValue')) {
              toDate = dateRecord.get('ParameterToValue');
            }
            if (priceRecord.get('ParameterFromValue')) {
              fromPrice = priceRecord.get('ParameterFromValue');
            }
            if (priceRecord.get('ParameterToValue')) {
              toPrice = priceRecord.get('ParameterToValue');
            }

            Ext.each(cprRecords, function (row) {
              if (row.get('ParameterID') == 'FromDate') {
                if (fromDate) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue').dateFormat('d M Y') == fromDate.dateFormat('d M Y')) {
                      isFromDateSameAsCPRGridFromDate = true;
                    } else {
                      isFromDateSameAsCPRGridFromDate = false;
                    }
                  } else {
                    isFromDateSameAsCPRGridFromDate = false;
                  }
                } else {
                  isFromDateSameAsCPRGridFromDate = true;
                }
              }
              if (row.get('ParameterID') == 'ToDate') {
                if (toDate) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue').dateFormat('d M Y') == toDate.dateFormat('d M Y')) {
                      isToDateSameAsCPRGridToDate = true;
                    } else {
                      isToDateSameAsCPRGridToDate = false;
                    }
                  } else {
                    isToDateSameAsCPRGridToDate = false;
                  }
                } else {
                  isToDateSameAsCPRGridToDate = true;
                }
              }
              if (row.get('ParameterID') == 'FromPrice') {
                if (fromPrice) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue') == fromPrice) {
                      isFromPriceSameAsCPRGridFromPrice = true;
                    } else {
                      isFromPriceSameAsCPRGridFromPrice = false;
                    }
                  } else {
                    isFromPriceSameAsCPRGridFromPrice = false;
                  }
                } else {
                  isFromPriceSameAsCPRGridFromPrice = true;
                }
              }
              if (row.get('ParameterID') == 'ToPrice') {
                if (toPrice) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue') == toPrice) {
                      isToPriceSameAsCPRGridToPrice = true;
                    } else {
                      isToPriceSameAsCPRGridToPrice = false;
                    }
                  } else {
                    isToPriceSameAsCPRGridToPrice = false;
                  }
                } else {
                  isToPriceSameAsCPRGridToPrice = true;
                }
              }
            });

            if (!isFromDateSameAsCPRGridFromDate) {
              saveConnectorPublicationRule = false
              msg += 'The From Date field is filled but not used!. <br>';
            }
            if (!isToDateSameAsCPRGridToDate) {
              saveConnectorPublicationRule = false
              msg += 'The To Date field is filled but not used!. <br>';
            }
            if (!isFromPriceSameAsCPRGridFromPrice) {
              saveConnectorPublicationRule = false
              msg += 'The From Price field is filled but not used!. <br>';
            }
            if (!isToPriceSameAsCPRGridToPrice) {
              saveConnectorPublicationRule = false
              msg += 'The To Price field is filled but not used!. <br>';
            }

            msg += '<br> Do you still want to save Connector Publication Rule?';

            if (saveConnectorPublicationRule) {
              functions.requestToSaveConnectorPublicationRule(listOfParameters);
            } else {
              Ext.Msg.show({
                title: 'Save Changes?',
                msg: msg,
                buttons: { ok: "Yes", cancel: "No" },
                fn: function (result) {
                  if (result == 'ok') {
                    functions.requestToSaveConnectorPublicationRule(listOfParameters);
                  } else {
                  };
                },
                animEl: 'elId',
                icon: Ext.MessageBox.QUESTION
              });
            }
          } else {
            Ext.Msg.alert('Required Values', 'Faild to save Connector Publication Rule. Vendor does not exist');
          }
        },
        requestToSaveConnectorPublicationRule: function (listOfParameters) {
          Diract.request({
            url: Concentrator.route('CreateConnectorPublicationRule', 'ConnectorPublicationRule'),
            waitMsg: 'Saving Connector Publication Rule',
            params: listOfParameters,
            success: function () {
              connectorPublictationRuleGird.getStore().reload();
              window.close();
            },
            failure: function () { }
          });
        }
      };

      var gridConnectorPublicationRule = this.getFunctions.getConnectorPublicationRuleGrid();
      var tabSettingGrid = this.getFunctions.getSettingGrid();
      var tabDateGrid = this.getFunctions.getDateGrid();
      var tabPriceGrid = this.getFunctions.getPriceGrid();
      var tabVendorGrid = this.getFunctions.getVendorGrid();
      var tabConnectorGrid = this.getFunctions.getConnectorGrid();
      var tabMasterGroupMappingGrid = this.getFunctions.getMasterGroupMappingGrid();
      var tabBrandGrid = this.getFunctions.getBrandGrid();
      var tabProductGrid = this.getFunctions.getProductGrid();
      var tabConcentratorStatusGrid = this.getFunctions.getConcentratorStatusGrid();
      var tabCustomerGrid = this.getFunctions.getCustomerGrid();
      var tabAttributes = this.getFunctions.getAttributesGrid();

      var centerPanelTabs = new Ext.TabPanel({
        activeTab: 0,
        region: 'north',
        height: 200,
        enableTabScroll: true,
        split: true,
        items: functions.getTabPanelItems()
      });
      var centerPanel = new Ext.Panel({
        layout: 'fit',
        region: 'center',
        split: true,
        title: 'Choose the values',
        width: 450,
        items: [centerPanelTabs]
      });
      var eastPanel = new Ext.Panel({
        layout: 'fit',
        region: 'east',
        split: true,
        title: 'Connector Publication Rule Values',
        width: 250,
        items: gridConnectorPublicationRule
      });

      var window = new Ext.Window({
        width: 800,
        height: 500,
        modal: true,
        maximizable: true,
        resize: true,
        layout: 'border',
        items: [centerPanel, eastPanel],
        buttons: [{
          text: 'Save',
          handler: function () {
            functions.saveConnectorPublicationRule();
          }
        }, {
          text: 'Cancel',
          handler: function () {
            window.close();
          }
        }]
      });
      Diract.request({
        url: Concentrator.route('Get', 'Connector'),
        waitMsg: 'Getting Connector',
        params: {
          connectorID: ConnectorID
        },
        success: function (record) {
          window.title = 'Create a new Connector Publication Rule for Connector "' + record.data.Name + '"';
          window.show();
        },
        failure: function () {
          Ext.Msg.alert('Faild to find connector', 'The system can not find connector.');
        }
      });
    },
    getWindowForModifyConnectorPublicationRule: function (connectorPublicationRuleID, connectorPublictationRuleGird) {
      var thisFunction = this;
      var functions = {
        getTabPanelItems: function () {

          tabSettingGrid.title = 'Setting';
          tabSettingGrid.layout = 'fit';

          tabDateGrid.title = 'Date';
          tabDateGrid.layout = 'fit';

          tabPriceGrid.title = 'Price';
          tabPriceGrid.layout = 'fit';

          tabVendorGrid.title = 'Vendors';
          tabVendorGrid.layout = 'fit';

          tabMasterGroupMappingGrid.title = 'Master Group Mapping';
          tabMasterGroupMappingGrid.layout = 'fit';

          tabBrandGrid.title = 'Brand';
          tabBrandGrid.layout = 'fit';

          tabProductGrid.title = 'Product';
          tabProductGrid.layout = 'fit';

          tabConcentratorStatusGrid.title = 'Concentrator Status';
          tabConcentratorStatusGrid.layout = 'fit';

          tabCustomerGrid.title = 'Customer';
          tabCustomerGrid.layout = 'fit';

          tabConnectorGrid.title = 'Connectors';
          tabConnectorGrid.layout = 'fit';

          tabAttributesGrid.title = 'Attribute';
          tabAttributesGrid.layout = 'fit';

          return [
            tabVendorGrid,
            tabConnectorGrid,
            tabMasterGroupMappingGrid,
            tabBrandGrid,
            tabProductGrid,
            tabConcentratorStatusGrid,
            tabSettingGrid,
            tabDateGrid,
            tabPriceGrid,
            tabCustomerGrid,
            tabAttributesGrid
          ];
        },

        updateConnectorPublicationRule: function () {
          var listOfParameters = {
            id: connectorPublicationRuleID
          };
          var records = gridConnectorPublicationRule.getStore().getModifiedRecords();
          Ext.each(records, function (record) {
            if (record.get('ParameterID')) {
              if (record.get('ParameterID') == 'Vendor') {
                listOfParameters.VendorID = record.get('ParameterValue');
              }

              if (record.get('ParameterID') == 'Connector') {
                listOfParameters.ConnectorID = record.get('ParameterValue');
              }

              if (record.get('ParameterID') == 'MasterGroupMapping') {
                listOfParameters.MasterGroupMappingID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'Brand') {
                listOfParameters.BrandID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'Product') {
                listOfParameters.ProductID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'ConcentratorStatus') {
                listOfParameters.ConcentratorStatusID = record.get('ParameterValue');
              }

              if (record.get('ParameterID') == 'FromDate') {
                listOfParameters.FromDate = record.get('ParameterValue').dateFormat('Y-m-d');
              }
              if (record.get('ParameterID') == 'ToDate') {
                listOfParameters.ToDate = record.get('ParameterValue').dateFormat('Y-m-d');
              }
              if (record.get('ParameterID') == 'FromPrice') {
                listOfParameters.FromPrice = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'ToPrice') {
                listOfParameters.ToPrice = record.get('ParameterValue');
              }

              if (record.get('ParameterID') == 'Publication') {
                listOfParameters.PublicationType = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'PublishOnlyStock') {
                listOfParameters.PublishOnlyStock = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'OnlyApprovedProducts') {
                listOfParameters.OnlyApprovedProducts = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'IsActive') {
                listOfParameters.IsActive = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'ConnectorRelationCustomerID') {
                listOfParameters.CustomerID = record.get('ConnectorRelationCustomerID');
              }
              if (record.get('ParameterID') == 'Attribute') {
                listOfParameters.AttributeID = record.get('ParameterValue');
              }
              if (record.get('ParameterID') == 'AttributeValue') {
                listOfParameters.AttributeValue = record.get('ParameterValue');
              }
            }
          });

          if (listOfParameters.id) {
            var saveConnectorPublicationRule = true;
            var msg = '';

            var fromDate;
            var toDate;
            var fromPrice;
            var toPrice;

            var isFromDateSameAsCPRGridFromDate = false;
            var isToDateSameAsCPRGridToDate = false;

            var isFromPriceSameAsCPRGridFromPrice = false;
            var isToPriceSameAsCPRGridToPrice = false;

            var cprRecords = gridConnectorPublicationRule.getStore().getRange();
            var dateRecords = tabDateGrid.getStore().getRange();
            var dateRecord = dateRecords[0];
            var priceRecords = tabPriceGrid.getStore().getRange();
            var priceRecord = priceRecords[0];

            if (dateRecord.get('ParameterFromValue')) {
              fromDate = dateRecord.get('ParameterFromValue');
            }
            if (dateRecord.get('ParameterToValue')) {
              toDate = dateRecord.get('ParameterToValue');
            }
            if (priceRecord.get('ParameterFromValue')) {
              fromPrice = priceRecord.get('ParameterFromValue');
            }
            if (priceRecord.get('ParameterToValue')) {
              toPrice = priceRecord.get('ParameterToValue');
            }

            Ext.each(cprRecords, function (row) {
              if (row.get('ParameterID') == 'FromDate') {
                if (fromDate) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue').dateFormat('d M Y') == fromDate.dateFormat('d M Y')) {
                      isFromDateSameAsCPRGridFromDate = true;
                    } else {
                      isFromDateSameAsCPRGridFromDate = false;
                    }
                  } else {
                    isFromDateSameAsCPRGridFromDate = false;
                  }
                } else {
                  isFromDateSameAsCPRGridFromDate = true;
                }
              }
              if (row.get('ParameterID') == 'ToDate') {
                if (toDate) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue').dateFormat('d M Y') == toDate.dateFormat('d M Y')) {
                      isToDateSameAsCPRGridToDate = true;
                    } else {
                      isToDateSameAsCPRGridToDate = false;
                    }
                  } else {
                    isToDateSameAsCPRGridToDate = false;
                  }
                } else {
                  isToDateSameAsCPRGridToDate = true;
                }
              }
              if (row.get('ParameterID') == 'FromPrice') {
                if (fromPrice) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue') == fromPrice) {
                      isFromPriceSameAsCPRGridFromPrice = true;
                    } else {
                      isFromPriceSameAsCPRGridFromPrice = false;
                    }
                  } else {
                    isFromPriceSameAsCPRGridFromPrice = false;
                  }
                } else {
                  isFromPriceSameAsCPRGridFromPrice = true;
                }
              }
              if (row.get('ParameterID') == 'ToPrice') {
                if (toPrice) {
                  if (row.get('ParameterValue')) {
                    if (row.get('ParameterValue') == toPrice) {
                      isToPriceSameAsCPRGridToPrice = true;
                    } else {
                      isToPriceSameAsCPRGridToPrice = false;
                    }
                  } else {
                    isToPriceSameAsCPRGridToPrice = false;
                  }
                } else {
                  isToPriceSameAsCPRGridToPrice = true;
                }
              }
            });

            if (!isFromDateSameAsCPRGridFromDate) {
              saveConnectorPublicationRule = false
              msg += 'The From Date field is filled but not used!. <br>';
            }
            if (!isToDateSameAsCPRGridToDate) {
              saveConnectorPublicationRule = false
              msg += 'The To Date field is filled but not used!. <br>';
            }
            if (!isFromPriceSameAsCPRGridFromPrice) {
              saveConnectorPublicationRule = false
              msg += 'The From Price field is filled but not used!. <br>';
            }
            if (!isToPriceSameAsCPRGridToPrice) {
              saveConnectorPublicationRule = false
              msg += 'The To Price field is filled but not used!. <br>';
            }

            msg += '<br> Do you still want to save Connector Publication Rule?';


            if (saveConnectorPublicationRule) {
              functions.requestToUpdateConnectorPublicationRule(listOfParameters);
            } else {
              Ext.Msg.show({
                title: 'Save Changes?',
                msg: msg,
                buttons: { ok: "Yes", cancel: "No" },
                fn: function (result) {
                  if (result == 'ok') {
                    functions.requestToUpdateConnectorPublicationRule(listOfParameters);
                  } else {
                  };
                },
                animEl: 'elId',
                icon: Ext.MessageBox.QUESTION
              });
            }
          } else {
            Ext.Msg.alert('Required Values', 'Faild to save Connector Publication Rule. Connector Publication Rule ID does not exist');
          }
        },
        requestToUpdateConnectorPublicationRule: function (listOfParameters) {
          Diract.request({
            url: Concentrator.route('Update', 'ConnectorPublicationRule'),
            waitMsg: 'Updating Connector Publication Rule',
            params: listOfParameters,
            success: function () {
              connectorPublictationRuleGird.getStore().reload();
              window.close();
            },
            failure: function () { }
          });
        }
      };

      var gridConnectorPublicationRule = this.getFunctions.getConnectorPublicationRuleGrid();
      var tabSettingGrid = this.getFunctions.getSettingGrid();
      var tabDateGrid = this.getFunctions.getDateGrid();
      var tabPriceGrid = this.getFunctions.getPriceGrid();
      var tabVendorGrid = this.getFunctions.getVendorGrid();
      var tabConnectorGrid = this.getFunctions.getConnectorGrid();
      var tabMasterGroupMappingGrid = this.getFunctions.getMasterGroupMappingGrid();
      var tabBrandGrid = this.getFunctions.getBrandGrid();
      var tabProductGrid = this.getFunctions.getProductGrid();
      var tabConcentratorStatusGrid = this.getFunctions.getConcentratorStatusGrid();
      var tabCustomerGrid = this.getFunctions.getCustomerGrid();
      var tabAttributesGrid = this.getFunctions.getAttributesGrid();

      var centerPanelTabs = new Ext.TabPanel({
        activeTab: 0,
        region: 'north',
        height: 200,
        enableTabScroll: true,
        split: true,
        items: functions.getTabPanelItems()
      });
      var centerPanel = new Ext.Panel({
        layout: 'fit',
        region: 'center',
        split: true,
        title: 'Choose the values',
        width: 450,
        items: [centerPanelTabs]
      });
      var eastPanel = new Ext.Panel({
        layout: 'fit',
        region: 'east',
        split: true,
        title: 'Connector Publication Rule Values',
        width: 250,
        items: gridConnectorPublicationRule
      });

      var window = new Ext.Window({
        width: 800,
        height: 500,
        modal: true,
        maximizable: true,
        resize: true,
        layout: 'border',
        items: [centerPanel, eastPanel],
        buttons: [{
          text: 'Save',
          handler: function () {

            functions.updateConnectorPublicationRule();
          }
        }, {
          text: 'Cancel',
          handler: function () {
            window.close();
          }
        }]
      });
      Diract.request({
        url: Concentrator.route('Get', 'ConnectorPublicationRule'),
        waitMsg: 'Getting Connector Publication Rule',
        params: {
          ConnectorPublicationRuleID: connectorPublicationRuleID
        },
        success: function (record) {
          thisFunction.getFunctions.fillConnectorPublicationRuleGrid(record.data, gridConnectorPublicationRule);
          window.title = 'Modify Connector Publication Rule "' + record.data.Connector + ', ' + record.data.VendorName + '"';
          window.show();
        },
        failure: function () {
          Ext.Msg.alert('Faild to find connector', 'The system can not find connector.');
        }
      });
    }
  });
})
();