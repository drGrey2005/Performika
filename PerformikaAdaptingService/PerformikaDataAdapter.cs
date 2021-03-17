using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<int> DeleteProgramsAsync()
        {
            List<Guid> performikaGuids = await _postModule.GetUidsAsync("/gridview?definitionId=b4276b7c-a409-434d-8a0a-6ed74fa24ca0&_=1612775221043", "743d464a-2310-4a9d-b71d-32021b7a9961");
            return _dbLoader.DeleteRows("public.\"Program\"", "\"ProgramUid\"", performikaGuids);
        }

        public async Task<int> DeleteProgramObjectsAsync()
        {
            List<Guid> performikaGuids1 = await _postModule.GetUidsAsync("/gridview?definitionId=4abb7482-7c79-44fe-a9e9-6d94dc6fcaaf&_=1612857915341", "894b51d7-396c-4b33-940b-1d0a2777fcb6");

            List<Guid> performikaGuids2 = await _postModule.GetUidsAsync("/gridview?definitionId=a9ff91e2-337d-4c98-9d66-188c67805bce&_=1612874554748", "279ca164-c0ba-4469-90f9-b0ecdb13e224");

            List<Guid> performikaGuids3 = await _postModule.GetUidsAsync("/gridview?definitionId=a9ff91e2-337d-4c98-9d66-188c67805bce&_=1613030767702", "279ca164-c0ba-4469-90f9-b0ecdb13e224");

            List<Guid> performikaGuids = performikaGuids1.Union(performikaGuids2).Union(performikaGuids3).ToList();

            return _dbLoader.DeleteRows("public.\"ProgramObject\"", "\"ObjectUid\"", performikaGuids);
        }

        public async Task<int> DeleteRoadRepairsAsync()
        {
            List<Guid> performikaGuids = await _postModule.GetUidsAsync("/gridview?definitionId=1be20f82-ff64-4b3a-bb38-920a8d7b2bfe&_=1615965457226", "47f5d6b9-3c0c-43f0-a99a-93692565b05e");
            return _dbLoader.DeleteRows("public.\"ChildRoad\"", "\"ChildObjectUid\"", performikaGuids);
        }

        public async Task<int> DeleteRoadRepairIssoAsync()
        {
            List<Guid> performikaGuids = await _postModule.GetUidsAsync(
                "/gridview?definitionId=1be20f82-ff64-4b3a-bb38-920a8d7b2bfe&_=1615472618015",
                "47f5d6b9-3c0c-43f0-a99a-93692565b05e");
            return _dbLoader.DeleteRows("public.\"ChildISSO\"", "\"ChildObjectUid\"", performikaGuids);
        }

        public async Task<int> DeleteDictIssoAsync()
        {
            List<Guid> performikaGuids = await _postModule.GetUidsAsync(
                "/gridview?definitionId=9490c54b-8318-49b1-a28c-a1759a5ac806&_=1613371201694",
                "c1a527ef-5dfd-4833-a966-b6d822cbafe9");
            return _dbLoader.DeleteRows("dic.\"ISSO\"", "\"ObjectUid\"", performikaGuids);
        }
    }
}
