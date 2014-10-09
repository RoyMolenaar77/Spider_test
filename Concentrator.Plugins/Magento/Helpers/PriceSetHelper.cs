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
	public class PriceSetHelper : MagentoMySqlHelper
	{


		public PriceSetHelper(string connectionString)
			: base(connectionString)
		{
			AutoMapper.Mapper.CreateMap<IDataReader, salesrule>();
			AutoMapper.Mapper.CreateMap<IDataRecord, salesrule>();

		}

		public List<salesrule> GetSalesRules(int website_id)
		{

			var selectSalesRulesCommand = Connection.CreateCommand();
			selectSalesRulesCommand.CommandText = @"SELECT * FROM salesrule";
			//WHERE website_id = ?website_id";
			selectSalesRulesCommand.CommandType = System.Data.CommandType.Text;
			//attributeListCommand.Parameters.AddWithValue("?website_id", website_id);

			using (var reader = selectSalesRulesCommand.ExecuteReader())
			{
				return Mapper.Map<IDataReader, List<salesrule>>(reader);
			}

		}



		private MySqlCommand syncSalesRuleLabelCommand = null;
		private MySqlCommand syncSalesRuleCommand = null;
		public void SyncSalesRule(salesrule entity)
		{

			#region Command Definition

			if (syncSalesRuleCommand == null)
			{
				syncSalesRuleCommand = Connection.CreateCommand();
				syncSalesRuleCommand.CommandType = CommandType.Text;
				syncSalesRuleCommand.CommandText = String.Format(@"INSERT INTO salesrule 
(rule_id, name, description, from_date, to_date, uses_per_customer, customer_group_ids, is_active, conditions_serialized, actions_serialized, stop_rules_processing, is_advanced,
product_ids, sort_order, simple_action, discount_amount, discount_qty, discount_step, simple_free_shipping, apply_to_shipping, times_used, is_rss, website_ids, coupon_type)
VALUES
(?rule_id, ?name, ?description, ?from_date, ?to_date, ?uses_per_customer, ?customer_group_ids, ?is_active, ?conditions_serialized, ?actions_serialized, ?stop_rules_processing, ?is_advanced,
?product_ids, ?sort_order, ?simple_action, ?discount_amount, ?discount_qty, ?discount_step, ?simple_free_shipping, ?apply_to_shipping, ?times_used, ?is_rss, ?website_ids, ?coupon_type)
ON DUPLICATE KEY UPDATE from_date = VALUES(from_date), to_date = VALUES(to_date), is_active = VALUES(is_active), conditions_serialized=VALUES(conditions_serialized),
actions_serialized=VALUES(actions_serialized), is_advanced=VALUES(is_advanced), is_rss=VALUES(is_rss), customer_group_ids = VALUES(customer_group_ids), website_ids = VALUES(website_ids),
discount_qty = VALUES(discount_qty), discount_amount = VALUES(discount_amount), discount_step = VALUES(discount_step), simple_action=VALUES(simple_action)");



				syncSalesRuleCommand.Prepare();
				syncSalesRuleCommand.Parameters.AddWithValue("?rule_id", 0);
				syncSalesRuleCommand.Parameters.AddWithValue("?name", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?description", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?from_date", (DateTime?)null);
				syncSalesRuleCommand.Parameters.AddWithValue("?to_date", (DateTime?)null);
				syncSalesRuleCommand.Parameters.AddWithValue("?uses_per_customer", 0);
				syncSalesRuleCommand.Parameters.AddWithValue("?customer_group_ids", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?is_active", false);
				syncSalesRuleCommand.Parameters.AddWithValue("?conditions_serialized", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?actions_serialized", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?stop_rules_processing", false);
				syncSalesRuleCommand.Parameters.AddWithValue("?is_advanced", false);
				syncSalesRuleCommand.Parameters.AddWithValue("?product_ids", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?sort_order", 0);
				syncSalesRuleCommand.Parameters.AddWithValue("?simple_action", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?discount_amount", (decimal)0);
				syncSalesRuleCommand.Parameters.AddWithValue("?discount_qty", (decimal)0);
				syncSalesRuleCommand.Parameters.AddWithValue("?discount_step", 0);
				syncSalesRuleCommand.Parameters.AddWithValue("?simple_free_shipping", false);
				syncSalesRuleCommand.Parameters.AddWithValue("?apply_to_shipping", false);
				syncSalesRuleCommand.Parameters.AddWithValue("?times_used", 0);
				syncSalesRuleCommand.Parameters.AddWithValue("?is_rss", false);
				syncSalesRuleCommand.Parameters.AddWithValue("?website_ids", string.Empty);
				syncSalesRuleCommand.Parameters.AddWithValue("?coupon_type", 0);

			}


			if (syncSalesRuleLabelCommand == null)
			{
				syncSalesRuleLabelCommand = Connection.CreateCommand();
				syncSalesRuleLabelCommand.CommandType = CommandType.Text;
				syncSalesRuleLabelCommand.CommandText = String.Format(@"INSERT INTO salesrule_label (rule_id, store_id, label) VALUES (?rule_id, ?store_id, ?label) ON DUPLICATE KEY UPDATE label = VALUES(label)");


				syncSalesRuleLabelCommand.Prepare();
				syncSalesRuleLabelCommand.Parameters.AddWithValue("?rule_id", 0);
				syncSalesRuleLabelCommand.Parameters.AddWithValue("?store_id", 0);
				syncSalesRuleLabelCommand.Parameters.AddWithValue("?label", string.Empty);

			}
			#endregion

			syncSalesRuleCommand.Parameters["?rule_id"].Value = entity.rule_id;
			syncSalesRuleCommand.Parameters["?name"].Value = entity.name;
			syncSalesRuleCommand.Parameters["?description"].Value = entity.description;
			syncSalesRuleCommand.Parameters["?from_date"].Value = entity.from_date;
			syncSalesRuleCommand.Parameters["?to_date"].Value = entity.to_date;
			syncSalesRuleCommand.Parameters["?customer_group_ids"].Value = entity.customer_group_ids;
			syncSalesRuleCommand.Parameters["?actions_serialized"].Value = entity.actions_serialized;
			syncSalesRuleCommand.Parameters["?conditions_serialized"].Value = entity.conditions_serialized;
			syncSalesRuleCommand.Parameters["?is_advanced"].Value = entity.is_advanced;
			syncSalesRuleCommand.Parameters["?is_active"].Value = entity.is_active;
			syncSalesRuleCommand.Parameters["?website_ids"].Value = entity.website_ids;
			syncSalesRuleCommand.Parameters["?simple_action"].Value = entity.simple_action;
			syncSalesRuleCommand.Parameters["?is_rss"].Value = entity.is_rss;

			syncSalesRuleCommand.Parameters["?discount_step"].Value = entity.discount_step;
			syncSalesRuleCommand.Parameters["?discount_amount"].Value = entity.discount_amount;
			syncSalesRuleCommand.Parameters["?discount_qty"].Value = entity.discount_qty;



			syncSalesRuleCommand.ExecuteNonQuery();

			if (entity.rule_id == 0)
				entity.rule_id = (int)syncSalesRuleCommand.LastInsertedId;

			syncSalesRuleLabelCommand.Parameters["?rule_id"].Value = entity.rule_id;
			syncSalesRuleLabelCommand.Parameters["?store_id"].Value = 0;
			syncSalesRuleLabelCommand.Parameters["?label"].Value = entity.label;
			syncSalesRuleLabelCommand.ExecuteNonQuery();
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

		private MySqlCommand _selectSalesRuleCommand;
		public salesrule GetSalesRule(int rule_id)
		{
			if (_selectSalesRuleCommand == null)
			{
				_selectSalesRuleCommand = Connection.CreateCommand();
				_selectSalesRuleCommand.CommandText = "select * from salesrule where rule_id = ?rule_id";
				_selectSalesRuleCommand.CommandType = CommandType.Text;
				_selectSalesRuleCommand.Parameters.AddWithValue("?rule_id", 0);
			}


			_selectSalesRuleCommand.Parameters["?rule_id"].Value = rule_id;
			using (var reader = _selectSalesRuleCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					return Mapper.Map<IDataReader, salesrule>(reader);
				}
			}
			return null;
		}


		public List<int> GetCustomerGroups()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = String.Format(@"select customer_group_id from customer_group");
				cmd.CommandType = System.Data.CommandType.Text;


				List<int> result = new List<int>();
				using (var reader = cmd.ExecuteReader())
				{

					while (reader.Read())
					{
						result.Add(reader.GetInt32(0));
					}
				}
				return result;
			}


		}
	}
}
