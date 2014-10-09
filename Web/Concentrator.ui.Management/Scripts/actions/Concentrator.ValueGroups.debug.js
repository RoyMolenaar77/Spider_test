Concentrator.ValueGroups = Ext.extend(Concentrator.GridAction, {

	getPanel: function () {

		var grid = new Diract.ui.TranslationGrid({
			singularObjectName: 'attribute value group',
			singularObjectName: 'attribute values groups',
			permissions: {
				list: 'GetValueGrouping',
				create: 'CreateValueGrouping',
				remote: 'DeleteValueGrouping',
				update: 'UpdateValueGrouping'
			},
			translationsUrl: Concentrator.route('GetTranslations', 'ProductAttributeValueGroup'),
			translationsUrlUpdate: Concentrator.route('SetTranslation', 'ProductAttributeValueGroup'),
			getParams: function (rec) {
				return {
					attributeValueGroupID: rec.get('AttributeValueGroupID')
				};
			},
			translationsGridStructure: [
        { dataIndex: 'AttributeValueGroupID', type: 'int' },
        { dataIndex: 'Language', type: 'string', header: 'Language' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield'} },
        { dataIndex: 'LanguageID', type: 'int' }
      ],
			url: Concentrator.route('GetList', 'ProductAttributeValueGroup'),
			updateUrl: Concentrator.route('Update', 'ProductAttributeValueGroup'),
			deleteUrl: Concentrator.route('Delete', 'ProductAttributeValueGroup'),
			newUrl: Concentrator.route('Create', 'ProductAttributeValueGroup'),
			newFormConfig: Concentrator.FormConfigurations.newProductAttributeValueGroup,
			primaryKey: 'AttributeValueGroupID',
			windowConfig: {
				height: 450,
				width: 490
			},
			autoLoadStore: false,
			structure: [
        { dataIndex: 'AttributeValueGroupID', type: 'int', header: 'Value group', renderer: function (val, m, rec) { return rec.get('AttributeValueGroup') }, editable: false,
        	editor: { xtype: 'attributeValueGroup' }, filter: { type: 'string', filterField: 'AttributeValueGroup' }
        },
        { dataIndex: 'AttributeValueGroup', type: 'string' },
        { dataIndex: 'Score', type: 'int', header: 'Score', editor: { xtype: 'numberfield'} },
        { dataIndex: 'ImagePath', type: 'string', header: 'Image' },
        { dataIndex: 'ConnectorID', type: 'int', editor: {
        	xtype: 'connector',
        	allowBlank: true
        }
        }
      ],
			rowActions: [
        {
        	text: 'View Image',
        	iconCls: 'merge',
        	predicate: function (record) { return (record.get('AttributeValueGroupID') > 0); },
        	handler: function (record) {

        		var box = new Ext.BoxComponent(
              { autoEl: {
              	tag: 'img',
              	src: Concentrator.GetProductAttributeValueGroupImageUrl(record.get('ImagePath'), 256, 256)
              }
              });

        		var closeButton = new Ext.Button({
        			text: 'Close',
        			handler: function () {
        				window.destroy();
        			}
        		});

        		var uploadButton = new Ext.Button({
        			text: 'Upload new image',
        			handler: function () {
        				uploadWindow.show();
        			}
        		});

        		var form = new Ext.form.FormPanel({
        			padding: 10,
        			items: [
                box
              ],
        			buttons: [
              uploadButton,
                closeButton
              ]
        		});

        		var window = new Ext.Window({
        			title: 'Image',
        			height: 200,
        			width: 300,
        			modal: true,
        			layout: 'fit',
        			items: [
                form
              ]
        		});

        		var self = this;
        		var that = record;

        		var uploadWindow = new Diract.ui.FormWindow({
        			url: Concentrator.route('UploadImage', 'ProductAttributeValueGroup'),
        			cancelButton: true,
        			title: 'Upload image',
        			autoDestroy: true,
        			height: 120,
        			width: 340,
        			modal: true,
        			fileUpload: true, layout: 'fit',
        			items: [{
        				xtype: 'fileuploadfield',
        				id: 'form-file',
        				emptyText: 'Select image',
        				name: 'ImagePath',
        				hiddenName: 'ImagePath',
        				fieldLabel: 'Image',
        				buttonText: '',
        				width: 200,
        				buttonCfg: { iconCls: 'upload-icon' },
        				allowBlank: false
        			}],
        			params: {
        				AttributeValueGroupID: record.get('AttributeValueGroupID')
        			},
        			success: function () {
        				uploadWindow.destroy(),
                window.updateBox(box);
        				gr.store.reload();
        			}
        		});

        		window.show();
        	}
        }
      ]
		});

		return grid;
	}
});