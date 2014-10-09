using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Xml.Linq;
using System.Reflection;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml;
using System.IO;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ordering.Dispatch;


namespace Concentrator.Order.OrderDispatch
{
	public class OutboundProcessor : ConcentratorPlugin
	{
		private const string vendorSettingType = "DispatcherType";

		public override string Name
		{
			get { return "Outbound Processor"; }
		}

		protected override void Process()
		{
			using (var unit = GetUnitOfWork())
			{
				var outboundMessages = (from o in unit.Scope.Repository<Outbound>().GetAllAsQueryable()
																where !o.Processed && o.OutboundUrl != "MISSING"
																&& (!o.ProcessedCount.HasValue || o.ProcessedCount.Value < 5)
																select o).ToList();

#if DEBUG
        outboundMessages = (from o in unit.Scope.Repository<Outbound>().GetAll(c => c.OutboundID == 610316) select o).ToList();
#endif

        foreach (var message in outboundMessages)
				{

					var url = message.OutboundUrl;

#if DEBUG
					url = "http://localhost:1331";
#endif

					DateTime startTimePost = DateTime.Now;
					try
					{
						System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
						HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
						request.Method = "POST";

						ServicePointManager.ServerCertificateValidationCallback +=
									delegate(object sender, X509Certificate certificate, X509Chain chain,
									SslPolicyErrors sslPolicyErrors)
									{
										return true;
									};

						XmlDocument doc = new XmlDocument();
						string contentType = "text/xml";
						string document = message.OutboundMessage;

						HttpOutboundPostState state = new HttpOutboundPostState(request, message.OutboundID, unit, startTimePost);

						byte[] byteData = UTF8Encoding.UTF8.GetBytes(document);

						state.Request.ContentType = contentType;
						state.Request.ContentLength = byteData.Length;
						using (Stream postStream = request.GetRequestStream())
						{
							postStream.Write(byteData, 0, byteData.Length);
						}

						request.BeginGetResponse(HttpOutboundCallBack, state);
					}
					catch (Exception ex)
					{
						if (message.ProcessedCount.HasValue)
							message.ProcessedCount++;
						else
							message.ProcessedCount = 1;

						message.ErrorMessage = ex.Message;
						DateTime resDateTime = new DateTime(DateTime.Now.Ticks - startTimePost.Ticks);
						message.ResponseTime = resDateTime.Second;
					}
					finally
					{
						unit.Save();
					}
				}
			}
		}

		public static void HttpOutboundCallBack(IAsyncResult result)
		{
			try
			{
				HttpOutboundPostState state = (HttpOutboundPostState)result.AsyncState;

				using (HttpWebResponse httpResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
				{
					var outbound = state.Unit.Scope.Repository<Outbound>().GetSingle(o => o.OutboundID == state.OutBoundID);


					DateTime dateTime = new DateTime(DateTime.Now.Ticks - state.StartPost.Ticks);
					outbound.ResponseRemark = string.Format("HTTP POST Status {0} in {1} seconds", httpResponse.StatusCode, dateTime.Second);
					outbound.ResponseTime = dateTime.Second;

					switch (httpResponse.StatusCode)
					{
						case HttpStatusCode.OK:
							//POST OK
							outbound.Processed = true;
							break;
						case HttpStatusCode.GatewayTimeout:
						case HttpStatusCode.ServiceUnavailable:
						case HttpStatusCode.BadGateway:
							outbound.Processed = false;
							break;
						default:
							break;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Callback failed");
			}
		}
	}
}
