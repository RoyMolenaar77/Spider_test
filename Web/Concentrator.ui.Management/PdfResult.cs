using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using ExpertPdf.HtmlToPdf;
using System.Web;
using System.IO;
using System.Threading;
using Concentrator.Objects.Web;
using System.Configuration;

namespace Concentrator.ui.Management
{
  public class PdfResult : ActionResult
  {
    private string _logo= string.Empty;
    private int _languageID = 1;
    private int _productID;

    public PdfResult(string logo, int languageID, int productID){
      _logo = logo;
      _languageID = languageID;
      _productID = productID;
    }

    public override void ExecuteResult(ControllerContext context)
    {
      var pdfConverter = new PdfConverter
                 {
                   LicenseKey = "f/1a0tPOp3hfiJQaOn6P3nNmy4naJk9vM5/uDwaDgYeS+Zz0xa3rM6/D3au4Hwaq",
                   PdfDocumentOptions =
                   {
                     PdfPageSize = PdfPageSize.A4,
                     PdfPageOrientation = PDFPageOrientation.Portrait,
                     PdfCompressionLevel = PdfCompressionLevel.NoCompression,
                     ShowHeader = false,
                     ShowFooter = false,
                     LeftMargin = 0,
                     RightMargin = 0,
                     TopMargin = 0,
                     BottomMargin = 0,
                     FitWidth = false,
                     GenerateSelectablePdf = true,
                     EmbedFonts = true
                   },
                   ActiveXEnabled = true,
                   PdfFooterOptions = { ShowPageNumber = true }
                 };

      UrlHelper helper = new UrlHelper(context.RequestContext);
      //string url = string.Format("http://localhost/Concentrator.ui.Management/FactSheet/FactSheet?productID={0}&logo={1}&languageID={2}",_productID,_logo,_languageID);

      var url = string.Format("http://{0}/FactSheet/FactSheet?productID={0}&logo={1}&languageID={2}", ConfigurationManager.AppSettings["ConcentratorManagement"].ToString(), _productID, _logo, _languageID);


      byte[] downloadBytes = pdfConverter.GetPdfFromUrlBytes(url);

      var response = HttpContext.Current.Response;
      response.Clear();
      response.ClearHeaders();
      response.ClearContent();
      response.AddHeader("Content-Type", "binary/octet-stream");
      response.AddHeader("Content-Disposition",
        "attachment; filename=FactSheet.pdf;");
      response.Buffer = false;
      response.BufferOutput = false;
      response.Flush();

      using (var writer = new StreamWriter(response.OutputStream))
      {
        writer.WriteLine("");
        writer.Flush();
      }

      response.Flush();

      response.BinaryWrite(downloadBytes);
      response.Flush();

      response.End();
    }
  }
}
