Diract.addComponent({ dependencies: [] }, function () {

  Diract.mute_request = function (config) {
    Diract.request(Ext.apply(config, { suppressSuccessMsg: true, suppressFailureMsg: true }));
  };

  Diract.silent_request = function (config) {
    Diract.request(Ext.apply(config, { suppressSuccessMsg: true, suppressFailureMsg: false }));
  };

  Diract.request = (function () {

    var requestBuffer = [];

    var defaults = {
      /*
      The URL to send the request to, should be overridden.
      This controller is to specify "success" in it's JSON response.
        
      Example: url = Diract.route("Edit", "Something")
      */
      url: "",

      /*
      The parameters to pass onto the URL, most likely needs an override.
        
      Example: params = { somethingID: someField.getValue() }
      */
      params: {},

      /*
      The scope to be applied to "this" in the callbacks.
      Defaults to undefined, which will set the scope to the Diract.request call.
      */
      scope: undefined,

      /*
      XML document to use for the post.
      Note: This will be used instead of params for the post data. Any params will be appended to the URL.
      */
      xmlData: undefined,

      /*
      Timeout the request after specified amount of milliseconds.
      Defaults to 30000 (30 seconds).
      */
      timeout: 30000,

      /*
      Wait title and message to be displayed with the progress bar while this request is pending.
      If waitMsg is undefined, it will not be displayed.
      */
      waitTitle: Diract.text.waitingMessage,
      waitMsg: undefined,

      /*
      Request method.
      */
      method: "POST",

      /*
      Use this to indicate how many requests responses need to be received before outputting.
      Defaults to 1, which means it will output any request directly.
    
      Example: flushAt = requests.length
      */
      flushAt: 1,

      /*
      Flush messages. Feel free to overwrite.
      */
      summaryMsg: Diract.text.flushSummarySuccess,

      summaryFailMsg: Diract.text.flushSummaryFailure,

      /*
      Message displayed when the server fails (exception).
      */
      errorMsg: function (response, request) { return Diract.text.serverError500 + "<br /><br />" + request.url + "."; },

      /*
      Message displayed when the request is incorrect (response.success = false returned).
      If not specified, the server's response.message is used.
      */
      failureMsg: undefined,

      /*
      Message displayed when the request has succeeded (response.success = true).
      If not specified, the server's response.message is used.
      */
      successMsg: undefined,

      failureTitle: Diract.text.failureTitle,
      successTitle: Diract.text.successTitle,

      /*
      Additional functions to perform after a success or failure. Will be called every request return, even when buffered.
    
      @param result: decoded responseText
    
      Example: success = function(result) { grid.store.reload(); }
      */
      success: undefined,

      failure: undefined,

      /*
      Additional function to perform after a flush. Single requests also count as flushed, although I recommend using success and failure there.
    
      @param succeeds: number of the amount of requests that were successfull
      @param fails: number of the amount of requests that failed (either failure or error)
      @param errors: array of strings of the error and failure messages collected - errors.length = fails
    
      Example: onFlush = function(succeeds, fails, errors) { if(fails > 0) button.enable(); else button.disable(); }
      */
      onFlush: undefined,

      /*
      Specify a callback function to bypass further Diract.request processing.
      Note that this is the combined success and failure output! These functions do NOT exist outside this scope, to avoid confusion.
      Within Diract.request, term failure means a successful request with a success=false param. Error is the term used for failing requests.
        
      Not recommended to override.
      */
      callback: undefined,

      /*
      Quickly allow suppressing messages. All default to false.
      */
      suppressSuccessMsg: false,

      suppressFailureMsg: false,

      suppressErrorMsg: false,

      suppressFlushMsg: false
    };

    var output = function (config, success, title, msg, suppress) {
      // if we're flushing
      if ((requestBuffer.length + 1) >= config.flushAt) {
        var fails = 0, succeeds = 0, errors = [];
        // if there was no buffered data, simply output
        if (requestBuffer.length == 0) {
          // make sure the flush call has correct params
          if (success) {
            succeeds++;
          } else {
            errors[fails++] = msg;
          }
          // and display message
          if (!suppress && success) {
            Diract.message(title, msg);
          } else if (!suppress) {
            Ext.MessageBox.alert(title, msg);
          }
          // else, summarize buffer and output
        } else {
          // first, make sure we include the current call, too
          requestBuffer.push({ success: success, msg: msg });
          // summarize
          for (var i = 0; i < requestBuffer.length; i++) {
            if (requestBuffer[i].success) {
              succeeds++;
            } else {
              errors[fails++] = requestBuffer[i].msg;
            }
          }
          // formulate
          var s = String.format(config.summaryMsg, succeeds);
          if (fails > 0) {
            s += "<br /><br />";
            s += String.format(config.summaryFailMsg, fails, errors.join("<br />"));

          }
          // output
          if (!config.suppressFlushMsg && fails > 0) {
            Ext.MessageBox.alert(Diract.text.summaryTitle, s);
          } else if (fails == 0) {
            Diract.message(Diract.text.summaryTitle, s);
          }
          // reset
          requestBuffer = [];
        }
        // launch onFlush function if specified
        if (config.onFlush) {
          config.onFlush.createDelegate(config.scope)(succeeds, fails, errors);
        }
        // else, we're buffering
      } else {
        requestBuffer.push({ success: success, msg: msg });
      }

    };

    var request = function (config) {

      // set configs from defaults
      config = Ext.apply({}, config, defaults);

      // show wait messagebox if required
      if (config.waitMsg) {
        config.waitMessageBox = Ext.MessageBox.wait(config.waitTitle, config.waitMsg);
      }

      // do the actual request
      Ext.Ajax.request({
        // default params
        url: config.url,
        disableCaching: true,
        method: config.method,
        timeout: config.timeout,
        params: config.params,
        scope: config.scope,
        xmlData: config.xmlData,
        // upon success
        success: function (response, options) {
          // cancel a possible wait messagebox
          if (config.waitMessageBox) config.waitMessageBox.hide();
          // allow overriding process


          if (!config.callback) {
            // get decoded response
            var result = Ext.decode(response.responseText);
            // check for standard success property

            if (result.success === false) {
              // display failure message
              output(config, false, config.failureTitle, !config.failureMsg ? result.message : (typeof config.failureMsg == "function" ? config.failureMsg(result) : config.failureMsg), config.suppressFailureMsg);
              if (config.failure) {
                config.failure.createDelegate(config.scope)(result);
              }
            }
            else {
              //something changed the state of the applicaton and it requiresa a refresh

              if (result.needsRefresh) {
                var refreshMsg = " This page will automatically be refreshed in order to apply the changes."
                result.message = result.message ? result.message + refreshMsg : refreshMsg;
                setTimeout(function () {
                  window.location.reload();
                }, 2000);
              }

              // display sucess message
              output(config, true, config.successTitle,
                     result.message ?
                             result.message :
                             (typeof config.successMsg == "function" ? config.successMsg(result) : config.successMsg)

                             , config.suppressSuccessMsg);


              // launch success function if specified
              if (config.success) {
                config.success.createDelegate(config.scope)(result || response.responseText);
              }
            }
            // call the callback, with success = true
          } else {
            config.callback(options, true, response);
          }
        },
        // upon error
        failure: function (response, options) {


          // cancel a possible wait messagebox
          if (config.waitMessageBox) config.waitMessageBox.hide();
          // allow overriding process
          if (!config.suppressFailureMsg) {
            if (!config.callback) {
              // display error
              if (response.status == 404) {
                output(config, false, Diract.text.errorTitle, Diract.text.serverError404 + "<br /><br />" + config.url, config.suppressErrorMsg);
              } else {
                output(config, false, Diract.text.errorTitle, typeof config.errorMsg == "function" ? config.errorMsg(response, config) : config.errorMsg, config.suppressErrorMsg);
              }
              // launch error function if specified
              if (config.error) {
                config.error(response);
              }
              // call the callback, with success = false
            } else {
              config.callback(options, false, response);
            }
          }
        }
        // note the explicit lack of the callback function!
      });
    }

    return request;

  })();

});