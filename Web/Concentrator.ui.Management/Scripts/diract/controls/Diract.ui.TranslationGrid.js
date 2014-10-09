/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.Grid', 'Diract.ui.ExcelGrid'] }, function () {

    Diract.ui.TranslationGrid = (function () {

        var trans = Ext.extend(Diract.ui.ExcelGrid, {

            constructor: function (config) {
                Ext.apply(this, config);
                this.initRowActions();

                Diract.ui.TranslationGrid.superclass.constructor.call(this, config);
            },

            initCustomButtons: function () {
                var bbarCustomButtons = this.bbarCustomButtons || [];

                bbarCustomButtons.push(
          {
              text: 'Export translations to excel',
              iconCls: 'xls',
              handler: function () { alert('export pressed') }
          }
        );
                this.bbarCustomButtons = bbarCustomButtons;

                Diract.ui.TranslationGrid.superclass.initCustomButtons.call(this);
            },

            initRowActions: function () {
                var that = this;
                var rowActions = this.rowActions || [];

                rowActions.push({
                    text: 'Translations',
                    iconCls: 'magic-wand',
                    handler: function (rec) {

                        

                        var wind = new Ext.Window({
                            title: 'Translation management',
                            items: [
                            new Diract.ui.Grid({
                            url: that.translationsUrl,
                            params: that.getParams(rec),
                            updateUrl: that.translationsUrlUpdate,
                            primaryKey: that.translationsGridPrimaryKey || [that.primaryKey, 'LanguageID'], //based on convention
                            sortBy: that.primaryKey,
                            permissions: that.translationsPermissions || that.permissions,
                            singularObjectName: that.singularObjectName + ' translation',
                            pluralObjectName: that.pluralObjectName + ' translations',
                            structure: that.translationsGridStructure,
                            sortBy: that.translationsGridSortBy || that.primaryKey
                        })
                            ],
                            width: 400,
                            height: 300,
                            layout: 'fit'
                            
                        });

                        wind.show();
                    }
                });

                this.rowActions = rowActions;
            }

        });
        return trans;
    })();
});