using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PerformikaLib.Entities;

namespace PerformikaLib
{
	/// <summary>
	/// Получение данных через POST-запросы
	/// </summary>
	public class PerformikaPostModule
	{
		private readonly PerformikaApiConnector _connector;

		private Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly Dictionary<string, dynamic> _rowsMapping = new Dictionary<string, dynamic>();
		private readonly Dictionary<string, string> _lookups = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _members = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _statuses = new Dictionary<string, string>();

		//Общее число записей в воркспейсе
		public int LastTotalCount { get; set; }

		//Группа хранилищ для загрузки программы работ
		private readonly HashSet<string> _programTypesFilterValues = new HashSet<string>();
		private Dictionary<string, string> _programTypesMapping = new Dictionary<string, string>();
		private Dictionary<string, string> _fkusMapping = new Dictionary<string, string>();
		private Dictionary<string, string> _regionsMapping = new Dictionary<string, string>();
		private Dictionary<string, string> _roadSectionsMapping = new Dictionary<string, string>();

		public Task<Dictionary<string, string>> RoadSectionMapping
		{
			get
			{
				return Task.Run(async () => await GetRoadSectionsMappingAsync());
			}
		}

		private readonly HashSet<(Guid UID, int programType, int startYear, int endYear, int fku)> _programObjectConnectionValues = new HashSet<(Guid UID, int programType, int startYear, int endYear, int fku)>();
		public HashSet<(Guid UID, int programType, int startYear, int endYear, int fku)>
			ProgramObjectConnectionValues => _programObjectConnectionValues;


		public string BaseUrl { get; }
		public PerformikaPostModule(string url, string login, string password)
		{
			_connector = new PerformikaApiConnector(url, login, password);
			BaseUrl = url;
		}

		/// <summary>
		/// Получение программ работ
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<ProgramInfo>> GetProgramTypesAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();

			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=b4276b7c-a409-434d-8a0a-6ed74fa24ca0&_=1612775221043",
						_connector.CreateJsonContent(
							"743d464a-2310-4a9d-b71d-32021b7a9961",
							new List<string>
							{
						"e4045542-ea50-4f8e-a559-71072e369ff2",//Программа работ
						"afdb964c-1314-4eaa-9f8c-5efb29f12b8f",//Год
						"0571d342-ff2a-4447-96a8-57aaad12665b",//ФКУ
						"c637468e-68d4-4929-83ac-168d3258523b",//Статус
						"12b86e25-d8df-4bd6-a606-b4bb19771e38",//Дата создания
						"9d87a1b0-948f-4912-8ca5-ef09f09785ef"//Дата изменения
							},
							_programTypesFilterValues.Select(value => new FilterOperator
							{
								fieldId = "e4045542-ea50-4f8e-a559-71072e369ff2",
								@operator = 1,
								fieldValue = value
							}).ToList(),
							new List<FilterOperator>
							{
								new FilterOperator
								{
									fieldId = "9d87a1b0-948f-4912-8ca5-ef09f09785ef",
									@operator = 6,
									fieldValue = dt.ToString("s")+"Z"
								}
							},
							offset,
							pageSize
						));

