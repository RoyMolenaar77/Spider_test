using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Users
{
  public class Organization
  {
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<User> Users { get; set; }

    public virtual ICollection<Connector> Connectors { get; set; }

    public virtual ICollection<ProductAttributeValueLabel> ProductAttributeValueLabels { get; set; }
  }
}
