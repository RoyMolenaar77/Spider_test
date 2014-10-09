using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Concentrator.Objects.Tests
{
	public class TestObject
	{
		public string Name { get; set; }

		public int Age { get; set; }

		public bool Man { get; set; }

		public DateTime BirthDate { get; set; }

		public decimal Money { get; set; }
	}

	[TestClass]
	public class EnumerableExtensionsTests
	{
		[TestMethod]
		public void UniqueOn_Should_Return_Unique_Values_On_String_Comparison()
		{
			List<TestObject> testList = new List<TestObject>()
			{
				new TestObject(){
					Name = "John Doe",
					Age = 22,
					BirthDate = DateTime.Now,
					Man = true,
					Money = 10m
				},
				new TestObject(){
					Name = "John Doe",
					Age = 23,
					BirthDate = DateTime.Now,
					Man = false,
					Money = 11m
				}
			};

			var result = testList.UniqueOn(c => c.Name);

			Assert.IsTrue(result.Count() == 1, "Resulting list is bigger than expected");
		}

		[TestMethod]
		public void UniqueOn_Should_Return_Unique_Values_On_String_Comparison_With_Multiple_Different_Values()
		{
			List<TestObject> testList = new List<TestObject>()
			{
				new TestObject(){
					Name = "John Doe",
					Age = 22,
					BirthDate = DateTime.Now,
					Man = true,
					Money = 10m
				},
				new TestObject(){
					Name = "John Doe",
					Age = 23,
					BirthDate = DateTime.Now,
					Man = false,
					Money = 11m
				},
				new TestObject(){
					Name = "John Doe 1",
					Age = 23,
					BirthDate = DateTime.Now,
					Man = false,
					Money = 11m
				}
			};

			var result = testList.UniqueOn(c => c.Name);

			Assert.IsTrue(result.Count() == 2, "Resulting list is bigger than expected");
		}

		[TestMethod]

		public void UniqueOn_Should_Return_Unique_Values_On_Int_Comparison()
		{
			List<TestObject> testList = new List<TestObject>()
			{
				new TestObject(){
					Name = "John Doe",
					Age = 22,
					BirthDate = DateTime.Now,
					Man = true,
					Money = 10m
				},
				new TestObject(){
					Name = "John Doe",
					Age = 22,
					BirthDate = DateTime.Now,
					Man = false,
					Money = 11m
				}
			};

			var result = testList.UniqueOn(c => c.Age);

			Assert.IsTrue(result.Count() == 1, "Resulting list is bigger than expected");
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void Random_Should_Throw_A_Null_Reference_If_List_Is_Null()
		{
			List<int> numbers = null;

			numbers.Random();
		}

		[TestMethod]
		public void Random_Should_Return_Default_If_List_Is_Empty()
		{
			List<int> numbers = new List<int>();

			Assert.AreEqual(0, numbers.Random());
		}

		[TestMethod]
		public void Random_Should_Return_An_Item_In_The_Original_Collection()
		{
			List<int> numbers = new List<int>() { 1, 2, 3, 4 };
			var n = numbers.Random();

			Assert.IsTrue(numbers.Contains(n));
		}
	}
}
