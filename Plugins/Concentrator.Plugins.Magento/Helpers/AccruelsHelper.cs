using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Plugins.Magento.Models;
using AutoMapper;
using MySql.Data.MySqlClient;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class AccruelsHelper : MagentoMySqlHelper
  {


    public AccruelsHelper(string connectionString)
      : base(connectionString)
    {
      AutoMapper.Mapper.CreateMap<IDataReader, weee_tax>();
      AutoMapper.Mapper.CreateMap<IDataRecord, weee_tax>();

    }

    public List<weee_tax> GetWeeeTaxList(int website_id)
    {

      var attributeListCommand = Connection.CreateCommand();
      attributeListCommand.CommandText = @"SELECT * FROM weee_tax WHERE website_id = ?website_id";
      attributeListCommand.CommandType = System.Data.CommandType.Text;
      attributeListCommand.Parameters.AddWithValue("?website_id", website_id);

      using (var reader = attributeListCommand.ExecuteReader())
      {
        return Mapper.Map<IDataReader, List<weee_tax>>(reader);
      }

    }




    private MySqlCommand syncWeeeAttributeCommand = null;
    public void SyncWeeeAttribute(weee_tax entity)
    {
      if (syncWeeeAttributeCommand == null)
      {
        syncWeeeAttributeCommand = Connection.CreateCommand();
        syncWeeeAttributeCommand.CommandType = CommandType.Text;
        syncWeeeAttributeCommand.CommandText = String.Format(@"INSERT INTO weee_tax (value_id, website_id, entity_id, country, value, state, attribute_id, entity_type_id)
                                                               VALUES (?value_id, ?website_id, ?entity_id, ?country, ?value, ?state, ?attribute_id, ?entity_type_id)
                                                               ON DUPLICATE KEY UPDATE value = VALUES(value)");

        syncWeeeAttributeCommand.Prepare();
        syncWeeeAttributeCommand.Parameters.AddWithValue("?value_id", 0);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?website_id", 0);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?entity_id", 0);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?country", String.Empty);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?value", 0m);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?state", string.Empty);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?attribute_id", 0);
        syncWeeeAttributeCommand.Parameters.AddWithValue("?entity_type_id", 0);
      }

      syncWeeeAttributeCommand.Parameters["?value_id"].Value = entity.value_id;
      syncWeeeAttributeCommand.Parameters["?website_id"].Value = entity.website_id;
      syncWeeeAttributeCommand.Parameters["?entity_id"].Value = entity.entity_id;
      syncWeeeAttributeCommand.Parameters["?country"].Value = entity.country;
      syncWeeeAttributeCommand.Parameters["?value"].Value = entity.value;
      syncWeeeAttributeCommand.Parameters["?state"].Value = entity.state;
      syncWeeeAttributeCommand.Parameters["?attribute_id"].Value = entity.attribute_id;
      syncWeeeAttributeCommand.Parameters["?entity_type_id"].Value = entity.entity_type_id;


      syncWeeeAttributeCommand.ExecuteNonQuery();

      if (entity.value_id == 0)
        entity.value_id = (int)syncWeeeAttributeCommand.LastInsertedId;
    }

    public void CleanupWeeeAttributes(List<int> touched_valued_id_list, string country)
    {

      if (touched_valued_id_list.Count == 0)
        return;

      var idList = String.Join(",", touched_valued_id_list.Distinct());
      using (var cleanupCommand = Connection.CreateCommand())
      {
        cleanupCommand.CommandText = String.Format(@"DELETE FROM weee_tax WHERE country = ?country AND value_id NOT IN ({0})", idList);
        cleanupCommand.CommandType = System.Data.CommandType.Text;
        cleanupCommand.Parameters.AddWithValue("?country", country);

        cleanupCommand.ExecuteNonQuery();
      }
      
    }

  }
}
