using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PerformikaDb;
using PerformikaLib;
using PerformikaLib.Entities;

namespace TestConsoleApplication
{
    internal class Program
    {
        private static string _connectionString = @"Host=risadtest;Port=5432;Database=Risad2;User ID=postgres;Password=mipsa;";

        private static async Task Main(string[] args)
        {
            DateTime start = DateTime.Now;


            PerformikaPostModule postModule = new PerformikaPostModule("http://10.10.1.141:8080/api", "risad-change@mail.ru", "Risadchange1_");
            DbLoader loader = new DbLoader(_connectionString);


            PerformikaGetModule getModule = new PerformikaGetModule("https://asurisad.fad.ru/api", "risad-change@mail.ru", "Risadchange1_");

            //string id = "a3a9f38c-8591-4748-8831-801a372d4878";

            /*
			string itemId = "2977fb30-f64e-42e7-96eb-edc83520ed4d";

			Console.WriteLine(await getModule.GetMainObjectInfoAsync(itemId));
			Console.WriteLine("**************************************************************************");


			Console.WriteLine(await getModule.GetJobAmountAsync(itemId));
			Console.WriteLine("**************************************************************************");


			Console.WriteLine(await postModule.GetVolumeOfWorkByYearAsync(itemId));
			Console.WriteLine("**************************************************************************");


			Console.WriteLine(await postModule.GetJobAmountArticleAsync(itemId));
			Console.WriteLine("**************************************************************************");
			*/


            //foreach (string id in File.ReadLines(@"d:\UID.csv"))
            {
                //Console.WriteLine(await getModule.GetMainObjectInfoAsync(id));
                //Console.WriteLine(await getModule.GetJobAmountAsync(id));
                //Console.WriteLine(await postModule.GetVolumeOfWorkByYearAsync(id));
                //Console.WriteLine(await postModule.GetJobAmountArticleAsync("d66c413d-8d9a-4b35-9824-cfc496febe8f"));

            }

            /*
			List<ProgramInfo> infos = await postModule.GetProgramTypesAsync(0, 15000);
			loader.LoadProgramTypesInDb(infos);
			Console.WriteLine($"Обработано: {infos.Count} записей.");
			*/

            /*
			List<ProgramObjectInfo> infos = await postModule.GetProgramObjectsOneAsync(0, 5000);
			loader.LoadProgramObjectsInDb(infos);
			loader.LoadProgramObjectsConnection(postModule.ProgramObjectConnectionValues);
			Console.WriteLine($"Обработано: {infos.Count} записей.");
			*/

            /*
			List<ProgramObjectInfo> infos = await postModule.GetProgramObjectsTwoAsync(0, 5000);
			loader.LoadProgramObjectsInDb(infos);
			loader.LoadProgramObjectsConnection(postModule.ProgramObjectConnectionValues);
			Console.WriteLine($"Обработано {infos.Count} записей.");
			*/

            /*
			List<ProgramObjectInfo> infos = await postModule.GetProgramObjectsThreeAsync(0, 5000);
			loader.LoadProgramObjectsInDb(infos);
			loader.LoadProgramObjectsConnection(postModule.ProgramObjectConnectionValues);
			Console.WriteLine($"Обработано {infos.Count} записей.");
			*/


            /*
			List<RoadRepair> infos = await postModule.GetRoadRepairAsync(0, 5000);
			loader.LoadChildRoadsInDb(infos);
			Console.WriteLine($"Обработано {infos.Count} записей.");
			*/


            /*
			List<RoadRepairIsso> infos = await postModule.GetRoadRepairIssoAsync(50000, 5000);
			loader.LoadChildRoadsIssoInDb(infos);
			Console.WriteLine($"Обработано {infos.Count} записей.");
			*/

            /*
			for (int offset = 0; offset <= 164000; offset += 1000)
			{
				List<IssoValue> infos = await postModule.GetDictIssoAsync(offset, 1000);
				loader.LoadIssoDictInDb(infos);
				Console.WriteLine($"Обработан уровень смещения: {offset}");
			}
			*/

            //List<IssoValue> infos = await postModule.GetDictIssoAsync(0, 1000);
            //loader.LoadIssoDictInDb(infos);
            //infos.ForEach(val=>Console.WriteLine(val.ChangeDate));
            //Console.WriteLine($"Обработано: {infos.Count} записей.");



            /*
			Dictionary<string, string> roadSections = await postModule.RoadSectionMapping;
			loader.UpdateRoadSection(roadSections);
			File.WriteAllLines(@"d:\out.csv", roadSections.Select(d => d.Value + ";" + d.Key));
			*/

            //var v = (await postModule.GetGetRoadSectionsAsync(0, 15)).ToList();
            //v.ForEach(val =>
            //{
            //    Console.WriteLine(val);
            //});


            try
            {
                List<Guid> performikaGuids = await postModule.GetUids("/gridview?definitionId=b4276b7c-a409-434d-8a0a-6ed74fa24ca0&_=1612775221043", "743d464a-2310-4a9d-b71d-32021b7a9961");
                int count = loader.DeleteRows("public.\"Program\"", "\"ProgramUid\"", performikaGuids);
                Console.WriteLine($"Удалено {count} записей.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            Console.WriteLine("\nОбработка завершена");
            Console.WriteLine($"Время обработки: {DateTime.Now - start}");
            Console.ReadLine();
        }


    }
}