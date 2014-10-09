using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Plugins.PFA.Repos;
using Moq;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Helpers;


namespace Concentrator.Plugins.PFA.Tests
{
  [TestClass]
  public class PriceTests
  {
    [TestMethod]
    public void One_Price_Rule_Should_Return_The_Same_Price()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 10         
        }
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is not as it is expected");
    }

    [TestMethod]
    public void Product_Should_Always_Have_A_Price()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 10,
          discount_code = "afp"
        }
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 0, "Price from PFA is not as it is expected");
    }

    [TestMethod]
    public void Price_Rule_Should_Not_Return_Discount_As_Price()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,6),
          price = 10          
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 8,
          discount_code = "afp"
        }
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is not as it is expected");
    }

    [TestMethod]
    public void Price_Rules_With_Color_Should_Have_Priority_Over_Product_Rules()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 10         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 5,
          color_code = colorCode
        },
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Price from PFA is not on color level");

    }

    [TestMethod]
    public void Price_Rules_With_Color_And_Different_Start_Dates_Should_Always_Take_The_Newest_Rule()
    {

      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,8),
          price = 10         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 5,
          color_code = colorCode
        },
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is on color level but that is not the newest rule");
    }

    [TestMethod]
    public void Colors_With_Different_Prices_Should_Be_Correct()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          color_code = colorCode,
          price = 10         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 5,
          color_code = "5"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 7          
        },
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is not checked for an end date");
      Assert.AreEqual(help.GetPrice(productNumber, "5", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Price from PFA is not checked for an end date");
      Assert.AreEqual(help.GetPrice(productNumber, "12", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 7, "Price from PFA is not checked for an end date");
    }

    [TestMethod]
    public void Colors_With_Different_Prices_And_Different_Start_Dates_Should_Be_Correct()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";
      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),
          color_code = colorCode,
          price = 10         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,8),
          price = 5,
          color_code = "5"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 7          
        },
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is not checked for an end date");
      Assert.AreEqual(help.GetPrice(productNumber, "5", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Price from PFA is not checked for an end date");
      Assert.AreEqual(help.GetPrice(productNumber, "12", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 7, "Price from PFA is not checked for an end date");
    }

    [TestMethod]
    public void Price_Of_Zero_On_Product_Level_And_Price_On_Color_Level_Should_Word()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771029";
      string currencyCode = "EURN";
      string colorCode = "10";

      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),
          color_code = colorCode,
          price = 10         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),
          price = 10,
          color_code = "300",

        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),
          price = 0,         
        } 
      });

      AssortmentHelper help = new AssortmentHelper();
      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is not checked for an end date");
      Assert.AreEqual(help.GetPrice(productNumber, "300", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 10, "Price from PFA is not checked for an end date");
      Assert.AreEqual(help.GetPrice(productNumber, "500", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 0, "Price from PFA is not checked for an end date");
    }

    [TestMethod]
    public void Price_Should_Be_Checked_For_Size_Levels()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771031";
      string currencyCode = "EURN";
      string colorCode = "10";

      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,6),
          color_code = colorCode,
          price = 12.95m         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),
          price = 7.50m,
          color_code = colorCode,
					size_code = "XS"

        }
      });

      AssortmentHelper help = new AssortmentHelper();

      Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 12.95m, "Latest price should also ignore the size level");
    }

    [TestMethod]
    public void Change_Of_Price_With_Code_OMP_Should_Be_Imported()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "2EB6771031";
      string currencyCode = "EURN";
      string colorCode = "10";

      string sizeCode = "s";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,6),
          color_code = colorCode,
          price = 12.95m,
         	discount_code = "omp"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,3),
          price = 7.50m,
          color_code = colorCode,
					size_code = "XS",
					discount_code = "afp"
				},
				new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),
          price = 10m,
          color_code = colorCode,
					size_code = "XS"					
				}
      });

      AssortmentHelper help = new AssortmentHelper();

      Assert.AreEqual(12.95m, help.GetPrice(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), "OMP price should be imported");
    }

    [TestMethod]
    public void Ignore_Price_Rules_With_Zero()
    {
      var priceMockRepo = new Mock<IPriceRepository>();
      string productNumber = "1112001046";
      string currencyCode = "EURN";
      string colorCode = "395";

      priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,3,23),
          price = 59.95m         	
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,2),
          end_date = new DateTime(2012,9,24),
          price = 69.95m,
          color_code = colorCode					
				},
				new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,25),
          price = 0,          
					
				},
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,9,25),
          end_date = new DateTime(2012,9,24),
          price = 69.95m          ,
          color_code = colorCode
				}

      });

      AssortmentHelper help = new AssortmentHelper();

      Assert.AreEqual(69.95m, help.GetPrice(productNumber, colorCode, string.Empty, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), "OMP price should be imported");
    }
  }
}
