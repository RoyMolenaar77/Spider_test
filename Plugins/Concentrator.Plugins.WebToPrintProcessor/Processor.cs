using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.WebToPrint;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Concentrator.Objects;
using System.IO;
using System.Security.Policy;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.WebToPrint;
using Concentrator.Objects.WebToPrint.Components;

namespace Concentrator.Plugins.WebToPrintProcessor
{
  public class Processor: ConcentratorPlugin
  {
    public override string Name
    {
      get { return "WebToPrintQueueProcessor"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var ProcessQueue = unit.Scope.Repository<WebToPrintQueue>().GetAll(wtpqd => wtpqd.Status == (int)WebToPrintQueueStatus.Queued).ToList();
        QueryBuilder qb;
        PrintXMLParser xmlparser;
        PrintableDocument doc;
        PdfDocument pdfdocument;
        PdfPage pdfpage; XGraphics gfx;

        string savepath = GetConfiguration().AppSettings.Settings["DesignerPDFDirectory"].Value.ToString();
        log.AuditInfo("Beginning to process queue, " + ProcessQueue.Count() + " documents found");
        
        // process each document still in the queue, where x is the queueitem and y is the index
        ProcessQueue.ForEach((x,y) =>
        {
          try
          {
            string input = x.Data;
            // the querybuilder will get the bindings and fill the related objects with data
            qb = new QueryBuilder();
            // the xml parser will transform the XML from the queue to a usable object
            xmlparser = new PrintXMLParser(input, qb);
            // build the document
            doc = xmlparser.GeneratePrintableDocument();

            if (doc.Pages.Count == 0)
              throw new Exception("A document must have atleast 1 page");

            // execute the querybuilder
            qb.Execute(unit);

            // the index builder is responsible for building and filling the index in the document
            IndexBuilder ib = new IndexBuilder(doc);
            // create more pages for the index if it wasn't big enough
            ib.ExpandIndexPages();

            // create an actual PDF document
            pdfdocument = new PdfDocument();

            // assign page numbers to all objects for later use by the indexbuilder
            doc.AssignPageNumbers();
            // creates the index and fills it into the index pages' components
            ib.BuildIndex();

            foreach (PrintablePage page in doc.Pages)
            {
              pdfpage = pdfdocument.AddPage();
              // convert mm to inch, and then to points
              pdfpage.Width = Util.MillimeterToPoint(page.Width);
              pdfpage.Height = Util.MillimeterToPoint(page.Height);
              // get graphics draw handle
              gfx = XGraphics.FromPdfPage(pdfpage);
              // render the page (and all subcomponents)
              page.Render(ref gfx);
            }
            // find the path on where we want to save the generated document
            string user = x.WebToPrintProject.User != null ? x.WebToPrintProject.User.Username : "";
            if (!Directory.Exists(Path.Combine(new string[] { savepath, user, x.WebToPrintProject.Name })))
            {
              Directory.CreateDirectory(Path.Combine(new string[] { savepath, user, x.WebToPrintProject.Name }));
            }
            string filename = "PrintService_" + x.QueueID+"_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
            string path = Path.Combine(user, x.WebToPrintProject.Name, filename);

            // finally save it
            pdfdocument.Save(Path.Combine(savepath,path));

            // set the status and the download link for the designer client
            x.Status = (int)WebToPrintQueueStatus.Done;
            x.Message = path.ToString().Replace('\\', '/');

            log.AuditInfo("Document " + x.QueueID + " with " + doc.Pages.Count + " page(s) processed");
          }

          catch (Exception e)
          {
#if !DEBUG
            x.Status = (int)WebToPrintQueueStatus.Error;
            x.Message = e.Message;
#endif
            log.AuditError("Error while processing "+x.QueueID.ToString(),e);
          }
        });
        unit.Save();
       
      }
    }
  }
}
