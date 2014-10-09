/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ui.ProductMediaViewer = (function () {
  var dataViewer = Ext.extend(Ext.Panel, {
    cls: 'media-view-container',
    autoScroll: true,
    title: 'Media viewer',
    padding: 10,

    constructor: function (config) {
      Ext.apply(this, config);

      this.mediaDetailsPanel = this.masterPanel.items.items[0];
      this.mediaImagePanel = this.masterPanel.items.items[1];

      this.initTbars();
      Concentrator.ui.ProductMediaViewer.superclass.constructor.call(this, config);
      this.initDataView();

      this.doLayout();

    },
    initTbars: function () {
      var tb = new Ext.Toolbar({
        items: [
        {
          xtype: 'button',
          iconCls: 'add',
          text: 'Add new media',
          handler: (function () {
            this.getNewRecordWindow();
          }).createDelegate(this)
        },
        'Vendor: ',
          {
            xtype: 'combo',
            fieldLabel: 'Vendor',
            emptyText: 'select a vendor...',
            store: this.getStore(),
            valueField: 'VendorID',
            mode: 'local',
            displayField: 'Name',
            typeAhead: true,
            clearFilterOnReset: true,
            listeners: {

              // Calls the function queryVendor, which in turn filters by vendor
              'change': (function (combo, records, index) {
                this.queryVendor(records, index);
              }).createDelegate(this),

              // Removes all the current filters
              'expand': function (combo, records, index) {
                combo.store.clearFilter();
                combo.store.reload();
              }
            }

          },

           {
             xtype: 'button', text: 'Copy from vendor', iconCls: 'merge',
             handler: (function () {
               var exStore = new Ext.data.JsonStore({
                 url: Concentrator.route('GetByProduct', 'Vendor'),
                 baseParams: { productID: this.productID, includeConcentratorVendor: false },
                 root: 'results',
                 idProperty: 'VendorID',
                 fields: ['VendorName', 'VendorID']
               });

               var inf = new Ext.form.ComboBox({
                 displayField: 'VendorName',
                 valueField: 'VendorID',
                 triggerAction: 'all',
                 editable: false,
                 hiddenName: 'vendorID',
                 name: 'vendorID',
                 fieldLabel: 'Vendor',
                 width: 170,
                 store: exStore
               });

               var wind = new Diract.ui.FormWindow({
                 title: 'Copy information to Concentrator',
                 url: Concentrator.route('Copy', 'ProductDescription'),
                 items: [inf,
                         new Ext.form.ComboBox({
                           xtype: 'combobox',
                           store: Concentrator.stores.languages,
                           displayField: 'Name',
                           valueField: 'ID',
                           triggerAction: 'all',
                           fieldLabel: 'Language',
                           hiddenName: 'languageID',
                           editable: false,
                           width: 170,
                           typeAhead: false,
                           value: Concentrator.user.languageID
                         }), { xtype: 'hidden', name: 'productID', value: this.productID}],
                 width: 320,
                 height: 150,
                 success: function () {
                   wind.destroy();
                 }
               });

               wind.show();
             }).createDelegate(this)
           }, {
             xtype: 'button',
             iconCls: 'download',
             text: 'Download',
             handler: (function () {
               if (this.mediaPath || this.mediaUrl) {
                 window.location = Concentrator.route('Download', 'ProductMedia', { path: this.mediaPath ? this.mediaPath : this.mediaUrl });
               } else {
                 Ext.Msg.alert('Error', 'No media selected');
               }
             }).createDelegate(this)
           }
        ]
      });

      this.tbar = tb;
    },

    queryVendor: function (rec, index) {

      this.storeInst.filterBy(function (recd) {
        if (recd.get('VendorID') == rec || rec == '')
          return true;
        return false;
      });

    },
    getNewRecordWindow: function () {
      var self = this;

      var uploadField = new Ext.ux.form.FileUploadField({
        emptyText: 'Select media',
        name: 'MediaPath',
        hiddenName: 'MediaPath',
        fieldLabel: 'Media',
        buttonText: '',
        width: 200,
        buttonCfg: { iconCls: 'upload-icon' },
        allowBlank: true
      });

      var wind = new Diract.ui.FormWindow({
        url: Concentrator.route('Create', 'ProductMedia'),
        title: 'Add new product media',
        buttonText: 'Create product media',
        cancelButton: true,
        height: 500,
        fileUpload: true,
        width: 400,
        items: [
          {
            xtype: 'hidden',
            name: 'productID',
            value: this.productID
          },
          {
            xtype: 'hidden',
            name: 'isSearched',
            value: this.isSearched
          },
          {
            xtype: 'radio',
            fieldLabel: 'File',
            name: 'radioButton',
            checked: true,
            listeners: {
              check: function (box, isChecked) {
                var ob = uploadField;
                if (isChecked) {

                  ob.allowBlank = false;
                  ob.enable();

                } else {

                  ob.allowBlank = true;
                  ob.disable();
                }
              }
            }
          },
          {
            xtype: 'radio',
            fieldLabel: 'Url',
            name: 'radioButton',
            listeners: {
              check: function (box, isChecked) {
                var ob = Ext.getCmp('urlField');
                if (isChecked) {
                  ob.allowBlank = false;
                  ob.enable();

                  //                  this.wind.items.items[0].items.items[5].allowBlank = false;
                  //                  this.wind.items.items[0].items.items[5].enable();
                } else {
                  ob.allowBlank = true;
                  ob.disable();
                  //                  this.wind.items.items[0].items.items[5].allowBlank = true;
                  //                  this.wind.items.items[0].items.items[5].disable();
                }
              }
            }
          },
          {
            xtype: 'vendor',
            fieldLabel: 'Vendor',
            width: 200,
            allowBlank: false
          },
          {
            xtype: 'mediatype',
            fieldLabel: 'Media Type',
            name: 'typeID',
            allowBlank: false
          },
          {
            xtype: 'textfield',
            hiddenName: 'MediaUrl',
            id: 'urlField',
            name: 'mediaurl',
            width: 200,
            fieldLabel: 'Media Url',
            allowBlank: true,
            disabled: true
          },
          uploadField,
          {
            xtype: 'textfield',
            fieldLabel: 'Description',
            name: 'Description',
            allowBlank: true,
            width: 200
          }

        ],
        success: function () {

          Diract.ui.FormWindow.superclass.destroy.call(wind);
          self.storeInst.reload();
        }
      }).show();


    },
    initDataView: function () {
      var self = this;
      if (!this.dataView)
        this.dataView = new Ext.DataView({
          store: this.getStore(),
          tpl: this.getTemplate(),
          //id: 'items',
          listeners: {
            'click': (function (view, index, node, e) {

              this.getDetails(view, index, node, e);

              this.mediaPath = new Ext.Element(node).getAttribute('mediaPath');
              this.mediaUrl = new Ext.Element(node).getAttribute('mediaUrl');

              var el = new Ext.Element(e.target);
              var mediaID = el.getAttribute('mediaid');
              var vendorItemNumber = el.getAttribute('vendorItemNumber');

              if (mediaID) {
                //remove was clicked
                Ext.Msg.confirm("Delete product media", "Are you sure you want to delete the product media?", function (answer) {
                  if (answer === "yes") {

                    Ext.Msg.confirm("Delete sku media", "Do you also want to delete the media for the skus", function (answer) {
                      if (answer == "yes")
                        self.removeMedia(mediaID, el, true);
                      else
                        if (answer == "yes")
                          self.removeMedia(mediaID, el);
                    }, this);
                  } else {
                    return;
                  }
                }, this);

              } else if (vendorItemNumber) {

                var isOn = el.hasClass('add');
                var message = "Are you sure you want to " + (isOn ? "hide" : "show") + " this color?";               

                Ext.Msg.confirm("Hide color from website", message, function (answer) {
                  if (answer == "no") return;

                  var isOn = el.hasClass('add');
                  //make request to update product
                  Diract.request({
                    url: Concentrator.route('UpdateProductVisibility', 'Product'),
                    params: {
                      vendorItemNumber: vendorItemNumber,
                      Visible: !isOn
                    },
                    success: function () {
                      jQuery(view.el.dom).find('[vendorItemNumber="' + vendorItemNumber + '"]').each(function (index) {
                        if (isOn) //switch off
                          jQuery(this).removeClass('add').addClass('delete');
                        else
                          jQuery(this).removeClass('delete').addClass('add');
                      });
                    }
                  });

                }, this);
              }
              e.stopPropagation();
            }).createDelegate(this),
            'mouseenter': (function (view, index, node, e) {
              var el = new Ext.Element(node);
              el.child('.media-toolbox').setStyle('display', 'inline');
              el.child('.enable-product').setStyle('background-color', '#efefef');


            }).createDelegate(this),
            'mouseleave': (function (view, index, node, e) {
              var el = new Ext.Element(node);
              el.child('.media-toolbox').setStyle('display', 'none');
              el.child('.enable-product').setStyle('background-color', 'white');
            }).createDelegate(this),
            'afterrender': (function (view) {
            }).createDelegate(this)


          },
          itemSelector: 'li.thumb-wrap',
          overClass: 'x-view-over',
          emptyText: 'No media',
          singleSelect: true,
          autoScroll: true
        });
      this.add(this.dataView);
    },

    removeMedia: function (mediaID, button, deleteChildrenMedia) {

      Diract.request({
        url: Concentrator.route('Delete', 'ProductMedia'),
        params: { mediaID: mediaID, isSearched: this.isSearched, deleteChildren: deleteChildrenMedia || false },
        success: (function () {
          button.parent('.thumb-wrap').remove();

        }).createDelegate(this)
      });
    },

    getDetails: function (view, index, node, e) {

      if (!this.mediaDetailsPanel) {
        return;
      }

      var counter = 0;
      var storeCounter = 0;
      var storeIndex = 0;
      var dataIndex = 0;
      var dataCounter = 0;
      var dataCounterInner = 0;
      var stop = false;

      for (var i = 0; i < view.store.getCount(); i++) {
        dataCounterInner = 0;

        for (var j = 0; j < view.store.getAt(i).json.Media.length; j++) {
          if (dataCounter == index) {
            dataIndex = dataCounterInner;
            storeIndex = storeCounter;
            stop = true;
            break;
          }
          dataCounterInner++;
          dataCounter++;
        }

        if (stop) break;
        storeCounter++;
      }

      var mediaData = view.store.getAt(storeIndex).json;

      var form = new Diract.ui.Form({
        border: false,
        url: Concentrator.route('Update', 'ProductMedia'),
        items: [
                    {
                      xtype: 'hidden',
                      name: 'MediaID',
                      value: mediaData.Media[dataCounterInner].MediaID
                    },
                    {
                      xtype: 'hidden',
                      name: 'sequence_old',
                      value: mediaData.Media[dataCounterInner].Sequence
                    },
                    {
                      xtype: 'hidden',
                      name: 'ProductID',
                      value: this.productID
                    },
                    {
                      xtype: 'hidden',
                      name: 'isSearched',
                      value: this.isSearched
                    },
                    {
                      xtype: 'numberfield',
                      fieldLabel: 'Sequence',
                      width: 160,
                      name: 'sequence_new',
                      value: mediaData.Media[dataCounterInner].Sequence,
                      emptyText: 'Enter sequence...'
                    },
                    {
                      xtype: 'hidden',
                      name: 'VendorID',
                      value: mediaData.VendorID
                    },
                    {
                      xtype: 'textfield',
                      width: 160,
                      fieldLabel: 'Vendor',
                      value: mediaData.Name,
                      readOnly: true
                    },
                    {
                      xtype: 'hidden',
                      name: 'TypeID_Old',
                      value: mediaData.Media[dataCounterInner].TypeID
                    },
                    {
                      xtype: 'mediatype',
                      width: 160,
                      value: mediaData.Media[dataCounterInner].TypeID,
                      fieldLabel: 'Media type',
                      name: 'TypeID'
                    },
                    {
                      xtype: 'textfield',
                      width: 160,
                      name: 'MediaPath',
                      value: mediaData.Media[dataCounterInner].MediaPath ? mediaData.Media[dataCounterInner].MediaPath : '',
                      fieldLabel: 'Path',
                      readOnly: false
                    },
                     {
                       xtype: 'textfield',
                       width: 160,
                       name: 'MediaUrl',
                       value: mediaData.Media[dataCounterInner].MediaUrl ? mediaData.Media[dataCounterInner].MediaUrl : '',
                       fieldLabel: 'Url',
                       readOnly: false
                     },
                     {
                       xtype: 'textfield',
                       width: 160,
                       name: 'Size',
                       value: mediaData.Media[dataCounterInner].Size ? mediaData.Media[dataCounterInner].Size : '',
                       fieldLabel: 'Size',
                       readOnly: false
                     },
                     {
                       xtype: 'textfield',
                       width: 160,
                       name: 'Resolution',
                       value: mediaData.Media[dataCounterInner].Resolution ? mediaData.Media[dataCounterInner].Resolution : '',
                       fieldLabel: 'Resolution',
                       readOnly: false
                     },

                    {
                      xtype: 'textfield',
                      width: 160,
                      name: 'Description',
                      value: mediaData.Media[dataCounterInner].Description ? mediaData.Media[dataCounterInner].Description : '',
                      readOnly: false,
                      fieldLabel: 'Description'
                    }
        ]
        ,

        success: (function () {
          this.storeInst.load();
          this.dataView.refresh();
        }).createDelegate(this)
      });

      this.mediaDetailsPanel.removeAll();
      this.mediaDetailsPanel.add(form);
      this.mediaDetailsPanel.doLayout();

      var imagePath = mediaData.Media[dataCounterInner].MediaPath;

      var box = new Ext.BoxComponent({
        autoEl: {
          tag: 'img',
          src: Concentrator.GetImageUrl(imagePath, 256, 256)
        }
      });


      this.mediaImagePanel.removeAll();
      this.mediaImagePanel.add(box);
      this.mediaImagePanel.doLayout();
    },
    getTemplate: function () {
      var tp = new Ext.XTemplate(
        '<tpl for=".">',
        '<div class="vendor-collection">',
        '<p class="ul-caption">{Name}</p>',
        '<ul class="media-collection-ul" id="{VendorID}">',
          '<tpl for="Media">',
            '<li id="{values.MediaID}" class="thumb-wrap" mediaPath="{values.MediaPath}" mediaUrl="{values.MediaUrl}">',
              '<span style="font-weight:bold">{values.VendorItemNumber}</span>',
              '<div class="thumb">',
                '<img src="{[this.getThumbSrc(values.Type, values.MediaUrl,values.MediaPath, true)]}"/>',
                '<div class="media-toolbox">',
                '<button class="media-thumb-toolbox delete x-btn" mediaid="{values.MediaID}"></button>',
                '</div>',
              '</div>',
              '<button class="{[this.getProductEnabledClass(values.Visible)]}" style="width:18px;height:18px;background-position: 0 center;padding:1px;border:none; background-color:white; margin-left:20px" vendorItemNumber="{values.VendorItemNumber}"></button>',
            '</li>',
          '</tpl>',
        '</ul>',
        '</div>',
        '</tpl>',

          {
            getProductEnabledClass: function (visible) {
              if (visible) return 'add x-btn enable-product';
              else return 'delete x-btn enable-product';
            },
            getThumbSrc: function (type, src, path, restrict) {
              switch (type) {
                case 'Image':
                  if (path)
                    if (restrict) {
                      return Concentrator.GetImageUrl(path, 64, 64);
                    } else {
                      return Concentrator.GetImageUrl(path);
                    }
                  else
                    return Concentrator.ResizeImageFromUrl(src, 64, 64);
                  break;
                case 'Video':
                  return Concentrator.content('Content/images/Icons/Video.png');
                  break;
                case 'Audio':
                  return Concentrator.content('Content/images/Icons/Audio.png');
                  break;
                default:
                  return Concentrator.GetImageUrl(path, 64, 64, null, src);
                  break;
              }
            }
          }
      );

      tp.compile();

      return tp;
    },
    getStore: function () {
      var that = this;
      if (!this.storeInst) {
        this.storeInst = new Ext.data.JsonStore({
          url: Concentrator.route('GetMedia', 'ProductMedia'),
          baseParams: { productID: this.productID },
          root: 'results',
          idProperty: ['VendorID', 'Sequence', 'TypeID'],
          fields: ['Name', 'VendorID', 'Media']
          //listeners: {
          //  'load': function (store) {
          //    var liCollection = [],
          //        ulChecks = jQuery('<ul style="clear:both"></ul>');

          //    jQuery('.thumb').each(function (index) {
          //      var check = jQuery('<input type="checkbox" />');

          //      liCollection.push(check);
          //      check.change(function () {
          //        alert('Check changed for media : ' + jQuery(this).parent('li:first').attr('id'));
          //      });

          //      var liCheck = jQuery('<li style="display:inline; width:83px; text-align:center; padding-left:30px; padding-right:30px"></li>')
          //      liCheck.append(check);


          //      ulChecks.append(liCheck);
          //    });

          //    jQuery('.vendor-collection').append(ulChecks);
          //  }
          //}
        });


        this.storeInst.load();
      }
      return this.storeInst;
    },

    getMediaTypeStore: function () {
      if (!this.mediaTypeStoreInst) {
        this.mediaTypeStoreInst = new Ext.data.JsonStore({
          url: Concentrator.route('GetMediaTypes', 'ProductMedia'),
          baseParams: { productID: this.productID },
          root: 'results',
          idProperty: 'TypeID',
          fields: ['TypeID', 'Type']
        });

        this.mediaTypeStoreInst.load();
      }
      return this.mediaTypeStoreInst;
    },
    applyFilter: function (vendor, media) {

    }

  });

  return dataViewer;
})();


Ext.reg('mediaViewer', Concentrator.ui.ProductMediaViewer);