using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;

namespace AlatestService
{
    internal static class Alatest
    {
        #region Properties
        private static AlatestReview Review
        { get; set; }
        private static AlatestRating Rating
        { get; set; }
        private static DivType AlatestType
        { get; set; }
        private static string SaveStoredProcedureName
        { get; set; }
        private static string RetrieveStoredProcedureName
        { get; set; }
        private static string ProductID
        { get; set; }
        private static string value
        { get; set; }
        private static ILog log
        {
            get
            {
                return LogManager.GetLogger(typeof(Alatest));
            }
        }
        #endregion

        static Alatest()
        {
            SaveStoredProcedureName = string.Empty;
            RetrieveStoredProcedureName = string.Empty;
            ProductID = string.Empty;
            value = string.Empty;
        }

        internal static bool SaveReview(AlatestReview reviewObject)
        {
            if (string.IsNullOrEmpty(reviewObject.ProductID) || string.IsNullOrEmpty(reviewObject.Review))
                return false;

            Review = reviewObject;
            AlatestType = DivType.Review;
            ProductID = reviewObject.ProductID;
            value = ParseDivToXml(ProductID, reviewObject.Review, DivType.Review);
            SetStoredProcedureNames();
            return Save();
        }
        internal static bool SaveRating(AlatestRating ratingObject)
        {
            if (string.IsNullOrEmpty(ratingObject.ProductID) || string.IsNullOrEmpty(ratingObject.Review))
                return false;

            Rating = ratingObject;
            AlatestType = DivType.Rating;
            ProductID = ratingObject.ProductID;
            value = ParseDivToXml(ProductID, ratingObject.Review, DivType.Rating);
            SetStoredProcedureNames();
            return Save();
        }

        internal static string GetReviewFromDatabase(string productID)
        {
            if (string.IsNullOrEmpty(productID))
                return string.Empty;
            else
                return GetAlatestValue(productID, DivType.Review);
        }
        internal static string GetRatingFromDatabase(string productID)
        {
            if (string.IsNullOrEmpty(productID))
                return string.Empty;
            else
                return GetAlatestValue(productID, DivType.Rating);
        }

        private static bool Save()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DevTim"].ConnectionString;

                using (SqlConnection dataConnection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = dataConnection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = SaveStoredProcedureName;

                        cmd.Parameters.Add("@productID", SqlDbType.NChar).Value = ProductID;
                        switch (AlatestType)
                        {
                            case DivType.Review:
                                cmd.Parameters.Add("@review", SqlDbType.NVarChar).Value = value;
                                break;
                            case DivType.Rating:
                                cmd.Parameters.Add("@rating", SqlDbType.NVarChar).Value = value;
                                break;

                            default:
                                break;
                        }

                        dataConnection.Open();

                        cmd.ExecuteReader();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static void SetStoredProcedureNames()
        {
            switch (AlatestType)
            {
                case DivType.Review:
                    SaveStoredProcedureName = "sp_UpdateAlatestReview";
                    RetrieveStoredProcedureName = "sp_FetchAlatestReview";
                    break;

                case DivType.Rating:
                    SaveStoredProcedureName = "sp_UpdateAlatestRating";
                    RetrieveStoredProcedureName = "sp_FetchAlatestRating";
                    break;

                default:
                    SaveStoredProcedureName = string.Empty;
                    break;
            }
        }

        private static string ParseDivToXml(string productID, string value, DivType divType)
        {
            if (string.IsNullOrEmpty(productID) || string.IsNullOrEmpty(value))
                return string.Empty;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.ConformanceLevel = ConformanceLevel.Document;

            try
            {
                using (StringWriter stream = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Alatest");

                        switch (divType)
                        {
                            case DivType.Review:
                                writer.WriteStartElement("Reviews");
                                break;
                            case DivType.Rating:
                                writer.WriteStartElement("Rating");
                                break;
                            default:
                                writer.WriteStartElement("UnknownType");
                                break;
                        }

                        writer.WriteStartElement("ProductID");
                        writer.WriteString(productID);
                        writer.WriteEndElement();
                        writer.WriteString(Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(value)));
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();

                        writer.Flush();

                        return stream.ToString();
                    }
                }
            }
            catch (Exception)
            {
            }

            return string.Empty;
        }
        private static string GetAlatestValue(string ProductID, DivType type)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(
                       ConfigurationManager.ConnectionStrings["DevTim"].ConnectionString))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = RetrieveStoredProcedureName;

                        cmd.Parameters.Add("@productID", SqlDbType.NChar).Value = ProductID;

                        conn.Open();

                        using (DataSet ds = new DataSet())
                        {
                            new SqlDataAdapter(cmd).Fill(ds);

                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                                return ds.Tables[0].Rows[0][type.ToString()].ToString();
                            else return string.Empty;
                        }

                    }
                }
            }
            catch (Exception)
            {
                //TODO:Log errors
                return string.Empty;
            }
        }
    }

    #region Alatest Type Objects
    internal struct AlatestReview
    {
        internal string ProductID { get; set; }
        internal string Review { get; set; }
    }

    internal struct AlatestRating
    {
        internal string ProductID { get; set; }
        internal string Review { get; set; }
    }
    #endregion

    internal enum DivType
    {
        Review = 0,
        Rating = 1
    }
}