using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.ui.Management.Controllers
{
	public class PaymentMethodController : BaseController
	{
		[RequiresAuthentication(Functionalities.ViewOrders)]
		public ActionResult GetList()
		{
			return List(unit => from o in unit.Service<PaymentMethod>().GetAll()
													from p in o.PaymentMethodDescriptions
													select new
													{
														o.ID,
														o.Code,
														p.LanguageID,
														p.Description
													});

		}

		[RequiresAuthentication(Functionalities.ViewOrders)]
		public ActionResult Create(string Code, int LanguageID, string Description)
		{
			try
			{
				using (var unit = GetUnitOfWork())
				{
					var paymentMethod = unit.Service<PaymentMethod>().Get(c => c.Code == Code);
					if (paymentMethod == null)
					{
						paymentMethod = new PaymentMethod()
						{
							Code = Code,
							PaymentMethodDescriptions = new List<PaymentMethodDescription>()
						};
						unit.Service<PaymentMethod>().Create(paymentMethod);
					}

					paymentMethod.PaymentMethodDescriptions.Add(new PaymentMethodDescription()
					{
						LanguageID = LanguageID,
						Description = Description
					});

					unit.Save();
					return Success("Payment method added");
				}
			}
			catch (Exception e)
			{
				return Failure("Failed to add payment method", e);
			}
		}

		[RequiresAuthentication(Functionalities.ViewOrders)]
		public ActionResult Update(int _ID, int _LanguageID, string Description)
		{
			try
			{
				using (var unit = GetUnitOfWork())
				{
					var paymentMethod = unit.Service<PaymentMethod>().Get(c => c.ID == _ID);

					if (paymentMethod == null) throw new InvalidOperationException("Payment code must exist");

					var description = paymentMethod.PaymentMethodDescriptions.FirstOrDefault(c => c.LanguageID == _LanguageID);
					if (description == null) throw new InvalidOperationException("Description must exist");

					description.Description = Description;

					unit.Save();
					return Success("Payment method updated");
				}
			}
			catch (Exception e)
			{
				return Failure("Failed to update payment method", e);
			}
		}

		[RequiresAuthentication(Functionalities.ViewOrders)]
		public ActionResult Delete(int _ID, int _LanguageID)
		{
			try
			{
				using (var unit = GetUnitOfWork())
				{
					var paymentMethod = unit.Service<PaymentMethod>().Get(c => c.ID == _ID);

					if (paymentMethod == null) throw new InvalidOperationException("Payment code must exist");

					var description = paymentMethod.PaymentMethodDescriptions.FirstOrDefault(c => c.LanguageID == _LanguageID);
					if (description == null) throw new InvalidOperationException("Description must exist");

					paymentMethod.PaymentMethodDescriptions.Remove(description);

					unit.Save();
					return Success("Payment method description deleted");
				}
			}
			catch (Exception e)
			{
				return Failure("Failed to delete payment method description", e);
			}
		}
	}
}
