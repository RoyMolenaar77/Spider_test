using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.WebToPrint.Components;
using Concentrator.Objects.WebToPrint;

namespace Concentrator.Plugins.WebToPrintProcessor
{
    public class IndexBuilder
    {
        private List<IndexItem> components;
        private List<IndexComponent> indexElements;
        private PrintablePage indexPage;

        private int indexPageOriginalIndex;
        private int indexesPerIndexPage;
        private double indexElementHeight = 0.0;
        private double lineHeight;
        private PrintableStyle indexStyle;

        private PrintableDocument doc;

        class IndexItem
        {
            public string ItemName;
            public PrintableComponent Component;
            public int Page
            {
                get
                {
                    if (Component != null)
                    {
                        CompositeComponent cc = Component.Parent;
                        while ((cc is PrintablePage) == false)
                        {
                            cc = cc.Parent;
                        }
                        return ((PrintablePage)cc).PageNumber;
                    }

                    return 0;
                }
            }
        }

        public IndexBuilder(PrintableDocument pd)
        {
            doc = pd;
            components = new List<IndexItem>();
            indexElements = new List<IndexComponent>();

            // loop through the whole document, picking up the elements designated as index holders and adding all other elements to the components list;
            foreach (PrintablePage pp in pd.Pages)
            {
              if (indexPage == null)
              {
                indexesPerIndexPage = 0;
              }

              foreach (PrintableComponent pc in pp.Children)
              {
                if (pc is CompositeComponent)
                {
                  AddComponent((CompositeComponent)pc);
                }

                if ((indexPage == null || indexPage == pp) && pc is TextComponent && ((TextComponent)pc).BindingOption == TextComponent.SpecialBinding.Index)
                {
                  if (indexPage == null) // no index page set?
                  {
                    indexPage = pp;
                    indexPageOriginalIndex = doc.Pages.IndexOf(indexPage);
                  }
                  
                  if (indexElementHeight == 0.0)
                    indexElementHeight = pc.Height;
                  else
                    pc.Height = indexElementHeight;

                  if (indexStyle == null)
                  {
                    indexStyle = ((TextComponent)pc).Style;
                    lineHeight = Util.PointToMillimeter(((TextComponent)pc).Style.AsXFont().GetHeight());
                  }
                  else
                  {
                    ((TextComponent)pc).Style = indexStyle;
                  }
                  
                  indexesPerIndexPage++;
                }
              }
            }
            if (indexPage != null)
              doc.Pages.Remove(indexPage);
        }

        public void AddComponent(CompositeComponent cc)
        {
            string indexName = "";
            foreach (var item in cc.Children)
            {
                if (item.IsIndexComponent && item is TextComponent)
                    indexName = ((TextComponent)item).GetDataValue();
            }
            if (indexName != "")
                components.Add(new IndexItem()
                {
                    ItemName = indexName,
                    Component = cc
                });
        }

        public void ExpandIndexPages()
        {
          if (indexPage != null)
          {
            int pages = Math.Max(1, components.Count  // how many lines do we have?
                        / (int)Math.Floor(indexElementHeight / lineHeight) // how many lines fit in each element?
                        / indexesPerIndexPage); // how many elements per page?

            for (int i = indexPageOriginalIndex; i < pages + indexPageOriginalIndex; i++)
            {
              PrintablePage pp = new PrintablePage(indexPage.Width, indexPage.Height);
              foreach (PrintableComponent pc in indexPage.Children)
              {
                if (pc is TextComponent && ((TextComponent)pc).BindingOption == TextComponent.SpecialBinding.Index)
                {
                  IndexComponent ic = IndexComponent.FromTextComponent((TextComponent)pc);
                  pp.AddChild(ic);
                  indexElements.Add(ic);
                }
                else
                {
                  pp.AddChild(pc.Clone());
                }
              }
              doc.Pages.Insert(i, pp);
            }
          }
        }

        public void BuildIndex()
        {
          if (indexPage != null)
          {
            // sort the components per name, and add the pagenumber to that so that components with the same name are sorted by page aswell
            components.Sort(delegate(IndexItem c1, IndexItem c2) { return (c1.ItemName + " " + c1.Page).CompareTo(c2.ItemName + " " + c2.Page); });
            int linesPerElement = (int)Math.Floor(indexElementHeight / lineHeight);
            string left = "", right = "";
            int indexComponent = 0, itemno = 0;

            foreach (IndexItem item in components)
            {
              left += item.ItemName + "\n";
              right += item.Page + "\n";
              itemno++;
              if (itemno > linesPerElement)
              {
                if (indexComponent >= indexElements.Count)
                {
                  break;
                }
                else
                {
                  indexElements[indexComponent].LeftText = left;
                  indexElements[indexComponent].RightText = right;
                  left = "";
                  right = "";
                }
                itemno = 0;
                indexComponent++;
              }

            }

            if (itemno > 0 && indexComponent < indexElements.Count)
            {
              indexElements[indexComponent].LeftText = left;
              indexElements[indexComponent].RightText = right;
            }
          }
        }
    }
}
