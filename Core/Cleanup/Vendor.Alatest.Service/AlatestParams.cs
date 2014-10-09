using System;
using System.Runtime.Serialization;

namespace AlatestService
{
    [Serializable]
    [DataContract]
    public class AlatestParams
    {
        private ILog log = LogManager.GetLogger(typeof(AlatestParams));

        private string _Review = string.Empty;
        private string _ReviewCollectID = string.Empty;
        private string _MinNumberOfReviewsPerProduct = string.Empty;
        private string _MaxNumberOfReviewsToFetch = string.Empty;
        private string _BaseUrl = string.Empty;
        private string _ProductID = string.Empty;

        [DataMember(IsRequired = true)]
        public string Review
        {
            get { return _Review; }
            set { _Review = value; }
        }
        [DataMember(IsRequired = true)]
        public string ReviewCollectID
        {
            get { return _ReviewCollectID; }
            set
            {
                try
                {
                    _ReviewCollectID = int.Parse(value).ToString();
                }
                catch (FormatException ex)
                {
                    log.Error(string.Format("ReviewCollectID : Failed! Value is not an int! \r\n{0}"), ex);
                    throw new Exception("ReviewCollectID: value is not an int!", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("ReviewCollectID: Failed!", ex);
                }
            }
        }
        [DataMember(IsRequired = true)]
        public string MinNumberOfReviewsPerProduct
        {
            get { return _MinNumberOfReviewsPerProduct; }
            set
            {
                try
                {
                    _MinNumberOfReviewsPerProduct = int.Parse(value).ToString();
                }
                catch (FormatException ex)
                {
                    log.Error(string.Format("MinNumberOfReviewsPerProduct : Failed! Value is not an int! \r\n{0}"), ex);
                    throw new Exception("MinNumberOfReviewsPerProduct: value is not an int!", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("MinNumberOfReviewsPerProduct: Failed!", ex);
                }
            }
        }
        [DataMember(IsRequired = true)]
        public string MaxNumberOfReviewsToFetch
        {
            get { return _MaxNumberOfReviewsToFetch; }
            set
            {
                try
                {
                    _MaxNumberOfReviewsToFetch = int.Parse(value).ToString();
                }
                catch (FormatException ex)
                {
                    log.Error(string.Format("MaxNumberOfReviewsToFetch : Failed! Value is not an int! \r\n{0}"), ex);
                    throw new Exception("MaxNumberOfReviewsToFetch: value is not an int!", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("MaxNumberOfReviewsToFetch: Failed!", ex);
                }
            }
        }
        [DataMember(IsRequired = true)]
        public string BaseUrl
        {
            get { return _BaseUrl; }
            set { _BaseUrl = value; }
        }
        [DataMember(IsRequired = true)]
        public string ProductID
        {
            get { return _ProductID; }
            set
            {
                try
                {
                    //TODO: Add validation
                    _ProductID = value;
                }
                catch (FormatException ex)
                {
                    log.Error(string.Format("ProductID : Failed! Value is not an int! \r\n{0}"), ex);
                    throw new Exception("ProductID: value is not an int.", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("ProductID: Failed!", ex);
                }
            }
        }
    }
}