using System.Collections.Generic;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Bootstrapper.Installation.Roles
{
  class RoleToInstall
  {
    public RoleToInstall(string name, bool isHidden, params Functionalities[] functionalities)
    {
      Name = name;
      IsHidden = isHidden;
      Functionalities = functionalities;
    }
    public RoleToInstall(string name, params Functionalities[] functionalities)
      : this(name, false, functionalities)
    { }

    public string Name { get; private set; }
    public bool IsHidden { get; private set; }
    public IEnumerable<Functionalities> Functionalities { get; private set; }
  }
}