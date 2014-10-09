using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using System.Configuration;
using System.Net;
using System.IO;
using Concentrator.Objects;


namespace Concentrator.Plugins.Slurp
{
  class Slurp : ConcentratorPlugin
  {

    private const string _name = "Concentrator Slurp Import";

    public override string Name
    {
      get { return _name; }
    }

    protected override void Process()
    {
      using (ConcentratorDataContext context = new ConcentratorDataContext())
      {
        
        var mappingList = (from i in context.ProductGroupMappings
                           select i).ToList();

      }
    }


  }
}
