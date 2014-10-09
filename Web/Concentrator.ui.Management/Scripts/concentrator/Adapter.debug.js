Concentrator.adapter = {

    createAliases: function () {
        Concentrator.ui.Grid = Diract.ui.Grid;
        Concentrator.request = Diract.request;
        Concentrator.silent_request = Diract.silent_request;
        Concentrator.mute_request = Diract.mute_request;
        Concentrator.route = Diract.route;
        Concentrator.externalRoute = Diract.externalRoute;
        Concentrator.content = Diract.content;
        Concentrator.ui.NavigationPanel = Diract.ui.NavigationPanel;
        Concentrator.editors = Diract.editors;
        Concentrator.root = Diract.APPLICATION_ROOT;
        Concentrator.selected = Diract.selected;
        Concentrator.ui.TreeGrid = Diract.ui.TreeGrid;
        Concentrator.ui.PropertyGrid = Diract.ui.PropertyGrid;
        Concentrator.ui.TranslationGrid = Diract.ui.TranslationGrid;
        Concentrator.ui.ExcelGrid = Diract.ui.ExcelGrid;
        Concentrator.ui.Wizard = Diract.ui.Wizard;
        Concentrator.modals = Diract.modals;

    },
    createOverrides: function () {

        Diract.user.isInRole = function (functionality) {

            if (Ext.isArray(functionality)) {
                for (var idx = 0; idx < functionality.length; idx++) {
                    if (Concentrator.user.functionalities.indexOf(functionality[idx]) >= 0)
                        return true;
                }
            } else {
                return Concentrator.user.functionalities.indexOf(functionality) >= 0;
            }

            return false;
        }

        Diract.user.hasFunctionality = function (functionality) {

            if (Ext.isArray(functionality)) {
                for (var idx = 0; idx < functionality.length; idx++) {
                    if (Concentrator.user.functionalities.indexOf(functionality[idx]) >= 0)
                        return true;
                }
            } else {
                return Concentrator.user.functionalities.indexOf(functionality) >= 0;
            }

            return false;
        };

        Diract.user.isAuthorizedToAccess = function (obj) {
          var key = 'requires';
          var requirements = obj[key];

          if (!!!requirements) return true;


          if (typeof (requirements) == 'string' || typeof (requirements) == 'function') {
            requirements = [requirements];
          }

          if (!(requirements instanceof Array)) {
            throw 'requirements must be a function, string or an array(existing of strings or functions) to be validated for access';
          }

          return Enumerable.From(requirements).All(function ($) {
            if (typeof ($) == "string") {
              return Diract.user.hasFunctionality($);
            }

            if (typeof ($) == "function") {
              return $();
            }

            return false;
          });
        };

        Diract.IfAllowed = function (functionalities, value) {
            if (Diract.user.hasFunctionality(functionalities)) {
                return value;
            } else {
                return null;
            }
        };

        Ext.apply(Concentrator.user, Diract.user);
    }
}