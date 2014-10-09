Concentrator.ProductBrowserFactory = function (config) {
  var that = this;
  

  Diract.silent_request({
    url: Concentrator.route('GetProductDetails', 'Product'),
    params: {
      productID: config.productID,
      isSearched: config.isSearched
    },
    success: (function (data) {
      var product = data.product;
      product.AveragePrice = Ext.util.Format.round(product.AveragePrice, 2);
      var title = product.ProductName || product.ShortContentDescription || product.VendorItemNumber;
      title = Ext.util.Format.ellipsis(title, 15);

      Concentrator.ViewInstance.open(config.productID, null, new Concentrator.ui.ProductBrowser({

        productID: config.productID,
        product: product,
        isSearched: config.isSearched,
        title: title,
        closable: true
      }), { text: title, iconCls: 'node' });

    }).createDelegate(this)
  });
}