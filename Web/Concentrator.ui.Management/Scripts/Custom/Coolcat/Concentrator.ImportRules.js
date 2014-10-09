Concentrator.ImportRules = Ext.extend(Concentrator.GridAction, {
	getPanel: function () {
		var grid = new Concentrator.ui.Grid({
			singularObjectName: 'Import season product group rule',
			pluralObjectName: 'Import season product group ruleImport ',
			primaryKey: ['SeasonCode'],
			sortBy: 'SeasonCode',
			url: Concentrator.route('GetAll', 'Import'),
			updateUrl: Concentrator.route('Update', 'Import'),
			deleteUrl: Concentrator.route('Delete', 'Import'),
			newUrl: Concentrator.route('Create', 'Import'),
			permissions: {
				list: 'GetSeasonRules',
				create: 'CreateSeasonRules',
				remote: 'DeleteSeasonRules',
				update: 'UpdateSeasonRules'
			},
			structure: [
                    { dataIndex: 'SeasonCode', type: 'string', header: 'Season code', editor: { xtype: 'textfield', allowBlank: false }, editable: false },
                    { dataIndex: 'ProductGroupCodes', type: 'string', header: 'Product group code (comma separated)', editor: { xtype: 'textfield', allowBlank: false} }
              ]

		});
		return grid;
	}
});