using System;
using System.Collections.Generic;

namespace Concentrator.ui.Mailer.Models
{
  public class IngramMailProduct
  {
    public String CustomItemNumber { get; set; }

    public String ProductDescription { get; set; }

    public String NumberOfProducts { get; set; }

    public String Price { get; set; }
  }

  public class IngramMailData
  {
    public String CustomerName { get; set; }
    
    public String Address { get; set; }

    public String Email { get; set; }
    
    public String PhoneNumber { get; set; }

    public List<IngramMailProduct> ProductList { get; set; }
  }
}