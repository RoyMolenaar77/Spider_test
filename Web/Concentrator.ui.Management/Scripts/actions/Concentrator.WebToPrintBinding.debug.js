Concentrator.WebToPrintBinding = Ext.extend(Concentrator.BaseAction, {

    getPanel: function () {

        var comboBoxRenderer = function (combo) {
            return function (value) {
                var idx = combo.store.find(combo.valueField, value);
                var rec = combo.store.getAt(idx);
                return rec.get(combo.displayField);
            };
        };

        var typeStore = new Ext.data.JsonStore({
            url: Concentrator.route('GetFieldTypes', 'WebToPrint'),
            root: 'results',
            idProperty: 'Key',
            fields: ['Key', 'Value'],
            autoLoad: true
        });

        var optionsStore = new Ext.data.JsonStore({
            autoDestroy: false,
            url: Concentrator.route("GetFieldOptionsStore", "WebToPrint"),
            method: 'GET',
            root: 'results',
            idProperty: 'OptionName',
            fields: ['OptionName', 'OptionValue']
        });

        var typeCombo = new Diract.ui.Select({
            allowBlank: false,
            store: typeStore,
            valueField: 'Key',
            displayField: 'Value'
        });

        var bindingIDeditor = new Ext.form.Hidden({
            name: 'BindingID'
        });

        var bindingsStore = new Ext.data.JsonStore({
            url: Concentrator.route('GetBindings', 'WebToPrint'),
            root: 'results',
            idProperty: 'BindingID',
            fields: ['BindingID', 'Name', 'QueryText'],
            listeners: {
                'load': function () {
                    bindinggrid.selModel.selectFirstRow();
                }
            }
        });

        var bindingItemsStore = new Ext.data.JsonStore({
            url: Concentrator.route('GetItems', 'WebToPrint'),
            root: 'results',
            idProperty: 'FieldID',
            fields: ['FieldID', 'BindingID', 'Name', 'Query'],
            autoLoad: false
        });

        var fieldgrid = new Concentrator.ui.Grid({
            pluralObjectName: 'Fields',
            singularObjectName: 'Field',
            primaryKey: ['FieldID', 'BindingID'],
            url: Concentrator.route("GetFields", "WebToPrint"),
            newUrl: Concentrator.route("CreateField", "WebToPrint"),
            updateUrl: Concentrator.route("UpdateField", "WebToPrint"),
            deleteUrl: Concentrator.route("DeleteField", "WebToPrint"),
            editUrl: Concentrator.route("EditField", "WebToPrint"),
            permissions: {
                list: 'GetBrand',
                create: 'CreateBrand',
                remove: 'DeleteBrand',
                update: 'UpdateBrand'
            },
            callback: function (a, b, c) {
                bindingsStore.reload();
            },
            structure: [
        { dataIndex: 'FieldID', type: 'int', header: 'Field ID', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'BindingID', type: 'int', editor: bindingIDeditor },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'Type', type: 'int', header: 'Type', editor: typeCombo, renderer: comboBoxRenderer(typeCombo) },
        { dataIndex: 'Options', type: 'string', header: 'Options', editor: 
            {
            xtype: 'multiselect',
            label: 'Options',
            multi: true,
            displayField: 'OptionName',
            valueField: 'OptionValue',
            store: optionsStore,
            allowBlank: false,
            width: 175
            }
        }
        ]
        });


        var bindinggrid = new Concentrator.ui.Grid({
            pluralObjectName: 'Bindings',
            singularObjectName: 'Binding',
            primaryKey: 'BindingID',
            url: Concentrator.route("GetBindings", "WebToPrint"),
            newUrl: Concentrator.route("CreateBinding", "WebToPrint"),
            updateUrl: Concentrator.route("UpdateBinding", "WebToPrint"),
            deleteUrl: Concentrator.route("DeleteBinding", "WebToPrint"),
            editUrl: Concentrator.route("EditBinding", "WebToPrint"),
            permissions: {
                list: 'GetBrand',
                create: 'CreateBrand',
                remove: 'DeleteBrand',
                update: 'UpdateBrand'
            },
            viewConfig: {
                forceFit: true,
                enableRowBody: true,
                showPreview: false
            },

            listeners: {
                'rowclick': function (grid, rowindex, r) {
                    var id = grid.store.getAt(rowindex).get('BindingID');
                    bindingIDeditor.setValue(id);
                    fieldgrid.store.load({
                        params: { 'bindingid': id }
                    });
                }
            },
            callback: function (a, b, c) {
                bindingsStore.reload();
            },
            structure: [
        { dataIndex: 'BindingID', type: 'int', header: 'ID' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'QueryText', type: 'string', header: 'Query', editor: { xtype: 'textarea', allowBlank: false}}]
        });




        var panel = new Ext.Panel({
            layout: 'border',
            items: [
      { items: [bindinggrid],
          region: 'north',
          height: 200,
          layout: 'fit'
      },
      { items: [fieldgrid],
          region: 'center',
          layout: 'fit'
      }]
        });
        return panel;
    }
});