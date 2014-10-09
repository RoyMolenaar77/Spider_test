Concentrator.ui.login = (function ()
{
  var instance = null;
  var isShown = false;

  function LoginWindow()
  {
    this.show = function (stayOnPage)
    {
      if (isShown)
      {
        return;
      }

      if (stayOnPage == null)
      {
        stayOnPage = false;
      }

      var formValidator = function ()
      {
        return userNameField.getValue() != '' && passwordField.getValue() != '';
      }

      function validateFields()
      {
        if (formValidator())
          Ext.getCmp('submitButton').enable();
        else
          Ext.getCmp('submitButton').disable();
      }

      function submitForm()
      {
        validateFields();

        if (formValidator())
        {
          loginForm.form.submit({
            waitMsg: 'Logging in',
            failure: function (form, action)
            {
              Ext.MessageBox.show({
                title: 'Login Unsuccesful',
                msg: action.result.message,
                buttons: Ext.Msg.OK
              });
            },

            success: function (form, action)
            {
              isShown = false;

              Concentrator.user.timeout = action.result.timeout;

              // update js variables
              Concentrator.user.functionalities = action.result.functionalities;
              Concentrator.user.loggedIn = true;

              Concentrator.user.userID = action.result.userID;
              Concentrator.user.name = action.result.fullName;




              if (!stayOnPage)
              {
                window.location = Concentrator.root;
              }

              else
              {
                loginWindow.destroy();
              }

            }
          });
        }
      }

      var userNameField = new Ext.form.TextField({
        fieldLabel: 'Username',
        id: 'username',
        allowBlank: true,
        enableKeyEvents: true,
        listeners: {
          'change': validateFields,
          'specialkey': function (field, ev)
          {
            if (ev.getKey() == ev.ENTER)
            {
              ev.preventDefault();
              passwordField.focus('', 20);;
            }
          }
        }
      });

      var passwordField = new Ext.form.TextField({
        id: 'password',
        fieldLabel: 'Password',
        inputType: 'password',
        allowBlank: true,
        enableKeyEvents: true,
        listeners: {
          'change': validateFields,
          'specialkey': function (field, ev)
          {
            if (ev.getKey() == ev.ENTER)
            {
              ev.preventDefault();
              submitForm();
            }
          }
        }
      });

      var loginButton = new Ext.Button({
        id: 'submitButton',
        text: 'Login',
        disabled: true,
        minWidth: 75,
        type: 'submit',
        handler: submitForm
      });

      var formItems = [];

      var logo = {
        border: false,
        html: '<img src="' + Concentrator.root + 'Content/images/logo.png">'
      };

      var hr = {
        border: false,
        html: '<hr style="color: #dfe8f6; background-color: #dfe8f6; height: 2px;" />'
      };

      var forgotPasswordLink = {
        border: false,
        html: '<a href="#" style="float: right; color: #696969;" onClick="javascript:ForgotPasswordFunction()"> Forgot your password? </a>'
      };

      formItems.push(logo);
      formItems.push(userNameField);
      formItems.push(passwordField);
      formItems.push(hr);
      formItems.push(forgotPasswordLink);

      var loginForm = new Ext.form.FormPanel({
        url: Concentrator.user.loggedIn ? Concentrator.route('Unlock', 'Account') : Concentrator.route('Login', 'Account'),
        method: 'POST',
        items: formItems,
        bodyStyle: 'padding: 20px 15px;',
        defaults: { anchor: '90%' },
        border: false,
        buttons: [loginButton]
      });

      var loginWindow = new Ext.Window({
        title: Concentrator.user.loggedIn ? 'Unlock' : 'Login',
        layout: 'fit',
        modal: true,
        closable: false,
        items: [loginForm],
        width: 450,
        height: 350,
        listeners: {
          'show': function ()
          {
            userNameField.focus('', 100);
          },
          'specialkey': function (field, ev)
          {
            if (ev.getKey() == ev.ENTER)
            {
              ev.preventDefault();
              submitForm();
            }
          }
        }
      });

      isShown = true;
      loginWindow.show();
      userNameField.focus(false, 250);
    }
  }

  return new function ()
  {
    this.getInstance = function ()
    {
      return new LoginWindow();
    }
  }
})();

ForgotPasswordFunction = function ()
{
  var that = this;

  var emailField = new Ext.form.TextField({
    name: 'EmailAddress',
    fieldLabel: 'Emaill address'
  });

  var window = new Diract.ui.FormWindow({
    url: Concentrator.route('ResetPassword', 'Account'),
    cancelButton: true,
    title: 'forgot your pasword?',
    autoDestroy: true,
    height: 120,
    suppressSuccessMsg: true,
    width: 340,
    modal: true,
    layout: 'fit',
    items: [
      emailField
    ],
    success: function (result)
    {
      window.destroy();

      Ext.Msg.show({
        title: 'Forgotten password',
        msg: 'Your password has been sent to the specified email address',
        buttons: Ext.Msg.OK,
        icon: Ext.MessageBox.QUESTION
      });

    }
  });

  window.show();
};

Concentrator.ui.logout = function ()
{
  // Fix for the "emtpy blue page after logging in" bug. 
  // Only show the login window with stayOnPage = true when the user is actually logged in.
  if (Concentrator.user.loggedIn)
  {
    Concentrator.ui.getInstance().show(true);
    Concentrator.user.loggedIn = false;
    Concentrator.mute_request({
      url: Concentrator.route("DoLogout", "Account")
    });
  }
};

var beforeRequestCallback = function (connection, options)
{
  if (options.url.indexOf('Unlock') >= 0)
    return;

  Concentrator.lastRequestMade = { connection: connection, options: options };
}

Ext.Ajax.on('beforerequest', beforeRequestCallback);

var requestCompleteCallback = function (connection, response, options, y, x, z)
{
  var result;

  if (response.getResponseHeader && response.getResponseHeader("Content-Type") && response.getResponseHeader("Content-Type").indexOf('application/json') >= 0)
  {
    result = Ext.decode(response.responseText);
  }
  else
  {
    result = response.responseText;
  }

  if (result.authorized === false)
  {
    Concentrator.ui.login.getInstance().show(true);
    Concentrator.user.loggedIn = false;
    Concentrator.user.lastLoggedIn = false;
  }
  else
  {
    //check if the previous request has been not authorized
    if (Concentrator.user.lastLoggedIn !== undefined && !Concentrator.user.lastLoggedIn)
    {
      //so now he is authorized but he logged in again
      //if some action is specified as a result in such cases execute it

      var active = Concentrator.ViewInstance.lastOpened();

      Ext.Ajax.request(Concentrator.lastRequestMade.options);

      if (active)
      {
        if (active.refresh != null)
        {
          active.refresh();
        }
        else
        {
          active.doLayout();
        }
      }
      Ext.each(Concentrator.modals, function (wind) { wind.destroy(); });
    }
    Concentrator.user.lastLoggedIn = true;

  }

}

Ext.Ajax.on('requestcomplete', requestCompleteCallback);