using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Plugins.Magento.Models;
using MySql.Data.MySqlClient;
using AutoMapper;
using AuditLog4Net.Adapter;
using MySql.Data.Types;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class AssortmentHelper : MagentoMySqlHelper
  {

    private eav_attribute visibility_attribute = null;
    private eav_attribute media_gallery_attribute = null;
    private MagentoVersion version;

    public AssortmentHelper(string connectionString, MagentoVersion version)
      : base(connectionString)
    {

      AutoMapper.Mapper.CreateMap<IDataReader, basstock>();
      AutoMapper.Mapper.CreateMap<IDataRecord, basstock>();

      AutoMapper.Mapper.CreateMap<IDataReader, basstore>();
      AutoMapper.Mapper.CreateMap<IDataRecord, basstore>();

      _tablesNames.Add("basstock", TableName("basstock"));
      _tablesNames.Add("basstore", TableName("basstore"));


      visibility_attribute = GetAttribute("visibility");
      media_gallery_attribute = GetAttribute("media_gallery");

      this.version = version;
    }

    public AssortmentHelper(string connectionString)
      : this(connectionString, MagentoVersion.Version_15)
    {
    }

    #region Stock



    public Dictionary<string, int> GetStockStoreList()
    {
      Dictionary<string, int> result = new Dictionary<string, int>();
      using (var cn = Connection.CreateCommand())
      {

        cn.CommandType = CommandType.Text;
        cn.CommandText = String.Format(@"SELECT DISTINCT companyno, store_id FROM {0}", _tablesNames["basstore"]);

        using (var reader = cn.ExecuteReader())
        {
          while (reader.Read())
          {
            result.Add(reader["companyno"].ToString().Trim(), Convert.ToInt32(reader["store_id"]));
          }
        }

      }

      return result;

    }

    private MySqlCommand _deleteStoreStockCommand = null;
    private MySqlCommand _syncStoreCommand = null;

    internal void SyncStoreStock(string sku, int bas_store_id, int quantityOnHand)
    {
      if (_syncStoreCommand == null)
      {
        _syncStoreCommand = Connection.CreateCommand();
        _syncStoreCommand.CommandType = CommandType.Text;
        _syncStoreCommand.CommandText =
            String.Format(@"INSERT INTO {0} (product_sku, bas_store_id, qty) 
                            VALUES (?sku, ?bas_store_id, ?qty)
                            ON DUPLICATE KEY UPDATE qty = VALUES(qty)
                          ", _tablesNames["basstock"]);

        _syncStoreCommand.Prepare();
        _syncStoreCommand.Parameters.AddWithValue("?sku", String.Empty);
        _syncStoreCommand.Parameters.AddWithValue("?bas_store_id", 0);
        _syncStoreCommand.Parameters.AddWithValue("?qty", 0);
      }

      if (_deleteStoreStockCommand == null)
      {
        _deleteStoreStockCommand = Connection.CreateCommand();
        _deleteStoreStockCommand.CommandType = CommandType.Text;
        _deleteStoreStockCommand.CommandText =
            String.Format(@"DELETE FROM {0} WHERE bas_store_id = ?bas_store_id AND product_sku = ?sku", _tablesNames["basstock"]);

        _deleteStoreStockCommand.Prepare();
        _deleteStoreStockCommand.Parameters.AddWithValue("?sku", String.Empty);
        _deleteStoreStockCommand.Parameters.AddWithValue("?bas_store_id", 0);
      }


      if (quantityOnHand <= 0)
      {

        _deleteStoreStockCommand.Parameters["?sku"].Value = sku;
        _deleteStoreStockCommand.Parameters["?bas_store_id"].Value = bas_store_id;
        _deleteStoreStockCommand.ExecuteNonQuery();
      }
      else
      {
        _syncStoreCommand.Parameters["?sku"].Value = sku;
        _syncStoreCommand.Parameters["?bas_store_id"].Value = bas_store_id;
        _syncStoreCommand.Parameters["?qty"].Value = quantityOnHand;

        _syncStoreCommand.ExecuteNonQuery();
      }

    }

    private MySqlCommand _clearStoreStockCommand = null;
    internal void ClearStoreStock(string sku)
    {
      if (_clearStoreStockCommand == null)
      {
        _clearStoreStockCommand = Connection.CreateCommand();
        _clearStoreStockCommand.CommandType = CommandType.Text;
        _clearStoreStockCommand.CommandText =
            String.Format(@"DELETE FROM {0} WHERE product_sku = ?sku", _tablesNames["basstock"]);

        _clearStoreStockCommand.Prepare();
        _clearStoreStockCommand.Parameters.AddWithValue("?sku", String.Empty);
      }

      _clearStoreStockCommand.Parameters["?sku"].Value = sku;
      _clearStoreStockCommand.ExecuteNonQuery();
    }


    #endregion
    #region Categories

    public void UpdateCategoryChildCount()
    {

      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;
      cmd.CommandText = string.Format(@"
UPDATE  {0} as cce
		LEFT JOIN 
			(SELECT s1.entity_id as cid, COALESCE( COUNT( s2.entity_id ) , 0 ) AS cnt
				FROM {0} AS s1
				LEFT JOIN {0} AS s2 ON s2.parent_id = s1.entity_id
			GROUP BY s1.entity_id) as sq ON sq.cid=cce.entity_id
			SET cce.children_count=sq.cnt
            ", _tablesNames["cce"]);

      cmd.ExecuteNonQuery();

    }


    internal void SyncCategory(catalog_category_entity cat_entity)
    {
      var t = TableName("catalog_category_entity");


      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;

      if (cat_entity.entity_id > 0)
      {
        cmd.CommandText = String.Format(@"
                                     UPDATE {0} SET entity_type_id = ?entity_type_id , attribute_set_id =?attribute_set_id, parent_id=?parent_id,
    created_at=?created_at, updated_at=?updated_at, path=?path, position=?position, level=?level, children_count=?children_count
WHERE entity_id = ?entity_id", t);

      }
      else
      {

        cmd.CommandText = String.Format(@"
                                     INSERT INTO {0} (entity_id, entity_type_id, attribute_set_id, parent_id, 
                                                                          created_at, updated_at, path, position, level, children_count)
                                      VALUES (?entity_id, ?entity_type_id, ?attribute_set_id, ?parent_id, ?created_at, ?updated_at, ?path, ?position, ?level, ?children_count)", t);
      }

      cmd.Parameters.AddWithValue("?entity_id", cat_entity.entity_id > 0 ? (int?)cat_entity.entity_id : null);
      cmd.Parameters.AddWithValue("?entity_type_id", cat_entity.entity_type_id);
      cmd.Parameters.AddWithValue("?attribute_set_id", cat_entity.attribute_set_id);
      cmd.Parameters.AddWithValue("?parent_id", cat_entity.parent_id);
      cmd.Parameters.AddWithValue("?created_at", cat_entity.created_at);
      cmd.Parameters.AddWithValue("?updated_at", cat_entity.updated_at);
      cmd.Parameters.AddWithValue("?path", cat_entity.path);
      cmd.Parameters.AddWithValue("?position", cat_entity.position);
      cmd.Parameters.AddWithValue("?level", cat_entity.level);
      cmd.Parameters.AddWithValue("?children_count", 0);


      cmd.ExecuteNonQuery();
      if (cat_entity.entity_id == 0)
      {
        cat_entity.entity_id = (int)cmd.LastInsertedId;
        cat_entity.path += cat_entity.entity_id.ToString();


        var updatePath = Connection.CreateCommand();
        updatePath.CommandText = String.Format(@"UPDATE {0} SET path = ?path WHERE entity_id = ?entity_id", t);
        updatePath.CommandType = CommandType.Text;
        updatePath.Parameters.AddWithValue("?path", cat_entity.path);
        updatePath.Parameters.AddWithValue("?entity_id", cat_entity.entity_id);
        updatePath.ExecuteNonQuery();
      }




      //update name

    }
    #endregion

    #region Products



    public void AddProduct(catalog_product_entity entity)
    {
      var cpe = TableName("catalog_product_entity");

      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;
      cmd.CommandText = String.Format(@"INSERT INTO {0} (entity_type_id, attribute_set_id, type_id, sku, has_options, required_options, created_at, updated_at)
                                        VALUES (?entity_type_id, ?attribute_set_id, ?type_id, ?sku, ?has_options, ?required_options, now(), now())", cpe);

      cmd.Parameters.AddWithValue("?entity_type_id", entity.entity_type_id);
      cmd.Parameters.AddWithValue("?attribute_set_id", entity.attribute_set_id);
      cmd.Parameters.AddWithValue("?type_id", entity.type_id);
      cmd.Parameters.AddWithValue("?sku", entity.sku);
      cmd.Parameters.AddWithValue("?has_options", entity.has_options);
      cmd.Parameters.AddWithValue("?required_options", entity.required_options);
      cmd.Parameters.AddWithValue("?created_at", new MySqlDateTime(entity.created_at));
      cmd.Parameters.AddWithValue("?updated_at", new MySqlDateTime(entity.updated_at));

      cmd.ExecuteNonQuery();

      entity.entity_id = (int)cmd.LastInsertedId;

    }


    private MySqlCommand _syncCategoryProductCommand = null;
    internal void SyncCategoryProduct(int category_id, int product_id, int position)
    {
      if (_syncCategoryProductCommand == null)
      {
        _syncCategoryProductCommand = Connection.CreateCommand();
        _syncCategoryProductCommand.CommandType = CommandType.Text;
        _syncCategoryProductCommand.CommandText = String.Format(@"INSERT IGNORE catalog_category_product (category_id, product_id, position)
                                                                  VALUES (?category_id, ?product_id, ?position)");

        _syncCategoryProductCommand.Prepare();

        _syncCategoryProductCommand.Parameters.AddWithValue("?category_id", category_id);
        _syncCategoryProductCommand.Parameters.AddWithValue("?product_id", product_id);
        _syncCategoryProductCommand.Parameters.AddWithValue("?position", position);

      }
      else
      {
        _syncCategoryProductCommand.Parameters["?category_id"].Value = category_id;
        _syncCategoryProductCommand.Parameters["?product_id"].Value = product_id;
        _syncCategoryProductCommand.Parameters["?position"].Value = position;

      }

      _syncCategoryProductCommand.ExecuteNonQuery();
    }


    private MySqlCommand _syncWebsiteProductCommand = null;
    public void SyncWebsiteProduct(int website_id, int product_id)
    {
      if (_syncWebsiteProductCommand == null)
      {
        _syncWebsiteProductCommand = Connection.CreateCommand();
        _syncWebsiteProductCommand.CommandType = CommandType.Text;
        _syncWebsiteProductCommand.CommandText = String.Format(@"REPLACE INTO catalog_product_website (website_id, product_id)
                                                                  VALUES (?website_id, ?product_id)");

        _syncWebsiteProductCommand.Prepare();

        _syncWebsiteProductCommand.Parameters.AddWithValue("?website_id", website_id);
        _syncWebsiteProductCommand.Parameters.AddWithValue("?product_id", product_id);

      }
      else
      {
        _syncWebsiteProductCommand.Parameters["?website_id"].Value = website_id;
        _syncWebsiteProductCommand.Parameters["?product_id"].Value = product_id;


      }

      _syncWebsiteProductCommand.ExecuteNonQuery();
    }


    private MySqlCommand _syncStockCommand = null;
    public void SyncStock(cataloginventory_stock_item item)
    {

      if (_syncStockCommand == null)
      {
        _syncStockCommand = Connection.CreateCommand();
        _syncStockCommand.CommandType = CommandType.Text;

        if (version == MagentoVersion.Version_15)
        {
          _syncStockCommand.CommandText = String.Format(@"INSERT INTO cataloginventory_stock_item (
                                                        product_id ,stock_id,qty,min_qty,use_config_min_qty,is_qty_decimal,backorders,use_config_backorders,min_sale_qty
                                                        ,use_config_min_sale_qty,max_sale_qty,use_config_max_sale_qty,is_in_stock,low_stock_date,notify_stock_qty,use_config_notify_stock_qty,manage_stock
                                                        ,use_config_manage_stock,stock_status_changed_automatically,use_config_qty_increments,qty_increments,use_config_enable_qty_increments
                                                        ,enable_qty_increments)
                                                        VALUES ( ?product_id , ?stock_id, ?qty, ?min_qty, ?use_config_min_qty, ?is_qty_decimal, ?backorders, ?use_config_backorders, ?min_sale_qty
                                                        , ?use_config_min_sale_qty, ?max_sale_qty, ?use_config_max_sale_qty, ?is_in_stock, ?low_stock_date, ?notify_stock_qty, ?use_config_notify_stock_qty, ?manage_stock
                                                        , ?use_config_manage_stock, ?stock_status_changed_automatically, ?use_config_qty_increments, ?qty_increments, ?use_config_enable_qty_increments
                                                        , ?enable_qty_increments)
                                                        ON DUPLICATE KEY UPDATE qty=VALUES(qty),
                                                        min_qty=VALUES(min_qty),
                                                        use_config_min_qty=VALUES(use_config_min_qty),
                                                        is_qty_decimal=VALUES(is_qty_decimal),
                                                        backorders=VALUES(backorders),
                                                        use_config_backorders=VALUES(use_config_backorders),
                                                        min_sale_qty=VALUES(min_sale_qty),
                                                        use_config_min_sale_qty=VALUES(use_config_min_sale_qty),
                                                        max_sale_qty=VALUES(max_sale_qty),
                                                        use_config_max_sale_qty=VALUES(use_config_max_sale_qty),
                                                        is_in_stock=VALUES(is_in_stock),
                                                        low_stock_date=VALUES(low_stock_date),
                                                        notify_stock_qty=VALUES(notify_stock_qty),
                                                        use_config_notify_stock_qty=VALUES(use_config_notify_stock_qty),
                                                        manage_stock=VALUES(manage_stock),
                                                        use_config_manage_stock=VALUES(use_config_manage_stock),
                                                        stock_status_changed_automatically=VALUES(stock_status_changed_automatically),
                                                        use_config_qty_increments=VALUES(use_config_qty_increments),
                                                        qty_increments=VALUES(qty_increments),
                                                        use_config_enable_qty_increments=VALUES(use_config_enable_qty_increments),
                                                        enable_qty_increments=VALUES(enable_qty_increments)
          ");
        }
        else
        {
          _syncStockCommand.CommandText = String.Format(@"INSERT INTO cataloginventory_stock_item (
                                                        product_id ,stock_id,qty,min_qty,use_config_min_qty,is_qty_decimal,backorders,use_config_backorders,min_sale_qty
                                                        ,use_config_min_sale_qty,max_sale_qty,use_config_max_sale_qty,is_in_stock,low_stock_date,notify_stock_qty,use_config_notify_stock_qty,manage_stock
                                                        ,use_config_manage_stock,stock_status_changed_auto,use_config_qty_increments,qty_increments,use_config_enable_qty_inc
                                                        ,enable_qty_increments)
                                                        VALUES ( ?product_id , ?stock_id, ?qty, ?min_qty, ?use_config_min_qty, ?is_qty_decimal, ?backorders, ?use_config_backorders, ?min_sale_qty
                                                        , ?use_config_min_sale_qty, ?max_sale_qty, ?use_config_max_sale_qty, ?is_in_stock, ?low_stock_date, ?notify_stock_qty, ?use_config_notify_stock_qty, ?manage_stock
                                                        , ?use_config_manage_stock, ?stock_status_changed_auto, ?use_config_qty_increments, ?qty_increments, ?use_config_enable_qty_inc
                                                        , ?enable_qty_increments)
                                                        ON DUPLICATE KEY UPDATE qty=VALUES(qty),
                                                        min_qty=VALUES(min_qty),
                                                        use_config_min_qty=VALUES(use_config_min_qty),
                                                        is_qty_decimal=VALUES(is_qty_decimal),
                                                        backorders=VALUES(backorders),
                                                        use_config_backorders=VALUES(use_config_backorders),
                                                        min_sale_qty=VALUES(min_sale_qty),
                                                        use_config_min_sale_qty=VALUES(use_config_min_sale_qty),
                                                        max_sale_qty=VALUES(max_sale_qty),
                                                        use_config_max_sale_qty=VALUES(use_config_max_sale_qty),
                                                        is_in_stock=VALUES(is_in_stock),
                                                        low_stock_date=VALUES(low_stock_date),
                                                        notify_stock_qty=VALUES(notify_stock_qty),
                                                        use_config_notify_stock_qty=VALUES(use_config_notify_stock_qty),
                                                        manage_stock=VALUES(manage_stock),
                                                        use_config_manage_stock=VALUES(use_config_manage_stock),
                                                        stock_status_changed_auto=VALUES(stock_status_changed_auto),
                                                        use_config_qty_increments=VALUES(use_config_qty_increments),
                                                        qty_increments=VALUES(qty_increments),
                                                        use_config_enable_qty_inc=VALUES(use_config_enable_qty_inc),
                                                        enable_qty_increments=VALUES(enable_qty_increments)
          ");

        }

        _syncStockCommand.Prepare();


        _syncStockCommand.Parameters.AddWithValue("?product_id", null);
        _syncStockCommand.Parameters.AddWithValue("?stock_id", null);
        _syncStockCommand.Parameters.AddWithValue("?qty", null);
        _syncStockCommand.Parameters.AddWithValue("?min_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?use_config_min_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?is_qty_decimal", null);
        _syncStockCommand.Parameters.AddWithValue("?backorders", null);
        _syncStockCommand.Parameters.AddWithValue("?use_config_backorders", null);
        _syncStockCommand.Parameters.AddWithValue("?min_sale_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?use_config_min_sale_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?max_sale_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?use_config_max_sale_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?is_in_stock", null);
        _syncStockCommand.Parameters.AddWithValue("?low_stock_date", null);
        _syncStockCommand.Parameters.AddWithValue("?notify_stock_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?use_config_notify_stock_qty", null);
        _syncStockCommand.Parameters.AddWithValue("?manage_stock", null);
        _syncStockCommand.Parameters.AddWithValue("?use_config_manage_stock", null);

        if (version == MagentoVersion.Version_15)
        {
          _syncStockCommand.Parameters.AddWithValue("?stock_status_changed_automatically", null);
          _syncStockCommand.Parameters.AddWithValue("?use_config_enable_qty_increments", null);
        }
        else
        {
          _syncStockCommand.Parameters.AddWithValue("?stock_status_changed_auto", null);
          _syncStockCommand.Parameters.AddWithValue("?use_config_enable_qty_inc", null);
        }

        _syncStockCommand.Parameters.AddWithValue("?use_config_qty_increments", null);
        _syncStockCommand.Parameters.AddWithValue("?qty_increments", null);

        _syncStockCommand.Parameters.AddWithValue("?enable_qty_increments", null);


      }

      _syncStockCommand.Parameters["?product_id"].Value = item.product_id;
      _syncStockCommand.Parameters["?stock_id"].Value = item.stock_id;
      _syncStockCommand.Parameters["?qty"].Value = item.qty;
      _syncStockCommand.Parameters["?min_qty"].Value = item.min_qty;
      _syncStockCommand.Parameters["?use_config_min_qty"].Value = item.use_config_min_qty;
      _syncStockCommand.Parameters["?is_qty_decimal"].Value = item.is_qty_decimal;
      _syncStockCommand.Parameters["?backorders"].Value = item.backorders;
      _syncStockCommand.Parameters["?use_config_backorders"].Value = item.use_config_backorders;
      _syncStockCommand.Parameters["?min_sale_qty"].Value = item.min_sale_qty;
      _syncStockCommand.Parameters["?use_config_min_sale_qty"].Value = item.use_config_min_sale_qty;
      _syncStockCommand.Parameters["?max_sale_qty"].Value = item.max_sale_qty;
      _syncStockCommand.Parameters["?use_config_max_sale_qty"].Value = item.use_config_max_sale_qty;
      _syncStockCommand.Parameters["?is_in_stock"].Value = item.is_in_stock;
      _syncStockCommand.Parameters["?low_stock_date"].Value = item.low_stock_date;
      _syncStockCommand.Parameters["?notify_stock_qty"].Value = item.notify_stock_qty;
      _syncStockCommand.Parameters["?use_config_notify_stock_qty"].Value = item.use_config_notify_stock_qty;
      _syncStockCommand.Parameters["?manage_stock"].Value = item.manage_stock;
      _syncStockCommand.Parameters["?use_config_manage_stock"].Value = item.use_config_manage_stock;

      if (version == MagentoVersion.Version_15)
      {
        _syncStockCommand.Parameters["?stock_status_changed_automatically"].Value = item.stock_status_changed_automatically;
        _syncStockCommand.Parameters["?use_config_enable_qty_increments"].Value = item.use_config_enable_qty_increments;
      }
      else
      {
        _syncStockCommand.Parameters["?stock_status_changed_auto"].Value = item.stock_status_changed_automatically;
        _syncStockCommand.Parameters["?use_config_enable_qty_inc"].Value = item.use_config_enable_qty_increments;
      }

      _syncStockCommand.Parameters["?use_config_qty_increments"].Value = item.use_config_qty_increments;
      _syncStockCommand.Parameters["?qty_increments"].Value = item.qty_increments;
      _syncStockCommand.Parameters["?enable_qty_increments"].Value = item.enable_qty_increments;

      _syncStockCommand.ExecuteNonQuery();
    }
    #endregion

    #region URL Rewrite Index




    MySqlCommand _urlRewriteSelectCommand = null;
    MySqlCommand _getProductCategoryPathsCommand = null;

    public void UpdateURLRewriteIndex(catalog_product_entity entity)
    {
      #region Build Commands

      if (_getProductCategoryPathsCommand == null)
      {
        _getProductCategoryPathsCommand = Connection.CreateCommand();
        _getProductCategoryPathsCommand.CommandType = CommandType.Text;
        _getProductCategoryPathsCommand.CommandText = String.Format(@"SELECT cce.path as cpath,SUBSTR(cce.path,LOCATE('/',cce.path,3)+1) as cshortpath,csg.default_store_id as store_id,cce.entity_id as catid
			  FROM {0} as ccp 
			  JOIN {1} as cce ON cce.entity_id=ccp.category_id 
			  JOIN {2} as csg ON csg.root_category_id=SUBSTR(SUBSTRING_INDEX(cce.path,'/',2),LOCATE('/',SUBSTRING_INDEX(cce.path,'/',2))+1)
			  WHERE ccp.product_id=?product_id", _tablesNames["ccp"], _tablesNames["cce"], _tablesNames["csg"]);

        _getProductCategoryPathsCommand.Prepare();
        _getProductCategoryPathsCommand.Parameters.AddWithValue("?product_id", 0);
      }


      if (_urlRewriteSelectCommand == null)
      {
        _urlRewriteSelectCommand = Connection.CreateCommand();
        _urlRewriteSelectCommand.CommandType = CommandType.Text;
        _urlRewriteSelectCommand.CommandText = String.Format(@"SELECT cce.entity_id as catid,COALESCE(ccev.value,ccevd.value) as value 
				  FROM {0} as cce 
			  	JOIN {1} as ea1 ON ea1.attribute_code='children'
			 	JOIN {1} as ea2 ON ea2.attribute_code ='name' AND ea2.entity_type_id=ea1.entity_type_id
			  	JOIN {2} as ccevd ON ccevd.attribute_id=ea2.attribute_id AND ccevd.entity_id=cce.entity_id AND ccevd.store_id=0
			  	LEFT JOIN {3} as ccev ON ccev.attribute_id=ea2.attribute_id AND ccev.entity_id=cce.entity_id AND ccev.store_id=?store_id
			  	WHERE FIND_IN_SET( cce.entity_id  ,?category_list) != 0
			  	GROUP BY cce.entity_id", _tablesNames["cce"], _tablesNames["ea"], _tablesNames["ccev"], _tablesNames["ccev"]);

        _urlRewriteSelectCommand.Prepare();
        _urlRewriteSelectCommand.Parameters.AddWithValue("?store_id", 0);
        _urlRewriteSelectCommand.Parameters.AddWithValue("?category_list", string.Empty);
      }


      #endregion


      string productUrlKey = BuildProductUrl(entity);


      List<string> data = new List<string>();


      _getProductCategoryPathsCommand.Parameters["?product_id"].Value = entity.entity_id;

      Dictionary<int, List<path_info>> productCategoryPaths = new Dictionary<int, List<path_info>>();

      using (var reader = _getProductCategoryPathsCommand.ExecuteReader())
      {
        var atts = Mapper.Map<IDataReader, IList<path_info>>(reader);
        foreach (var at in atts)
        {
          if (!productCategoryPaths.ContainsKey(at.store_id))
            productCategoryPaths.Add(at.store_id, new List<path_info>());

          productCategoryPaths[at.store_id].Add(at);
        }
      }






      foreach (var store in productCategoryPaths)
      {
        List<int> category_ids = new List<int>();
        foreach (var cat in store.Value)
        {
          category_ids.AddRange(cat.cshortpath.Split('/').ConvertTo<int>());
        }

        _urlRewriteSelectCommand.Parameters["?store_id"].Value = store.Key;
        _urlRewriteSelectCommand.Parameters["?category_list"].Value = string.Join(",", category_ids.Distinct());



        Dictionary<int, string> categoryNames = new Dictionary<int, string>();
        using (var reader = _urlRewriteSelectCommand.ExecuteReader())
        {
          while (reader.Read())
          {
            categoryNames.Add(reader.GetInt32("catid"), reader.SafeGetString("value"));
          }
        }

        foreach (var path in store.Value)
        {
          var sp = path.cshortpath;
          var cpids = path.cshortpath.Split('/').ConvertTo<int>();

          List<string> names = new List<string>();
          foreach (var id in cpids)
            names.Add(categoryNames[id]);

          var namestring = String.Join("/", names);
          var curlk = Slugger.Slug(namestring, true);

          var sdata = new string[]{
            
            string.Format("'{0}'", entity.entity_id),
            string.Format("'{0}'", store.Key),
            string.Format("'{0}'", path.catid),
            string.Format( "'product/{0}/{1}'", entity.entity_id, path.catid),
            string.Format( "'catalog/product/view/id/{0}/category/{1}'", entity.entity_id.ToString(), path.catid.ToString()) ,
            string.Format("'{0}/{1}'", curlk,productUrlKey),
            "1"
          };


          data.Add("(" + String.Join(",", sdata) + ")");

        }


      }


      var insertCategoryUrlRewriteCommand = Connection.CreateCommand();
      insertCategoryUrlRewriteCommand.CommandType = CommandType.Text;
      insertCategoryUrlRewriteCommand.CommandText = String.Format(@"INSERT IGNORE INTO {0} (product_id, store_id, category_id, id_path, target_path, request_path, is_system) VALUES {1}"
        , _tablesNames["curw"], String.Join(",", data));

      insertCategoryUrlRewriteCommand.ExecuteNonQuery();





    }


    private MySqlCommand _getProductUrlCommand = null;
    private MySqlCommand _deleteProductUrlCommand = null;
    private MySqlCommand _insertProductUrlCommand = null;
    public string BuildProductUrl(catalog_product_entity entity)
    {
      #region Build Commands

      if (_getProductUrlCommand == null)
      {
        _getProductUrlCommand = Connection.CreateCommand();
        _getProductUrlCommand.CommandType = CommandType.Text;
        _getProductUrlCommand.CommandText = String.Format(@"SELECT ea.attribute_code,cpei.value as visvalue,cpev.attribute_id,cpev.value as value
			  FROM {0} AS cpe
			  JOIN {1} as ea ON ea.attribute_code IN ('
y','name')
			  JOIN {2} as cpev ON cpev.entity_id=cpe.entity_id AND cpev.attribute_id=ea.attribute_id
			  JOIN {3} as cpei ON cpei.entity_id=cpe.entity_id AND cpei.attribute_id= ?attribute_id AND cpei.value>1
			  WHERE cpe.entity_id= ?product_id", _tablesNames["cpe"], _tablesNames["ea"],
                                _tablesNames["cpev"], _tablesNames["cpei"]);

        _getProductUrlCommand.Prepare();
        _getProductUrlCommand.Parameters.AddWithValue("?attribute_id", visibility_attribute.attribute_id);
        _getProductUrlCommand.Parameters.AddWithValue("?product_id", 0);

      }

      if (_deleteProductUrlCommand == null)
      {
        _deleteProductUrlCommand = Connection.CreateCommand();
        _deleteProductUrlCommand.CommandType = CommandType.Text;
        _deleteProductUrlCommand.CommandText = String.Format(@"DELETE FROM {0} WHERE product_id=?product_id AND is_system=1", _tablesNames["curw"]);

        _deleteProductUrlCommand.Prepare();
        _deleteProductUrlCommand.Parameters.AddWithValue("?product_id", 0);

      }


      if (_insertProductUrlCommand == null)
      {
        string baseQuery = String.Format(@"SELECT cpe.entity_id,cs.store_id,
				 CONCAT('product/',cpe.entity_id) as id_path,
				 CONCAT('catalog/product/view/id/',cpe.entity_id) as target_path,
				 ?request_path AS request_path,
				 1 as is_system
				 FROM {0} as cpe
				 JOIN {1} as cpw ON cpw.product_id=cpe.entity_id
				 JOIN {2} as cs ON cs.website_id=cpw.website_id
				 JOIN {3} as ccp ON ccp.product_id=cpe.entity_id
				 JOIN {4} as cce ON ccp.category_id=cce.entity_id
				 WHERE cpe.entity_id=?product_id", _tablesNames["cpe"], _tablesNames["cpw"], _tablesNames["cs"], _tablesNames["ccp"], _tablesNames["cce"]);

        _insertProductUrlCommand = Connection.CreateCommand();
        _insertProductUrlCommand.CommandType = CommandType.Text;
        _insertProductUrlCommand.CommandText = String.Format(@"INSERT INTO {0} (product_id, store_id, id_path, target_path, request_path, is_system) {1}
                                          ON DUPLICATE KEY UPDATE request_path = VALUES(request_path)", _tablesNames["curw"], baseQuery);



        _insertProductUrlCommand.Prepare();
        _insertProductUrlCommand.Parameters.AddWithValue("?product_id", 0);
        _insertProductUrlCommand.Parameters.AddWithValue("?request_path", String.Empty);

      }




      #endregion


      _getProductUrlCommand.Parameters["?attribute_id"].Value = visibility_attribute.attribute_id;
      _getProductUrlCommand.Parameters["?product_id"].Value = entity.entity_id;

      string pburlk = null;
      string pname = null;
      string result = null;
      using (var reader = _getProductUrlCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          if (reader["attribute_code"].ToString() == "url_key")
            pburlk = reader.SafeGetString("value");

          if (reader["attribute_code"].ToString() == "name")
            pname = reader.SafeGetString("value");

        }
      }

      if (string.IsNullOrEmpty(pburlk))
        pburlk = Slugger.Slug(pname) + ".html";

      _deleteProductUrlCommand.Parameters["?product_id"].Value = entity.entity_id;
      _deleteProductUrlCommand.ExecuteNonQuery();




      _insertProductUrlCommand.Parameters["?product_id"].Value = entity.entity_id;
      _insertProductUrlCommand.Parameters["?request_path"].Value = pburlk;
      _insertProductUrlCommand.ExecuteNonQuery();

      return result;
    }

    #endregion


    #region Images
    public catalog_product_entity_media_gallery CreateMediaGallery(int entity_id, int attribute_id, string value)
    {
      catalog_product_entity_media_gallery mg = new catalog_product_entity_media_gallery() { attribute_id = attribute_id, entity_id = entity_id, value = value };
      return mg;
    }


    private MySqlCommand syncImageCommand = null;

    public int SyncImage(int product_id, int attribute_id, string value, int? refid, IAuditLogAdapter Logger = null)
    {
      //tg, cpev

      if (syncImageCommand == null)
      {
        syncImageCommand = Connection.CreateCommand();
        syncImageCommand.CommandType = CommandType.Text;
        syncImageCommand.CommandText = String.Format(@"INSERT INTO {0} (value_id, attribute_id, entity_id, value) 
                                                       VALUES (?value_id, ?attribute_id, ?entity_id, ?value)
                                                       ON DUPLICATE KEY UPDATE value = VALUES(value)
                                                       ", _tablesNames["tg"]);

        syncImageCommand.Parameters.AddWithValue("?attribute_id", 0);
        syncImageCommand.Parameters.AddWithValue("?entity_id", 0);
        syncImageCommand.Parameters.AddWithValue("?value", string.Empty);
        syncImageCommand.Parameters.AddWithValue("?value_id", 0);
      }

      string query = @"SELECT {0}.value_id FROM {0}";

      var cn = Connection.CreateCommand();
      cn.CommandType = CommandType.Text;
      cn.Parameters.AddWithValue("?attribute_id", attribute_id);
      cn.Parameters.AddWithValue("?entity_id", product_id);

      if (refid.HasValue)
      {
        query += @" JOIN {1} ON {0}.entity_id = {1}.entity_id AND {0}.value = {1}.value AND {1}.attribute_id = ?attribute_id
                  WHERE {0}.entity_id = ?entity_id";
      }
      else
      {
        query += " WHERE value = ?value AND entity_id = ?entity_id AND attribute_id = ?attribute_id";
        cn.Parameters.AddWithValue("?value", value);
      }


      cn.CommandText = String.Format(query, _tablesNames["tg"], _tablesNames["cpev"]);

      try
      {
        object imgid = cn.ExecuteScalar();


        syncImageCommand.Parameters["?entity_id"].Value = product_id;
        syncImageCommand.Parameters["?value_id"].Value = imgid;
        syncImageCommand.Parameters["?attribute_id"].Value = attribute_id;
        syncImageCommand.Parameters["?value"].Value = value;
        syncImageCommand.ExecuteNonQuery();

        if (imgid == null)
          return (int)syncImageCommand.LastInsertedId;
        else
          return Convert.ToInt32(imgid);
      }
      catch (Exception ex)
      {
        if (Logger != null)
        {
          Logger.Error("Sync", ex);
          Logger.Debug(cn.CommandText);
          Logger.Debug("Entity ID: " + product_id.ToString());
          Logger.Debug("Attribute ID: " + attribute_id.ToString());
        }
        throw ex;
      }




    }

    private MySqlCommand selectMaxImagePositionCommand = null;
    private MySqlCommand syncImageLabelsCommand = null;
    public void AddImageToGallery(int product_id, int store_id, string value, int position, string label = "", IAuditLogAdapter Logger = null)
    {
      #region Command Definition
      /* REPLACED by passing of position
      
      if (selectMaxImagePositionCommand == null)
      {
        
        selectMaxImagePositionCommand = Connection.CreateCommand();
        selectMaxImagePositionCommand.CommandType = CommandType.Text;
        selectMaxImagePositionCommand.CommandText = @String.Format(@"SELECT MAX( position ) as maxpos
					 FROM {0} AS emgv
					 JOIN {1} AS emg ON emg.value_id = emgv.value_id AND emg.entity_id = ?entity_id
					 WHERE emgv.store_id=?store_id
			 		 GROUP BY emg.entity_id", _tablesNames["tgv"], _tablesNames["tg"]);
        selectMaxImagePositionCommand.Prepare();
        selectMaxImagePositionCommand.Parameters.AddWithValue("?entity_id", 0);
        selectMaxImagePositionCommand.Parameters.AddWithValue("?store_id", 0);
         
      }
      */
      if (syncImageLabelsCommand == null)
      {
        syncImageLabelsCommand = Connection.CreateCommand();
        syncImageLabelsCommand.CommandType = CommandType.Text;
        syncImageLabelsCommand.CommandText = @String.Format(@"INSERT INTO {0} (value_id, store_id, position, disabled, label)
                                                              VALUES(?value_id, ?store_id, ?position, ?disabled, ?label)
                                                              ON DUPLICATE KEY UPDATE label = VALUES(label), position = VALUES(position)", _tablesNames["tgv"]);

        syncImageLabelsCommand.Prepare();
        syncImageLabelsCommand.Parameters.AddWithValue("?value_id", 0);
        syncImageLabelsCommand.Parameters.AddWithValue("?store_id", 0);
        syncImageLabelsCommand.Parameters.AddWithValue("?position", 0);
        syncImageLabelsCommand.Parameters.AddWithValue("?disabled", false);
        syncImageLabelsCommand.Parameters.AddWithValue("?label", label);
      }
      #endregion


      int imgid = this.SyncImage(product_id, media_gallery_attribute.attribute_id, value, null, Logger);
      if (imgid > 0)
      {

        /* REPLACED by actual position
        selectMaxImagePositionCommand.Parameters["?entity_id"].Value = product_id;
        selectMaxImagePositionCommand.Parameters["?store_id"].Value = store_id;

        
        object result = selectMaxImagePositionCommand.ExecuteScalar();
        int pos = 0;
        if (result != null)
          pos = Convert.ToInt32(result) + 1;
         */

        syncImageLabelsCommand.Parameters["?value_id"].Value = imgid;
        syncImageLabelsCommand.Parameters["?store_id"].Value = store_id;
        syncImageLabelsCommand.Parameters["?position"].Value = position;
        syncImageLabelsCommand.Parameters["?disabled"].Value = false;
        syncImageLabelsCommand.Parameters["?label"].Value = label;

        syncImageLabelsCommand.ExecuteNonQuery();
      }
    }

    private MySqlCommand _deleteGalleryItemsCommand = null;
    internal void DeleteGalleryItems(IEnumerable<catalog_product_entity_media_gallery> list)
    {
      if (_deleteGalleryItemsCommand == null)
      {

        _deleteGalleryItemsCommand = Connection.CreateCommand();
        _deleteGalleryItemsCommand.CommandType = CommandType.Text;
        _deleteGalleryItemsCommand.CommandText = String.Format(@"DELETE FROM {0} WHERE value_id = ?value_id", _tablesNames["tg"]);

        _deleteGalleryItemsCommand.Prepare();
        _deleteGalleryItemsCommand.Parameters.AddWithValue("?value_id", 0);

      }

      foreach (var item in list)
      {

        _deleteGalleryItemsCommand.Parameters["?value_id"].Value = item.value_id;

        _deleteGalleryItemsCommand.ExecuteNonQuery();
      }
    }




    private MySqlCommand _resetGalleryCommand = null;
    public void ResetGallery(catalog_product_entity entity, int store_id)
    {


      if (_resetGalleryCommand == null)
      {

        _resetGalleryCommand = Connection.CreateCommand();
        _resetGalleryCommand.CommandType = CommandType.Text;
        _resetGalleryCommand.CommandText = String.Format(@"DELETE emgv,emg FROM {0} AS emgv JOIN {1} AS emg ON (emgv.value_id = emg.value_id AND emgv.store_id = ?store_id)
                                        WHERE emg.entity_id =?entity_id AND emg.attribute_id =?attribute_id", _tablesNames["tgv"], _tablesNames["tg"]);

        _resetGalleryCommand.Prepare();
        _resetGalleryCommand.Parameters.AddWithValue("?entity_id", 0);
        _resetGalleryCommand.Parameters.AddWithValue("?attribute_id", 0);
        _resetGalleryCommand.Parameters.AddWithValue("?store_id", 0);

      }

      _resetGalleryCommand.Parameters["?entity_id"].Value = entity.entity_id;
      _resetGalleryCommand.Parameters["?store_id"].Value = store_id;
      _resetGalleryCommand.Parameters["?attribute_id"].Value = media_gallery_attribute.attribute_id;

      _resetGalleryCommand.ExecuteNonQuery();


    }
    #endregion


    #region Indexes

    #region CatalogCategory Index

    private MySqlCommand _selectPathsCommand = null;
    private MySqlCommand _deleteCatalogCategoryProductIndexCommand = null;
    private MySqlCommand _insertCatalogCategoryProductIndexCommand = null;

    public void UpdateCatalogCategoryProductIndex(catalog_product_entity entity)
    {
      #region Build Commmands
      if (_selectPathsCommand == null)
      {
        _selectPathsCommand = Connection.CreateCommand();
        _selectPathsCommand.CommandType = CommandType.Text;
        _selectPathsCommand.CommandText = String.Format(@"SELECT cce.path 
                  FROM {0} as ccp 
                  JOIN {1} as cce ON ccp.category_id=cce.entity_id
                  WHERE ccp.product_id=?product_id", _tablesNames["ccp"], _tablesNames["cce"]);

        _selectPathsCommand.Prepare();
        _selectPathsCommand.Parameters.AddWithValue("?product_id", 0);

      }
      if (_deleteCatalogCategoryProductIndexCommand == null)
      {
        _deleteCatalogCategoryProductIndexCommand = Connection.CreateCommand();
        _deleteCatalogCategoryProductIndexCommand.CommandType = CommandType.Text;
        _deleteCatalogCategoryProductIndexCommand.CommandText = @"DELETE FROM catalog_category_product_index WHERE product_id = ?product_id";
        _deleteCatalogCategoryProductIndexCommand.Prepare();
        _deleteCatalogCategoryProductIndexCommand.Parameters.AddWithValue("?product_id", 0);
      }

      if (_insertCatalogCategoryProductIndexCommand == null)
      {
        _insertCatalogCategoryProductIndexCommand = Connection.CreateCommand();
        _insertCatalogCategoryProductIndexCommand.CommandType = CommandType.Text;
        _insertCatalogCategoryProductIndexCommand.CommandText = String.Format(@"
	        INSERT IGNORE INTO {0}
				 SELECT cce.entity_id as category_id,ccp.product_id,ccp.position,IF(cce.entity_id=ccp.category_id,1,0) as is_parent,csg.default_store_id,cpei.value as visibility 
				 FROM {1} as ccp
				 JOIN {2} as cpe ON ccp.product_id=cpe.entity_id
				 JOIN {3} as cpei ON cpei.attribute_id=?attribute_id AND cpei.entity_id=cpe.entity_id
				 JOIN {4} as cce ON FIND_IN_SET( cce.entity_id ,?category_list) != 0
				 JOIN {5} as csg ON csg.root_category_id=SUBSTR(SUBSTRING_INDEX(cce.path,'/',2),LOCATE('/',SUBSTRING_INDEX(cce.path,'/',2))+1)
				 WHERE ccp.product_id=?product_id
	    		 ORDER by is_parent DESC,csg.default_store_id,cce.entity_id",
              _tablesNames["ccpi"], _tablesNames["ccp"], _tablesNames["cpe"], _tablesNames["cpei"], _tablesNames["cce"], _tablesNames["csg"]);

        _insertCatalogCategoryProductIndexCommand.Prepare();

        _insertCatalogCategoryProductIndexCommand.Parameters.AddWithValue("?attribute_id", 91);
        _insertCatalogCategoryProductIndexCommand.Parameters.AddWithValue("?product_id", 0);
        _insertCatalogCategoryProductIndexCommand.Parameters.AddWithValue("?category_list", string.Empty);

      }


      #endregion

      _deleteCatalogCategoryProductIndexCommand.Parameters["?product_id"].Value = entity.entity_id;
      _deleteCatalogCategoryProductIndexCommand.ExecuteNonQuery();

      _selectPathsCommand.Parameters["?product_id"].Value = entity.entity_id;
      List<string> categoryIds = new List<string>();
      using (var reader = _selectPathsCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          string pathLine = reader.GetString("path");
          string[] parts = pathLine.Split('/');

          categoryIds.AddRange(parts);

        }
      }

      var parm = String.Join(",", categoryIds.Distinct().OrderBy(x => x).Skip(1).ToArray());

      _insertCatalogCategoryProductIndexCommand.Parameters["?product_id"].Value = entity.entity_id;
      _insertCatalogCategoryProductIndexCommand.Parameters["?category_list"].Value = parm;
      _insertCatalogCategoryProductIndexCommand.ExecuteNonQuery();




    }


    #endregion

    public void TurnOffCategories(List<int> categoryList, bool MagentoIgnoreCategoryDisabling)
    {
      if (categoryList.Count == 0)
        return; // nothing to do

      string attributeCodeQuery;

      var updateCommand = Connection.CreateCommand();
      updateCommand.CommandType = CommandType.Text;

      if (MagentoIgnoreCategoryDisabling)
        attributeCodeQuery = "'include_in_menu'";
      else
        attributeCodeQuery = "'is_active','include_in_menu'";

      var parm = String.Join(",", categoryList.Distinct().OrderBy(x => x).ToArray());
      updateCommand.CommandText = String.Format(@"UPDATE catalog_category_entity_int
                    SET value = 0
                    WHERE attribute_id IN ( select attribute_id FROM eav_attribute WHERE entity_type_id = 3 AND attribute_code IN ({0}))
                    AND entity_id NOT IN (SELECT entity_id FROM catalog_category_entity WHERE level = 0)
                    AND entity_id IN ({1})", attributeCodeQuery, parm);

      updateCommand.ExecuteNonQuery();

      var deactivateQuery = Connection.CreateCommand();
      deactivateQuery.CommandType = CommandType.Text;
      deactivateQuery.CommandText = string.Format("update catalog_category_entity set updated_at = now() where entity_id in ({0})", parm);

      deactivateQuery.ExecuteNonQuery();
    }

    #endregion


    private MySqlCommand _getGalleryItemsCommand = null;
    public IEnumerable<catalog_product_entity_media_gallery> GetGalleryItems(catalog_product_entity entity)
    {
      if (_getGalleryItemsCommand == null)
      {
        _getGalleryItemsCommand = Connection.CreateCommand();
        _getGalleryItemsCommand.CommandType = CommandType.Text;
        _getGalleryItemsCommand.CommandText = String.Format(@"SELECT * FROM {0} WHERE entity_id = ?entity_id AND attribute_id = ?attribute_id", _tablesNames["tg"]);


        _getGalleryItemsCommand.Prepare();
        _getGalleryItemsCommand.Parameters.AddWithValue("?attribute_id", media_gallery_attribute.attribute_id);
        _getGalleryItemsCommand.Parameters.AddWithValue("?entity_id", 0);
      }

      _getGalleryItemsCommand.Parameters["?entity_id"].Value = entity.entity_id;

      using (var reader = _getGalleryItemsCommand.ExecuteReader())
      {

        return Mapper.Map<IDataReader, IList<catalog_product_entity_media_gallery>>(reader);

      }

    }

    internal void CleanRelatedProducts(IEnumerable<int> listOfExistsProducts)
    {
      if (!listOfExistsProducts.Any()) return;
      using (var cleanupRelationsCommand = Connection.CreateCommand())
      {
        cleanupRelationsCommand.CommandType = CommandType.Text;
        cleanupRelationsCommand.CommandText = String.Format(@"
          DELETE FROM {0} 
          WHERE product_id NOT IN ({1})",
          _tablesNames["cpl"], String.Join(",", listOfExistsProducts.ToArray()));
        cleanupRelationsCommand.ExecuteNonQuery();
      }
    }

    private MySqlCommand _syncProductLinkCommand = null;
    internal void SyncProductLink(int magentoConfigurableID, Dictionary<int, int> magentoSimpleIDs, int linkTypeID = 1)
    {
      //catalog_product_link
      #region Command Definition
      if (_syncProductLinkCommand == null)
      {
        _syncProductLinkCommand = Connection.CreateCommand();
        _syncProductLinkCommand.CommandType = CommandType.Text;
        _syncProductLinkCommand.CommandText = String.Format(@"
          INSERT 
          IGNORE 
            INTO {0} (product_id, linked_product_id, link_type_id, order_index) 
            VALUES (?product_id, ?linked_product_id, ?link_type_id, ?order_index) 
          ON DUPLICATE KEY 
            UPDATE order_index = VALUES(order_index)"
          , _tablesNames["cpl"]);
        _syncProductLinkCommand.Prepare();
        _syncProductLinkCommand.Parameters.AddWithValue("?product_id", 0);
        _syncProductLinkCommand.Parameters.AddWithValue("?linked_product_id", 0);
        _syncProductLinkCommand.Parameters.AddWithValue("?link_type_id", 0);
        _syncProductLinkCommand.Parameters.AddWithValue("?order_index", 0);
      }
      #endregion

      _syncProductLinkCommand.Parameters["?product_id"].Value = magentoConfigurableID;
      _syncProductLinkCommand.Parameters["?link_type_id"].Value = linkTypeID;

      foreach (var childID in magentoSimpleIDs)
      {
        _syncProductLinkCommand.Parameters["?linked_product_id"].Value = childID.Key;
        _syncProductLinkCommand.Parameters["?order_index"].Value = childID.Value;
        _syncProductLinkCommand.ExecuteNonQuery();
      }

      // cleanup
      using (var cleanupRelationsCommand = Connection.CreateCommand())
      {
        cleanupRelationsCommand.CommandType = CommandType.Text;
        cleanupRelationsCommand.CommandText = String.Format("DELETE FROM {0} WHERE product_id = {1} AND linked_product_id NOT IN ({2}) AND link_type_id = {3}",
          _tablesNames["cpl"], magentoConfigurableID, String.Join(",", magentoSimpleIDs.Select(c => c.Key).ToList()), linkTypeID);
        cleanupRelationsCommand.ExecuteNonQuery();
      }
    }



    private MySqlCommand _syncProductRelationsCommand = null;
    private MySqlCommand _syncProductSuperLinkCommand = null;
    internal void SyncProductRelations(int parent_id, List<int> child_ids)
    {

      #region Command Definition


      if (_syncProductSuperLinkCommand == null)
      {
        _syncProductSuperLinkCommand = Connection.CreateCommand();
        _syncProductSuperLinkCommand.CommandType = CommandType.Text;
        _syncProductSuperLinkCommand.CommandText = String.Format(@"INSERT IGNORE INTO {0} (parent_id, product_id) VALUES (?parent_id, ?product_id)", _tablesNames["cpsl"]);
        _syncProductSuperLinkCommand.Prepare();
        _syncProductSuperLinkCommand.Parameters.AddWithValue("?product_id", 0);
        _syncProductSuperLinkCommand.Parameters.AddWithValue("?parent_id", 0);
      }

      if (_syncProductRelationsCommand == null)
      {
        _syncProductRelationsCommand = Connection.CreateCommand();
        _syncProductRelationsCommand.CommandType = CommandType.Text;
        _syncProductRelationsCommand.CommandText = String.Format(@"INSERT IGNORE INTO {0} (parent_id, child_id) VALUES (?parent_id, ?child_id)", _tablesNames["cpr"]);
        _syncProductRelationsCommand.Prepare();
        _syncProductRelationsCommand.Parameters.AddWithValue("?parent_id", 0);
        _syncProductRelationsCommand.Parameters.AddWithValue("?child_id", 0);
      }

      #endregion


      _syncProductRelationsCommand.Parameters["?parent_id"].Value = parent_id;
      _syncProductSuperLinkCommand.Parameters["?parent_id"].Value = parent_id;

      foreach (int child_id in child_ids)
      {
        _syncProductRelationsCommand.Parameters["?child_id"].Value = child_id;
        _syncProductRelationsCommand.ExecuteNonQuery();

        _syncProductSuperLinkCommand.Parameters["?product_id"].Value = child_id;
        _syncProductSuperLinkCommand.ExecuteNonQuery();
      }

      // cleanup
      using (var cleanupRelationsCommand = Connection.CreateCommand())
      {
        cleanupRelationsCommand.CommandType = CommandType.Text;

        if (child_ids.Count > 0)
          cleanupRelationsCommand.CommandText = String.Format("DELETE FROM {0} WHERE parent_id = {1} AND child_id NOT IN ({2})", _tablesNames["cpr"], parent_id, String.Join(",", child_ids));
        else
          cleanupRelationsCommand.CommandText = String.Format("DELETE FROM {0} WHERE parent_id = {1} ", _tablesNames["cpr"], parent_id);
        cleanupRelationsCommand.ExecuteNonQuery();
      }

      // cleanup
      using (var cleanupSuperLinkCommand = Connection.CreateCommand())
      {
        cleanupSuperLinkCommand.CommandType = CommandType.Text;
        if (child_ids.Count > 0)
          cleanupSuperLinkCommand.CommandText = String.Format("DELETE FROM {0} WHERE parent_id = {1} AND product_id NOT IN ({2})", _tablesNames["cpsl"], parent_id, String.Join(",", child_ids));
        else
          cleanupSuperLinkCommand.CommandText = String.Format("DELETE FROM {0} WHERE parent_id = {1} ", _tablesNames["cpsl"], parent_id);
        cleanupSuperLinkCommand.ExecuteNonQuery();
      }

    }

    private MySqlCommand _selectProductSuperAttributeCommand = null;
    private MySqlCommand _syncProductSuperAttributesCommand = null;
    private MySqlCommand _syncProductSuperAttributeLabelCommand = null;
    internal void SyncProductSuperAttributes(List<catalog_product_super_attribute> configurable_attributes)
    {

      //_tablesNames.Add("cpsa", TableName("catalog_product_super_attribute"));
      //      _tablesNames.Add("cpsal", TableName("catalog_product_super_attribute_label"));

      #region Command Definition

      if (_selectProductSuperAttributeCommand == null)
      {
        _selectProductSuperAttributeCommand = Connection.CreateCommand();
        _selectProductSuperAttributeCommand.CommandType = CommandType.Text;
        _selectProductSuperAttributeCommand.CommandText = String.Format(@"SELECT product_super_attribute_id FROM {0} WHERE product_id = ?product_id AND attribute_id = ?attribute_id", _tablesNames["cpsa"]);
        _selectProductSuperAttributeCommand.Prepare();
        _selectProductSuperAttributeCommand.Parameters.AddWithValue("?product_id", 0);
        _selectProductSuperAttributeCommand.Parameters.AddWithValue("?attribute_id", 0);

      }

      if (_syncProductSuperAttributesCommand == null)
      {
        _syncProductSuperAttributesCommand = Connection.CreateCommand();
        _syncProductSuperAttributesCommand.CommandType = CommandType.Text;
        _syncProductSuperAttributesCommand.CommandText = String.Format(@"INSERT INTO {0} (product_super_attribute_id, product_id, attribute_id, position) 
                                                                  VALUES (?product_super_attribute_id, ?product_id, ?attribute_id, ?position)
																																	ON DUPLICATE KEY UPDATE position = VALUES(position)
                                                                  ", _tablesNames["cpsa"]);
        _syncProductSuperAttributesCommand.Prepare();
        _syncProductSuperAttributesCommand.Parameters.AddWithValue("?product_super_attribute_id", 0);
        _syncProductSuperAttributesCommand.Parameters.AddWithValue("?product_id", 0);
        _syncProductSuperAttributesCommand.Parameters.AddWithValue("?attribute_id", 0);
        _syncProductSuperAttributesCommand.Parameters.AddWithValue("?position", 0);
      }




      if (_syncProductSuperAttributeLabelCommand == null)
      {
        _syncProductSuperAttributeLabelCommand = Connection.CreateCommand();
        _syncProductSuperAttributeLabelCommand.CommandType = CommandType.Text;
        _syncProductSuperAttributeLabelCommand.CommandText = String.Format(@"INSERT INTO {0} (product_super_attribute_id, store_id, use_default, value)
                                                                              VALUES (?product_super_attribute_id, ?store_id, ?use_default, ?value)
                                                                              ON DUPLICATE KEY UPDATE use_default = VALUES(use_default), value = VALUES(value)", _tablesNames["cpsal"]);

        _syncProductSuperAttributeLabelCommand.Prepare();
        _syncProductSuperAttributeLabelCommand.Parameters.AddWithValue("?product_super_attribute_id", 0);
        _syncProductSuperAttributeLabelCommand.Parameters.AddWithValue("?store_id", 0);
        _syncProductSuperAttributeLabelCommand.Parameters.AddWithValue("?use_default", 1);
        _syncProductSuperAttributeLabelCommand.Parameters.AddWithValue("?value", string.Empty);
      }

      #endregion


      foreach (var attribute in configurable_attributes)
      {

        _selectProductSuperAttributeCommand.Parameters["?product_id"].Value = attribute.product_id;
        _selectProductSuperAttributeCommand.Parameters["?attribute_id"].Value = attribute.attribute_id;
        var obj = _selectProductSuperAttributeCommand.ExecuteScalar();
        if (obj != null)
          attribute.product_super_attribute_id = Convert.ToInt32(obj);


        _syncProductSuperAttributesCommand.Parameters["?product_id"].Value = attribute.product_id;
        _syncProductSuperAttributesCommand.Parameters["?product_super_attribute_id"].Value = attribute.product_super_attribute_id;
        _syncProductSuperAttributesCommand.Parameters["?attribute_id"].Value = attribute.attribute_id;
        _syncProductSuperAttributesCommand.Parameters["?position"].Value = attribute.position;
        _syncProductSuperAttributesCommand.ExecuteNonQuery();

        if (attribute.product_super_attribute_id == 0)
          attribute.product_super_attribute_id = (int)_syncProductSuperAttributesCommand.LastInsertedId;


        //label
        _syncProductSuperAttributeLabelCommand.Parameters["?product_super_attribute_id"].Value = attribute.product_super_attribute_id;
        _syncProductSuperAttributeLabelCommand.Parameters["?value"].Value = attribute.label;
        _syncProductSuperAttributeLabelCommand.Parameters["?use_default"].Value = attribute.use_default;

        _syncProductSuperAttributeLabelCommand.ExecuteNonQuery();

      }

      if (configurable_attributes.Count > 0)
      {
        // cleanup
        using (var cleanupAttributesCommand = Connection.CreateCommand())
        {
          cleanupAttributesCommand.CommandType = CommandType.Text;
          cleanupAttributesCommand.CommandText = String.Format("DELETE FROM {0} WHERE product_id = {1} AND product_super_attribute_id NOT IN ({2})", _tablesNames["cpsa"], configurable_attributes.First().product_id, String.Join(",", configurable_attributes.Select(x => x.product_super_attribute_id)));
          cleanupAttributesCommand.ExecuteNonQuery();
        }
      }
    }


    public void CleanupCategoryProducts(int entity_id, List<int> currentCatIds, List<int> websiteCatIds)
    {
      if (websiteCatIds == null)
        return;
      if (currentCatIds == null || currentCatIds.Count == 0)
        return;

      using (var cleanupCatCommand = Connection.CreateCommand())
      {
        cleanupCatCommand.CommandType = CommandType.Text;
        cleanupCatCommand.CommandText = String.Format("DELETE FROM catalog_category_product WHERE product_id = {0} AND (category_id NOT IN ({1}) AND category_id in ({2}))",
           entity_id, String.Join(",", currentCatIds), String.Join(",", websiteCatIds));
        cleanupCatCommand.ExecuteNonQuery();
      }
    }

    /// <summary>
    ///  Removes a product from any category from a list of website categories
    /// </summary>
    /// <param name="entity_id">The entity id of the product</param>
    /// <param name="websiteCatIds">The website category ids</param>
    public void CleanupCategoryProductRelations(int entity_id, List<int> websiteCatIds)
    {
      if (websiteCatIds == null) return;

      using (var cleanupCatCommand = Connection.CreateCommand())
      {
        cleanupCatCommand.CommandType = CommandType.Text;
        cleanupCatCommand.CommandText = String.Format("DELETE FROM catalog_category_product WHERE product_id = {0} AND category_id in ({1})",
           entity_id, String.Join(",", websiteCatIds));

        cleanupCatCommand.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Synchronize an existing attribute set with another one (Default-attribute set for example). 
    /// This is useful if new attributes have been added to the source attribute set that also should be added to the destination attribute set.
    /// </summary>
    public void SyncMissingEntityAttribute(int entity_type_id, int source_attribute_set_id, int destination_attribute_set_id)
    {
      if (source_attribute_set_id != destination_attribute_set_id)
      {
        var selectEntityAttributeCommand = Connection.CreateCommand();

        selectEntityAttributeCommand.CommandText = @"
select      *
from        eav_entity_attribute
where       entity_type_id = ?entity_type_id and attribute_set_id = ?source_attribute_set_id and attribute_id not in
(
    select  attribute_id 
    from    eav_entity_attribute 
    where   entity_type_id = ?entity_type_id 
    and     attribute_set_id = ?destination_attribute_set_id   
)";
        selectEntityAttributeCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);
        selectEntityAttributeCommand.Parameters.AddWithValue("?source_attribute_set_id", source_attribute_set_id);
        selectEntityAttributeCommand.Parameters.AddWithValue("?destination_attribute_set_id", destination_attribute_set_id);

        var selectAttributeGroupCommand = Connection.CreateCommand();

        selectAttributeGroupCommand.CommandText = @"
select      attribute_group_id
from        eav_attribute_group
where       attribute_set_id = ?attribute_set_id and attribute_group_name =
(
    select  attribute_group_name 
    from    eav_attribute_group 
    where   attribute_group_id = ?attribute_group_id   
)";
        selectAttributeGroupCommand.Parameters.AddWithValue("?attribute_set_id", destination_attribute_set_id);
        selectAttributeGroupCommand.Parameters.Add("?attribute_group_id", MySqlDbType.Int32);

        var insertEntityAttributeCommand = Connection.CreateCommand();

        insertEntityAttributeCommand.CommandText = @"
insert into eav_entity_attribute ( entity_type_id , attribute_set_id , attribute_group_id , attribute_id , sort_order )
values ( ?entity_type_id , ?attribute_set_id , ?attribute_group_id , ?attribute_id , ?sort_order )";
        insertEntityAttributeCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);
        insertEntityAttributeCommand.Parameters.AddWithValue("?attribute_set_id", destination_attribute_set_id);
        insertEntityAttributeCommand.Parameters.Add("?attribute_group_id", MySqlDbType.Int16);
        insertEntityAttributeCommand.Parameters.Add("?attribute_id", MySqlDbType.Int16);
        insertEntityAttributeCommand.Parameters.Add("?sort_order", MySqlDbType.Int16);

        var entityAttributeList = new List<eav_entity_attribute>();

        using (var reader = selectEntityAttributeCommand.ExecuteReader())
        {
          while (reader.Read())
          {
            entityAttributeList.Add(Mapper.Map<eav_entity_attribute>(reader));
          }
        }

        foreach (var entityAttribute in entityAttributeList)
        {
          selectAttributeGroupCommand.Parameters["?attribute_group_id"].Value = entityAttribute.attribute_group_id;

          insertEntityAttributeCommand.Parameters["?attribute_group_id"].Value = Convert.ToInt32(selectAttributeGroupCommand.ExecuteScalar());
          insertEntityAttributeCommand.Parameters["?attribute_id"].Value = entityAttribute.attribute_id;
          insertEntityAttributeCommand.Parameters["?sort_order"].Value = entityAttribute.sort_order;
          insertEntityAttributeCommand.ExecuteNonQuery();
        }
      }
    }


    private MySqlCommand _updateCatalogProductEntityLastModificationTimeCommand = null;
    internal void SetProductLastModificationTime(int entity_id)
    {
      if (_updateCatalogProductEntityLastModificationTimeCommand == null)
      {
        _updateCatalogProductEntityLastModificationTimeCommand = Connection.CreateCommand();
        _updateCatalogProductEntityLastModificationTimeCommand.CommandType = CommandType.Text;
        _updateCatalogProductEntityLastModificationTimeCommand.CommandText = @"update catalog_product_entity set updated_at = now() where entity_id = ?entity_id";
        _updateCatalogProductEntityLastModificationTimeCommand.Prepare();

        _updateCatalogProductEntityLastModificationTimeCommand.Parameters.AddWithValue("?entity_id", 0);

      }


      _updateCatalogProductEntityLastModificationTimeCommand.Parameters["?entity_id"].Value = entity_id;

      _updateCatalogProductEntityLastModificationTimeCommand.ExecuteNonQuery();
    }


  }
}
