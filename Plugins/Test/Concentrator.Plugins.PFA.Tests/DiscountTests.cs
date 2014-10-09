using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Plugins.PFA.Repos;
using Moq;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Helpers;
using System.IO;
using Excel;
using System.Data;
using System.Globalization;
using Concentrator.Objects.Extensions;

namespace Concentrator.Plugins.PFA.Tests
{
	[TestClass]
	public class DiscountTests
	{
		[TestMethod]
		public void One_Discount_Rule_Should_Return_A_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 5,
          discount_code = discountCode
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Discount from PFA is not as it is expected");
		}

		[TestMethod]
		public void Discount_Rule_On_Color_Level_Should_Return_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Discount_Rule_On_Color_Level_With_End_Date_Should_Not_Return_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),
          end_date = new DateTime(2011,7,8),
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), null, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Discount_Rule_On_Color_Level_Should_Have_Precedence()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,          
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Newer_Discount_Rule_On_Color_Level_Should_Have_Precedence()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,         
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),          
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Newer_Discount_Rule_Should_Have_Precedence()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,          
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),          
          price = 5,
          discount_code = discountCode          
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Several_Discount_Rules_On_Color_Level_With_Different_Start_Dates_Should_Be_Respected()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,     
          color_code = "5"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),          
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        }
      });

			AssortmentHelper help = new AssortmentHelper();
			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 5, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "5", sizeCode, priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode)), 7, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Color_Levels_That_Dont_Have_Discounts_Should_Not_Be_Discounted()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,     
          color_code = "5"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),          
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        },
        
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, prices), 5, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "5", sizeCode, prices), 7, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "12", sizeCode, prices), null, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Newer_Discounts_On_Product_Level_Should_Override_Old_Ones()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,     
          color_code = "5"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),          
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,10),          
          price = 3,
          discount_code = discountCode,
          
        },
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, prices), 3, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "5", sizeCode, prices), 3, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "12", sizeCode, prices), 3, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Older_Discounts_On_Product_Level_Should_Not_Override_Newer_Color_Levels()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,7),          
          price = 7,
          discount_code = discountCode,     
          color_code = "5"
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,9),          
          price = 5,
          discount_code = discountCode,
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2011,7,6),          
          price = 3,
          discount_code = discountCode,
          
        },
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, prices), 5, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "5", sizeCode, prices), 7, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetDiscount(productNumber, "12", sizeCode, prices), 3, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Discounts_With_Date_Later_Than_Today_Should_Be_Discarded()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "2EB6771029";
			string currencyCode = "EURN";
			string colorCode = "10";
			string sizeCode = "s";
			string discountCode = "afp";

			DateTime currentTime = new DateTime(2012, 7, 20);

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,20),          
					end_date = new DateTime(2012,7,23),          
          price = 15,
          discount_code = discountCode,     
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,24),          
					end_date = new DateTime(2012,7,23),          
          price = 19.95m,
          discount_code = discountCode,
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,21),          
          price = 10,
          discount_code = discountCode,
					color_code = "20"          
        },
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), 15, "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Price_With_A_Start_Date_Later_Than_Discount_With_Start_Date_And_No_End_Date_Should_Override_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512510009";
			string currencyCode = "EURN";
			string colorCode = "100";
			string sizeCode = "s";
			string discountCode = "tijd";

			DateTime currentTime = new DateTime(2012, 7, 30);

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,24),          		
					price = 15,
          discount_code = discountCode,     
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,30),          					
          price = 21.50m,
          color_code = colorCode
        }
        
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), null, "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(help.GetPrice(productNumber, colorCode, sizeCode, prices, currentTime), 21.50m, "Discount from PFA on color level is not as it is expected");

		}

		[TestMethod]
		public void Price_With_A_Start_Date_Earlier_Than_Discount_With_Start_Date_And_No_End_Date_Should_Not_Override_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512509031";
			string currencyCode = "EURN";
			string colorCode = "800";
			string sizeCode = "s";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
				new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2007,11,15),          
					price = 49.95m,
          discount_code = "spec"          
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,2),          					
					end_date = new DateTime(2013,12,31),          					
          price = 59.95m,
          color_code = colorCode,
					discount_code = "tijd"
        }, new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,3,7),          
					price = 64.95m      
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,3,7),          					
					end_date = new DateTime(2013,12,31),
          price = 64.95m,
          color_code = colorCode
        }
			
			});

			DateTime currentTime = new DateTime(2012, 7, 30);


			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(59.95m, help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(64.95m, help.GetPrice(productNumber, colorCode, sizeCode, prices, currentTime), "Price from PFA on color level is not as it is expected");

		}

		[TestMethod]
		public void Discount_on_Product_Level_Later_Than_On_Color_Level()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512509031";
			string currencyCode = "EURN";
			string colorCode = "800";
			string sizeCode = "s";

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
				new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,2),           
					price = 49.95m,
          discount_code = "spec"          
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2007,11,15),          					
					end_date = new DateTime(2013,12,31),          					
          price = 59.95m,
          color_code = colorCode,
					discount_code = "tijd"
        }, new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,3,7),          
					price = 64.95m      
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,3,7),          					
					end_date = new DateTime(2013,12,31),
          price = 64.95m,
          color_code = colorCode
        }
			
			});

			DateTime currentTime = new DateTime(2012, 7, 30);


			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(49.95m, help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), "Discount from PFA on color level is not as it is expected");
			Assert.AreEqual(64.95m, help.GetPrice(productNumber, colorCode, sizeCode, prices, currentTime), "Price from PFA on color level is not as it is expected");

		}

		[TestMethod]
		public void Price_With_A_Start_Date_Later_Than_Discount_With_Start_Date_And_End_Date_Should_Not_Override_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512510009";
			string currencyCode = "EURN";
			string colorCode = "100";
			string sizeCode = "s";
			string discountCode = "tijd";

			DateTime currentTime = new DateTime(2012, 7, 30);

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,24),          		
					end_date = new DateTime(2013,7,24),
					price = 15,
          discount_code = discountCode,     
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,30),          					
          price = 21.50m,
          color_code = colorCode
        }
        
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(15m, help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Price_With_A_Start_Date_Later_Than_Discount_With_Start_Date_And_Same_End_Date_Should_Not_Override_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512510009";
			string currencyCode = "EURN";
			string colorCode = "100";
			string sizeCode = "s";
			string discountCode = "tijd";

			DateTime currentTime = new DateTime(2012, 7, 30);

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,24),          		
					end_date = new DateTime(2012,7,30),
					price = 15,
          discount_code = discountCode,     
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,30),          					
          price = 21.50m,
          color_code = colorCode
        }
        
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(15m, help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Price_With_A_Start_Date_Later_Than_Discount_With_Start_Date_And_Earlier_End_Date_Should_Override_The_Discount()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512510009";
			string currencyCode = "EURN";
			string colorCode = "100";
			string sizeCode = "s";
			string discountCode = "tijd";

			DateTime currentTime = new DateTime(2012, 7, 30);

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,24),          		
					end_date = new DateTime(2012,7,29),
					price = 15,
          discount_code = discountCode,     
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,30),          					
          price = 21.50m,
          color_code = colorCode
        }
        
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(null, help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), "Discount from PFA on color level is not as it is expected");
		}

		[TestMethod]
		public void Discount_With_Code_OMP_Should_Be_Ignored()
		{
			var priceMockRepo = new Mock<IPriceRepository>();
			string productNumber = "1512510009";
			string currencyCode = "EURN";
			string colorCode = "100";
			string sizeCode = "s";
			string discountCode = "omp";

			DateTime currentTime = new DateTime(2012, 7, 30);

			priceMockRepo.Setup(c => c.GetProductPriceRules(productNumber, currencyCode)).Returns(new List<PriceResult>() { 
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,24),          		
					price = 15,
          discount_code = discountCode,     
          color_code = colorCode
        },
        new PriceResult(){
          art_code = productNumber,
          currency_code = currencyCode,
          start_date  = new DateTime(2012,7,23),          					
          price = 21.50m,
          color_code = colorCode
        }        
      });

			AssortmentHelper help = new AssortmentHelper();

			var prices = priceMockRepo.Object.GetProductPriceRules(productNumber, currencyCode);

			Assert.AreEqual(null, help.GetDiscount(productNumber, colorCode, sizeCode, prices, currentTime), "Discount from PFA on color level is not as it is expected");
		}
	}
}
