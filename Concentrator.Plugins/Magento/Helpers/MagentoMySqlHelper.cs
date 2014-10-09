using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using Concentrator.Plugins.Magento.Models;
using AutoMapper;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class MagentoMySqlHelper : IDisposable
  {

    protected const int PRODUCT_ENTITY_TYPE_ID = 4;
    protected const int CATEGORY_ENTITY_TYPE_ID = 3;
    protected const int CUSTOMER_ENTITY_TYPE_ID = 1;
    protected const int CUSTOMER_ADDRESS_ENTITY_TYPE_ID = 2;
    private string _tablePrefix = String.Empty;

    protected MySqlConnection Connection = null;


    protected Dictionary<string, string> _tablesNames = new Dictionary<string, string>();

    /// <summary>
    /// Gets all tax rates and their associated tax_class ids 
    /// </summary>
    /// <returns>A rate, tax class id dictionary</returns>
    public Dictionary<decimal, int> GetTaxClasses()
    {

      Dictionary<decimal, int> result = new Dictionary<decimal, int>();

      var cs = TableName("tax_class");
      var tc = TableName("tax_calculation");
      var tcr = tc + "_rate";

      var cn = Connection.CreateCommand();
      cn.CommandType = CommandType.Text;
      cn.CommandText = string.Format(@"
													select distinct tcr.rate, cs.class_id  from {0} cs
													inner join {1} tc on tc.product_tax_class_id = cs.class_id
													inner join {2} tcr on tcr.tax_calculation_rate_id = tc.tax_calculation_rate_id	
												", cs, tc, tcr);

      using (var reader = cn.ExecuteReader())
      {
        while (reader.Read())
        {
          if (!result.ContainsKey(Convert.ToDecimal(reader["rate"])))
            result.Add(Convert.ToDecimal(reader["rate"]), Convert.ToInt32(reader["class_id"]));
        }
      }
      return result;
    }

    public MagentoMySqlHelper(string connectionString)
    {
      Connection = new MySqlConnection(connectionString);
     Connection.Open();


      AutoMapper.Mapper.CreateMap<IDataReader, catalog_category_entity>();
      AutoMapper.Mapper.CreateMap<IDataRecord, catalog_category_entity>();

      AutoMapper.Mapper.CreateMap<IDataReader, catalog_product_entity>();
      AutoMapper.Mapper.CreateMap<IDataRecord, catalog_product_entity>();

      AutoMapper.Mapper.CreateMap<IDataReader, catalog_product_entity_media_gallery>();
      AutoMapper.Mapper.CreateMap<IDataRecord, catalog_product_entity_media_gallery>();



      AutoMapper.Mapper.CreateMap<IDataReader, eav_attribute>();
      AutoMapper.Mapper.CreateMap<IDataRecord, eav_attribute>();
      AutoMapper.Mapper.CreateMap<IDataReader, eav_attribute_group>();
      AutoMapper.Mapper.CreateMap<IDataReader, eav_attribute_set>();
      AutoMapper.Mapper.CreateMap<IDataRecord, eav_attribute_set>();
      AutoMapper.Mapper.CreateMap<IDataReader, eav_entity_attribute>();
      AutoMapper.Mapper.CreateMap<IDataRecord, eav_attribute_group>();




      AutoMapper.Mapper.CreateMap<IDataReader, eav_attribute_label>();
      AutoMapper.Mapper.CreateMap<IDataRecord, eav_attribute_label>();

      AutoMapper.Mapper.CreateMap<IDataReader, path_info>();
      AutoMapper.Mapper.CreateMap<IDataRecord, path_info>();


      AutoMapper.Mapper.CreateMap<IDataReader, catalog_product_relation>();
      AutoMapper.Mapper.CreateMap<IDataRecord, catalog_product_relation>();

      AutoMapper.Mapper.CreateMap<IDataReader, catalog_product_link>();
      AutoMapper.Mapper.CreateMap<IDataRecord, catalog_product_link>();



      _tablesNames.Add("cpe", TableName("catalog_product_entity"));
      _tablesNames.Add("cce", TableName("catalog_category_entity"));
      _tablesNames.Add("ccp", TableName("catalog_category_product"));
      _tablesNames.Add("cpw", TableName("catalog_product_website"));
      _tablesNames.Add("cpr", TableName("catalog_product_relation"));

      _tablesNames.Add("cpsa", TableName("catalog_product_super_attribute"));
      _tablesNames.Add("cpsal", TableName("catalog_product_super_attribute_label"));
      _tablesNames.Add("cpsl", TableName("catalog_product_super_link"));

      _tablesNames.Add("cs", TableName("core_store"));
      _tablesNames.Add("csg", TableName("core_store_group"));
      _tablesNames.Add("cpev", TableName("catalog_product_entity_varchar"));
      _tablesNames.Add("cpei", TableName("catalog_product_entity_int"));
      _tablesNames.Add("ccev", TableName("catalog_category_entity_varchar"));
      _tablesNames.Add("ea", TableName("eav_attribute"));
      _tablesNames.Add("eas", TableName("eav_attribute_set"));
      _tablesNames.Add("eag", TableName("eav_attribute_group"));
      _tablesNames.Add("eea", TableName("eav_entity_attribute"));
      _tablesNames.Add("ccpi", TableName("catalog_category_product_index"));
      _tablesNames.Add("curw", TableName("core_url_rewrite"));
      _tablesNames.Add("tg", TableName("catalog_product_entity_media_gallery"));
      _tablesNames.Add("tgv", TableName("catalog_product_entity_media_gallery_value"));
      _tablesNames.Add("eao", TableName("eav_attribute_option"));
      _tablesNames.Add("eaov", TableName("eav_attribute_option_value"));
      _tablesNames.Add("customer_entity", TableName("customer_entity"));
      _tablesNames.Add("customer_eav_attribute", TableName("customer_eav_attribute"));
      _tablesNames.Add("customer_address_entity", TableName("customer_address_entity"));
      _tablesNames.Add("customer_address_entity_int", TableName("customer_address_entity_int"));

      _tablesNames.Add("cpl", TableName("catalog_product_link"));


    }

    #region Attributes

    private eav_attribute_group GetDefaultAttributeGroup(int entity_type_id = PRODUCT_ENTITY_TYPE_ID)
    {
      var eas = TableName("eav_attribute_set");
      var eag = TableName("eav_attribute_group");



      var attributeListCommand = Connection.CreateCommand();

      attributeListCommand.CommandText = String.Format(@"
                                        SELECT * FROM {0} 
                                        WHERE attribute_set_id = (SELECT attribute_set_id FROM {1} WHERE attribute_set_name = 'Default' AND entity_type_id = ?entity_type_id) 
                                        AND default_id = 1", eag, eas);
      attributeListCommand.CommandType = System.Data.CommandType.Text;
      attributeListCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);

      using (var reader = attributeListCommand.ExecuteReader())
      {
        if (reader.Read())
        {
          IDataRecord rec = reader as IDataRecord;
          return Mapper.Map<IDataRecord, eav_attribute_group>(rec);
        }
        return null;
      }
    }

    public eav_attribute_set GetAttributeSet(string name = "", bool default_set = false, int entity_type_id = PRODUCT_ENTITY_TYPE_ID)
    {

      if (entity_type_id == 0)
        entity_type_id = PRODUCT_ENTITY_TYPE_ID;

      var eas = TableName("eav_attribute_set");

      if (default_set)
        name = "Default";

      var attributeListCommand = Connection.CreateCommand();
      attributeListCommand.CommandText = String.Format(@"SELECT * FROM {0} WHERE entity_type_id = ?entity_type_id AND attribute_set_name = ?attribute_set_name", eas);
      attributeListCommand.CommandType = System.Data.CommandType.Text;
      attributeListCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);
      attributeListCommand.Parameters.AddWithValue("?attribute_set_name", name);

      using (var reader = attributeListCommand.ExecuteReader())
      {
        if (reader.Read())
          return Mapper.Map<IDataRecord, eav_attribute_set>(reader);

      }

      return null;
    }


    internal SortedDictionary<string, int> GetActiveSkus()
    {
      int entity_type_id = PRODUCT_ENTITY_TYPE_ID;

      SortedDictionary<string, int> result = new SortedDictionary<string, int>();

      var getSkuCommand = Connection.CreateCommand();
      getSkuCommand.CommandText = @"SELECT cpe.sku, cpe.entity_id FROM catalog_product_entity cpe
                                  inner join catalog_product_entity_int ac on cpe.entity_id = ac.entity_id and ac.store_id = 0
                                  inner join eav_attribute active on ac.attribute_id = active.attribute_id
                                  WHERE cpe.entity_type_id = ?entity_type_id and active.attribute_code = 'status' and ac.value = 1
                                  ";

      getSkuCommand.CommandType = System.Data.CommandType.Text;
      getSkuCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);

      using (var reader = getSkuCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          result.Add(reader.GetString(0), reader.GetInt32(1));
        }
      }

      return result;
    }

    internal SortedDictionary<string, catalog_product_entity> GetSkuList()
    {
      int entity_type_id = PRODUCT_ENTITY_TYPE_ID;

      SortedDictionary<string, catalog_product_entity> result = new SortedDictionary<string, catalog_product_entity>();

      var getSkuCommand = Connection.CreateCommand();
      getSkuCommand.CommandText = @"SELECT * FROM catalog_product_entity WHERE entity_type_id = ?entity_type_id";
      getSkuCommand.CommandType = System.Data.CommandType.Text;
      getSkuCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);

      using (var reader = getSkuCommand.ExecuteReader())
      {
        var atts = Mapper.Map<IDataReader, IList<catalog_product_entity>>(reader);

        foreach (var at in atts)
          result.Add(at.sku, at);
      }

      return result;
    }

    public List<eav_attribute_label> GetAttributeLabels()
    {
      List<eav_attribute_label> result = new List<eav_attribute_label>();

      var attributeLabelListCommand = Connection.CreateCommand();
      attributeLabelListCommand.CommandText = @"SELECT * FROM eav_attribute_label";
      attributeLabelListCommand.CommandType = System.Data.CommandType.Text;

      using (var reader = attributeLabelListCommand.ExecuteReader())
      {
        return Mapper.Map<IDataReader, List<eav_attribute_label>>(reader);
      }

    }

    private MySqlCommand _selectWebsiteCodeCommand = null;
    private MySqlCommand _selectShowDiscountCommand = null;
    private MySqlCommand _selectShowSaleCommand = null;
    private MySqlCommand _createShowDiscountCommand = null;
    private MySqlCommand _createShowSaleCommand = null;
    private MySqlCommand _updateShowDiscountCommand = null;
    private MySqlCommand _updateShowSaleCommand = null;
    public void InsertSoldenPeriod(bool isSolden, string coreWebsiteCode)
    {
      if (_selectWebsiteCodeCommand == null)
      {
        _selectWebsiteCodeCommand = Connection.CreateCommand();
        _selectWebsiteCodeCommand.CommandType = CommandType.Text;
        _selectWebsiteCodeCommand.CommandText = String.Format(@"SELECT website_id FROM core_website WHERE code = '{0}'", coreWebsiteCode);
        _selectWebsiteCodeCommand.Prepare();
      }

      var result = _selectWebsiteCodeCommand.ExecuteScalar();

      if (result != null && result != DBNull.Value)
      {
        var websiteID = Convert.ToInt16(result);

        if (_selectShowDiscountCommand == null)
        {
          _selectShowDiscountCommand = Connection.CreateCommand();
          _selectShowDiscountCommand.CommandType = CommandType.Text;
          _selectShowDiscountCommand.CommandText = string.Format(@"
            SELECT config_id 
            FROM core_config_data 
            WHERE path = 'at_sales/salecategory/showdiscount' and scope_id = {0} and scope = 'websites'"
            , websiteID);
          _selectShowDiscountCommand.Prepare();
        }

        if (_selectShowSaleCommand == null)
        {
          _selectShowSaleCommand = Connection.CreateCommand();
          _selectShowSaleCommand.CommandType = CommandType.Text;
          _selectShowSaleCommand.CommandText = string.Format(@"
            SELECT config_id 
            FROM core_config_data 
            WHERE path = 'at_sales/salecategory/showsales' and scope_id = {0} and scope = 'websites'"
            , websiteID);
          _selectShowSaleCommand.Prepare();
        }

        if (_createShowDiscountCommand == null)
        {
          _createShowDiscountCommand = Connection.CreateCommand();
          _createShowDiscountCommand.CommandType = CommandType.Text;
          _createShowDiscountCommand.CommandText = string.Format(@"
            INSERT INTO core_config_data (scope, scope_id, path, value) 
            VALUES (?scope, ?scope_id, ?path, ?value)");
          _createShowDiscountCommand.Prepare();
          _createShowDiscountCommand.Parameters.AddWithValue("?scope", "websites");
          _createShowDiscountCommand.Parameters.AddWithValue("?scope_id", websiteID);
          _createShowDiscountCommand.Parameters.AddWithValue("?path", "at_sales/salecategory/showdiscount");
          _createShowDiscountCommand.Parameters.AddWithValue("?value", 0);
        }

        if (_createShowSaleCommand == null)
        {
          _createShowSaleCommand = Connection.CreateCommand();
          _createShowSaleCommand.CommandType = CommandType.Text;
          _createShowSaleCommand.CommandText = string.Format(@"
            INSERT INTO core_config_data (scope, scope_id, path, value) 
            VALUES (?scope, ?scope_id, ?path, ?value)");
          _createShowSaleCommand.Prepare();
          _createShowSaleCommand.Parameters.AddWithValue("?scope", "websites");
          _createShowSaleCommand.Parameters.AddWithValue("?scope_id", websiteID);
          _createShowSaleCommand.Parameters.AddWithValue("?path", "at_sales/salecategory/showsales");
          _createShowSaleCommand.Parameters.AddWithValue("?value", 0);
        }

        if (_updateShowDiscountCommand == null)
        {
          _updateShowDiscountCommand = Connection.CreateCommand();
          _updateShowDiscountCommand.CommandType = CommandType.Text;
          _updateShowDiscountCommand.CommandText = string.Format(@"
            UPDATE core_config_data
            SET value = ?value
            WHERE config_id = ?config_id");
          _updateShowDiscountCommand.Prepare();
          _updateShowDiscountCommand.Parameters.AddWithValue("?value", 0);
          _updateShowDiscountCommand.Parameters.AddWithValue("?config_id", 0);
        }

        if (_updateShowSaleCommand == null)
        {
          _updateShowSaleCommand = Connection.CreateCommand();
          _updateShowSaleCommand.CommandType = CommandType.Text;
          _updateShowSaleCommand.CommandText = string.Format(@"
            UPDATE core_config_data
            SET value = ?value
            WHERE config_id = ?config_id");
          _updateShowSaleCommand.Prepare();
          _updateShowSaleCommand.Parameters.AddWithValue("?value", 0);
          _updateShowSaleCommand.Parameters.AddWithValue("?config_id", 0);
        }

        var showDiscountConfigID = _selectShowDiscountCommand.ExecuteScalar();
        var showSaleConfigID = _selectShowSaleCommand.ExecuteScalar();

        if (showDiscountConfigID == null)
        {
          _createShowDiscountCommand.Parameters["?value"].Value = isSolden ? 1 : 0;
          _createShowDiscountCommand.ExecuteNonQuery();
        }
        else
        {
          _updateShowDiscountCommand.Parameters["?value"].Value = isSolden ? 1 : 0;
          _updateShowDiscountCommand.Parameters["?config_id"].Value = showDiscountConfigID;
          _updateShowDiscountCommand.ExecuteNonQuery();
        }

        if (showSaleConfigID == null)
        {
          _createShowSaleCommand.Parameters["?value"].Value = isSolden ? 1 : 0;
          _createShowSaleCommand.ExecuteNonQuery();
        }
        else
        {
          _updateShowSaleCommand.Parameters["?value"].Value = isSolden ? 1 : 0;
          _updateShowSaleCommand.Parameters["?config_id"].Value = showSaleConfigID;
          _updateShowSaleCommand.ExecuteNonQuery();
        }
      }
      else
      {
        throw new Exception(string.Format("Cannot find website in core_website table for code {0}", coreWebsiteCode));
      }



      //int showDiscount = isSolden ? 1 : 0;

      //      int config_id = -1;

      //      var soldenCommand = Connection.CreateCommand();

      //      soldenCommand.CommandText = @"select * from core_config_data
      //                                   where path = 'at_sales/salecategory/showdiscount';";

      //      using (var reader = soldenCommand.ExecuteReader())
      //      {
      //        if (reader.Read())
      //        {
      //          config_id = int.Parse(reader["config_id"].ToString());
      //        }
      //      }

      //      if (config_id == -1)
      //      {
      //        soldenCommand.CommandText = string.Format(@"insert into core_config_data (scope, scope_id, path, `value`)
      //                                      values('default', 0, 'at_sales/salecategory/showdiscount', '{0}'); ", showDiscount);
      //      }
      //      else
      //      {
      //        soldenCommand.CommandText = string.Format(@"update core_config_data
      //                                                    set `value` = {0} 
      //                                                    where config_id = {1};", showDiscount, config_id);
      //      }

      //      soldenCommand.ExecuteNonQuery();
    }


    public SortedDictionary<string, eav_attribute> GetAttributeList(int entity_type_id)
    {
      SortedDictionary<string, eav_attribute> result = new SortedDictionary<string, eav_attribute>();

      var attributeListCommand = Connection.CreateCommand();
      attributeListCommand.CommandText = @"SELECT * FROM eav_attribute WHERE entity_type_id = ?entity_type_id";
      attributeListCommand.CommandType = System.Data.CommandType.Text;
      attributeListCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);

      using (var reader = attributeListCommand.ExecuteReader())
      {
        var atts = Mapper.Map<IDataReader, IList<eav_attribute>>(reader);

        foreach (var at in atts)
          result.Add(at.attribute_code, at);
      }
      return result;
    }

    public void EnsureAttributeGroup(string attribute_group_name, int entity_type_id)
    {
      using (var cn = Connection.CreateCommand())
      {

        cn.CommandType = CommandType.Text;
        cn.CommandText = String.Format(@"INSERT INTO eav_attribute_group
                                        SELECT NULL, eas.attribute_set_id, ?attribute_group_name, 40, 0
                                        FROM
                                            eav_attribute_set eas 
                                            LEFT JOIN eav_attribute_group eag ON (eag.attribute_set_id = eas.attribute_set_id AND eag.attribute_group_name = ?attribute_group_name)
                                        WHERE eas.entity_type_id = ?entity_type_id
                                        AND eag.attribute_group_id IS NULL");

        cn.Parameters.AddWithValue("?entity_type_id", entity_type_id);
        cn.Parameters.AddWithValue("?attribute_group_name", attribute_group_name);
        cn.ExecuteNonQuery();
      }



    }

    public void EnsureEntityAttribute(eav_attribute attribute, int entity_type_id, string attribute_group_name)
    {

      using (var cn = Connection.CreateCommand())
      {

        cn.CommandType = CommandType.Text;
        cn.CommandText = String.Format(@"
INSERT INTO eav_entity_attribute (entity_type_id, attribute_set_id, attribute_group_id, attribute_id, sort_order)
SELECT eas.entity_type_id, eas.attribute_set_id, eag.attribute_group_id, ?attribute_id, 0
FROM
    {0} eas 
    INNER JOIN {1} eag ON (eag.attribute_set_id = eas.attribute_set_id AND eag.attribute_group_name = ?attribute_group_name)
    LEFT JOIN {2} eea ON (eea.entity_type_id = eas.entity_type_id 
                                            AND eea.attribute_set_id = eas.attribute_set_id
                                            AND eag.attribute_group_id = eag.attribute_group_id
                                            AND eea.attribute_id = ?attribute_id)
WHERE eas.entity_type_id = ?entity_type_id
AND  eea.attribute_id IS NULL", _tablesNames["eas"], _tablesNames["eag"], _tablesNames["eea"]);

        cn.Parameters.AddWithValue("?entity_type_id", entity_type_id);
        cn.Parameters.AddWithValue("?attribute_id", attribute.attribute_id);
        cn.Parameters.AddWithValue("?attribute_group_name", attribute_group_name);
        cn.ExecuteNonQuery();
      }

    }

    public List<eav_entity_attribute> GetEntityAttributeList(int entity_type_id, int attribute_set_id)
    {
      List<eav_entity_attribute> result = new List<eav_entity_attribute>();


      var attributeListCommand = Connection.CreateCommand();
      attributeListCommand.CommandText = @"SELECT * FROM eav_entity_attribute WHERE entity_type_id = ?entity_type_id AND attribute_set_id = ?attribute_set_id";
      attributeListCommand.CommandType = System.Data.CommandType.Text;
      attributeListCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);
      attributeListCommand.Parameters.AddWithValue("?attribute_set_id", attribute_set_id);
      using (var reader = attributeListCommand.ExecuteReader())
      {
        return Mapper.Map<IDataReader, List<eav_entity_attribute>>(reader);
      }

    }

    private MySqlCommand _syncEntityAttributeCommand = null;
    public void SyncEntityAttribute(eav_entity_attribute attribute)
    {
      var eea = TableName("eav_entity_attribute");

      if (_syncEntityAttributeCommand == null)
      {
        _syncEntityAttributeCommand = Connection.CreateCommand();
        _syncEntityAttributeCommand.CommandType = CommandType.Text;
        _syncEntityAttributeCommand.CommandText = String.Format(@"INSERT INTO {0} (entity_attribute_id, entity_type_id, attribute_set_id, attribute_group_id, attribute_id, sort_order) 
                                                                  VALUES  (?entity_attribute_id, ?entity_type_id, ?attribute_set_id, ?attribute_group_id, ?attribute_id, ?sort_order)
                                                                  ON DUPLICATE KEY UPDATE sort_order = VALUES(sort_order), attribute_group_id = VALUES(attribute_group_id), attribute_set_id = VALUES(attribute_set_id)
                                                                  ", eea);
        _syncEntityAttributeCommand.Prepare();

        _syncEntityAttributeCommand.Parameters.AddWithValue("?entity_attribute_id", null);
        _syncEntityAttributeCommand.Parameters.AddWithValue("?entity_type_id", null);
        _syncEntityAttributeCommand.Parameters.AddWithValue("?attribute_set_id", null);
        _syncEntityAttributeCommand.Parameters.AddWithValue("?attribute_group_id", null);
        _syncEntityAttributeCommand.Parameters.AddWithValue("?attribute_id", null);
        _syncEntityAttributeCommand.Parameters.AddWithValue("?sort_order", null);
      }

      _syncEntityAttributeCommand.Parameters["?entity_attribute_id"].Value = attribute.entity_attribute_id;
      _syncEntityAttributeCommand.Parameters["?entity_type_id"].Value = attribute.entity_type_id;
      _syncEntityAttributeCommand.Parameters["?attribute_set_id"].Value = attribute.attribute_set_id;
      _syncEntityAttributeCommand.Parameters["?attribute_group_id"].Value = attribute.attribute_group_id;
      _syncEntityAttributeCommand.Parameters["?attribute_id"].Value = attribute.attribute_id;
      _syncEntityAttributeCommand.Parameters["?sort_order"].Value = attribute.sort_order;
      _syncEntityAttributeCommand.ExecuteNonQuery();

      if (attribute.entity_attribute_id == 0)
        attribute.entity_attribute_id = (int)_syncEntityAttributeCommand.LastInsertedId;
    }

    protected eav_attribute GetAttribute(string attribute_code)
    {

      var attributeListCommand = Connection.CreateCommand();
      attributeListCommand.CommandText = @"SELECT * FROM eav_attribute WHERE attribute_code = ?attribute_code";
      attributeListCommand.CommandType = System.Data.CommandType.Text;
      attributeListCommand.Parameters.AddWithValue("?attribute_code", attribute_code);

      using (var reader = attributeListCommand.ExecuteReader())
      {
        if (reader.Read())
        {
          return Mapper.Map<IDataRecord, eav_attribute>(reader);
        }
      }

      return null;
    }




    public eav_attribute CreateAttribute(string attribute_code, int entity_type_id, string frontend_label, string datatype = "varchar", string note = "",
      bool is_visible = true, bool addToDefaultSet = false, bool is_required = false, bool is_user_defined = true, bool used_in_product_listing = false, bool is_searchable = false)
    {
      string backend_type = "varchar";
      string backend_model = null;
      string frontend_input = "text";

      if (datatype == "date")
      {
        backend_type = "datetime";
        backend_model = "eav/entity_attribute_backend_datetime";
      }

      if (datatype == "int")
      {
        backend_type = "int";
      }

      if (datatype == "decimal")
      {
        backend_type = "decimal";
      }

      if (datatype == "weee")
      {
        backend_model = "weee/attribute_backend_weee_tax";
        backend_type = "static";
        frontend_input = "weee";
      }

      eav_attribute my_attribute = new eav_attribute()
      {
        attribute_code = attribute_code,
        entity_type_id = entity_type_id,
        backend_model = backend_model,
        backend_type = backend_type,
        is_required = is_required,
        is_user_defined = is_user_defined,
        frontend_input = frontend_input,
        is_unique = false,
        note = String.Empty,
        is_visible = is_visible,
        frontend_label = frontend_label,
        used_in_product_listing = used_in_product_listing,
        is_searchable = is_searchable
      };

      return my_attribute;
    }

    private MySqlCommand _syncAttributeLabelCommand = null;
    public void SyncAttributeLabel(eav_attribute_label label)
    {
      var ea = TableName("eav_attribute_label");

      if (_syncAttributeLabelCommand == null)
      {
        _syncAttributeLabelCommand = Connection.CreateCommand();
        _syncAttributeLabelCommand.CommandType = CommandType.Text;
        _syncAttributeLabelCommand.CommandText = String.Format(@"INSERT INTO {0} (attribute_label_id, attribute_id, store_id, value) 
                                                  VALUES (?attribute_label_id, ?attribute_id, ?store_id, ?value)
                                                  ON DUPLICATE KEY UPDATE value = VALUES(value)", ea);
        _syncAttributeLabelCommand.Prepare();

        _syncAttributeLabelCommand.Parameters.AddWithValue("?attribute_label_id", null);
        _syncAttributeLabelCommand.Parameters.AddWithValue("?attribute_id", null);
        _syncAttributeLabelCommand.Parameters.AddWithValue("?store_id", null);
        _syncAttributeLabelCommand.Parameters.AddWithValue("?value", null);
      }


      _syncAttributeLabelCommand.Parameters["?attribute_label_id"].Value = label.attribute_label_id;
      _syncAttributeLabelCommand.Parameters["?attribute_id"].Value = label.attribute_id;
      _syncAttributeLabelCommand.Parameters["?store_id"].Value = label.store_id;
      _syncAttributeLabelCommand.Parameters["?value"].Value = label.value;
      _syncAttributeLabelCommand.ExecuteNonQuery();

    }

    public void CopyDefaultSet(int attribute_set_id)
    {
      string copyDefaultQuery = string.Format(@"
                      INSERT INTO eav_attribute_group (attribute_set_id, attribute_group_name, sort_order, default_id)
                      SELECT {0}, attribute_group_name, sort_order, default_id
                      FROM eav_attribute_group
                      WHERE attribute_set_id = (SELECT attribute_set_id FROM eav_attribute_set WHERE entity_type_id = 4 AND attribute_set_name = 'Default');

                      INSERT INTO eav_entity_attribute (entity_type_id, attribute_set_id, attribute_group_id, attribute_id, sort_order)
                      SELECT eea.entity_type_id, {0}, ng.attribute_group_id, eea.attribute_id, eea.sort_order
                      FROM eav_entity_attribute eea
                      INNER JOIN eav_attribute_group eag ON (eea.attribute_group_id = eag.attribute_group_id
                                AND eag.attribute_set_id = (SELECT attribute_set_id FROM eav_attribute_set WHERE entity_type_id = 4 AND attribute_set_name = 'Default')
                                )
                
                      INNER JOIN eav_attribute_group ng ON (ng.attribute_set_id = {0} AND ng.attribute_group_name = eag.attribute_group_name)
    
                      WHERE eea.entity_type_id = 4
                      AND eea.attribute_set_id = (SELECT attribute_set_id FROM eav_attribute_set WHERE entity_type_id = 4 AND attribute_set_name = 'Default')
                      ", attribute_set_id);

      // copy default attributes to new set
      Execute(copyDefaultQuery);
    }


    private MySqlCommand _sycAttributeSetCommand = null;
    public void SyncAttributeSet(eav_attribute_set set)
    {
      var eas = TableName("eav_attribute_set");

      if (_sycAttributeSetCommand == null)
      {
        _sycAttributeSetCommand = Connection.CreateCommand();
        _sycAttributeSetCommand.CommandType = CommandType.Text;
        _sycAttributeSetCommand.CommandText = String.Format(@"INSERT INTO {0} (attribute_set_id, entity_type_id, attribute_set_name, sort_order) VALUES  (?attribute_set_id, ?entity_type_id, ?attribute_set_name, ?sort_order)", eas);
        _sycAttributeSetCommand.Prepare();

        _sycAttributeSetCommand.Parameters.AddWithValue("?attribute_set_id", null);
        _sycAttributeSetCommand.Parameters.AddWithValue("?entity_type_id", null);
        _sycAttributeSetCommand.Parameters.AddWithValue("?attribute_set_name", null);
        _sycAttributeSetCommand.Parameters.AddWithValue("?sort_order", null);
      }

      _sycAttributeSetCommand.Parameters["?attribute_set_id"].Value = set.attribute_set_id;
      _sycAttributeSetCommand.Parameters["?entity_type_id"].Value = set.entity_type_id;
      _sycAttributeSetCommand.Parameters["?attribute_set_name"].Value = set.attribute_set_name;
      _sycAttributeSetCommand.Parameters["?sort_order"].Value = set.sort_order;
      _sycAttributeSetCommand.ExecuteNonQuery();

      set.attribute_set_id = (int)_sycAttributeSetCommand.LastInsertedId;

    }

    private MySqlCommand _selectAttributeSetsCommand = null;
    public Dictionary<string, eav_attribute_set> GetAttributeSetList(int entity_type_id)
    {
      Dictionary<string, eav_attribute_set> result = new Dictionary<string, eav_attribute_set>();
      var eas = TableName("eav_attribute_set");

      if (_selectAttributeSetsCommand == null)
      {
        _selectAttributeSetsCommand = Connection.CreateCommand();
        _selectAttributeSetsCommand.CommandType = CommandType.Text;
        _selectAttributeSetsCommand.CommandText = String.Format(@"SELECT * FROM {0} WHERE entity_type_id = ?entity_type_id", eas);

        _selectAttributeSetsCommand.Prepare();
        _selectAttributeSetsCommand.Parameters.AddWithValue("?entity_type_id", 0);

      }
      _selectAttributeSetsCommand.Parameters["?entity_type_id"].Value = entity_type_id;

      using (var reader = _selectAttributeSetsCommand.ExecuteReader())
      {
        var sets = Mapper.Map<IDataReader, List<eav_attribute_set>>(reader);

        foreach (var set in sets)
          result.Add(set.attribute_set_name, set);
      }

      return result;

    }

    private MySqlCommand syncCatalogAttributeCommand = null;
    private MySqlCommand syncCustomerAttributeCommand = null;
    private MySqlCommand syncShopWeekAttributeCommand = null; //TODO: CLEAN THIS UP!
    public int SyncAttribute(eav_attribute attribute, bool addToDefaultSet = false, bool configurableAttributeType = false)
    {

      var ea = TableName("eav_attribute");

      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;

      if (attribute.attribute_id > 0)
      {
        cmd.CommandText = String.Format(@"UPDATE {0} SET entity_type_id = ?entity_type_id, attribute_code = ?attribute_code, attribute_model =?attribute_model,
backend_model=?backend_model, backend_type = ?backend_type, backend_table = ?backend_table, 
frontend_model = ?frontend_model,  frontend_input=?frontend_input, frontend_label=  ?frontend_label, frontend_class =?frontend_class, 
is_required = ?is_required, is_user_defined = ?is_user_defined, default_value = ?default_value, is_unique =?is_unique, note =?note
WHERE attribute_id =?attribute_id", ea);

      }
      else
      {
        cmd.CommandText = String.Format(@"INSERT INTO {0} (attribute_id, entity_type_id, attribute_code, attribute_model,  backend_model, 
                                                          backend_type, backend_table, frontend_model, frontend_input, frontend_label, frontend_class, 
                                                          is_required, is_user_defined, default_value, is_unique, note)
                                        VALUES (?attribute_id, ?entity_type_id, ?attribute_code, ?attribute_model, ?backend_model, 
                                                ?backend_type, ?backend_table, ?frontend_model, ?frontend_input, ?frontend_label, ?frontend_class,
                                                ?is_required, ?is_user_defined, ?default_value, ?is_unique, ?note)"
                                        , ea);

      }


      cmd.Parameters.AddWithValue("?attribute_id", attribute.attribute_id);
      cmd.Parameters.AddWithValue("?entity_type_id", attribute.entity_type_id);
      cmd.Parameters.AddWithValue("?attribute_code", attribute.attribute_code);
      cmd.Parameters.AddWithValue("?attribute_model", attribute.attribute_model);
      cmd.Parameters.AddWithValue("?backend_model", attribute.backend_model);
      cmd.Parameters.AddWithValue("?backend_type", attribute.backend_type);
      cmd.Parameters.AddWithValue("?backend_table", attribute.backend_table);

      cmd.Parameters.AddWithValue("?frontend_model", attribute.frontend_model);
      cmd.Parameters.AddWithValue("?frontend_input", attribute.frontend_input);
      cmd.Parameters.AddWithValue("?frontend_label", attribute.frontend_label);
      cmd.Parameters.AddWithValue("?frontend_class", attribute.frontend_class);
      cmd.Parameters.AddWithValue("?is_required", attribute.is_required);
      cmd.Parameters.AddWithValue("?is_user_defined", attribute.is_user_defined);
      cmd.Parameters.AddWithValue("?default_value", attribute.default_value);
      cmd.Parameters.AddWithValue("?is_unique", attribute.is_unique);
      cmd.Parameters.AddWithValue("?note", attribute.note);


      cmd.ExecuteNonQuery();

      if (attribute.attribute_id == 0)
        attribute.attribute_id = (int)cmd.LastInsertedId;


      #region Catalog Attributes

      if (attribute.entity_type_id == PRODUCT_ENTITY_TYPE_ID || attribute.entity_type_id == CATEGORY_ENTITY_TYPE_ID)
      {
        if (syncCatalogAttributeCommand == null)
        {

          syncCatalogAttributeCommand = Connection.CreateCommand();
          syncCatalogAttributeCommand.CommandType = System.Data.CommandType.Text;
          syncCatalogAttributeCommand.CommandText = @"REPLACE INTO catalog_eav_attribute (attribute_id, frontend_input_renderer, is_visible, is_visible_on_front,
                                            used_for_sort_by, is_configurable, is_global, apply_to, is_visible_in_advanced_search, is_filterable, is_comparable,is_filterable_in_search, is_searchable, used_in_product_listing, is_used_for_promo_rules, position)
                                            VALUES (?attribute_id, ?frontend_input_renderer, ?is_visible, ?is_visible_on_front,
                                            ?used_for_sort_by, ?is_configurable, ?is_global, ?apply_to, ?is_visible_in_advanced_search, ?is_filterable, ?is_comparable,?is_filterable_in_search, ?is_searchable, ?used_in_product_listing, ?is_used_for_promo_rules, ?position)";

          syncCatalogAttributeCommand.Prepare();

          syncCatalogAttributeCommand.Parameters.AddWithValue("?attribute_id", attribute.attribute_id);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?frontend_input_renderer", null);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_visible", true);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_visible_on_front", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?used_for_sort_by", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_configurable", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_global", true);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?apply_to", "simple");
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_visible_in_advanced_search", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_filterable", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_comparable", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_filterable_in_search", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_searchable", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?used_in_product_listing", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?is_used_for_promo_rules", false);
          syncCatalogAttributeCommand.Parameters.AddWithValue("?position", 0);
        }

        var attributeID = TableName("eav_attribute_set");

        syncCatalogAttributeCommand.Parameters["?attribute_id"].Value = attribute.attribute_id;
        syncCatalogAttributeCommand.Parameters["?frontend_input_renderer"].Value = null;
        syncCatalogAttributeCommand.Parameters["?is_visible"].Value = true;
        syncCatalogAttributeCommand.Parameters["?is_visible_on_front"].Value = attribute.is_visible;
        syncCatalogAttributeCommand.Parameters["?used_for_sort_by"].Value = configurableAttributeType ? true : false;
        syncCatalogAttributeCommand.Parameters["?is_configurable"].Value = attribute.is_configurable;
        syncCatalogAttributeCommand.Parameters["?is_global"].Value = true;
        syncCatalogAttributeCommand.Parameters["?apply_to"].Value = configurableAttributeType ? "configurable" : "simple";
        syncCatalogAttributeCommand.Parameters["?is_visible_in_advanced_search"].Value = attribute.is_configurable;
        syncCatalogAttributeCommand.Parameters["?is_filterable_in_search"].Value = attribute.is_comparable;
        syncCatalogAttributeCommand.Parameters["?is_filterable"].Value = attribute.is_filterable;
        syncCatalogAttributeCommand.Parameters["?is_comparable"].Value = attribute.is_comparable;
        syncCatalogAttributeCommand.Parameters["?is_searchable"].Value = attribute.is_searchable;
        syncCatalogAttributeCommand.Parameters["?used_in_product_listing"].Value = configurableAttributeType ? true : false;
        syncCatalogAttributeCommand.Parameters["?is_used_for_promo_rules"].Value = attribute.is_used_for_promo_rules;
        syncCatalogAttributeCommand.Parameters["?position"].Value = attribute.position;

        syncCatalogAttributeCommand.ExecuteNonQuery();

        if (configurableAttributeType)
        {
          syncShopWeekAttributeCommand = Connection.CreateCommand();
          syncShopWeekAttributeCommand.CommandType = System.Data.CommandType.Text;
          syncShopWeekAttributeCommand.CommandText = @"UPDATE catalog_eav_attribute set used_for_sort_by = ?used_for_sort_by where attribute_id = ?attribute_id";

          syncShopWeekAttributeCommand.Prepare();

          syncShopWeekAttributeCommand.Parameters.AddWithValue("?used_for_sort_by", true);
          syncShopWeekAttributeCommand.Parameters.AddWithValue("?attribute_id", attribute.attribute_id);

          syncShopWeekAttributeCommand.ExecuteNonQuery();
        }

        if (addToDefaultSet)
        {

          var default_group = GetDefaultAttributeGroup();
          if (default_group != null)
          {
            eav_entity_attribute eea = new eav_entity_attribute()
            {
              attribute_id = attribute.attribute_id,
              attribute_set_id = default_group.attribute_set_id,
              attribute_group_id = default_group.attribute_group_id,
              entity_type_id = PRODUCT_ENTITY_TYPE_ID,
              sort_order = 0
            };

            this.SyncEntityAttribute(eea);



          }
        }
      }
      #endregion

      #region Customer Attributes

      if (attribute.entity_type_id == CUSTOMER_ENTITY_TYPE_ID || attribute.entity_type_id == CUSTOMER_ADDRESS_ENTITY_TYPE_ID)
      {

        if (syncCustomerAttributeCommand == null)
        {

          syncCustomerAttributeCommand = Connection.CreateCommand();
          syncCustomerAttributeCommand.CommandType = System.Data.CommandType.Text;
          syncCustomerAttributeCommand.CommandText = String.Format(@"REPLACE INTO {0} (attribute_id, is_visible, input_filter, multiline_count, validate_rules, is_system, sort_order, data_model)
                                                       VALUES (?attribute_id, ?is_visible, ?input_filter, ?multiline_count, ?validate_rules, ?is_system, ?sort_order, ?data_model)",
                                                        _tablesNames["customer_eav_attribute"]);

          syncCustomerAttributeCommand.Prepare();

          syncCustomerAttributeCommand.Parameters.AddWithValue("?attribute_id", attribute.attribute_id);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?is_visible", true);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?input_filter", string.Empty);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?multiline_count", 0);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?validate_rules", null);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?is_system", true);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?sort_order", 1);
          syncCustomerAttributeCommand.Parameters.AddWithValue("?data_model", null);
        }
        syncCustomerAttributeCommand.Parameters["?attribute_id"].Value = attribute.attribute_id;
        syncCustomerAttributeCommand.Parameters["?is_visible"].Value = true;
        syncCustomerAttributeCommand.Parameters["?input_filter"].Value = string.Empty;
        syncCustomerAttributeCommand.Parameters["?multiline_count"].Value = 1;
        syncCustomerAttributeCommand.Parameters["?validate_rules"].Value = null;
        syncCustomerAttributeCommand.Parameters["?is_system"].Value = true;
        syncCustomerAttributeCommand.Parameters["?sort_order"].Value = 0;
        syncCustomerAttributeCommand.Parameters["?data_model"].Value = null;

        syncCustomerAttributeCommand.ExecuteNonQuery();


        if (addToDefaultSet)
        {

          var default_group = GetDefaultAttributeGroup(attribute.entity_type_id);
          if (default_group != null)
          {
            eav_entity_attribute eea = new eav_entity_attribute()
            {
              attribute_id = attribute.attribute_id,
              attribute_set_id = default_group.attribute_set_id,
              attribute_group_id = default_group.attribute_group_id,
              entity_type_id = attribute.entity_type_id,
              sort_order = 0
            };

            this.SyncEntityAttribute(eea);

          }


          string form_code = "adminhtml_customer";
          if (attribute.entity_type_id == CUSTOMER_ADDRESS_ENTITY_TYPE_ID)
            form_code = "adminhtml_customer_address";
          // Default form

          this.Execute(String.Format("REPLACE INTO customer_form_attribute (form_code, attribute_id) VALUES ( '{0}',{1} )", form_code, attribute.attribute_id));

        }
      }
      #endregion

      return attribute.attribute_id;

    }




    #endregion

    #region Categories

    //    internal int GetExistingCategory(int category_id)
    //    {
    //      var t = TableName("catalog_category_entity");
    //      var csg = TableName("core_store_group");
    //      var cs = TableName("core_store");
    //      var ccev = t + "_varchar";
    //      var ea = TableName("eav_attribute");

    //      var getCategoriesCommand = _connection.CreateCommand();

    //      getCategoriesCommand.CommandText = String.Format(@"SELECT cs.store_id,csg.website_id,cce.entity_type_id,cce.path,ccev.value as name
    //                  FROM {0} as cs 
    //                  JOIN {1} as csg on csg.group_id=cs.group_id
    //                  JOIN {2} as cce ON cce.entity_id=csg.root_category_id
    //                  JOIN {3} as ea ON ea.attribute_code='icecat_cat_id' AND ea.entity_type_id=cce.entity_type_id
    //                  JOIN {4} as ccev ON ccev.attribute_id=ea.attribute_id AND ccev.entity_id=cce.entity_id
    //                  ", cs, csg, t, ea, ccev);
    //      getCategoriesCommand.CommandType = System.Data.CommandType.Text;
    //      getCategoriesCommand.Parameters.AddWithValue("?entity_type_id", PRODUCT_ENTITY_TYPE_ID);

    //      List<root_category_info> result = new List<root_category_info>();

    //      using (var reader = getCategoriesCommand.ExecuteReader())
    //      {
    //        while (reader.Read())
    //        {
    //          result.Add(Mapper.Map<IDataReader, root_category_info>(reader));
    //        }

    //      }

    //      return result;
    //    }


    public List<catalog_category_entity> GetRootCategories()
    {
      var t = TableName("catalog_category_entity");
      var csg = TableName("core_store_group");
      var cs = TableName("core_store");
      var ccev = t + "_varchar";
      var ea = TableName("eav_attribute");

      var getCategoriesCommand = Connection.CreateCommand();

      getCategoriesCommand.CommandText = String.Format(@"SELECT cs.store_id,csg.website_id,cce.entity_type_id,cce.path,ccev.value as name, cce.entity_id, cce.level, cce.created_at, cce.updated_at
                  FROM {0} as cs 
                  JOIN {1} as csg on csg.group_id=cs.group_id
                  JOIN {2} as cce ON cce.entity_id=csg.root_category_id
                  JOIN {3} as ea ON ea.attribute_code='name' AND ea.entity_type_id=cce.entity_type_id
                  JOIN {4} as ccev ON ccev.attribute_id=ea.attribute_id AND ccev.entity_id=cce.entity_id
                  ", cs, csg, t, ea, ccev);
      getCategoriesCommand.CommandType = System.Data.CommandType.Text;
      getCategoriesCommand.Parameters.AddWithValue("?entity_type_id", PRODUCT_ENTITY_TYPE_ID);

      List<catalog_category_entity> result = new List<catalog_category_entity>();

      using (var reader = getCategoriesCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          result.Add(Mapper.Map<IDataReader, catalog_category_entity>(reader));
        }

      }

      return result;

    }

    /// <summary>
    /// Retrieves the categories for a website based on the icecat_cat_id
    /// </summary>
    /// <param name="website_id"></param>
    /// <param name="store_id"></param>
    /// <returns></returns>
    public List<catalog_category_entity> GetWebsiteCategories(int website_id, int store_id = 0)
    {
      var categories = GetCategories(store_id);

      return categories.Where(c => c.icecat_value.StartsWith(string.Format("{0}_", website_id))).ToList();
    }

    public List<catalog_category_entity> GetCategories(int store_id = 0)
    {
      var cs = TableName("catalog_category_entity");
      var ccev = cs + "_varchar";
      var ccei = cs + "_int";
      var ea = TableName("eav_attribute");


      var cn = Connection.CreateCommand();
      cn.CommandType = CommandType.Text;
      cn.CommandText = String.Format(@"SELECT cce.entity_id, cce.entity_type_id, cce.attribute_set_id,
																	              cce.parent_id, 
																								cce.path, cce.position, cce.level, cce.children_count, ccev.attribute_id as name_attribute_id, ccev.value as name, ice.attribute_id as icecat_attribute_id, 
                                                 icev.value as icecat_value, ccev.store_id, -1 as website_id
                                                FROM {0} cce
                                                INNER JOIN {1} as ea ON ea.attribute_code='name' AND ea.entity_type_id=cce.entity_type_id
                                                INNER JOIN {2} AS ccev ON ccev.attribute_id=ea.attribute_id AND ccev.entity_id=cce.entity_id
                                                INNER JOIN {1} as ice ON ice.attribute_code='icecat_cat_id' AND ice.entity_type_id=cce.entity_type_id
                                                INNER JOIN {3} AS icev ON ice.attribute_id=icev.attribute_id AND icev.entity_id=cce.entity_id
                                                WHERE ccev.store_id = ?store_id
                                                ", cs, ea, ccev, ccev);
      cn.Parameters.AddWithValue("?store_id", store_id);


      using (var reader = cn.ExecuteReader())
      {
        return Mapper.Map<IDataReader, List<catalog_category_entity>>(reader);

      }

    }
    #endregion

    #region Helper Functions

    public string TableName(string name)
    {
      if (_tablePrefix != String.Empty)
        return _tablePrefix + name;

      return name;
    }

    public int Execute(string query)
    {
      var cmd = Connection.CreateCommand();
      cmd.CommandType = System.Data.CommandType.Text;
      cmd.CommandText = query;
      return cmd.ExecuteNonQuery();
    }

    public DataRow Select(string sql, params object[] parameters)
    {
      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;
      cmd.CommandText = sql;

      var result = new DataTable();

      using (var reader = cmd.ExecuteReader())
      {
        var schemaTable = reader.GetSchemaTable();

        for (int i = 0; i <= schemaTable.Rows.Count - 1; i++)
        {
          DataRow dataRow = schemaTable.Rows[i];
          string columnName = dataRow["ColumnName"].ToString();
          DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
          result.Columns.Add(column);

        }

        if (reader.Read())
        {
          DataRow dataRow = result.NewRow();
          for (int i = 0; i <= reader.FieldCount - 1; i++)
          {

            dataRow[i] = reader.GetValue(i);

          }
          return dataRow;
        }
      }

      return null;
    }

    public DataRow[] SelectAll(string sql, params object[] parameters)
    {
      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;
      cmd.CommandText = sql;

      var result = new DataTable();

      using (var reader = cmd.ExecuteReader())
      {
        var schemaTable = reader.GetSchemaTable();

        for (int i = 0; i <= schemaTable.Rows.Count - 1; i++)
        {
          DataRow dataRow = schemaTable.Rows[i];
          string columnName = dataRow["ColumnName"].ToString();
          DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
          result.Columns.Add(column);

        }

        while (reader.Read())
        {
          DataRow dataRow = result.NewRow();
          for (int i = 0; i <= reader.FieldCount - 1; i++)
          {

            dataRow[i] = reader.GetValue(i);

          }
          result.Rows.Add(dataRow);
        }
      }

      DataRow[] bla = new DataRow[result.Rows.Count];
      result.Rows.CopyTo(bla, 0);
      return bla;
    }

    #endregion

    public void Dispose()
    {
      if (Connection != null)
        Connection.Close();
    }



    public void DeleteAttributeValue(int attribute_id, int entity_type_id, int store_id, int entity_id, string type = "varchar")
    {
      SyncAttributeValue(attribute_id, entity_type_id, store_id, entity_id, null, type, delete: true);

    }

    public void SyncAttributeValue(int attribute_id, int entity_type_id, int store_id, int entity_id, object value,
      string type = "varchar", bool delete = false, string concentrator_attribute_value_id = null, int sort_order = 0)
    {

      if (type == "option")
      {
        value = GetOptionId(attribute_id, store_id: store_id, concentrator_attribute_value_id: concentrator_attribute_value_id, value: value, sort_order: sort_order);
        type = "int";
      }

      string tableName = String.Empty;
      switch (entity_type_id)
      {
        case PRODUCT_ENTITY_TYPE_ID:
          tableName = TableName("catalog_product_entity_");
          break;
        case CUSTOMER_ENTITY_TYPE_ID:
          tableName = TableName("customer_entity_");
          break;
        case CUSTOMER_ADDRESS_ENTITY_TYPE_ID:
          tableName = TableName("customer_address_entity_");
          break;
        case CATEGORY_ENTITY_TYPE_ID:
          tableName = TableName("catalog_category_entity_");
          break;
      }

      tableName += type;

      var cmd = Connection.CreateCommand();
      cmd.CommandType = CommandType.Text;

      if (delete)
      {
        #region Delete Value

        if (entity_type_id == PRODUCT_ENTITY_TYPE_ID || entity_type_id == CATEGORY_ENTITY_TYPE_ID)
        {
          cmd.CommandText = String.Format(@"DELETE FROM {0} 
                                      WHERE entity_type_id = ?entity_type_id AND attribute_id = ?attribute_id  AND store_id = ?store_id
                                      AND entity_id = ?entity_id", tableName);

          cmd.Parameters.AddWithValue("?store_id", store_id);
        }
        else
        {
          cmd.CommandText = String.Format(@"DELETE FROM {0} 
                                      WHERE entity_type_id = ?entity_type_id AND attribute_id = ?attribute_id 
                                      AND entity_id = ?entity_id", tableName);
        }


        cmd.Parameters.AddWithValue("?entity_type_id", entity_type_id);
        cmd.Parameters.AddWithValue("?attribute_id", attribute_id);
        cmd.Parameters.AddWithValue("?entity_id", entity_id);

        cmd.ExecuteNonQuery();
        #endregion

      }
      else
      {

        #region Insert/Update

        if (entity_type_id == PRODUCT_ENTITY_TYPE_ID || entity_type_id == CATEGORY_ENTITY_TYPE_ID)
        {

          cmd.CommandText = String.Format(@"INSERT INTO {0} (entity_type_id, attribute_id, store_id, entity_id, value)
                                        VALUES (?entity_type_id, ?attribute_id, ?store_id, ?entity_id, ?value)
                                        ON DUPLICATE KEY UPDATE value = VALUES(value)", tableName);

          cmd.Parameters.AddWithValue("?store_id", store_id);

        }
        else
        {

          cmd.CommandText = String.Format(@"INSERT INTO {0} (entity_type_id, attribute_id, entity_id, value)
                                        VALUES (?entity_type_id, ?attribute_id, ?entity_id, ?value)
                                        ON DUPLICATE KEY UPDATE value = VALUES(value)", tableName);

        }

        cmd.Parameters.AddWithValue("?entity_type_id", entity_type_id);
        cmd.Parameters.AddWithValue("?attribute_id", attribute_id);

        cmd.Parameters.AddWithValue("?entity_id", entity_id);
        cmd.Parameters.AddWithValue("?value", value);
        cmd.ExecuteNonQuery();

        #endregion

      }
    }


    private MySqlCommand createNewOptionValueCommand = null;
    private MySqlCommand createNewOptionCommand = null;
    private MySqlCommand selectOptionIdCommand = null;
    private MySqlCommand selectOptionIdWithCidCommand = null;
    private MySqlCommand updateOptionCommand = null;
    private MySqlCommand updateOptionValueCommand = null;
    private MySqlCommand selectOptionValueCommand = null;

    public int GetOptionId(int attribute_id, int store_id, string concentrator_attribute_value_id, object value, bool grouped = false, int sort_order = 0)
    {
      string optionValue = value.ToString().Trim();

      #region Command Definition
      if (selectOptionIdCommand == null)
      {
        selectOptionIdCommand = Connection.CreateCommand();
        selectOptionIdCommand.CommandType = CommandType.Text;
        selectOptionIdCommand.CommandText = String.Format(@"SELECT eaov.option_id FROM {0} eao
                                  INNER JOIN {1} eaov ON (eao.option_id = eaov.option_id)
                              WHERE eao.attribute_id = ?attribute_id
                              AND eaov.store_id = 0
                              AND eaov.value = ?value", _tablesNames["eao"], _tablesNames["eaov"]);

        selectOptionIdCommand.Prepare();
        selectOptionIdCommand.Parameters.AddWithValue("?attribute_id", 0);
        selectOptionIdCommand.Parameters.AddWithValue("?value", null);


      }

      if (selectOptionIdWithCidCommand == null)
      {
        selectOptionIdWithCidCommand = Connection.CreateCommand();
        selectOptionIdWithCidCommand.CommandType = CommandType.Text;
        selectOptionIdWithCidCommand.CommandText = String.Format(@"SELECT eao.option_id FROM {0} eao
                              WHERE eao.attribute_id = ?attribute_id 
                              AND eao.concentrator_attribute_id = ?concentrator_attribute_value_id
                              ", _tablesNames["eao"], _tablesNames["eaov"]);

        selectOptionIdWithCidCommand.Prepare();
        selectOptionIdWithCidCommand.Parameters.AddWithValue("?attribute_id", 0);
        selectOptionIdWithCidCommand.Parameters.AddWithValue("?concentrator_attribute_value_id", string.Empty);

      }




      if (createNewOptionCommand == null)
      {
        createNewOptionCommand = Connection.CreateCommand();
        createNewOptionCommand.CommandType = CommandType.Text;
        createNewOptionCommand.CommandText = string.Format(@"INSERT INTO {0} (attribute_id, concentrator_attribute_id, sort_order) 
            VALUES (?attribute_id, ?concentrator_attribute_id, ?sort_order)", _tablesNames["eao"]);
        createNewOptionCommand.Prepare();
        createNewOptionCommand.Parameters.AddWithValue("?attribute_id", 0);
        createNewOptionCommand.Parameters.AddWithValue("?sort_order", 0);
        createNewOptionCommand.Parameters.AddWithValue("?concentrator_attribute_id", string.Empty);

      }

      if (updateOptionCommand == null)
      {
        updateOptionCommand = Connection.CreateCommand();
        updateOptionCommand.CommandType = CommandType.Text;
        updateOptionCommand.CommandText = string.Format(@"UPDATE {0} SET sort_order = ?sort_order WHERE option_id = ?option_id", _tablesNames["eao"]);
        updateOptionCommand.Prepare();
        updateOptionCommand.Parameters.AddWithValue("?option_id", 0);
        updateOptionCommand.Parameters.AddWithValue("?sort_order", 0);

      }
      if (createNewOptionValueCommand == null)
      {
        createNewOptionValueCommand = Connection.CreateCommand();
        createNewOptionValueCommand.CommandType = CommandType.Text;
        createNewOptionValueCommand.CommandText = string.Format(@"INSERT INTO {0} (option_id, store_id, value) VALUES (?option_id, ?store_id, ?value)", _tablesNames["eaov"]);
        createNewOptionValueCommand.Prepare();
        createNewOptionValueCommand.Parameters.AddWithValue("?option_id", 0);
        createNewOptionValueCommand.Parameters.AddWithValue("?store_id", 0);
        createNewOptionValueCommand.Parameters.AddWithValue("?value", String.Empty);
      }

      if (updateOptionValueCommand == null)
      {
        updateOptionValueCommand = Connection.CreateCommand();
        updateOptionValueCommand.CommandType = CommandType.Text;
        updateOptionValueCommand.CommandText = string.Format(@"UPDATE {0} SET value = ?value WHERE store_id =?store_id AND option_id = ?option_id", _tablesNames["eaov"]);
        updateOptionValueCommand.Prepare();
        updateOptionValueCommand.Parameters.AddWithValue("?option_id", 0);
        updateOptionValueCommand.Parameters.AddWithValue("?store_id", 0);
        updateOptionValueCommand.Parameters.AddWithValue("?value", String.Empty);
      }

      if (selectOptionValueCommand == null)
      {
        selectOptionValueCommand = Connection.CreateCommand();
        selectOptionValueCommand.CommandType = CommandType.Text;
        selectOptionValueCommand.CommandText = string.Format(@"SELECT value_id FROM {0} WHERE store_id =?store_id AND option_id = ?option_id", _tablesNames["eaov"]);
        selectOptionValueCommand.Prepare();
        selectOptionValueCommand.Parameters.AddWithValue("?option_id", 0);
        selectOptionValueCommand.Parameters.AddWithValue("?store_id", 0);
      }
      #endregion

      object result = null;
      if (!String.IsNullOrEmpty(concentrator_attribute_value_id))
      {
        selectOptionIdWithCidCommand.Parameters["?attribute_id"].Value = attribute_id;
        selectOptionIdWithCidCommand.Parameters["?concentrator_attribute_value_id"].Value = concentrator_attribute_value_id;
        result = selectOptionIdWithCidCommand.ExecuteScalar();
      }
      else
      {
        selectOptionIdCommand.Parameters["?attribute_id"].Value = attribute_id;
        selectOptionIdCommand.Parameters["?value"].Value = optionValue;
        result = selectOptionIdCommand.ExecuteScalar();
      }
      if (result != null)
      {
        int option_id = Convert.ToInt32(result);
        updateOptionCommand.Parameters["?option_id"].Value = option_id;
        updateOptionCommand.Parameters["?sort_order"].Value = sort_order;
        updateOptionCommand.ExecuteNonQuery();

        // update value
        selectOptionValueCommand.Parameters["?option_id"].Value = option_id;
        selectOptionValueCommand.Parameters["?store_id"].Value = store_id;
        object value_result = selectOptionValueCommand.ExecuteScalar();
        if (value_result == null)
        {
          //add value
          createNewOptionValueCommand.Parameters["?option_id"].Value = option_id;
          createNewOptionValueCommand.Parameters["?store_id"].Value = store_id;
          createNewOptionValueCommand.Parameters["?value"].Value = optionValue;

          createNewOptionValueCommand.ExecuteNonQuery();
          int valueId = (int)createNewOptionValueCommand.LastInsertedId;
        }
        else
        {
          //update
          updateOptionValueCommand.Parameters["?option_id"].Value = option_id;
          updateOptionValueCommand.Parameters["?store_id"].Value = store_id;
          updateOptionValueCommand.Parameters["?value"].Value = optionValue;

          updateOptionValueCommand.ExecuteNonQuery();
        }

        return option_id;
        //return option_id;
      }
      else
      {
        //create new

        createNewOptionCommand.Parameters["?attribute_id"].Value = attribute_id;
        createNewOptionCommand.Parameters["?sort_order"].Value = sort_order;
        createNewOptionCommand.Parameters["?concentrator_attribute_id"].Value = concentrator_attribute_value_id;

        createNewOptionCommand.ExecuteNonQuery();

        int optionId = (int)createNewOptionCommand.LastInsertedId;

        createNewOptionValueCommand.Parameters["?option_id"].Value = optionId;
        createNewOptionValueCommand.Parameters["?store_id"].Value = store_id;
        createNewOptionValueCommand.Parameters["?value"].Value = optionValue;

        createNewOptionValueCommand.ExecuteNonQuery();
        int valueId = (int)createNewOptionValueCommand.LastInsertedId;

        return optionId;
      }

    }

    #region Attributes

    public eav_attribute_set CreateAttributeSet(int entity_type_id, string name)
    {
      eav_attribute_set set = new eav_attribute_set() { entity_type_id = entity_type_id, attribute_set_name = name, sort_order = 0 };
      return set;
    }

    private MySqlCommand _selectAttributeGroupCommand = null;
    public eav_attribute_group GetAttributeGroup(eav_attribute_set set, string attribute_group_name)
    {
      if (_selectAttributeGroupCommand == null)
      {
        _selectAttributeGroupCommand = Connection.CreateCommand();
        _selectAttributeGroupCommand.CommandType = CommandType.Text;
        _selectAttributeGroupCommand.CommandText = @"SELECT * FROM eav_attribute_group WHERE attribute_set_id = ?attribute_set_id AND attribute_group_name = ?attribute_group_name;";

        _selectAttributeGroupCommand.Prepare();
        _selectAttributeGroupCommand.Parameters.AddWithValue("?attribute_set_id", 0);
        _selectAttributeGroupCommand.Parameters.AddWithValue("?attribute_group_name", string.Empty);
      }

      _selectAttributeGroupCommand.Parameters["?attribute_set_id"].Value = set.attribute_set_id;
      _selectAttributeGroupCommand.Parameters["?attribute_group_name"].Value = attribute_group_name;

      using (var reader = _selectAttributeGroupCommand.ExecuteReader())
      {
        if (reader.Read())
        {
          return Mapper.Map<IDataRecord, eav_attribute_group>(reader);
        }
      }

      return null;
    }


    public eav_attribute_group CreateAttributeGroup(eav_attribute_set attributeset, string name)
    {
      var group = new eav_attribute_group()
      {
        attribute_set_id = attributeset.attribute_set_id,
        attribute_group_name = name,
        sort_order = 0,
        default_id = 0
      };
      return group;

    }

    private MySqlCommand _syncAttributeGroupCommand = null;
    public void SyncAttributeGroup(eav_attribute_group group)
    {
      var eag = TableName("eav_attribute_group");

      if (_syncAttributeGroupCommand == null)
      {
        _syncAttributeGroupCommand = Connection.CreateCommand();
        _syncAttributeGroupCommand.CommandType = CommandType.Text;
        _syncAttributeGroupCommand.CommandText = String.Format(@"INSERT INTO {0} (attribute_group_id, attribute_set_id, attribute_group_name, sort_order, default_id) 
                                VALUES (?attribute_group_id, ?attribute_set_id, ?attribute_group_name, ?sort_order, ?default_id)
                                 ON DUPLICATE KEY UPDATE attribute_set_id = VALUES(attribute_set_id), attribute_group_name = VALUES(attribute_group_name),
                                        sort_order = VALUES(sort_order) ,default_id = VALUES(default_id)", eag);


        _syncAttributeGroupCommand.Prepare();

        _syncAttributeGroupCommand.Parameters.AddWithValue("?attribute_group_id", null);
        _syncAttributeGroupCommand.Parameters.AddWithValue("?attribute_set_id", null);
        _syncAttributeGroupCommand.Parameters.AddWithValue("?attribute_group_name", null);
        _syncAttributeGroupCommand.Parameters.AddWithValue("?sort_order", null);
        _syncAttributeGroupCommand.Parameters.AddWithValue("?default_id", null);
      }

      _syncAttributeGroupCommand.Parameters["?attribute_group_id"].Value = group.attribute_group_id;
      _syncAttributeGroupCommand.Parameters["?attribute_set_id"].Value = group.attribute_set_id;
      _syncAttributeGroupCommand.Parameters["?attribute_group_name"].Value = group.attribute_group_name;
      _syncAttributeGroupCommand.Parameters["?sort_order"].Value = group.sort_order;
      _syncAttributeGroupCommand.Parameters["?default_id"].Value = group.default_id;
      _syncAttributeGroupCommand.ExecuteNonQuery();

      if (group.attribute_group_id == 0)
        group.attribute_group_id = (int)_syncAttributeGroupCommand.LastInsertedId;
    }

    private MySqlCommand _selectAttributeGroupsCommand = null;
    public SortedDictionary<string, eav_attribute_group> GetAttributeGroupList(eav_attribute_set attributeset)
    {
      SortedDictionary<string, eav_attribute_group> result = new SortedDictionary<string, eav_attribute_group>();
      if (_selectAttributeGroupsCommand == null)
      {
        _selectAttributeGroupsCommand = Connection.CreateCommand();
        _selectAttributeGroupsCommand.CommandType = CommandType.Text;
        _selectAttributeGroupsCommand.CommandText = @"SELECT * FROM eav_attribute_group WHERE attribute_set_id = ?attribute_set_id";

        _selectAttributeGroupsCommand.Prepare();
        _selectAttributeGroupsCommand.Parameters.AddWithValue("?attribute_set_id", 0);
      }

      _selectAttributeGroupsCommand.Parameters["?attribute_set_id"].Value = attributeset.attribute_set_id;


      using (var reader = _selectAttributeGroupsCommand.ExecuteReader())
      {
        var groups = Mapper.Map<IDataReader, List<eav_attribute_group>>(reader);
        foreach (var group in groups)
          result.Add(group.attribute_group_name, group);
      }


      return result;

    }


    public Dictionary<int, Dictionary<string, eav_attribute_group>> GetAttributeGroupsPerSet(int entity_type_id)
    {
      Dictionary<int, Dictionary<string, eav_attribute_group>> result = new Dictionary<int, Dictionary<string, eav_attribute_group>>();

      var selectGroupsCommand = Connection.CreateCommand();
      selectGroupsCommand.CommandType = System.Data.CommandType.Text;
      selectGroupsCommand.CommandText = @"select * From eav_attribute_group where attribute_set_id IN (select attribute_set_id FROM eav_attribute_set WHERE entity_type_id = ?entity_type_id)";
      selectGroupsCommand.Parameters.AddWithValue("?entity_type_id", entity_type_id);
      using (var reader = selectGroupsCommand.ExecuteReader())
      {
        var groups = Mapper.Map<IDataReader, List<eav_attribute_group>>(reader);
        foreach (var group in groups)
        {
          if (!result.ContainsKey(group.attribute_set_id))
            result.Add(group.attribute_set_id, new Dictionary<string, eav_attribute_group>());

          result[group.attribute_set_id].Add(group.attribute_group_name, group);
        }

      }

      return result;

    }

    public eav_entity_attribute CreateEntityAttribute(int entity_type_id, int attribute_set_id, int attribute_group_id, int attribute_id)
    {
      return new eav_entity_attribute()
      {
        entity_type_id = entity_type_id,
        attribute_set_id = attribute_set_id,
        attribute_id = attribute_id,
        attribute_group_id = attribute_group_id
      };
    }


    #endregion

    /// <summary>
    /// Retrieves the store list from magento
    /// </summary>
    /// <param name="usesWebsiteCodeConcatenation">Whether to construct the store code as website_code_store_code</param>
    /// <returns></returns>
    public Dictionary<string, StoreInfo> GetStoreList(bool usesWebsiteCodeConcatenation = false)
    {
      Dictionary<string, StoreInfo> result = new Dictionary<string, StoreInfo>();
      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = String.Format(@"select cs.code, cs.store_id, cs.website_id, cw.code as website_code from core_store cs
                                          inner join core_website cw on cw.website_id = cs.website_id
                                          where is_active = 1");
        cmd.CommandType = CommandType.Text;
        using (var reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            string code = reader["code"].ToString().ToUpper();

            if (usesWebsiteCodeConcatenation) //applies when a store code is not unique globally but on website level
              code = string.Format("{0}_{1}", reader["website_code"].ToString().ToUpper(), code);

            result.Add(code,

              new StoreInfo()
              {
                code = reader["code"].ToString(),
                store_id = Convert.ToInt32(reader["store_id"]),
                website_id = Convert.ToInt32(reader["website_id"])
              }
              );
          }
        }
      }

      return result;
    }

    private MySqlCommand _selectAttributeOptionsCommand = null;
    public Dictionary<string, int> GetAttributeOptions(int attribute_id)
    {

      if (_selectAttributeOptionsCommand == null)
      {
        _selectAttributeOptionsCommand = Connection.CreateCommand();
        _selectAttributeOptionsCommand.CommandType = CommandType.Text;

        _selectAttributeOptionsCommand.CommandText = @String.Format(@"select eao.concentrator_attribute_id, eao.option_id 
                                            From eav_attribute_option eao
                                        where eao.attribute_id=?attribute_id
                                        AND eao.concentrator_attribute_id IS NOT NULL and eao.concentrator_attribute_id != '0'
                                        ");

        _selectAttributeOptionsCommand.Prepare();
        _selectAttributeOptionsCommand.Parameters.AddWithValue("?attribute_id", 0);
      }

      _selectAttributeOptionsCommand.Parameters["?attribute_id"].Value = attribute_id;

      Dictionary<string, int> result = new Dictionary<string, int>();

      using (var reader = _selectAttributeOptionsCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          result.Add(reader.GetString(0), reader.GetInt32(1));
        }
      }

      return result;
    }


    private MySqlCommand _selectAttributeOptionValueCommand = null;
    private MySqlCommand _insertAttributeOptionCommand = null;
    private MySqlCommand _insertAttributeOptionValueCommand = null;
    private MySqlCommand _updateAttributeOptionValueCommand = null;
    public void SyncAttributeOption(eav_attribute_option_value record)
    {


      if (_selectAttributeOptionValueCommand == null)
      {
        _selectAttributeOptionValueCommand = Connection.CreateCommand();
        _selectAttributeOptionValueCommand.CommandType = CommandType.Text;
        _selectAttributeOptionValueCommand.CommandText = @"SELECT value_id FROM eav_attribute_option_value WHERE option_id = ?option_id AND store_id =?store_id AND value=?value";
        _selectAttributeOptionValueCommand.Prepare();
        _selectAttributeOptionValueCommand.Parameters.AddWithValue("?option_id", 0);
        _selectAttributeOptionValueCommand.Parameters.AddWithValue("?store_id", 0);
        _selectAttributeOptionValueCommand.Parameters.AddWithValue("?value", string.Empty);
      }

      if (_insertAttributeOptionValueCommand == null)
      {
        _insertAttributeOptionValueCommand = Connection.CreateCommand();
        _insertAttributeOptionValueCommand.CommandType = CommandType.Text;
        _insertAttributeOptionValueCommand.CommandText = @"INSERT INTO eav_attribute_option_value (option_id, store_id, value) VALUES (?option_id, ?store_id, ?value)";
        _insertAttributeOptionValueCommand.Prepare();
        _insertAttributeOptionValueCommand.Parameters.AddWithValue("?option_id", 0);
        _insertAttributeOptionValueCommand.Parameters.AddWithValue("?store_id", 0);
        _insertAttributeOptionValueCommand.Parameters.AddWithValue("?value", string.Empty);
      }

      if (_updateAttributeOptionValueCommand == null)
      {
        _updateAttributeOptionValueCommand = Connection.CreateCommand();
        _updateAttributeOptionValueCommand.CommandType = CommandType.Text;
        _updateAttributeOptionValueCommand.CommandText = @"UPDATE eav_attribute_option_value SET value = ?value WHERE value_id = ?value_id";
        _updateAttributeOptionValueCommand.Prepare();
        _updateAttributeOptionValueCommand.Parameters.AddWithValue("?value_id", 0);
        _updateAttributeOptionValueCommand.Parameters.AddWithValue("?value", string.Empty);
      }


      if (_insertAttributeOptionCommand == null)
      {
        _insertAttributeOptionCommand = Connection.CreateCommand();
        _insertAttributeOptionCommand.CommandType = CommandType.Text;
        _insertAttributeOptionCommand.CommandText = @"INSERT INTO eav_attribute_option (attribute_id, sort_order, concentrator_attribute_id) VALUES (?attribute_id, ?sort_order, ?concentrator_attribute_id)";
        _insertAttributeOptionCommand.Prepare();
        _insertAttributeOptionCommand.Parameters.AddWithValue("?attribute_id", 0);
        _insertAttributeOptionCommand.Parameters.AddWithValue("?sort_order", 0);
        _insertAttributeOptionCommand.Parameters.AddWithValue("?concentrator_attribute_id", 0);


      }
      if (record.option_id == 0)
      {
        _insertAttributeOptionCommand.Parameters["?attribute_id"].Value = record.attribute_id;
        _insertAttributeOptionCommand.Parameters["?sort_order"].Value = record.sort_order;
        _insertAttributeOptionCommand.Parameters["?concentrator_attribute_id"].Value = record.concentrator_attribute_id;
        _insertAttributeOptionCommand.ExecuteNonQuery();
        record.option_id = (int)_insertAttributeOptionCommand.LastInsertedId;
      }

      _selectAttributeOptionValueCommand.Parameters["?store_id"].Value = record.store_id;
      _selectAttributeOptionValueCommand.Parameters["?option_id"].Value = record.option_id;
      _selectAttributeOptionValueCommand.Parameters["?value"].Value = record.value;
      var value_id = _selectAttributeOptionValueCommand.ExecuteScalar();

      if (value_id == null)
      {
        _insertAttributeOptionValueCommand.Parameters["?store_id"].Value = record.store_id;
        _insertAttributeOptionValueCommand.Parameters["?option_id"].Value = record.option_id;
        _insertAttributeOptionValueCommand.Parameters["?value"].Value = record.value;
        _insertAttributeOptionValueCommand.ExecuteNonQuery();
      }
      else
      {

        _updateAttributeOptionValueCommand.Parameters["?value_id"].Value = Convert.ToInt32(value_id);
        _updateAttributeOptionValueCommand.Parameters["?value"].Value = record.value;
        _updateAttributeOptionValueCommand.ExecuteNonQuery();

      }


    }
  }

  public class StoreInfo
  {
    public int store_id { get; set; }
    public string code { get; set; }
    public int website_id { get; set; }
    public string website_code { get; set; }
  }


  public static class HelperExtensions
  {
    public static T[] ConvertTo<T>(this string[] source)
    {
      return (from l in source
              select (T)Convert.ChangeType(l, typeof(T))).ToArray();
    }

    public static string Multiply(this string source, int multiplier)
    {
      StringBuilder sb = new StringBuilder(multiplier * source.Length);
      for (int i = 0; i < multiplier; i++)
      {
        sb.Append(source);
      }

      return sb.ToString();


    }
    public static string ToFieldList(this IEnumerable<string> source)
    {

      var str = "?,".Multiply(source.Count());

      return str.Substring(0, str.Length - 1);

    }
  }
}

