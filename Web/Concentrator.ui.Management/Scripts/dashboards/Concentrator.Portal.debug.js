/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.Portal = (function () {
  //extend Ext.ux.Portal

  var portal = Ext.extend(Ext.ux.Portal, {
    region: this.region || 'center',
    autoScroll: this.autoScroll || true,
    autoDestroy: true,
    displayedCustomFilters: [],
    customFilterValues: {},
    border: false,
    listeners: {
      'drop': (function (e) {
        e.portal.saveLayout();
      }).createDelegate(this),
      'afterrender': function (comp, layout) {     

        var dropTarget = new Ext.dd.DropTarget(comp.body.dom, {
          ddGroup: 'testDD',
          notifyDrop: (function (source, e, data) {

            Ext.each(data.selections, function (item) {

              var portletID = item.get('PortletID');
              var name = item.get('Name');

              var x = source.lastPageX;
              var y = source.lastPageY;


              var col = x >= comp.left.getBox().width ? 1 : 0;
              var colObj = col == 0 ? comp.left : comp.right;

              var row = 0;
              var height = 0;

              for (var i = 0; i < colObj.items.length; i++) {

                var item = colObj.items.get(i);

                height += item.getBox().height;
                if (y <= height) {
                  row = i;
                  break;
                }
                row++;
              }

              var portlet = {
                PortletID: portletID,
                Name: name,
                Column: col,
                Row: row
              };

              var buildPortlet = comp._getPortlet(portlet);

              if (buildPortlet != null) {

                Diract.silent_request({
                  url: Concentrator.route("AddPortlet", "Portal"),
                  params: {
                    portalID: comp.portalID,
                    portletID: portlet.PortletID,
                    column: col,
                    row: row
                  },
                  success: (function () {
                    var columnToInsertInto = comp.items.get(col);

                    columnToInsertInto.insert(row, buildPortlet);

                    Ext.each(columnToInsertInto.items.items, function (it) {
                      if (it.PortletID != portlet.PortletID && it.row >= row) {
                        it.row++;
                      }
                    })

                    var selected = source.grid.getSelectionModel().getSelected();
                    source.grid.store.remove(selected);

                    columnToInsertInto.doLayout();

                    this.saveLayout()

                  }).createDelegate(comp)
                });
              }

            }, this);

          }).createDelegate(this)
        });
      }
    },
    saveLayout: function (successAction) {
      var that = this;
      var portlets = [];

      Ext.each(this.items.items, function (column, colIndex) {

        Ext.each(column.items.items, function (portlet, rowIndex) {
          portlets.push({
            portletID: portlet.portletID,
            column: colIndex,
            row: rowIndex
          });

        });
      });

      var win = Ext.getCmp('widgetWindow');
      if (win) {
        win.destroy();
      }

      Diract.silent_request({
        url: Concentrator.route('SaveLayout', 'Portal'),
        params: {
          portalID: this.portalID,
          portletsSerializedJson: Ext.encode(portlets)
        },
        success: function (that) {
          if (successAction)
            successAction();
        }
      });
    },

    id: this.portalID + Math.floor(Math.random() * 44),

    tools: [
      {
        id: 'refresh',
        handler: (function (evt, o, e) {
          e.refresh();
        })
      },
      {
        id: 'plus',
        handler: function (evt, o, e) {

          var data = [];

          Ext.each(e.activePortlets, function (item) {

            data.push(item.portletID)

          });

          var gridPanel = new Diract.ui.Grid({
            ddGroup: 'testDD',
            url: Concentrator.route("GetPortlets", "Portal"),
            params: {
              portletIDs: data
            },
            border: false,
            enableDragDrop: true,
            permissions: {
              list: 'GetPortal',
              update: 'UpdatePortal'
            },
            structure: [
                { dataIndex: 'PortletID', type: 'int' },
                { dataIndex: 'Name', type: 'string' },
                { dataIndex: 'Title', header: 'Name', type: 'string', width: 50 },
                { dataIndex: 'Description', header: 'Description', type: 'string' }
            ],
            hideHeaders: false
          });

          var window = new Ext.Window({
            title: 'Widgets',
            height: 500,
            width: 520,
            id: 'widgetWindow',
            layout: 'fit',
            items: [
              gridPanel
            ]
          });

          window.show();
        }
      }
    ],
    initComponent: function () {
      this.tbar = this.getToolbar();

      Concentrator.Portal.superclass.initComponent.call(this);
      this.activePortlets = new Array();
      this._fetchWidgetData();
    },

    getToolbar: function () {
      return new Ext.Toolbar({
        allowOtherMenus: true,
        autoShow: false,
        floating: false,
        ignoreParentClicks: true

      });
    },

    refresh: function (params) {

      var portal = this;
      Ext.each(this.activePortlets, function (portlet) {
        var p = {};

        if (params) {
          for (var key in params) p[key] = params[key];
        }
        var customFilter = portal.customFilterValues[portlet.portletID];
        if (customFilter) Ext.apply(p, customFilter.getValue());

        portlet.refresh(p);
      });
    },
    removePortlet: function () {

    },
    _fetchWidgetData: function () {
      var self = this;
      Diract.silent_request({
        url: Concentrator.route('Get', 'Portal'),
        params: {
          id: this.portalID, //'main',
          name: this.title
        },
        success: function (data) {
          self._processPortal(data.portal[0]);
          id = data.id
        }
      });
    },
    _processPortal: function (portal) {
      var lft;
      var rgh;
      Ext.each(portal.columns, function (col) {
        if (col.column === 0)
          lft = col;
        else
          rgh = col;
      });

      this.addLeftColumn(lft);
      this.addRightColumn(rgh);

      this.doLayout();
    },

    addRightColumn: function (column) {
      this.right = new Ext.ux.PortalColumn({
        columnWidth: .50,
        style: 'padding:10px 10px 10px 10px'
      });

      if (column) {
        Ext.each(column.portlets, function (w) {
          this.right.add(this._getPortlet(w));

        }, this);
      }
      this.add([this.right]);
    },

    addLeftColumn: function (column) {
      this.left = new Ext.ux.PortalColumn({
        columnWidth: .50,
        style: 'padding:10px 10px 10px 10px'
      });

      if (column) {
        Ext.each(column.portlets, function (w) {
          this.left.add(this._getPortlet(w));
        }, this);
      }
      this.add([this.left]);
    },

    /**
    Creates and returns a portlet
    arguments:
    @portlet : portlet object {PortletID, Column, Row}
    @params  : additional parameters to be applied to the portlet requests
    @
    */
    _getPortlet: function (portlet, params, portletConfig) {

      var config = this._getPortletConfig(portlet, params, portletConfig);

      if (portletConfig) Ext.apply(config, portletConfig);

      var portlet = new Concentrator.Portlets[portlet.Name](config);

      this.activePortlets.push(portlet);

      return portlet;
    },

    _getPortletConfig: function (portlet, params, portletConfig) {
      var config = {
        portletID: portlet.PortletID,
        portal: this,
        column: portlet.Column,
        row: portlet.Row,
        params: params || {},
        portalID: this.portalID,
        listeners: {
          'filterUpdate': (function (portlet, filter) {
            this.customFilterValues[portlet.portletID] = filter;
            //refresh portal
            this.refresh();

          }).createDelegate(this),
          'registerCustomFilter': (function (portlet, filter) {
            this.customFilterValues[portlet.portletID] = filter;
          }).createDelegate(this)
        }
      };

      var portal = this;
      Ext.apply(config, {
        settingsHandler: function (evt, btn, component) {

          var toolbar = null,
              filters = component.getCustomFilters(),
              i = 0,
              count = 0;

          if (!filters) return;

          toolbar = portal.getTopToolbar();
          if (!toolbar) throw "There must be a toolbar defined for this product";

          if (!component.filterEnabled) {
            component.filterEnabled = true;
          } else {
            component.filterEnabled = false;


            //remove those items
            Ext.each(portal.displayedCustomFilters, function (it) {
              toolbar.remove(it);
            });

            toolbar.doLayout();
            return;
          }


          var itCount = toolbar.items.getCount();
          count = itCount == 0 ? 0 : itCount - 2;


          var separator = new Ext.Toolbar.Separator({
            height: 40
          });

          //add separator
          toolbar.insert(count, separator);
          portal.displayedCustomFilters.push(separator);
          count++;

          //attach items
          var position = count;
          for (i = 0; i < filters.length; i++) {
            var f = filters[i];
            toolbar.insert(position, f);
            portal.displayedCustomFilters.push(f);
            position++;
          }

          toolbar.doLayout();
        }
      });
      return config;
    }

  });
  return portal;
})();

Ext.reg('portal', Concentrator.Portal);