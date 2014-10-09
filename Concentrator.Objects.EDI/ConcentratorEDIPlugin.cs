using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.EDI.Order;

namespace Concentrator.Objects.EDI
{
  public abstract class ConcentratorEDIPlugin : ConcentratorPlugin
  {
    protected EdiVendor ediVendor;
    protected List<EdiOrderType> ediOrderTypes;
    protected IEdiProcessor ediProcessor
    {
      private set;
      get;
    }

    public ConcentratorEDIPlugin()
    {
      using (var unit = GetUnitOfWork())
      {
        int ediVendorID = int.Parse(Config.AppSettings.Settings["EdiVendorID"].Value);

        ediVendor = unit.Scope.Repository<EdiVendor>().GetSingle(x => x.EdiVendorID == ediVendorID);

        ediOrderTypes = unit.Scope.Repository<EdiOrderType>().GetAll().ToList();                         

        Type ediT = Type.GetType(ediVendor.EdiVendorType);
        ediProcessor = Activator.CreateInstance(ediT) as IEdiProcessor;
       // ediProcessor =  (IEdiProcessor)Activator.CreateInstance(Assembly.GetAssembly(typeof(IEdiProcessor)).GetType(ediVendor.EdiVendorType));
      }
    }

    public abstract override string Name
    {
      get;
    }

    public abstract System.Configuration.Configuration Config
    {
      get;
    }
        
    protected abstract override void Process();
    

  }
}
