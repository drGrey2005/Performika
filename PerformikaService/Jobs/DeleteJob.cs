﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using PerformikaAdaptingService;
using Quartz;

namespace PerformikaService.Jobs
{
    [DisallowConcurrentExecution]
    public class DeleteJob : IJob
    {
        private readonly PerformikaDataAdapter _performikaDataAdapter;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public DeleteJob(PerformikaDataAdapter performikaDataAdapter)
        {
            _performikaDataAdapter = performikaDataAdapter;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.Info($"Инициирована служба удаления данных.");

            //Удаление программ работ, не найденных в перформике
            await DeletePrograms();

            _logger.Info("Работа службы удаления данных завершена.");
        }

        private async Task DeletePrograms()
        {
            try
            {
                _logger.Info("Запуск процесса удаления программ работ");
                int count = await _performikaDataAdapter.DeletePrograms();
                _logger.Info($"Процесс удаления программ работ завершен. Удалено {count} записей.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка удаления данных: {ex.Message}");
            }
        }
    }
}