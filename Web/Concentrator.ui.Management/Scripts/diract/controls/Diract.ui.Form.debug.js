Ext.form.FormPanel.prototype.submitForm = function (config) {

  if (this.form.isValid()) {
    var url = this.url; // Create a variable that can be captured     

    if (this.fireEvent('beforesubmit', this)) {

      var args = {};
      this.form.submit({
        url: url,
        params: Ext.apply(args, typeof config.params == "function" ? config.params.apply(this) : config.params),
        waitMsg: 'Processing',
        method: 'POST',
        failure: function (form, action) {
          if (!config.suppressFailureMsg) {
            if (action.result && action.result.message) {
              Ext.MessageBox.alert('Error', config.failureMsg || action.result.message);
            } else {
              if (action.response.status == 404) {
                Ext.MessageBox.alert('Error', "Page not found: " + url);
              } else {
                Ext.MessageBox.alert('Error', config.failureMsg || "An unknown error occurred on the server");
              }
            }
          }

          if (config.failure) {
            config.failure(form, action);
          }
        },
        success: function (form, action) {
          if (!config.suppressSuccessMsg) {
            Diract.message("Success", action.result.message || config.successMsg);
          }

          if (config.success) {
            config.success(form, action);
          }

          if (config.closeWindow) {
            config.closeWindow.destroy();
          }
        }
      });
    }
    this.fireEvent('aftersubmit', this);
  } else {
    Ext.MessageBox.alert('Form Invalid', 'Please fix the errors noted.');
  }

};
Diract.ui.Form = Ext.extend(Ext.form.FormPanel, {
  bodyStyle: 'padding: 10px;',
  monitorValid: true,
  buttonAlign: 'left',
  buttonText: "Submit",
  border: false,
  autoLoadForm: true,
  editLoadSuccess: function (form, action) { },
  suppressSuccessMsg: false,
  constructor: function (config) {
    Ext.apply(this, config);

    if (config.disableButton == undefined) {

      if (!this.noButton && !this.buttons) {
        this.buttons = [
            {
              text: this.buttonText,
              formBind: true,
              scope: this,
              handler: this.buttonHandler || function () {
                this.submit();
              }
            }
        ]
      }
      if (this.extraButtons) {
        Ext.each(this.extraButtons, function (btn) {
          this.buttons.push(btn);
        }, this);
      }
    }

    Diract.ui.Form.superclass.constructor.call(this);

    if (this.loadUrl && this.autoLoadForm) {
      this.on('render', function () {
          this.loadForm({});
      });
    }
  },
  loadForm: function (params) {
      this.load({ url: this.loadUrl, method: "GET", success: this.editLoadSuccess, params: Ext.apply(params, this.loadParams) });
  },
  submit: function () {

    this.submitForm({
      url: this.url,
      suppressSuccessMsg: this.suppressSuccessMsg,
      params: typeof this.params == 'function' ? this.params() : this.params,
      successMsg: this.successMsg ? this.successMsg : "Save succesful",
      success: this.success,
      failure: this.failure
    });
  }
});

Diract.ui.FormWindow = Ext.extend(Ext.Window, {
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

    Diract.ui.FormWindow.superclass.constructor.call(this, config);
  }
});

// window to allow quick uploading of a file
Diract.ui.ImportWindow = Ext.extend(Ext.Window, {

  // mandatory
  url: "",
  callback: function () { },

  // defaults
  autoShow: true,
  title: "Upload Excel file",
  successMsg: "Successfully uploaded Excel file!",
  width: 400,
  height: 110,
  layout: "fit",
  modal: true,
  // field defaults
  fieldText: "Select an Excel file (.xls or .xlsx)",
  fieldLabel: "Excel file",
  fieldName: "file",

  // submit
  submit: function () {
    if (this.form.form.isValid()) {
      this.form.form.submit({
        waitMsg: "Uploading...",
        scope: this,
        failure: function (form, action) {
          Ext.MessageBox.alert("Error", action.result.message);
        },
        success: function (form, action) {
          Diract.message("Success", this.successMsg);
          this.callback(action.result);
          this.destroy();
        }
      });
    }
  },

  // constructor
  constructor: function (config) {

    // apply config
    Ext.apply(this, config);

    // build form
    this.form = new Ext.FormPanel({
      bodyStyle: "padding: 10px",
      border: false,
      url: this.url,
      fileUpload: true,
      items: [
                new Ext.form.FileUploadField({
                  emptyText: this.fieldText,
                  fieldLabel: this.fieldLabel,
                  name: this.fieldName,
                  allowBlank: false
                })
      ]
    });

    // attach form to window
    this.items = [
            this.form
    ];

    // add buttons
    this.buttons = [
            {
              text: "Upload",
              scope: this,
              handler: function () {
                this.submit();
              }
            },
            {
              text: "Clear",
              scope: this,
              handler: function () {
                this.form.form.reset();
              }
            }
    ];

    // init
    Diract.ui.ImportWindow.superclass.constructor.call(this);

    // autoshow if desired
    if (this.autoShow) this.show();

  } // eo constructor

});
