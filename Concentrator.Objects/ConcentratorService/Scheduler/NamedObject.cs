using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.ConcentratorService.Scheduler
{
  public class NamedObject
  {
    public NamedObject(string name)
    {
      Name = name;
    }

    public string Name { get; private set; }
  }

}
