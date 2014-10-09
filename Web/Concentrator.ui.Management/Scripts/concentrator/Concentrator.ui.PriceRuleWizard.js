var averagePositionValue;

Concentrator.ui.PriceRuleWizard = (function () {

  var wiz = Ext.extend(Diract.ui.Wizard, {
    title: 'Price rule wizard',
    lazyLoad: true,
    width: 1000,
    height: 520,
    layout: 'fit',
    modal: true,

    constructor: function (config) {
      Ext.apply(this, config);

      this.params = {
        //connectorID: config.connectorID,
        productGroupMappingID: config.productGroupMappingID,
        productGroupID: config.productGroupID,
        brandID: config.brandID,
        vendorID: parseInt(config.vendorID)
      };

      this.items = this.getItems();
      this.url = Concentrator.route('Update', 'CalculatedPrice');



      Concentrator.ui.PriceRuleWizard.superclass.constructor.call(this, config);
    },
    getItems: function () {

      var that = this;

      if (this.pages) {
        return this.pages;
      }

      // Start of items for the first page
      var averagePosition = new Ext.form.Label({
        cls: 'averagePosition',
        height: 17,
        html: 'Average Position:'
      });

      averagePositionValue = new Ext.form.Label({
        cls: 'averagePositionValue',
        height: 17,
        html: '<img src="' + Concentrator.content("Content/images/default/grid/grid-loading.gif") + '" />'
        //html: '<img src=\'loading.gif\' /> Loading'
      });

      this.productSources = Ext.extend(Diract.ui.SearchBox, {
        valueField: 'ProductCompareSourceID',
        displayField: 'Name',
        searchUrl: Concentrator.route('GetPcSourceStore', 'ProductCompetitor'),
        allowBlank: false
      });

      Ext.reg('productsources', this.productSources);

      //      this.activeProductCompareSource = Ext.extend(Diract.ui.SearchBox, {
      //        valueField: 'ProductCompareSourceID',
      //        displayField: 'Value',
      //        allowBlank: false,
      //        name: 'ProductCompareSourceID',
      //        store: this.activeProductCompareSources
      //      });

      //      Ext.reg('activeProductCompareSource', this.activeProductCompareSource);

      var grid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        url: Concentrator.route("GetAveragePositionList", "CalculatedPrice"),
        primaryKey: 'ProductID',
        height: 300,
        updateUrl: Concentrator.route("UpdateAveragePosition", "CalculatedPrice"),
        permissions: {
          list: 'GetCalculatedPrice',
          create: 'CreateCalculatedPrice',
          update: 'UpdateCalculatedPrice'
        },
        params: this.params,
        structure: [
          { dataIndex: 'ProductID', type: 'string', header: 'Product',
            renderer: function (val, meta, record) {
              return record.get('ProductDescription');
            },
            editable: false
          },
          { dataIndex: 'ProductDescription', type: 'string' },
          { dataIndex: 'OwnPriceInc', type: 'float', header: 'Own Price Inc', editable: false },
          { dataIndex: 'CurrentRank', type: 'int', header: 'Current Rank', editable: false },
          { dataIndex: 'MinPriceInc', type: 'float', header: 'Min Price Inc', editable: false },
          { dataIndex: 'MaxPriceInc', type: 'float', header: 'Max Price ', editable: false },
          { dataIndex: 'EditableRank', type: 'int', header: 'Preferred Price Rank',
            editor: {
              xtype: 'numberfield'
            },
            editable: true
          },
        { dataIndex: 'ProductCompareSourceID', type: 'int', header: 'Product Compare Source',
          editor: {
            xtype: 'productsources'
          },
          editable: false,
          renderer: function (val, meta, record) {
            return record.get('Value');
          }
        },
          { dataIndex: 'Value', type: 'string' },
          { dataIndex: 'AverageRank', type: 'int' },
          { dataIndex: 'BrandID', type: 'int' },
          { dataIndex: 'VendorID', type: 'int' },
          { dataIndex: 'ProductGroupID', type: 'int' }
        ],
        rowActions: [
          {
            text: 'Compare data',
            iconCls: 'merge',
            handler: function (record) {

              var compareGrid = new Diract.ui.Grid({
                pluralObjectName: 'Compare Data',
                singularObjectName: 'Compare Data',
                url: Concentrator.route("GetCompareData", "CalculatedPrice"),
                params: {
                  productID: record.get('ProductID')
                },
                permissions: {
                  list: 'GetCalculatedPrice',
                  create: 'CreateCalculatedPrice',
                  update: 'UpdateCalculatedPrice'
                },
                structure: [
                  { dataIndex: 'ProductCompetitorPriceID', type: 'int' },
                  { dataIndex: 'ProductCompetitorMappingID', type: 'int', header: 'Product Competitor Mapping',
                    renderer: function (val, meta, record) {
                      return record.get('Competitor');
                    }
                  },
                  { dataIndex: 'Competitor', type: 'string' },
                  { dataIndex: 'CompareProductID', type: 'int', header: 'Compare Product' },
                  { dataIndex: 'ProductID', type: 'int', header: 'Product',
                    renderer: function (val, meta, record) {
                      return record.get('ProductDescription');
                    }
                  },
                  { dataIndex: 'ProductDescription', type: 'string' },
                  { dataIndex: 'Price', type: 'float', header: 'Price' },
                  { dataIndex: 'Stock', type: 'string', header: 'Stock' }
                ]
              });

              var widow = new Ext.Window({
                width: 800,
                height: 350,
                modal: true,
                layout: 'fit',
                items: [
                  compareGrid
                ]
              });

              widow.show();
            }

          },
          {

            text: 'Change Product Compare source',
            iconCls: 'merge',
            handler: function (record) {

              var widow;

              Ext.Ajax.request({
                url: Concentrator.route("GetActiveProductCompareSourceRadioButtons", "CalculatedPrice"),
                success: function (response, options) {
                  //Parse the resulting radioItems
                  var responseJSON = JSON.parse(response.responseText).results;

                  //Create a RadioGroup with the items
                  var newGroup = {
                    xtype: 'radiogroup',
                    fieldLabel: 'Compare Source',
                    id: 'compareSourceGroup',
                    columns: 1,
                    vertical: true
                  };

                  //Add the Ext.Radio Items
                  var items = new Array();
                  var i = 0;
                  for (var RadioItem in responseJSON) {
                    if (RadioItem != 'remove') {
                      items[i] = {
                        xtype: 'radio',
                        inputValue: responseJSON[RadioItem].InputValue,
                        boxLabel: responseJSON[RadioItem].BoxLabel,
                        checked: responseJSON[RadioItem].Checked,
                        listeners: {
                          check: function (radio, checked) {
                            Ext.Ajax.request({
                              url: Concentrator.route("UpdateProductCompareSource", "CalculatedPrice"),
                              success: function (response, options) {
                                // Compare source has been updated successfully
                                grid.store.reload();
                                this.close();
                              },
                              params: {
                                ProductID: record.get('ProductID'),
                                ProductCompareSourceID: radio.inputValue,
                                BrandID: record.get('BrandID'),
                                VendorID: record.get('VendorID'),
                                ProductGroupID: record.get('ProductGroupID'),
                                EditableRank: record.get('EditableRank')
                              },
                              scope: widow
                            })
                          }
                        }
                      };
                      i = i + 1;
                    }
                  }

                  newGroup.items = items;

                  //Add the new RadioGroup to the window
                  widow = new Ext.Window({
                    width: 200,
                    height: 500,
                    modal: true,
                    layout: 'fit',
                    items: newGroup,
                    title: 'Choose the new Product Compare Source'
                  });

                  widow.show();
                },
                params: {
                  productID: record.get('ProductID'),
                  ProductCompareSourceID: record.get('ProductCompareSourceID')
                },
                scope: window
              });


            }
          }
        ]
      });

      grid.store.on('load', function (store, recs, opt) {
        if (recs != undefined && recs[0] != undefined) {
          if (recs[0].json.AverageRank != undefined) {
            averagePositionValue.container.dom.children[1].innerHTML = recs[0].json.AverageRank;
          }
          if (recs[0].json.GroupStartRank != undefined) {
            firstPosition.container.dom.children[0].value = recs[0].json.GroupStartRank;
            groupStartRank = recs[0].json.GroupStartRank;
          }
          if (recs[0].json.GroupEndRank != undefined) {
            secondPosition.container.dom.children[0].value = recs[0].json.GroupEndRank;
            groupEndRank = recs[0].json.GroupEndRank;
          }
          if (recs[0].json.ContentPriceLabel != undefined) {
            contentPriceLabelField.container.dom.children[0].value = recs[0].json.ContentPriceLabel;
            contentPriceLabel = recs[0].json.ContentPriceLabel;
          }
        }
      }, this);

      var averagePanel = new Ext.Panel({
        items: [averagePosition, averagePositionValue],
        border: false
      });

      var gridPanel = new Ext.Panel({
        bodyStyle: "margin-top: 10px;",
        items: [grid],
        border: false
      });

      var firstPosition = new Ext.form.NumberField({
        name: 'MinComparePricePosition',
        fieldLabel: 'Group start rank:',
        width: 165,
        listeners: {
          change: function (field, newValue, oldValue) {
            groupStartRank = newValue;
          }
        }
      });

      var secondPosition = new Ext.form.NumberField({
        name: 'MaxComparePricePosition',
        fieldLabel: 'Group end rank:',
        listeners: {
          change: function (field, newValue, oldValue) {
            groupEndRank = newValue;
          }
        }
      });
      
      var contentPriceLabelField = new Ext.form.TextField({
        name: 'ContentPriceLabel',
        fieldLabel: 'Content Price Label:',
        listeners: {
          change: function (field, newValue, oldValue) {
            contentPriceLabel = newValue;
          }
        }
      });

      var positionRangePanel = new Ext.Panel({
        border: false,
        layout: 'form',
        bodyStyle: 'margin-top: 10px;',
        items: [ firstPosition, secondPosition, contentPriceLabelField]
      });

      // Start of items for the second page
      var averageMargin = new Ext.form.Label({
        cls: 'averageMargin',
        height: 17,
        html: 'Average Margin:'
      });

      var averageMarginValue = new Ext.form.Label({
        cls: 'averageMarginValue',
        height: 17,
        html: '<img src="' + Concentrator.content("Content/images/default/grid/grid-loading.gif") + '" />'
        //        html: '<img src=\'loading.gif\' /> Loading'
      });

      var bottomMargin = new Ext.form.NumberField({
        name: 'BottomMargin',
        fieldLabel: 'Bottom Margin',
        listeners: {
          change: function (field, newValue, oldValue) {
            bottomMargin = newValue;
          }
        }
      });

      var secondGrid = new Diract.ui.Grid({
        pluralObjectName: 'Products',
        singularObjectName: 'Product',
        url: Concentrator.route("GetAverageMarginList", "CalculatedPrice"),
        primaryKey: 'ProductID',
        params: this.params,
        //height: 250,
        updateUrl: Concentrator.route("UpdateAverageMargin", "CalculatedPrice"),
        permissions: {
          list: 'GetCalculatedPrice',
          create: 'CreateCalculatedPrice',
          update: 'UpdateCalculatedPrice'
        },
        structure: [
          { dataIndex: 'ProductID', type: 'int', header: 'Product',
            renderer: function (val, meta, record) {
              return record.get('ProductDescription');
            },
            editable: false
          },
          { dataIndex: 'ProductDescription', type: 'string' },
          { dataIndex: 'CostPriceEx', type: 'float', header: 'CostPrice', editable: false },
          { dataIndex: 'PriceEx', type: 'float', header: 'Price', editable: false },
          { dataIndex: 'Margin', type: 'float', header: 'Margin', editable: true, editor: { xtype: 'numberfield'} },
          { dataIndex: 'AverageMargin', type: 'int' },
          { dataIndex: 'MinMargin', type: 'float' }
        ]
      });

      secondGrid.store.on('load', function (store, recs, opt) {
        if (recs != undefined && recs[0] != undefined) {
          if (recs[0].json.AverageMargin != undefined) {
            averageMarginValue.container.dom.children[1].innerHTML = recs[0].json.AverageMargin;
          }

          if (recs[0].json.AverageMargin != undefined) {
            bottomMargin.container.dom.children[0].value = recs[0].json.MinMargin;
          }
        }
      }, this);

      var averageMarginPanel = new Ext.Panel({
        border: false,
        items: [averageMargin, averageMarginValue]
      });

      var secondGridPanel = new Ext.Panel({
        border: false,
        layout: 'fit',
        height: 300,
        items: [secondGrid],
        bodyStyle: 'margin-top: 20px;'
      });


      var bottomMarginPanel = new Ext.Panel({
        padding: 20,
        layout: 'form',
        border: false,
        items: [bottomMargin],
        bodyStyle: 'margin-top: 20px;'
      });

      // Start of items for third panel

      var startDate = new Ext.form.DateField({
        name: 'StartDate',
        fieldLabel: 'Start Date:',
        width: 165,
        listeners: {
          change: function (field, newValue, oldValue) {
            startDate = newValue;
          }
        }
      });

      var endDate = new Ext.form.DateField({
        name: 'EndDate',
        fieldLabel: 'End Date:',
        width: 165,
        listeners: {
          change: function (field, newValue, oldValue) {
            endDate = newValue;
          }
        }
      });

      var index = new Ext.form.TextField({
        name: 'ContentPriceRuleIndex',
        hiddenName: 'ContentPriceRuleIndex',
        fieldLabel: 'Index:',
        width: 165,
        listeners: {
          change: function (field, newValue, oldValue) {
            index = newValue;
          }
        }
      });

      var dateForm = new Ext.Panel({
        padding: 20,
        border: false,
        layout: 'form',
        items: [
          startDate,
          endDate, //margin is altijd procent
          index
        ]
      });

      // Defines all the pages and fill them with panels containing the items
      var firstPage = new Ext.FormPanel({
        padding: 20,
        border: false,
        items: [averagePanel, gridPanel, positionRangePanel]
      });

      var secondPage = new Ext.FormPanel({
        padding: 20,
        border: false,
        items: [averageMarginPanel, secondGridPanel, bottomMarginPanel],
        height: 520
      });

      var thirdPage = new Ext.form.FormPanel({
        padding: 20,
        border: false,
        items: [dateForm],
        height: 520
      });

      this.pages = [firstPage, secondPage, thirdPage];
      // and finally get values for the two labels
      //      Diract.silent_request({
      //        url: Concentrator.route("GetWizardValues", "CalculatedPrice"),
      //        params: {
      //          productGroupID: this.productGroupID,
      //          brandID: this.brandID,
      //          vendorID: this.vendorID,
      //          start: 0,
      //          limit: 5,
      //          sort: 'AverageMargin'
      //        },
      //        success: function (data) {
      //          if (data.results.length > 0) {
      //            averagePositionValue.setText(' ' + data.results[0].AveragePricePosition + ' ');
      //            averageMarginValue.setText(' ' + data.results[0].AverageMargin + ' ');
      //          }
      //        }
      //      });

      return this.pages;
    }

    //    getFormValues: function () {
    //      var initializations = this.getItems();
    //      var params = this.getFormValues(initializations);               
    //          
    //      Ext.apply(params, {
    //        productGroupMappingID: this.productGroupMappingID,
    //        brandID: 2,
    //        connectorID: this.connectorID
    //      });

    //      this.submitFormWizard(params);
    //    }

  });

  return wiz;
})();
