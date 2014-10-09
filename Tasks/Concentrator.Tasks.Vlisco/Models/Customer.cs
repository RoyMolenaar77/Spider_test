using System;

namespace Concentrator.Tasks.Vlisco.Models
{
  public class Customer : IEquatable<Customer>
  {
    public String ShopCode
    {
      get;
      set;
    }

    public Int32 Client
    {
      get;
      set;
    }

    public String Name
    {
      get;
      set;
    }

    public String FirstName
    {
      get;
      set;
    }

    public String Address1
    {
      get;
      set;
    }

    public String Address2
    {
      get;
      set;
    }

    public String Address3
    {
      get;
      set;
    }

    public String Email
    {
      get;
      set;
    }

    public String PostCode
    {
      get;
      set;
    }

    public String City
    {
      get;
      set;
    }

    public String TelephonePersonal
    {
      get;
      set;
    }

    public String TelephoneBusiness
    {
      get;
      set;
    }

    public DateTime BirthDay
    {
      get;
      set;
    }

    public String CreditCard
    {
      get;
      set;
    }

    public DateTime FirstBuy
    {
      get;
      set;
    }

    public DateTime LastBuy
    {
      get;
      set;
    }

    public Decimal TotalAmountSpend
    {
      get;
      set;
    }

    public DateTime CreationTime
    {
      get;
      set;
    }

    public DateTime LastModificationTime
    {
      get;
      set;
    }

    public Boolean Equals(Customer other)
    {
      return other != null && ShopCode == other.ShopCode && Client == other.Client;
    }

    public override Int32 GetHashCode()
    {
      return ShopCode.GetHashCode() ^ Client;
    }
  }
}
