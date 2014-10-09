using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.PFA
{
  public class VpnCheck : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "CCAT VPN Check"; }
    }

    protected override void Process()
    {
      Check();
    }

    public void Check()
    {
      bool vpnOk = true;
      System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();

      foreach (var ip in GetConfiguration().AppSettings.Settings["VpnIps"].Value.Split(','))
      {
        if (p.Send(ip).Status != System.Net.NetworkInformation.IPStatus.Success)
        {
          vpnOk = false;
        }
      }

      if (!vpnOk)
      {
        log.AuditCritical("VPN is down", "VPN Check CCAT");
      }
    }

  }
}
