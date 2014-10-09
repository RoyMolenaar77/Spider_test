using System.Text;
using FileHelpers;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord(";")]
  public class DatColCustomerInformation
  {
    public string CustomerNumber;
    public string SupplierName;
    public string SupplierAdress;
    public string ExpeditionCity;
    public string ExpeditionPc;
    public string ExpeditionCountry;

    [FieldQuoted]
    public string ErrorLog;

    private const int MaxCustomerNumberLength = 10;
    private const int MaxSupplierNameLength = 35;
    private const int MaxSupplierAdressLength = 35;
    private const int MaxExpeditionCityLength = 35;
    private const int MaxExpeditionPcLength = 11;
    private const int MaxExpeditionCountryLength = 4;

    public bool IsValidCustomer
    {
      get
      {
        var error = new StringBuilder();

        if (string.IsNullOrEmpty(CustomerNumber))
          error.Append("Customer Number cann't be empty! ");
        else if (CustomerNumber.Length > MaxCustomerNumberLength)
            error.Append(string.Format("Customer Number length is bigger then {0}! ", MaxCustomerNumberLength));

        if (string.IsNullOrEmpty(SupplierName))
          error.Append("Supplier Name cann't be empty! ");
        else if (SupplierName.Length > MaxSupplierNameLength || SupplierName.Length < 3)
          error.Append(string.Format("Supplier Name length must be between 3 and {0}! ", MaxSupplierNameLength));

        if (string.IsNullOrEmpty(SupplierAdress))
          error.Append("Supplier Adress cann't be empty! ");
        else if (SupplierAdress.Length > MaxSupplierAdressLength || SupplierAdress.Length < 3)
          error.Append(string.Format("Supplier Adress length must be between 3 and {0}! ", MaxSupplierAdressLength));

        if (string.IsNullOrEmpty(ExpeditionCity))
          error.Append("City cann't be empty! ");
        else if (ExpeditionCity.Length > MaxExpeditionCityLength || ExpeditionCity.Length < 3)
          error.Append(string.Format("City length must be between 3 and {0}! ", MaxExpeditionCityLength));

        // Op verzoek van Bart.
        if (string.IsNullOrEmpty(ExpeditionPc))
          error.Append("Zip cann't be empty! ");
        //else if (ExpeditionPc.Length > MaxExpeditionPcLength || ExpeditionPc.Length < 3)
        //  error.Append(string.Format("Zip length must be between 3 and {0}! ", MaxExpeditionPcLength));

        if (string.IsNullOrEmpty(ExpeditionCountry))
          error.Append("Zip cann't be empty! ");
        //else if (ExpeditionCountry.Length > MaxExpeditionCountryLength || ExpeditionCountry.Length == 1)
        //  error.Append(string.Format("Zip length must be between 1 and {0}! ", MaxExpeditionCountryLength));

        ErrorLog = error.ToString();

        return error.Length <= 0;
      }
    }

    //public string InventLocation { get; set; }
    //public string LanguageID { get; set; }
    //public string RecID { get; set; }
    //public string Kind { get; set; }
    //public string AdressTotal { get; set; }
    //public string Blocked { get; set; }
    //public string ILN { get; set; }
  }
}
