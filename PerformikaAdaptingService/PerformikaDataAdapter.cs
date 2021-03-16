using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PerformikaDb;
using PerformikaLib;

namespace PerformikaAdaptingService
{
    /// <summary>
    /// Класс для сопоставления данных из перформики с данными из БД с целью их согласования
    /// </summary>
    public class PerformikaDataAdapter
    {
        private readonly PerformikaPostModule _postModule;
        private readonly DbLoader _dbLoader;

        public PerformikaDataAdapter(PerformikaPostModule postModule, DbLoader dbLoader)
        {
            _postModule = postModule;
            _dbLoader = dbLoader;
        }

        public async Task<int> DeletePrograms()
        {
            List<Guid> performikaGuids = await _postModule.GetUidsAsync("/gridview?definitionId=b4276b7c-a409-434d-8a0a-6ed74fa24ca0&_=1612775221043", "743d464a-2310-4a9d-b71d-32021b7a9961");
            return _dbLoader.DeleteRows("public.\"Program\"", "\"ProgramUid\"", performikaGuids);
        }
    }
}
