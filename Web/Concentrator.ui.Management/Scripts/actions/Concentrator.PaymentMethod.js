Concentrator.PaymentMethod = Ext.extend(Concentrator.BaseAction, {
	getPanel: function () {

		return new Diract.ui.Grid({
			singularObjectName: 'Payment method',
			pluralObjectName: 'Payment methods',
			permissions: {
				list: 'ViewOrders',
				create: 'ViewOrders',
				update: 'ViewOrders',
				remove : 'ViewOrders'
			},
			primaryKey: ['LanguageID', 'ID'],
			url: Concentrator.route('GetList', 'PaymentMethod'),
			newUrl: Concentrator.route('Create', 'PaymentMethod'),
			updateUrl: Concentrator.route('Update', 'PaymentMethod'),
			deleteUrl: Concentrator.route('Delete', 'PaymentMethod'),
			structure: [
				{ dataIndex: 'ID', type: 'int' },
				{ dataIndex: 'Code', type: 'string', header: 'Code', editor: { xtype: 'textfield'} },
				{ dataIndex: 'LanguageID', type: 'int', editor: { xtype: 'language', allowBlank: false }, renderer: Concentrator.renderers.field('languages', 'Name'), header: 'Language' },
				{ dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield'} }
			]
		});

	}
});