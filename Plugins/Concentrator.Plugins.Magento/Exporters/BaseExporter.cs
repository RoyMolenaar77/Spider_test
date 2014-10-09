using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using AuditLog4Net.Adapter;
using Concentrator.Plugins.Magento.Helpers;
using Concentrator.Objects;
using Concentrator.Plugins.Magento.Models;
using Concentrator.Web.Services;

namespace Concentrator.Plugins.Magento.Exporters
{

  public abstract class BaseExporter
  {

    protected string DEFAULT_STOCK_NAME = "Default";
    protected string DEFAULT_GROUP_NAME = "General";

    protected int CUSTOMER_ENTITY_TYPE_ID = 1;
    protected int CUSTOMER_ADDRESS_ENTITY_TYPE_ID = 2;
    protected int CATEGORY_ENTITY_TYPE_ID = 3;
    protected int PRODUCT_ENTITY_TYPE_ID = 4;

    protected MagentoVersion Version { get; private set; }

    protected bool MultiLanguage = false;
    protected ConnectorLanguage PrimaryLanguage;
    protected ConnectorLanguage CurrentLanguage;
    protected bool UsesShortDescAsName { get; private set; }
    protected bool MagentoIgnoreCategoryDisabling { get; private set; }

    protected string CurrentStoreCode
    {
      get { return String.Format(MagentoWebsitePattern, CurrentLanguage.Language.DisplayCode).ToUpper(); }
    }
    protected string MagentoWebsitePattern = String.Empty;
    protected List<ConnectorLanguage> Languages;

    public bool IsPrimaryLanguage
    {
      get { return CurrentLanguage.LanguageID == PrimaryLanguage.LanguageID; }
    }

    public Connector Connector { get; protected set; }
    public IAuditLogAdapter Logger { get; protected set; }
    protected bool RequiresStockUpdate { get; private set; }
    protected bool IgnoreProductStatusUpdates { get; private set; }
    protected bool UsesStoreCodesUniqueOnWebsiteLevel { get; private set; }
    public Dictionary<string, StoreInfo> StoreList;

    public BaseExporter(Connector connector, IAuditLogAdapter logger)
    {
      Connector = connector;
      Logger = logger;
      RequiresStockUpdate = !connector.ParentConnectorID.HasValue;

      var version = connector.ConnectorSettings.GetValueByKey<string>("MagentoVersion", string.Empty);
      UsesShortDescAsName = connector.ConnectorSettings.GetValueByKey<bool>("UsesShortDescAsName", false);
      MagentoIgnoreCategoryDisabling = connector.ConnectorSettings.GetValueByKey<bool>("MagentoIgnoreCategoryDisabling", false); //If true: is_active won't be disabled
      IgnoreProductStatusUpdates = Connector.ConnectorSettings.GetValueByKey<bool>("IgnoreProductStatusUpdates", false);
      UsesStoreCodesUniqueOnWebsiteLevel = Connector.ConnectorSettings.GetValueByKey<bool>("StoreCodesUniqueOnWebsiteLevel", false);

      if (string.IsNullOrEmpty(version))
      {
        Version = MagentoVersion.Version_15;
      }
      else
      {
        try
        {
          Version = (MagentoVersion)Enum.Parse(typeof(MagentoVersion), version);
        }
        catch (Exception e)
        {
          Version = MagentoVersion.Version_15;
        }
      }
    }

    public void Execute()
    {
      Languages = Connector.ConnectorLanguages.ToList();
      PrimaryLanguage = Languages.First();

      MultiLanguage = Languages.Count > 1;

      MagentoWebsitePattern = Connector.ConnectorSettings.GetValueByKey<string>("MagentoWebsitePattern", "{0}");

      using (var helper = new MagentoMySqlHelper(Connector.Connection))
      {
        StoreList = helper.GetStoreList(UsesStoreCodesUniqueOnWebsiteLevel);
      }

      Process();
    }

    protected abstract void Process();

    protected void SyncRequiredCatalogAttributes()
    {

      using (var helper = new AssortmentHelper(Connector.Connection, Version))
      {
        var attributes = helper.GetAttributeList(CATEGORY_ENTITY_TYPE_ID);

        eav_attribute att = null;
        attributes.TryGetValue("icecat_cat_id", out att);
        if (att == null)
          att = helper.CreateAttribute("icecat_cat_id", CATEGORY_ENTITY_TYPE_ID, "icecat_cat_id", "int", is_required: true);
        else
        {
          att.is_required = false;
          att.is_user_defined = false;
        }

        helper.SyncAttribute(att, addToDefaultSet: true);

        attributes = helper.GetAttributeList(PRODUCT_ENTITY_TYPE_ID);
        if (!attributes.TryGetValue("concentrator_product_id", out att))
          att = helper.CreateAttribute("concentrator_product_id", PRODUCT_ENTITY_TYPE_ID, "concentrator_product_id", "int", is_visible: false);
        else
        {
          att.frontend_label = "Concentrator ID";
          att.is_required = false;
          att.is_user_defined = false;
          att.used_in_product_listing = true;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);




        if (!attributes.TryGetValue("barcode", out att))
          att = helper.CreateAttribute("barcode", PRODUCT_ENTITY_TYPE_ID, "barcode", "varchar");
        else
        {
          att.frontend_label = "Barcode";
          att.is_required = false;
          att.is_user_defined = false;
          att.is_searchable = true;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);



        if (!attributes.TryGetValue("stock_status", out att))
          att = helper.CreateAttribute("stock_status", PRODUCT_ENTITY_TYPE_ID, "stock_status", "varchar", is_visible: false);
        else
        {
          att.frontend_label = "Stock Status";
          att.is_required = false;
          att.is_user_defined = false;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);



        if (!attributes.TryGetValue("quantity_to_receive", out att))
          att = helper.CreateAttribute("quantity_to_receive", PRODUCT_ENTITY_TYPE_ID, "quantity_to_receive", "int", is_visible: false);
        else
        {
          att.frontend_label = "Quantity on PO";
          att.is_required = false;
          att.is_user_defined = false;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);



        if (!attributes.TryGetValue("promised_delivery_date", out att))
          att = helper.CreateAttribute("promised_delivery_date", PRODUCT_ENTITY_TYPE_ID, "promised_delivery_date", "date", is_visible: true, used_in_product_listing: true);
        else
        {
          att.frontend_label = "Promised Delivery Date";
          att.frontend_input = "date";
          att.is_required = false;
          att.is_user_defined = false;
          att.is_visible = false;
          att.used_in_product_listing = true;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);


        if (!attributes.TryGetValue("custom_item_number", out att))
          att = helper.CreateAttribute("custom_item_number", PRODUCT_ENTITY_TYPE_ID, "custom_item_number", "varchar", is_visible: true, used_in_product_listing: true, is_searchable: true);
        else
        {
          att.frontend_label = "Custom Item Number";
          att.is_required = false;
          att.is_user_defined = false;
          att.is_visible = false;
          att.used_in_product_listing = false;
          att.is_searchable = true;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);


        if (!attributes.TryGetValue("commercial_status", out att))
          att = helper.CreateAttribute("commercial_status", PRODUCT_ENTITY_TYPE_ID, "commercial_status", "varchar", is_visible: true, used_in_product_listing: true);
        else
        {
          att.frontend_label = "Commercial Status";
          att.is_required = false;
          att.is_user_defined = false;
          att.is_visible = false;
          att.used_in_product_listing = true;
        }
        helper.SyncAttribute(att, addToDefaultSet: true);
      }
    }
  }
}
