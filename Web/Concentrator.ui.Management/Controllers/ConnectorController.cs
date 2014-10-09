using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;

using PetaPoco;

namespace Concentrator.ui.Management.Controllers
{
  using Concentrator.Objects.Environments;
  using Objects.DataAccess.Repository;
  using Objects.Models;
  using Objects.Models.Connectors;
  using Objects.Models.Users;
  using Objects.Sql;
  using Objects.Web;
  using Web.Shared;
  using Web.Shared.Controllers;

  public class ConnectorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetList()
    {
      return List(unit => from c in unit.Service<Connector>().GetAll()
                          select new
                          {
                            c.ConnectorID,
                            c.Name,
                            System = c.ConnectorSystem.Name
                          });
    }

    private class ConnectorDTO
    {
      public Int32 ConnectorID
      {
        get;
        set;
      }

      public Int32 ConnectorSystemID
      {
        get;
        set;
      }

      public Boolean IsActive
      {
        get;
        set;
      }

      public String Name
      {
        get;
        set;
      }

      public Int32 ParentConnectorID
      {
        get;
        set;
      }
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetListByConnectorID(Int32? ConnectorID)
    {
      using (var database = new Database(Environments.Current.Connection, Database.MsSqlClientProvider))
      {
        var queryBuilder = new QueryBuilder()
          .From("[dbo].[Connector] AS C1")
          .Join(JoinType.CrossApply, "[dbo].[GetConnectorDescendants](C1.[ConnectorID], DEFAULT, DEFAULT) AS CD")
          .Join(JoinType.Inner, "[dbo].[Connector] AS C2", "CD.[ConnectorID] = C2.[ConnectorID]");

        if (ConnectorID.HasValue)
        {
          queryBuilder.Where("C1.[ConnectorID] = @0");
        }
        else
        {
          queryBuilder.Where("C1.[ParentConnectorID] IS NULL");
        }

        var query = queryBuilder.Select("C2.[ConnectorID]", "C2.[ConnectorSystemID]", "C2.[ParentConnectorID]", "C2.[IsActive]", "C2.[Name]");

        return List(database.Query<ConnectorDTO>(query, ConnectorID).AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult Get(int connectorID)
    {
      var connector = GetObject<Connector>(c => c.ConnectorID == connectorID);

      return Json(new
      {
        success = true,
        data = new
        {
          connector.Name,
          ConnectorType = ((ConnectorType)connector.ConnectorType).GetActiveList(connector),
          connector.BackendEanIdentifier,
          connector.BSKIdentifier,
          connector.ConcatenateBrandName,
          connector.Connection,
          connector.ImportCommercialText,
          connector.IsActive,
          connector.ObsoleteProducts,
          connector.OverrideDescriptions,
          connector.UseConcentratorProductID,
          connector.ZipCodes,
          connector.ParentConnectorID,
          connector.ConnectorSystemID,
          connector.DefaultImage,
          connector.Selectors,
          connector.ConnectorLogoPath
        }
      });
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetStore()
    {
      var currentOrganizationID = Client.User.OrganizationID;
      return SimpleList(unit => (from con in unit.Service<Connector>().GetAll(connector => connector.OrganizationID == currentOrganizationID)
                                            select new { ID = con.ConnectorID, Name = con.Name }));

    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetStoreByConnector()
    {
      return SimpleList(c => (from con in c.Service<Connector>().GetAll(x => x.ParentConnectorID == Client.User.ConnectorID || x.ConnectorID == Client.User.ConnectorID)
                              select new { ID = con.ConnectorID, Name = con.Name }));
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetConnectorTypeStore()
    {
      return Json(new
      {
        results = (from c in Enums.Get<ConnectorType>().AsQueryable()
                   select new
                   {
                     ConnectorTypeName = Enum.GetName(typeof(ConnectorType), c),
                     ConnectorType = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetConnectorSystemStore()
    {
      return Json(new
      {
        results = SimpleList<ConnectorSystem>(c => new { ConnectorSystemID = c.ConnectorSystemID, ConnectorSystemName = c.Name })
      });
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetParentConnectorStore()
    {
      return Json(new
      {
        results = SimpleList<Connector>(c => new
        {
          ParentConnectorID = c.ParentConnectorID,
          ParentConnectorName = c.ParentConnector.Name
        })
      });
    }

    [RequiresAuthentication(Functionalities.CreateConnector)]
    public ActionResult Create(string ConnectorType)
    {
      return Create<Connector>(onCreatingAction: (unit, connectorModel) =>
      {
        connectorModel.ConnectorType = ConnectorType.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Sum(x => int.Parse(x));
      });
    }

    [RequiresAuthentication(Functionalities.UpdateConnector)]
    public ActionResult Update(int connectorID)
    {
      return Update<Connector>(c => c.ConnectorID == connectorID);
    }

    [RequiresAuthentication(Functionalities.UpdateConnector)]
    public ActionResult AddImage(int connectorID)
    {
      return Update<Connector>(c => c.ConnectorID == connectorID,
      action: (unit, connector) =>
      {
        foreach (string f in Request.Files)
        {
          string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
          string internalPath = ConfigurationManager.AppSettings["FTPConnectorLogoMediaPath"];

          string dirPath = Path.Combine(externalPath, internalPath);

          if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

          var file = Request.Files.Get(f);
          string path = Path.Combine(dirPath, file.FileName);

          if (file.FileName != String.Empty)
          {
            file.SaveAs(path);
          }

          connector.ConnectorLogoPath = file.FileName;
          file.InputStream.Close();
        }

        SetSpecialPropertyValues<Connector>(connector, null);

      }, isMultipartRequest: true);
    }

    [RequiresAuthentication(Functionalities.UpdateConnector)]
    public ActionResult AddDefaultImage(int connectorID)
    {
      return Update<Connector>(c => c.ConnectorID == connectorID,
      action: (unit, connector) =>
      {
        foreach (string f in Request.Files)
        {
          string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
          string internalPath = ConfigurationManager.AppSettings["FTPDefaultImagePath"];

          string dirPath = Path.Combine(externalPath, internalPath);

          if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

          var file = Request.Files.Get(f);
          string path = Path.Combine(dirPath, file.FileName);

          if (file.FileName != String.Empty)
          {
            file.SaveAs(path);
          }

          connector.DefaultImage = file.FileName;
          file.InputStream.Close();
        }

        SetSpecialPropertyValues<Connector>(connector, null);

      }, isMultipartRequest: true);
    }

    [RequiresAuthentication(Functionalities.UpdateConnector)]
    public ActionResult DeleteLogo(int connectorID)
    {
      return Update<Connector>(x => x.ConnectorID == connectorID,
        action: (unit, connector) =>
        {
          string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
          string internalPath = ConfigurationManager.AppSettings["FTPConnectorLogoMediaPath"];
          string fileName = connector.ConnectorLogoPath;

          string dirPath = Path.Combine(externalPath, internalPath, fileName);

          System.IO.File.Delete(dirPath);

          connector.ConnectorLogoPath = null;
        });
    }

    [RequiresAuthentication(Functionalities.UpdateConnector)]
    public ActionResult DeleteDefaultImage(int connectorID)
    {
      return Update<Connector>(x => x.ConnectorID == connectorID,
        action: (unit, connector) =>
        {
          string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
          string internalPath = ConfigurationManager.AppSettings["FTPDefaultImagePath"];
          string fileName = connector.DefaultImage;

          string dirPath = Path.Combine(externalPath, internalPath, fileName);

          System.IO.File.Delete(dirPath);

          connector.DefaultImage = null;
        });
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult Search(string query, bool? searchID)
    {
      int connectorID = 0;
      int.TryParse(query, out connectorID);

      var currentOrganizationID = Client.User.OrganizationID;

      if (searchID.HasValue && searchID.Value && connectorID > 0)
      {
        return SimpleList((unit) => from c in unit.Service<Connector>().GetAll()
                                    where (c.OrganizationID ==  currentOrganizationID) &&
                                    c.ConnectorID == connectorID
                                    select new
                                    {
                                      c.ConnectorID,
                                      c.Name
                                    });
      }
      else
      {
        return SimpleList((unit) => from c in unit.Service<Connector>().Search(query)
                                    where (c.OrganizationID == currentOrganizationID)
                                    select new
                                    {
                                      c.ConnectorID,
                                      c.Name
                                    });
      }
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetSystemTypeStore()
    {
      return Json(new
      {
        results = (from c in Enums.Get<ConnectorSystemType>().AsQueryable()
                   select new
                   {
                     ConnectorSystemTypeName = c.ToString(),
                     ConnectorSystemType = (int)c
                   }
                 )
      });
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetConnectorSystem(int? connectorID)
    {

      if (!connectorID.HasValue)
      {
        connectorID = Client.User.ConnectorID;
      }

      String connectorSystem = GetUnitOfWork().Service<Connector>().Get(x => x.ConnectorID == connectorID).Try(x => x.ConnectorSystem.Name, String.Empty);

      return Json(new
      {
        Success = true,
        ConnectorSystem = connectorSystem
      });
    }

    [RequiresAuthentication(Functionalities.GetConnector)]
    public ActionResult GetConnectorSettings(int connectorID)
    {
      return List(unit => unit.Service<ConnectorSetting>()
        .GetAll(x => x.ConnectorID == connectorID)
        .Select(x => new
        {
          x.ConnectorID,
          x.SettingKey,
          x.Value
        }));
    }

    [RequiresAuthentication(Functionalities.UpdateConnector)]
    public ActionResult UpdateConnectorSetting(int _connectorId, string _settingKey)
    {
      return Update<ConnectorSetting>(x => x.ConnectorID == _connectorId && x.SettingKey == _settingKey);
    }

  }
}
