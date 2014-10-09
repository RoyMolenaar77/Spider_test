using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;

namespace Concentrator.Objects.WebToPrint
{
  public class PrintableDocument
  {
    public List<PrintableStyle> SharedStyles;
    public List<PrintablePage> Pages;

    public PrintableDocument()
    {
      SharedStyles = new List<PrintableStyle>();
      Pages = new List<PrintablePage>();
    }

    public void AssignPageNumbers()
    {
        int pageNo = 1;
        foreach (PrintablePage pp in Pages)
        {
            pp.PageNumber = pageNo;
            pageNo++;
        }
    }
  }
}
