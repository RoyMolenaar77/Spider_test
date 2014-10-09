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

namespace Concentrator.Vendors.Sennheiser
{
  public class PdfResult : ActionResult
  {
    public int? BrandID { get; set; }

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
      string url = helper.Action("Index", "Home", null, "http");
      
      if(BrandID.HasValue)
        url = helper.Action("Index", "Home", new { brandID = BrandID.Value }, "http");
      
      pdfConverter.NavigationTimeout = int.Parse(new TimeSpan(0, 15, 0).TotalSeconds.ToString());
      byte[] downloadBytes = pdfConverter.GetPdfFromUrlBytes(url);

      var response = HttpContext.Current.Response;
      response.Clear();
      response.ClearHeaders();
      response.ClearContent();
      response.AddHeader("Content-Type", "binary/octet-stream");
      response.AddHeader("Content-Disposition",
        "attachment; filename=Pricelist.pdf;");
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
