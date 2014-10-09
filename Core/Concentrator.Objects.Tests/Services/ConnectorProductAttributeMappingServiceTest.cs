using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using Xunit;

namespace Concentrator.Objects.Services
{
  using Contracts;
  using DataAccess;
  using DataAccess.Repository;
  using Enumerations;
  using Models;
  using Models.Attributes;
  using Models.Connectors;

  public class ConnectorProductAttributeMappingServiceTest
  {
    private const string DisplayName = "Connector Product Attribute Mapping Service: ";


    #region Repository Mock-ups
    
    private Mock<IRepository<Connector>> ConnectorRepositoryMock
    {
      get;
      set;
    }
    
    private Mock<IConnectorProductAttributeRepository> ConnectorProductAttributeRepositoryMock
    {
      get;
      set;
    }
    
    private Mock<IConnectorProductAttributeSettingRepository> ConnectorProductAttributeSettingRepositoryMock
    {
      get;
      set;
    }

    private Mock<IConnectorProductAttributeSettingTemplateRepository> ConnectorProductAttributeSettingTemplateRepositoryMock
    {
      get;
      set;
    }

    private Mock<IRepository<ProductAttributeMetaData>> ProductAttributeRepositoryMock
    {
      get;
      set;
    }

    private void CreateMocks()
    {
      ConnectorRepositoryMock = new Mock<IRepository<Connector>>();
      ConnectorProductAttributeRepositoryMock = new Mock<IConnectorProductAttributeRepository>();
      ConnectorProductAttributeSettingRepositoryMock = new Mock<IConnectorProductAttributeSettingRepository>();
      ConnectorProductAttributeSettingTemplateRepositoryMock = new Mock<IConnectorProductAttributeSettingTemplateRepository>();
      ProductAttributeRepositoryMock = new Mock<IRepository<ProductAttributeMetaData>>();
    }

    private const Int32 TestConnectorID = 1;
    private const String TestConnectorName = "Test";

    private const Int32 TestConnectorSystemID = 1;
    private const String TestConnectorSystemName = "Default";

    private const Int32 TestConnectorProductAttributeID = 1;

    private const String TestProductAttributeCode = "Test";
    private const Int32 TestProductAttributeID = 1;

