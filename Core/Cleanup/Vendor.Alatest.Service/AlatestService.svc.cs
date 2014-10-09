using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace AlatestService
{
    public class AlatestService : IAlatestService
    {
        #region Properties
        private static DivType AlatestType { get; set; }
        #region Readonly
        private static string AlatestServerUrl
        {
            get
            {
                try
                {
                    string url = ConfigurationManager.AppSettings["AlatestServerUrl"];
                    return !string.IsNullOrEmpty(url) && url.StartsWith("http://", true, null) ?
                        url.EndsWith("/") ? url : string.Concat(url, '/') : string.Empty;
                }
                catch (Exception ex)
                {
                    log.Error("An error ocurred trying to retrieve the Alatest Server Url.", ex);
                    return string.Empty;
                }
            }
        }
        private static string AlatestVirtualDirUrl
        {
            get
            {
                try
                {
                    string url = string.Empty;

                    switch (AlatestType)
                    {
                        case DivType.Rating:
                            url = ConfigurationManager.AppSettings["AlatestVirtualRatingDir"];
                            break;

                        case DivType.Review:
                            url = ConfigurationManager.AppSettings["AlatestVirtualReviewDir"];
                            break;

                        default:
                            url = string.Empty;
                            break;
                    }

                    url = !string.IsNullOrEmpty(url) ? url.StartsWith("/") ? url.Remove(0, 1) : url : string.Empty;
                    return !string.IsNullOrEmpty(url) ? url.EndsWith("/") ? url : string.Concat(url, '/') : string.Empty;
                }
                catch (Exception ex)
                {
                    log.Error("An error ocurred trying to retrieve the virtual directory part of the Alatest Server Url.", ex);
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// Standard alatest parameters to add.
        /// <para>Note: Add to the End of the Querystring</para>
        /// </summary>
        private static string ReviewPartnerID
        {
            get
            {
                try
                {
                    string partnerid = ConfigurationManager.AppSettings["partnerid"];
                    return !string.IsNullOrEmpty(partnerid) ? partnerid.EndsWith("/") ? partnerid : string.Concat(partnerid, '/') : string.Empty;
                }
                catch (Exception)
                {
                    //TODO: Log error
                    return string.Empty;
                }
            }
        }
        private static string MinimumReviews
        {
            get
            {
                try
                {
                    string prodMinViews = ConfigurationManager.AppSettings["prodMinViews"];
                    return !string.IsNullOrEmpty(prodMinViews) ? prodMinViews.EndsWith("/") ? prodMinViews : string.Concat(prodMinViews, '/') : string.Empty;
                }
                catch (Exception)
                {
                    //TODO: Log error
                    return string.Empty;
                }
            }
        }

        private static ILog log
        {
            get
            {
                return LogManager.GetLogger(typeof(AlatestService));
            }
        }
        #endregion
        #endregion

        public string GetReview(string productID)
        {
            try
            {
                if (string.IsNullOrEmpty(productID))
                    return string.Empty;
                else
                {
                    AlatestType = DivType.Review;
                    productID = productID.Replace(" ", "");
                }

                string url = string.Concat(AlatestServerUrl, AlatestVirtualDirUrl, ReviewPartnerID,
                    productID.EndsWith("/") ? productID : string.Concat(productID, "/"),
                    MinimumReviews);

                if (!string.IsNullOrEmpty(url))
                    using (var stream = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()))
                    {
                        string value = stream.ReadToEnd();

                        if (!string.IsNullOrEmpty(value))
                            if (Alatest.SaveReview(new AlatestReview { ProductID = productID, Review = value }))
                                return Alatest.GetReviewFromDatabase(productID);


                    }

                return string.Empty;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("AlatestService.GetReview(): Failed! \r\n{0}"), ex);
                return string.Empty;
            }
        }

        public string GetRating(string productID)
        {
            if (string.IsNullOrEmpty(productID))
                return string.Empty;
            else
            {
                AlatestType = DivType.Rating;
                productID = productID.Replace(" ", "");
            }

            string url = string.Concat(AlatestServerUrl, AlatestVirtualDirUrl, ReviewPartnerID,
                productID.EndsWith("/") ? productID : string.Concat(productID, "/"));

            if (!string.IsNullOrEmpty(url))
                using (var stream = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()))
                {
                    string value = stream.ReadToEnd();

                    if (!string.IsNullOrEmpty(value))
                        if (Alatest.SaveRating(new AlatestRating { ProductID = productID, Review = value }))
                            return Alatest.GetRatingFromDatabase(productID);
                }

            return string.Empty;
        }
    }
}