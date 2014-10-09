using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Concentrator.Objects.WebToPrint;
using Concentrator.Objects.WebToPrint.Components;
using System.Drawing;
using System.Xml.Linq;
using System.Xml;
using Concentrator.Objects;
using System.Data.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;
using Quartz;
using Quartz.Impl;
using Concentrator.Objects.Web;
using System.Net;
using System.Web.Services.Protocols;
using Concentrator.Web.Services.Base;
using Concentrator.Objects.Models.WebToPrint;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;
using System.IO;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Plugin;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Lucene;

namespace Concentrator.Web.Services.Designer
{

  /// <summary>
  /// Summary description for PrintService
  /// </summary>
  [WebService(Namespace = "http://webtoprint.concentrator.diract")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class PrintService : BaseConcentratorService
  {
    #region Printing
    [WebMethod(EnableSession = true)]
    public string BuildPDF(string input, int projectId)
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      XElement result = new XElement("Result");

      using (var unit = GetUnitOfWork())
      {
        var project = unit.Scope.Repository<WebToPrintProject>().GetSingle(c => c.ProjectID == projectId);
        Plugin processor = unit.Scope.Repository<Plugin>().GetSingle(c => c.PluginType == "Concentrator.Plugins.WebToPrintProcessor.Processor");
        if (project != null)
        {
          WebToPrintQueue wtpq = new WebToPrintQueue()
          {
            Data = input,
            Status = (int)WebToPrintQueueStatus.Queued,
            WebToPrintProject = project,
            Message = (processor != null && processor.NextRun != null) ? string.Format("Will be processed at {0}", processor.NextRun) : ""
          };

          unit.Scope.Repository<WebToPrintQueue>().Add(wtpq);
          unit.Save();
          result.Add(new XElement("PrintQueueID", wtpq.QueueID));
        }
      }

      doc.Add(result);
      return doc.ToString();
    }

    [WebMethod(EnableSession = true)]
    public string GetPrintQueue()
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {

        //var qu = unit.Scope.Repository<WebToPrintQueue>().Include(c => c.WebToPrintProject, c => c.WebToPrintProject.User);

        XDocument doc = new XDocument();

        doc.Add(new XElement("Queue", from q in unit.Scope.Repository<WebToPrintQueue>().GetAll(c => c.WebToPrintProject.UserID == Client.User.UserID).ToList()
                                      select new XElement("Document", new XAttribute[] {
         new XAttribute("QueueID",q.QueueID),
         new XAttribute("Status",((WebToPrintQueueStatus)q.Status).ToString()),
         new XAttribute("Project", q.WebToPrintProject.Name),
         new XAttribute("User", q.WebToPrintProject.User.Username),
         new XAttribute("Message",(((WebToPrintQueueStatus)q.Status) == WebToPrintQueueStatus.Done) ? ConfigurationManager.AppSettings["DesignerPDFURL"] + (q.Message ?? "") : q.Message ?? "")
      })));
        return doc.ToString();
      }
    }
    #endregion

    #region Binding

