/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ui.FormulaBox = (function () {

  var formulaBox = Ext.extend(Ext.Panel, {

    border: false,

    /**
    ctr
    */
    constructor: function (config) {
      Ext.apply(this, config);
      this.areaConfig = config;

      this.initLayout();

      Concentrator.ui.FormulaBox.superclass.constructor.call(this, config);
    },

    /**
    Performs the calculation
    */
    doCalculation: function (field, isProduct) {
      var areaValue,
      productValue,
      self = this;

      if (isProduct) {
        productValue = field.getValue();
        areaValue = this.formulaArea.getValue();
      }
      else {
        areaValue = field.getValue();
        productValue = this.productSearchField.getValue();
      }


      if (areaValue == '' || productValue == '') return;

      Diract.silent_request({
        url: Concentrator.route('CalculatePrice', 'ContentPrice'),
        params: {
          productID: productValue,
          connectorID: this.connectorID,
          formula: areaValue
        },
        success: function (data) {
          //show the value in some way
          self.syncResultField(data.calculatedPrice, data.price);
        }
      });
    },

    /**
    Adds a field to test the formula 
    */
    getTestEditor: function () {

      var self = this;
      this.productSearchField = new Diract.ui.SearchBox({
        valueField: 'ProductID',
        fieldLabel: 'Product',
        displayField: 'ProductDescription',
        allowBlank: true,
        searchUrl: Concentrator.route('Search', 'Product'),
        listeners: {
          'change': (function (field, newValue, oldValue) {
            self.doCalculation(field, true);
          }).createDelegate(this)
        }
      });

      this.ed = new Ext.form.FieldSet({
        labelWidth: 75,
        title: 'Testing',
        width: 350,
        collapsible: true,
        style: 'clear:both',
        items: [this.productSearchField]
      });

      return this.ed;

    },

    /**
    Returns the extjs label object that displays the resulting value
    */
    syncResultField: function (calculatedPrice, price) {
      if(this.resultField)
        this.ed.remove(this.resultField);

      this.resultField = new Ext.form.Label({
        html: '<span id="test-price-value-label" style="font-weight:bold"/>&#8364;' + calculatedPrice + ' -> &#8364;' + price + '</span>'
      });

      this.ed.add(this.resultField);
      this.doLayout();
      this.resultField.el.highlight("19FF19", { attr: 'color', duration: 2 });    
      
    },

    /**
    Initializes the layout
    */
    initLayout: function () {
      var self = this;

      var helper = new Ext.Button({
        border: false,
        height: 16,
        width: 16,
        tooltip: 'View how to format the calculations',
        style: "border:none;display:inline;float:left;margin-left:2px",
        icon: Concentrator.content('Content/images/help.png'),
        handler: function () {
          self.showHelp();
        }
      });

      var config = {
        name: 'calculation',
        width: 250,
        maskRe: new RegExp('[x<>=&|0-9,m* ]'),
        vtype: 'priceRoundingFormula',
        style: 'float:left;display:inline',
        listeners: {
          'valid': (function (field) {
            this.doCalculation(field);
          }).createDelegate(this)
        }
      };

      this.formulaArea = new Ext.form.TextArea(config);

      Ext.apply(config, this.areaConfig);
      this.items = [this.formulaArea, helper, this.getTestEditor()];
    },

    /**
    Shows help in a new window  
    */
    showHelp: function () {
      var w = new Ext.Window({
        title: 'Help on calculations',
        width: 668,
        layout: 'fit',
        height: 400,
        items: [
          {
            padding: 4,
            html: this.html || '<div id="calculation-wiki">' +
                  '<h3 style="margin-bottom: 3px">' +
                    'Allowed symbols</h3>' +
                  '<div style="margin:0 0 3px 5px">' +
                  '<p>' +
                    'The following symbols are allowed in the calculations field:</p>' +
                  '<p>' +
                    '<h5 style="margin-bottom: 3px">' +
                      'Comparison operators</h5>' +
                    '<span style="font-style: italic; font-weight: bold;">&lt;, &gt; , =&lt;, &gt;=, ==, &lt;&gt;</span><span> - Less ' +
                      'than, greater than, less than or equal to, greater than or equal to, equal to, not equal to</span>' +
                  '</p>' +
                  '<p>' +
                    '<h5 style="margin-bottom: 3px">' +
                      'Conditional operators</h5>' +
                    '<span style="font-weight: bold;">&&, ||</span><span> - AND, OR </span>' +
                  '</p>' +
                  '<p>' +
                    '<h5 style="margin-bottom: 3px">' +
                      'Value (will be replaced with value)</h5>' +
                    '<span style="font-style: italic; font-weight: bold;">x</span><span> - The "x" will be replaced with the actual value for comparison</span>' +

                  '</p>' +

                  '<p>' +
                    '<h5 style="margin-bottom: 3px">' +
                      'Margin (will be replaced with value)</h5>' +
                  '<span style="font-style: italic; font-weight: bold;">m</span><span> - The "m" will be replaced with the margin for comparison</span>' +
                  '</p>' +


                  '</div>' +
                  '<h3 style="margin-bottom: 3px">' +
                    'Example calculations' +
                  '</h3>' +
                  '<p><span style="font-style:italic; font-weight: bold;">x &gt; *,49 = ,98</span> - When the value is greater than .49, the result is rounded to .98 </p>' +
                  '<p><span style="font-style:italic; font-weight: bold;">m &lt; 10 && m &gt; 5 = ,99</span> - When the value is smaller than 10% and bigger than 5%, the result is rounded to .95 </p>' +
                '</div>'

          }
        ]
      });
      w.show();
    }

  });
  return formulaBox;
})();

Ext.reg('formula', Concentrator.ui.FormulaBox);