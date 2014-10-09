﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Service.Scheduler
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
