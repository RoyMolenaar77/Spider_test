Concentrator.DatcolLinks = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Concentrator.ui.ExcelGrid({
      singularObjectName: 'Datcol order link',
      pluralObjectName: 'Datcol order links',
      primaryKey: ['Id'],
      sortBy: 'Id',
      direction: 'DESC',
      url: Concentrator.route('GetAll', 'Datcol'),
      overridePageSize: true,
      permissions: {
        list: 'Default',
        create: 'Default',
        remote: 'Default',
        update: 'Default'
      },
      structure: [
                    { dataIndex: 'Id', type: 'int' },
                    { dataIndex: 'WebsiteOrderNumber', type: 'string', header: 'Magento order number' },
                    {
                      dataIndex: 'DatcolNumber', type: 'string', header: 'Bon number', renderer: function (val, m, rec) {
                        return String.leftPad(rec.get('DatcolNumber'), 4,'0');
                      }
                    },
                    {
                      dataIndex: 'ShopNumber', type: 'string', header: 'Shop number'
                    },
                    { dataIndex: 'DateCreated', type: 'date', header: 'Timestamp', renderer: Ext.util.Format.dateRenderer('d-m-Y  H:i') },
                    { dataIndex: 'Amount', type: 'float', header: 'Order amount', renderer: Ext.util.Format.numberRenderer('0.00') },
                    { dataIndex: 'PaymentMethod', type: 'string', header: 'Payment method' },
                    { dataIndex: 'MessageType', type: 'string', header: 'Message type ' }
      ]
    });
    return grid;
  }
});