/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.Grid'] }, function () {
  var managePageName = '';
  var controller = '';
  var defaultAction = '';
  var showTemplateButtons = false;
  var priceLabelColumns = false;
  Diract.ui.ExcelGrid = (function () {
    var templatewindow = null;
    var that = this;
    var templates = new Array();

    var trans = Ext.extend(Diract.ui.Grid, {

      constructor: function (config) {
        Ext.Ajax.disableCaching = false;
        managePageName = config.managePage;
        controller = config.controller;
        defaultAction = config.defaultAction;
        priceLabelColumns = config.priceLabelColumns;
        //if (config.showTemplateButtons != undefined) {
        //    showTemplateButtons = config.showTemplateButtons;
        //} else {
        //    showTemplateButtons = false;
        //}
        if ((managePageName != undefined) && (managePageName != '') &&
        (controller != undefined) && (controller != '') &&
        (defaultAction != undefined) && (defaultAction != '')) {
          showTemplateButtons = true;
        } else {
          showTemplateButtons = false;
        }
        Ext.apply(this, config);
        this.initCustomButtons();

        Diract.ui.ExcelGrid.superclass.constructor.call(this, config);
      },

      initCustomButtons: function () {

        var customBBarButtons = this.bbarCustomButtons || [],
that = this;

        if (showTemplateButtons) {
          customBBarButtons.push({
            text: 'Export from template',
            iconCls: 'xls',
            handler: function () {



              Ext.Ajax.on('requestcomplete', function () {


              });
              Ext.Ajax.request({
                url: Concentrator.route("GetTemplates", "Template"),
                params: { id: managePageName },
                method: 'GET',
                success: function (response, request) {


                  templates = Ext.decode(response.responseText);

                  if (that.chooseColumns) {

                    that.itemsLeft = [];
                    //that.columnsSet = templates;
                    Ext.each(templates, function (item) {

                      that.header = item.header;
                      that.name = item.dataIndex;

                      checkBox = new Ext.form.Radio({
                        fieldLabel: that.header,
                        name: 'chosenTemplates',
                        value: that.dataIndex
                      });

                      that.itemsLeft.push(checkBox);

                    });

                    itemsPerForm = (that.itemsLeft.length / 2);

                    that.itemsRight = [];

                    that.itemsRight.push(that.itemsLeft.splice(itemsPerForm));

                    var formLeft = new Ext.form.FormPanel({
                      padding: 20,
                      region: 'west',
                      width: 350,
                      //height: 300,                  
                      labelWidth: 150,
                      items: [
that.itemsLeft
                      ]
                    });

                    var formRight = new Ext.form.FormPanel({
                      padding: 20,
                      region: 'center',
                      width: 350,
                      labelWidth: 150,
                      items: [
that.itemsRight
                      ]
                    });

                    var window = new Ext.Window({
                      title: 'Select the template',
                      width: 715,
                      height: 300,
                      layout: 'border',
                      modal: true,
                      items: [
formLeft,
formRight
                      ],
                      buttons: [
new Ext.Button({
  text: 'Export',
  handler: function () {

    var checkedArray = [];

    Ext.each(formLeft.items.items, function (formItem) {

      if (formItem.checked == true) {
        checkedArray.push(formItem.initialConfig.fieldLabel);
      }

    });

    Ext.each(formRight.items.items, function (formItem) {

      if (formItem.checked == true) {
        checkedArray.push(formItem.initialConfig.fieldLabel);
      }

    });

    state = that.getState();

    return that.exportToExcelFromTemplate(state, checkedArray);

  }
})
                      ]
                    });

                    window.show();

                  }
                  else {

                    var state = that.getState();

                    return that.exportToExcel(state);
                  }

                },
                failure: function (result, request) {
                  alert("Failure: " + result.responseText);
                }

              });

            }

          });
          customBBarButtons.push({
            text: 'Create new export template',
            iconCls: 'xls',
            handler: function () {

              if (that.chooseColumns) {

                that.itemsLeft = [];
                //get the columns

                Ext.each(that.columnsSet, function (item) {

                  that.header = item.header;
                  that.name = item.dataIndex;

                  checkBox = new Ext.form.Checkbox({
                    fieldLabel: that.header,
                    name: that.name
                  });

                  that.itemsLeft.push(checkBox);

                });

                itemsPerForm = (that.itemsLeft.length / 2);

                that.itemsRight = [];



                var formTemplatesLeft = new Ext.form.FormPanel({
                  padding: 20,
                  region: 'west',
                  width: 350,
                  //height: 300,                  
                  labelWidth: 150,
                  items: [
  that.itemsLeft
                  ]
                });

                var textfield = new Ext.form.TextField({
                  fieldLabel: 'Template name',
                  name: 'templateName',
                  value: ''
                });

                var formTemplatesRight = new Ext.form.FormPanel({
                  padding: 20,
                  region: 'center',
                  width: 350,
                  labelWidth: 150,
                  items: [

  that.itemsRight,
  textfield

                  ]
                });
                templatewindow = new Ext.Window({
                  title: 'Create new export template',
                  width: 715,
                  height: 300,
                  layout: 'border',
                  modal: true,
                  items: [
  formTemplatesLeft,
  formTemplatesRight
                  ],
                  buttons: [
  new Ext.Button({
    text: 'Create',
    handler: function () {

      var checkedArray = [];

      Ext.each(formTemplatesLeft.items.items, function (formItem) {

        if (formItem.checked == true) {
          checkedArray.push(formItem.initialConfig.fieldLabel);
        }

      });

      Ext.each(formTemplatesRight.items.items, function (formItem) {

        if (formItem.checked == true) {
          checkedArray.push(formItem.initialConfig.fieldLabel);
        }


      });
      var name = formTemplatesRight.getForm().findField("templateName").getValue();
      state = that.getState();

      that.createNewTemplate(state, checkedArray, name);


    }
  })
                  ]
                });

                templatewindow.show();

              }
              else {

                var state = that.getState();

                that.createNewTemplate(state);
              }

            }


          });
        }
        customBBarButtons.push(
  {
    text: 'Export to excel',
    iconCls: 'xls',
    handler: function () {

      if (that.chooseColumns) {

        that.itemsLeft = [];

        Ext.each(that.columnsSet, function (item) {

          that.header = item.header;
          that.name = item.dataIndex;

          checkBox = new Ext.form.Checkbox({
            fieldLabel: that.header,
            name: that.name
          });

          that.itemsLeft.push(checkBox);

        });

        itemsPerForm = (that.itemsLeft.length / 2);

        that.itemsRight = [];

        that.itemsRight.push(that.itemsLeft.splice(itemsPerForm));

        var formLeft = new Ext.form.FormPanel({
          padding: 20,
          region: 'west',
          width: 350,
          //height: 300,                  
          labelWidth: 150,
          items: [
    that.itemsLeft
          ]
        });

        var formRight = new Ext.form.FormPanel({
          padding: 20,
          region: 'center',
          width: 350,
          labelWidth: 150,
          items: [
    that.itemsRight
          ]
        });

        var window = new Ext.Window({
          title: 'Select the columns you want to export',
          width: 715,
          height: 300,
          layout: 'border',
          modal: true,
          items: [
    formLeft,
    formRight
          ],
          buttons: [
    new Ext.Button({
      text: 'Export',
      handler: function () {

        var checkedArray = [];

        Ext.each(formLeft.items.items, function (formItem) {

          if (formItem.checked == true) {
            checkedArray.push(formItem.initialConfig.fieldLabel);
          }

        });

        Ext.each(formRight.items.items, function (formItem) {

          if (formItem.checked == true) {
            checkedArray.push(formItem.initialConfig.fieldLabel);
          }

        });

        state = that.getState();

        return that.exportToExcel(state, checkedArray);

      }
    })
          ]
        });

        window.show();

      }
      else {

        var state = that.getState();

        return that.exportToExcel(state);
      }

    }
  }
);
        this.bbarCustomButtons = customBBarButtons;
      },
      createNewTemplate: function (state, columns, name) {
        var that = this;

        Diract.silent_request({
          url: Concentrator.route('Save', 'UserComponentState'),
          params: {
            data: Ext.encode(state),
            name: that.stateId
          },
          success: function () {
            var pageNumber = (Math.ceil((that.bbarPagingTool.p.cursor + that.pageSize) / that.pageSize)),
            start = (pageNumber - 1) * that.pageSize;
            var params = {
              url: that.exportToExcelUrl || that.url,
              name: that.pluralObjectName.replace(' ', '_'), //otherwise param fail
              limit: that.pageSize,
              start: start,
              field: that.store.sortInfo.direction,
              sort: that.store.sortInfo.field,
              stateName: that.stateId,
              templateName: name,
              managePageName: managePageName
            };

            if (columns) {
              Ext.apply(params, {
                columnsOverride: Ext.encode(columns)
              });
            }
            if (that.gridFilters) {
              var tempFilters = that.convertFilters(that.gridFilters.getFilterData());
              Ext.apply(params, {
                filters: tempFilters
              });
            } else {
              Ext.apply(params, {
                filters: ''
              });
            }
            //var exportUrl = Concentrator.route('CreateTemplate', 'Template', params);
            Diract.silent_request({
              url: Concentrator.route("CreateTemplate", "Template"),
              params: params,
              success: function (result) {

                templatewindow.hide();
                templatewindow = null;
                textfield = null;
                formTemplatesRight = null;
              }
            });

            //var exportUrl = '';
            //if (that.gridFilters) {
            //    exportUrl = Ext.urlAppend(exportUrl, that.gridFilters.buildQuery(that.gridFilters.getFilterData()));
            //}

            //if (that.excelRequestParams) {

            //                            exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode(that.excelRequestParams));
            //                      }

            // we need to get the controller and send a getlist
            //var exportUrl = Concentrator.route(defaultAction, controller);
            //window.location = exportUrl;
          }
        })

      },
      exportToExcelFromTemplate: function (state, columns) {
        var that = this;
        Diract.silent_request({
          url: Concentrator.route("Save", "UserComponentState"),
          params: {
            data: Ext.encode(state),
            name: that.stateId
          },
          success: function () {
            var pageNumber = (Math.ceil((that.bbarPagingTool.p.cursor + that.pageSize) / that.pageSize)),
        start = (pageNumber - 1) * that.pageSize;
            var params = {
              url: that.exportToExcelUrl || that.url,
              name: that.pluralObjectName.replace(' ', '_'), //otherwise param fail
              limit: that.pageSize,
              start: start,
              field: that.store.sortInfo.direction,
              sort: that.store.sortInfo.field,
              stateName: that.stateId,
              labelColumns: priceLabelColumns
            };
            if (columns) {
              var templateName = columns[0];
              Ext.Ajax.request({
                url: Concentrator.route("GetColumnsForTemplate", "Template"),
                params: {
                  id: templateName
                },
                success: function (response, request) {

                  var templateColumns = Ext.decode(response.responseText);

                  var templateFilters = that.getFiltersFromTemplates(templateColumns);
                  Ext.apply(params, {
                    columnsOverride: Ext.encode(templateColumns)
                  });
                  var exportUrl = Concentrator.route('ToExcel', 'Excel', params);

                  if (that.gridFilters) {
                    exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode(that.gridFilters.buildQuery(templateFilters)));

                  }

                  if (that.excelRequestParams) {

                    exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode(that.excelRequestParams));
                  }

                  window.location = exportUrl;
                }
              });
            }
          }



        });
      },
      exportToExcel: function (state, columns) {
        //take care of the state of the grid        

        var that = this;

        Diract.silent_request({
          url: Concentrator.route('Save', 'UserComponentState'),
          params: {
            data: Ext.encode(state),
            name: that.stateId
          },
          success: function () {
            var pageNumber = (Math.ceil((that.bbarPagingTool.p.cursor + that.pageSize) / that.pageSize)),
       start = (pageNumber - 1) * that.pageSize;

            var params = {
              url: that.exportToExcelUrl || that.url,
              name: that.pluralObjectName.replace(' ', '_'), //otherwise param fail

              limit: that.pageSize,
              start: start,

              field: that.store.sortInfo.direction,
              sort: that.store.sortInfo.field,
              stateName: that.stateId,
              labelColumns: priceLabelColumns
            };

            if (that.overridePageSize) {
              params.limit = 1000000;
              params.start = 0;
            }

            if (columns) {
              Ext.apply(params, {
                columnsOverride: Ext.encode(columns)
              });
            }

            var exportUrl = Concentrator.route('ToExcel', 'Excel', params);

            if (that.gridFilters) {
              exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode(that.gridFilters.buildQuery(that.gridFilters.getFilterData())));
            }

            if (that.excelRequestParams) {

              exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode(that.excelRequestParams));
            }

            window.location = exportUrl;
          }
        });
      },
      convertFilters: function (filters) {

        var t = that;
        var columnConfig = this.getColumnModel().config;
        if (filters.length != 0) {
          var result = new Array();
          var filterCounter = 0;
          var resultCounter = 0;
          var filtersLength = filters.length;
          for (filterCounter = 0; filterCounter < filtersLength; filterCounter++) {
            var element = filters[filterCounter];
            var temp = '';
            var configCounter = 0;
            for (configCounter = 0; configCounter < columnConfig.length; configCounter++) {
              var cElement = columnConfig[configCounter];
              if (cElement.dataIndex == element.field) {
                temp = cElement.header;
              }
            }
            if (element.data.type == 'string') {
              temp = temp + '|eq|' + element.data.value + '|' + element.data.type;
              result[resultCounter++] = temp;
            } else {
              temp = temp + '|' + element.data.comparison + '|' + element.data.value + '|' + element.data.type;
              result[resultCounter++] = temp;
            }
          }
          var filterstring = result.join(',');
          return filterstring;
        }

        return '';
      },
      getFiltersFromTemplates: function (templateArray) {

        var filterArray = new Array();
        var filterCounter = 0;
        var templatesLength = templateArray.length;
        var columnConfig = this.getColumnModel().config;
        var configLength = columnConfig.length;
        for (var templateCounter = 0; templateCounter < templatesLength; templateCounter++) {
          var t = templateArray[templateCounter];
          if (t.comparison != null) {
            var filterData = { comparison: t.comparison, type: t.type, value: t.value };
            var filterName = '';
            for (var columnCounter = 0; columnCounter < configLength; columnCounter++) {
              var ce = columnConfig[columnCounter];
              if (ce.header == t.name) {
                filterName = ce.dataIndex;
              }
            }
            var filter = { field: filterName, data: filterData }
            filterArray[filterCounter++] = filter;
          }
        }
        return filterArray;
      },
      mergeState: function (checkedArray) {
        var data = [],
    cm = this.colModel;

        Ext.each(checkedArray, function (item) {
          var col = cm.getColumnsBy(function (column) {
            return column.header == item.initialConfig.fieldLabel;

          })[0];

          data.push({
            width: col.width,
            header: col.header,
            hidden: col.hidden === true,
            dataIndex: col.dataIndex
          });
        });
        return { columns: data };

      }


    });

    return trans;

  })();
});