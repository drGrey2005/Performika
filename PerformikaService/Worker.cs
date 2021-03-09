using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace PerformikaService
{
	public class Worker : BackgroundService
	{
		private NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Task.Run(() => _logger.Info("Выполнен запуск сервиса"));
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.Info("Выполнена остановка сервиса.");
			return base.StopAsync(cancellationToken);
		}
	}
}
