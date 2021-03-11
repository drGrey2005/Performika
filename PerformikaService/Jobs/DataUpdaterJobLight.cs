using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog;
using PerformikaDb;
using PerformikaLib;
using PerformikaLib.Entities;
using Quartz;

namespace PerformikaService.Jobs
{
	[DisallowConcurrentExecution]
	public class DataUpdaterJobLight : IJob
	{
		private readonly PerformikaPostModule _performikaPostModule;
		private DbLoader _dbLoader;
		private ILogger _logger = LogManager.GetCurrentClassLogger();
		private readonly int _loadForMinutes;
		public DataUpdaterJobLight(PerformikaPostModule postModule, DbLoader loader, IConfiguration config)
		{
			_loadForMinutes = -int.Parse(config["Quartz:LoadForMinutesLight"]);
			_performikaPostModule = postModule;
			_dbLoader = loader;
		}
		public async Task Execute(IJobExecutionContext context)
		{
			_logger.Info($"Инициирована загрузка данных за последние {_loadForMinutes} минут.");

			try
			{
				PerformikaUpdater updater = new PerformikaUpdater(_performikaPostModule, _dbLoader, _logger, _loadForMinutes);
				await updater.LoadProgramAsync();
				await updater.LoadProgramObjectsOneAsync();
				await updater.LoadProgramObjectsTwoAsync();
				await updater.LoadProgramObjectsThreeAsync();
				await updater.LoadRoadRepairAsync();
				await updater.LoadRoadRepairIssoAsync();
				await updater.LoadDictIssoAsync();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Ошибка загрузки данных.");
			}

			_logger.Info("Загрузка данных завершена.");
		}

	}
}
