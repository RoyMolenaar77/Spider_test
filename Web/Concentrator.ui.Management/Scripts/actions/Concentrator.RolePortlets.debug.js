Concentrator.RolePortlets = Ext.extend(Concentrator.BaseAction, {




  getPanel: function () {

    this.RoleSearch = Ext.extend(Diract.ui.SearchField, {
      valueField: 'RoleID',
      displayField: 'RoleName',
      allowBlank: false,
      searchUrl: Concentrator.route('SearchRoles', 'Role')
    });

    Ext.reg('RoleSearch', this.RoleSearch);

    this.PortletSearch = Ext.extend(Diract.ui.SearchField, {
      valueField: 'PortletID',
      displayField: 'PortletName',
      allowBlank: false,
      searchUrl: Concentrator.route('SearchPortlets', 'Portlet')
    });

    Ext.reg('PortletSearch', this.PortletSearch);

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Portlets',
      singularObjectName: 'Portlet Role',
      primaryKey: ['PortletID', 'RoleID'],
      url: Concentrator.route("GetList", "Portlet"),
      newUrl: Concentrator.route("Create", "Portlet"),
      updateUrl: Concentrator.route("Update", "Portlet"),
      deleteUrl: Concentrator.route("Delete", "Portlet"),
      //newformConfig: //link,
      permissions: {
        all: 'Default'
      },
      structure: [
        { dataIndex: 'PortletID', type: 'int', header: 'Portlet Name',
          renderer: function (val, meta, record) {
            return record.get('PortletName');
          },
          editor:
          {
            xtype: 'PortletSearch',
            allowBlank: false
          }

        },
        {
          dataIndex: 'RoleID', type: 'int', header: 'Role',
          renderer: function (val, meta, record) {
            return record.get('RoleName');
          },
          editor: {
            xtype: 'RoleSearch',
            allowBlank: false
          }
        },

        { dataIndex: 'PortletName', type: 'string' },
        { dataIndex: 'RoleName', type: 'string' }
      ]
//      ,
//      rowActions: [
//          {
//            text: 'TODO change this',
//            iconCls: 'merge',
//            handler: function (record) {

//              var id = record.get('PortletID');


//            }
//          }]
    });

    return grid;
  }

});

