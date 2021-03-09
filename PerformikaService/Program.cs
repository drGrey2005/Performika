using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using PerformikaDb;
using PerformikaLib;
using PerformikaService.Jobs;
using Quartz;

namespace PerformikaService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseWindowsService()
				.ConfigureServices((hostContext, services) =>
				{
					IConfiguration configuration = hostContext.Configuration;
					string baseAddress = configuration["PerformikaSettings:BaseAddress"];
					string login = configuration["PerformikaSettings:Login"];
					string password = configuration["PerformikaSettings:Password"];
					string connStr = configuration.GetConnectionString("postgresConnection");


					services.AddHostedService<Worker>();

					services.AddSingleton(context => new PerformikaPostModule(baseAddress, login, password));
					services.AddSingleton(context => new DbLoader(connStr));

					services.AddQuartz(q =>
					{
						q.UseMicrosoftDependencyInjectionScopedJobFactory();
						
						q.AddJobAndTrigger<DataUpdaterJob>(hostContext.Configuration);
					});

					services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

				});
	}
}
