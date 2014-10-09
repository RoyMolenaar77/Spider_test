using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Concentrator.Service.Contracts
{
  public interface IConcentratorChannel : IConcentratorService, IClientChannel
  {
  }
}
