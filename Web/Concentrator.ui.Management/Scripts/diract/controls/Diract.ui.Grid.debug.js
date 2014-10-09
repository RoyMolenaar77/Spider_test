/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />
//Diract.components.push(function() {
Diract.addComponent({ dependencies: [] }, function () {
  /**
  * @class Diract.ui.Grid
  * @extends Ext.grid.EditorGridPanel
  * 
  * A custom class to create a generic grid panel to be used in Diract's ExtJS applications.
  * It's an EditorGridPanel in it's pure form but set defaults and allows for easy expanding. Every grid within
  * the application should eventually inherit from this grid to keep the main config central.
  *
  * Built on Ext JS Library 3.0.3
  * 
  * @param {Object} config The configuration options
  * 
  * @author Coen van der Wel, Diract IT <c.wel@diract-it.nl>
  * @copyright Diract IT 2009-2010
  */

  /**
  * The following changes were introduced turning GridHelper from a factory method to a real Object:
  *
  * - Make sure you create a Diract.ui.Grid using the "new" keyword now! It's an Object!
  * - Renamed the following config options:
  *   - columns              ->  columnsSet
  *   - columnModel          ->  colModel
  *   - selectionModel       ->  selModel
  *   - selectionModelConfig ->  selModelConfig
  *   - sourceUrl            ->  url
  *   - sourceParams         ->  params
  *   - editObjectUrl        ->  updateUrl
  *   - editObjectUrlGet     ->  editUrl
  *   - newObjectUrl         ->  newUrl
  *   - deleteObjectUrl      ->  deleteUrl
  *   - newObjectFields      ->  fields
  *   - gridListeners        ->  listeners
  *   - newWindowConfig      ->  windowConfig
  *                          ->  formConfig
  * - It's now stimulated to use Update for altering records and Edit to get record data, try to sustain this!
  * - It used to be possible to hide the bottom toolbar specifying an empty string for it. Use hideBottomBar=True now.
  */

  Diract.ui.Grid = (function () {

    var createSortFunctionExtJs = Ext.data.GroupingStore.prototype.createSortFunction;
    Ext.override(Ext.data.GroupingStore, {
      createSortFunction: function (fieldName, direction) {
        var field = this.fields.get(fieldName);
        if (!field || !field.sortFunction) {
          return createSortFunctionExtJs.call(this, fieldName, direction);
        }
        direction = direction || "ASC";
        var directionModifier = direction.toUpperCase() == "DESC" ? -1 : 1;

        var f = field.sortFunction;

        return function (r1, r2) {
          var comp = directionModifier * f(r1, r2);
          return comp;
        };
      }
    });


    var createAddButton = function () {

      if (this.addMenuActions) {
        return [{
          text: String.format(Diract.text.addNewBtnText, this.singularObjectName),
          tooltip: String.format(Diract.text.addNewBtnTooltip, this.singularObjectName),
          iconCls: "add",
          scope: this,
          menu: [new Ext.Action({
            text: "Default",
            tooltip: String.format(Diract.text.addNewBtnTooltip, this.singularObjectName),
            iconCls: "add",
            scope: this,
            handler: function () {
              this.showNewWindow();
            }
          }), this.addMenuActions]
        }];
      } else {
        // the button to add new records (new popup)
        return new Ext.Toolbar.Button({
          text: String.format(Diract.text.addNewBtnText, this.singularObjectName),
          tooltip: String.format(Diract.text.addNewBtnTooltip, this.singularObjectName),
          iconCls: "add",
          scope: this,
          handler: function () {
            this.showNewWindow();
          }
        });
      }
    };

    var createUrl = function (baseUrl, record, prefix) {

      if (typeof this.primaryKey === "string") {
        return baseUrl + "/" + record.get(this.primaryKey);
      } else if (Ext.isArray(this.primaryKey)) {
        var params = {};
        Ext.each(this.primaryKey, function (key) {
          var k = (this.keyPrefix || '') + key;
          params[k] = record.get(prefix ? k : key);
        }, this);

        return baseUrl + "?" + Ext.urlEncode(params);
      }

    };

    var createDeleteButton = function () {
      // button to delete selected row(s)
      return new Ext.Toolbar.Button({
        text: this.deleteButtonText || String.format(Diract.text.deleteBtnText, this.singularObjectName),
        tooltip: String.format(Diract.text.deleteBtnTooltip, this.singularObjectName),
        iconCls: "delete",
        disabled: this.deletePredicate,
        scope: this,
        handler: function () {
          Ext.MessageBox.confirm(Diract.text.confirmTitle, Diract.text.deleteBtnConfirmation, function (btn) {
            if (btn == "yes") {
              // for each selected record
              var selectedRows = this.selModel.getSelections();
              for (var i = 0; i < selectedRows.length; i++) {
                // build the URL to send data to (POST)
                var url = createUrl.call(this, this.deleteUrl, selectedRows[i], true);
                // take additional keys into account
                if (this.otherKeys) {
                  for (var j = 0; j < this.otherKeys.length; j++) {
                    var otherKey = {}; var otherValue = selectedRows[i][self.keyPrefix + this.otherKeys[j]];
                    otherKey["key_" + this.otherKeys[j]] = otherValue ? otherValue : selectedRows[i].get(this.otherKeys[j]);
                    url = Ext.urlAppend(url, Ext.urlEncode(otherKey));
                  }
                }

                // the actual request
                Diract.request({
                  url: url,
                  scope: this,
                  params: this.params,
                  flushAt: selectedRows.length,
                  waitMsg: Diract.text.processingMessage,
                  success: this.deleteCallback || undefined,
                  successMsg: String.format(Diract.text.deleteSuccessMessage, this.singularObjectName),
                  summaryMsg: Diract.text.deleteSuccessSummaryMessage + this.pluralObjectName + ".",
                  onFlush: function (succeeds, fails, errors) {
                    if (succeeds > 0 && this.callback) this.callback(this.store, succeeds, "delete");
                    this.store.reload();
                  }
                });
              } // eo for
            } // eo if btn
          }, this); // eo confirm
        } // eo handler
      });
    };

    var getChangedAndBooleanFields = function (record) {
      var changes = record.getChanges();
      Ext.each(record.fields.items, function (field) {
        if (field.type === "boolean") {
          changes[field.name] = record.get(field.name);
        }
      });
      return changes;
    };

    var createSaveAndExitButton = function () {
      // the button to save modified records
      return new Ext.Toolbar.Button({
        text: Diract.text.editAndExitBtnText,
        tooltip: Diract.text.editBtnTooltip,
        iconCls: "save",
        disabled: true, // start disabled
        scope: this,
        handler: function () {
          // get edits and set button state
          var edits = this.store.getModifiedRecords();
          this.discardButton.disable(); this.saveButton.disable();
          this.saveAndExitButton.disable();
          // build requests
          for (var i = 0, len = edits.length; i < len; i++) {
            // get field changes

            var changedFields = getChangedAndBooleanFields(edits[i]);
            // build URL to send update data to (POST)

            var url = createUrl.call(this, this.updateUrl, edits[i], true);

            // do the actual request
            Diract.request({
              url: url,
              scope: this,
              params: Ext.apply(changedFields, this.params),
              flushAt: edits.length,
              summaryMsg: "{0} " + this.pluralObjectName + Diract.text.editSuccessMessage,
              successMsg: this.singularObjectName + Diract.text.editSuccessMessage,
              success: this.successCallback || undefined,
              onFlush: function (succeeds, fails, errors) {

                if (succeeds > 0 && this.callback) this.callback(this.store, succeeds, "update");
                if (fails <= 0) {
                  this.store.commitChanges();
                  if (this.refreshAfterSave) {
                    this.store.reload();
                  }
                  //check if grid is in tab
                  if (!Concentrator.ViewInstance.tabPanel.remove(this.ownerCt, true)) {
                    this.ownerCt.destroy();
                  };
                }
                //   this.failureCallback(result);
                //  }
              }
            });
          } // eo for
        } // eo handler
      });
    }

    var createSaveButton = function () {
      // the button to save modified records
      return new Ext.Toolbar.Button({
        text: Diract.text.editBtnText,
        tooltip: Diract.text.editBtnTooltip,
        iconCls: "save",
        disabled: true, // start disabled
        scope: this,
        handler: function () {
          // get edits and set button state
          var edits = this.store.getModifiedRecords();
          this.discardButton.disable(); this.saveButton.disable();
          if (this.saveAndExitButton) { this.saveAndExitButton.disable(); }
          // build requests
          for (var i = 0, len = edits.length; i < len; i++) {
            // get field changes

            var changedFields = getChangedAndBooleanFields(edits[i]);
            // build URL to send update data to (POST)

            var url = createUrl.call(this, this.updateUrl, edits[i], true);

            // do the actual request
            Diract.request({
              url: url,
              scope: this,
              params: Ext.apply(changedFields, this.params),
              flushAt: edits.length,
              summaryMsg: "{0} " + this.pluralObjectName + Diract.text.editSuccessMessage,
              successMsg: this.singularObjectName + Diract.text.editSuccessMessage,
              success: this.successCallback || undefined,
              onFlush: function (succeeds, fails, errors) {

                if (succeeds > 0 && this.callback) this.callback(this.store, succeeds, "update");
                if (fails <= 0) {
                  this.store.commitChanges();
                  if (this.refreshAfterSave) {
                    this.store.reload();
                  }
                }
                //   this.failureCallback(result);
                //  }
              }
            });
          } // eo for
        } // eo handler
      });
    };

    var createDiscardButton = function () {
      // button to discard changes
      return new Ext.Toolbar.Button({
        text: Diract.text.discardBtnText,
        tooltip: Diract.text.discardBtnTooltip,
        iconCls: "cancel",
        disabled: true, // start disabled
        scope: this,
        handler: function () {
          var edits = this.store.getModifiedRecords();
          Ext.MessageBox.confirm(Diract.text.confirmTitle, Diract.text.discardBtnConfirmation, function (btn) {
            if (btn == "yes") {
              this.store.rejectChanges();
              this.discardButton.disable();
              this.saveButton.disable();
              if (this.saveAndExitButton) { this.saveAndExitButton.disable(); }
            }
          }, this);
        } // eo handler
      });
    };



    /**
    * Inline adding buttons.
    */

    // button to allow adding new lines (inline adding)
    var createAddNewLineButton = function () {
      return new Ext.Toolbar.Button({
        text: Diract.text.newLineBtnText,
        tooltip: Diract.text.newLineBtnTooltip,
        iconCls: "add",
        scope: this,
        handler: function () {
          // create a new record
          var newRecord = new this.emptyRecord();
          // for every field
          for (var val in this.dataSet) if (this.dataSet[val].name) {
            // set an approperiate default value
            switch (this.dataSet[val].type) {
              case "numeric":
              case "int":
                newRecord.data[this.dataSet[val].name] = 0;
                break;
              case "date":
                newRecord.data[this.dataSet[val].name] = new Date();
                break;
              case "boolean":
              case "bool":
                newRecord.data[this.dataSet[val].name] = false;
                break;
              default:
                newRecord.data[this.dataSet[val].name] = "Undefined";
                break;
            } // eo switch
          } // eo for
          // process new record
          this.store.numNewLines++; this.store.add(newRecord);
          // and enable related buttons
          this.saveNewLinesButton.enable(); this.removeNewLinesButton.enable();
        } // eo handler
      });
    };

    // button to save the newly added inline rows to the database
    var createSaveNewLinesButton = function () {
      return new Ext.Toolbar.Button({
        text: Diract.text.saveNewLineBtnText,
        disabled: true, // start disabled
        tooltip: Diract.text.saveNewLineBtnTooltip,
        iconCls: "save",
        scope: this,
        handler: function () {
          // disable buttons
          this.saveNewLinesButton.disable(); this.removeNewLinesButton.disable();
          // for each item in the grid
          var amount = this.store.numNewLines;
          for (var rowIndex in this.store.data.items) {
            // if this is a newly inline added item
            if (this.store.data.items[rowIndex].id && this.store.data.items[rowIndex].phantom) {
              // do the actual request
              Diract.request({
                url: this.inlineNewUrl,
                params: this.store.data.items[rowIndex].data,
                flushAt: amount,
                summaryMsg: Diract.text.saveNewLineSuccessSummaryMessage + this.pluralObjectName + ".",
                successMsg: this.singularObjectName + Diract.text.saveNewLineSuccessMessage,
                onFlush: function (succeeds, fails, errors) {
                  this.store.numNewLines = fails;
                  if (succeeds > 0 && this.callback) this.callback(this.store, succeeds, "add");
                  if (fails > 0) {
                    this.saveNewLinesButton.enable(); this.removeNewLinesButton.enable();
                  } else this.store.reload();
                }
              });
            } // eo if newly inline added item
          } // eo for
        } // eo handler
      });
    };

    // button to remove selected newly inline added rows
    var createRemoveNewLinesButton = function () {
      return new Ext.Toolbar.Button({
        text: Diract.text.deleteNewLineBtnText,
        disabled: true, // start disabled
        tooltip: Diract.text.deleteNewLineBtnTooltip,
        iconCls: "delete",
        scope: this,
        handler: function () {
          // get all selected rows that were added with the inline adding stuff
          var selectedRows = this.selModel.getSelections(), removeLines = [];
          for (i = 0; i < selectedRows.length; i++) if (selectedRows[i].phantom) removeLines[removeLines.length] = selectedRows[i];
          // if we found 1 or more of these lines, process them
          if (removeLines.length > 0) {
            Ext.MessageBox.confirm(Diract.text.confirmTitle, String.format(Diract.text.deleteNewLineBtnConfirmation, removeLines.length, (removeLines.length == 1 ? Diract.text.lineSingular : Diract.text.linePlural)), function (btn) {
              if (btn == "yes") {
                // remove them all
                for (i = 0; i < removeLines.length; i++) {
                  this.store.remove(removeLines[i]);
                  // if this was the last one, disable buttons, too
                  if (--this.store.numNewLines == 0) {
                    this.saveNewLinesButton.disable();
                    this.removeNewLinesButton.disable();
                  }
                } // eo for
              } // eo if btn
            }); // eo confirm
          } else Ext.MessageBox.alert(Diract.text.errorTitle, Diract.text.deleteNewLineBtnError);
        } // eo handler
      });
    };


    /**
    * Bottom buttons.
    */
    var createClearFilterButton = function () {
      // the button to clear grid filters
      return new Ext.Toolbar.Button({
        text: Diract.text.clearFilterBtnText,
        tooltip: Diract.text.clearFilterBtnTooltip,
        iconCls: "clearFilters",
        scope: this,
        handler: function () {
          this.clearFilters();
        }
      });
    };

    var createSetColumnNamesButton = function () {
      // reset grid view button
      return new Ext.Toolbar.Button({
        text: Diract.text.setColumnNamesBtnText,
        tooltip: Diract.text.setColumnNamesTooltip,
        iconCls: "state_reset",
        scope: this,
        handler: function () {

          var columnHeaders = [];

          for (var i = 0; i < this.columnsSet.length; i++) {
            columnHeaders.push({ xtype: 'field', id: this.columnsSet[i].defaultHeader, fieldLabel: this.columnsSet[i].defaultHeader + " " });
          }

          var form = new Ext.FormPanel({ autoScroll: true, bodyStyle: 'padding: 5, 5, 5, 5;', items: columnHeaders });

          var customColumnNamesWindow = new Ext.Window({
            title: Diract.text.setColumnNamesBtnText,
            bodyCfg: { tag: 'center' },
            modal: true,
            items: form,
            layout: 'fit',
            height: 400,
            width: 310,
            bbar: new Ext.Toolbar({
              items: [{
                xtype: 'button',
                text: 'Save',
                self: this,
                binding: form,
                handler: function () {

                  var formData = this.binding.getForm().getFieldValues();

                  Diract.request({
                    url: Diract.route('SetCustomLabels', 'ManagementPage'), // WMS.Routing.Route("Save", "UserComponentState"),
                    method: "POST",
                    scope: this,
                    params: {
                      userID: Diract.user.UserID,
                      name: this.self.stateId,
                      data: Ext.encode(formData)
                    },
                    success: function (result) {
                      var formData = this.binding.getForm().getFieldValues();
                      //apply column names
                      var columnModel = this.self.colModel;

                      for (var i = 0; i < columnModel.getColumnCount() ; i++) {
                        var column = columnModel.getColumnById(i);
                        if (column != undefined)
                          for (var key in formData) {
                            if (key == column.defaultHeader) {
                              if (formData[key] != "") {
                                columnModel.setColumnHeader(i, formData[key]);
                              }
                            }
                          }
                      }
                      this.binding.parent.destroy();
                      Concentrator.stores.mangementLabels.reload();
                    } // eo success
                  });
                }
              }]
            })
          });
          customColumnNamesWindow.show();
          form.parent = customColumnNamesWindow;
        } // eo handler
      });

    };

    var createResetStateButton = function () {

      // reset grid view button
      return new Ext.Toolbar.Button({
        text: Diract.text.resetViewBtnText,
        tooltip: Diract.text.resetViewBtnTooltip,
        iconCls: "state_reset",
        scope: this,
        handler: function () {
          // tell the statemanager to clear it
          Ext.state.Manager.clear(this.stateId);
          // copy column model config (deep)
          var cmcfg = this.colModel.config, newcfg = [];
          for (var i = 0; i < cmcfg.length; i++) {
            var ccfg = this.initialColumnConfig[cmcfg[i].dataIndex];
            //Apply default header
            ccfg.header = cmcfg[i].defaultHeader;
            newcfg[ccfg.position] = Ext.apply({}, ccfg, cmcfg[i]);
          }
          // apply new config and refresh view
          this.colModel.config = newcfg;
          this.view.refresh(true);
          //Delete custom config 
          this.deleteConcentratorState();
        } // eo handler
      });
    };
    var e = function () {

      // button to export all grid's data to MS Office Excel
      return new Ext.Toolbar.Button({
        text: Diract.text.excelBtnText,
        iconCls: "exportExcelTemplate",
        tooltip: Diract.text.excelBtnTooltip,
        scope: this,
        handler: function () {
          // get column model
          var data = this.getState();
          // save this column model
          Diract.silent_request({
            url: this.saveComponentStateUrl, // WMS.Routing.Route("Save", "UserComponentState"),
            method: "POST",
            scope: this,
            params: {
              userID: Diract.user.UserID,
              componentID: this.stateId,
              data: Ext.encode(data)
            },
            success: function (result) {
              // prefix url with exportgrid controller
              var exportUrl = this.exportComponentToExcelUrl, // WMS.Routing.Route("ExportGrid", "ExportGrid");
              // set lastURL and name in there, too
            exportUrl = Ext.urlAppend(exportUrl, this.store.lastUrl);
              exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode({ originalName: this.pluralObjectName }));
              // append grid's stateId
              exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode({ stateId: this.stateId }));
              // ...and put filter settings in there, too
              if (this.gridFilters) {
                exportUrl = Ext.urlAppend(exportUrl, Ext.urlEncode(this.gridFilters.buildQuery(this.gridFilters.getFilterData())));
              }
              // the actual redirAct
              window.location = exportUrl;
            } // eo success
          }); // eo Request
        } // eo handler
      });
    };


    // constructor
    var constructor = function (config) {
      // base apply
      Ext.apply(this, config);
      var p = config.permissions;

      if (p.all) {
        this.allowCreate = this.allowEdit = this.allowDelete = this.allowList = Diract.user.isInRole(p.all);
      } else {
        this.allowCreate = Diract.user.isInRole(p.create);
        this.allowEdit = Diract.user.isInRole(p.update);
        this.allowDelete = Diract.user.isInRole(p.remove);
        this.allowList = Diract.user.isInRole(p.list);
      }

      // save the stateID
      if (!this.stateId) this.stateId = "grid" + this.url, "/" + (Ext.isArray(this.primaryKey) ? this.primaryKey.join('_') : this.primaryKey);

      // allow specifying plugins as single object, or multiple in an array
      if (!this.plugins) this.plugins = [];
      else if (!Ext.isArray(this.plugins)) this.plugins = [this.plugins];


      // if structure is supplied, generate sets and filters
      if (this.structure.length > 0) {
        this.dataSet = []; this.columnsSet = []; this.filters = []; this.creationFields = [];
        Ext.each(this.structure, function (struct) {
          var editor = struct.editor;

          if (Ext.isArray(editor))
            editor = editor[0];


          // note: ---> if (Concentrator.renderers.field('mangementLabels', 'Name')(struct.dataIndex) != null && Concentrator.renderers.field('mangementLabels', 'Name')(struct.dataIndex) == "")
          //if above line is true but you still want to display the data set, then define forceShowForRenderer = true in your dataIndex of the grid
          var forceShowForRenderer = false;
          if (struct.forceShowForRenderer)
            forceShowForRenderer = struct.forceShowForRenderer;



          if (struct.header) {

            var temp = {
              editor: editor,
              renderer: struct.renderer,
              sortBy: struct.sortBy,
              header: Concentrator.renderers.field('mangementLabels', 'Name', 'Grid')(struct.header, this.stateId) || struct.header,
              defaultHeader: struct.header,
              css: struct.css,
              cls: struct.cls,
              filter: struct.filter,
              dataIndex: struct.dataIndex,
              width: struct.width,
              fixed: struct.fixed,
              excelDataIndex: struct.excelDataIndex,
              preRenderer: struct.preRenderer,
              defaultValue: struct.defaultValue,
              locked: struct.locked,
              colType: struct.type,
              filterable: struct.filterable,
              validateFn: struct.validateFn,
              hideable: struct.hideable,
              isAdditional: struct.isAdditional,
              hidden: (struct.hidden == true) || (Concentrator.renderers.field('mangementLabels', 'Name')(struct.dataIndex) != null &&
                                                 Concentrator.renderers.field('mangementLabels', 'Name')(struct.dataIndex) == "" && !forceShowForRenderer) ? true : false,
              sortable: (struct.sortable !== false),
              editable: (struct.editable == true || Boolean(editor)) && struct.editable !== false
            };

            if (struct.type === "boolean") {
              if (struct.isRadio) {
                var radioCol = new Ext.grid.RadioColumn({
                  header: temp.header,
                  defaultHeader: temp.defaultHeader,
                  dataIndex: temp.dataIndex,
                  width: temp.width,
                  fixed: temp.width,
                  filterable: false,
                  colType: temp.colType,
                  readonly: !temp.editable || !this.allowEdit
                });
                this.columnsSet.push(radioCol);
                this.plugins.push(radioCol);
              } else {
                var checkCol = new Ext.grid.CheckColumn({
                  header: temp.header,
                  defaultHeader: temp.defaultHeader,
                  dataIndex: temp.dataIndex,
                  width: temp.width,
                  fixed: temp.width,
                  filterable: struct.filterable,
                  colType: temp.colType,
                  readonly: !temp.editable || !this.allowEdit
                });
                this.columnsSet.push(checkCol);
                this.plugins.push(checkCol);
              }

            } else if (struct.type == "string" && struct.isColor) {
              var colorCol = new Ext.grid.ColorColumn({
                header: temp.header,
                defaultHeader: temp.defaultHeader,
                dataIndex: temp.dataIndex,
                width: temp.width,
                fixed: temp.width,
                filterable: false,
                locked: temp.locked,
                validateFn: temp.validateFn,
                colType: temp.colType,
                defaultValue: temp.defaultValue,
                preRenderer: temp.preRenderer,
                readonly: !temp.editable || !this.allowEdit
              });
              this.columnsSet.push(colorCol);
              this.plugins.push(colorCol);
            } else {
              if (!this.allowEdit) {
                delete temp.editor;
              }

              this.columnsSet.push(temp);
            }
          }
          // push to dataset
          this.dataSet.push({
            name: struct.dataIndex,
            type: struct.type == "boolean" ? "auto" : struct.type
          });

          if (struct.pluginType) {
            //construct plugin
            var dataIndex = struct.dataIndex;
            delete struct.dataIndex; //remove to prevent errors in grid for duplicate dataIndexes
            if (dataIndex && !struct.tpl) {
              //default template
              var defaultTemplate = new Ext.XTemplate(
               '<tpl for="' + dataIndex + '">',
                    '<p>{.}</p>',
                '</tpl>'
                  );
              struct.tpl = defaultTemplate;
            }
            var col = Ext.create(struct, struct.pluginType); //create object from xtype

            this.columnsSet.push(col); //add plugin column
            this.plugins.push(col); //add plugin 
          }
          // push to filters
          if (struct.filterable !== false && struct.header && !this.useHeaderFilters) {
            if (!struct.filter) {
              this.filters.push({
                dataIndex: struct.dataIndex,
                type: struct.type || "string"
              });
            } else {
              struct.filter.dataIndex = struct.dataIndex;
              this.filters.push(struct.filter);
            }
          }

          // Defined an editor and didn't set 'creationField' to false,
          // meaning it will be used as a field in the new object window only if 'fields' is undefined.
          // If you use this, the editor must be specified as an object with an xtype, not be initialized with a constructor
          if (struct.creationField !== false && struct.editor || (struct.type === "boolean" && struct.editable)) {
            var editors = new Array();

            if (!Ext.isArray(struct.editor)) {
              editors.push(struct.editor);
            } else { editors = struct.editor; }

            Ext.each(editors, function (ed) {
              var newField;
              if (struct.type === "boolean") {
                newField = {
                  xtype: 'checkbox',
                  name: struct.dataIndex,
                  fieldLabel: struct.header
                };
              } else {
                newField = ed;
                newField.fieldLabel = ed.fieldLabel || struct.header;
                newField.name = ed.name || struct.dataIndex;
              }


              this.creationFields.push(newField);
            }, this);



          }
        }, this);
        if (this.useHeaderFilters) {
          this.gridHeaderFilters = new Ext.ux.grid.GridHeaderFilters({
            highlightOnFilter: false
          });
          this.plugins.push(this.gridHeaderFilters);
        }
        //apply actions
        if (this.actions) {
          var width = this.actions.length * 21;
          var rowActionCol = new Ext.grid.ActionColumn({
            width: width,
            menuDisabled: true,
            hideable: false,
            fixed: true,
            items: this.actions
          });
          this.columnsSet.push(rowActionCol);
        }

        if (!this.fields) {
          this.fields = function () {
            var newFields = [];

            Ext.each(this.creationFields, function (field) {
              newFields.push(Ext.apply({}, field));
            });

            if (this.mandatoryFields) {
              Ext.each(this.mandatoryFields, function (field) {
                newFields.push(Ext.apply({}, field));
              });
            }
            return newFields;
          };
        }
      } // eo if structure

      delete this.structure;

      // save original columns config
      this.initialColumnConfig = [];
      for (var i = 0; i < this.columnsSet.length; i++) {
        this.initialColumnConfig[this.columnsSet[i].dataIndex] = {
          position: i,
          width: this.columnsSet[i].width,
          hidden: this.columnsSet[i].hidden
        };
      }

      // initialise editors when using the inline adding feature
      if (this.inlineNewUrl) {
        for (var col in this.columnsSet) {
          if (!isNaN(+col)) {
            if (this.columnsSet[col].editable === false) {
              this.columnsSet[col].editable = undefined;
            }
            if (this.columnsSet[col].editor === undefined) {
              for (var val in this.dataSet) if (this.dataSet[val].name == this.columnsSet[col].dataIndex)
                switch (this.dataSet[val].type) {
                  case "numeric":
                  case "int":
                    this.columnsSet[col].editor = new Ext.form.NumberField({ allowBlank: false });
                    break;
                  case "date":
                    this.columnsSet[col].editor = new Ext.form.DateField({ allowBlank: false });
                    break;
                  case "boolean":
                  case "bool":
                    this.columnsSet[col].editor = new Ext.form.Checkbox();
                    break;
                  default:
                    this.columnsSet[col].editor = new Ext.form.TextField({ allowBlank: false });
                } // eo switch
            } else this.columnsSet[col].editable = true;
          }
        }
      }

      // new config
      this.formConfig = Ext.apply({
        bodyStyle: "padding: 10px;",
        defaults: {
          anchor: "90%"
        },
        labelWidth: 120,
        border: false,
        monitorValid: true
      }, this.formConfig);

      // edit window config
      this.windowConfig = Ext.apply({
        width: 400, minWidth: 400,
        height: 275, minHeight: 275,
        layout: "fit",
        buttonAlign: "right",
        modal: true
      }, this.windowConfig);

      // min checks
      this.windowConfig.minWidth = Math.min(this.windowConfig.minWidth, this.windowConfig.width);
      this.windowConfig.minHeight = Math.min(this.windowConfig.minHeight, this.windowConfig.height);

      // initialise listeners
      if (!this.listeners) this.listeners = {};

      // listen for dblclick if we need to edit in a new window
      if (this.editInNewWindow) {
        this.listeners.rowdblclick = function (grid, index, e) {
          var record = this.store.getAt(index);
          if (!this.editPredicate || this.editPredicate(record)) this.showNewWindow(record);
          else Ext.MessageBox.alert(Diract.text.errorTitle, Diract.text.editNotAllowedMessage);
        };
      }

      // prevent editors from showing up if not allowed/desired
      this.listeners.beforeedit = function (e) {
        //For search box control binding
        var editor = e.grid.colModel.getCellEditor(e.column, e.row);
        editor.field.updateObject = e;
        return !this.editInNewWindow && (!this.editPredicate || this.editPredicate(e.record));
      };

      if (!this.overrideAfterrender) {//with this.overrideAfterrender you can override the afterrender event in your grid
        this.listeners.afterrender = (function (obj, data) {
          this.addListener('sortchange', function (obj, sortData) {
            this.sortData = sortData;
            this.saveConcentratorState();
          });

        }).createDelegate(this);
      }


      /**
      * Set up the store.
      */

      // define the reader to map the proxy return to a data type
      this.emptyRecord = Ext.data.Record.create(this.dataSet);
      this.reader = new Ext.data.JsonReader(Ext.apply({
        id: this.primaryKey,
        root: this.root || "results",
        totalProperty: "total"
      }, this.readerConfig), this.emptyRecord);

      // define the proxy to get our data
      this.proxy = new Ext.data.HttpProxy(Ext.apply({
        url: this.url,
        method: this.method
      }, this.proxyConfig));

      // define the default sorting info, only applies to primary column

      this.sortInfo = Ext.apply({
        field: this.primaryValueField || this.sortBy || this.primaryKey,
        direction: (this.sortAscending == undefined) ? "ASC" : (this.sortAscending ? "ASC" : "DESC")
      }, this.sortInfo);

      // set base params to include limits
      this.params = Ext.apply({
        start: 0,
        limit: this.pageSize
      }, this.params);

      // create the actual store
      if (!this.store) {
        this.store = new Ext.data.GroupingStore(Ext.apply({
          proxy: this.proxy,
          reader: this.reader,
          autoLoad: this.autoLoadStore,
          sortInfo: this.sortInfo,
          groupField: this.groupField,
          pageSize: this.pageSize,
          remoteSort: true,
          remoteGroup: this.remoteGroup,
          baseParams: this.params,
          listeners: {
            'beforeload': function (store, options) {
              store.baseParams["timestamp"] = new Date().getTime();
            }
          }
        }, this.storeConfig));
      }

      // initialise a var for inline adding
      this.store.numNewLines = 0;

      // when the store updates
      this.store.on("update", function () {
        // disable all buttons
        if (this.saveNewLinesButton) this.saveNewLinesButton.disable();
        if (this.removeNewLinesButton) this.removeNewLinesButton.disable();
        if (this.discardButton) this.discardButton.disable();
        if (this.saveButton) this.saveButton.disable();
        if (this.saveAndExitButton) this.saveAndExitButton.disable()
        // enable those for inline adding when allowed
        if (this.store.numNewLines > 0) {
          this.saveNewLinesButton.enable();
          this.removeNewLinesButton.enable();
        }
        // enable those for editing when allowed
        if (this.store.getModifiedRecords().length > 0) {
          if (this.discardButton) this.discardButton.enable();
          if (this.saveButton) this.saveButton.enable();
          if (this.saveAndExitButton) this.saveAndExitButton.enable();
        }
      }, this);

      // when the store loads
      this.store.on("load", function (store) {
        // reset inline adding var
        this.store.numNewLines = 0;

        if (Ext.isArray(this.primaryKey)) {
          var self = this;

          store.each(function (record) {
            Ext.each(self.primaryKey, function (key) {
              record.data[self.keyPrefix + key] = record.data[key];
            });
          });
        }

      }, this);

      // just before the store loads
      this.store.on("beforeload", function (store, options) {

        //sort override
        var currentDataIndexName = store.sortInfo.field;

        Ext.each(this.columnsSet, (function (col) {
          if (col.dataIndex == currentDataIndexName) {
            if (col.sortBy !== undefined) {
              options.params.sort = col.sortBy;
            };
          }

        }).createDelegate(this));

        // save the lastUrl for export purposes
        var params = {};
        for (var prop in options.params) if (prop.indexOf("filter[") !== 0) params[prop] = options.params[prop];
        store.lastUrl = Ext.urlEncode(Ext.apply({}, { original: store.proxy.url }, params));
        // prevent user from doing things he doesn't want to when there's inline added rows
        if (store.numNewLines > 0) return confirm(Diract.text.discardNewLinesConfirmation);
      }, this);

      /**
      * Set up the column model.
      */

      // insert row number column if required
      if (this.showRowNumbers) this.columnsSet.splice(0, 0, new Ext.grid.RowNumberer());

      // define column model
      if (!this.colModel) {
        if (this.lockColumn) {
          this.colModel = new Ext.ux.grid.LockingColumnModel(this.columnsSet);
        } else {
          this.colModel = new Ext.grid.ColumnModel(this.columnsSet);
        }
      }

      // if we're inline adding, redefine cell edit condition
      if (this.inlineNewUrl) {
        this.colModel.isCellEditable = function (colIndex, rowIndex) {
          return (this.columnsSet[colIndex].editable !== undefined) || this.store.data.items[rowIndex].phantom;
        };
      }

      //apply saved column state
      var saveStateStore = Concentrator.stores.UserSaveStatesStore;
      var saveStateRecordID = saveStateStore.findExact("EntityName", this.stateId);
      //found saveState
      if (saveStateRecordID != -1) {
        var saveStateRecord = saveStateStore.getAt(saveStateRecordID);
        var state = Ext.decode(saveStateRecord.get("SavedState"));
        this.applyState(state);
      }

      this.colModel.on('hiddenchange', (this.saveConcentratorState).createDelegate(this));
      this.colModel.on('columnmoved', (this.saveConcentratorState).createDelegate(this));



      /**
      * Set up the selection model.
      */

      // define selection model config
      this.selModelConfig = Ext.apply({
        singleSelect: false
      }, this.selModelConfig);

      // create the selection model
      if (!this.selModel) this.selModel = new Ext.grid.RowSelectionModel(this.selModelConfig);

      // listen to the selection change event to modify button states
      this.selModel.on("selectionchange", function (model, rowIndex, record) {
        // get the selections from the selection model
        var selections = this.selModel.getSelections();
        // if deleting rows is conditional, check it
        if (this.deletePredicate) {
          if (this.deletePredicate(selections)) this.deleteButton.enable();
          else this.deleteButton.disable();
        }
        // if there are rowAction buttons, check their states
        if (this.rowActions) {
          var rowActions = Ext.apply([], this.rowActions);
          for (var i = 0; i < rowActions.length; i++) {
            var rowAction = rowActions[i];

            if (!!rowAction.button.fireEvent) {
              rowAction.button.fireEvent("selectionchange", selections, rowAction, this);
            }

            // if it's a menu
            if (rowAction.items && Ext.isArray(rowAction.items)) {
              var foundOne = false;
              // process all menu items
              for (var j = 0; j < rowAction.items.length; j++) {
                if (rowAction.items[j].alwaysEnabled || this.getButtonState(selections, rowAction.items[j])) {
                  foundOne = true;
                  rowAction.items[j].button.enable();
                } else rowAction.items[j].button.disable();
              }
              // if none of the subitems is enabled, disable this button as well
              if (foundOne) rowAction.menuButton.enable(); else rowAction.menuButton.disable();
              // else, if it's a normal button
            } else {
              // toggle status
              if (rowAction.alwaysEnabled || this.getButtonState(selections, rowAction)) rowAction.button.enable();
              else rowAction.button.disable();
            }
          } // eo for i
        } // eo if this.rowActions
      }, this);

      /**
      * Define grid filters.
      */

      // apply the config
      this.filterConfig = Ext.apply({
        filters: this.filters
      }, this.filterConfig);
      // if filters was specified, define GridFilters
      if (this.filters) {
        this.gridFilters = new Ext.ux.grid.GridFilters(this.filterConfig);
        // and don't forget to add it to the grid's plugins!
        this.plugins.push(this.gridFilters);
      }

      /**
      * Create the grid view.
      */

      // allow grouping using config
      if (!this.view) {
        if (this.groupField) {
          this.view = new Ext.grid.GroupingView({
            forceFit: this.forceFit,
            startCollapsed: this.startCollapsed,
            groupTextTpl: this.groupTextTpl,
            hideGroupedColumn: this.hideGroupedColumn
          });
        } else if (this.lockColumn) {
          this.view = new Ext.ux.grid.LockingGridView();
        } else {
          this.view = new Ext.grid.GridView({
            forceFit: this.forceFit
          });
        }
      }

      // allow additional classes for certain rows
      if (this.rowFormat) {
        this.view.getRowClass = this.rowFormat.createDelegate();
      }

      /**
      * Define buttons.
      */


      /**
      * Default add/edit/delete buttons.
      */

      /**
      * Set up toolbar.
      */

      // only if the user isn't persistent on his own tbar
      if (!this.tbar) {

        // let's start empty
        this.toolbarButtons = [];

        // push buttons into toolbar when approperiate
        if (this.inlineNewUrl) {
          this.addNewLineButton = createAddNewLineButton.createDelegate(this)();
          this.saveNewLinesButton = createSaveNewLinesButton.createDelegate(this)();
          this.removeNewLinesButton = createRemoveNewLinesButton.createDelegate(this)();
          this.toolbarButtons.push(this.addNewLineButton, this.saveNewLinesButton, this.removeNewLinesButton);
        }
        if (this.allowCreate && this.newUrl) {
          this.addNewButton = createAddButton.createDelegate(this)();
          this.toolbarButtons.push(this.addNewButton);
        }
        if ((this.allowEdit && this.updateUrl) && (!this.editInNewWindow)) {
          this.saveButton = createSaveButton.createDelegate(this)();
          if (this.saveAndExit) this.saveAndExitButton = createSaveAndExitButton.createDelegate(this)();
          this.discardButton = createDiscardButton.createDelegate(this)();
          this.toolbarButtons.push(this.saveButton);
          if (this.saveAndExit) this.toolbarButtons.push(this.saveAndExitButton);
          this.toolbarButtons.push(this.discardButton);
        }
        if (this.allowDelete && this.deleteUrl) {
          this.deleteButton = createDeleteButton.createDelegate(this)();
          this.toolbarButtons.push(this.deleteButton);
        }

        // if there are custom buttons supplied, add them as well
        if (this.customButtons) {
          for (var i = 0; i < this.customButtons.length; i++) {
            var btn = this.customButtons[i];

            if (btn.alignRight) {
              this.toolbarButtons.push('->');
            }

            var hasRights = true;
            if (btn.roles) {
              hasRights = false;
              var roles = btn.roles;

              if (typeof roles == 'string') roles = [roles];

              for (var j = 0; j < roles.length; j++) {
                if (Diract.user.isInRole(roles[j]))
                  hasRights = true;
              }

            }
            if (hasRights) {
              this.toolbarButtons.push(btn);
            }
          }
          //this.toolbarButtons = this.toolbarButtons.concat(this.customButtons);
        }

        // if there are rowactions specified
        if (this.rowActions) {
          var authorizedActions = [];
          for (var i = 0; i < this.rowActions.length; i++) {
            var ac = this.rowActions[i];
            var hasRights = true;
            if (ac.roles) {
              hasRights = false;
              for (var j = 0; j < ac.roles.length; j++) {
                if (Diract.user.isInRole(ac.roles[j]))
                  hasRights = true;
              }

            }
            if (hasRights) {
              authorizedActions.push(ac);
            }

          }
          this.rowActions = authorizedActions;

          // all next buttons will be aligned right
          this.toolbarButtons.push('->');
          // for each rowaction button
          for (var i = 0; i < this.rowActions.length; i++) {
            var rowAction = this.rowActions[i];
            // if the rowAction has items, make it a button with a dropdown
            if (rowAction.items && Ext.isArray(rowAction.items)) {
              // create the subitems
              var items = [], foundOne = false;
              for (var j = 0; j < rowAction.items.length; j++) {
                rowAction.items[j].button = this.generateToolbarButton(rowAction.items[j], true);
                if (rowAction.items[j].button) {
                  if (rowAction.items[j].alwaysEnabled) foundOne = true;
                  items.push(rowAction.items[j].button);
                }
              } // eo for j
              // and the menu button itself
              rowAction.menuButton = (items.length > 0) ? new Ext.Toolbar.Button({
                text: rowAction.text,
                iconCls: rowAction.iconCls,
                disabled: !(foundOne || rowAction.alwaysEnabled),
                menu: new Ext.menu.Menu({
                  items: items
                })
              }) : null;
              if (rowAction.menuButton) {
                if (this.toolbarButtons.length > 1) this.toolbarButtons.push("-");
                this.toolbarButtons.push(rowAction.menuButton);
              }
              // else, make a normal button
            } else {
              rowAction.button = this.generateToolbarButton(rowAction);
              if (rowAction.button) {
                if (this.toolbarButtons.length > 1) this.toolbarButtons.push("-");
                this.toolbarButtons.push(rowAction.button);
              }
            }
          } // eo for
          // cleanup unauthorized actions
          var clean = false; while (!clean) {
            clean = true;
            for (var i = 0; i < this.rowActions.length; i++) {
              if (this.rowActions[i].items) for (var j = 0; j < this.rowActions[i].items.length; j++) {
                if (!this.rowActions[i].items[j].button) {
                  this.rowActions[i].items.splice(j, 1);
                  clean = false; break;
                }
              }
              if (!clean) {
                break;
              }
              if (!(this.rowActions[i].button || this.rowActions[i].menuButton)) {
                this.rowActions.splice(i, 1);
                clean = false; break;
              }
            }
          }
        } // eo if rowActions

        // create the actual toolbar with these buttons, if there are any
        if (this.toolbarButtons.length > 0) this.tbar = new Ext.Toolbar(this.toolbarButtons);

      } // eo if !this.tbar

      /**
      * And the bottom toolbar as well.
      */

      // only if the user hasn't specified his own bbar or wants to hide it
      if (!this.bbar && !this.hideBottomBar) {
        // define paging tool
        this.bbarPagingTool = {
          init: function (p) {
            this.p = p;
          },
          setPageSize: function (pageSize) {
            this.p.pageSize = pageSize;
          }
        };
        // define configuration

        this.clearFilterButton = createClearFilterButton.createDelegate(this)();
        this.resetStateButton = createResetStateButton.createDelegate(this)();
        this.setColumnNamesButton = createSetColumnNamesButton.createDelegate(this)();

        var bbarItems = [this.clearFilterButton, "-", this.resetStateButton, "-", this.setColumnNamesButton, "-"];
        if (this.bbarCustomButtons) {
          bbarItems.push('-');
          Ext.each(this.bbarCustomButtons, function (btn) {
            bbarItems.push(btn);
          });
        }
        if (!this.disableExport && !Diract.GRID_EXCEL_EXPORT_DISABLED) {
          bbarItems.push(this.exportToExcelButton);
        }
        this.bbarConfig = Ext.apply({
          pageSize: this.pageSize,
          store: this.store,
          displayInfo: true,
          displayMsg: this.pluralObjectName + Diract.text.pagingBarSummary,
          emptyMsg: String.format(Diract.text.pagingBarEmpty, this.pluralObjectName),
          items: bbarItems,
          plugins: [this.bbarPagingTool, this.gridFilters]
        }, this.bbarConfig);
        // and create a new bar
        this.bbar = new Ext.PagingToolbar(this.bbarConfig);
      } // eo if

      /**
      * Set up the actual grid.
      */

      // core to this element; the actual grid constructing
      Diract.ui.Grid.superclass.constructor.call(this);
    };

    return constructor;

  })(); // eo constructor


  Ext.extend(Diract.ui.Grid, Ext.grid.EditorGridPanel, {

    /**
    * Required configs.
    */

    /**
    * @cfg {String} primaryKey The primary key to use for the store.
    */
    primaryKey: undefined,

    /**
    * @cfg {String[]} otherKeys Other (primary) keys.
    */
    otherKeys: undefined,

    /**
    * @cfg {String} singularObjectName The name to give a single row.
    */
    singularObjectName: "Record",

    /**
    * @cfg {String} pluralObjectName The name to give multiple rows.
    */
    pluralObjectName: "Records",

    /**
    * @cfg {String} url The URL to retrieve data from.
    *
    * By default, the {Reader} will assume a Json {Object} return that contains {Number} "totals" and {Record[]} "results".
    * These default settings can be overridden using the readerConfig configuration.
    *
    * The method to use (POST or GET) for this request is defined in the method configuration.
    *
    * Shorthand for specifying proxyConfig's url.
    */
    url: undefined,

    /**
    * @cfg {String} method The method to use to retrieve data from the url. Defaults to GET.
    *
    * Note that all params (filters and such) are appended to the URL. This may cause the URL to exceed the maximum URL length,
    * which in Internet Explorer is 2083. If this may occur, using POST method is recommended. Elsewise, use GET.
    *
    * Shorthand for specifying proxyConfig's method.
    */
    method: "GET",

    /**
    * @cfg {String} newUrl The URL to submit new data to (POST).
    */
    newUrl: undefined,

    /**
    * @cfg {Object} params The baseParams to apply to the store before loading initially.
    */
    params: undefined,

    /**
    * @cfg {String} updateUrl The URL to submit changes to (POST).
    *
    * Calls to this URL will alter a record, NOT retrieve data.
    */
    updateUrl: undefined,

    /**
    * @cfg {String} editUrl The URL to retrieve edit data from (GET).
    *
    * Calls to this URL will get specific record data, NOT alter it.
    */
    editUrl: undefined,

    /**
    * @cfg {String} deleteUrl The URL to submit delete requests to (POST).
    */
    deleteUrl: undefined,

    /**
    * @cfg {Object[]} structure Definition of the grid's data structure.
    *
    * Combines the dataSet, filters and columnSet properties.
    *      structure: [
    *          // normal display, no filter
    *          { dataIndex: 'ProductID', type: 'int', header: 'Product #' },
    *          // not displayed but listed in store
    *          { dataIndex: 'CompanyID', type: 'int' },
    *          // normal display, filterable
    *          { dataIndex: 'WarehouseID', type: 'int', header: 'Warehouse', filterable: true }
    *      ]
    */
    structure: [],

    /**
    * @cfg {Object[]} dataSet Definition of the data set.
    *
    * Recommended to set through the structure property.
    *
    * Each object in this array must define a {String} "type" and a {String} "name".
    */
    dataSet: [],

    /**
    * @cfg {Object[]} columnsSet Definition of the columns set.
    *
    * Recommended to set through the structure property.
    *
    * Each object in this array must define a {String} "dataIndex" and a {String} "header". The dataIndex property
    * of an object obj1 in this array must refer to an object obj2 in the this.dataSet array where obj1.dataIndex == obj2.name.
    */
    columnsSet: [],

    /**
    * @cfg {Object[]} filters Use this to specify the grid filters.
    *
    * Each object in this array must define a {String} "dataIndex" and a {String} "type". The dataIndex property
    * of an object obj1 in this array must refer to an object obj2 in the this.dataSet array where obj1.dataIndex == obj2.name.
    */
    filters: undefined,

    /**
    * @cfg {Function} fields Function that returns {Field[]} fields to be used for the popup window (edit & new).
    *
    * This function is called upon every construction of the popup window. When editing, you can define the editLoadSucces function
    * which will be called after loading data into the poup window, where you can post-process these fields.
    */
    fields: undefined,

    /**
    * Features!
    */

    /**
    * @cfg {String} inlineNewUrl The URL to submit inline added line data to (POST).
    *
    * Specifying this will allow the user to add empty rows to the grid. These empty lines the user then
    * can edit (default editors are generated) and submit to the server. It will be POSTed to this URL.
    */
    inlineNewUrl: undefined,

    /**
    * @cfg {Function} callback Function called from the grid for data change actions.
    *
    * This function will be called upon user actions.
    *
    * @param {Store store The store that generated this event.
    * @param {Number} num The number of amount of records affected.
    * @param {String} action The action performed. Can be "update", "delete" or "new".
    */
    callback: undefined,

    /**
    * @cfg {Function} editLoadSuccess Function called when the popup window loads.
    *
    * This function will be called whenever the new popup window loads it's data. This loading will only
    * occur when a specific record is being edited in a new window. This can only happen if this.editUrl
    * is specified, and this is also the URL from which the load event triggers.
    *
    * @param {BasicForm} form The form that was loaded.
    * @param {Action} action The action params from the original load event.
    */
    editLoadSuccess: undefined,

    /**
    * @cfg {Function} deletePredicate Function to allow specifying if the selection may be deleted or not.
    *
    * This function will give you more control on which records in the grid are allowed to be removed.
    *
    * @param {Record[]} records The array of selected records.
    * @return True to enable the "Delete"-button, and false to disable it.
    */
    deletePredicate: undefined,

    /**
    * @cfg {Function} editPredicate Function to allow specifying if the selection may be edited or not.
    *
    * This function will give you more control on which records in the grid are allowed to be edited.
    *
    * @param {Record} record The selected record.
    * @return True to allow editing, and false to disallow it.
    */
    editPredicate: undefined,

    /**
    * @cfg {Bool} editInNewWindow Specify whether the user will be able to edit rows inline or in a new popup window.
    *
    * Set this to True to have a doubleclick on a row open up a new popup window. The fields used in this window are
    * defined in this.fields and they are loaded with data from this.editUrl.
    */
    editInNewWindow: false,

    /**
    * @cfg {Button[]} customButtons An array to add custom buttons to the main toolbar.
    */
    customButtons: undefined,

    /**
    * @cfg {Object[]} rowActions An array to add rowaction buttons to the main toolbar.
    *
    * A rowAction {Object} must contain the following properties:
    * - {String} text The text to be used on the Button.
    * - {Function} handler Function to be performed upon clicking the button.
    *                      Will be called with a {Record} record parameter.
    *
    * And it may contain the following properties:
    * - {Sting} iconCls The class to be applied to the Button.
    * - {Function} conditionalText The Function to return a {String} to be used as text on the Button.
    Will be called with a {Record} record parameter.
    * - {Function} conditionalIconCls The Function to return a class {String} to be applied to the Button.
    Will be called with a {Record} record parameter.
    * - {Function} predicate The Function to return whether or not a button should be enabled.
    Will be called with a {Record} record parameter. Return false to disable the Button, true to enable it.
    * - {Number[]} roles An array of Roles to specify which user groups can use this Button.
    * - {Function} sharedCondition The Function to return whether or not a button should be enabled.
    Will be called with a {Record[]} records parameter. Return false to disable the Button, true to enable it.
    */
    rowActions: undefined,

    /**
    * @cfg {Object[]/Object} plugins Specify grid plugins here.
    *
    * Grid plugins are required for certain addons to work, for example a editable CheckColumn.
    */
    plugins: undefined,

    /**
    * @cfg {Function} rowFormat Function to be called to add classes to rows. Defaults to undefined.
    *
    * This is a shorthand for overriding the grid's view's getRowClass.
    */
    rowFormat: undefined,

    /**
    * Default stuff.
    */

    /**
    * @cfg {Number} pageSize Default grid's page size. Defaults to Diract.GRID_PAGE_SIZE.
    */
    pageSize: Diract.GRID_PAGE_SIZE,

    /**
    * @cfg {Bool} forceFit Force columns to fit in the grid's width. Defaults to True.
    *
    * This is a shorthand for specifying it in the view config.
    */
    forceFit: true,

    /**
    * @cfg {Bool/Object} loadMask An Ext.LoadMask config or true to mask the grid while loading. Defaults to True.
    */
    loadMask: true,

    /**
    * @cfg {Bool} showRowNumbers True to show row numbers in the grid. Defaults to false.
    */
    showRowNumbers: false,

    /**
    * @cfg {String} groupTextTpl The template HTML used for group headings.
    */
    groupTextTpl: "{text} - ({[values.rs.length]} {[values.rs.length > 1 ? 'Items' : 'Item']})",

    /**
    * @cfg {Bool} autoLoadStore Automatically load store on render. Defaults to True.
    *
    * Setting this to False will prevent the Store from loading when the Grid rendered.
    */
    autoLoadStore: true,

    /**
    * @cfg {Bool} disableExport Allow disabling the feature to export this grid to MS Office Excel. Defaults to False.
    *
    * Set this to true to prevent the user from being able to export this grid to MS Office Excel. You may want to prevent this
    * for example when the Grid uses renderers to display it's main contents.
    */
    disableExport: false,

    /**
    * @cfg {Bool} hideBottomBar Hide the bottom toolbar. Defaults to False.
    */
    hideBottomBar: false,

    /**
    * @cfg {Object} listeners Listeners for grid events.
    *
    * Do not specify a "rowdblclick" event when editInNewWindow = True, it will be overridden.
    * The "afteredit" and "beforeedit" are overridden at all times.
    */
    listeners: undefined,

    /**
    * @cfg {Number} clicksToEdit The number of clicks to edit grid cells. Defaults to 1.
    */
    clicksToEdit: 2,

    /**
    * Child configs.
    */

    /**
    * @cfg {Object} readerConfig The config to apply to the grid store reader.
    */
    readerConfig: {},

    /**
    * @cfg {Object} proxyConfig The config to apply to the http proxy for the store.
    */
    proxyConfig: {},

    /**
    * @cfg {Object} sortInfo The config to apply for default sorting.
    */
    sortInfo: {},

    /**
    * @cfg {Object} storeConfig The config to apply to the grid store.
    */
    storeConfig: {},

    /**
    * @cfg {Object} bbarConfig The config to apply to the bottom toolbar.
    */
    bbarConfig: {},

    /**
    * @cfg {Object} filterConfig The config to apply to the grid's filters.
    */
    filterConfig: {},

    /**
    * @cfg {Object} windowConfig The config to apply to the object popup window (new & edit).
    */
    windowConfig: {},

    /**
    * @cfg {Object} formConfig The config to apply to the object popup window form (new & edit).
    */
    formConfig: {},

    /**
    * @cfg {Object} selModelConfig The config to apply to the grid's selection model.
    */
    selModelConfig: {},

    // Gets prefixed to your primary keys in a post to delete/update in case you have multiple primary keys
    keyPrefix: "_",

    /**
    * Functions.
    */

    clearFilters: function (suppressReload) {
      if (this.gridFilters) this.gridFilters.clearFilters();
      if (this.gridHeaderFilters) this.resetHeaderFilters();

      //for additional filters not specified within the column filters
      if (this.customFilterCollection && this.customFilterCollection.length > 0) {

        Ext.each(this.customFilterCollection, function (filter) {
          delete this.store.baseParams[filter.key];
        }, this);

        this.customFilterCollection = [];
        if (!suppressReload)
          this.store.reload();
      }
    },

    /**
    * Apply specified state to the column model.
    *
    * @param {Object} state The state to apply to this grid.
    * @return void
    */
    applyState: function (state) {
      // initialise
      if (!this.disableSaveState) {
        var columns = [], columnState = state["columns"], columnIndex, filters = state["filters"], sortData = state["sortData"];
        // for every column from the original column model
        for (var i = 0; i < this.columnsSet.length; i++) {
          // if a new state was specified
          if (columnState[i]) {
            // find the column and stop if we can't find it
            columnIndex = this.colModel.findColumnIndex(columnState[i].dataIndex);
            if (columnIndex < 0) return; // applyState()
            // apply the state and save this columns config
            delete columnState[i].header;
            if (columnIndex != columnState[i].columnIndex) {
              this.colModel.moveColumn(columnIndex, columnState[i].columnIndex);
            }
            columnState[i] = Ext.apply(this.columnsSet[columnState[i].columnIndex], columnState[i]);
            columns[i] = columnState[i];
          } else return; // applyState()
        } // eo for i
        // if we've not returned by now, apply new column config
        this.colModel.setConfig(columns, true);
        //apply filters
        //			if (filters) {
        //				if (this.listeners)
        //					this.listeners.afterrender = function () { this.gridFilters.applyState(this, { filters: filters }); };
        //			}
        //apply sort column
        if (sortData) {
          this.store.setDefaultSort(sortData["field"], sortData["direction"]);
        }
      }
    },
    deleteConcentratorState: function () {
      //Delete custom state
      Diract.silent_request({
        url: this.deleteComponentStateUrl,
        method: "POST",
        scope: this,
        params: {
          userID: Diract.user.UserID,
          name: this.stateId
        },
        success: function (result) {
          Concentrator.stores.UserSaveStatesStore.reload();
          Concentrator.stores.mangementLabels.reload();
        } // eo success
      });
    },
    saveConcentratorState: function () {
      if (!this.disableSaveState) {
        var task = new Ext.util.DelayedTask(function () {
          // get column model
          var data = this.getState();
          // save this column model
          Diract.silent_request({
            url: this.saveComponentStateUrl, // WMS.Routing.Route("Save", "UserComponentState"),
            method: "POST",
            scope: this,
            params: {
              userID: Diract.user.UserID,
              name: this.stateId,
              data: Ext.encode(data)
            },
            success: function (result) {
              Concentrator.stores.UserSaveStatesStore.reload();
            } // eo success
          });
        }, this);
        task.delay(1000);
      }
    },
    /**
    * Get the state from the specified grid.
    *
    * @return {Object} The grid's state.
    */
    getState: function () {
      // initialise
      var data = [], cm = this.colModel;
      var filters = this.gridFilters.getFilters();
      var sortData = this.sortData;
      // for every column
      var indexCount = 0;
      cm.getColumnsBy(function (c) {
        // specifically exclude check columns!
        if (c.id == "checker") return false;
        indexCount++;
        // return value will be false
        // fill the data array with config options
        return !(data[cm.getIndexById(c.id)] = {
          columnIndex: indexCount - 1,
          width: c.width,
          header: c.header,
          excelDataIndex: c.excelDataIndex,
          defaultHeader: c.defaultHeader,
          hidden: c.hidden === true,
          dataIndex: c.dataIndex,
          isAdditional: c.isAdditional
        });
      });
      // return the config options array
      return { columns: data, filters: filters, sortData: sortData };
    },

    /**
    * Function to add toolbar buttons, primarily used for rowAction button adding.
    *
    * @param {Object} item The item to add to the toolbar. This is a RowAction Object.
    * @param {Bool} menuItem Is it a menu item or not.
    * @return {Ext.menu.Item/Ext.Toolbar.Button} The newly generated button.
    */
    generateToolbarButton: function (item, isMenuItem) {
      // if the user is allowed to use this button
      if (!item.functionalities || Diract.user.hasFunctionality(item.functionalities)) {
        // set a default predicate
        if (!item.predicate) {
          item.predicate = function () {
            return true;
          };
        }

        var config = {
          text: item.text,
          iconCls: item.iconCls ? item.iconCls : "",
          scope: this,
          handler: function () {
            var selections = this.selModel.getSelections();

            if (selections.length == 1) {
              item.handler(item.allowMultipleSelect ? selections : selections[0]);
            }
            else {
              item.handler(selections);
            }
          },
          disabled: !item.alwaysEnabled,
          listeners: item.listeners || {}
        };

        return isMenuItem
          ? new Ext.menu.Item(config)
          : new Ext.Toolbar.Button(config);
      }
    },

    /**
    * Evaluate a RowAction button's state and retrieve it's new status.
    *
    * @param {Record[]} selections Selected records.
    * @param {Object} rowAction The RowAction Object to evaluate.
    * @return {Bool} True when it should be enabled, False if not.
    */
    getButtonState: function (selections, rowAction) {
      // no selection
      if (selections.length == 0) {
        rowAction.button.setIconClass(rowAction.iconCls);
        rowAction.button.setText(rowAction.text);
        return false;
      }
      // process icon class
      if (rowAction.conditionalIconClass) {
        rowAction.button.setIconClass(rowAction.conditionalIconClass(selections[0]));
        for (var i = 1; i < selections.length; i++) {
          if (rowAction.button.iconCls != rowAction.conditionalIconClass(selections[0])) {
            rowAction.button.setIconClass(rowAction.iconCls); break;
          }
        }
      }
      // process conditional text
      if (rowAction.conditionalText) {
        rowAction.button.setText(rowAction.conditionalText(selections[0]));
        for (var i = 1; i < selections.length; i++) {
          if (rowAction.button.text != rowAction.conditionalText(selections[0])) {
            rowAction.button.setText(rowAction.text); break;
          }
        }
      }
      // process toggle conditionals
      if (selections.length > 1) {
        if (!rowAction.allowMultipleSelect && !rowAction.allowOnlyMultipleSelect) return false;
        if (rowAction.predicate) for (var i = 0; i < selections.length; i++) {
          if (!rowAction.predicate(selections[i])) return false;
        }
      } else if (!rowAction.predicate(selections[0]) || rowAction.allowOnlyMultipleSelect) return false;
      if (rowAction.sharedCondition && !rowAction.sharedCondition(selections)) return false;
      // not returned yet? then return true! \o/
      return true;
    },

    /**
    * Show a new window of object fields, optionally filled with record's data.
    *
    * @param {Record} record The record to edit. Uses editUrl to GET data from and updateUrl to POST data to.
    *        If record is undefined, the window will allow adding new objects. Uses newUrl to POST to.
    * @Param {createSaveUrl} function that createn an url to send data to, editing a record will also use this url. When not defined defaults to this.updateUrl or this.newUrl
    * @Param {createLoadUrl} function that creates an url to load data from, editing a record will also use this url. When not defined defaults to this.editUrl
    * @return void
    */
    showNewWindow: function (record, extraItems, successAction, createLoadUrl, createSaveUrl, title, successMessage) {
      // if there's a record specified

      if (record) {
        // set the URLs properly
        this.formConfig.loadUrl = createLoadUrl instanceof Function ? createLoadUrl(record) : this.editUrl + "/" + record.id;
        this.formConfig.url = createSaveUrl instanceof Function ? createSaveUrl(record) : this.updateUrl + "/" + record.id;
        // and take into account additional keys
        if (this.otherKeys) {
          for (var i = 0; j < this.otherKeys.length; i++) {
            var otherKey = {}; var otherValue = self.keyPrefix + record[this.otherKeys[i]];
            otherKey["key_" + this.otherKeys[i]] = otherValue ? otherValue : record.get(this.otherKeys[i]);
            this.formConfig.loadUrl = Ext.urlAppend(loadUrl, Ext.urlEncode(otherKey));
            this.formConfig.url = Ext.urlAppend(url, Ext.urlEncode(otherKey));
          }
        } // eo if this.otherKeys
        // no record specified, use new url only
      }
      else {
        var url = this.formConfig.url || this.newUrl;
        this.formConfig.url = url;

        delete this.formConfig.loadUrl;
      }
      // set the proper fields
      if (this.newFormConfig) {
        this.formConfig.items = this.newFormConfig.items;
      }
      else if (this.fields) {
        this.formConfig.items = this.fields();
      }

      if (extraItems) {
        for (var i = 0; i < extraItems.length; i++)
          this.formConfig.items.push(extraItems[i]);
      }

      // replace "-" with spacers
      for (var i = 0; i >= 0; i = this.formConfig.items.indexOf("-")) {
        if (this.formConfig.items[i] == "-") this.formConfig.items[i] = new Ext.Spacer({
          height: 25
        });
      }
      var that = this;
      // and set the buttons
      this.formConfig.buttons = [
            {
              text: Diract.text.saveBtnText,
              scope: this,
              formBind: true,
              handler: function () {

                form.form.timeout = 120;
                form.submitForm({
                  url: this.formConfig.url,
                  waitMsg: Diract.text.processingMessage,
                  suppressSuccessMsg: true,
                  params: record ? this.editParams : this.newParams,
                  failure: function (form, action) {
                    if (that.handleFailure) {
                      that.failureCallback(action);
                    } else {
                      //Ext.MessageBox.alert(Diract.text.errorTitle, action.result.message);
                    }
                  },
                  success: function (form, action) {

                    var message = successMessage || (record ? Diract.text.editedTitle : Diract.text.createdTitle, that.singularObjectName + (record ? Diract.text.editedMessage : Diract.text.createdMessage));
                    Diract.message(message);

                    that.store.reload();
                    if (that.createCallback) that.createCallback(that.store, Ext.decode(Ext.decode(action.response.responseText).includes));
                    if (that.callback) that.callback(that.store, 1, record ? "edit" : "add", record);
                    window.destroy();
                    Diract.modals = []; //dirty
                    if (successAction)
                      successAction();
                  }
                }); // eo form.form.submit()
              } // eo handler
            },
            {
              text: record ? Diract.text.resetBtnText : Diract.text.clearBtnText,
              scope: this,
              handler: record ? function () { form.load({ url: this.formConfig.loadUrl, success: this.editLoadSuccess || function () { } }); }
                                : function () { form.form.reset(); }
            }
      ]; // eo config.buttons
      // build the form
      var form = new Ext.FormPanel(this.formConfig);
      if (this.formConfig.loadUrl) form.on("render", form.load.createDelegate(form, [{ url: this.formConfig.loadUrl, success: this.editLoadSuccess || function () { } }]));
      // set the proper form
      this.windowConfig.items = form;
      // build window buttons

      // and set the title
      this.windowConfig.title = title || (record ? Diract.text.editTitle : Diract.text.newTitle) + " " + this.singularObjectName;

      // define the window
      var window = new Ext.Window(this.windowConfig);

      if (!Diract.modals) Diract.modals = [];
      Diract.modals.push(window);

      // focus on first field on show
      window.on("show", function () { Diract.utility.focusFirst(form); });
      // and show!
      window.show();
      this.window = window;
    },

    /**
    * Set the paging size of this grid.
    *
    * Note: call this before (re)loading the store.
    *
    * @param {Int} pageSize The new size of the pages.
    * @return void
    */
    setPageSize: function (pageSize) {
      this.originalPageSize = this.pageSize;
      this.pageSize = pageSize;
      this.bbarPagingTool.setPageSize(pageSize);
    },
    /**
    * Reset the paging size of this grid back to it's original. Will do nothing
    * when it was not modified.
    *
    * Note: call this before (re)loading the store.
    *
    * @return void
    */
    resetPageSize: function () {
      if (this.originalPageSize) {
        this.setPageSize(this.originalPageSize);
        delete this.originalPageSize;
      }
    },
    deleteComponentStateUrl: Diract.route('DeleteSaveState', 'UserComponentState'),
    saveComponentStateUrl: Diract.route('Save', 'UserComponentState'),
    exportComponentToExcelUrl: Diract.route("ExportGrid", "ExportGrid")


  }); // eo Ext.extend

  // load up quicktips for validation messages
  Ext.QuickTips.init();

});