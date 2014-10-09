/// <reference path="~/Scripts/ext/ext-base-debug.js" />
/// <reference path="~/Scripts/ext/ext-all-debug.js" />
/// <reference path="~/Scripts/linq.js" />
Ext.ns("Diract");
(function () {

  var ItemsAuthority = {
    recurseExcludeUnauthorizedItems: function(items) {
      var k, item;
      for (k in items) {
        item = items[k];
        if (!!item['items'] && item['items'] instanceof Array) {
          item['items'] = Diract.Authorization.excludeUnauthorizedItems(item['items']);
        }
      }
    },
    removePrecedingDashes: function (items) {
      // remove preceding '-' so the menu's won't show preceding horizontal lines
      var max = 1000; // prevent infinite loop
      while ((max--) > 0 && items.length > 0 && items.indexOf('-') == 0) {
        items.shift();
      }
    },
    removeTrailingDashes: function (items) {
      // remove trailing '-' so the menu's won't show trailing horizontal lines
      var max = 1000; // prevent infinite loop
      while ((max--) > 0 && items.length > 0 && items.lastIndexOf('-') == items.length - 1) {
        items.pop();
      }
    },
    excludeUnauthorizedItems: function(items) {
      if (items instanceof Array) {
        items = Enumerable.From(items).Where("Diract.user.isAuthorizedToAccess($)").ToArray();
      } else {
        throw new "parameter items must be an array";
      }

      ItemsAuthority.removePrecedingDashes(items);
      ItemsAuthority.removeTrailingDashes(items);
      ItemsAuthority.recurseExcludeUnauthorizedItems(items);

      return items;
    }
  };

Diract.Authorization = {
  excludeUnauthorizedItems: function (items) {
    return ItemsAuthority.excludeUnauthorizedItems(items);
  }
};
})();


Diract.ExtFactory = {
  create: function DiractExtFactoryCreate(extType, overrides) {
    var newType = function DiractExtFactoryCreatedType(config) {
      var self = this;

      self.parentConstructor = extType.prototype.constructor;

      config.items = Diract.Authorization.excludeUnauthorizedItems(config.items);

      self.parentConstructor(config);
    };

    newType.prototype = extType.prototype;

    return newType;
  }
};

Ext.ns("Diract.Ext");
Diract.Ext.TabPanel = Diract.ExtFactory.create(Ext.TabPanel);

Ext.ns("Diract.Ext.menu");
Diract.Ext.menu.Menu = Diract.ExtFactory.create(Ext.menu.Menu);

Ext.ns("Diract.Ext.form.FormPanel");
Diract.Ext.form.FormPanel = Diract.ExtFactory.create(Ext.form.FormPanel);