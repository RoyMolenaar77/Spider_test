Diract = {
  components: [],

  cmp: new Queue(),

  addComponent: function (d, func) {

    Diract.cmp.enqueue({
      dependencies: d.dependencies,
      init: func
    });

  },

  init: function () {

    if (!Diract.APPLICATION_ROOT) throw "Diract.APPLICATION_ROOT has not been defined";
    if (!Diract.GRID_PAGE_SIZE) throw "Diract.GRID_PAGE_SIZE has not been defined";
    if (!Diract.MESSAGES_ELEMENT) throw "Diract.MESSAGES_ELEMENT has not been defined";
    if (!Diract.LANGUAGE) throw "Diract.LANGUAGE has not been defined";

    switch (Diract.LANGUAGE) {
      case "English":
        Diract.setLanguageEnglish()
        break;
    }

    var cmp = Diract.cmp;

    Diract.override();

    while (!cmp.isEmpty()) {
      var item = cmp.dequeue();

      var ready = true;
      Ext.each(item.dependencies, function (d) {
        if (!eval(d)) {
          ready = false;
        }
      });

      if (!ready) {
        cmp.enqueue(item);
      } else {
        item.init();
      }

    }

    this.editors = new DefaultEditorRepository();

  },
  content: function (contentPath) {
    return Diract.APPLICATION_ROOT + contentPath;
  },
  externalRoute: function (path, params) {
    return path + '?' + Ext.urlEncode(params);
  },
  route: function (action, controller, id) {
    
    var queryString = null;
    if (id && typeof id == "object") {
      queryString = "?" + Ext.urlEncode(id);
      return Diract.APPLICATION_ROOT + controller + "/" + action + queryString;
    }
    return Diract.APPLICATION_ROOT + controller + "/" + action + ((id != null) ? "/" + id : "");
  },


  message: function (title, message) {
    var msgCt;
    function createBox(t, s) {
      return ['<div class="msg">',
              '<div class="x-box-tl"><div class="x-box-tr"><div class="x-box-tc"></div></div></div>',
              '<div class="x-box-ml"><div class="x-box-mr"><div class="x-box-mc"><h3>', t, '</h3>', s, '</div></div></div>',
              '<div class="x-box-bl"><div class="x-box-br"><div class="x-box-bc"></div></div></div>',
              '</div>'].join('');
    };

    var m = Ext.DomHelper.append(Diract.MESSAGES_ELEMENT, { html: createBox(title, message || Diract.text.defaultConfirmationMessage) }, true);
    m.on('click', function () { m.remove(); });
    m.slideIn('t').pause(5).ghost("t", { remove: true });
  },
  override: function () {

    Ext.override(Ext.ux.grid.GridFilters, {
      getFilterData: function () {
        var filters = [], i, len;

        this.filters.each(function (f) {
          if (f.active) {
            var d = [].concat(f.serialize());
            for (i = 0, len = d.length; i < len; i++) {
              filters.push({
                field: f.filterField || f.dataIndex,
                data: d[i]
              });
            }
          }
        });
        return filters;
      }
    });
  },
  user: {
    isInRole: function (roles) {
      return true;
    },
    ifInRole: function (roles, value) {
      if (Diract.user.isInRole(roles)) {
        return value;
      } else {
        return null;
      }
    }
  },
  ui: {},
  utility: {
    focusFirst: function (form) {
      var item = form.items.find(Diract.utility.findFirst);
    },
    findFirst: function (item) {
      if (item instanceof Ext.form.FieldSet) {
        return item.items.find(Diract.utility.findFirst);
      }
      if (item instanceof Ext.form.Field && !item.hidden && !item.disabled) {
        item.focus(false, 100); // delayed focus by 100 ms
        return true;
      }
      return false;
    }
  },
  type: function (members, constructor) {

    var f = constructor || function () { };

    for (var m in members) {
      f.prototype[m] = members[m];
    }
    return f;
  },
  setLanguageEnglish: function () {

    Diract.text = {
      defaultConfirmationMessage: "Completed action",
      waitingMessage: "Please wait",
      flushSummarySuccess: "Successfully processed {0} requests.",
      flushSummaryFailure: "Unfortunately, also {0} requests failed. Below are the concatenated error responses:<br /><br /> {1}",
      serverError500: "The request failed. There was an error on the server at this location:",
      serverError404: "Page not found:",

      failureTitle: "Failure",
      errorTitle: "Error",
      successTitle: "Success",
      summaryTitle: "Summary",
      editedTitle: "Edited",
      createdTitle: "Created",
      editTitle: "Edit",
      newTitle: "New",
      confirmTitle: "Confirm",

      createdMessage: " was succesfully created",
      editedMessage: " was succesfully edited",
      editNotAllowedMessage: "It is not allowed to edit this record.",
      discardNewLinesConfirmation: "This will discard all new added lines! Are you sure you want to continue?",

      saveBtnText: "Save",
      processingMessage: "Processing..",

      resetBtnText: "Reset",
      clearBtnText: "Clear",
      clearFilterBtnText: "Clear Filters",
      clearFilterBtnTooltip: "Click to clear all filters on this grid",

      resetViewBtnText: "Reset View",
      resetViewBtnTooltip: "Reset the custom view to default",

      excelBtnText: "Export to Excel",
      excelBtnTooltip: "Export grid content to an Excel spreadsheet",

      excelTemplateBtn: "Export template to Excel",
      excelTemplateBtnTooltip: "Export tempate to an excel spreadsheet",

      addNewBtnText: "Add new {0}",
      addNewBtnTooltip: "Click to add a new {0}",

      editBtnText: "Save changes",
      editBtnTooltip: "Save the changes you've made",
      editSuccessMessage: " successfully edited.",

      discardBtnText: "Discard changes",
      discardBtnTooltip: "Reverts the changes you've made",
      discardBtnConfirmation: "Are you sure?",

      deleteBtnText: "Delete {0}",
      deleteBtnTooltip: "Deletes a {0}",
      deleteBtnConfirmation: "Are you sure?",
      deleteSuccessMessage: "{0} successfully deleted.",
      deleteSuccessSummaryMessage: "Successfully deleted {0} ",

      newLineBtnText: "Add new line",
      newLineBtnTooltip: "Click to add an empty new line to the grid",

      saveNewLineBtnText: "Save new line(s)",
      saveNewLineBtnTooltip: "Click to save all newly added lines",
      saveNewLineSuccessSummaryMessage: "Added {0} new ",
      saveNewLineSuccessMessage: " successfully added.",

      deleteNewLineBtnText: "Remove new line(s)",
      deleteNewLineBtnTooltip: "Click to remove selected new line(s)",
      deleteNewLineBtnConfirmation: "Are you sure you want to remove {0} new {1}?",
      lineSingular: "line",
      linePlural: "lines",
      deleteNewLineBtnError: "Please select a newly added line.",

      pagingBarSummary: " {0} - {1} of {2}",
      pagingBarEmpty: "No {0} to display",

      loginInProgress: "Logging in..",
      loginFailure: "Login Unsuccessful",
      loginSuccess: "Login Successful",
      applicationLoading: 'Please wait while ' + Diract.APPLICATION_NAME + ' is loading'



    }
  }

};