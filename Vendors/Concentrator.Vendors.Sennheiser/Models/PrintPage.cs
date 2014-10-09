using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.Vendors.Sennheiser.Models
{
  public abstract class PrintPage
  {
    public static IEnumerable<PrintPage<PriceListModel>> GeneratePages(List<PriceListModel> items, int perPage)
    {
      int pageCount = items.Count() % perPage;
      if (pageCount == 0)
        pageCount = items.Count() / perPage;
      else
        pageCount = items.Count() / perPage + 1;

      var itemList = new List<PrintPage<PriceListModel>>();
      string chapter = items.FirstOrDefault().Chapter;
      var page = new PrintPage<PriceListModel>();
      page.Items = new List<PriceListModel>();
      var pageCounter = perPage;

      items.ForEach((item, idx) =>
      {
        if (chapter == item.Chapter)
        {
          page.Items.Add(item);
        }
        else
        {
          chapter = item.Chapter;
          itemList.Add(page);
          page = new PrintPage<PriceListModel>();
          page.Items = new List<PriceListModel>();
          pageCounter = perPage;
          page.Items.Add(item);
        }

        pageCounter--;

        if (pageCounter == 0)
        {
          itemList.Add(page);
          page = new PrintPage<PriceListModel>();
          page.Items = new List<PriceListModel>();
          pageCounter = perPage;
        }
      });

      if(page.Items.Count > 0)
      itemList.Add(page);

      //return (from i in Enumerable.Range(0, pageCount)
      //        select new PrintPage<TResult>
      //        {
      //          Items = items.Skip(i * perPage).Take(perPage).ToList()
      //        }).ToList();

      return itemList;
    }
  }

  public class PrintPage<T> : PrintPage
  {
    public List<T> Items { get; set; }
  }

}
