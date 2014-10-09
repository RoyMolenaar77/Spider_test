using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Slurp
{
  
  public class SlurpItemEventArgs : EventArgs
  {
    #region Fields

    private SlurpItem item;
    private bool willStart;

    #endregion

    #region Constructor

    public SlurpItemEventArgs(SlurpItem item)
    {
      this.item = item;
    }

    public SlurpItemEventArgs(SlurpItem item, bool willStart)
      : this(item)
    {
      this.willStart = willStart;
    }

    #endregion

    #region Properties

    public SlurpItem SlurpItem
    {
      get { return item; }
    }

    public bool WillStart
    {
      get { return willStart; }
    }

    #endregion
  }
  
}
