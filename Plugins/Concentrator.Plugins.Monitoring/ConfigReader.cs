using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Concentrator.Plugins.Monitoring
{
  public class TestCaseSection : ConfigurationSection
  {

    [ConfigurationProperty("", IsDefaultCollection = true)]
    public TestCaseCollection TestCases
    {
      get
      {
        TestCaseCollection hostCollection = (TestCaseCollection)base[""];
        return hostCollection;
      }
    }
  }

  public class TestCaseCollection : ConfigurationElementCollection
  {
    public TestCaseCollection()
    {
      TestCaseConfigElement details = (TestCaseConfigElement)CreateNewElement();
      if (details.Name != "")
      {
        Add(details);
      }
    }

    public override ConfigurationElementCollectionType CollectionType
    {
      get
      {
        return ConfigurationElementCollectionType.BasicMap;
      }
    }

    protected override ConfigurationElement CreateNewElement()
    {
      return new TestCaseConfigElement();
    }

    protected override Object GetElementKey(ConfigurationElement element)
    {
      return ((TestCaseConfigElement)element).Name;
    }

    public TestCaseConfigElement this[int index]
    {
      get
      {
        return (TestCaseConfigElement)BaseGet(index);
      }
      set
      {
        if (BaseGet(index) != null)
        {
          BaseRemoveAt(index);
        }
        BaseAdd(index, value);
      }
    }

    new public TestCaseConfigElement this[string name]
    {
      get
      {
        return (TestCaseConfigElement)BaseGet(name);
      }
    }

    public int IndexOf(TestCaseConfigElement details)
    {
      return BaseIndexOf(details);
    }

    public void Add(TestCaseConfigElement details)
    {
      BaseAdd(details);
    }
    protected override void BaseAdd(ConfigurationElement element)
    {
      BaseAdd(element, false);
    }

    public void Remove(TestCaseConfigElement details)
    {
      if (BaseIndexOf(details) >= 0)
        BaseRemove(details.Name);
    }

    public void RemoveAt(int index)
    {
      BaseRemoveAt(index);
    }

    public void Remove(string name)
    {
      BaseRemove(name);
    }

    public void Clear()
    {
      BaseClear();
    }

    protected override string ElementName
    {
      get { return "testcase"; }
    }
  }

  public class TestCaseConfigElement : ConfigurationElement
  {

    [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
    public string Name
    {
      get { return (string)this["name"]; }
      set { this["name"] = value; }
    }

    [ConfigurationProperty("queries", IsDefaultCollection = false)]
    public QueryCollection Queries
    {
      get { return (QueryCollection)base["queries"]; }
    }
  }

  public class QueryCollection : ConfigurationElementCollection
  {
    public new QueryConfigElement this[string name]
    {
      get
      {
        if (IndexOf(name) < 0) return null;

        return (QueryConfigElement)BaseGet(name);
      }
    }

    public QueryConfigElement this[int index]
    {
      get { return (QueryConfigElement)BaseGet(index); }
    }

    public int IndexOf(string name)
    {
      name = name.ToLower();

      for (int idx = 0; idx < base.Count; idx++)
      {
        if (this[idx].Name.ToLower() == name)
          return idx;
      }
      return -1;
    }

    public override ConfigurationElementCollectionType CollectionType
    {
      get { return ConfigurationElementCollectionType.BasicMap; }
    }

    protected override ConfigurationElement CreateNewElement()
    {
      return new QueryConfigElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((QueryConfigElement)element).Name;
    }

    protected override string ElementName
    {
      get { return "query"; }
    }
  }

  public class QueryConfigElement : ConfigurationElement
  {
    public QueryConfigElement()
    {
    }

    public QueryConfigElement(string name)
    {
      this.Name = name;
    }

    [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue = "")]
    public string Name
    {
      get { return (string)this["name"]; }
      set { this["name"] = value; }
    }

    [ConfigurationProperty("validationType", IsRequired = true)]
    [StringValidator(InvalidCharacters = "  ~!@#$%^&*()[]{}/;’\"|\\")]
    public string validationType
    {
      get { return (string)this["validationType"]; }
      set { this["validationType"] = value; }
    }

    [ConfigurationProperty("errorMessage", IsRequired = false)]
    public string ErrorMessage
    {
      get { return (string)this["errorMessage"]; }
      set { this["errorMessage"] = value; }
    }

    [ConfigurationProperty("minValue", IsRequired = false, DefaultValue = -1)]
    [IntegerValidator(MinValue = -1, MaxValue = 65536)]
    public int minValue
    {
      get { return (int)this["minValue"]; }
      set { this["minValue"] = value; }
    }

    [ConfigurationProperty("maxValue", IsRequired = false, DefaultValue = -1)]
    [IntegerValidator(MinValue = -1, MaxValue = 65536)]
    public int maxValue
    {
      get { return (int)this["maxValue"]; }
      set { this["maxValue"] = value; }
    }

    [ConfigurationProperty("exactValue", IsRequired = false)]
    public string exactValue
    {
      get { return (string)this["exactValue"]; }
      set { this["exactValue"] = value; }
    }

    public string Query { get; set; }

    protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
    {
      //Extract CDATA, serializer will crash if this is not set empty
      XElement x = XElement.Parse(reader.ReadOuterXml());
      Query = x.Value;
      x.SetValue("");

      StringReader s = new StringReader(x.ToString());
      XmlReader r = XmlReader.Create(s, new XmlReaderSettings() { ValidationType = System.Xml.ValidationType.None });
      r.Read();
      
      base.DeserializeElement(r, serializeCollectionKey);
    }
  }
}
