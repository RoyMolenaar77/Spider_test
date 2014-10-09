using System;
using System.Xml;
using System.Xml.Schema;

namespace System.Xml.Linq
{
  public static class XDocumentExtensions
  {
    public static Boolean ValidateForErrors ( this XDocument document , XmlSchema schema )
    {
      document.ThrowIfNull ( "document" );
      schema.ThrowArgNull ( "schema" );

      var isValid = true;

      ValidationEventHandler handler = ( sender , arguments ) =>
      {
        if ( arguments.Severity == XmlSeverityType.Error )
        {
          isValid = false;
        }
      };

      var schemaSet = new XmlSchemaSet ( );

      schemaSet.Add ( schema );

      document.Validate ( schemaSet , handler );

      return isValid;
    }

    public static Boolean ValidateForErrorsAndWarnings ( this XDocument document , XmlSchema schema )
    {
      document.ThrowIfNull ( "document" );
      schema.ThrowArgNull ( "schema" );

      var isValid = true;

      ValidationEventHandler handler = ( sender , arguments ) =>
      {
        if ( arguments.Severity == XmlSeverityType.Error || arguments.Severity == XmlSeverityType.Warning )
        {
          isValid = false;
        }
      };

      var schemaSet = new XmlSchemaSet ( );

      schemaSet.Add ( schema );

      document.Validate ( schemaSet , handler );

      return isValid;
    }
  }
}
