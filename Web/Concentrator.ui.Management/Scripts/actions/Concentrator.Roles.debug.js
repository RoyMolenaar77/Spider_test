Concentrator.Roles = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Roles',
      singularObjectName: 'Role',

      url: Concentrator.route('GetList', 'Role'),
      newUrl: Concentrator.route('Create', 'Role'),
      deleteUrl: Concentrator.route('Delete', 'Role'),
      updateUrl: Concentrator.route('Update', 'Role'),
      primaryKey: 'RoleID',
      permissions: {
        list: 'ViewRoles',
        create: 'CreateRole',
        remove: 'DeleteRole',
        update: 'UpdateRole'
      },
      structure: [
        { dataIndex: 'RoleID', type: 'int', header: 'Role ID', width: 75 },
        { dataIndex: 'RoleName', type: 'string', header: 'Name', width: 200, editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'isHidden', type: 'boolean', header: 'Hidden', width: 100, editor: { xtype: 'boolean', allowBlank: false} }
      ],
      rowActions: [
        {
          text: 'View Users',
          iconCls: 'merge',
          handler: function(record) {

            var grid = new Concentrator.ui.Grid({
              pluralObjectName: 'Users',
              singularObjectName: 'User',
              primaryKey: ['RoleID', 'UserID', 'VendorID'],
              sortBy: 'RoleID',
              permissions: {
                list: 'GetUserRole',
                create: 'CreateUserRole',
                remove: 'DeleteUserRole'
              },
              url: Concentrator.route("UsersPerRole", "UserRole"),
              newUrl: Concentrator.route("Create", "UserRole"),
              deleteUrl: Concentrator.route("DeleteUserFromRole", "UserRole"),
              params: {
                RoleID: record.get('RoleID')
              },
              newParams: {
                RoleID: record.get('RoleID')
              },
              structure: [
                { dataIndex: 'RoleID', type: 'int' },
                { dataIndex: 'UserID', type: 'int' },
                { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'user', allowBlank: false} },
                { dataIndex: 'VendorID', type: 'int', editor: { xtype: 'vendor', fieldLabel: 'Vendor'} }
              ]
            });

            var window = new Ext.Window({
              title: 'Users in' + ' ' + record.get('RoleName'),
              width: 600,
              height: 300,
              modal: true,
              layout: 'fit',
              items: [
              grid
            ]
            });

            window.show();

          }
        },
        {
          text: 'View Functionalities',
          iconCls: 'default',
          handler: function(record) {
            var button = new Ext.Button({
              text: 'Save Changes',
              iconCls: 'save',
              handler: function() {

                var enabled = [];
                var disabled = [];

                grid.store.each(function(record) {
                  if (record.get('IsEnabled')) {
                    enabled.push(record.get('FunctionalityName'));
                  } else {
                    disabled.push(record.get('FunctionalityName'));
                  }
                });

                Diract.request({
                  url: Concentrator.route("UpdateFunctionalities", "Role"),
                  params: {
                    roleID: record.get('RoleID'),
                    enabledfunctionalities: enabled,
                    disabledfunctionalities: disabled
                  },
                  success: function() {
                  }
                });

                window.destroy();
              }

            });

            var isEnabled = new Ext.grid.CheckColumn({
              header: "Is Enabled",
              dataIndex: "IsEnabled"
            });

            var grid = new Concentrator.ui.Grid({
              pluralObjectaName: 'Functionalities',
              singularObjectName: 'Functionality',
              primaryKey: 'FunctionalityName',
              permissions: {
                list: 'ViewRoles',
                create: 'CreateRole',
                remove: 'DeleteRole',
                update: 'UpdateRole'
              },
              customButtons: [button],
              plugins: [isEnabled],
              groupField: 'Group',
              sortBy: 'Group',
              hideGroupedColumn: 'Group',
              params: {
                roleID: record.get('RoleID')
              },
              url: Concentrator.route("GetFunctionalities", "Role"),
              structure: [
                { dataIndex: 'FunctionalityName', type: 'string' },
                { dataIndex: 'DisplayName', header: 'Name', type: 'string', width: 500 },
                { dataIndex: 'Group', header: 'Group', type: 'string' },
                isEnabled
              ]
            });

            var window = new Ext.Window({
              title: 'Functionalities of' + ' ' + record.get('RoleName'),
              width: 900,
              height: 450,
              modal: true,
              layout: 'fit',
              items: [grid]
            });

            window.show();

          }
        }
      ]
    });

    return grid;
  }
});