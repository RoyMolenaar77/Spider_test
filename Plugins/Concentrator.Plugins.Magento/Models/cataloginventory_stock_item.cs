using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class cataloginventory_stock_item
  {

    public cataloginventory_stock_item()
    {
      stock_id = 1;
      use_config_backorders = true;
      use_config_enable_qty_increments = true;
      use_config_manage_stock = true;
      use_config_max_sale_qty = true;
      use_config_min_qty = true;
      use_config_min_sale_qty = true;
      use_config_notify_stock_qty = true;
      use_config_qty_increments = true;

    }

    public int item_id { get; set; }
    public int product_id { get; set; }
    public int stock_id { get; set; }
    public int qty { get; set; }
    public int min_qty { get; set; }
    public bool use_config_min_qty { get; set; }
    public bool is_qty_decimal { get; set; }
    public int backorders { get; set; }
    public bool use_config_backorders { get; set; }
    public int min_sale_qty { get; set; }
    public bool use_config_min_sale_qty { get; set; }
    public int max_sale_qty { get; set; }
    public bool use_config_max_sale_qty { get; set; }
    public bool is_in_stock { get; set; }
    public int low_stock_date { get; set; }
    public int notify_stock_qty { get; set; }
    public bool use_config_notify_stock_qty { get; set; }
    public int manage_stock { get; set; }
    public bool use_config_manage_stock { get; set; }
    public int stock_status_changed_automatically { get; set; }
    public bool use_config_qty_increments { get; set; }
    public int qty_increments { get; set; }
    public bool use_config_enable_qty_increments { get; set; }
    public bool enable_qty_increments { get; set; }
  }
}
