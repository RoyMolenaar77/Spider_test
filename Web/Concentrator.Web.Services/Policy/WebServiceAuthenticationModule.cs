using System;
using System.Web;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Web.Services.Protocols;
using System.Security.Principal;

namespace Concentrator.Web.Services.Policy
{
  public delegate void WebServiceAuthenticationEventHandler(Object sender, WebServiceAuthenticationEvent e);

  public sealed class WebServiceAuthenticationModule : IHttpModule
  {
    private WebServiceAuthenticationEventHandler
                  _eventHandler = null;

    public event WebServiceAuthenticationEventHandler Authenticate
    {
      add { _eventHandler += value; }
      remove { _eventHandler -= value; }
    }

    public void Dispose()
    {
    }

    public void Init(HttpApplication app)
    {
      app.AuthenticateRequest += new
                 EventHandler(this.OnEnter);
    }

    private void OnAuthenticate(WebServiceAuthenticationEvent e)
    {
      if (_eventHandler == null)
        return;

      _eventHandler(this, e);
      if (e.User != null)
        e.Context.User = e.Principal;
    }

    public string ModuleName
    {
      get { return "WebServiceAuthentication"; }
    }

    void OnEnter(Object source, EventArgs eventArgs)
    {
      HttpApplication app = (HttpApplication)source;
      HttpContext context = app.Context;
      Stream HttpStream = context.Request.InputStream;

      // Save the current position of stream.
      long posStream = HttpStream.Position;

      // If the request contains an HTTP_SOAPACTION 
      // header, look at this message.
      if (context.Request.ServerVariables["HTTP_SOAPACTION"]
                     == null)
        return;

      // Load the body of the HTTP message
      // into an XML document.
      XmlDocument dom = new XmlDocument();
      string soapUser;
      string soapPassword;

      try
      {
        dom.Load(HttpStream);

        // Reset the stream position.
        HttpStream.Position = posStream;

        // Bind to the Authentication header.
        soapUser =
            dom.GetElementsByTagName("User").Item(0).InnerText;
        soapPassword =
            dom.GetElementsByTagName("Password").Item(0).InnerText;
      }
      catch (Exception e)
      {
        // Reset the position of stream.
        HttpStream.Position = posStream;

        // Throw a SOAP exception.
        XmlQualifiedName name = new
                     XmlQualifiedName("Load");
        SoapException soapException = new SoapException(
                  "Unable to read SOAP request", name, e);
        throw soapException;
      }

      // Raise the custom global.asax event.
      OnAuthenticate(new WebServiceAuthenticationEvent
                   (context, soapUser, soapPassword));
      return;
    }
  }

    public class WebServiceAuthenticationEvent : EventArgs
    {
      private System.Security.Principal.IPrincipal _IPrincipalUser;
      private HttpContext _Context;
      private string _User;
      private string _Password;

      public WebServiceAuthenticationEvent(HttpContext context)
      {
        _Context = context;
      }

      public WebServiceAuthenticationEvent(HttpContext context,
                      string user, string password)
      {
        _Context = context;
        _User = user;
        _Password = password;
      }
      public HttpContext Context
      {
        get { return _Context; }
      }
      public IPrincipal Principal
      {
        get { return _IPrincipalUser; }
        set { _IPrincipalUser = value; }
      }
      public void Authenticate()
      {
        GenericIdentity i = new GenericIdentity(User);
        this.Principal = new GenericPrincipal(i, new String[0]);
      }
      public void Authenticate(string[] roles)
      {
        GenericIdentity i = new GenericIdentity(User);
        this.Principal = new GenericPrincipal(i, roles);
      }
      public string User
      {
        get { return _User; }
        set { _User = value; }
      }
      public string Password
      {
        get { return _Password; }
        set { _Password = value; }
      }
      public bool HasCredentials
      {
        get
        {
          if ((_User == null) || (_Password == null))
            return false;
          return true;
        }
      }
    }
}