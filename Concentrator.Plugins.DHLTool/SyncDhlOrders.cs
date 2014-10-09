using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.DhlTool.Helpers;

namespace Concentrator.Plugins.DhlTool
{
  public class SyncDhlOrders : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sync DHL Orders from Magento to WMS"; }
    }

    protected override void Process()
    {
      foreach (var connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.DhlTool)))
      {
        using (var helper = new MySqlHelper(connector.Connection))
        {
          var dhlOrders = helper.GetDhlOrders();
          
          //Skip if no orders were retrieved
          if (dhlOrders.Tables[0].Rows.Count != 0)
          {
            var wmsXml = CreateXmlForWms(dhlOrders);

            SaveXmlToDestination(wmsXml);

            var updateDhlOrders = helper.UpdateRetrievedDhlOrders(dhlOrders);

          }
        }
      }
    }

    private static XDocument CreateXmlForWms(DataSet datasetDhlOrders)
    {
      var DhlOrdersXml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

      var dhlOrders = (from order in datasetDhlOrders.Tables[0].AsEnumerable()
                       group order by order.Field<object>("shipment_number").ToString() into grouped
                       let wo = grouped.FirstOrDefault()
                       select new
                       {
                         shipmentNumber = wo.Field<string>("shipment_number"),
                         ggb = wo.Field<string>("ggb"),
                         reason = wo.Field<int>("reason"),
                         quantity = wo.Field<int>("quantity"),
                         type = wo.Field<int>("type"),
                         remark = wo.Field<string>("remark"),
                         sending_shopnumber = wo.Field<string>("sending_companyno"),
                         sending_name = wo.Field<string>("sending_name"),
                         sending_street = wo.Field<string>("sending_street"),
                         sending_number = wo.Field<string>("sending_housenumber"),
                         sending_numberextension = wo.Field<string>("sending_housenumber_extension"),
                         sending_zipcode = wo.Field<string>("sending_postcode"),
                         sending_city = wo.Field<string>("sending_city"),
                         sending_country = wo.Field<string>("sending_countrycode"),
                         receiving_shopnumber = wo.Field<string>("receiving_companyno"),
                         receiving_name = wo.Field<string>("receiving_name"),
                         receiving_street = wo.Field<string>("receiving_street"),
                         receiving_number = wo.Field<string>("receiving_housenumber"),
                         receiving_numberextension = wo.Field<string>("receiving_housenumber_extension"),
                         receiving_zipcode = wo.Field<string>("receiving_postcode"),
                         receiving_city = wo.Field<string>("receiving_city"),
                         receiving_country = wo.Field<string>("receiving_countrycode")
                       }).Distinct().ToList();

      var shipments = new XElement("shipments");

      foreach (var order in dhlOrders)
      {
        var shipment = new XElement("shipment");
        shipment.Add(new XElement("shipmentNumber", order.shipmentNumber));
        shipment.Add(new XElement("ggb", order.ggb));
        shipment.Add(new XElement("reason", order.reason));
        shipment.Add(new XElement("quantity", order.quantity));
        shipment.Add(new XElement("type", order.type));
        shipment.Add(new XElement("remark", order.remark));
        shipment.Add(new XElement("sending",
                        new XElement("shopNumber", order.sending_shopnumber),
                        new XElement("name", order.sending_name),
                        new XElement("street", order.sending_street),
                        new XElement("number", order.sending_number),
                        new XElement("numberExtension", order.sending_numberextension),
                        new XElement("zipcode", order.sending_zipcode),
                        new XElement("city", order.sending_city),
                        new XElement("country", order.sending_country)));
        shipment.Add(new XElement("receiving",
                        new XElement("shopNumber", order.receiving_shopnumber),
                        new XElement("name", order.receiving_name),
                        new XElement("street", order.receiving_street),
                        new XElement("number", order.receiving_number),
                        new XElement("numberExtension", order.receiving_numberextension),
                        new XElement("zipcode", order.receiving_zipcode),
                        new XElement("city", order.receiving_city),
                        new XElement("country", order.receiving_country)));

        shipments.Add(shipment);
      }

      DhlOrdersXml.Add(shipments);

      return DhlOrdersXml;
    }

    private void SaveXmlToDestination(XDocument wmsXml)
    {
      var config = GetConfiguration();
      var dhlOutFolder = new DirectoryInfo(config.AppSettings.Settings["DhlOutFolder"].Value);

      if (!dhlOutFolder.Exists)
        dhlOutFolder.Create();

      wmsXml.Save(Path.Combine(dhlOutFolder.FullName, CreateXmlFileName()));
    }

    private static string CreateXmlFileName()
    {
      return String.Format("DhlShipments_{0:yyyyMMdd}_{0:HHmmssffffff}.xml", DateTime.UtcNow);
    }
  }
}
