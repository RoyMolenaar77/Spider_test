Diract.renderers = {

  renderHelper: function(store, value, displayProperty) {
    if (value && value.toString().length > 0) {
      return store.getById(value).get(displayProperty);
    }
    else
      return '';
  }


};