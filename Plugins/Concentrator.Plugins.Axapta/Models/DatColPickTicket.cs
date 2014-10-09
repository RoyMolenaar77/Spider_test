using FileHelpers;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord("\t")]
  public class DatColPickTicket
  {
    public string RunNumber;
    public string AxID;
    public string RowNumber;
    public string PickTicketNumber;

    public string ModelCode;
    public string CustomItemNumber;
    //[FieldQuoted]
    public string Barcode;
    public string Size;
    public string Subsize;
    public string ModelColor;
    public string Quantity;
    public string QuantityOnPickTicket;
    public string PickedItems;
    public string CustomerNumber;

    public string CustomerName;
    public string CustomerAddress;
    public string CustomerPostcode;
    public string CustomerCountry;

    public string DeliveryDate;
    public string MaxDeliveryDate;

    public string OrderNumber;

    public string Warehouse;

    public string CombineCustomItemNumber
    {
      get
      {
        return string.Format("{0} {1} {2}{3}", ModelCode, ModelColor, Size,
                      string.IsNullOrEmpty(Subsize) ? string.Empty : " " + Subsize);
      }
    }
  }

  [DelimitedRecord("\t")]
  public class DatColOrderTransfer
  {
    public string AxaptaCustomerNumber;
    public string FromWarehouse;
    public string ToWarehouse;
    public string DeliveryDate;
    public string PickedItems;
  }
}
