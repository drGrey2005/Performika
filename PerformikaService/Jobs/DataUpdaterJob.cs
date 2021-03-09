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
	public class DataUpdaterJob : IJob
	{
		private readonly PerformikaPostModule _performikaPostModule;
		private DbLoader _dbLoader;
		private ILogger _logger = LogManager.GetCurrentClassLogger();
		private readonly int _loadForMinutes;
		public DataUpdaterJob(PerformikaPostModule postModule, DbLoader loader, IConfiguration config)
		{
			_loadForMinutes = -int.Parse(config["Quartz:LoadForMinutes"]);
			_performikaPostModule = postModule;
			_dbLoader = loader;
		}
		public async Task Execute(IJobExecutionContext context)
		{
			_logger.Info("Инициирована загрузка данных.");

			try
			{
				await LoadProgramAsync();
				await LoadProgramObjectsOneAsync();
				await LoadProgramObjectsTwoAsync();
				await LoadProgramObjectsThreeAsync();
				await LoadRoadRepairAsync();
				await LoadRoadRepairIssoAsync();
				await LoadDictIssoAsync();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Ошибка загрузки данных.");
			}

			_logger.Info("Загрузка данных завершена.");
		}

		public async Task LoadProgramAsync()
		{
			try
			{
				_logger.Info("Загрузка программ...");
				List<ProgramInfo> infos = await _performikaPostModule.GetProgramTypesAsync(0, 5000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadProgramTypesInDb(infos);
				_logger.Info($"Загрузка программ завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки программ: '{ex.Message}'.");
			}
		}

		public async Task LoadProgramObjectsOneAsync()
		{
			try
			{
				_logger.Info("Загрузка программ работ 'Для остальных'.");
				List<ProgramObjectInfo> infos = await _performikaPostModule.GetProgramObjectsOneAsync(0, 5000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadProgramObjectsInDb(infos);
				_dbLoader.LoadProgramObjectsConnection(_performikaPostModule.ProgramObjectConnectionValues);
				_logger.Info($"Загрузка программ работ 'Для остальных' завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки программ работ 'Для остальных': '{ex.Message}'.");
			}
		}

		public async Task LoadProgramObjectsTwoAsync()
		{
			try
			{
				_logger.Info("Загрузка программ работ 'Содержание дорог'");
				List<ProgramObjectInfo> infos = await _performikaPostModule.GetProgramObjectsTwoAsync(0, 5000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadProgramObjectsInDb(infos);
				_dbLoader.LoadProgramObjectsConnection(_performikaPostModule.ProgramObjectConnectionValues);
				_logger.Info($"Загрузка программ работ 'Содержание дорог' завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки программ работа 'Содержание дорог': '{ex.Message}'.");
			}
		}

		public async Task LoadProgramObjectsThreeAsync()
		{
			try
			{
				_logger.Info("Загрузка программ работ 'Содержание ИССО'");
				List<ProgramObjectInfo> infos = await _performikaPostModule.GetProgramObjectsThreeAsync(0, 5000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadProgramObjectsInDb(infos);
				_dbLoader.LoadProgramObjectsConnection(_performikaPostModule.ProgramObjectConnectionValues);
				_logger.Info($"Загрузка программ работ 'Содержание ИССО' завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки программ работ 'Содержание ИССО': '{ex.Message}'.");
			}
		}

		public async Task LoadRoadRepairAsync()
		{
			try
			{
				_logger.Info("Загрузка «участков дорог» объектов ПР «Кап. ремонт дорог ПИР/СМР», «Ремонт дорог ПИР/СМР/ШПО», «Содержание дорог» и «Содержание ИССО (норм. содержание)».");
				List<RoadRepair> infos = await _performikaPostModule.GetRoadRepairAsync(0, 5000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadChildRoadsInDb(infos);
				_logger.Info($"Загрузка «участков дорог» объектов ПР «Кап. ремонт дорог ПИР/СМР», «Ремонт дорог ПИР/СМР/ШПО», «Содержание дорог» и «Содержание ИССО (норм. содержание)» завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки «участков дорог» объектов ПР «Кап. ремонт дорог ПИР/СМР», «Ремонт дорог ПИР/СМР/ШПО», «Содержание дорог» и «Содержание ИССО (норм. содержание)»: '{ex.Message}'.");
			}
		}

		public async Task LoadRoadRepairIssoAsync()
		{
			try
			{
				_logger.Info("Загрузка «участков дорог» объектов ПР «Кап. ремонт ИССО ПИР/СМР», «Ремонт ИССО ПИР/СМР» и «Содержание ИССО (ППР)».");
				List<RoadRepairIsso> infos = await _performikaPostModule.GetRoadRepairIssoAsync(0, 5000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadChildRoadsIssoInDb(infos);
				_logger.Info($"Загрузка «участков дорог» объектов ПР «Кап. ремонт ИССО ПИР/СМР», «Ремонт ИССО ПИР/СМР» и «Содержание ИССО (ППР)» завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки «участков дорог» объектов ПР «Кап. ремонт ИССО ПИР/СМР», «Ремонт ИССО ПИР/СМР» и «Содержание ИССО (ППР)»: '{ex.Message}'.");
			}
		}


		public async Task LoadDictIssoAsync()
		{
			try
			{
				_logger.Info("Загрузка справочника ИССО.");
				List<IssoValue> infos = await _performikaPostModule.GetDictIssoAsync(0, 1000, DateTime.Now.AddMinutes(_loadForMinutes));
				_dbLoader.LoadIssoDictInDb(infos);
				_logger.Info($"Загрузка справочников ИССО завершена. Обработано: {infos.Count} записей.");
			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка загрузки загрузки справочников ИССО: '{ex.Message}'.");
			}
		}

	}
}