			return entityInfos.Where(info => !string.IsNullOrEmpty(info.Values["0571d342-ff2a-4447-96a8-57aaad12665b"].Item1)).Select(info =>
			{
				try
				{
					return new ProgramInfo
					{
						Id = info.Id,
						ProgramType = int.Parse(_programTypesMapping[info.Values["e4045542-ea50-4f8e-a559-71072e369ff2"].Item2]),
						Year = int.Parse(info.Values["afdb964c-1314-4eaa-9f8c-5efb29f12b8f"].Item1),
						Fku = int.Parse(_fkusMapping[info.Values["0571d342-ff2a-4447-96a8-57aaad12665b"].Item2]),
						Status = info.Values["c637468e-68d4-4929-83ac-168d3258523b"].Item1,
						ChangeDate = DateTime.Parse(info.Values["9d87a1b0-948f-4912-8ca5-ef09f09785ef"].Item1)
					};
				}
				catch (Exception ex)
				{
					_logger.Error($"{ex.Message}\nObject is: {info}");
					return null;
				}
			}).ToList();
		}

		/// <summary>
		/// Получение программ работ "Для остальных" (2.1)
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<ProgramObjectInfo>> GetProgramObjectsOneAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();
			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=4abb7482-7c79-44fe-a9e9-6d94dc6fcaaf&_=1612857915341",
				_connector.CreateJsonContent(
					"894b51d7-396c-4b33-940b-1d0a2777fcb6",
					new List<string>
					{
						"9c95064a-9fe0-41b8-b609-529dc723a57a",//Наименование объекта
						"f59fef1b-36f1-4d52-b78a-6be0a9de2b45",//Программа работ для ФДА
						"6578d87e-32cf-44bc-9897-84f8644400b9",//Год начала работ (число)
						"8cf25335-1f94-43c7-b8da-3cc6720e2926",//Год окончания работ (число)
						"9b46331e-5a92-4f78-9297-19c9f9967926",//ФКУ
						"0ad9b3ee-6163-47ac-bf91-5e1c91f520ca",//Регион
						"a1242253-dd56-4ce1-bc51-59596c9c43ae",//Статус
						"57ce8472-54aa-440b-b252-f6d57d74e19c",//Дата создания
						"61a7afbe-8481-4b5b-ae2d-812e9292a40e" //Дата изменения
					}, _programTypesFilterValues.Select(value => new FilterOperator
					{
						fieldId = "f59fef1b-36f1-4d52-b78a-6be0a9de2b45",
						@operator = 1,
						fieldValue = value
					}).ToList(), new List<FilterOperator>
							{
								new FilterOperator
								{
									fieldId = "61a7afbe-8481-4b5b-ae2d-812e9292a40e",
									@operator = 6,
									fieldValue = dt.ToString("s")+"Z"
								}
							}, offset, pageSize
				));

			return entityInfos.Where(info =>
				!string.IsNullOrEmpty(info.Values["9b46331e-5a92-4f78-9297-19c9f9967926"].Item1) &&
				info.Values.ContainsKey("9c95064a-9fe0-41b8-b609-529dc723a57a") &&
				info.Values.ContainsKey("6578d87e-32cf-44bc-9897-84f8644400b9") &&
				info.Values.ContainsKey("8cf25335-1f94-43c7-b8da-3cc6720e2926")
				).Select(info =>
				{
					_programObjectConnectionValues.Add(
						(
							UID: info.Id,
							programType: int.Parse(_programTypesMapping[info.Values["f59fef1b-36f1-4d52-b78a-6be0a9de2b45"].Item2]),
							startYear: int.Parse(info.Values["6578d87e-32cf-44bc-9897-84f8644400b9"].Item1),
							endYear: int.Parse(info.Values["8cf25335-1f94-43c7-b8da-3cc6720e2926"].Item1),
							fku: int.Parse(_fkusMapping[info.Values["9b46331e-5a92-4f78-9297-19c9f9967926"].Item2]
							)));
					return new ProgramObjectInfo
					{
						Id = info.Id,
						ObjectName = info.Values["9c95064a-9fe0-41b8-b609-529dc723a57a"].Item1,
						RegionId = int.Parse(_regionsMapping[info.Values["0ad9b3ee-6163-47ac-bf91-5e1c91f520ca"].Item2]),
						Status = info.Values["a1242253-dd56-4ce1-bc51-59596c9c43ae"].Item1,
						RoadSectionId = null,
						NrsId = null,
						Type = null,
						YearStart = int.Parse(info.Values["6578d87e-32cf-44bc-9897-84f8644400b9"].Item1),
						YearEnd = int.Parse(info.Values["8cf25335-1f94-43c7-b8da-3cc6720e2926"].Item1),
						ChangeDate = DateTime.Parse(info.Values["61a7afbe-8481-4b5b-ae2d-812e9292a40e"].Item1)
					};
				}).ToList();
		}

		/// <summary>
		/// Получение программ работ "Содержание дорог" (2.2)
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<ProgramObjectInfo>> GetProgramObjectsTwoAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();
			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=a9ff91e2-337d-4c98-9d66-188c67805bce&_=1612874554748",
				_connector.CreateJsonContent(
					"279ca164-c0ba-4469-90f9-b0ecdb13e224",
					new List<string>
					{
						"5c463311-b5bc-4cd4-9182-0c6f4c215275",//Наименование объекта
						"c1e5afed-214d-4b11-9f27-854ced671050",//Регион
						"a52f275a-7589-4c85-a92b-7ca2c711447e",//ФКУ
						"ec9346f7-2460-4049-b7f8-49e807bc38ef",//Программа работ для ФДА
						"67af7381-320e-47b7-9e85-349fa927b4c6",//Год начала работ (число)
						"96ae2d78-70e1-49e5-8f84-bbf506ae1f10",//Год окончания работ (число)
						"a3d97553-558d-494b-b08e-74c475bec7b2",//Статус
						"da386618-6062-4ac5-9692-9982cdb254e3",//Дорога
						"24a44ea7-93fa-4a6c-82a6-e498830a765f",//Направление расходования средств
						"e13aa72a-0197-4063-8d84-a53a18f702f2",//Дата создания
						"422ce8cc-b534-49b8-93ba-78d61f81fbab" //Дата изменения
					},
					new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "ec9346f7-2460-4049-b7f8-49e807bc38ef",
							@operator = 1,
							fieldValue = "2c0c5e97-1c02-4e8e-926d-03f756bbfdc9"
						}
					}, new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "422ce8cc-b534-49b8-93ba-78d61f81fbab",
							@operator = 6,
							fieldValue = dt.ToString("s")+"Z"
						}
					}, offset, pageSize
				));


			return entityInfos.Where(info =>
					info.Values.ContainsKey("a52f275a-7589-4c85-a92b-7ca2c711447e") &&
					!string.IsNullOrEmpty(info.Values["a52f275a-7589-4c85-a92b-7ca2c711447e"].Item1) &&
					!string.IsNullOrEmpty(info.Values["da386618-6062-4ac5-9692-9982cdb254e3"].Item1) &&
					info.Values.ContainsKey("5c463311-b5bc-4cd4-9182-0c6f4c215275") &&
					info.Values.ContainsKey("67af7381-320e-47b7-9e85-349fa927b4c6") &&
					info.Values.ContainsKey("96ae2d78-70e1-49e5-8f84-bbf506ae1f10")
				).Select(info =>
				{
					try
					{
						_programObjectConnectionValues.Add(
										(
											UID: info.Id,
											programType: int.Parse(_programTypesMapping[info.Values["ec9346f7-2460-4049-b7f8-49e807bc38ef"].Item2]),
											startYear: int.Parse(info.Values["67af7381-320e-47b7-9e85-349fa927b4c6"].Item1),
											endYear: int.Parse(info.Values["96ae2d78-70e1-49e5-8f84-bbf506ae1f10"].Item1),
											fku: int.Parse(_fkusMapping[info.Values["a52f275a-7589-4c85-a92b-7ca2c711447e"].Item2]
											)));
						return new ProgramObjectInfo
						{
							Id = info.Id,
							ObjectName = info.Values["5c463311-b5bc-4cd4-9182-0c6f4c215275"].Item1,
							RegionId = int.Parse(_regionsMapping[info.Values["c1e5afed-214d-4b11-9f27-854ced671050"].Item2]),
							Status = info.Values["a3d97553-558d-494b-b08e-74c475bec7b2"].Item1,
							RoadSectionId = int.Parse(_roadSectionsMapping[info.Values["da386618-6062-4ac5-9692-9982cdb254e3"].Item2]),
							NrsId = null,
							Type = null,
							YearStart = int.Parse(info.Values["67af7381-320e-47b7-9e85-349fa927b4c6"].Item1),
							YearEnd = int.Parse(info.Values["96ae2d78-70e1-49e5-8f84-bbf506ae1f10"].Item1),
							ChangeDate = DateTime.Parse(info.Values["422ce8cc-b534-49b8-93ba-78d61f81fbab"].Item1)
						};
					}
					catch (Exception ex)
					{
						_logger.Error($"{ex.Message}\nObject is: {info}");
						return null;
					}
				}).Where(val => val != null).ToList();
		}

		/// <summary>
		/// Получение программ работ "Содержание ИССО" (2.3)
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<ProgramObjectInfo>> GetProgramObjectsThreeAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();
			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=a9ff91e2-337d-4c98-9d66-188c67805bce&_=1613030767702",
				_connector.CreateJsonContent(
					"279ca164-c0ba-4469-90f9-b0ecdb13e224",
					new List<string>
					{
						"5c463311-b5bc-4cd4-9182-0c6f4c215275",//Наименование объекта
						"ec9346f7-2460-4049-b7f8-49e807bc38ef",//Программа работ для ФДА
						"67af7381-320e-47b7-9e85-349fa927b4c6",//Год начала работ (число)
						"96ae2d78-70e1-49e5-8f84-bbf506ae1f10",//Год окончания работ (число)
						"c1e5afed-214d-4b11-9f27-854ced671050",//Регион
						"a52f275a-7589-4c85-a92b-7ca2c711447e",//ФКУ
						"a3d97553-558d-494b-b08e-74c475bec7b2",//Статус
						"da386618-6062-4ac5-9692-9982cdb254e3",//Дорога
						"93c3db8b-c329-4dd2-9c4c-1a86a6b71374",//Объект
						"8cb5a724-6078-4299-9899-1f78800fa6bb",//Тип объекта
						"e13aa72a-0197-4063-8d84-a53a18f702f2",//Дата создания
						"422ce8cc-b534-49b8-93ba-78d61f81fbab" //Дата изменения
					},
					new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "ec9346f7-2460-4049-b7f8-49e807bc38ef",
							@operator = 1,
							fieldValue = "7479a33b-977c-44cb-b799-490be34de4f3"
						}
					}
				, new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "422ce8cc-b534-49b8-93ba-78d61f81fbab",
							@operator = 6,
							fieldValue = dt.ToString("s")+"Z"
						}
					}, offset, pageSize
				));


			return entityInfos.Where(info =>
					info.Values.ContainsKey("a52f275a-7589-4c85-a92b-7ca2c711447e") &&
					!string.IsNullOrEmpty(info.Values["a52f275a-7589-4c85-a92b-7ca2c711447e"].Item1) &&
					!string.IsNullOrEmpty(info.Values["da386618-6062-4ac5-9692-9982cdb254e3"].Item1) &&
					!string.IsNullOrEmpty(info.Values["c1e5afed-214d-4b11-9f27-854ced671050"].Item1) &&
					info.Values.ContainsKey("5c463311-b5bc-4cd4-9182-0c6f4c215275") &&
					info.Values.ContainsKey("67af7381-320e-47b7-9e85-349fa927b4c6") &&
					info.Values.ContainsKey("96ae2d78-70e1-49e5-8f84-bbf506ae1f10")
				).Select(info =>
				{
					try
					{
						_programObjectConnectionValues.Add(
										(
											UID: info.Id,
											programType: int.Parse(_programTypesMapping[info.Values["ec9346f7-2460-4049-b7f8-49e807bc38ef"].Item2]),
											startYear: int.Parse(info.Values["67af7381-320e-47b7-9e85-349fa927b4c6"].Item1),
											endYear: int.Parse(info.Values["96ae2d78-70e1-49e5-8f84-bbf506ae1f10"].Item1),
											fku: int.Parse(_fkusMapping[info.Values["a52f275a-7589-4c85-a92b-7ca2c711447e"].Item2]
											)));
						return new ProgramObjectInfo
						{
							Id = info.Id,
							ObjectName = info.Values["5c463311-b5bc-4cd4-9182-0c6f4c215275"].Item1,
							RegionId = int.Parse(_regionsMapping[info.Values["c1e5afed-214d-4b11-9f27-854ced671050"].Item2]),
							Status = info.Values["a3d97553-558d-494b-b08e-74c475bec7b2"].Item1,
							RoadSectionId = int.Parse(_roadSectionsMapping[info.Values["da386618-6062-4ac5-9692-9982cdb254e3"].Item2]),
							NrsId = null,
							Type = info.Values["8cb5a724-6078-4299-9899-1f78800fa6bb"].Item1 == "Нормативное содержание" ? 1 : 2,
							IssoUid = Guid.TryParse(info.Values.TryGetValue("93c3db8b-c329-4dd2-9c4c-1a86a6b71374", out (string, string) iUid) ? iUid.Item2 : "", out Guid issoUid) ? issoUid : (Guid?)null,
							YearStart = int.Parse(info.Values["67af7381-320e-47b7-9e85-349fa927b4c6"].Item1),
							YearEnd = int.Parse(info.Values["96ae2d78-70e1-49e5-8f84-bbf506ae1f10"].Item1),
							ChangeDate = DateTime.Parse(info.Values["422ce8cc-b534-49b8-93ba-78d61f81fbab"].Item1)
						};
					}
					catch (Exception ex)
					{
						_logger.Error($"{ex.Message}\nObject is: {info}");
						return null;
					}
				}).Where(val => val != null).ToList();
		}

		/// <summary>
		/// Участки работ для объектов ПР "Кап. ремонт дорог ПИР/СМР", "Ремонт дорог ПИР/СМР/ШПО",
		/// "Содержание дорог" и "Содержание ИССО (норм. содержание)" (3.1)
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<RoadRepair>> GetRoadRepairAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();
			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=1be20f82-ff64-4b3a-bb38-920a8d7b2bfe&_=1613048079167",
				_connector.CreateJsonContent(
					"47f5d6b9-3c0c-43f0-a99a-93692565b05e",
					new List<string>
					{
						"4ef1b691-bbae-491f-8088-b65cdb4f15f1",//Наименование дороги
						"73598329-6859-4445-9388-b02d9c6a87f8",//Адрес начала, км
						"2ed3ac3d-e566-44dd-ad9f-7c3bee6980bb",//Адрес начала (приращение), м 
						"7bec8245-075e-4726-a32d-dacaf7e5ef0d",//Адрес конца, км
						"4683b0d3-23c0-47d5-b943-0a91452a088f",//Адрес конца (приращение), м
						"902fc999-544b-45b6-a963-6d95564b528e",//Объект ремонта автодорог
						"a47e1bd8-bf3d-4ba7-bcb1-99680c114b33",//Объект содержания автодорог
						"d346e882-ad1f-49f4-800a-6f1551adaa58",//Объект
						"2ef43c8b-5290-49e3-9068-32eacd2f73cf",//Дата создания
						"80a79e95-a5de-486a-b281-3d73d187d958", //Дата изменения
						"a0422c6b-844c-438a-9e64-5c769b695980",//Год

					}, null, new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "80a79e95-a5de-486a-b281-3d73d187d958",
							@operator = 6,
							fieldValue = dt.ToString("s")+"Z"
						}
					}, offset: offset, limit: pageSize
				));


			return entityInfos.Select(info =>
			{
				try
				{
					return new RoadRepair
					{
						ChildObjectUid = info.Id,
						RoadSectionId = GetRoadSectionId(info.Values),
						Start = int.Parse(info.Values["73598329-6859-4445-9388-b02d9c6a87f8"].Item1),
						StartAdd = int.Parse(info.Values["2ed3ac3d-e566-44dd-ad9f-7c3bee6980bb"].Item1),
						Finish = int.Parse(info.Values["7bec8245-075e-4726-a32d-dacaf7e5ef0d"].Item1),
						FinishAdd = int.Parse(info.Values.ContainsKey("4683b0d3-23c0-47d5-b943-0a91452a088f") ? info.Values["4683b0d3-23c0-47d5-b943-0a91452a088f"].Item1 : "0"),
						ObjectId = null,
						ObjectUid = GetObjectUid(info.Values),
						ChangeDate = DateTime.Parse(info.Values["80a79e95-a5de-486a-b281-3d73d187d958"].Item1)
					};
				}
				catch (Exception ex)
				{
					_logger.Error($"{ex.Message}\nObject is: {info}");
					return null;
				}
			}).Where(val => val != null).ToList();
		}

		/// <summary>
		/// Участки работ для объектов ПР "Кап. ремонт ИССО ПИР/СМР", "Ремонт ИССО ПИР/СМР" и "Содержание ИССО (ППР)" (3.2)
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<RoadRepairIsso>> GetRoadRepairIssoAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();
			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=1be20f82-ff64-4b3a-bb38-920a8d7b2bfe",
				_connector.CreateJsonContent(
					"47f5d6b9-3c0c-43f0-a99a-93692565b05e",
					new List<string>
					{
						"4ef1b691-bbae-491f-8088-b65cdb4f15f1",//Наименование дороги
						"d8b2ba99-6929-460e-886c-a7da9be027c8",//Адрес (км)
						"41c66c92-71fc-4dfc-8856-08757c602e2e",//Адрес (м)
						"d346e882-ad1f-49f4-800a-6f1551adaa58",//Объект
						"902fc999-544b-45b6-a963-6d95564b528e",//Объект ремонта автодорог
						"2ef43c8b-5290-49e3-9068-32eacd2f73cf",//Дата создания
						"80a79e95-a5de-486a-b281-3d73d187d958" //Дата изменения
					}, null, new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "80a79e95-a5de-486a-b281-3d73d187d958",
							@operator = 6,
							fieldValue = dt.ToString("s")+"Z"
						}
					}, offset, pageSize
				));


			return entityInfos.Select(info =>
			{
				try
				{
					return new RoadRepairIsso()
					{
						ChildObjectUid = info.Id,
						RoadSectionId = GetRoadSectionId(info.Values),
						Start = int.Parse(info.Values.TryGetValue("d8b2ba99-6929-460e-886c-a7da9be027c8", out (string, string) start) ? start.Item1 : "0"),
						StartAdd = int.Parse(info.Values.TryGetValue("41c66c92-71fc-4dfc-8856-08757c602e2e", out (string, string) startAdd) ? startAdd.Item1 : "0"),
						Name = info.Values["d346e882-ad1f-49f4-800a-6f1551adaa58"].Item1,
						IssoUid = Guid.TryParse(info.Values["d346e882-ad1f-49f4-800a-6f1551adaa58"].Item2, out Guid result) ? result : (Guid?)null,
						ObjectId = null,
						ObjectUid = GetObjectUidIsso(info.Values),
						ChangeDate = DateTime.Parse(info.Values["80a79e95-a5de-486a-b281-3d73d187d958"].Item1)
					};
				}
				catch (Exception ex)
				{
					_logger.Error($"{ex.Message}\nObject is: {info}");
					return null;
				}
			}).Where(val => val != null).ToList();


			Guid? GetObjectUidIsso(Dictionary<string, (string, string)> infoValues)
			{
				if (!string.IsNullOrEmpty(infoValues["902fc999-544b-45b6-a963-6d95564b528e"].Item1))
				{
					return Guid.Parse(infoValues["902fc999-544b-45b6-a963-6d95564b528e"].Item2);
				}
				throw new Exception(@"Нет значения для поля ""Объект ремонта автодорог""");
			}

		}

		/// <summary>
		/// Справочние ИССО
		/// </summary>
		/// <param name="offset">Смещение для получения данных (начиная с нуля)</param>
		/// <param name="pageSize">Количество получаемых данных</param>
		/// <param name="dt">Дата, начиная с которой необходимо получать значения</param>
		public async Task<List<IssoValue>> GetDictIssoAsync(int offset, int pageSize, DateTime dt = default)
		{
			await LoadDict();
			List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=9490c54b-8318-49b1-a28c-a1759a5ac806&_=1613371201694",
				_connector.CreateJsonContent(
					"c1a527ef-5dfd-4833-a966-b6d822cbafe9",
					new List<string>
					{
						"0201a8c0-1eec-4b61-bfe9-88eb359daba5",//Дата создания
						"c433e697-1ecd-47ce-877c-623aec0cf268",//Дата изменения
						"07cd56f5-6a46-46fc-bfef-2d328273109c",//Длина сооружения, пог.м.
						"34ccdb7f-67f5-40ad-ad48-36a22b42740e",//Оценка сооружения, категория дефекта по диагностике 
						"4e98b73d-ba52-4044-a107-95fb6f676995",//Габарит проезжей части сооружения
						"f10d8eef-5f91-43c7-b424-ed3dda679d24",//Год постройки/реконструкции сооружения
						"4d952e9f-8913-4599-9f4e-290c5a02cc55",//Грузоподьемность фактическая в неконтролируемом режиме/ в контролируемом режиме
						"8d52b5fb-472e-48f8-ba3c-e8a258303e9a",//Год проведения диагностики
						"e9240e5e-ab13-4dae-ae7a-d7d12e8f6143",//ФКУ
						"bf4b8088-b6ef-439f-a869-22adbeaff16b",//ID
						"8d2a65aa-6ef9-4362-8612-b6d0189b56a3",//Дорога
						"42542927-e583-4cf5-b78e-83388322dbba",//Название сооружения
						"5241355a-cc62-4f77-aaca-b3b35ce1dbaf",//Описание
						"2678f881-340d-4b63-ac1e-7d03af20d871",//Тип сооружений
						"3c88a26a-140a-4222-892b-c14082f0113e",//Регион
						"f6e4e97d-675f-4da9-9ea3-46b9432c78b5" //Категория
					}, null, new List<FilterOperator>
					{
						new FilterOperator
						{
							fieldId = "c433e697-1ecd-47ce-877c-623aec0cf268",
							@operator = 6,
							fieldValue = dt.ToString("s")+"Z"
						}
					}, offset: offset, limit: pageSize
				));


			return entityInfos.Select(info =>
			{
				try
				{
					return new IssoValue()
					{
						ObjectUid = info.Id,
						AbdmId = int.TryParse(info.Values.TryGetValue("bf4b8088-b6ef-439f-a869-22adbeaff16b", out (string, string) abdmId) ? abdmId.Item1 : "", out int id) ? id : (int?)null,
						Road = info.Values["8d2a65aa-6ef9-4362-8612-b6d0189b56a3"].Item1,
						RoadUid = Guid.TryParse(info.Values["8d2a65aa-6ef9-4362-8612-b6d0189b56a3"].Item2, out Guid roadUid) ? roadUid : (Guid?)null,
						Name = info.Values.TryGetValue("42542927-e583-4cf5-b78e-83388322dbba", out (string, string) name) ? name.Item1 : "",
						Description = info.Values["5241355a-cc62-4f77-aaca-b3b35ce1dbaf"].Item1,
						Type = info.Values["2678f881-340d-4b63-ac1e-7d03af20d871"].Item1,
						TypeUid = Guid.TryParse(info.Values["2678f881-340d-4b63-ac1e-7d03af20d871"].Item2, out Guid typeUid) ? typeUid : (Guid?)null,
						Region = info.Values["3c88a26a-140a-4222-892b-c14082f0113e"].Item1,
						RegionUid = Guid.TryParse(info.Values["3c88a26a-140a-4222-892b-c14082f0113e"].Item2, out Guid regionUid) ? regionUid : (Guid?)null,
						Category = info.Values.TryGetValue("f6e4e97d-675f-4da9-9ea3-46b9432c78b5", out (string, string) category) ? category.Item1 : "",
						Length = int.TryParse(info.Values.TryGetValue("07cd56f5-6a46-46fc-bfef-2d328273109c", out (string, string) len) ? len.Item1 : "", out int length) ? length : (int?)null,
						Evaluation = info.Values.TryGetValue("34ccdb7f-67f5-40ad-ad48-36a22b42740e", out (string, string) evaluation) ? evaluation.Item1 : "",
						RoadSize = int.TryParse(info.Values.TryGetValue("4e98b73d-ba52-4044-a107-95fb6f676995", out (string, string) rSize) ? rSize.Item1 : "", out int roadSize) ? roadSize : (int?)null,
						BuildReconYear = info.Values.TryGetValue("f10d8eef-5f91-43c7-b424-ed3dda679d24", out (string, string) buildReconYear) ? buildReconYear.Item1 : "",
						Capacity = int.TryParse(info.Values.TryGetValue("4d952e9f-8913-4599-9f4e-290c5a02cc55", out (string, string) cap) ? cap.Item1 : "", out int capacity) ? capacity : (int?)null,
						DiagnosticsYear = DateTime.TryParse(info.Values.TryGetValue("8d52b5fb-472e-48f8-ba3c-e8a258303e9a", out (string, string) dYear) ? dYear.Item1 : "", out DateTime diagnosticsYear) ? diagnosticsYear : (DateTime?)null,
						Fku = info.Values.TryGetValue("e9240e5e-ab13-4dae-ae7a-d7d12e8f6143", out (string, string) fku) ? fku.Item1 : "",
						FkuUid = Guid.TryParse(info.Values["e9240e5e-ab13-4dae-ae7a-d7d12e8f6143"].Item2, out Guid fkuUid) ? fkuUid : (Guid?)null,
						ChangeDate = DateTime.Parse(info.Values["c433e697-1ecd-47ce-877c-623aec0cf268"].Item1)
					};
				}
				catch (Exception ex)
				{
					_logger.Error($"{ex.Message}\nObject is: {info}");
					return null;
				}
			}).Where(val => true).ToList();
		}

		/// <summary>
		/// Получение объемов работ по годам для типов ПР: рем/капремонт дорог/ИССО СМР (2.2 Паспорт объекта)
		/// </summary>
		/// <param name="itemId">идентификатор объекта ПР</param>
		public async Task<string> GetVolumeOfWorkByYearAsync(string itemId)
		{
			try
			{
				await LoadDict();
				List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=4670a8c9-89ea-40a2-8991-9cc4315f175e&_=1613725124924",
					_connector.CreateJsonContent(
						"e931582b-499a-4187-9ada-efc48d9da43d",
						new List<string>
						{
						"fc8c2785-6351-4f79-b7ea-28c9b276da02",//Дата создания
						"85ce2449-ff78-4d20-bd82-822506a68a0d",//Дата изменения
						"2d8d5fab-554d-41a5-9bb6-614e7cb5f12b",//Статья
						"0e0a7754-9dc6-40c7-9e49-1d2f6b630477",//Объект
						"9339385d-71a0-4195-9486-ca8aa044e283",//Объект ремонта автодорог
						"72040b49-9411-4ff7-b794-d1c1bbd16c41",//Объемы работ (прокси)
						"7e7a8c55-7c1f-49a2-9e26-73e11a761336",//Год
						"fb3f0fd9-51cd-4a15-8d67-941f3d01538b",//Единица измерения
						"fc583d11-fc95-408c-8601-9c81da86020d",//Кем создано
						"c4be51bd-d231-4bb9-9320-f70b38292c2f",//Физ. объем на год
						"f501ef8f-9d5e-4efe-b671-a7e71295aeba",//Физ. объем всего
						"e04f9759-735d-4c58-9f77-e719a5682e09",//На год, тыс.руб
						"7dfef9ec-7967-45ef-81d6-c1389f17f46a",//Всего (тыс.руб)
						"f255f770-3a09-4b96-b3fb-f0e282f568ae" //Права для ФКУ

						}, new List<FilterOperator>
						{
						new FilterOperator
						{
							fieldId = "9339385d-71a0-4195-9486-ca8aa044e283",
							@operator = 1,
							fieldValue = itemId
						}
						}, null, 0, 100
					));

				var result = entityInfos.Select(info => new
				{
					year = (info.Values.TryGetValue("7e7a8c55-7c1f-49a2-9e26-73e11a761336", out (string, string) year) ? year.Item1 : "") ?? "",
					value = (info.Values.TryGetValue("c4be51bd-d231-4bb9-9320-f70b38292c2f", out (string, string) value) ? value.Item1 : "") ?? ""
				}).ToList();

				dynamic jsonObject = result.Any() ? result : null; 
				//File.WriteAllText(@"d:/result.json", JsonConvert.SerializeObject(jsonObject));
				return JsonConvert.SerializeObject(jsonObject);
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Получение объемов работ по статьям по объекту программы работ ремонтов/капремонтов дорог и ИССО (2.3 Паспорт объекта)
		/// </summary>
		/// <param name="itemId">идентификатор объекта ПР</param>
		public async Task<string> GetJobAmountArticleAsync(string itemId)
		{
			try
			{
				await LoadDict();
				List<EntityInfo> entityInfos = await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=4670a8c9-89ea-40a2-8991-9cc4315f175e&_=1613725124924",
					_connector.CreateJsonContent(
						"e931582b-499a-4187-9ada-efc48d9da43d",
						new List<string>
						{
						"fc8c2785-6351-4f79-b7ea-28c9b276da02",//Дата создания
						"85ce2449-ff78-4d20-bd82-822506a68a0d",//Дата изменения
						"2d8d5fab-554d-41a5-9bb6-614e7cb5f12b",//Статья
						"0e0a7754-9dc6-40c7-9e49-1d2f6b630477",//Объект
						"9339385d-71a0-4195-9486-ca8aa044e283",//Объект ремонта автодорог
						"72040b49-9411-4ff7-b794-d1c1bbd16c41",//Объемы работ (прокси)
						"7e7a8c55-7c1f-49a2-9e26-73e11a761336",//Год
						"fb3f0fd9-51cd-4a15-8d67-941f3d01538b",//Единица измерения
						"fc583d11-fc95-408c-8601-9c81da86020d",//Кем создано
						"c4be51bd-d231-4bb9-9320-f70b38292c2f",//Физ. объем на год
						"f501ef8f-9d5e-4efe-b671-a7e71295aeba",//Физ. объем всего
						"e04f9759-735d-4c58-9f77-e719a5682e09",//На год, тыс.руб
						"7dfef9ec-7967-45ef-81d6-c1389f17f46a",//Всего (тыс.руб)
						"f255f770-3a09-4b96-b3fb-f0e282f568ae" //Права для ФКУ

						}, new List<FilterOperator>
						{
						new FilterOperator
						{
							fieldId = "9339385d-71a0-4195-9486-ca8aa044e283",
							@operator = 1,
							fieldValue = itemId
						}
						}, offset: 0, limit: 100
					));



				dynamic jsonObject = new ExpandoObject();

				var result = entityInfos.GroupBy(val =>
				{
					if (!val.Values.TryGetValue("2d8d5fab-554d-41a5-9bb6-614e7cb5f12b", out _))
					{
						throw new Exception($@"Не найдено поле ""Статья"" с идентификатором: 2d8d5fab-554d-41a5-9bb6-614e7cb5f12b для itemId={itemId}");
					}
					return val.Values["2d8d5fab-554d-41a5-9bb6-614e7cb5f12b"];

				}).Select(val => val);

				jsonObject = result.Select(v => v.ToList()).Select(s =>
				 {
					 return new
					 {
						 articleName = s[0].Values["2d8d5fab-554d-41a5-9bb6-614e7cb5f12b"].Item1,
						 workOfTypeName = "Empty",
						 articleValues = s.Select(val =>
						 {
							 return new
							 {
								 year = int.TryParse(val.Values.TryGetValue("7e7a8c55-7c1f-49a2-9e26-73e11a761336", out (string, string) yearStr) ? yearStr.Item1 : "", out int year) ? year : (int?)null,
								 value = double.TryParse(val.Values.TryGetValue("e04f9759-735d-4c58-9f77-e719a5682e09", out (string, string) valueStr) ? valueStr.Item1 : "", out double value) ? value : (double?)null
							 };
						 })
					 };
				 }).ToList();

				return JsonConvert.SerializeObject(jsonObject);
			}
			catch (Exception ex)
			{
				_logger.Error(ex.Message);
				return null;
			}
		}

		private int? GetRoadSectionId(Dictionary<string, (string, string)> infoValues)
		{
			if (_roadSectionsMapping.ContainsKey(infoValues["4ef1b691-bbae-491f-8088-b65cdb4f15f1"].Item2))
			{
				return int.Parse(_roadSectionsMapping[infoValues["4ef1b691-bbae-491f-8088-b65cdb4f15f1"].Item2]);
			}

			return null;
		}

		private Guid? GetObjectUid(Dictionary<string, (string, string)> infoValues)
		{
			if (!string.IsNullOrEmpty(infoValues["902fc999-544b-45b6-a963-6d95564b528e"].Item1))
			{
				return Guid.Parse(infoValues["902fc999-544b-45b6-a963-6d95564b528e"].Item2);
			}

			if (!string.IsNullOrEmpty(infoValues["a47e1bd8-bf3d-4ba7-bcb1-99680c114b33"].Item1))
			{
				return Guid.Parse(infoValues["a47e1bd8-bf3d-4ba7-bcb1-99680c114b33"].Item2);
			}

			return Guid.Empty;
		}

		private async Task LoadDict()
		{
			if (!_programTypesMapping.Any())
			{
				_programTypesMapping = await GetProgramTypesMappingAsync();
			}

			if (!_fkusMapping.Any())
			{
				_fkusMapping = await GetFkusMappingAsync();
			}

			if (!_regionsMapping.Any())
			{
				_regionsMapping = await GetRegionsMappingAsync();
			}

			if (!_roadSectionsMapping.Any())
			{
				_roadSectionsMapping = await GetRoadSectionsMappingAsync();
			}
		}
		private async Task<Dictionary<string, string>> GetProgramTypesMappingAsync()
		{
			StringContent content = _connector.CreateJsonContent(
				"7409df57-57b2-4c1f-83d3-b4664fe8fced",
				new List<string>
				{
					"5959b88a-39a0-4446-9c21-521ce5766b9a"//ИД
				},
				new List<FilterOperator>
				{
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 1},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 2},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 3},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 4},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 23},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 5},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 21},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 22},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 19},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 20},
					new FilterOperator{fieldId = "5959b88a-39a0-4446-9c21-521ce5766b9a",@operator = 1,fieldValue = 12},
				},
				offset: 0,
				limit: 30000);

			Dictionary<string, string> mappingDict = new Dictionary<string, string>();
			foreach (EntityInfo info in await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=fd4305c7-3d45-412a-91bf-3cc33a1ecf71&_=1612512770619", content))
			{
				mappingDict.Add(info.Id.ToString(), info.Values["5959b88a-39a0-4446-9c21-521ce5766b9a"].Item1);
				_programTypesFilterValues.Add(info.Id.ToString());
			}

			return mappingDict;
		}
		private async Task<Dictionary<string, string>> GetFkusMappingAsync()
		{
			StringContent content = _connector.CreateJsonContent(
				"8df73b8d-f705-451d-97a5-e206638eab55",
				new List<string>
				{
					"56565f7f-7df4-431d-8c5d-484504a88f67"//ИД
				},
				limit: 1000);

			Dictionary<string, string> mappingDict = new Dictionary<string, string>();
			foreach (EntityInfo info in await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=cd8ae744-d89c-4bfe-b903-8fbc010a2e7f&_=1612515795408", content))
			{
				mappingDict.Add(info.Id.ToString(), info.Values["56565f7f-7df4-431d-8c5d-484504a88f67"].Item1);
			}

			return mappingDict;
		}
		private async Task<Dictionary<string, string>> GetRegionsMappingAsync()
		{
			StringContent content = _connector.CreateJsonContent(
				workspaceId: "277afb1e-f0dc-44ea-afda-f0575c84efcd",
				viewFields: new List<string>
				{
					"9a44b4a8-3378-4405-aefe-c04636c69629"//ИД
				}, limit: 5000);

			Dictionary<string, string> mappingDict = new Dictionary<string, string>();
			foreach (EntityInfo info in await GetEntityInfosAsync($"{BaseUrl}/gridview?definitionId=2189fc82-e2c2-4e86-b7d9-16874824b878&_=1612851703671", content))
			{
				mappingDict.TryAdd(info.Id.ToString(), info.Values["9a44b4a8-3378-4405-aefe-c04636c69629"].Item1);
			}

			return mappingDict;
		}
		private async Task<Dictionary<string, string>> GetRoadSectionsMappingAsync()
		{
			StringContent content = _connector.CreateJsonContent(
				workspaceId: "06c8d042-c446-484d-87c5-679274d5c716",
				viewFields: new List<string>
				{
					"f1beb7f0-38c1-4287-a547-a848d40565a9",//ИД
					"01689068-54d8-48f9-b1ec-b6b1f3a9d24f",//Полное наименование
				}, new List<FilterOperator>
				{
					new FilterOperator
					{
						fieldId = "3de5ad41-4729-48cf-b5a7-1480151908d2",
						@operator = 1,
						fieldValue = false
					},
					new FilterOperator
					{
						fieldId = "3de5ad41-4729-48cf-b5a7-1480151908d2",
						@operator = 1,
						fieldValue = null
					}
				}, limit: 10000);

			Dictionary<string, string> mappingDict = new Dictionary<string, string>();
			foreach (EntityInfo info in await GetEntityInfosAsync(
				$"{BaseUrl}/gridview?definitionId=47794b6a-c72d-4c0a-b0d9-3cb0a2c11813&_=1612873559550", content))
			{

				if (info.Values.ContainsKey("01689068-54d8-48f9-b1ec-b6b1f3a9d24f"))
				{
					mappingDict.TryAdd(info.Id.ToString(), info.Values["f1beb7f0-38c1-4287-a547-a848d40565a9"].Item1);
				}
			}

			return mappingDict;
		}

		private async Task<List<EntityInfo>> GetEntityInfosAsync(string url, StringContent content)
		{
			dynamic resultObject = null;
			try
			{
				resultObject = await _connector.GetJsonResultAsync(url, HttpMethod.Post, content);
				if (resultObject == null)
				{
					_logger.Info($"resultObject равен NULL. Url = {url}, Content = {await content.ReadAsStringAsync()}");
				}
				else
				{
					_logger.Info($"Данные успешно получены с Url = {url}, Content = {await content.ReadAsStringAsync()}");
				}

			}
			catch (Exception ex)
			{
				_logger.Error($"Ошибка обращения к API: {ex.StackTrace}");
			}



			List<EntityInfo> entityInfos = new List<EntityInfo>();
			return ParseInfo(resultObject, entityInfos);
		}
		//Парсинг полученной информации
		private List<EntityInfo> ParseInfo(dynamic resultObject, List<EntityInfo> entityInfos)
		{
			CreateDicts(resultObject);
			LastTotalCount = int.TryParse(resultObject.totalCount.ToString(), out int count) ? count : 0;

			foreach (dynamic value in resultObject.rows)
			{
				CreateEntityInfo(value, entityInfos);
			}

			return entityInfos;
		}
		//Создание сущности со всей полученной информацией
		private void CreateEntityInfo(dynamic value, List<EntityInfo> entityInfos)
		{
			EntityInfo entityInfo = new EntityInfo() { Id = Guid.Parse(value.id.ToString()) };

			foreach (dynamic cell in value.cells)
			{
				try
				{
					switch (_rowsMapping[cell.id.ToString()].type)
					{
						case "1":
							entityInfo.Values.Add(cell.id.ToString(), new ValueTuple<string, string>(cell.value.stringValue?.ToString(), ""));
							break;
						case "5":
							entityInfo.Values.Add(cell.id.ToString(), new ValueTuple<string, string>(_statuses[cell.value[0].ToString()], ""));
							break;
						case "7":
							entityInfo.Values.Add(cell.id.ToString(), new ValueTuple<string, string>(
								cell.value.Count > 0 ? _lookups[cell.value[0].ToString()] : "", cell.value.Count > 0 ? cell.value[0].ToString() : ""));
							break;
						case "9":
							entityInfo.Values.Add(cell.id.ToString(),
								new ValueTuple<string, string>(cell.value.Count > 0 ? _members[cell.value[0].ToString()] : "", ""));
							break;
						default:
							entityInfo.Values.Add(cell.id.ToString(), new ValueTuple<string, string>(cell.value.ToString(), ""));
							break;
					}
				}
				catch (Exception ex)
				{
					Exception e = ex;
				}
			}

			entityInfos.Add(entityInfo);
		}
		private void CreateDicts(dynamic resultObject)
		{
			CreateColumnsInfoDict(resultObject);

			CreateLookupsDict(resultObject);

			CreateMembersDict(resultObject);

			CreateStatusesDict(resultObject);
		}

		//Создание словаря сопоставлений информации по столбцам
		private void CreateColumnsInfoDict(dynamic dynObject)
		{
			_rowsMapping.Clear();
			foreach (dynamic value in dynObject.columns)
			{
				try
				{
					_rowsMapping.Add(value.id.ToString(),
						new { title = value.title.ToString(), type = value.field.type.ToString() });
				}
				catch (Exception e)
				{
					_logger.Error($"Ошибка создания словаря сопоставлений информации по столбцам: {e.Message}");
				}
			}
		}

		//Создание словаря сопоставлений списков выбора
		private void CreateLookupsDict(dynamic dynObject)
		{
			_lookups.Clear();
			foreach (dynamic value in dynObject.lookups)
			{
				_lookups.TryAdd(value.id.ToString(), value.value?.ToString());
			}
		}

		//Создание словаря сопоставлений списков пользователей
		private void CreateMembersDict(dynamic dynObject)
		{
			_members.Clear();
			foreach (dynamic value in dynObject.members)
			{
				try
				{
					_members.Add(value.id.ToString(), value.title.ToString());
				}
				catch (Exception e)
				{
					_logger.Error($"Ошибка создания словаря сопоставлений списка пользователей: {e.Message}");
				}
			}
		}

		//Создание словаря сопоставлений статусов
		private void CreateStatusesDict(dynamic resultObject)
		{
			_statuses.Clear();
			foreach (dynamic column in resultObject.columns)
			{
				if (column.field?.settings?.options != null)
				{
					foreach (dynamic option in column.field.settings.options)
					{
						_statuses.Add(option.id.ToString(), option.title.stringValue.ToString());
					}
				}
			}

		}
	}
}
