using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Objects.Environments
{
  public class Environments
  {
    private Environments()
    { }

    public string Name
    {
      get;
      private set;
    }

    public string Connection
    {
      get;
      private set;
    }

    public IdentificationMethodType IdentificationType
    {

      get;
      private set;
    }

    public static Environments Current
    {
      get
      {
        if (!String.IsNullOrEmpty(ConfigSection.Current))
        {
          return AllEnvironments.FirstOrDefault(e => e.Name == ConfigSection.Current);
        }
        else
        {
          return null;
        }
      }
    }

    private static List<Environments> _allEnvironments = null;
    private static volatile string lockString = "THISISMYLOCK,KEEP AWAY";

    public static List<Environments> AllEnvironments
    {
      get
      {

        if (_allEnvironments == null)
        {
          lock (lockString)
          {
            _allEnvironments = new List<Environments>(ConfigSection.Environments.Count);

            foreach (EnvironmentElement el in ConfigSection.Environments)
            {
              _allEnvironments.Add(new Environments()
              {
                Name = el.Name,
                Connection = el.Connection,
                IdentificationType = el.IdentificationMethod
              });
            }
          }
        }

        return _allEnvironments;
      }
    }

    private static EnvironmentConfigSection _configSection = null;
    private static EnvironmentConfigSection ConfigSection
    {
      get
      {
        return _configSection ?? (_configSection = (EnvironmentConfigSection)ConfigurationManager.GetSection("Environment"));
      }
    }
  }
}