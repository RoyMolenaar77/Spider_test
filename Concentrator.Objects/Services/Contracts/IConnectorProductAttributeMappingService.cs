using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Concentrator.Objects.Services.Contracts
{
  using Models;
  using Models.Attributes;
  using Models.Connectors;

  /// <summary>
  /// Defines the operations for manipulating the product attribute mappings between Concentrator and Magento.
  /// </summary>
  [ServiceContract]
  public interface IConnectorProductAttributeMappingService
  {
    /// <summary>
    /// Deletes the connector product attribute mapping.
    /// </summary>
    [OperationContract]
    void Delete(ConnectorProductAttributeMapping connectorProductAttributeMapping);

    /// <summary>
    /// Retrieves zero or more connector product attribute mappings, based on the specified criteria.
    /// </summary>
    [OperationContract]
    IEnumerable<ConnectorProductAttributeMapping> Get(Int32? connectorID = null, Int32? productAttributeID = null, Boolean? isFilter = null);

    /// <summary>
    /// Retrieves zero or more connector product attribute mappings, based on the specified criteria.
    /// </summary>
    [OperationContract]
    IEnumerable<ConnectorProductAttributeMapping> Get(Connector connector = null, ProductAttributeMetaData productAttribute = null, Boolean? isFilter = null);

    /// <summary>
    /// Creates or updates the specified connector product attribute mapping.
    /// </summary>
    [OperationContract]
    void Upsert(ConnectorProductAttributeMapping connectorProductAttributeMapping);
  }
}
