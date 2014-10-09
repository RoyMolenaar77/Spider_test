using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Plugins.Magento.Models;
using AutoMapper;
using MySql.Data.MySqlClient;
using System.Net;
using Concentrator.Objects.Models.Connectors;
using AuditLog4Net.Adapter;
using Concentrator.Web.Objects.EDI;
using System.Configuration;

namespace Concentrator.Plugins.Magento.Helpers
{
  public class OrderHelper : MagentoMySqlHelper
  {


    public OrderHelper(string connectionString)
      : base(connectionString)
    {

      AutoMapper.Mapper.CreateMap<IDataReader, sales_flat_order>();
      AutoMapper.Mapper.CreateMap<IDataRecord, sales_flat_order>();

      AutoMapper.Mapper.CreateMap<IDataReader, sales_flat_order_grid>();
      AutoMapper.Mapper.CreateMap<IDataRecord, sales_flat_order_grid>();

      AutoMapper.Mapper.CreateMap<IDataReader, sales_flat_order_item>();
      AutoMapper.Mapper.CreateMap<IDataRecord, sales_flat_order_item>();


      AutoMapper.Mapper.CreateMap<IDataReader, basstore_info>();
      AutoMapper.Mapper.CreateMap<IDataRecord, basstore_info>();

    }



    public sales_flat_order GetSalesOrder(string increment_id)
    {
      using (var cmd = Connection.CreateCommand())
      {

        cmd.CommandText = @"SELECT * FROM sales_flat_order WHERE increment_id = ?increment_id";
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.AddWithValue("?increment_id", increment_id);

        using (var reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            return Mapper.Map<IDataRecord, sales_flat_order>(reader);
          }

          return null;

        }
      }
    }

    public bool ColumnExists(string table, string column)
    {

      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = String.Format("SHOW COLUMNS FROM {0} LIKE '{1}';", table, column);

        using (var reader = cmd.ExecuteReader())
        {
          if (reader.Read())
            return true;
        }

      }

