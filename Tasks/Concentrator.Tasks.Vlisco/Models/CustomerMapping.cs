using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Models
{
  public sealed class CustomerMapping : CsvClassMap<Customer>
  {
    public CustomerMapping()
    {
      Map(customer => customer.ShopCode).Index(0);
      Map(customer => customer.Client).Index(1);
      Map(customer => customer.Name).Index(2);
      Map(customer => customer.FirstName).Index(3);
      Map(customer => customer.Address1).Index(4);
      Map(customer => customer.Address2).Index(5);
      Map(customer => customer.Address3).Index(6);
      Map(customer => customer.PostCode).Index(7);
      Map(customer => customer.City).Index(8);
      Map(customer => customer.TelephonePersonal).Index(9);
      Map(customer => customer.TelephoneBusiness).Index(10);
      Map(customer => customer.BirthDay).Index(11).TypeConverterOption(Constants.Culture.English);
      Map(customer => customer.CreditCard).Index(12);
      Map(customer => customer.FirstBuy).Index(13).TypeConverterOption(Constants.Culture.English);
      Map(customer => customer.LastBuy).Index(14).TypeConverterOption(Constants.Culture.English);
      Map(customer => customer.TotalAmountSpend).Index(15).TypeConverterOption(Constants.Culture.English);
      Map(customer => customer.CreationTime).Index(16).TypeConverterOption(Constants.Culture.English);
      Map(customer => customer.LastModificationTime).Index(17).TypeConverterOption(Constants.Culture.English);
      Map(customer => customer.Email).Index(18);
    }
  }
}
