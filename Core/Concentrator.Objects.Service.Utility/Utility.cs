using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Concentrator.Service.Contracts;

namespace Concentrator.Objects.Service
{
  public static class Utility
  {
    public static void OpenChannel(ref IConcentratorChannel channel, IConcentratorService client, string host, int port)
    {
      if (channel != null && channel.State == CommunicationState.Faulted)
      {
        channel.Close();
        channel = null;
      }

      if (channel == null)
      {
        InstanceContext context = new InstanceContext(client);

        NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, true);

        string uri = String.Format("net.tcp://{0}:{1}/MiddleWareService", host, port);
        EndpointAddress address = new EndpointAddress(uri);
        
        channel = DuplexChannelFactory<IConcentratorChannel>.CreateChannel(context, binding, address);
        channel.OperationTimeout = new TimeSpan(0, 10, 5);

        try
        {
          if (channel.State == CommunicationState.Faulted)
          {
            channel = null;
          }
          else
          {
            channel.Open(new TimeSpan(0, 0, 3));
          }
        }
        catch (TimeoutException)
        {
          channel = null;
        }
      }
    }
  }
}
