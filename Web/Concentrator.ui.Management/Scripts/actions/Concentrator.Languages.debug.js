Concentrator.Languages = Ext.extend(Concentrator.BaseAction, {
    getPanel: function () {
        var self = this;

        var grid = new Concentrator.ui.Grid({
            pluralObjectName: 'Languages',

            singularObjectName: 'Language',
            primaryKey: 'ID',
            sortBy: 'ID',
            url: Concentrator.route("GetList", "Language"),
            newUrl: Concentrator.route("Create", "Language"),
            updateUrl: Concentrator.route("Update", "Language"),
            deleteUrl: Concentrator.route("Delete", "Language"),
            permissions: {
                list: 'GetLanguage',
                create: 'CreateLanguage',
                remove: 'DeleteLanguage',
                update: 'UpdateLanguage'
            },
            structure: [
        {
            dataIndex: 'ID', type: 'int'
        },
        { dataIndex: 'Name', type: 'string', header: 'Language',
            editor: { xtype: 'textfield' }
        },
        { dataIndex: 'DisplayCode', type: 'string', header: 'Display code',
            editor: { xtype: 'textfield' }
        }

      ]
        });

        return grid;
    }

});