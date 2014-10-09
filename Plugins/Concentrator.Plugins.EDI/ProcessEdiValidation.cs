using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Concentrator.Objects.EDI;
using System.Configuration;
using System.Data;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.EDI.Validation;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.EDI
{
  public class ProcessEdiValidation : ConcentratorEDIPlugin
  {
    public override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

    public override string Name
    {
      get { return "EDI validation process"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var orders = (from o in unit.Scope.Repository<EdiOrder>().GetAll()
                      where o.Status == (int)EdiOrderStatus.Validate
                      select o).ToList();

        int edivendorID = ediVendor.EdiVendorID;
        var ediValidations = unit.Scope.Repository<EdiValidate>().GetAll(v => v.EdiVendorID == edivendorID).ToList();
                             

        foreach (var order in orders)
        {
          try
          {
            List<string> errorMessages = new List<string>();
            var orderResult = ValidateOrder<EdiOrder>(order, ediValidations, order.EdiOrderID);

            foreach (var line in orderResult.Keys)
            {
              string error = string.Format("Validation failed for order {0}: {1}", line, orderResult[line]);
              log.AuditError(error, "Validation EDI orders");
              errorMessages.Add(error);
            }

            foreach (var orderLine in order.EdiOrderLines.Where(x => !x.EdiOrderLedgers.Any(y => y.Status == (int)EdiOrderStatus.Validate)))
            {
              var orderLineResult = ValidateOrder<EdiOrderLine>(orderLine, ediValidations, orderLine.EdiOrderLineID);

              foreach (var line in orderLineResult.Keys)
              {
                string error = string.Format("Validation failed for orderline {0}: {1}", line, orderLineResult[line]);
                log.AuditError(error, "Validation EDI orders");
                errorMessages.Add(error);
              }

              orderLine.SetStatus(EdiOrderStatus.Validate, unit);           
            }

            if (order.ShipToCustomerID.HasValue)
            {
              var shipToCustomerResult = ValidateOrder<Customer>(order.ShippedToCustomer, ediValidations, order.ShipToCustomerID.Value);

              foreach (var line in shipToCustomerResult.Keys)
              {
                string error = string.Format("Validation failed for customer {0}: {1}", line, shipToCustomerResult[line]);
                log.AuditError(error, "Validation EDI orders");
                errorMessages.Add(error);
              }
            }

            if (order.SoldToCustomerID.HasValue)
            {
              var soldToCustomerResult = ValidateOrder<Customer>(order.SoldToCustomer, ediValidations, order.SoldToCustomerID.Value);

              foreach (var line in soldToCustomerResult.Keys)
              {
                string error = string.Format("Validation failed for customer {0}: {1}", line, soldToCustomerResult[line]);
                log.AuditError(error, "Validation EDI orders");
                errorMessages.Add(error);
              }
            }

            ConnectorRelation ediRelation = null;
            if(order.ConnectorRelationID.HasValue)
              ediRelation = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == order.ConnectorRelationID.Value);
            else
              ediRelation = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.CustomerID == order.ShippedToCustomer.EANIdentifier);

            if (ediRelation == null)
            {
              throw new Exception(string.Format("No Edi Customer availible for {0}", order.ShippedToCustomer.EANIdentifier));
            }
            order.ConnectorRelationID = ediRelation.ConnectorRelationID;

            var response = ediProcessor.ValidateOrder(order, ediVendor, ediRelation, Config,unit);
            unit.Save();

            if (response.ResponseErrors != null)
              errorMessages.AddRange(response.ResponseErrors.Select(x => x.ErrorMessage));

            bool skipOrder = false;
            foreach (var rLine in response.EdiOrderResponseLines.Where(x => x.ResponseErrors.Count > 0))
            {
              errorMessages.AddRange(rLine.ResponseErrors.Select(x => x.ErrorMessage + "," + x.ErrorCode));
              rLine.ResponseErrors.ForEach(error =>
              {
                if (error.SkipOrder)
                  skipOrder = error.SkipOrder;
              });
            }

            if (errorMessages.Count > 0)
            {
              var listener = unit.Scope.Repository<EdiOrderListener>().GetSingle(x => x.EdiRequestID == order.EdiRequestID);
              listener.ErrorMessage = string.Join(";", errorMessages);
              if(skipOrder)
                order.Status = (int)EdiOrderStatus.Error;
              else
                order.Status = (int)EdiOrderStatus.ProcessOrder;
            }
            else
              order.Status = (int)EdiOrderStatus.ProcessOrder;

            unit.Save();
          }
          catch (Exception ex)
          {
            order.EdiOrderListener.ErrorMessage = string.Format("Validation failed: {0}", ex.Message);
            log.AuditError(string.Format("Validation failed for order {0}", order.EdiOrderID), ex, "Validation EDI orders");
          }
        }
      }
    }


    public Dictionary<int, string> ValidateOrder<T>(T obj, List<EdiValidate> validations, int entityID)
  where T : class
    {
      Type type = (typeof(T));

      Dictionary<int, string> errormessages = new Dictionary<int, string>();

      foreach (var validation in validations.Where(x => x.TableName == type.Name))
      {
        var value = obj.GetType().GetProperty(validation.FieldName).GetValue(obj, null).ToString().Trim();

        if (validation.MaxLength.HasValue && value.Length > validation.MaxLength.Value)
          errormessages.Add(entityID, string.Format("Max length for field {0} exceeded", validation.FieldName));

        if (!string.IsNullOrEmpty(validation.Value))
        {
          switch ((EdiValidationType)validation.EdiValidationType)
          {
            case EdiValidationType.ConstantValue:
              if (validation.Value != value)
                errormessages.Add(entityID, string.Format("Value for field {0} should be {1}", validation.FieldName, validation.Value));
              break;
            case EdiValidationType.Query:
              string connection = validation.Connection;

              if (ConfigurationManager.ConnectionStrings[validation.Connection] != null)
                connection = ConfigurationManager.ConnectionStrings[validation.Connection].ConnectionString;

              EDICommunicationLayer communication = new EDICommunicationLayer((EdiConnectionType)validation.EdiConnectionType, connection);
              if (communication.IsValid(string.Format(validation.Value, value)))
                errormessages.Add(entityID, string.Format("Value for field {0} not valid query returned false", validation.FieldName, validation.Value));
              break;
            case EdiValidationType.Formula:
              //var calculateTotal = DynamicExpression.ParseLambda<T, string>("SubTotal*@0+Shipping", validation.Value).Compile();
              if (!f(validation.Value, value))
                errormessages.Add(entityID, string.Format("Value for field {0} not valid formula returned false", validation.FieldName, validation.Value));
              break;
            default:
              break;
          }
        }
      }

      return errormessages;
    }

    public bool f(string expression, string x)
    {
      ParameterExpression xExpr = Expression.Parameter(typeof(string), "x");

      LambdaExpression e = System.Linq.Dynamic.DynamicExpression.ParseLambda(
          new ParameterExpression[] { xExpr }, typeof(bool), expression);

      return (bool)e.Compile().DynamicInvoke(x);
    }
  }
}
