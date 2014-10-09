using System;

namespace Concentrator.Plugins.PFA.Objects.Model
{
  public class WehkampStockMutation
  {
    public String Articlenumber { get; set; }

    public String Colorcode { get; set; }

    public String Size { get; set; }

    public Int32 ProductID { get; set; }    

    public Int32 MutationQuantity { get; set; }

    public DateTime MutationDate { get; set; }
  }
}
