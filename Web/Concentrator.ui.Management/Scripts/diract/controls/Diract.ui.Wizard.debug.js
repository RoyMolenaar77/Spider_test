Diract.ui.Wizard = (function () {

  var window = Ext.extend(Ext.Window, {

    constructor: function (config) {
      Ext.apply(this, config);

      this.initStore();
      this.initBBar();

      Diract.ui.Wizard.superclass.constructor.call(this, config);

      // When creating a wizard. You need to send the following:

      // - Send the grid(for refreshing the grid and display your changes)
      // - The getItems() function, which contains all components you want to display in your wizard
      // - Your additional parameters(only if you have any)
      // - A URL.
      // - And give your form some height

      // For future reference, check out the PriceRuleWizard      

      var that = this;
      that.params = this.params;
      that.grid = this.grid;

      this.doLayout();
    },

    initStore: function (config) {
      this.totalNumberOfPages = this.getItems().length;

      this.currentPageNumber = 1;
      this.indexNumber = 0;
    },
    initBBar: function () {

      this.backButton = new Ext.Button({
        text: 'Back',
        handler: (function () {
          this.moveBackward();
        }).createDelegate(this),
        disabled: true,
        iconCls: 'skip-backward'
      });

      this.nextButton = new Ext.Button({
        text: 'Next',
        handler: (function () {
          this.moveForward();
        }).createDelegate(this),
        iconCls: 'skip-forward'
      });

      this.bbarLabel = new Ext.form.Label({
        html: 'Page 1 of ' + this.totalNumberOfPages
      });

      this.bbar = [this.bbarLabel, "->", this.backButton, this.nextButton]
    },

    syncIndexes: function (isAscending, isDecending) {

      if (isAscending) {
        this.currentPageNumber++;
        this.indexNumber++;
      }

      if (isDecending) {
        this.currentPageNumber--;
        this.indexNumber--;
      }

      this.populatePanel(this.indexNumber);

      this.bbarLabel.setText('Page ' + (this.currentPageNumber) + ' of ' + this.totalNumberOfPages);
    },

    moveForward: function (callback) {
      var that = this;

      that.syncIndexes(true, false);

      if (this.currentPageNumber == this.totalNumberOfPages) {

        this.nextButton.setText('Finish');
        this.nextButton.setHandler(function () {
          that.submitFormWizard();
        });

      }

      this.backButton.setHandler(function () {
        that.moveBackward();
        that.nextButton.setText('Next');
        that.nextButton.setHandler(function (button) {
          that.moveForward();
        });

      });

      this.backButton.setDisabled(false);

    },

    moveBackward: function (callback) {
      var that = this;

      that.syncIndexes(false, true);

      if (this.currentPageNumber == 1) {
        this.backButton.setDisabled(true);
      }

    },

    populatePanel: function (indexNumber, backwards, forward) {
      var that = this,
          hiddenIndex,
          itemArray = [];

      itemArray = that.getItems();

      Ext.each(itemArray, function (item) {
        item.hide();
      });

      that.items.itemAt(indexNumber).show();
    },

    getFormValues: function (initializations) {

      var params = {},
          items;

      if (initializations) {
        items = initializations;
      } else {
        items = this.getItems();
      }

      Ext.each(items, function (form) {
        Ext.apply(params, form.getForm().getFieldValues());
      });

      return params;
    },

    submitFormWizard: function () {      

      var that = this,
          formParams = [];

      formParams = that.getFormValues();

      if (that.params) {
        Ext.apply(formParams, that.params);
      }

      Diract.request({
        url: that.url,
        params: formParams,
        success: function () {

          that.destroy();

          if (that.grid) {
            that.grid.store.reload();
          }
        }
      });
    }

  });

  return window;
})();