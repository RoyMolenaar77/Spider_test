/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />
Concentrator.ExtendedSearchManager = (function () {

  var myBorderLayout = new Ext.layout.BorderLayout;




  var searchManager = Ext.extend(Ext.Panel, {

    title: 'Advanced Search',
    border: false,
    layout: 'border',
    cls: 'plain-border',
    pageSize: 20,
    closable: true,
    style: 'padding: 10px;',
    constructor: function (config) {
      Ext.apply(this, config);

      this.initSearchStore();
      this.initForm();


      Concentrator.ExtendedSearchManager.superclass.constructor.call(this, config);

      //this.doLayout();
    },

    /**
    Initialize paging
    */
    getPagingToolbar: function () {
      this.pagingToolbar = new Ext.PagingToolbar({
        store: this.store,
        pageSize: this.pageSize
      });
      return this.pagingToolbar;
    },

    /**
    Initialize the searching store that will be used
    */
    initSearchStore: function () {
      this.store = new Ext.data.JsonStore({
        url: Concentrator.route('AdvancedSearch', 'Product'),
        idProperty: 'ProductID',
        fields: ['ProductID', 'ProductName', 'ProductDescription', 'MediaPath', 'VendorItemNumber'],
        //autoLoad : false,
        root: 'results',
        listeners: {
          //attach the params dynamically on before load
          'beforeload': (function (store, opt) {

            var query = this.searchBox.getValue(),
                      params = {};

            //add the query
            params.query = query;

            //add the search by
            Ext.apply(params, this.buildSearchByParams());

            //unless the start is still 0 do not specify

            if (!opt.params.start || opt.params.start == 0) {
              params.start = 0;
              params.limit = this.pageSize;
            }
            Ext.apply(opt.params, params);

          }).createDelegate(this),
          'load': (function (store, records, options) {
            //                        if (!this.resultsContainer) {

            //                            this.resultsContainer = this.getResultsContainer();
            //                            this.add(this.resultsContainer);
            //                            this.doLayout();
            //                        }

            this.resultsContainer.removeAll();
            this.setResults(this.resultsContainer, records);
            //this.add(this.getResultsPanel(records));
            //            var that = this;
            //            setTimeout(function() {
            this.resultsContainer.doLayout();
            //            }, 0);
          }).createDelegate(this)
        }
      });

    },

    /**
    Initializes the search form --- search text box, checkboxes for all search-by-entities
    */
    initForm: function () {

      this.searchBox = new Ext.form.TextField({
        emptyText: 'Search...',
        fieldLabel: 'Search for',
        name: 'query',
        listeners: {
          specialkey: (function (f, e) {
            if (e.getKey() == e.ENTER) {
              this.search();
            }
          }).createDelegate(this)
        }
      });

      this.checkBoxGroup = new Ext.form.CheckboxGroup({

        items: [
              { name: 'includeBrands', boxLabel: 'Brands' },
              { name: 'includeProductGroups', boxLabel: 'Product groups' },
              { name: 'includeDescriptions', boxLabel: 'Product descriptions' },
              { name: 'includeIdentifiers', boxLabel: 'Identifiers' }
            ]
      });

      this.resultsContainer = new Ext.Panel({
        title: 'Results',
        borders: true,
        region: 'center',
        autoScroll: true,
        listeners: {
          'afterrender': (function () {

            if (!this.loadingMask) {
              this.loadingMask = new Ext.LoadMask(this.resultsContainer.id, { store: this.store });
            }
          }).createDelegate(this)
        },
        bbar: this.getPagingToolbar()
      });

      this.resultsContainer.setVisible(false);

      this.checkFieldSet = new Ext.form.FieldSet({
        title: 'Search by',
        items: [this.checkBoxGroup]
      });

      this.searchButton = new Ext.Button({
        iconCls: 'magnify',

        handler: (function (button, evt) {
          this.search();
        }).createDelegate(this)
      });

      this.searchBoxContainer = new Ext.Panel({
        layout: 'column',
        items: [this.searchBox, this.searchButton],
        border: false,
        style: 'margin-bottom:10px'
      });

      this.north = new Ext.Panel({
        region: 'north',
        height: 130,
        padding: 10,
        border: false,
        margins: '0 0 5 0',
        items: [this.searchBoxContainer, this.checkFieldSet]
      });

      this.items = [
      //this.searchBox,
      //this.searchButton,
        this.north,
        this.resultsContainer
      ];

    },

    /** 
    Perform the actual search operation
    */
    search: function () {
      var query = this.searchBox.getValue();
      params = {};


      if (!this.resultsContainer.isVisible())
        this.resultsContainer.setVisible(true);

      //add the query
      params.query = query;

      //add the search by
      Ext.apply(params, this.buildSearchByParams());

      params.start = 0;
      params.limit = this.pageSize;

      //somewhere in here ---> add the pagination params      

      //set the params
      //this.store.baseParams = params;      
      this.store.load({ params: params });
    },

    /**
    get the results panel
    */
    getResultsContainer: function (results) {
      //If the panel is not created -- create it

      var resultsCont = new Ext.Panel({
        title: 'Results',
        borders: true,
        region: 'south',
        autoScroll: true,
        height: 500,
        listeners: {
          'afterrender': (function () {

            if (!this.loadingMask) {
              this.loadingMask = new Ext.LoadMask(resultsCont.id, { store: this.store });
            }
          }).createDelegate(this)
        },
        bbar: this.getPagingToolbar()
      });

      return resultsCont;
    },

    /**
    Sets the results
    */
    setResults: function (container, results) {


      if (results.length == 0) {
        container.add({ html: 'No data to display', border: false });
      }

      for (var i = 0; i < results.length; i++) {
        container.add(this.getSingleResultPanel(results[i]));
      }
    },

    getSingleResultPanel: function (result) {
      var name = result.get('ProductName');
      description = result.get('ProductDescription');
      vendorItemNumber = result.get('VendorItemNumber');


      if (!name)
        name = 'ID:' + result.get('ProductID');

      if (!description)
        description = 'N/A';

      if (!vendorItemNumber)
        vendorItemNumber = 'N/A'

      return {
        xtype: 'panel',
        layout: 'border',
        style: 'margin: 2px 0 2px 0',
        padding: 5,
        height: 100,
        tools: [
                {
                  id: 'search',
                  handler: (function () {
                    this.triggerProductBrowser(result.get('ProductID'), Ext.util.Format.ellipsis(name, 20))
                  }).createDelegate(this)
                }
            ],
        defaults: { border: false },
        items: [
        //west element --> image and a link
            {
            region: 'west',
            padding: 5,
            html: '<img src="' + Concentrator.GetImageUrl(result.get('MediaPath'), 65, 65) + '" /><div style="font-size:11px"><a href="' +
                    Concentrator.externalRoute(Concentrator.printFactSheetUrl, { priceTagID: 7, connectorID: 1, printBarcode: true,
                      dataStr: Ext.util.JSON.encode(
                            [{ CustomProductID: 0, ProductID: Number(result.get('ProductID')), Quantity: 1, AdditionalLine: '', PriceOverride: null}])
                    }) + '">Print pdf factsheet</a></div>'

          },
        //center element --> name, description, and such
            {
            region: 'center',
            padding: 5,
            html: '<h1>' + name + '</h1><div style="margin-top:5px"><p>' + description + '</p></div>' + '<h3 style="margin-top: 2px;">' + 
                  'Vendor Item Number' + '</h3> <div style="margin-top:2px"><p>' + vendorItemNumber + '</p></div>'
          }
            ]
      }
    },
    /**
    Collects all selected to-search-by items and returns them
    */
    buildSearchByParams: function () {

      var searchBy = this.checkBoxGroup.getValue();
      var params = {};

      for (var i = 0; i < searchBy.length; i++) {
        var s = searchBy[i];
        params[s.name] = s.getValue();
      }
      return params;
    },
    triggerProductBrowser: function (id, text) {
      var factory = new Concentrator.ProductBrowserFactory({ productID: id });
    }
  });

  return searchManager;
})();

