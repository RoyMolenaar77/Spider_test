using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class CompositeComponent : PrintableComponent
  {
      public List<PrintableComponent> Children { get; private set; }
    public Dictionary<string, string> Inputs;

    public CompositeComponent(double left, double top, double width, double height, double angle) : base(left, top, width,height, angle)
    {
      Children = new List<PrintableComponent>();
      Inputs = new Dictionary<string, string>();
    }

    public void AddChild(PrintableComponent pc)
    {
        Children.Add(pc);
        pc.Parent = this;
    }

    public string GetIndexName()
    {
        foreach (PrintableComponent pc in Children)
        {

        }
        return "";
    }

    /// <summary>
    /// This function provides the children elements of the data from the binding query
    /// </summary>
    /// <param name="values">Dictionary of mappings and values</param>
    public void SetData(Dictionary<string, string> values)
    {
      foreach (PrintableComponent pc in Children)
      {
        if (pc.DataBindingSource != null && values.ContainsKey(pc.DataBindingSource))
        {
          pc.SetData(values[pc.DataBindingSource]);
        }
      }
    }

    public override void Render(ref XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
      foreach (PrintableComponent pc in Children)
      {
        pc.Render(ref gfx, offsetLeft + Left, offsetTop+Top, Width / 100, Height / 100);
      }
    }
  }
}
