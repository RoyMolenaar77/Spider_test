using System.Collections.Generic;

namespace Concentrator.Plugins.PFA.Transfer.Model.Mutation
{

  public class MutMessage
  {
    public MutMessage()
    {
      details = new List<MutDetail>();
    }

    public MutHeader header { get; set; }

    public List<MutDetail> details { get; set; }

    public MutTotal total { get; set; }
  }
}
