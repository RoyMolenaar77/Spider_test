Concentrator.Users = Ext.extend(Concentrator.BaseAction, {
  editVendorRoles: function (userID) {

    var roleGrid = new Diract.ui.Grid({
      pluralObjectName: 'User Vendor Roles',
      width: 150,
      singularObjectName: 'User Vendor Role',
      primaryKey: ['RoleID', 'VendorID'],
      sortBy: 'RoleID',
      permissions: {
        create: 'CreateUserRole',
        remove: 'DeleteUserRole',
        list: 'GetUserRole'
      },
      params: {
        userID: userID        
      },
      newParams: {
        userID: userID
      },
      url: Concentrator.route("GetList", "UserRole"),
      newUrl: Concentrator.route("Create", "UserRole"),
      deleteUrl: Concentrator.route("Delete", "UserRole"),
      updateUrl: Concentrator.route("Update", "UserRole"),
      structure: [
        {
          dataIndex: 'RoleID',
          header: 'Role',
          renderer: function (v, m, record)
          {
            return Concentrator.stores.roles.getById(v).get('Name');
          },
          editable: false,
          editor: {
            xtype: 'select',
            store: Concentrator.stores.roles,
            label: 'Roles',
            allowBlank: false,
            name: 'RoleID'
          }
        },
        {
          dataIndex: 'VendorID',
          header: 'Vendor',
          renderer: function (v, m, record)
          {
            return Concentrator.stores.vendors.getById(v).get('VendorName');
          }, editable: false,
          editor: {
            xtype: 'select',
            store: Concentrator.stores.vendors,
            label: 'Vendors',
            allowBlank: false,
            name: 'VendorID'
          }
        }
      ]
    });
    var roleWindow = new Ext.Window({
      modal: true,
      items: roleGrid,
      layout: 'fit',
      width: 600,
      height: 300
    });
    roleWindow.show();
  },

  getPanel: function () {

    var self = this;
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Users',
      width: 300,
      singularObjectName: 'User',
      height: 400,
      width: 600,
      primaryKey: 'UserID',
      permissions: {
        update: 'UpdateUser',
        create: 'CreateUser',
        remove: 'DeleteUser',
        list: 'ViewUser'
      },
      url: Concentrator.route("GetList", "User"),
      updateUrl: Concentrator.route("Update", "User"),
      newUrl: Concentrator.route("Create", "User"),
      deleteUrl: Concentrator.route("Delete", "User"),
      structure: [
        { dataIndex: 'UserID', type: 'int' },
        { dataIndex: 'Username', type: 'string', header: 'Username', editor: { xtype: 'textfield', allowBlank: false } },
        { dataIndex: 'Firstname', header: 'First Name', type: 'string', editor: { xtype: 'textfield', allowBlank: false } },
        { dataIndex: 'Lastname', header: 'Last Name', type: 'string', editor: { xtype: 'textfield', allowBlank: false } },
        { dataIndex: 'Email', header: 'E-mail', type: 'string', editor: { xtype: 'textfield', allowBlank: false } },
        { dataIndex: 'IsActive', header: 'Active', type: 'boolean', editable: true },
        { dataIndex: 'LanguageID', header: 'Language', renderer: Concentrator.renderers.field('languages', 'Name'), type: 'int',
          editor: {
            xtype: 'language',
            enableCreateItem: false,
            allowBlank: true
          },
          filter: {
            type: 'list',
            labelField: 'Name',
            store: Concentrator.stores.languages
          }
        },
        { dataIndex: 'Password', type: 'string',
          editor: {
            fieldLabel: 'Password',
            xtype: 'textfield',
            inputType: 'password',
            allowBlank: false
          }
        },
        { dataIndex: 'Timeout', type: 'int', header: 'Timeout',
          editor: {
            xtype: 'int',
            fieldLabel: 'Timeout',
            allowBlank: false
          }
        }
      ],
      rowActions: [
        {
          iconCls: 'userFamily',
          text: 'View User Roles',
          handler: function (record) {
            self.editVendorRoles(record.get('UserID'));
          },
          roles: ["ChangePasswordUser"]
        },
        {
          iconCls: 'merge',
          text: 'Notify Me',
          handler: function (record) {
            self.pluginList(record);
          },
          roles: ['CreateUser']
        }
      ]
    });

    return grid;
  },
  pluginList: function (record)
  {
    this.Plugins = Ext.extend(Diract.ui.SearchBox, {
      valueField: 'PluginID',
      displayField: 'PluginName',
      searchUrl: Concentrator.route('Search', 'Plugin')
    });

    Ext.reg('plugins', this.Plugins);

    this.Events = Ext.extend(Diract.ui.SearchBox, {
      valueField: 'TypeID',
      displayField: 'Type',
      searchUrl: Concentrator.route('Search', 'EventType')
    });

    Ext.reg('events', this.Events);

    var grid = new Diract.ui.Grid({
      pluralObjectName: 'Plugins',
      singularObjectName: 'Plugin',
      primaryKey: ['UserID', 'PluginID', 'TypeID'],
      layout: 'fit',
      height: 410,
      newUrl: Concentrator.route('AddPlugin', 'User'),
      deleteUrl: Concentrator.route('DeletePlugin', 'User'),
      params: {
        userID: record.id
      },
      newParams: {
        userID: record.id,
        SubscriptionTime: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear() + " " + new Date().getHours() + ":" + new Date().getMinutes() + ":" + new Date().getSeconds()
      },
      permissions: {
        all: 'Default'
      },
      url: Concentrator.route("GetPlugins", "User"),
      structure: [
        { dataIndex: 'UserID', type: 'int' },
        { dataIndex: 'PluginID', type: 'int', renderer: function (val, meta, record) { return record.get('PluginName'); }, header: 'Plugin', editable: false, editor: { xtype: 'plugins' } },
        { dataIndex: 'PluginName', type: 'string' },
        { dataIndex: 'TypeID', type: 'int', renderer: function (val, meta, record) { return record.get('Type'); }, header: 'Type', editable: false, editor: { xtype: 'events' } },
        { dataIndex: 'Type', type: 'string' }
      ]
    });

    var window = new Ext.Window({
      width: 900,
      height: 450,
      items: [grid],
      model: true
    });

    window.show();
  }
});