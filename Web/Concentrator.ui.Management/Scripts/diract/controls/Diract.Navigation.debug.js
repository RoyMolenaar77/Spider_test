/// <reference path="~/Content/js/ext/ext-base-debug.js" /> 
/// <reference path="~/Content/js/ext/ext-all-debug.js" />


Diract.MenuItem = (function ()
{
  var constructor = function (config)
  {
    Ext.apply(this, config);
    Diract.MenuItem.superclass.constructor.call(this, config);
  }

  var onClick = function ()
  {
    Concentrator.ViewInstance.open(this.id);
  }

  var menuItem = Ext.extend(Ext.menu.Item, {
    initComponent: function ()
    {
      Diract.MenuItem.superclass.initComponent.call(this);
    },
    listeners: {
      'click': onClick
    }
  });

  return menuItem;
})();


Diract.Navigation = (function ()
{
  var constructor = function (config)
  {
    Ext.apply(this, config);
    Diract.Navigation.superclass.constructor.call(this, config);
  };

  var nav = Ext.extend(Ext.Panel, {
    id: 'concentrator-navigation-panel',
    region: 'center',
    split: true,
    layout: 'accordion',
    collapseFirst: true,
    fill: false,
    width: 200,
    minWidth: 150,
    border: false,
    baseCls: 'x-plain',
    initComponent: function ()
    {
      this.initMenuItems();
      Diract.Navigation.superclass.initComponent.call(this);
    },
    getMenuItem: function (id)
    {
      var item = null;
      for (var i = 0; i < this.extItems.length; i++)
      {
        var it = this.extItems[i];
        if (it.id == id)
        {
          item = it;
          break;
        }
      }
      return item;
    },
    initMenuItems: function ()
    {
      /*the menu items that will be rendered on the menu
      Parent item -- toolbar containing links
      Object struct
      {
      items: [] The menu items in the parent item
      title: The title of the item
      cls  : The cssclass of the item. Defaults to extjs styles
      id   : The id of the item. Defaults to the title of the item
      }
    
      Child item -- menu items
      {
      text: Link text,
      action: a delegate
      roles: An array of the roles allowed to view the link,
      icon : defaults to ext blank image
      id : id of the menu item
      }
      */
      if (!this.menuItems)
      {
        return;
      }

      this.extItems = new Array(); //store all items for later reference

      this.menu = new Object();

      var panls = [];

      for (var i = 0; i < this.menuItems.length; i++)
      {
        var pan = this.menuItems[i];
        this.menu[pan.id] = pan;
        panls.push(this._addMenuItem(pan));
      }

      this.items = panls;
    },
    _addMenuItem: function (item)
    {
      var children = [];
      for (var j = 0; j < item.children.length; j++)
      {
        var i = item.children[j];

        var s = new Diract.MenuItem({
          text: i.Name,
          iconCls: i.Icon || 'empty-icon',
          id: i.ID,
          //overCls: 'x-menu-item-active',
          href: '#',
          action: Concentrator[i.action.trim()]
        });

        this.extItems.push(s);

        children.push(s);
      }

      var it = {
        title: item.title,
        id: item.id,
        border: false,
        items: children
      }
      return it;
    }
  });

  return nav;
})();


