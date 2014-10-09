using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;

namespace Concentrator.Tasks.Vlisco.Importers
{
  using Objects.Models.Orders;
  using Objects.Monitoring;

  using CustomerModel = Models.Customer;

  [Task(Constants.Vendor.Vlisco + " Customer Importer Task")]
  public class CustomerImporterTask : MultiMagImporterTask<Models.Customer, Models.CustomerMapping>
  {
    protected override String ImportFilePrefix
    {
      get
      {
        return Constants.Prefixes.Customer;
      }
    }

    protected override void Import(IDictionary<Models.Customer, String> customers)
    {
      var customerRepository = Unit.Scope.Repository<Customer>();
      var existingCustomers = customerRepository
        .GetAll(customer => (customer.ServicePointCode ?? String.Empty).Trim() != String.Empty && (customer.ServicePointID ?? String.Empty).Trim() != String.Empty)
        .Select(customer => customer.ServicePointCode + "-" + customer.ServicePointID)
        .Distinct()
        .ToList();

      foreach (var customerModel in customers.Keys)
      {
        if (!existingCustomers.Contains(customerModel.ShopCode + "-" + customerModel.Client))
        {
          customerRepository.Add(new Customer
          {
            City = customerModel.City,
            CoCNumber = customerModel.CreditCard,
            CustomerAddressLine1 = customerModel.Address1,
            CustomerAddressLine2 = customerModel.Address2,
            CustomerAddressLine3 = customerModel.Address3,
            CustomerEmail = customerModel.Email,
            CustomerName = String.Join(", ", customerModel.Name, customerModel.FirstName),
            CustomerTelephone = customerModel.TelephonePersonal ?? customerModel.TelephoneBusiness,
            PostCode = customerModel.PostCode,
            ServicePointCode = customerModel.ShopCode,
            ServicePointID = customerModel.Client.ToString()
          });
        }
      }

      Unit.Save();
    }
  }
}
