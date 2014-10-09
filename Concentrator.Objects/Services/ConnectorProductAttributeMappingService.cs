using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

using Ninject;

namespace Concentrator.Objects.Services
{
  using Contracts;
  using DataAccess;
  using DataAccess.Repository;
  using Enumerations;
  using Models;
  using Models.Attributes;
  using Models.Connectors;
  
  public class ConnectorProductAttributeMappingService : IConnectorProductAttributeMappingService
  {
    public IRepository<Connector> ConnectorRepository
    {
      get;
      private set;
    }

    public IConnectorProductAttributeRepository ConnectorProductAttributeRepository
    {
      get;
      private set;
    }

    public IConnectorProductAttributeSettingRepository ConnectorProductAttributeSettingRepository
    {
      get;
      private set;
    }

    public IConnectorProductAttributeSettingTemplateRepository ConnectorProductAttributeSettingTemplateRepository
    {
      get;
      private set;
    }

    public IRepository<ProductAttributeMetaData> ProductAttributeRepository
    {
      get;
      private set;
    }

    public ConnectorProductAttributeMappingService(
      IRepository<Connector>                              connectorRepository,
      IConnectorProductAttributeRepository                connectorProductAttributeRepository, 
      IConnectorProductAttributeSettingRepository         connectorProductAttributeSettingRepository, 
      IConnectorProductAttributeSettingTemplateRepository connectorProductAttributeSettingTemplateRepository,
      IRepository<ProductAttributeMetaData>               productAttributeRepository)
    {
      if (connectorRepository == null)
      {
        throw new ArgumentNullException("connectorRepository");
      }

      ConnectorRepository = connectorRepository;

      if (connectorProductAttributeRepository == null)
      {
        throw new ArgumentNullException("connectorProductAttributeRepository");
      }

      ConnectorProductAttributeRepository = connectorProductAttributeRepository;

      if (connectorProductAttributeSettingRepository == null)
      {
        throw new ArgumentNullException("ConnectorProductAttributeSettingRepository");
      }

      ConnectorProductAttributeSettingRepository = connectorProductAttributeSettingRepository;

      if (connectorProductAttributeSettingTemplateRepository == null)
      {
        throw new ArgumentNullException("ConnectorProductAttributeSettingTemplateRepository");
      }

      ConnectorProductAttributeSettingTemplateRepository = connectorProductAttributeSettingTemplateRepository;

      if (productAttributeRepository == null)
      {
        throw new ArgumentNullException("productAttributeRepository");
      }

      ProductAttributeRepository = productAttributeRepository;
    }
    
    public void Delete(ConnectorProductAttributeMapping connectorProductAttributeMapping)
    {
      Validate(connectorProductAttributeMapping);

      var connectorProductAttribute = ConnectorProductAttributeRepository.GetSingle(
        connectorProductAttributeMapping.Connector.ConnectorID, 
        connectorProductAttributeMapping.ProductAttribute.AttributeID, 
        connectorProductAttributeMapping.IsFilter);

      if (ConnectorProductAttributeRepository.Delete(connectorProductAttribute) == RepositoryOperationStatus.Nothing)
      {
        throw new InvalidOperationException("The specified product attribute mapping does not exist");
      }
    }

    public IEnumerable<ConnectorProductAttributeMapping> Get(Connector connector = null, ProductAttributeMetaData productAttribute = null, Boolean? isFilter = null)
    {
      return Get(connector != null ? (Int32?)connector.ConnectorID : null, productAttribute != null ? (Int32?)productAttribute.AttributeID : null, isFilter);
    }

    public IEnumerable<ConnectorProductAttributeMapping> Get(Int32? connectorID, Int32? productAttributeID, Boolean? isFilter)
    {
      var connectorProductAttributes = ConnectorProductAttributeRepository
        .GetMultiple(connectorID, productAttributeID, isFilter)
        .ToArray();

      foreach (var connectorProductAttribute in connectorProductAttributes)
      {
        var connectorProductAttributeSettings = ConnectorProductAttributeSettingRepository
          .GetMultiple(connectorProductAttribute.ConnectorProductAttributeID)
          .ToArray();
        
        var settingTypesByCode = connectorProductAttributeSettings.ToDictionary(
          connectorProductAttributeSetting => connectorProductAttributeSetting.Code, 
          connectorProductAttributeSetting => Type.GetType(connectorProductAttributeSetting.Type));

        if (settingTypesByCode.Any(pair => pair.Value == null))
        {
          throw new Exception("One or more types could not be found:\r\n" + String.Join(Environment.NewLine, settingTypesByCode
            .Where(pair => pair.Value == null)
            .Select(pair => String.Format(" - Code: '{0}', Type: '{1}'", pair.Key, pair.Value))
            .ToArray()));
        }

        var properties = connectorProductAttributeSettings.ToDictionary(
          settting => settting.Code, 
          setting => TypeConverterService.ConvertFromString(settingTypesByCode[setting.Code], setting.Value));

        var productAttributeType = Type.GetType(connectorProductAttribute.ProductAttributeType);

        yield return new ConnectorProductAttributeMapping(properties)
        {
          Connector = ConnectorRepository.GetSingle(connector => connector.ConnectorID == connectorProductAttribute.ConnectorID),
          DefaultValue = TypeConverterService.ConvertFromString(productAttributeType, connectorProductAttribute.DefaultValue),
          IsFilter = connectorProductAttribute.IsFilter,
          ProductAttribute = ProductAttributeRepository.GetSingle(productAttribute => productAttribute.AttributeID == connectorProductAttribute.ProductAttributeID),
          ProductAttributeType = Type.GetType(connectorProductAttribute.ProductAttributeType)
        };
      }
    }

