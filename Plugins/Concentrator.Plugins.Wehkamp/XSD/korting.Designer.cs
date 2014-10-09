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
  public partial class kortingActie
  {

    private static System.Xml.Serialization.XmlSerializer serializer;

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public headerType header { get; set; }
    [System.Xml.Serialization.XmlElementAttribute("korting", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public List<kortingActieKorting> korting { get; set; }

    public kortingActie()
    {
      this.korting = new List<kortingActieKorting>();
      this.header = new headerType();
    }

    private static System.Xml.Serialization.XmlSerializer Serializer
    {
      get
      {
        if ((serializer == null))
        {
          serializer = new System.Xml.Serialization.XmlSerializer(typeof(kortingActie));
        }
        return serializer;
      }
    }

    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current kortingActie object into an XML document
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
    /// Deserializes workflow markup into an kortingActie object
    /// </summary>
    /// <param name="xml">string workflow markup to deserialize</param>
    /// <param name="obj">Output kortingActie object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool Deserialize(string xml, out kortingActie obj, out System.Exception exception)
    {
      exception = null;
      obj = default(kortingActie);
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

    public static bool Deserialize(string xml, out kortingActie obj)
    {
      System.Exception exception = null;
      return Deserialize(xml, out obj, out exception);
    }

    public static kortingActie Deserialize(string xml)
    {
      System.IO.StringReader stringReader = null;
      try
      {
        stringReader = new System.IO.StringReader(xml);
        return ((kortingActie)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
      }
      finally
      {
        if ((stringReader != null))
        {
          stringReader.Dispose();
        }
      }
    }

    public static kortingActie Deserialize(System.IO.Stream s)
    {
      return ((kortingActie)(Serializer.Deserialize(s)));
    }

    /// <summary>
    /// Serializes current kortingActie object into file
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
    /// Deserializes xml markup from file into an kortingActie object
    /// </summary>
    /// <param name="fileName">string xml file to load and deserialize</param>
    /// <param name="obj">Output kortingActie object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool LoadFromFile(string fileName, out kortingActie obj, out System.Exception exception)
    {
      exception = null;
      obj = default(kortingActie);
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

    public static bool LoadFromFile(string fileName, out kortingActie obj)
    {
      System.Exception exception = null;
      return LoadFromFile(fileName, out obj, out exception);
    }

    public static kortingActie LoadFromFile(string fileName)
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
  public partial class kortingActieKorting
  {

    private static System.Xml.Serialization.XmlSerializer serializer;

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string actieOmschrijving { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "positiveInteger")]
    public string kortingActie { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
    public System.DateTime actieBeginDatum { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
    public System.DateTime actieEindDatum { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date", IsNullable = true)]
    public System.Nullable<System.DateTime> actieVervalDatum { get; set; }
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool actieVervalDatumSpecified { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string kortingType { get; set; }
    
    
    //[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlIgnore]
    public decimal kortingWaarde { get; set; }

    [XmlElement("kortingWaarde")]
    public string KortingWaardeString
    {
      get
      {
        return kortingWaarde.ToString("F2", CultureInfo.InvariantCulture);
      }

      set
      {
        decimal amount = 0;
        if (Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
          kortingWaarde = amount;
      }
    }
    
    [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [System.Xml.Serialization.XmlArrayItemAttribute("artikel", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public List<kortingActieKortingArtikel> artikellijst { get; set; }

    public kortingActieKorting()
    {
      this.artikellijst = new List<kortingActieKortingArtikel>();
    }

    private static System.Xml.Serialization.XmlSerializer Serializer
    {
      get
      {
        if ((serializer == null))
        {
          serializer = new System.Xml.Serialization.XmlSerializer(typeof(kortingActieKorting));
        }
        return serializer;
      }
    }

    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current kortingActieKorting object into an XML document
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
    /// Deserializes workflow markup into an kortingActieKorting object
    /// </summary>
    /// <param name="xml">string workflow markup to deserialize</param>
    /// <param name="obj">Output kortingActieKorting object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool Deserialize(string xml, out kortingActieKorting obj, out System.Exception exception)
    {
      exception = null;
      obj = default(kortingActieKorting);
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

    public static bool Deserialize(string xml, out kortingActieKorting obj)
    {
      System.Exception exception = null;
      return Deserialize(xml, out obj, out exception);
    }

    public static kortingActieKorting Deserialize(string xml)
    {
      System.IO.StringReader stringReader = null;
      try
      {
        stringReader = new System.IO.StringReader(xml);
        return ((kortingActieKorting)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
      }
      finally
      {
        if ((stringReader != null))
        {
          stringReader.Dispose();
        }
      }
    }

    public static kortingActieKorting Deserialize(System.IO.Stream s)
    {
      return ((kortingActieKorting)(Serializer.Deserialize(s)));
    }

    /// <summary>
    /// Serializes current kortingActieKorting object into file
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
    /// Deserializes xml markup from file into an kortingActieKorting object
    /// </summary>
    /// <param name="fileName">string xml file to load and deserialize</param>
    /// <param name="obj">Output kortingActieKorting object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool LoadFromFile(string fileName, out kortingActieKorting obj, out System.Exception exception)
    {
      exception = null;
      obj = default(kortingActieKorting);
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

    public static bool LoadFromFile(string fileName, out kortingActieKorting obj)
    {
      System.Exception exception = null;
      return LoadFromFile(fileName, out obj, out exception);
    }

    public static kortingActieKorting LoadFromFile(string fileName)
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
  public partial class kortingActieKortingArtikel
  {

    private static System.Xml.Serialization.XmlSerializer serializer;

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string artikelNummer { get; set; }
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string kleurNummer { get; set; }
    
    
    //[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [XmlIgnore]
    public decimal actiePrijs { get; set; }

    [XmlElement("actiePrijs")]
    public string ActiePrijsString
    {
      get
      {
        return actiePrijs.ToString("F2", CultureInfo.InvariantCulture);
      }

      set
      {
        decimal amount = 0;
        if (Decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out amount))
          actiePrijs = amount;
      }
    }
    
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date", IsNullable = true)]
    public System.Nullable<System.DateTime> kortingVervalDatum { get; set; }
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool kortingVervalDatumSpecified { get; set; }

    private static System.Xml.Serialization.XmlSerializer Serializer
    {
      get
      {
        if ((serializer == null))
        {
          serializer = new System.Xml.Serialization.XmlSerializer(typeof(kortingActieKortingArtikel));
        }
        return serializer;
      }
    }

    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current kortingActieKortingArtikel object into an XML document
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
    /// Deserializes workflow markup into an kortingActieKortingArtikel object
    /// </summary>
    /// <param name="xml">string workflow markup to deserialize</param>
    /// <param name="obj">Output kortingActieKortingArtikel object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool Deserialize(string xml, out kortingActieKortingArtikel obj, out System.Exception exception)
    {
      exception = null;
      obj = default(kortingActieKortingArtikel);
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

    public static bool Deserialize(string xml, out kortingActieKortingArtikel obj)
    {
      System.Exception exception = null;
      return Deserialize(xml, out obj, out exception);
    }

    public static kortingActieKortingArtikel Deserialize(string xml)
    {
      System.IO.StringReader stringReader = null;
      try
      {
        stringReader = new System.IO.StringReader(xml);
        return ((kortingActieKortingArtikel)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
      }
      finally
      {
        if ((stringReader != null))
        {
          stringReader.Dispose();
        }
      }
    }

    public static kortingActieKortingArtikel Deserialize(System.IO.Stream s)
    {
      return ((kortingActieKortingArtikel)(Serializer.Deserialize(s)));
    }

    /// <summary>
    /// Serializes current kortingActieKortingArtikel object into file
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
    /// Deserializes xml markup from file into an kortingActieKortingArtikel object
    /// </summary>
    /// <param name="fileName">string xml file to load and deserialize</param>
    /// <param name="obj">Output kortingActieKortingArtikel object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
    public static bool LoadFromFile(string fileName, out kortingActieKortingArtikel obj, out System.Exception exception)
    {
      exception = null;
      obj = default(kortingActieKortingArtikel);
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

    public static bool LoadFromFile(string fileName, out kortingActieKortingArtikel obj)
    {
      System.Exception exception = null;
      return LoadFromFile(fileName, out obj, out exception);
    }

    public static kortingActieKortingArtikel LoadFromFile(string fileName)
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
