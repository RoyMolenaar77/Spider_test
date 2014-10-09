Concentrator.SlurpSchedule = Ext.extend(Concentrator.BaseAction, {

    getPanel: function () {
        var self = this;

        this.productSources = Ext.extend(Diract.ui.SearchBox, {
            valueField: 'ProductCompareSourceID',
            displayField: 'Name',
            searchUrl: Concentrator.route('GetPcSourceStore', 'ProductCompetitor'),
            allowBlank: false
        });

        Ext.reg('productsources', this.productSources);

        this.productSources = Ext.extend(Diract.ui.SearchBox, {
            valueField: 'IntervalType',
            displayField: 'IntervalName',
            searchUrl: Concentrator.route('GetIntervalType', 'Slurp'),
            allowBlank: false
        });

          Ext.reg('interval', this.productSources);

          this.productSources = Ext.extend(Diract.ui.SearchBox, {
            valueField: 'ProductGroupMappingID',
            displayField: 'ProductGroupName',
            searchUrl: Concentrator.route('GetProductGroupMappingStore', 'Slurp'),
            allowBlank: false
          });

          Ext.reg('productGroupMapping', this.productSources);
          
        //    self.cusButton = new Ext.Button({
        //      text: 'Create Scan Data',
        //      iconCls: 'add',
        //      handler: function () {

        //        var productMappingField = new Diract.ui.Select({
        //          label: 'Product Group',
        //          allowBlank: false,
        //          name: 'ID',
        //          displayfield: 'Name',
        //          valueField: 'ID',
        //          name: 'ProductGroupID',
        //          store: Concentrator.stores.productGroups
        //        });

        ////        var scanField = new Ext.form.ComboBox({
        ////          fieldLabel: 'Scan Product',
        ////          typeAhead: true,
        ////          triggerAction: 'all',
        ////          allowBlank: false,
        ////          lazyRender: true,
        ////          mode: 'local',
        ////          store: new Ext.data.ArrayStore({
        ////            id: 0,
        ////            fields: [
        ////              'ID',
        ////              'Name'
        ////            ],
        ////            data: [
        ////              [1, 'Once per day'],
        ////              [2, 'Once per hour'],
        ////              [3, 'Once per ' + 'x' + ' minutes'],
        ////              [4, 'Once per ' + 'x' + ' hours'],
        ////              [5, 'Once per ' + 'x' + ' days']
        ////            ]
        ////          }),
        ////          valueField: 'ID',
        ////          displayField: 'Name',
        ////          listeners: {
        ////            'select': function (combo, record, index) {

        ////              var id = record.get('ID');

        ////              if (id === 3 || id === 4 || id === 5) {
        ////                timeField.reset();
        ////                timeField.setDisabled(true);
        ////                valueField.enable();
        ////              }
        ////              else if (id === 1) {
        ////                valueField.reset();
        ////                valueField.setDisabled(true);
        ////                timeField.enable();
        ////              }
        ////              else {
        ////                valueField.reset();
        ////                timeField.reset();
        ////                valueField.setDisabled(true);
        ////                timeField.setDisabled(true);
        ////              }

        ////            }
        ////          }
        ////        });

        ////        var timeField = new Ext.form.TimeField({
        ////          fieldLabel: 'Time',
        ////          name: 'Time',
        ////          allowBlank: false,
        ////          disabled: true,
        ////          format: 'H:i'
        ////        });

        //        var window = new Ext.Window({
        //          width: 400,
        //          height: 235,
        //          modal: true,
        //          layout: 'fit',
        //          items: [
        //            new Ext.form.FormPanel({
        //              padding: 20,
        //              items: [
        //                productMappingField
        //              ]
        //            })
        //          ],
        //          buttons: [
        //            new Ext.Button({
        //              text: 'Save',
        //              handler: function () {

        //                Diract.request({
        //                  url: Concentrator.route("Create", "Slurp"),
        //                  params: {
        //                    productMappingID: productMappingField.getValue()
        //                  },
        //                  success: function () {
        //                    window.destroy();
        //                    self.grid.store.reload();
        //                  },
        //                  failure: function (form, action) {
        //                    Ext.MessageBox.show({
        //                      title: 'No productgroup match for this connector',
        //                      buttons: Ext.Msg.OK
        //                    });
        //                  }
        //                });

        //              }
        //            })
        //          ]
        //        });

        //        window.show();
        //      }
        //    });

        self.grid = new Concentrator.ui.Grid({
            singularObjectName: 'Slurp Schedule',
            pluralObjectName: 'Slurp Schedules',
            permissions: {
                list: 'GetSlurp',
                create: 'CreateSlurp',
                update: 'UpdateSlurp',
                remove: 'DeleteSlurp'
            },
            url: Concentrator.route('GetSlurpScheduleList', 'Slurp'),
            deleteUrl: Concentrator.route('DeleteSlurpSchedule', 'Slurp'),
            updateUrl: Concentrator.route('UpdateSlurpSchedule', 'Slurp'),
            newUrl: Concentrator.route("CreateSlurpSchedule", "Slurp"),
            primaryKey: 'SlurpScheduleID',
            sortBy: 'SlurpScheduleID',
            structure: [
        { dataIndex: 'SlurpScheduleID', type: 'int' },
        { dataIndex: 'ProductCompareSourceID', type: 'int', header: 'Product Compare Source',
            editor: {
                xtype: 'productsources',
                allowBlank: false
            },
            renderer: function (val, meta, rec) {
                return rec.get('Source');
            }
        },
        { dataIndex: 'Source', type: 'string' },
        { dataIndex: 'ProductGroupMappingID', type: 'int', header: 'Product Group Name',
            editable: false,
            editor: {
              xtype: 'productGroupMapping',
              allowBlank: false
            },
            renderer: function (val, meta, record) {
              return record.get('ProductGroupName');
            }
          },
        { dataIndex: 'ProductGroupName', type: 'string' },
        { dataIndex: 'ProductID', type: 'int', header: 'Product',
            renderer: function (val, meta, record) {
                return record.get('ProductDescription');
            },
            editor: {
                xtype: 'product',
                allowBlank: true
            }
        },
        { dataIndex: 'ProductDescription', type: 'string' },
        { dataIndex: 'Interval', type: 'int', header: 'Interval',
            editor: {
                xtype: 'numberfield',
                allowBlank: false
            }
        },
        { dataIndex: 'IntervalType', type: 'int', header: 'Interval Type',
            editor: {
                xtype: 'interval',
                allowBlank: false
            },
            renderer: function (val, meta, record) {
                return record.get('IntervalName');
            }
            },
        
        { dataIndex: 'IntervalName', type: 'string' }
      ]
        });

        return self.grid;
    }
});