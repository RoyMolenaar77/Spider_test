using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.Magento.Models;
using System.Data;
using MySql.Data.MySqlClient;
using AutoMapper;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class CustomerHelper : MagentoMySqlHelper
  {
    private eav_attribute address_line_id_attribute = null;


    public CustomerHelper(string connectionString)
      : base(connectionString)
    {
      AutoMapper.Mapper.CreateMap<IDataReader, customer_entity>();
      AutoMapper.Mapper.CreateMap<IDataRecord, customer_entity>();

      AutoMapper.Mapper.CreateMap<IDataReader, customer_address_entity>();
      AutoMapper.Mapper.CreateMap<IDataRecord, customer_address_entity>();

      address_line_id_attribute = GetAttribute("address_line_id");
    }



    #region Customers

    private MySqlCommand selectCustomersCommand = null;
    internal Dictionary<int, customer_entity> GetCustomers()
    {
      #region Command Definition
      if (selectCustomersCommand == null)
      {
        selectCustomersCommand = Connection.CreateCommand();
        selectCustomersCommand.CommandType = CommandType.Text;
        selectCustomersCommand.CommandText = String.Format("SELECT * FROM {0}", _tablesNames["customer_entity"]);

        selectCustomersCommand.Prepare();
      }

      #endregion


      Dictionary<int, customer_entity> result = new Dictionary<int, customer_entity>();
      using (var reader = selectCustomersCommand.ExecuteReader())
      {
        var custs = Mapper.Map<IDataReader, List<customer_entity>>(reader);

        custs.ForEach(c => result.Add(c.entity_id, c));

      }

      return result;

    }

    private MySqlCommand addNewCustomerCommand = null;
    internal void AddCustomer(customer_entity entity)
    {
      #region Command Definition
      if (addNewCustomerCommand == null)
      {
        addNewCustomerCommand = Connection.CreateCommand();
        addNewCustomerCommand.CommandType = CommandType.Text;
        addNewCustomerCommand.CommandText = String.Format(@"INSERT INTO {0} (entity_id, entity_type_id, attribute_set_id, website_id, email, group_id, increment_id, store_id, created_at, updated_at, is_active)
                                                          VALUES (?entity_id, ?entity_type_id, ?attribute_set_id, ?website_id, ?email, ?group_id, ?increment_id, ?store_id, ?created_at, ?updated_at, ?is_active)
                                                          ", _tablesNames["customer_entity"]);

        addNewCustomerCommand.Prepare();

        addNewCustomerCommand.Parameters.AddWithValue("?entity_id", 0);
        addNewCustomerCommand.Parameters.AddWithValue("?entity_type_id", 0);
        addNewCustomerCommand.Parameters.AddWithValue("?attribute_set_id", 0);
        addNewCustomerCommand.Parameters.AddWithValue("?website_id", 0);
        addNewCustomerCommand.Parameters.AddWithValue("?email", string.Empty);
        addNewCustomerCommand.Parameters.AddWithValue("?group_id", 0);
        addNewCustomerCommand.Parameters.AddWithValue("?increment_id", string.Empty);
        addNewCustomerCommand.Parameters.AddWithValue("?store_id", 0);
        addNewCustomerCommand.Parameters.AddWithValue("?created_at", DateTime.Now);
        addNewCustomerCommand.Parameters.AddWithValue("?updated_at", DateTime.Now);
        addNewCustomerCommand.Parameters.AddWithValue("?is_active", true);
      }
      #endregion

      addNewCustomerCommand.Parameters["?entity_id"].Value = entity.entity_id;
      addNewCustomerCommand.Parameters["?entity_type_id"].Value = entity.entity_type_id;
      addNewCustomerCommand.Parameters["?attribute_set_id"].Value = entity.attribute_set_id;
      addNewCustomerCommand.Parameters["?website_id"].Value = entity.website_id;
      addNewCustomerCommand.Parameters["?email"].Value = entity.email;
      addNewCustomerCommand.Parameters["?group_id"].Value = entity.group_id;
      addNewCustomerCommand.Parameters["?increment_id"].Value = entity.increment_id;
      addNewCustomerCommand.Parameters["?store_id"].Value = entity.store_id;
      addNewCustomerCommand.Parameters["?created_at"].Value = entity.created_at;
      addNewCustomerCommand.Parameters["?updated_at"].Value = entity.updated_at;
      addNewCustomerCommand.Parameters["?is_active"].Value = entity.is_active;


      addNewCustomerCommand.ExecuteNonQuery();



    }

    private MySqlCommand selectCustomerEmailAddressesCommand = null;
    internal HashSet<string> GetCustomerEmailAddresses()
    {
      #region Command Definition
      if (selectCustomerEmailAddressesCommand == null)
      {
        selectCustomerEmailAddressesCommand = Connection.CreateCommand();
        selectCustomerEmailAddressesCommand.CommandType = CommandType.Text;
        selectCustomerEmailAddressesCommand.CommandText = String.Format("SELECT email FROM {0}", _tablesNames["customer_entity"]);

        selectCustomerEmailAddressesCommand.Prepare();
      }

      #endregion


      HashSet<string> result = new HashSet<string>();
      using (var reader = selectCustomerEmailAddressesCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          result.Add(reader["email"].ToString());
        }
      }

      return result;
    }

    private MySqlCommand selectCustomerAddressCommand = null;
    public customer_address_entity GetCustomerAddress(int entity_id, int? address_line_id = null)
    {
      if (selectCustomerAddressCommand == null)
      {
        selectCustomerAddressCommand = Connection.CreateCommand();
        selectCustomerAddressCommand.CommandType = CommandType.Text;
        selectCustomerAddressCommand.CommandText = String.Format(@"SELECT * FROM {0} cae  
                                                                INNER JOIN {1} caei
                                                        ON (cae.entity_id =caei.entity_id AND caei.attribute_id = ?attribute_id  AND cae.entity_type_id = caei.entity_type_id) 
                                                        WHERE cae.entity_type_id = ?entity_type_id 
                                                        AND parent_id = ?entity_id 
                                                        AND caei.value = ?address_line_id
                                                        ", _tablesNames["customer_address_entity"], _tablesNames["customer_address_entity_int"]);
        selectCustomerAddressCommand.Prepare();
        selectCustomerAddressCommand.Parameters.AddWithValue("?entity_type_id", CUSTOMER_ADDRESS_ENTITY_TYPE_ID);
        selectCustomerAddressCommand.Parameters.AddWithValue("?attribute_id", address_line_id_attribute.attribute_id);

        selectCustomerAddressCommand.Parameters.AddWithValue("?entity_id", 0);
        selectCustomerAddressCommand.Parameters.AddWithValue("?address_line_id", 0);

      }


      if (address_line_id == null)
        address_line_id = entity_id;

      selectCustomerAddressCommand.Parameters["?entity_id"].Value = entity_id;
      selectCustomerAddressCommand.Parameters["?address_line_id"].Value = address_line_id;

      using (var reader = selectCustomerAddressCommand.ExecuteReader())
      {
        if (reader.Read())
        {

          return Mapper.Map<IDataRecord, customer_address_entity>(reader);
        }
      }

      return null;
    }

    private MySqlCommand syncAddressCommand = null;

    public void SyncAddress(customer_address_entity addressEntity)
    {
      if (syncAddressCommand == null)
      {
        syncAddressCommand = Connection.CreateCommand();
        syncAddressCommand.CommandType = CommandType.Text;
        syncAddressCommand.CommandText = String.Format(@"INSERT INTO {0} (entity_id, entity_type_id, attribute_set_id, increment_id, parent_id, created_at, updated_at, is_active)
                                                         VALUES (?entity_id, ?entity_type_id, ?attribute_set_id, ?increment_id, ?parent_id, ?created_at, ?updated_at, ?is_active)
                                                         ON DUPLICATE KEY UPDATE is_active = VALUES(is_active)
                                                        ", _tablesNames["customer_address_entity"]);

        syncAddressCommand.Prepare();
        syncAddressCommand.Parameters.AddWithValue("?entity_id", null);
        syncAddressCommand.Parameters.AddWithValue("?entity_type_id", 0);
        syncAddressCommand.Parameters.AddWithValue("?attribute_set_id", 0);
        syncAddressCommand.Parameters.AddWithValue("?increment_id", String.Empty);
        syncAddressCommand.Parameters.AddWithValue("?parent_id", 0);
        syncAddressCommand.Parameters.AddWithValue("?created_at", DateTime.Now);
        syncAddressCommand.Parameters.AddWithValue("?updated_at", DateTime.Now);
        syncAddressCommand.Parameters.AddWithValue("?is_active", true);

      }



      syncAddressCommand.Parameters["?entity_id"].Value = addressEntity.entity_id;
      syncAddressCommand.Parameters["?entity_type_id"].Value = addressEntity.entity_type_id;
      syncAddressCommand.Parameters["?attribute_set_id"].Value = addressEntity.attribute_set_id;
      syncAddressCommand.Parameters["?increment_id"].Value = addressEntity.increment_id;
      syncAddressCommand.Parameters["?parent_id"].Value = addressEntity.parent_id;
      syncAddressCommand.Parameters["?created_at"].Value = addressEntity.created_at;
      syncAddressCommand.Parameters["?updated_at"].Value = addressEntity.updated_at;
      syncAddressCommand.Parameters["?is_active"].Value = addressEntity.is_active;

      syncAddressCommand.ExecuteNonQuery();

      if (addressEntity.entity_id == 0)
        addressEntity.entity_id = (int)syncAddressCommand.LastInsertedId;

    }


    #endregion



  }
}
