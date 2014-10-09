Diract.ui.GridWizard = (function () {
  var wiz = Ext.extend(Ext.Window, {
    width: 800,
    height: 400,
    modal: true,
    layout: 'fit',

    constructor: function (config) {
      Ext.apply(this, config);

      this.initStore();
      this.initBBar();
      this.title = this.title || 'Wizard';

      Diract.ui.GridWizard.superclass.constructor.call(this, config);
    },

    //retrieves the currently active record
    currentRecord: function () {

      return this.grid.store.getAt(this.currentStorePosition);

    },

    initStore: function () {

      this.currentPageNumber = 1; //start position
      this.currentStorePosition = 0; //the position in the store
      this.totalNumberOfPages = this.grid.store.totalLength; //total number of pages
      this.wizardEndPoint = this.totalNumberOfPages - 1;
      this.storePageSize = this.grid.pageSize; //the page size

    },
    finishWizard: function () {

    },
    moveForward: function (callback) {

      //handle finishing wizard
      if (this.currentPageNumber == this.wizardEndPoint) {

        var that = this;
        this.nextButton.setText('Finish');

        this.nextButton.setHandler(function (button) {

          that.destroy();

        }, this);

        // If this.backButton is pressed
        this.backButton.setHandler(function () {

          that.moveBackward();

          that.nextButton.setText('Next');

          that.nextButton.setHandler(function (button) {

            that.moveForward();

          });

        });

      }

      if (this.currentStorePosition == this.storePageSize - 1) //reached end of store
      {
        this.grid.store.reload({
          url: this.grid.url,
          params: {
            start: (this.currentPageNumber),
            limit: 50
          },
          callback: (function () {
            this.currentStorePosition = 0;
            this.syncIndexes(true);
            callback();
          }).createDelegate(this)
        });
      } else {
        this.syncIndexes(true, true);
        callback();
      }
    },

    /**
    Syncs the indexes 
    */
    syncIndexes: function (isAscending, setStore) {

      if (isAscending) {
        this.currentPageNumber++;
        if (setStore)
          this.currentStorePosition++;
      } else {
        this.currentPageNumber--;
        if (setStore)
          this.currentStorePosition--;
      }

      this.bbarLabel.setText('Page ' + (this.currentPageNumber) + ' of ' + this.totalNumberOfPages);
      //put code to update bbar text
    },
    moveBackward: function (callback) {
      //Decrement element   
      //handle finishing wizard
      if (this.currentPageNumber == 2) {
        this.backButton.setDisabled(true);
      }

      if (this.currentStorePosition == 0) //reached end of store
      {
        this.grid.store.reload({
          url: this.grid.url,
          params: {
            start: (this.currentPageNumber - 1 - this.storePageSize),
            limit: 50
          },
          callback: (function () {
            this.currentStorePosition = 49;
            this.syncIndexes(false);
            callback();
          }).createDelegate(this)
        });
      } else {
        this.syncIndexes(false, true);
        callback();
      }
    },

    noMatch: function (self) {

      if (this.currentPageNumber == this.totalNumberOfPages) {
        self.destroy();
      }
      else {
        this.moveForward();
      }


    },

    match: function () {

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


      if (this.totalNumberOfPages == 1) {

        var that = this;

        this.nextButton.setText('Finish');

        this.nextButton.setHandler(function (button) {
          that.destroy();
        }, this);

      }

      this.noButton = new Ext.Button({
        text: 'No',
        handler: (function () {

          if (this.currentPageNumber == this.wizardEndPoint) {
            this.nonMatch();
          }
          else {
            this.nonMatch();
          }

        }).createDelegate(this),
        iconCls: 'stop'
      });

      this.yesButton = new Ext.Button({
        text: 'Yes',
        handler: (function () {

          var that = this;

          if (this.currentPageNumber == this.totalNumberOfPages) {

            this.match();

            that.destroy();

          }
          else {

            this.match();

          }

        }).createDelegate(this),
        iconCls: 'play'
      });

      this.bbarLabel = new Ext.form.Label({
        html: 'Page 1 of ' + this.totalNumberOfPages
      });

      this.bbar = [this.bbarLabel, "->", this.backButton, this.nextButton, "-", this.noButton, this.yesButton]      
           
    }
  });

  return wiz;

})();