      return false;
    }



    public DataSet GetSalesOrders(string websiteCodeInCoreTable)
    {
      DataSet result = new DataSet();
      using (var cmd = Connection.CreateCommand())
      {
        bool addServicePointID = ColumnExists("sales_flat_order_address", "kp_id");
        bool addServicePointCode = ColumnExists("sales_flat_order", "kiala_service_point_code");
        bool addCompanyPointName = ColumnExists("sales_flat_order_address", "company");

        string servicePointIDQuery = addServicePointID ? "sales_flat_order_address.kp_id as ServicePointID," : "'' as ServicePointID,";
        string servicePointCodeQuery = addServicePointCode ? "sales_flat_order.kiala_service_point_code as ServicePointCode," : "'' as ServicePointCode,";
        string companyPointNameQuery = addCompanyPointName ? "sales_flat_order_address.company as CompanyName," : "'' as CompanyName,";

        cmd.CommandText = string.Format(@"SELECT   distinct   
            sales_flat_order.increment_id AS orderid,
						sales_flat_order_item.product_type,
						sales_flat_order_item.quote_item_id,
						sales_flat_order_item.parent_item_id AS parentid,
						sales_flat_order_item.sku AS itemcode,
						sales_flat_order_item.name AS itemname,
						sales_flat_order_item.qty_ordered AS qty_ordered,
						sales_flat_order_item.qty_shipped AS qty_shipped,
            sales_flat_order_item.discount_amount as lineDiscount,
						sales_flat_order_item.applied_rule_ids,
            cs.code as orderLanguageCode,
            cp.value as baseprice,
            cp2.value as baseprice_global,
						sales_flat_order.customer_email AS email,
						sales_flat_order_address.prefix AS title,
						sales_flat_order_address.firstname AS firstname,
						sales_flat_order_address.lastname AS lastname,
						sales_flat_order_address.street AS address,
            sales_flat_order_address.housenumber AS housenumber,
						sales_flat_order_address.city AS city,
						sales_flat_order_address.region AS region,
						sales_flat_order_address.country_id AS country,
						sales_flat_order_address.postcode AS postcode,
						sales_flat_order_address.telephone AS telephone,
            sales_flat_order_address.middlename AS middlename,
            sales_flat_order_address.housenumber_extension as housenumberextension,
            {0}
            {1}
            {2}
            sales_flat_order.customer_email AS bEmail,
            addressBilling.prefix AS bTitle,
            addressBilling.firstname AS bFirstname,
            addressBilling.lastname AS bLastname,
            addressBilling.street AS bAddress,
            addressBilling.housenumber AS bHousenumber,
            addressBilling.city AS bCity,
            addressBilling.region AS bRegion,
            addressBilling.country_id AS bCountry,
            addressBilling.postcode AS bPostcode,
            addressBilling.telephone AS bTelephone,
            addressBilling.middlename AS bMiddlename,
            addressBilling.housenumber_extension AS bHousenumberextension,

            sales_flat_order.state as OrderState,
            catalog_product_entity_int.value as concentrator_item_number,
            '' as customer_note,
            sales_flat_order_payment.method as payment_method,
            sales_flat_order_item.item_id as linenumber,
            eav_attribute_option_value.value as brandname,
            
            sales_flat_order.status AS OrderStatus,
            sales_flat_order.shipping_incl_tax AS ShippingAmount,
            sales_flat_order.store_companyno as StoreNumber,
            sales_flat_order.Customer_id AS CustomerID,
            sales_flat_order_item.row_total_incl_tax as LinePrice
						FROM sales_flat_order
            INNER JOIN sales_flat_order_item
              ON sales_flat_order.entity_id = sales_flat_order_item.order_id
            INNER JOIN sales_flat_order_payment
              on sales_flat_order.entity_id = sales_flat_order_payment.parent_id
						INNER JOIN sales_flat_order_address
						  ON sales_flat_order.entity_id = sales_flat_order_address.parent_id
            INNER JOIN sales_flat_order_address addressBilling
              ON addressBilling.parent_id = sales_flat_order.entity_id and addressBilling.address_type = 'billing'

            INNER JOIN catalog_product_entity 
                on sales_flat_order_item.sku = catalog_product_entity.sku
            
            INNER JOIN catalog_product_entity_int 
                on catalog_product_entity_int.entity_id = catalog_product_entity.entity_id            
            
            INNER join eav_attribute attributeCIN 
                on catalog_product_entity_int.attribute_id = attributeCIN.attribute_id            
            
            INNER JOIN catalog_product_entity_int entity_int_2 
                on entity_int_2.entity_id = catalog_product_entity.entity_id            
            
            INNER JOIN eav_attribute_option_value 
                on eav_attribute_option_value.option_id = entity_int_2.value
            
            INNER JOIN eav_attribute attributeBRAND 
                on attributeBRAND.attribute_id = entity_int_2.attribute_id            
       

INNER join catalog_product_entity_decimal cp ON (cp.entity_id=sales_flat_order_item.product_id AND sales_flat_order.store_id = cp.store_id
            AND cp.attribute_id = (SELECT attribute_id FROM eav_attribute WHERE attribute_code='price'  and entity_type_id=4)
            )
            
            INNER join core_store cs on cp.store_id = cs.store_id " +
             (string.IsNullOrEmpty(websiteCodeInCoreTable) ? "" : "INNER join core_website cw on cs.website_id = cw.website_id AND cw.code = '{3}'")
              +
            @"left join catalog_product_entity_decimal cp2 ON (cp2.entity_id=sales_flat_order_item.product_id AND 0 = cp2.store_id
            AND cp2.attribute_id = (SELECT attribute_id FROM eav_attribute WHERE attribute_code='price'  and entity_type_id=4)
            )

            WHERE  
            sales_flat_order_address.address_type = 'shipping'
            
            AND attributeCIN.attribute_code = 'concentrator_product_id'
            AND attributeBRAND.attribute_code = 'manufacturer'
            
						AND sales_flat_order_item.product_type = 'configurable'
						AND sales_flat_order_item.qty_shipped < sales_flat_order_item.qty_ordered
            AND (sales_flat_order.state IN ('new','processing'))
            AND (sales_flat_order.status IN ('processing' ,'shop_order', 'Acknowledged'))
            AND  ( 
                        (sales_flat_order.entity_id  not in (
                        SELECT parent_id as order_id
                        FROM sales_flat_order_status_history 
                        WHERE comment like '%(92)%'))
                        
                    OR
                    (
                    1 = (select max(additional_information  REGEXP 's:1:""9""') from sales_flat_order_payment where parent_id = sales_flat_order.entity_id)
                    )
                    
            )      
", servicePointIDQuery, servicePointCodeQuery, companyPointNameQuery, websiteCodeInCoreTable);

        cmd.CommandType = CommandType.Text;


        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        {
          da.Fill(result);
        }
      }

      return result;
    }



    internal basstore_info GetStoreInfo(string shopID)
    {
      DataSet result = new DataSet();
      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = @"SELECT  street as addressLine1,
            street_2 as addressLine2,
            city as city,
            postcode as postcode
            FROM basstore where companyno = ?companyno
          ";

        cmd.Parameters.AddWithValue("?companyno", shopID);

        using (var reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            return Mapper.Map<IDataRecord, basstore_info>(reader);
          }

          return null;

        }
      }
    }


    internal void UpdateOrderStatus(string increment_id, MagentoOrderState state, MagentoOrderStatus status)
    {
      using (var cm = Connection.CreateCommand())
      {

        cm.CommandText = "UPDATE sales_flat_order SET state = ?state, status = ?status WHERE increment_id = ?increment_id";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?increment_id", increment_id);
        cm.Parameters.AddWithValue("?state", state.ToString().ToLower());
        cm.Parameters.AddWithValue("?status", status.ToString());

        cm.ExecuteNonQuery();

        cm.CommandText = "UPDATE sales_flat_order_grid SET status = ?status WHERE increment_id = ?increment_id";
        cm.ExecuteNonQuery();

      }
    }



    internal bool UpdateOrderLine(sales_flat_order order, InvoiceOrderDetail line)
    {
      using (var cm = Connection.CreateCommand())
      {

        cm.CommandText = "UPDATE sales_flat_order_item SET qty_invoiced = ?qty_invoiced WHERE order_id = ?order_id AND sku = '?sku'";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?order_id", order.entity_id);
        cm.Parameters.AddWithValue("?sku", line.ProductIdentifier.ManufacturerItemID);

        cm.Parameters.AddWithValue("?qty_invoiced", line.Quantity.QuantityShipped);

        return (cm.ExecuteNonQuery() > 0);


      }
    }

    internal bool UpdateOrderLine(sales_flat_order order, ShipmentOrderDetail line)
    {
      using (var cm = Connection.CreateCommand())
      {

        cm.CommandText = "UPDATE sales_flat_order_item SET qty_backordered = ?qty_backordered, qty_shipped = ?qty_shipped, qty_canceled = ?qty_cancelled WHERE order_id = ?order_id AND sku = '?sku'";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?order_id", order.entity_id);
        cm.Parameters.AddWithValue("?sku", line.ProductIdentifier.ManufacturerItemID);

        cm.Parameters.AddWithValue("?qty_cancelled", line.Quantity.QuantityCancelled);
        cm.Parameters.AddWithValue("?qty_backordered", line.Quantity.QuantityBackordered);
        cm.Parameters.AddWithValue("?qty_shipped", line.Quantity.QuantityShipped);

        return (cm.ExecuteNonQuery() > 0);


      }
    }

    internal bool UpdateOrderLine(sales_flat_order order, OrderResponseDetail line)
    {
      using (var cm = Connection.CreateCommand())
      {

        cm.CommandText = "UPDATE sales_flat_order_item SET qty_canceled = ?qty_cancelled, qty_backordered =?qty_backordered WHERE order_id = ?order_id AND sku = '?sku'";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?order_id", order.entity_id);
        cm.Parameters.AddWithValue("?sku", line.ProductIdentifier.ManufacturerItemID);
        cm.Parameters.AddWithValue("?qty_backordered", line.Quantity.QuantityBackordered);
        cm.Parameters.AddWithValue("?qty_cancelled", line.Quantity.QuantityCancelled);

        return (cm.ExecuteNonQuery() > 0);


      }
    }

    //internal sales_flat_order_item GetOrderLine(sales_flat_order order, InvoiceOrderDetail line)
    //{
    //  using (var cm = Connection.CreateCommand())
    //  {

    //    cm.CommandText = "SELECT * From sales_flat_order_item WHERE   order_id = ?order_id AND item_id = ?item_id";
    //    cm.CommandType = CommandType.Text;
    //    cm.Parameters.AddWithValue("?order_id", order.entity_id);
    //    cm.Parameters.AddWithValue("?item_id", line.LineNumber);

    //    using (var reader = cm.ExecuteReader())
    //    {
    //      if (reader.Read())
    //      {
    //        return Mapper.Map<IDataRecord, sales_flat_order_item>(reader);
    //      }

    //      return null;

    //    }
    //  }
    //}

    internal int GetOrderLinesCount(sales_flat_order order, bool checkConfigurable = false)
    {
      using (var cm = Connection.CreateCommand())
      {
        cm.CommandText = "SELECT count(*) FROM sales_flat_order_item WHERE order_id = ?order_id AND product_type = ?product_type";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?order_id", order.entity_id);
        cm.Parameters.AddWithValue("?product_type", checkConfigurable ? "configurable" : "simple");

        //using (var reader = cm.ExecuteScalar())
        //{

        //  if (reader.Read())
        //  {
        //    return Mapper.Map<IDataRecord, List<sales_flat_order_item>>(reader);
        //  }

        //  return null;
        //}

        return Convert.ToInt32(cm.ExecuteScalar());
      }
    }

    internal sales_flat_order_item GetOrderLine(sales_flat_order order, string lineSku, bool checkConfigurable = false)
    {
      using (var cm = Connection.CreateCommand())
      {

        cm.CommandText = "SELECT * FROM sales_flat_order_item WHERE order_id = ?order_id AND sku = ?sku AND product_type = ?product_type";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?order_id", order.entity_id);
        cm.Parameters.AddWithValue("?sku", lineSku);
        cm.Parameters.AddWithValue("?product_type", checkConfigurable ? "configurable" : "simple");

        using (var reader = cm.ExecuteReader())
        {
          if (reader.Read())
          {
            return Mapper.Map<IDataRecord, sales_flat_order_item>(reader);
          }

          return null;

        }
      }
    }

    //internal sales_flat_order_item GetOrderLine(sales_flat_order order, OrderResponseDetail line)
    //{
    //  using (var cm = Connection.CreateCommand())
    //  {

    //    cm.CommandText = "SELECT * From sales_flat_order_item WHERE   order_id = ?order_id AND item_id = ?item_id";
    //    cm.CommandType = CommandType.Text;
    //    cm.Parameters.AddWithValue("?order_id", order.entity_id);
    //    cm.Parameters.AddWithValue("?item_id", line.LineNumber);

    //    using (var reader = cm.ExecuteReader())
    //    {
    //      if (reader.Read())
    //      {
    //        return Mapper.Map<IDataRecord, sales_flat_order_item>(reader);
    //      }

    //      return null;

    //    }
    //  }
    //}



    internal string GetOrderStoreCode(string increment_id)
    {
      using (var cm = Connection.CreateCommand())
      {

        cm.CommandText = "select cs.code from sales_flat_order so inner join core_store cs on so.store_id = cs.store_id where so.increment_id = ?increment_id";
        cm.CommandType = CommandType.Text;
        cm.Parameters.AddWithValue("?increment_id", increment_id);

        return (string)cm.ExecuteScalar();
      }
    }
  }

  public class EDISalesOrder
  {
    public string WebsiteOrderID { get; set; }
    public DateTime Updated { get; set; }
    public string Status { get; set; }
    public string ShopID { get; set; }
    public string State { get; set; }
    public string Payment { get; set; }
    public EDISalesOrderCustomer CustomerBilling { get; set; }
    public EDISalesOrderCustomer CustomerShipping { get; set; }
    public decimal ShippingAmount { get; set; }
    public List<EDISalesOrderLine> OrderLines { get; set; }
    public string OrderLanguageCode { get; set; }

    public bool IsPickupOrder
    {
      get
      {
        return !String.IsNullOrEmpty(ShopID);
      }
    }



    public string CustomerID { get; set; }
  }

  public class EDISalesOrderCustomer
  {
    public string CustomerPO { get; set; }
    public string Addressline1 { get; set; }
    public string ShopID { get; set; }
    public string Addressline2 { get; set; }
    public string Addressline3 { get; set; }
    public string ZIPcode { get; set; }
    public string Number { get; set; }
    public string NumberExtension { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string MailingName { get; set; }
    public string StoreNumber { get; set; }
    public string EmailAddress { get; set; }
    public string ServicePointID { get; set; } //information for Kiala
    public string ServicePointCode { get; set; } //information for Kiala
    public string KialaCompanyName { get; set; } //information for Kiala


    public string CustomerID { get; set; }
  }

  public class EDISalesOrderLine
  {
    public int ConcentratorProductID { get; set; }
    public string ManufacturerID { get; set; }
    public string BrandName { get; set; }
    public decimal Quantity { get; set; }
    public int LineNumber { get; set; }
    public bool InStock { get; set; }
    public decimal LinePrice { get; set; }
    public decimal? DiscountAmount { get; set; }

    public decimal BasePrice { get; set; }

    public List<EDISalesOrderLineDiscountRules> DiscountRules { get; set; }
  }

  public class EDISalesOrderLineDiscountRules
  {
    public int RuleID { get; set; }

    public string RuleCode { get; set; }

    public bool IsSetRule { get; set; }

    public decimal DiscountAmount { get; set; }

    public bool Percentage { get; set; }
  }
}