    public void Upsert(ConnectorProductAttributeMapping connectorProductAttributeMapping)
    {
      Validate(connectorProductAttributeMapping);
      ValidateProperties(connectorProductAttributeMapping.Properties);

      var status = ConnectorProductAttributeRepository.Upsert(new ConnectorProductAttribute
      {
        ConnectorID = connectorProductAttributeMapping.Connector.ConnectorID,
        DefaultValue = TypeConverterService.Default[connectorProductAttributeMapping.ProductAttributeType].ConvertToString(connectorProductAttributeMapping.DefaultValue),
        IsFilter = connectorProductAttributeMapping.IsFilter,
        ProductAttributeID = connectorProductAttributeMapping.ProductAttribute.AttributeID,
        ProductAttributeType = connectorProductAttributeMapping.ProductAttributeType.FullName
      });
      
      var connectorProductAttribute = ConnectorProductAttributeRepository.GetSingle(
        connectorProductAttributeMapping.Connector.ConnectorID, 
        connectorProductAttributeMapping.ProductAttribute.AttributeID, 
        connectorProductAttributeMapping.IsFilter);
        
      if (status == RepositoryOperationStatus.Created)
      {
        foreach (var connectorProductAttributeSettingTemplate in ConnectorProductAttributeSettingTemplateRepository.GetMultiple(connectorProductAttributeMapping.Connector.ConnectorSystemID))
        {
          var templateSettingCode = connectorProductAttributeSettingTemplate.Code;

          if (!connectorProductAttributeMapping.Properties.ContainsKey(templateSettingCode))
          {
            var templateSettingType = Type.GetType(connectorProductAttributeSettingTemplate.Type);

            if (templateSettingType != null)
            {
              var templateSettingValue = TypeConverterService.ConvertFromString(templateSettingType, connectorProductAttributeSettingTemplate.Value);

              connectorProductAttributeMapping.Properties[templateSettingCode] = templateSettingValue;
            }
          }
        }
      }

      SynchronizeProperties(connectorProductAttribute, connectorProductAttributeMapping.Properties);
    }

    private void SynchronizeProperties(ConnectorProductAttribute connectorProductAttribute, IDictionary<String, Object> properties)
    {
      var connectorProductAttributeSettingDictionary = ConnectorProductAttributeSettingRepository
        .GetMultiple(connectorProductAttribute.ConnectorProductAttributeID)
        .ToDictionary(connectorProductAttributeSetting => connectorProductAttributeSetting.Code);

      // Delete all the connector product attribute settings that are not in the specified property-dictionary.
      foreach (var connectorProductAttributeSettingCode in connectorProductAttributeSettingDictionary.Keys.Except(properties.Keys))
      {
        ConnectorProductAttributeSettingRepository.Delete(connectorProductAttributeSettingDictionary[connectorProductAttributeSettingCode]);
      }

      foreach (var connectorProductAttributeSettingCode in properties.Keys)
      {
        var value = properties[connectorProductAttributeSettingCode];
        var valueType = value.GetType();
        
        ConnectorProductAttributeSettingRepository.CreateOrUpdate(new ConnectorProductAttributeSetting
        {
          ConnectorProductAttributeID = connectorProductAttribute.ConnectorProductAttributeID.Value,
          Code = connectorProductAttributeSettingCode,
          Type =  valueType.FullName,
          Value = TypeConverterService.Default[valueType].ConvertToString(value)
        });
      }
    }

    private void Validate(ConnectorProductAttributeMapping connectorProductAttributeMapping)
    {
      if (connectorProductAttributeMapping == null)
      {
        throw new ArgumentNullException("connectorProductAttributeMapping");
      }

      if (connectorProductAttributeMapping.ProductAttributeType == null)
      {
        throw new ArgumentException("ProductAttributeType cannot be null");
      }

      if (connectorProductAttributeMapping.ProductAttributeType.IsValueType && connectorProductAttributeMapping.DefaultValue == null)
      {
        throw new ArgumentException("DefaultValue cannot be null when ProductAttributeType is a value-type");
      }
    }

    private void ValidateProperties(IDictionary<String, Object> properties)
    {
      if (properties != null)
      {
        if (properties.Keys.Any(code => code.IsNullOrEmpty() || Char.IsWhiteSpace(code.First()) || Char.IsWhiteSpace(code.Last())))
        {
          throw new ArgumentException("the keys in the dictionary cannot be empty, begin- or end with whitespaces");
        }
      }
    }
  }
}
