using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.VSN
{
  public abstract class SynopsisImportBase : VSNBase
  {

    protected void ProcessSynopsisTable(DataTable synTable, IUnitOfWork unit)
    {
      var synopsis = from r in synTable.Rows.Cast<DataRow>()
                     select new
                     {
                       ProductCode = r.Field<string>("ProductCode"),
                       Synopsis = r.Field<string>("Synopsis")
                     };

      int step = 100;
      int todo = synopsis.Count();
      int done = 0;
      log.AuditInfo("Starting processing of synopsis. To process:  " + todo);
      while (done < todo)
      {
        var toProcess = synopsis.Skip(done).Take(step);

        foreach (var syn in toProcess)
        {
          var product = unit.Scope.Repository<VendorAssortment>().GetSingle(va => va.CustomItemNumber == syn.ProductCode && va.VendorID == VendorID && va.IsActive == true);
          if (product == null)
          {
            log.AuditWarning(string.Format("Cannot process specs for product with VSN number: {0} because it doesn't exist in Concentrator database", syn.ProductCode));
            continue;
          }
          var desc = product.Product.ProductDescriptions.FirstOrDefault(pd => pd.VendorID == VendorID && pd.LanguageID == (int)LanguageTypes.Netherlands);
          if (desc == null)
          {
            desc = new ProductDescription
                     {
                       VendorID = VendorID,
                       Product = product.Product,
                       LanguageID = (int)LanguageTypes.Netherlands
                     };
            unit.Scope.Repository<ProductDescription>().Add(desc);
          }
          desc.LongContentDescription = syn.Synopsis;

          // var desc = ctx.ProductDescription.FirstOrDefault(pd => pd.VendorID == VendorID && pd.Product.)
        }

        unit.Save();
        done += toProcess.Count();
      }
      log.AuditInfo("Finished processing synopsis. Processed: " + done);
    }

  }
}
