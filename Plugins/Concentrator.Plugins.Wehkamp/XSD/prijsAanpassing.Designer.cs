//File handmatig aangepast i.v.m. maximaal 2 decimalen in prijzen
using System.Globalization;

namespace Concentrator.Plugins.Wehkamp
{
  using System;
  using System.Diagnostics;
  using System.Xml.Serialization;
  using System.Collections;
  using System.Xml.Schema;
  using System.ComponentModel;
  using System.IO;
  using System.Text;
  using System.Collections.Generic;


  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.33440")]
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
  public partial class prijsAanpassing
  {

    private static System.Xml.Serialization.XmlSerializer serializer;

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public headerType header { get; set; }
    [System.Xml.Serialization.XmlElementAttribute("aanpassing", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<prijsAanpassingAanpassing> aanpassing { get; set; }

    public prijsAanpassing()
    {
      this.aanpassing = new List<prijsAanpassingAanpassing>();
      this.header = new headerType();
    }

    private static System.Xml.Serialization.XmlSerializer Serializer
    {
      get
      {
        if ((serializer == null))
        {
          serializer = new System.Xml.Serialization.XmlSerializer(typeof(prijsAanpassing));
        }
        return serializer;
      }
    }

    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current prijsAanpassing object into an XML document
    /// </summary>
    /// <returns>string XML value</returns>
    public virtual string Serialize()
    {
      System.IO.StreamReader streamReader = null;
      System.IO.MemoryStream memoryStream = null;
      try
      {
        memoryStream = new System.IO.MemoryStream();
        Serializer.Serialize(memoryStream, this);
        memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
        streamReader = new System.IO.StreamReader(memoryStream);
        return streamReader.ReadToEnd();
      }
      finally
      {
        if ((streamReader != null))
        {
          streamReader.Dispose();
        }
        if ((memoryStream != null))
        {
          memoryStream.Dispose();
        }
      }
    }

    /// <summary>
    /// Deserializes workflow markup into an prijsAanpassing object
    /// </summary>
    /// <param name="xml">string workflow markup to deserialize</param>
    /// <param name="obj">Output prijsAanpassing object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool Deserialize(string xml, out prijsAanpassing obj, out System.Exception exception)
    {
      exception = null;
      obj = default(prijsAanpassing);
      try
      {
        obj = Deserialize(xml);
        return true;
      }
      catch (System.Exception ex)
      {
        exception = ex;
        return false;
      }
    }

    public static bool Deserialize(string xml, out prijsAanpassing obj)
    {
      System.Exception exception = null;
      return Deserialize(xml, out obj, out exception);
    }

    public static prijsAanpassing Deserialize(string xml)
    {
      System.IO.StringReader stringReader = null;
      try
      {
        stringReader = new System.IO.StringReader(xml);
        return ((prijsAanpassing)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
      }
      finally
      {
        if ((stringReader != null))
        {
          stringReader.Dispose();
        }
      }
    }

    public static prijsAanpassing Deserialize(System.IO.Stream s)
    {
      return ((prijsAanpassing)(Serializer.Deserialize(s)));
    }

    /// <summary>
    /// Serializes current prijsAanpassing object into file
    /// </summary>
    /// <param name="fileName">full path of outupt xml file</param>
    /// <param name="exception">output Exception value if failed</param>
    /// <returns>true if can serialize and save into file; otherwise, false</returns>
    public virtual bool SaveToFile(string fileName, out System.Exception exception)
    {
      exception = null;
      try
      {
        SaveToFile(fileName);
        return true;
      }
      catch (System.Exception e)
      {
        exception = e;
        return false;
      }
    }

    public virtual void SaveToFile(string fileName)
    {
      System.IO.StreamWriter streamWriter = null;
      try
      {
        string xmlString = Serialize();
        System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
        streamWriter = xmlFile.CreateText();
        streamWriter.WriteLine(xmlString);
        streamWriter.Close();
      }
      finally
      {
        if ((streamWriter != null))
        {
          streamWriter.Dispose();
        }
      }
    }

    /// <summary>
    /// Deserializes xml markup from file into an prijsAanpassing object
    /// </summary>
    /// <param name="fileName">string xml file to load and deserialize</param>
    /// <param name="obj">Output prijsAanpassing object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool LoadFromFile(string fileName, out prijsAanpassing obj, out System.Exception exception)
    {
      exception = null;
      obj = default(prijsAanpassing);
      try
      {
        obj = LoadFromFile(fileName);
        return true;
      }
      catch (System.Exception ex)
      {
        exception = ex;
        return false;
      }
    }

    public static bool LoadFromFile(string fileName, out prijsAanpassing obj)
    {
      System.Exception exception = null;
      return LoadFromFile(fileName, out obj, out exception);
    }

    public static prijsAanpassing LoadFromFile(string fileName)
    {
      System.IO.FileStream file = null;
      System.IO.StreamReader sr = null;
      try
      {
        file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
        sr = new System.IO.StreamReader(file);
        string xmlString = sr.ReadToEnd();
        sr.Close();
        file.Close();
        return Deserialize(xmlString);
      }
      finally
      {
        if ((file != null))
        {
          file.Dispose();
        }
        if ((sr != null))
        {
          sr.Dispose();
        }
      }
    }
    #endregion
  }

  
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.33440")]
  [System.SerializableAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class prijsAanpassingAanpassing
  {

    private static System.Xml.Serialization.XmlSerializer serializer;

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string artikelNummer { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string kleurNummer { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string maat { get; set; }
    
    
    //[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlIgnore]
    public decimal verkoopPrijs { get; set; }

    [XmlElement("verkoopPrijs")]
    public string VerkoopPrijsString
    {
      get
      {
        return verkoopPrijs.ToString("F2", CultureInfo.InvariantCulture);
      }

      set
      {
        decimal amount = 0;
        if (Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
          verkoopPrijs = amount;
      }
    }
    
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date", IsNullable = true)]
    public System.Nullable<System.DateTime> beginDatum { get; set; }
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool beginDatumSpecified { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date", IsNullable = true)]
    public System.Nullable<System.DateTime> vervalDatum { get; set; }
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool vervalDatumSpecified { get; set; }

    private static System.Xml.Serialization.XmlSerializer Serializer
    {
      get
      {
        if ((serializer == null))
        {
          serializer = new System.Xml.Serialization.XmlSerializer(typeof(prijsAanpassingAanpassing));
        }
        return serializer;
      }
    }

    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current prijsAanpassingAanpassing object into an XML document
    /// </summary>
    /// <returns>string XML value</returns>
    public virtual string Serialize()
    {
      System.IO.StreamReader streamReader = null;
      System.IO.MemoryStream memoryStream = null;
      try
      {
        memoryStream = new System.IO.MemoryStream();
        Serializer.Serialize(memoryStream, this);
        memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
        streamReader = new System.IO.StreamReader(memoryStream);
        return streamReader.ReadToEnd();
      }
      finally
      {
        if ((streamReader != null))
        {
          streamReader.Dispose();
        }
        if ((memoryStream != null))
        {
          memoryStream.Dispose();
        }
      }
    }

    /// <summary>
    /// Deserializes workflow markup into an prijsAanpassingAanpassing object
    /// </summary>
    /// <param name="xml">string workflow markup to deserialize</param>
    /// <param name="obj">Output prijsAanpassingAanpassing object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool Deserialize(string xml, out prijsAanpassingAanpassing obj, out System.Exception exception)
    {
      exception = null;
      obj = default(prijsAanpassingAanpassing);
      try
      {
        obj = Deserialize(xml);
        return true;
      }
      catch (System.Exception ex)
      {
        exception = ex;
        return false;
      }
    }

    public static bool Deserialize(string xml, out prijsAanpassingAanpassing obj)
    {
      System.Exception exception = null;
      return Deserialize(xml, out obj, out exception);
    }

    public static prijsAanpassingAanpassing Deserialize(string xml)
    {
      System.IO.StringReader stringReader = null;
      try
      {
        stringReader = new System.IO.StringReader(xml);
        return ((prijsAanpassingAanpassing)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
      }
      finally
      {
        if ((stringReader != null))
        {
          stringReader.Dispose();
        }
      }
    }

    public static prijsAanpassingAanpassing Deserialize(System.IO.Stream s)
    {
      return ((prijsAanpassingAanpassing)(Serializer.Deserialize(s)));
    }

    /// <summary>
    /// Serializes current prijsAanpassingAanpassing object into file
    /// </summary>
    /// <param name="fileName">full path of outupt xml file</param>
    /// <param name="exception">output Exception value if failed</param>
    /// <returns>true if can serialize and save into file; otherwise, false</returns>
    public virtual bool SaveToFile(string fileName, out System.Exception exception)
    {
      exception = null;
      try
      {
        SaveToFile(fileName);
        return true;
      }
      catch (System.Exception e)
      {
        exception = e;
        return false;
      }
    }

    public virtual void SaveToFile(string fileName)
    {
      System.IO.StreamWriter streamWriter = null;
      try
      {
        string xmlString = Serialize();
        System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
        streamWriter = xmlFile.CreateText();
        streamWriter.WriteLine(xmlString);
        streamWriter.Close();
      }
      finally
      {
        if ((streamWriter != null))
        {
          streamWriter.Dispose();
        }
      }
    }

    /// <summary>
    /// Deserializes xml markup from file into an prijsAanpassingAanpassing object
    /// </summary>
    /// <param name="fileName">string xml file to load and deserialize</param>
    /// <param name="obj">Output prijsAanpassingAanpassing object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool LoadFromFile(string fileName, out prijsAanpassingAanpassing obj, out System.Exception exception)
    {
      exception = null;
      obj = default(prijsAanpassingAanpassing);
      try
      {
        obj = LoadFromFile(fileName);
        return true;
      }
      catch (System.Exception ex)
      {
        exception = ex;
        return false;
      }
    }

    public static bool LoadFromFile(string fileName, out prijsAanpassingAanpassing obj)
    {
      System.Exception exception = null;
      return LoadFromFile(fileName, out obj, out exception);
    }

    public static prijsAanpassingAanpassing LoadFromFile(string fileName)
    {
      System.IO.FileStream file = null;
      System.IO.StreamReader sr = null;
      try
      {
        file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
        sr = new System.IO.StreamReader(file);
        string xmlString = sr.ReadToEnd();
        sr.Close();
        file.Close();
        return Deserialize(xmlString);
      }
      finally
      {
        if ((file != null))
        {
          file.Dispose();
        }
        if ((sr != null))
        {
          sr.Dispose();
        }
      }
    }
    #endregion
  }
}
