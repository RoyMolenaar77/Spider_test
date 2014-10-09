using System;
using System.Collections.Generic;

namespace Concentrator.Objects.Services.Contracts
{
  using Models.Connectors;
  using Models.Vendors;

  public interface IConnectorAssortmentProvider
  {
    IEnumerable<VendorAssortment> GetAssortment(Connector connector);
  }
}
