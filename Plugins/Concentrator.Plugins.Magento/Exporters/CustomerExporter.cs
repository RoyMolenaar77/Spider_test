using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using AuditLog4Net.Adapter;
using Concentrator.Plugins.Magento.Helpers;
using System.Configuration;
using Concentrator.Objects;
using Concentrator.Plugins.Magento.Models;
using Concentrator.Web.ServiceClient.JdeCustomerService;
using System.Xml.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.IO;

namespace Concentrator.Plugins.Magento.Exporters
{
  public class CustomerExporter : BaseExporter
  {

    public Configuration Configuration { get; private set; }


    public CustomerExporter(Connector connector, IAuditLogAdapter logger, Configuration configuration)
      : base(connector, logger)
    {
      Configuration = configuration;
    }


    private static string CustomerCacheFile = @"d:\magento\customers-{0}.xml";
    private XDocument CustomerXml;

    public class CustomerXmlRecord
    {
      public XElement Node { get; set; }
      public CustomerXmlRecord Parent { get; set; }
      public int BackendRelationID { get; set; }
      public string AddressType { get; set; }
      public IEnumerable<CustomerXmlRecord> Children { get; set; }
    }



    private static CustomerXmlRecord GetCustomerTreeNode(XElement parent, XElement node, IEnumerable<XElement> customers)
    {
      var curNode = new CustomerXmlRecord
      {
        Node = node,
        BackendRelationID = Convert.ToInt32(node.Attribute("BackendRelationID").Value),
        AddressType = node.Element("AddressInformation").Element("AddressType").Value
      };

      curNode.Children = (from child in customers
                          where child.Attribute("ParentBackendRelationID").Value == node.Attribute("BackendRelationID").Value
                          select GetCustomerTreeNode(node, child, customers)
                    ).ToList();

      curNode.Parent = (from p in customers
                        where p.Attribute("BackendRelationID").Value == node.Attribute("ParentBackendRelationID").Value
                        select new CustomerXmlRecord
                        {
                          Node = p,
                          BackendRelationID = Convert.ToInt32(p.Attribute("BackendRelationID").Value),
                          AddressType = p.Element("AddressInformation").Element("AddressType").Value
                        }

                        ).FirstOrDefault();


      return curNode;
    }

