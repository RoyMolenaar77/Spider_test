using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using log4net.Config;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using NinjectAdapter;
using Topshelf.Configuration;
using Topshelf.Configuration.Dsl;
using Topshelf;


namespace Concentrator.Host
{
	class Program
	{
		static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			var serviceName = ConfigurationManager.AppSettings["ServiceName"].IfNullOrEmpty("Concentrator Service Host");
      
			AppDomain.CurrentDomain.AssemblyResolve += (sender, arguments) =>
			{
				var pluginDirectory = ConfigurationManager.AppSettings["PluginPath"];

				if (!Directory.Exists(pluginDirectory))
				{
					throw new IOException(String.Format("'{0}' does not exist.", pluginDirectory));
				}

				var assemblyName = new AssemblyName(arguments.Name);

				var fullName = Path.Combine(pluginDirectory, assemblyName.Name + ".dll");

				if (!File.Exists(fullName))
				{
					throw new IOException(String.Format("'{0}' does not exist.", fullName));
				}

				return Assembly.LoadFile(fullName);
			};

#if DEBUG
			IKernel kernel = new ConcentratorKernel(new ManagementServiceModule(), new ContextThreadScopeModule(), new ThreadScopeUnitOfWorkModule(), new UnsecuredRepositoryModule(), new ServiceModule());
			var locator = new NinjectServiceLocator(kernel);

			ServiceLocator.SetLocatorProvider(() => locator);

			if (args.Length > 0)
			{
				ServiceLayer.Start(args[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray().Select(c => int.Parse(c)).ToArray());
			}
			else
			{
				ServiceLayer.Start();
			}

			Console.ReadLine();
			ServiceLayer.Stop();
#else
			RunConfiguration cfg = RunnerConfigurator.New(x =>
			{
				IKernel kernel = new ConcentratorKernel(new ManagementServiceModule(), new ContextThreadScopeModule(), new ThreadScopeUnitOfWorkModule(), new UnsecuredRepositoryModule(), new ServiceModule());

				var locator = new NinjectServiceLocator(kernel);
				ServiceLocator.SetLocatorProvider(() => locator);

				x.ConfigureService<ServiceLayer>(s =>
				{
					s.Named("tc");
					s.HowToBuildService(name => ServiceLayer.Instance);
					s.WhenStarted(tc => ServiceLayer.Start());
					s.WhenStopped(tc => ServiceLayer.Stop());
				});

				x.SetDescription("Concentrator Host Process for running plugins");
				x.SetDisplayName(serviceName); //staging
				x.SetServiceName(serviceName);//staging
				x.RunAsLocalSystem();
				// }
			});
			Runner.Host(cfg, args);
#endif
		}
	}
}
  