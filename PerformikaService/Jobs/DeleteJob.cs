using System;
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

            //Удаление программ работ, не найденных в Перформике
            await DeleteProgramsAsync();

            //Удаление объектов программ работ, не найденных в Перформике
            await DeleteProgramObjectsAsync();

            //Удаление участков дорог
            await DeleteRoadRepairsAsync();

            //Удаление участков дорог ИССО
            await DeleteRoadRepairIssoAsync();

            //Удаление справочников ИССО
            await DeleteDictIssoAsync();


            _logger.Info("Работа службы удаления данных завершена.");
        }

        private async Task DeleteProgramsAsync()
        {
            try
            {
                _logger.Info("Запуск процесса удаления программ работ");
                int count = await _performikaDataAdapter.DeleteProgramsAsync();
                _logger.Info($"Процесс удаления программ работ завершен. Удалено {count} записей.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка удаления программ работ: {ex.Message}");
            }
        }

        private async Task DeleteProgramObjectsAsync()
        {
            try
            {
                _logger.Info("Запуск процесса удаления объектов программ работ");
                int count = await _performikaDataAdapter.DeleteProgramObjectsAsync();
                _logger.Info($"Процесс удаления объектов программ работ завершен. Удалено {count} записей.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка удаления объектов программ работ: {ex.Message}");
            }
        }

        private async Task DeleteRoadRepairsAsync()
        {
            try
            {
                _logger.Info("Запуск процесса удаления участков дорог");
                int count = await _performikaDataAdapter.DeleteRoadRepairsAsync();
                _logger.Info($"Процесс удаления участков дорог завершен. Удалено {count} записей.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка удаления участков дорог: {ex.Message}");
            }
        }

        private async Task DeleteRoadRepairIssoAsync()
        {
            try
            {
                _logger.Info("Запуск процесса удаления участков дорог ИССО");
                int count = await _performikaDataAdapter.DeleteRoadRepairIssoAsync();
                _logger.Info($"Процесс удаления участков дорог ИССО завершен. Удалено {count} записей.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка удаления участков дорог ИССО: {ex.Message}");
            }
        }

        private async Task DeleteDictIssoAsync()
        {
            try
            {
                _logger.Info("Запуск процесса удаления справочников ИССО");
                int count = await _performikaDataAdapter.DeleteDictIssoAsync();
                _logger.Info($"Процесс удаления справочников ИССО завершен. Удалено {count} записей.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка удаления справочников дорог ИССО: {ex.Message}");
            }
        }

    }
}