    protected override void Process()
    {


      Logger.InfoFormat("Synchronizing Customer Information");

      CurrentLanguage = PrimaryLanguage;

      string BusinessUnits = string.Empty;

      BusinessUnits = Connector.ConnectorSettings.GetValueByKey<string>("BusinessUnits", string.Empty);

      if (string.IsNullOrEmpty(BusinessUnits))
      {
        Logger.DebugFormat("No business units set for this connector");
        return;
      }


      SyncRequiredCustomerAttributes();



      var units = BusinessUnits.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


      foreach (var businessUnit in units)
      {

        #if CACHE
        if (!File.Exists(String.Format(CustomerCacheFile, businessUnit)))
        {
          JDECustomerSoapClient client = new Web.ServiceClient.JdeCustomerService.JDECustomerSoapClient();

          CustomerXml = XDocument.Parse(client.GetCustomerData(businessUnit));
          CustomerXml.Save(String.Format(CustomerCacheFile, businessUnit));
        }
        else
        {
          CustomerXml = XDocument.Load(String.Format(CustomerCacheFile, businessUnit));
        }
#else
        JDECustomerSoapClient client = new Web.ServiceClient.JdeCustomerService.JDECustomerSoapClient();

        CustomerXml = XDocument.Parse(client.GetCustomerData(businessUnit));
#endif

        List<string> emailAddressCodes = new List<string>() { "E", "A", "F", "PL" };

        // (from c in CustomerXml.Root.Elements("Customer")
        //               select c).Take(100);


        var customers = (from c in CustomerXml.Root.Elements("Customer")
                         let bid = Convert.ToInt32(c.Attribute("BackendRelationID").Value)
                         //where (bid == 14833900)
                         where c.Attribute("ParentBackendRelationID").Value == "0" && c.Element("AddressInformation") != null
                         select GetCustomerTreeNode(null, c, CustomerXml.Root.Elements("Customer"))
                         ).ToList();




        //Setup Lookup Lists
        SortedDictionary<string, eav_attribute> attributeList = new SortedDictionary<string, eav_attribute>();
        SortedDictionary<string, eav_attribute> addressAttributeList = new SortedDictionary<string, eav_attribute>();
        Dictionary<int, customer_entity> existingCustomers = new Dictionary<int, customer_entity>();
        HashSet<string> existingEmailAddresses = new HashSet<string>();


        using (var helper = new CustomerHelper(Connector.Connection))
        {
          attributeList = helper.GetAttributeList(CUSTOMER_ENTITY_TYPE_ID);
          addressAttributeList = helper.GetAttributeList(CUSTOMER_ADDRESS_ENTITY_TYPE_ID);
          existingCustomers = helper.GetCustomers();
          existingEmailAddresses = helper.GetCustomerEmailAddresses();
        }


        int totalRecords = customers.Count;
        int totalProcessed = 0;

        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 16 };

        Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
        {


          using (var helper = new CustomerHelper(Connector.Connection))
          {

            for (int index = range.Item1; index < range.Item2; index++)
            {
              var record = customers[index];
              ProcessCustomerNode(emailAddressCodes, attributeList, addressAttributeList, existingCustomers, existingEmailAddresses, helper, record);

              Interlocked.Increment(ref totalProcessed);
              if (totalProcessed % 100 == 0)
                Logger.DebugFormat(String.Format("Processed {0} of {1} products", totalProcessed, totalRecords));

            }
          }
        });

      }


    }

    private void SyncRequiredCustomerAttributes()
    {

      using (var helper = new CustomerHelper(Connector.Connection))
      {
        var attributes = helper.GetAttributeList(CUSTOMER_ENTITY_TYPE_ID);
        var addressAttributes = helper.GetAttributeList(CUSTOMER_ADDRESS_ENTITY_TYPE_ID);

        #region Basic Customer data
        eav_attribute att = null;

        attributes.TryGetValue("account_number", out att);
        if (att == null)
          att = helper.CreateAttribute("account_number", CUSTOMER_ENTITY_TYPE_ID, "account_number", "int", is_required: true);
        att.is_required = true;
        att.is_user_defined = true;

        att.frontend_label = "Account Number";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("parent_customer_id", out att);
        if (att == null)
          att = helper.CreateAttribute("parent_customer_id", CUSTOMER_ENTITY_TYPE_ID, "parent_customer_id", "int", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Parent Customer ID";




        attributes.TryGetValue("company_name", out att);
        if (att == null)
          att = helper.CreateAttribute("company_name", CUSTOMER_ENTITY_TYPE_ID, "company_name", "varchar", is_required: true);
        att.is_required = true;
        att.is_user_defined = true;

        att.frontend_label = "Company Name";

        helper.SyncAttribute(att, addToDefaultSet: true);



        attributes.TryGetValue("store_name", out att);
        if (att == null)
          att = helper.CreateAttribute("store_name", CUSTOMER_ENTITY_TYPE_ID, "store_name", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Store Name";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("website", out att);
        if (att == null)
          att = helper.CreateAttribute("website", CUSTOMER_ENTITY_TYPE_ID, "website", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Website";

        helper.SyncAttribute(att, addToDefaultSet: true);



        attributes.TryGetValue("segment", out att);
        if (att == null)
          att = helper.CreateAttribute("segment", CUSTOMER_ENTITY_TYPE_ID, "segment", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Segment";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("purchase_group", out att);
        if (att == null)
          att = helper.CreateAttribute("purchase_group", CUSTOMER_ENTITY_TYPE_ID, "purchase_group", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Purchase Group";

        helper.SyncAttribute(att, addToDefaultSet: true);

        attributes.TryGetValue("electronic_invoice", out att);
        if (att == null)
          att = helper.CreateAttribute("electronic_invoice", CUSTOMER_ENTITY_TYPE_ID, "electronic_invoice", "int", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;
        att.frontend_input = "boolean";

        att.frontend_label = "Electronic Invoice";

        helper.SyncAttribute(att, addToDefaultSet: true);

        #endregion


        attributes.TryGetValue("new_account_confirmed", out att);
        if (att == null)
          att = helper.CreateAttribute("new_account_confirmed", CUSTOMER_ENTITY_TYPE_ID, "new_account_confirmed", "int", is_required: true);
        att.is_required = true;
        att.is_user_defined = true;
        att.frontend_input = "boolean";
        att.frontend_label = "New Account Confirmed";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("pending_change", out att);
        if (att == null)
          att = helper.CreateAttribute("pending_change", CUSTOMER_ENTITY_TYPE_ID, "pending_change", "int", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;
        att.frontend_input = "boolean";
        att.frontend_label = "Pending Change";


        attributes.TryGetValue("created_in", out att);
        if (att == null)
          att = helper.CreateAttribute("created_in", CUSTOMER_ENTITY_TYPE_ID, "created_in", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Source System";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("account_manager_name", out att);
        if (att == null)
          att = helper.CreateAttribute("account_manager_name", CUSTOMER_ENTITY_TYPE_ID, "account_manager_name", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Account Manager";

        helper.SyncAttribute(att, addToDefaultSet: true);

        attributes.TryGetValue("invoice_amount", out att);
        if (att == null)
          att = helper.CreateAttribute("invoice_amount", CUSTOMER_ENTITY_TYPE_ID, "invoice_amount", "decimal", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;
        att.backend_type = "decimal";
        att.frontend_label = "Invoice Amount";

        helper.SyncAttribute(att, addToDefaultSet: true);

        attributes.TryGetValue("invoice_amount_open", out att);
        if (att == null)
          att = helper.CreateAttribute("invoice_amount_open", CUSTOMER_ENTITY_TYPE_ID, "invoice_amount_open", "decimal", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;
        att.backend_type = "decimal";
        att.frontend_label = "Open Invoice Amount";

        helper.SyncAttribute(att, addToDefaultSet: true);




        attributes.TryGetValue("default_carrier", out att);
        if (att == null)
          att = helper.CreateAttribute("default_carrier", CUSTOMER_ENTITY_TYPE_ID, "default_carrier", "int", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Default Carrier ID";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("default_carrier_name", out att);
        if (att == null)
          att = helper.CreateAttribute("default_carrier_name", CUSTOMER_ENTITY_TYPE_ID, "default_carrier_name", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Default Carrier";

        helper.SyncAttribute(att, addToDefaultSet: true);

        attributes.TryGetValue("route_code", out att);
        if (att == null)
          att = helper.CreateAttribute("route_code", CUSTOMER_ENTITY_TYPE_ID, "route_code", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Route Code";

        helper.SyncAttribute(att, addToDefaultSet: true);

        attributes.TryGetValue("credit_limit", out att);
        if (att == null)
          att = helper.CreateAttribute("credit_limit", CUSTOMER_ENTITY_TYPE_ID, "credit_limit", "decimal", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;
        att.backend_type = "decimal";
        att.frontend_label = "Credit Limit";

        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("payment_days", out att);
        if (att == null)
          att = helper.CreateAttribute("payment_days", CUSTOMER_ENTITY_TYPE_ID, "payment_days", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;
        att.frontend_label = "Payment Days";
        helper.SyncAttribute(att, addToDefaultSet: true);


        attributes.TryGetValue("payment_instrument", out att);
        if (att == null)
          att = helper.CreateAttribute("payment_instrument", CUSTOMER_ENTITY_TYPE_ID, "payment_instrument", "varchar", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Payment Instrument";


        helper.SyncAttribute(att, addToDefaultSet: true);



        addressAttributes.TryGetValue("address_line_id", out att);
        if (att == null)
          att = helper.CreateAttribute("address_line_id", CUSTOMER_ADDRESS_ENTITY_TYPE_ID, "address_line_id", "int", is_required: false);
        att.is_required = false;
        att.is_user_defined = true;

        att.frontend_label = "Address Line ID";


        helper.SyncAttribute(att, addToDefaultSet: true);
      }


    }


    #region Helpers

    private void ProcessCustomerNode(List<string> emailAddressCodes, SortedDictionary<string, eav_attribute> attributeList, SortedDictionary<string, eav_attribute> addressAttributeList,
      Dictionary<int, customer_entity> existingCustomers, HashSet<string> existingEmailAddresses, CustomerHelper helper, CustomerXmlRecord record)
    {


      var customer = record.Node;

      int backendRelationId = Convert.ToInt32(customer.Attribute("BackendRelationID").Value);
      int parentBackendRelationId = 0;
      if (record.Parent != null)
        parentBackendRelationId = Convert.ToInt32(record.Parent.Node.Attribute("BackendRelationID").Value);

      customer_entity entity = null;

      bool prefixEmailRecords = false;


      var electronicAddressesNode = customer.Element("Addresses").Elements("Address").Where(x => x.Element("ElectronicAddressType") != null
        && emailAddressCodes.Contains(x.Element("ElectronicAddressType").Value)
        );

      //filter bas addresses
      electronicAddressesNode = electronicAddressesNode.Where(x =>
        !x.Element("ElectronicAddress").Value.Contains("basdistributie.")
        && !x.Element("ElectronicAddress").Value.Contains("basdistribution.")
        && !x.Element("ElectronicAddress").Value.Contains("basgroup."));





      if (electronicAddressesNode.Count() == 0)
      {

        if (parentBackendRelationId > 0)
        {
          var parent = GetBillToAddress(record);
          electronicAddressesNode = parent.Element("Addresses").Elements("Address").Where(x => x.Element("ElectronicAddressType") != null
        && emailAddressCodes.Contains(x.Element("ElectronicAddressType").Value));

          //filter bas addresses
          electronicAddressesNode = electronicAddressesNode.Where(x =>
            !x.Element("ElectronicAddress").Value.Contains("basdistributie.")
            && !x.Element("ElectronicAddress").Value.Contains("basdistribution.")
            && !x.Element("ElectronicAddress").Value.Contains("basgroup."));

          prefixEmailRecords = true;

        }

        if (electronicAddressesNode.Count() == 0)
        {

          Logger.WarnFormat("Ignoring customer {0}, because no email address exists", backendRelationId);
          return;
        }

      }


      string mainEmailAddress = electronicAddressesNode
        .OrderBy(e => emailAddressCodes.IndexOf(e.Element("ElectronicAddressType").Value)).FirstOrDefault().Try(x => x.Element("ElectronicAddress").Value, null);


      if (String.IsNullOrEmpty(mainEmailAddress))
      {
        if (parentBackendRelationId == 0)
        {
          Logger.WarnFormat("Ignoring customer {0}, because primary email address is empty", backendRelationId);
          return;
        }
        else
        {
          // get parents address
        }
      }

      string financialEmailAddress = electronicAddressesNode.FirstOrDefault(x => x.Element("ElectronicAddressType").Value == "F").Try(x => x.Element("ElectronicAddress").Value, null);

      if (prefixEmailRecords)
      {
        mainEmailAddress = String.Format("{0}_{1}", backendRelationId, mainEmailAddress);
        financialEmailAddress = String.Format("{0}_{1}", backendRelationId, financialEmailAddress);

      }


      if (!existingCustomers.TryGetValue(backendRelationId, out entity))
      {

        if (existingEmailAddresses.Contains(mainEmailAddress))
        {
          Logger.WarnFormat("Ignoring customer {0}, because email address {1} is already exists", backendRelationId, mainEmailAddress);
          return;
        }


        //create new
        entity = new customer_entity()
        {
          entity_id = backendRelationId,
          entity_type_id = CUSTOMER_ENTITY_TYPE_ID,
          attribute_set_id = 0,
          website_id = 1,
          group_id = 1,
          store_id = 1,
          created_at = DateTime.Now,
          updated_at = DateTime.Now,
          is_active = true,
          increment_id = "",
          email = mainEmailAddress
        };

        helper.AddCustomer(entity);

      }
      else
      {
        if (mainEmailAddress != entity.email)
        {
          Logger.WarnFormat("Cannot change email (for now), relation : {0}", backendRelationId);
          return;
        }
      }


      #region Password

      string password = HttpUtility.HtmlDecode(customer.Element("Password").Value.Trim());
      var hasher = System.Security.Cryptography.MD5.Create();

      var bytes = hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(password));

      StringBuilder sBuilder = new StringBuilder();

      // Loop through each byte of the hashed data 
      // and format each one as a hexadecimal string.
      for (int i = 0; i < bytes.Length; i++)
      {
        sBuilder.Append(bytes[i].ToString("x2"));
      }

      // Return the hexadecimal string.
      helper.SyncAttributeValue(attributeList["password_hash"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, sBuilder.ToString());
      #endregion

      #region Entity Attributes


      helper.SyncAttributeValue(attributeList["company_name"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, customer.Element("Name").Value);

      helper.SyncAttributeValue(attributeList["firstname"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, "-");
      helper.SyncAttributeValue(attributeList["lastname"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, "-");

      helper.SyncAttributeValue(attributeList["account_number"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, backendRelationId, type: "int");

      helper.SyncAttributeValue(attributeList["created_in"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, "JD Edwards");


      string taxNumber = customer.Element("TaxNumber").Try(x => x.Value.Trim(), string.Empty);

      helper.SyncAttributeValue(attributeList["taxvat"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, taxNumber, type: "varchar");

      var accountManagerNode = customer.Element("AccountManager");

      string accountManagerName = String.Empty;

      if (accountManagerNode != null)
        accountManagerName = accountManagerNode.Element("Name").Value;


      helper.SyncAttributeValue(attributeList["account_manager_name"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, accountManagerName);

      #region Financial Attributes
      string paymentInstrument = customer.Element("PaymentInstrument").Try(x => x.Value, string.Empty);
      string paymentDays = customer.Element("PaymentDays").Try(x => x.Value, string.Empty);
      string routeCode = customer.Element("RouteCode").Try(x => x.Value, string.Empty);
      decimal invoiceAmount = customer.Element("InvoiceAmount").Try(x => Convert.ToDecimal(x.Value), 0);
      decimal invoiceAmountOpen = customer.Element("OpenInvoiceAmount").Try(x => Convert.ToDecimal(x.Value), 0);
      decimal creditLimit = customer.Element("CreditLimit").Try(x => Convert.ToDecimal(x.Value), 0);

      helper.SyncAttributeValue(attributeList["payment_instrument"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, paymentInstrument, type: "varchar");
      helper.SyncAttributeValue(attributeList["payment_days"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, paymentDays, type: "varchar");

      helper.SyncAttributeValue(attributeList["credit_limit"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, creditLimit, type: "decimal");
      helper.SyncAttributeValue(attributeList["invoice_amount"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, invoiceAmount, type: "decimal");
      helper.SyncAttributeValue(attributeList["invoice_amount_open"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, invoiceAmountOpen, type: "decimal");

      helper.SyncAttributeValue(attributeList["route_code"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, routeCode, type: "varchar");

      #endregion

      #region Logistic Attributes

      int defaultCarrierId = customer.Element("DefaultCarrier").Try(x => Convert.ToInt32(x.Value), 0);
      string defaultCarrierName = customer.Element("DefaultCarrierName").Try(x => x.Value, string.Empty);

      helper.SyncAttributeValue(attributeList["default_carrier"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, defaultCarrierId, type: "int");
      helper.SyncAttributeValue(attributeList["default_carrier_name"].attribute_id, CUSTOMER_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, backendRelationId, defaultCarrierName, type: "varchar");

      #endregion


      #endregion

      #region Addresses

      customer_address_entity addressEntity = SyncEntityAddress(addressAttributeList, helper, customer, entity, entity.entity_id);

      if (record.Children.Count() == 0)  // no children, so default shipping remains here
        helper.SyncAttributeValue(attributeList["default_shipping"].attribute_id, entity.entity_type_id, StoreList[CurrentStoreCode].store_id, entity.entity_id, addressEntity.entity_id, "int");

      if (record.Parent == null)
      {
        //top level
        helper.SyncAttributeValue(attributeList["default_billing"].attribute_id, entity.entity_type_id, StoreList[CurrentStoreCode].store_id, entity.entity_id, addressEntity.entity_id, "int");
      }
      else
      {
        var billToAddress = GetBillToAddress(record);
        customer_address_entity billTo = SyncEntityAddress(addressAttributeList, helper, billToAddress,
                  entity, address_line_id: Convert.ToInt32(billToAddress.Attribute("BackendRelationID").Value));

        helper.SyncAttributeValue(attributeList["default_billing"].attribute_id, entity.entity_type_id, StoreList[CurrentStoreCode].store_id, entity.entity_id, billTo.entity_id, "int");
      }

      if (record.Children.Count() > 0)
      {
        var shipToAddresses = GetChildAddresses(record.Children);

        foreach (var address in shipToAddresses)
        {
          customer_address_entity childAddress = SyncEntityAddress(addressAttributeList, helper, address, entity, address_line_id: Convert.ToInt32(address.Attribute("BackendRelationID").Value));
        }
      }

      #endregion


    }

    private XElement GetBillToAddress(CustomerXmlRecord record)
    {
      var parent = record.Parent;
      while (parent != null)
      {
        if (parent.Parent == null)
          break;

        parent = parent.Parent;

      }


      return parent.Node;


    }

    private IEnumerable<XElement> GetChildAddresses(IEnumerable<CustomerXmlRecord> collection)
    {
      var result = collection.Select(x => x.Node);

      var childs = (from c in collection
                    where c.Children.Count() > 0
                    select GetChildAddresses(c.Children)).SelectMany(x => x);

      return result.Union(childs);


    }

    private customer_address_entity SyncEntityAddress(SortedDictionary<string, eav_attribute> addressAttributeList, CustomerHelper helper, XElement customer, customer_entity entity, int address_line_id)
    {

      var addressNode = customer.Element("AddressInformation");

      var phonesNode = customer.Element("TelephoneNumbers").Elements("TelephoneNumber");

      string mainPhone = String.Empty;

      if (phonesNode.Count() > 0)
        mainPhone = phonesNode.FirstOrDefault(x => x.Element("TelephoneType").Value == "TEL").Try(x => x.Element("AreaCode").Value + " " + x.Element("Number").Value, string.Empty);
      else
        mainPhone = "0123456789";

      string addressLines = addressNode.Element("AddressLine1").Value + "\n" + addressNode.Element("AddressLine2").Value;

      string companyName = customer.Element("Name").Value;

      customer_address_entity addressEntity = helper.GetCustomerAddress(entity.entity_id, address_line_id);
      if (addressEntity == null)
      {
        addressEntity = new customer_address_entity()
        {
          parent_id = entity.entity_id,
          entity_type_id = CUSTOMER_ADDRESS_ENTITY_TYPE_ID,
          is_active = true,
          attribute_set_id = 0,
          increment_id = "0",
          created_at = DateTime.Now,
          updated_at = DateTime.Now,
        };
      }

      helper.SyncAddress(addressEntity);

      helper.SyncAttributeValue(addressAttributeList["address_line_id"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, address_line_id, "int");

      helper.SyncAttributeValue(addressAttributeList["company"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, companyName, "varchar");

      helper.SyncAttributeValue(addressAttributeList["firstname"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, "-", "varchar");
      helper.SyncAttributeValue(addressAttributeList["lastname"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, "-", "varchar");

      helper.SyncAttributeValue(addressAttributeList["street"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, addressLines, "text");
      helper.SyncAttributeValue(addressAttributeList["postcode"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, addressNode.Element("ZipCode").Value, "varchar");
      helper.SyncAttributeValue(addressAttributeList["city"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, addressNode.Element("City").Value, "varchar");
      helper.SyncAttributeValue(addressAttributeList["country_id"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, addressNode.Element("Country").Value, "varchar");

      helper.SyncAttributeValue(addressAttributeList["telephone"].attribute_id, CUSTOMER_ADDRESS_ENTITY_TYPE_ID, StoreList[CurrentStoreCode].store_id, addressEntity.entity_id, mainPhone, "varchar");


      return addressEntity;
    }

    #endregion
  }
}