    private void CreateMocksWithTestSetup()
    {
      CreateMocks();

      ConnectorRepositoryMock
        .Setup(repository => repository.GetSingle(connector => connector.ConnectorID == TestConnectorID))
        .Returns(new Connector
        {
          ConnectorID = TestConnectorID,
          ConnectorSystemID = TestConnectorSystemID
        });
      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetMultiple(It.IsAny<Int32?>(), It.IsAny<Int32?>(), It.IsAny<Boolean?>()))
        .Returns(Enumerable.Empty<ConnectorProductAttribute>());
      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetSingle(It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Boolean>()))
        .Returns(default(ConnectorProductAttribute));

      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.GetMultiple(TestConnectorProductAttributeID, null))
        .Returns(Enumerable.Empty<ConnectorProductAttributeSetting>());
      
      ConnectorProductAttributeSettingTemplateRepositoryMock
        .Setup(repository => repository.GetMultiple(TestConnectorSystemID, null))
        .Returns(Enumerable.Empty<ConnectorProductAttributeSettingTemplate>());
      
      ProductAttributeRepositoryMock
        .Setup(repository => repository.GetSingle(attribute => attribute.AttributeID == TestProductAttributeID))
        .Returns(new ProductAttributeMetaData
        {
          AttributeCode = TestProductAttributeCode,
          AttributeID = TestProductAttributeID
        });
    }

    #endregion

    private ConnectorProductAttributeMappingService CreateService()
    {
      return new ConnectorProductAttributeMappingService(
        ConnectorRepositoryMock.Object,
        ConnectorProductAttributeRepositoryMock.Object,
        ConnectorProductAttributeSettingRepositoryMock.Object,
        ConnectorProductAttributeSettingTemplateRepositoryMock.Object,
        ProductAttributeRepositoryMock.Object);
    }

    [Fact(DisplayName = DisplayName + "Initialization with valid arguments")]
    public void InitializationWithValidArguments()
    {
      CreateMocks();
      CreateService();
    }

    [Fact(DisplayName = DisplayName + "Initialization with null parameters")]
    public void InitializationWithNullArguments()
    {
      Assert.Throws<ArgumentNullException>(delegate
      {
        new ConnectorProductAttributeMappingService(null, null, null, null, null);
      });
    }

    [Fact(DisplayName = DisplayName + "Get single with multiple settings")]
    public void GetSingleWithSettings()
    {
      CreateMocks();

      var defaultValue = 42;
      var isFilter = false;

      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetMultiple(TestConnectorID, TestConnectorProductAttributeID, false))
        .Returns(new[]
        {
          new ConnectorProductAttribute
          {
            ConnectorID = TestConnectorID,
            ConnectorProductAttributeID = TestConnectorProductAttributeID,
            DefaultValue = defaultValue.ToString(),
            IsFilter = isFilter,
            ProductAttributeID = TestProductAttributeID,
            ProductAttributeType = defaultValue.GetType().FullName
          }
        });

      var properties = new Dictionary<String, Object>
      {
        { "Property 1", true    },
        { "Property 2", '@'     },
        { "Property 3", Math.PI },
      };

      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.GetMultiple(1, null))
        .Returns(properties.Select(property => new ConnectorProductAttributeSetting
          {
            ConnectorProductAttributeID = TestConnectorProductAttributeID,
            Code = property.Key,
            Type = property.Value.GetType().FullName,
            Value = property.Value.ToString()
          }));

      var mappings = CreateService()
        .Get(TestConnectorID, TestConnectorProductAttributeID, false)
        .ToArray();

      Assert.Equal(mappings.Length, 1);

      if (mappings.Any())
      {
        var mapping = mappings.First();

        Assert.Equal(mapping.IsFilter, isFilter);
        Assert.Equal(mapping.ProductAttributeType, defaultValue.GetType());
        Assert.Equal(mapping.DefaultValue, defaultValue);

        foreach (var property in properties)
        {
          Assert.True(mapping.Properties.ContainsKey(property.Key));

          if (mapping.Properties[property.Key].GetType() == typeof(Double))
          {
            Assert.Equal(mapping.Properties[property.Key].ToString(), property.Value.ToString());
          }
          else
          {
            Assert.Equal(mapping.Properties[property.Key], property.Value);
          }
        }
      }
    }

    [Fact(DisplayName = DisplayName + "Get multiple mappings with same attribute and different IsFilter-values")]
    public void GetMultipleMappingsWithSameAttributeWithDifferentIsFilterValues()
    {
      CreateMocksWithTestSetup();

      var attributes = new[] 
      { 
        new { AttributeID = TestProductAttributeID, DefaultValue = 5000M, IsFilter = false  },
        new { AttributeID = TestProductAttributeID, DefaultValue = 500M,  IsFilter = true   },
      };

      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetMultiple(TestConnectorID, TestConnectorProductAttributeID, false))
        .Returns(attributes.Select(attribute => new ConnectorProductAttribute
        {
          ConnectorID                 = TestConnectorID,
          ConnectorProductAttributeID = TestConnectorProductAttributeID,
          DefaultValue                = attribute.DefaultValue.ToString(),
          IsFilter                    = attribute.IsFilter,
          ProductAttributeID          = attribute.AttributeID,
          ProductAttributeType        = attribute.DefaultValue.GetType().FullName
        }));

      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.GetMultiple(1, null))
        .Returns(Enumerable.Empty<ConnectorProductAttributeSetting>());

      var mappings = CreateService()
        .Get(TestConnectorID, TestConnectorProductAttributeID, false)
        .ToArray();

      Assert.Equal(mappings.Length, attributes.Length);

      for (var index = 0; index < attributes.Length; index++)
      {
        var attribute = attributes[index];
        var mapping = mappings[index];

        Assert.Equal(mapping.IsFilter, attribute.IsFilter);
        Assert.Equal(mapping.ProductAttributeType, attribute.DefaultValue.GetType());
        Assert.Equal(mapping.DefaultValue, attribute.DefaultValue);
      }
    }

    [Fact(DisplayName = DisplayName + "Create mapping with settings and with setting templates")]
    public void CreateMappingWithSettingsWithSettingTemplates()
    {
      CreateMocksWithTestSetup();

      var defaultValue = 42;
      var isFilter = false;

      var connectorProductAttribute = new ConnectorProductAttribute
      {
        ConnectorID = TestConnectorID,
        ConnectorProductAttributeID = TestConnectorProductAttributeID,
        DefaultValue = defaultValue.ToString(),
        IsFilter = isFilter,
        ProductAttributeID = TestProductAttributeID,
        ProductAttributeType = defaultValue.GetType().FullName
      };
      
      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetSingle(It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Boolean>()))
        .Returns(connectorProductAttribute);

      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetMultiple(It.IsAny<Int32?>(), It.IsAny<Int32?>(), It.IsAny<Boolean?>()))
        .Returns(new[] 
        { 
          connectorProductAttribute 
        });

      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.Upsert(It.IsAny<ConnectorProductAttribute>()))
        .Returns(RepositoryOperationStatus.Created);
      
      var propertySettings = new Dictionary<String, Object>
      {
        { "Property 1", true },
        { "Property 2", true },
      };
      
      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.GetMultiple(null, null))
        .Returns(propertySettings.Select(propertySetting => new ConnectorProductAttributeSetting
        {
          ConnectorProductAttributeID = TestConnectorProductAttributeID,
          Code = propertySetting.Key,
          Type = propertySetting.Value.GetType().FullName,
          Value = propertySetting.Value.ToString()
        }));
      
      var propertySettingTemplates = new Dictionary<String, Object>
      {
        { "Property 2", false },
        { "Property 3", false }
      };
      
      ConnectorProductAttributeSettingTemplateRepositoryMock
        .Setup(repository => repository.GetMultiple(TestConnectorSystemID, null))
        .Returns(propertySettingTemplates.Select(propertySettingTemplate => new ConnectorProductAttributeSettingTemplate
        {
          ConnectorSystemID = TestConnectorSystemID,
          Code = propertySettingTemplate.Key,
          Type = propertySettingTemplate.Value.GetType().FullName,
          Value = propertySettingTemplate.Value.ToString()
        }));

      var finalProperties = new Dictionary<String, Object>(propertySettings);

      foreach (var propertySettingTemplate in propertySettingTemplates)
      {
        if (!finalProperties.ContainsKey(propertySettingTemplate.Key))
        {
          finalProperties.Add(propertySettingTemplate.Key, propertySettingTemplate.Value);
        }
      }
      
      var connectorProductAttributeSettingList = new List<ConnectorProductAttributeSetting>();

      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.CreateOrUpdate(It.IsAny<ConnectorProductAttributeSetting>()))
        .Callback<ConnectorProductAttributeSetting>(setting => connectorProductAttributeSettingList.Add(setting));

      var mapping = new ConnectorProductAttributeMapping(propertySettings)
      {
        Connector = new Connector
        {
          ConnectorID = TestConnectorID,
          ConnectorSystemID = TestConnectorSystemID
        },
        DefaultValue = defaultValue,
        IsFilter = isFilter,
        ProductAttribute = new ProductAttributeMetaData
        {
          AttributeID = TestProductAttributeID
        },
        ProductAttributeType = defaultValue.GetType()
      };

      CreateService().Upsert(mapping);

      ConnectorProductAttributeSettingRepositoryMock.Verify(repository => repository.CreateOrUpdate(It.IsAny<ConnectorProductAttributeSetting>()), Times.AtLeast(finalProperties.Count));

      var connectorProductAttributeSettingDictionary = connectorProductAttributeSettingList.ToDictionary(setting => setting.Code);

      foreach (var finalProperty in finalProperties)
      {
        Assert.True(mapping.Properties.ContainsKey(finalProperty.Key));
        Assert.Equal(mapping.Properties[finalProperty.Key], finalProperty.Value);

        Assert.True(connectorProductAttributeSettingDictionary.ContainsKey(finalProperty.Key));
        Assert.Equal(connectorProductAttributeSettingDictionary[finalProperty.Key].Value, finalProperty.Value.ToString()); 
      }
    }

    [Fact(DisplayName = DisplayName + "Update mapping with settings and with setting templates")]
    public void UpdateMappingWithSettings()
    {
      CreateMocksWithTestSetup();

      var defaultValue = 42;
      var isFilter = false;

      var connectorProductAttribute = new ConnectorProductAttribute
      {
        ConnectorID = TestConnectorID,
        ConnectorProductAttributeID = TestConnectorProductAttributeID,
        DefaultValue = defaultValue.ToString(),
        IsFilter = isFilter,
        ProductAttributeID = TestProductAttributeID,
        ProductAttributeType = defaultValue.GetType().FullName
      };

      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetSingle(It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Boolean>()))
        .Returns(connectorProductAttribute);

      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.GetMultiple(It.IsAny<Int32?>(), It.IsAny<Int32?>(), It.IsAny<Boolean?>()))
        .Returns(new[] 
        { 
          connectorProductAttribute 
        });
      
      ConnectorProductAttributeRepositoryMock
        .Setup(repository => repository.Upsert(It.IsAny<ConnectorProductAttribute>()))
        .Returns(RepositoryOperationStatus.Updated);

      var propertySettings = new Dictionary<String, Object>
      {
        { "Property 1", true  },
        { "Property 2", false },
        { "Property 3", true  },
      };
      
      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.GetMultiple(TestConnectorProductAttributeID, null))
        .Returns(propertySettings.Skip(1).Select(propertySetting => new ConnectorProductAttributeSetting
        {
          ConnectorProductAttributeID = TestConnectorProductAttributeID,
          Code = propertySetting.Key,
          Type = propertySetting.Value.GetType().FullName,
          Value = propertySetting.Value.ToString()
        }));

      var connectorProductAttributeSettingDeleteDictionary = new Dictionary<String, ConnectorProductAttributeSetting>();

      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.Delete(It.IsAny<ConnectorProductAttributeSetting>()))
        .Callback<ConnectorProductAttributeSetting>(setting => connectorProductAttributeSettingDeleteDictionary[setting.Code] = setting);

      var connectorProductAttributeSettingUpsertDictionary = new Dictionary<String, ConnectorProductAttributeSetting>();

      ConnectorProductAttributeSettingRepositoryMock
        .Setup(repository => repository.CreateOrUpdate(It.IsAny<ConnectorProductAttributeSetting>()))
        .Callback<ConnectorProductAttributeSetting>(setting => connectorProductAttributeSettingUpsertDictionary[setting.Code] = setting);

      var mapping = new ConnectorProductAttributeMapping(propertySettings)
      {
        Connector = new Connector
        {
          ConnectorID = TestConnectorID,
          ConnectorSystemID = TestConnectorSystemID
        },
        DefaultValue = defaultValue,
        IsFilter = isFilter,
        ProductAttribute = new ProductAttributeMetaData
        {
          AttributeID = TestProductAttributeID
        },
        ProductAttributeType = defaultValue.GetType()
      };

      mapping.Properties.Remove("Property 3");

      CreateService().Upsert(mapping);

      ConnectorProductAttributeSettingRepositoryMock.Verify(repository => repository.CreateOrUpdate(It.IsAny<ConnectorProductAttributeSetting>()), Times.AtLeast(2));

      Assert.True(connectorProductAttributeSettingUpsertDictionary.ContainsKey("Property 1"));
      Assert.True(connectorProductAttributeSettingUpsertDictionary.ContainsKey("Property 2"));
      
      ConnectorProductAttributeSettingRepositoryMock.Verify(repository => repository.Delete(It.IsAny<ConnectorProductAttributeSetting>()), Times.AtLeastOnce());

      Assert.True(connectorProductAttributeSettingDeleteDictionary.ContainsKey("Property 3"));
    }
  }
}
