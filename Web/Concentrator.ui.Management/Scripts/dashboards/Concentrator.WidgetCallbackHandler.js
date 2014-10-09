
/**
Handles all events originating from widgets
@callbackUI - the name of the UI to be opened
@params - the params to be passed in the UI.A string object. Will be eval-ed
@functionName -  optional function to be executed instead of opening the callbackUI
*/
Concentrator.WidgetCallbackHandler = function (callbackUI, params, functionName) {
  if (!functionName)
    Concentrator.ViewInstance.open(callbackUI, Ext.decode(params));
  else
    functionName();
};