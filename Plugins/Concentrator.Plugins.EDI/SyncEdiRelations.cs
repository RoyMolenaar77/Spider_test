using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.EDI;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.EDI
{
  public class SyncEdiRelations : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "EDI Sync Relations Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        using (Concentrator.Web.ServiceClient.EDI.CommunicationServiceSoapClient client = new Concentrator.Web.ServiceClient.EDI.CommunicationServiceSoapClient())
        {
          var ediSettings = client.GetRelationSettings();
          using (TextReader reader = new StringReader(ediSettings))
          {
            XmlSerializer x = new XmlSerializer(typeof(List<RelationCommunicationProperty>));
            List<RelationCommunicationProperty> ediRelationSettings = (List<RelationCommunicationProperty>)x.Deserialize(reader);
            var relationRepository = unit.Scope.Repository<ConnectorRelation>();
            var connectorRelations = relationRepository.GetAll().ToList();

            foreach (var ediRelation in ediRelationSettings)
            {
              var connectorRelation = connectorRelations.FirstOrDefault(y => y.CustomerID == ediRelation.RelationNumber.ToString());

              if (connectorRelation == null)
              {
                connectorRelation = new ConnectorRelation()
                {
                  CustomerID = ediRelation.RelationNumber.ToString(),
                  OrderConfirmation = ediRelation.OrderConfirmation,
                  OutboundOrderConfirmation = ediRelation.OrderConfirmationEmail,
                  ShipmentConfirmation = ediRelation.ShipmentConfirmation,
                  OutboundShipmentConfirmation = ediRelation.ShipmentConfirmationEmail,
                  InvoiceConfirmation = ediRelation.InvoiceConfirmation,
                  OutboundInvoiceConfirmation = ediRelation.InvoiceConfirmationEmail,
                  OutboundTo = ediRelation.OutboundTo,
                  OutboundPassword = ediRelation.OutboundPassword,
                  OutboundUsername = ediRelation.OutboundUsername,
                  ConnectorType = ediRelation.ConnectorType.Try<string, int?>(c => int.Parse(c), null),
                  OutboundMessageType = ediRelation.OutboundMessageType.Try<string, int?>(c => int.Parse(c), null),
                  LanguageID = 1,
                  AccountPrivileges = (int)AccountPrivilegesEnum.XtractS,
                  XtractType = (int)XractTypeEnum.WebshopPriceList
                };

                relationRepository.Add(connectorRelation);
                connectorRelations.Add(connectorRelation);
                unit.Save();
              }
            }

            foreach (var relationCommunication in connectorRelations)
            {
              var ediRelation = ediRelationSettings.FirstOrDefault(y => y.RelationNumber == int.Parse(relationCommunication.CustomerID));

              if (ediRelation != null)
              {
                if (relationCommunication.OrderConfirmation != ediRelation.OrderConfirmation ||
                relationCommunication.OutboundOrderConfirmation != ediRelation.OrderConfirmationEmail ||
                relationCommunication.ShipmentConfirmation != ediRelation.ShipmentConfirmation ||
                relationCommunication.OutboundShipmentConfirmation != ediRelation.ShipmentConfirmationEmail ||
                relationCommunication.InvoiceConfirmation != ediRelation.InvoiceConfirmation ||
                relationCommunication.OutboundInvoiceConfirmation != ediRelation.InvoiceConfirmationEmail ||
                relationCommunication.OutboundTo != ediRelation.OutboundTo ||
                relationCommunication.OutboundPassword != ediRelation.OutboundPassword ||
                relationCommunication.OutboundUsername != ediRelation.OutboundUsername ||
                relationCommunication.ConnectorType.ToString() != ediRelation.ConnectorType ||
                relationCommunication.OutboundMessageType.ToString() != ediRelation.OutboundMessageType)
                {
                  RelationCommunicationProperty property = new RelationCommunicationProperty()
                  {
                    RelationNumber = int.Parse(relationCommunication.CustomerID),
                    OrderConfirmation = relationCommunication.OrderConfirmation,
                    OrderConfirmationEmail = relationCommunication.OutboundOrderConfirmation,
                    ShipmentConfirmation = relationCommunication.ShipmentConfirmation,
                    ShipmentConfirmationEmail = relationCommunication.OutboundShipmentConfirmation,
                    InvoiceConfirmation = relationCommunication.InvoiceConfirmation,
                    InvoiceConfirmationEmail = relationCommunication.OutboundInvoiceConfirmation,
                    OutboundTo = relationCommunication.OutboundTo,
                    OutboundPassword = relationCommunication.OutboundPassword,
                    OutboundUsername = relationCommunication.OutboundUsername,
                    ConnectorType = relationCommunication.ConnectorType.ToString(),
                    OutboundMessageType = relationCommunication.OutboundMessageType.ToString()
                  };

                  client.UpdateRelationSettings(property);
                }
              }
              else if (string.IsNullOrEmpty(relationCommunication.OutboundTo))
              {
                relationRepository.Delete(relationCommunication);
              }
              unit.Save();
            }
          }
        }
      }
    }
  }
}
