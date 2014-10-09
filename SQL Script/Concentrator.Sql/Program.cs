using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Configuration;
using Concentrator.Objects.Environments;
using Concentrator.Objects;
using System.Globalization;
using System.Data.SqlClient;
using Ninject;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Sql
{
  public class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Syncing database versions");

      if (ConfigurationManager.AppSettings["ProcessSQL"] != null && Convert.ToBoolean(int.Parse(ConfigurationManager.AppSettings["ProcessSQL"])))
      {
        try
        {
          using (IKernel kernel = new StandardKernel(new RequestScopeUnitOfWorkModule(), new InvariantModule()))
          {

            using (var unit = kernel.Get<IUnitOfWork>())
            {
              var scop = kernel.Get<IUnitOfWork>().Scope;

              var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);

              string localPath = fileInfo.Directory.FullName;

              if (localPath.EndsWith("\\bin\\Debug")) localPath = localPath.Replace("\\bin\\Debug", string.Empty);
              else if (localPath.EndsWith("\\bin\\Release")) localPath = localPath.Replace("\\bin\\Release", string.Empty);

              localPath = Path.Combine(localPath, "Script");

              var currentEnv = Concentrator.Configuration.Core.Environments.Current.Name;

              string envDir = Path.Combine(localPath, currentEnv);

              if (!Directory.Exists(envDir))
              {
                Directory.CreateDirectory(envDir);

                XDocument xdoc = new XDocument(new XDeclaration("1.0", "Utf-8", "yes"), new XElement("Envs", new XElement("Env", new XAttribute("name", currentEnv), new XAttribute("lastUpdate", DateTime.MinValue.ToString("yyyyMMddHHmm")))));

                xdoc.Save(Path.Combine(envDir, "Version.xml"));
              }

              foreach (var dir in Directory.GetDirectories(localPath).Where(c => new DirectoryInfo(c).Name != currentEnv))
              {
                string versionXML = Path.Combine(dir, "Version.xml");
                XDocument dirVersion = XDocument.Load(versionXML);
                var envElement = dirVersion.Root.Elements("Env").Where(x => x.Attribute("name").Value == currentEnv).FirstOrDefault();

                if (envElement == null)
                {
                  envElement = new XElement("Env", new XAttribute("name", currentEnv), new XAttribute("lastUpdate", DateTime.MinValue.ToString("yyyyMMddHHmm")));
                  dirVersion.Root.Add(envElement);
                }

                DateTime updated = DateTime.ParseExact(envElement.Attribute("lastUpdate").Value, "yyyyMMddHHmm", CultureInfo.InvariantCulture);

                foreach (var f in Directory.GetFiles(dir, "*.sql").OrderBy(c => DateTime.ParseExact(new FileInfo(c).Name.Split('_')[0], "yyyyMMddHHmm", CultureInfo.InvariantCulture)))
                {
                  FileInfo fInf = new FileInfo(f);

                  var fileSettings = fInf.Name.Split('_');

                  DateTime fileDate = DateTime.ParseExact(fileSettings[0], "yyyyMMddHHmm", CultureInfo.InvariantCulture);

                  if (DateTime.Compare(updated, fileDate) < 0)
                  {
                    try
                    {
                      using (StreamReader reader = new StreamReader(f))
                      {
                        unit.ExecuteStoreCommand(reader.ReadToEnd());
                        envElement.Attribute("lastUpdate").Value = fileDate.ToString("yyyyMMddHHmm");
                      }
                    }
                    catch (Exception e)
                    {
                      throw new ScriptException(f, dir, e.Message);
                    }
                  }
                  dirVersion.Save(versionXML);
                }
              }

              Console.WriteLine("Syncing complete. You are now up to date");
            }
          }
        }
        catch (ScriptException sql)
        {
          Console.WriteLine("{0}({1}): error {2}: {3}", sql._filename, 1, "666", sql.Message);

        }
        catch (Exception e)
        {
          Console.WriteLine("Something went wrong: " + e.Message);
        }
      }
    }
  }

  public class ScriptException : Exception
  {
    public string _filename;
    public string _environment;
    public string _message;

    public ScriptException(string filename, string environment, string message)
    {
      _filename = filename;
      _environment = environment;
      _message = message;
    }

    public override string Message
    {
      get
      {
        return String.Format("Environment : {0}: {1}", _environment, _message);
      }
    }
  }
}