    [WebMethod(EnableSession = true)]
    public string GetData(int BindingID, string[] parameters, bool singleRow = false)
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      switch (BindingID)
      {
        case -1: // page

          break;
        case -2: // static image

          break;
        default:
          using (var unit = GetUnitOfWork())
          {
            // get the query
            WebToPrintBinding wtpb = unit.Scope.Repository<WebToPrintBinding>().GetSingle(c => c.BindingID == BindingID);
            string sql = "";
            if (wtpb != null)
            {
              sql = wtpb.Query;

              using (SqlConnection conn = new SqlConnection(Environments.Current.Connection))//context.Connection.ConnectionString))
              {
                conn.Open();
                SqlCommand comm = new SqlCommand(sql, conn);
                int i = 0;
                foreach (WebToPrintBindingField wtpbf in wtpb.WebToPrintBindingFields.Where(c => c.Type % 2 == 0))
                {
                  comm.Parameters.Add(new SqlParameter("@" + wtpbf.FieldID.ToString(), parameters[i]));
                  i++;
                }

                SqlDataReader reader = comm.ExecuteReader();
                XElement root = new XElement("root");
                while (reader.Read())
                {
                  root.Add(new XElement("row", from WebToPrintBindingField wtpbf in wtpb.WebToPrintBindingFields where wtpbf.Type % 2 == 1 select new XElement("Value", new XAttribute("Name", wtpbf.Name), reader.GetSqlValue(reader.GetOrdinal(wtpbf.FieldID.ToString())))));
                  if (singleRow)
                  {
                    break;//dance
                  }
                }
                doc.Add(root);
                conn.Close();
              }
            }
          } break;
      }
      return doc.ToString();
    }

    [WebMethod(EnableSession = true)]
    public string GetBindings()
    {
      checkCookieAndLogin();
      try
      {
        using (var unit = GetUnitOfWork())
        {
          XElement root = new XElement("root", from wtpb
                                               in unit.Scope.Repository<WebToPrintBinding>().GetAll(c => c.WebToPrintBindingFields.Where<WebToPrintBindingField>(field => field.Type > 1).Count() > 0).ToList()
                                               // leave out bindings where there are unknowns in the field types
                                               select new XElement("Binding",
                                                 new XAttribute("ID", wtpb.BindingID),
                                                 new XAttribute("Name", wtpb.Name),
                                                 new XElement("Inputs", from field in wtpb.WebToPrintBindingFields
                                                                        // inputs are whole numbers
                                                                        where field.Type % 2 == 0
                                                                        select new XElement("Field",
                                                                          new XAttribute("Name", field.Name),
                                                                          new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), field.Type)),
                                                                          new XAttribute("Options", field.Options))),
                                                 new XElement("Outputs", from field in wtpb.WebToPrintBindingFields
                                                                         where field.Type % 2 == 1
                                                                         select new XElement("Field",
                                                                           new XAttribute("Name", field.Name.Replace("[", "").Replace("]", "")),
                                                                           new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), field.Type - 1))))));
          root.Add(new XElement("SpecialBinding",
                    new XAttribute("ID", -1),
                    new XAttribute("Name", "Page"),
                    new XElement("Outputs",
                      new XElement("Field",
                        new XAttribute("Name", "Page Number"),
                        new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), BindingFieldType.Int))),
                      new XElement("Field",
                        new XAttribute("Name", "Index"),
                        new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), BindingFieldType.String))))));

          root.Add(new XElement("SpecialBinding",
                    new XAttribute("ID", -2),
                    new XAttribute("Name", "Static Image"),
                    new XElement("Inputs",
                      new XElement("Field",
                        new XAttribute("Name", "ImagePath"),
                        new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), BindingFieldType.String)),
                        new XAttribute("Options", (int)BindingFieldOptions.StaticImageSearch))), // third bit set = static image search, see WebToPrintBindingField.cs
                    new XElement("Outputs",
                      new XElement("Field",
                        new XAttribute("Name", "ImageURL"),
                        new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), BindingFieldType.ImageURL))))));

          root.Add(new XElement("SpecialBinding",
                    new XAttribute("ID", -3),
                    new XAttribute("Name", "Static Text"),
                    new XElement("Inputs",
                      new XElement("Field",
                        new XAttribute("Name", "TextInput"),
                        new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), BindingFieldType.String)),
                        new XAttribute("Options", (int)BindingFieldOptions.StaticTextInput))), // fourth bit set = static text input
                    new XElement("Outputs",
                      new XElement("Field",
                        new XAttribute("Name", "TextOutput"),
                        new XAttribute("Type", Enum.GetName(typeof(BindingFieldType), BindingFieldType.String))))));

          using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
          {
            using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
            {
              writer.WriteStartDocument(true);
              root.WriteTo(writer);
              writer.Flush();
              writer.WriteEndDocument();
              writer.Flush();
            }

            stringWriter.Flush();
            return stringWriter.ToString();
          }
        }
      }
      catch (Exception e)
      {

        return "";
      }
    }
    #endregion

    #region Projects
    [WebMethod(EnableSession = true)]
    public int CreateProject(string name, string description)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        WebToPrintProject project = new WebToPrintProject()
        {
          Name = name,
          Description = description,
          UserID = Client.User.UserID
        };
        unit.Scope.Repository<WebToPrintProject>().Add(project);
        unit.Save();
        return project.ProjectID;
      }
    }

    [WebMethod(EnableSession = true)]
    public string GetProjects()
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      using (var unit = GetUnitOfWork())
      {
        doc.Add(new XElement("Projects", from proj in unit.Scope.Repository<WebToPrintProject>().GetAll(c => c.UserID == Client.User.UserID).ToList()
                                         select
                                           new XElement("Project",
                                           new XAttribute("ID", proj.ProjectID),
                                           new XAttribute("Name", proj.Name),
                                           new XAttribute("Description", proj.Description),
                                           new XAttribute("UserID", proj.UserID),
                                           new XAttribute("Documents", proj.WebToPrintDocuments.Count),
                                           new XAttribute("Composites", proj.WebToPrintComposites.Count))));
      }
      return doc.ToString();
    }
    #endregion

    #region Documents
    [WebMethod(EnableSession = true)]
    public int CreateDocument(int projectID, string name, string data)
    {
      using (var unit = GetUnitOfWork())
      {
        checkCookieAndLogin();
        WebToPrintDocument document = new WebToPrintDocument()
        {
          Name = name,
          Data = data,
          WebToPrintProject = unit.Scope.Repository<WebToPrintProject>().GetSingle(c => c.ProjectID == projectID)
        };
        if (document.WebToPrintProject == null)
        {
          throw new Exception("No valid project found!");
        }
        if (unit.Scope.Repository<WebToPrintDocument>().GetAllAsQueryable(c => c.ProjectID == projectID && c.Name.ToLower() == name).Count() > 0)
        {
          throw new Exception("Failed to create document: Duplicate name found!");
        }

        try
        {
          unit.Scope.Repository<WebToPrintDocument>().Add(document);
          unit.Save();
        }
        catch (SqlException e)
        {
          throw new Exception("Unknown error occured while inserting new document into the database");
        }
        return document.DocumentID;
      }
    }

    [WebMethod(EnableSession = true)]
    public void SaveDocument(int documentID, string data)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        ConcentratorPrincipal.Login("SYSTEM", "SYS");
        WebToPrintDocument document = unit.Scope.Repository<WebToPrintDocument>().GetSingle(c => c.DocumentID == documentID);
        if (document == null)
        {
          throw new Exception("Trying to save to a non-existing document!");
        }
        document.Data = data;
        try
        {
          unit.Save();
        }
        catch
        {
          throw new Exception("Unknown error occured while updating the document in the database");
        }
      }
    }

    [WebMethod(EnableSession = true)]
    public string GetDocuments(int projectID)
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      using (var unit = GetUnitOfWork())
      {
        doc.Add(new XElement("Documents", from document in unit.Scope.Repository<WebToPrintDocument>().GetAll(d => d.ProjectID == projectID).ToList() select new XElement("Document", new XAttribute("ID", document.DocumentID), new XAttribute("Name", document.Name), new XAttribute("Data", document.Data))));
      }
      return doc.ToString();
    }
    #endregion

    #region Pages
    [WebMethod(EnableSession = true)]
    public int CreatePage(int projectID, string page, string name)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        if (unit.Scope.Repository<WebToPrintPage>().GetAll(c => c.Name.ToLower() == name.ToLower() && c.ProjectID == projectID).Count() > 0)
          throw new Exception("Page name already exists!");

        WebToPrintPage wtpp = new WebToPrintPage() { Name = name, WebToPrintProject = unit.Scope.Repository<WebToPrintProject>().GetSingle(c => c.ProjectID == projectID), Data = page };
        if (wtpp.WebToPrintProject != null)
        {
          try
          {
            unit.Scope.Repository<WebToPrintPage>().Add(wtpp);
            unit.Save();
            return wtpp.PageID;
          }
          catch
          {
            throw new Exception("Failed to insert page: reason unknown");
          }
        }
        else
        {
          throw new Exception("Project doesn't exist!");
        }
      }
    }

    [WebMethod(EnableSession = true)]
    public void SavePage(int pageID, string data)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        var wtpl = unit.Scope.Repository<WebToPrintPage>().GetSingle(c => c.PageID == pageID);
        if (wtpl != null)
        {
          wtpl.Data = data;
          unit.Save();
        }
        else
        {
          throw new Exception("Page doesn't exist!");
        }
      }
    }

    [WebMethod(EnableSession = true)]
    public string GetPages(int projectID)
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      using (var unit = GetUnitOfWork())
      {
        doc.Add(new XElement("Pages", from page in unit.Scope.Repository<WebToPrintPage>().GetAll(page => page.ProjectID == projectID).ToList() select new XElement("Page", new XAttribute("ID", page.PageID), new XAttribute("Name", page.Name), new XAttribute("Data", page.Data))));
      }
      return doc.ToString();
    }

    #endregion

    #region Composites
    [WebMethod(EnableSession = true)]
    public string GetComposites(int projectID)
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      using (var unit = GetUnitOfWork())
      {
        doc.Add(new XElement("Composites", from composite in unit.Scope.Repository<WebToPrintComposite>().GetAll(composite => composite.ProjectID == projectID).ToList() select new XElement("Composite", new XAttribute("ID", composite.CompositeID), new XAttribute("Name", composite.Name), new XAttribute("Data", composite.Data))));
      }
      return doc.ToString();
    }

    [WebMethod(EnableSession = true)]
    public int CreateComposite(int projectID, string composite, string name)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        if (unit.Scope.Repository<WebToPrintComposite>().GetAllAsQueryable(c => c.Name.ToLower() == name.ToLower() && c.ProjectID == projectID).Count() > 0)
          throw new Exception("Composite name already exists!");

        WebToPrintComposite wtpc = new WebToPrintComposite() { Name = name, WebToPrintProject = unit.Scope.Repository<WebToPrintProject>().GetSingle(c => c.ProjectID == projectID), Data = composite };
        if (wtpc.WebToPrintProject != null)
        {
          try
          {
            unit.Scope.Repository<WebToPrintComposite>().Add(wtpc);
            unit.Save();
            return wtpc.CompositeID;
          }
          catch
          {
            throw new Exception("Failed to insert composite: reason unknown");
          }
        }
        else
        {
          throw new Exception("Project doesn't exist!");
        }
      }
    }

    [WebMethod(EnableSession = true)]
    public void SaveComposite(int composteID, string data)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        var wtpl = unit.Scope.Repository<WebToPrintComposite>().GetSingle(c => c.CompositeID == composteID);
        if (wtpl != null)
        {
          wtpl.Data = data;
          unit.Save();
        }
        else
        {
          throw new Exception("Composite doesn't exist!");
        }
      }
    }
    #endregion

    #region Layouts
    [WebMethod(EnableSession = true)]
    public int CreateLayout(int projectID, string data, string name, string type)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {

        WebToPrintLayout wtpl = new WebToPrintLayout()
        {
          Name = name,
          WebToPrintProject = unit.Scope.Repository<WebToPrintProject>().GetSingle(c => c.ProjectID == projectID),
          Data = data,
          LayoutType = type
        };
        if (wtpl.WebToPrintProject != null)
        {
          try
          {
            unit.Scope.Repository<WebToPrintLayout>().Add(wtpl);
            unit.Save();
            return wtpl.LayoutID;
          }
          catch
          {
            throw new Exception("Failed to insert layout: reason unknown");
          }
        }
        else
        {
          throw new Exception("Project doesn't exist!");
        }
      }
    }

    [WebMethod(EnableSession = true)]
    public void SaveLayout(int LayoutID, string data)
    {
      checkCookieAndLogin();
      using (var unit = GetUnitOfWork())
      {
        var wtpl = unit.Scope.Repository<WebToPrintLayout>().GetSingle(c => c.LayoutID == LayoutID);
        if (wtpl != null)
        {
          wtpl.Data = data;
          unit.Save();
        }
        else
        {
          throw new Exception("Layout doesn't exist!");
        }
      }
    }

    [WebMethod(EnableSession = true)]
    public string GetLayouts(int projectID)
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      using (var unit = GetUnitOfWork())
      {
        doc.Add(new XElement("Layouts", from layout in unit.Scope.Repository<WebToPrintLayout>().GetAll(layout => layout.ProjectID == projectID).ToList() select new XElement("Layout", new XAttribute("ID", layout.LayoutID), new XAttribute("Name", layout.Name), new XAttribute("Data", layout.Data), new XAttribute("Type", layout.LayoutType))));
      }
      return doc.ToString();
    }
    #endregion

    [WebMethod(EnableSession = true)]
    public string SearchProducts(string searchterms, int searchIDResult)
    {
      checkCookieAndLogin();

      XDocument doc = new XDocument();

      using (var unit = (IServiceUnitOfWork)GetUnitOfWork())
      {
        //var searchResultSet = ((IProductService)unit.Service<Product>()).SearchProducts(Client.User.LanguageID,
        //              Client.User.ConnectorID.Value,
        //                                                                        searchterms,
        //                                                                        true,
        //                                                                        true,
        //                                                                        true,
        //                                                                        true
        //                                     ).Distinct();

        LuceneIndexer indexer = new LuceneIndexer();

        var modelList = indexer.GetSearchResultsFromIndex(searchterms);

        var results = new XElement("Results");

        switch (searchIDResult)
        {
          default: // ProductID
            {
              results.Add(from p in
                            modelList.Distinct()
                          select new XElement("Product",
                            new XAttribute("ID", p.ProductID),
                            new XAttribute("Name", p.ProductName ?? ""),
                            new XAttribute("Description", p.ProductDescription ?? ""),
                            new XAttribute("ImagePath", !string.IsNullOrEmpty(p.ImagePath) ? string.Format("{0}/imagetranslate.ashx?mediapath={1}&width=128&height=96", BaseSiteUrl, p.ImagePath) : string.Empty)
                            ));
            }
            break;
          case 1: // customer item number
            {
              //TODO : This needs some SQL/LINQ guru cleaning it up
              var products = unit.Service<VendorAssortment>().GetAll();

              results.Add(from p in modelList.Distinct()
                          where p.ProductName != null && !string.IsNullOrEmpty(p.ImagePath)
                          select new XElement("Product",
                            new XAttribute("ID", products.First(c => c.ProductID == p.ProductID).CustomItemNumber),
                            new XAttribute("Name", p.ProductName),
                            new XAttribute("Description", p.ProductDescription ?? ""),
                            new XAttribute("ImagePath", string.Format("{0}/imagetranslate.ashx?mediapath={1}&width=128&height=96", BaseSiteUrl, p.ImagePath))
                            ));
            }
            break;
        }




        doc.Add(results);
      }
      return doc.ToString();
    }

    [WebMethod(EnableSession = true)]
    public string GetImages()
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      XElement elem = new XElement("Images");



      if (ConfigurationManager.AppSettings.AllKeys.Contains("DesignerImageDirectory") && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["DesignerImageDirectory"]))
      {
        string directory = ConfigurationManager.AppSettings["DesignerImageDirectory"].ToString();
        string[] files = Directory.GetFiles(directory);
        XElement directoryElement = new XElement("Category",
          from file in files
          select
            new XElement("Image",
            new XAttribute("Name", file.Substring(directory.Length + 1)),
            new XAttribute("Url", (ConfigurationManager.AppSettings["DesignerImageURL"].ToString() + directory.Substring(directory.Length) + file.Substring(directory.Length)).Replace('\\', '/')))
          ,
          new XAttribute("Name", directory.Substring(directory.Length)));

        elem.Add(directoryElement);

        string[] directories = Directory.GetDirectories(directory);
        foreach (string dir in directories)
        {

          files = Directory.GetFiles(dir);
          directoryElement = new XElement("Category",
            from file in files
            select
              new XElement("Image",
              new XAttribute("Name", file.Substring(dir.Length + 1)),
              new XAttribute("Url", (ConfigurationManager.AppSettings["DesignerImageURL"].ToString() + dir.Substring(directory.Length) + file.Substring(dir.Length)).Replace('\\', '/')))
            ,
            new XAttribute("Name", dir.Substring(directory.Length)));

          elem.Add(directoryElement);
        }

      }
      else
      {
        throw new Exception("Warning, FTP Folder Indexing support has not yet been implemented in the printservice, please edit the webconfig to use a local directory instead");
      }


      doc.Add(elem);
      return doc.ToString();
    }

    [WebMethod(EnableSession = true)]
    public string GetFTPConfig()
    {
      checkCookieAndLogin();
      XDocument doc = new XDocument();
      XElement settings = new XElement("Settings",
        new XAttribute("FTPAddress", ConfigurationManager.AppSettings["DesignerFTP"]),
        new XAttribute("Username", ConfigurationManager.AppSettings["DesignerFTPUsername"]),
        new XAttribute("Password", ConfigurationManager.AppSettings["DesignerFTPPassword"]));
      doc.Add(settings);

      return doc.ToString();
    }

    [WebMethod(EnableSession = true)]
    public string Login(string username, string password)
    {
      try
      {
        if (ConcentratorPrincipal.Login(username, password))
        {
          Session.Add("Username", username);
          Session.Add("Password", password);
          return Session.SessionID;
        }
      }
      catch
      {
        return "";
      }
      return "";
    }

    private string BaseSiteUrl
    {
      get
      {
        HttpContext context = HttpContext.Current;
        string baseUrl = context.Request.Url.Scheme + "://" + context.Request.Url.Authority + context.Request.ApplicationPath.TrimEnd('/') + '/';
        return baseUrl;
      }
    }

    private void checkCookieAndLogin()
    {
      if (Session["Username"] == null || Session["Password"] == null)
        throw new Exception("User not logged in!");

      ConcentratorPrincipal.Login((string)Session["Username"], (string)Session["Password"]);
    }
  }
}

