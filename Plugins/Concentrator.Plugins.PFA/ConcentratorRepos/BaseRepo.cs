using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.ConcentratorRepos
{
  public class BaseRepo
  {
    protected string Connection { get; private set; }
    
    public BaseRepo(string connection)
    {
      Connection = connection;
    }
  }
}
