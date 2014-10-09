Concentrator.renderers = Ext.apply({}, {

  connectorType: function (value, metadata, record) {
    return Concentrator.renderers.renderHelper(Concentrator.stores.connectorTypes, value, "Description");
  },
  language: function (value, metadata, record) {
    return Concentrator.renderers.renderHelper(Concentrator.stores.languages, value, "Name");
  },
  productGroup: function (value, metadata, record) {
    return Concentrator.renderers.renderHelper(Concentrator.stores.productGroups, value, "Name");
  },
  orderLineStatus: function (value, metadata, record) {
    return Concentrator.renderers.renderHelper(Concentrator.stores.orderLineStates, value, "Name");
  },
  getName: function (store, fieldname) {
    return function (val, m, record) {
      var storeRec = Concentrator.stores[store].getById(record.get(this.dataIndex));
      if (storeRec != null) {
        var name = storeRec.get(fieldname);
        return name;
      } else {
        return "";
      }
    }
  },
  percentage: function () {
    return function (val, m, record) {
      return Ext.util.Format.number(val, '0.00%');
    }
  },
  euro: function () {
    return function (val, m, record) {
      if (val && val != '') {
        return '&#8364 ' + Ext.util.Format.number(val, '0.00,00');
      }
      return val;
    }
  },

  
  field: function (store, field) {


    return function (val, m, record) {
      if (val != "" && val != undefined) {
        if (Concentrator.stores[store].getById(val) != undefined) {
          return Concentrator.stores[store].getById(val).get(field);
        }
      }
      else
        return "";
    }
  }


}, Diract.renderers);