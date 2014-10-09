/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ui.BrandMediaViewer = (function () {
  var dataViewer = Ext.extend(Ext.Window, {
    modal: true,
    id: 'media-view',
    autoScroll: true,
    title: 'Media viewer',
    padding: 10,
    width: 600,
    height: 350,
    constructor: function (config) {
      Ext.apply(this, config);
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
        }, '->', {
          xtype: 'button',
          iconCls: 'download',
          text: 'Download',
          handler: (function () {
            if (this.mediaPath || this.mediaUrl) {
              window.location = Concentrator.route('Download', 'BrandMedia', { path: this.mediaPath ? this.mediaPath : this.mediaUrl });
            } else {
              Ext.Msg.alert('Error', 'No media selected');
            }
          }).createDelegate(this)
        }
        //        },

        //        'Vendor: ',
        //          {
        //            xtype: 'combo',
        //            fieldLabel: 'Vendor',
        //            emptyText: 'select a vendor...',
        //            store: null, //this.getStore(),
        //            valueField: 'VendorID',
        //            mode: 'local',
        //            displayField: 'Name',
        //            typeAhead: true,
        //            clearFilterOnReset: true,
        //            listeners: {

        //              // Calls the function queryVendor, which in turn filters by vendor
        //              'change': (function (combo, records, index) {
        //                this.queryVendor(records, index);
        //              }).createDelegate(this),

        //              // Removes all the current filters
        //              'expand': function (combo, records, index) {
        //                combo.store.clearFilter();
        //                combo.store.reload();
        //              }
        //            }

        //          },

        //           { xtype: 'button', text: 'Copy from vendor', iconCls: 'merge',
        //             handler: (function () {
        //               var exStore = new Ext.data.JsonStore({
        //                 url: Concentrator.route('GetByProduct', 'Vendor'),
        //                 baseParams: { productID: this.productID, includeConcentratorVendor: false },
        //                 root: 'results',
        //                 idProperty: 'VendorID',
        //                 fields: ['VendorName', 'VendorID']
        //               });

        //               var inf = new Ext.form.ComboBox({
        //                 displayField: 'VendorName',
        //                 valueField: 'VendorID',
        //                 triggerAction: 'all',
        //                 editable: false,
        //                 hiddenName: 'vendorID',
        //                 name: 'vendorID',
        //                 fieldLabel: 'Vendor',
        //                 width: 170,
        //                 store: exStore
        //               });

        //               var wind = new Diract.ui.FormWindow({
        //                 title: 'Copy information to Concentrator',
        //                 url: Concentrator.route('Copy', 'ProductDescription'),
        //                 items: [inf,
        //                         new Ext.form.ComboBox({
        //                           xtype: 'combobox',
        //                           store: Concentrator.stores.languages,
        //                           displayField: 'Name',
        //                           valueField: 'ID',
        //                           triggerAction: 'all',
        //                           fieldLabel: 'Language',
        //                           hiddenName: 'languageID',
        //                           editable: false,
        //                           width: 170,
        //                           typeAhead: false,
        //                           value: Concentrator.user.languageID
        //                         }), { xtype: 'hidden', name: 'productID', value: this.productID}],
        //                 width: 320,
        //                 height: 150,
        //                 success: function () {
        //                   wind.destroy();
        //                 }
        //               });

        //               wind.show();
        //             }).createDelegate(this)
        //           }
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
        url: Concentrator.route('Create', 'BrandMedia'),
        title: 'Add new brand media',
        buttonText: 'Create brand media',
        cancelButton: true,
        height: 500,
        fileUpload: true,
        width: 400,
        items: [
          {
            xtype: 'hidden',
            name: 'brandID',
            value: this.brandID
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

                  //                  this.wind.items.items[0].items.items[6].allowBlank = false;
                  //                  this.wind.items.items[0].items.items[6].enable();
                } else {

                  ob.allowBlank = true;
                  ob.disable();
                  //                  this.wind.items.items[0].items.items[6].allowBlank = true;
                  //                  this.wind.items.items[0].items.items[6].disable();
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
         uploadField
         

        ],
        success: function () {

          Diract.ui.FormWindow.superclass.destroy.call(wind);
          self.storeInst.reload();

          //          var view = self.dataview;

          //          var delColl = view.getEl().query('.delete');
          //          Ext.each(delColl, function (item) {
          //            var el = new Ext.Element(item);
          //            var self = this;

          //            el.on('click', function (a, d, e) {
          //              debugger;
          //              var ele = new Ext.Element(d);
          //              var mediaID = ele.getAttribute('mediaid');
          //              self.removeMedia(mediaID, ele);
          //            });
          //          });
        }
      }).show();


    },
    initDataView: function () {
      var self = this;
      if (!this.dataView)
        this.dataView = new Ext.DataView({
          store: this.getStore(),
          tpl: this.getTemplate(),
          id: 'items',
          listeners: {
            'click': (function (view, index, node, e) {
              //              this.getDetails(view, index, node, e);

              this.mediaPath = new Ext.Element(node).getAttribute('mediaPath');
              this.mediaUrl = new Ext.Element(node).getAttribute('mediaUrl');
              var el = new Ext.Element(e.target);
              var mediaID = el.getAttribute('mediaid');
              if (mediaID) {
                //remove was clicked
                self.removeMedia(mediaID, el);
              }

            }).createDelegate(this),
            'mouseenter': (function (view, index, node, e) {
              var el = new Ext.Element(node);
              el.child('.media-toolbox').setStyle('display', 'inline');
            }).createDelegate(this),
            'mouseleave': (function (view, index, node, e) {
              var el = new Ext.Element(node);
              el.child('.media-toolbox').setStyle('display', 'none');
            }).createDelegate(this),
            'afterrender': (function (view) {

              //              /** Delete event */
              //              var delColl = view.getEl().query('.delete');
              //              Ext.each(delColl, function (item) {
              //                var el = new Ext.Element(item);
              //                var self = this;
              //                debugger;
              //                el.on('click', function (a, d, e) {

              //                  var ele = new Ext.Element(d);
              //                  var mediaID = ele.getAttribute('mediaid');
              //                  self.removeMedia(mediaID, ele);
              //                });

              //              }, this);

              /** eof delete */

              /** View event */
              //              Ext.each(view.getEl().query('.magnify'), function(item) {
              //                var el = new Ext.Element(item);
              //                var self = this;
              //                el.on('click', function(a, d, e) {
              //                  var ele = new Ext.Element(d);
              //                  var mediaID = ele.getAttribute('mediaid');
              //                  var mediaPath = ele.getAttribute('mediaUrl');
              //                  window.location = mediaPath;
              //                });
              //              }, this);

              /** eof view */

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

    removeMedia: function (mediaID, button) {
      Diract.request({
        url: Concentrator.route('Delete', 'BrandMedia'),
        params: { mediaID: mediaID },
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
        url: Concentrator.route('Update', 'BrandMedia'),
        items: [
                    {
                      xtype: 'hidden',
                      name: 'sequence_old',
                      value: mediaData.Media[dataCounterInner].Sequence
                    },
                    {
                      xtype: 'hidden',
                      name: 'BrandID',
                      value: this.brandID
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
                      name: 'TypeID',
                      value: mediaData.Media[dataCounterInner].TypeID
                    },
                    {
                      xtype: 'textfield',
                      width: 160,
                      value: mediaData.Media[dataCounterInner].Type,
                      readOnly: true,
                      fieldLabel: 'Media type'
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
                     }
                ],
        success: (function () {
          this.storeInst.load();
          this.dataView.refresh();
        }).createDelegate(this)
      });

      this.mediaDetailsPanel.removeAll();
      this.mediaDetailsPanel.add(form);
      this.mediaDetailsPanel.doLayout();
    },
    getTemplate: function () {
      var tp = new Ext.XTemplate(
        '<tpl for=".">',
        '<div class="vendor-collection">',
        '<p class="ul-caption">{Name}</p>',
        '<ul class="media-collection-ul" id="{VendorID}">',
          '<tpl for=".">',
            '<li class="thumb-wrap" mediaPath="{values.MediaPath}" mediaUrl="{values.MediaUrl}">',
              '<div class="thumb">',
                '<img src="{[this.getThumbSrc(values.Type, values.MediaUrl,values.MediaPath, true)]}"/>',
                '<div class="media-toolbox">',
                '<button class="media-thumb-toolbox delete x-btn" mediaid="{values.MediaID}"></button>',
                '</div>',
              '</div>',
            '</li>',
          '</tpl>',
        '</ul>',
        '</div>',
        '</tpl>',

          {
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
      if (!this.storeInst) {
        this.storeInst = new Ext.data.JsonStore({
          url: Concentrator.route('GetMedia', 'BrandMedia'),
          baseParams: { brandID: this.brandID },
          root: 'results',
          idProperty: ['Sequence', 'TypeID'],
          fields: ['MediaUrl', 'MediaPath', 'Size', 'Resolution', 'MediaID']
        });


        this.storeInst.load();
      }
      return this.storeInst;
    },

    getMediaTypeStore: function () {
      if (!this.mediaTypeStoreInst) {
        this.mediaTypeStoreInst = new Ext.data.JsonStore({
          url: Concentrator.route('GetMediaTypes', 'BrandMedia'),
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


Ext.reg('brandmediaViewer', Concentrator.ui.BrandMediaViewer);