using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PerformikaLib.Entities;

namespace PerformikaLib
{
	/// <summary>
	/// Получение данных через GET-запросы
	/// </summary>
	public class PerformikaGetModule
	{
		private readonly PerformikaApiConnector _connector;

		private Logger _logger = LogManager.GetCurrentClassLogger();

		public string BaseUrl { get; }
		public PerformikaGetModule(string url, string login, string password)
		{
			_connector = new PerformikaApiConnector(url, login, password);
			BaseUrl = url;
		}
		private readonly Dictionary<string, string> _lookups = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _members = new Dictionary<string, string>();

		/// <summary>
		/// Получение общей информации по объекту программы работ ремонтов/капремонтов дорог и ИССО (1.1 Паспорт объекта)
		/// </summary>
		/// <param name="itemId">идентификатор объекта ПР</param>
		public async Task<string> GetMainObjectInfoAsync(string itemId)
		{
			EntityInfo info = await GetEntityInfoAsync($"{BaseUrl}/page/17f17ec7-8d7c-41fe-8014-b6aa43e9bbf5?itemId={itemId}&suppressCondition=true&modalSuppressCondition=false&_=1613473985410");

			dynamic jsonObject = new ExpandoObject();

			jsonObject.name = (info.Values.TryGetValue("84bd7d17-e75c-4761-8ae7-31f0423256d3", out (string, string) name) ? name.Item1 : "") ?? "";
			jsonObject.region = (info.Values.TryGetValue("ed9a438a-4a36-4db3-9677-71db960c0606", out (string, string) region) ? region.Item1 : "") ?? "";
			jsonObject.agglomeration = (info.Values.TryGetValue("40aba7fb-da11-4c75-9573-a8801498d6b0", out (string, string) agglomeration) ? agglomeration.Item1 : "") ?? "";
			jsonObject.termsOfSMR = (info.Values.TryGetValue("bf06ff5d-b3f0-4297-b019-d52d89754d06", out (string, string) termsOfSMR) ? termsOfSMR.Item1 : "") ?? "";

			string start = (info.Values.TryGetValue("475efcd1-b82e-4218-a52d-fa5ed5db51ef", out (string, string) yearStart) ? yearStart.Item1 : "") ?? "";
			jsonObject.yearStart = start;

			string end = (info.Values.TryGetValue("71e3652a-1c82-4555-8fb3-131c4e3f1710", out (string, string) yearEnd) ? yearEnd.Item1 : "") ?? "";
			jsonObject.yearEnd = end;

			jsonObject.jobType = (info.Values.TryGetValue("bcff23c5-259e-44c6-b105-39ebf8097627", out (string, string) jobType) ? jobType.Item1 : "") ?? "";
			jsonObject.availabilityPSD = (info.Values.TryGetValue("5ecd747c-6555-4928-adc3-9fe98d6082c8", out (string, string) availabilityPSD) ? availabilityPSD.Item1 : "") ?? "";
			jsonObject.highCostReasons = (info.Values.TryGetValue("896cd9b8-491f-44f1-8035-b01c7a4c58c0", out (string, string) highCostReasons) ? highCostReasons.Item1 : "") ?? "";
			jsonObject.numberDateExpertise = (info.Values.TryGetValue("a1b594f6-6148-40b9-986b-54fba519ca96", out (string, string) numberDateExpertise) ? numberDateExpertise.Item1 : "") ?? "";
			jsonObject.transferYears = $"{start} - {end}";

			return JsonConvert.SerializeObject(jsonObject);
		}

		/// <summary>
		/// Получение сведений по объемам работ по объекту программы работ ремонтов/капемонтов дорог и ИССО (2.1 Паспорт объекта)
		/// </summary>
		/// <param name="itemId">идентификатор объекта ПР</param>
		public async Task<string> GetJobAmountAsync(string itemId)
		{
			EntityInfo info = await GetEntityInfoAsync($"{BaseUrl}/page/c06f69ab-2c5e-4eb9-9809-7df25db108c6?itemId={itemId}&suppressCondition=true&modalSuppressCondition=false&_=1613473985410");

			dynamic jsonObject = new ExpandoObject();

			jsonObject.amountOfWorkKmPSD = (info.Values.TryGetValue("3c9a5e32-1a8e-4434-8b4c-27c328fedf1c", out (string, string) amountOfWorkKmPSD) ? amountOfWorkKmPSD.Item1 : "") ?? "";
			jsonObject.repairAreaPSD = (info.Values.TryGetValue("347b36d9-a43b-443a-a150-44b3a5519c29", out (string, string) repairAreaPSD) ? repairAreaPSD.Item1 : "") ?? "";
			jsonObject.amountOfWorkPogMPSD = (info.Values.TryGetValue("3a507124-5a13-496c-abd5-cc51431e2386", out (string, string) amountOfWorkPogMPSD) ? amountOfWorkPogMPSD.Item1 : "") ?? "";

			jsonObject.amountOfWorkRubPSD = (info.Values.TryGetValue("a1b1d5d1-8181-4a7a-871d-076576035c86", out (string, string) amountOfWorkRubPSD) ? amountOfWorkRubPSD.Item1 : "") ?? "";
			jsonObject.lengthRoadRepair = (info.Values.TryGetValue("474aa1ea-6c63-419f-bcfc-d768097618ae", out (string, string) lengthRoadRepair) ? lengthRoadRepair.Item1 : "") ?? "";
			jsonObject.repairArea = (info.Values.TryGetValue("2a3e4e7b-463b-4637-b121-ad5eeb78b9a0", out (string, string) repairArea) ? repairArea.Item1 : "") ?? "";

			jsonObject.costPIRCalculation = (info.Values.TryGetValue("0b05796e-2a3a-4ad8-831c-ca1d4172257e", out (string, string) costPIRCalculation) ? costPIRCalculation.Item1 : "") ?? "";
			jsonObject.jobAmount = (info.Values.TryGetValue("15433973-2604-4b00-9fad-e8ba9f831a39", out (string, string) jobAmount) ? jobAmount.Item1 : "") ?? "";
			jsonObject.jobAmountKm = (info.Values.TryGetValue("6d12a032-91ac-4c5e-93e3-88484c444353", out (string, string) jobAmountKm) ? jobAmountKm.Item1 : "") ?? "";


			return JsonConvert.SerializeObject(jsonObject);
		}

		//Получение сырой информации
		private async Task<EntityInfo> GetEntityInfoAsync(string url)
		{
			var resultObject = await _connector.GetJsonResultAsync(url,
				HttpMethod.Get, null);

			CreateLookupsDict(resultObject);
			CreateMembersDict(resultObject);
			EntityInfo entityInfo = new EntityInfo { Id = resultObject.item.id };
			foreach (dynamic controlValue in resultObject.controlValues)
			{
				try
				{
					if (controlValue.control.type != "10000000" && controlValue.control.type != "10000027")
					{
						switch (controlValue.control.type.ToString())
						{

							case "1":
								entityInfo.Values.Add(controlValue.id.ToString(), new ValueTuple<string, string>(controlValue?.value?.stringValue?.ToString(), ""));
								break;
							case "7":
								entityInfo.Values.Add(controlValue.id.ToString(), new ValueTuple<string, string>(
									controlValue.value.Count > 0 ? _lookups[controlValue.value[0].ToString()] : "", controlValue.value.Count > 0 ? controlValue.value[0].ToString() : ""));
								break;
							case "8":
								entityInfo.Values.Add(controlValue.id.ToString(), new ValueTuple<string, string>(controlValue.value?.ToString(), ""));
								break;
							case "9":
								entityInfo.Values.Add(controlValue.id.ToString(),
									new ValueTuple<string, string>(controlValue.value.Count > 0 ? _members[controlValue.value[0].ToString()] : "", ""));
								break;
							default:
								entityInfo.Values.Add(controlValue.id.ToString(), new ValueTuple<string, string>(controlValue.value?.ToString(), ""));
								break;
						}
					}
				}
				catch (Exception ex)
				{
					Exception e = ex;
				}
			}


			return entityInfo;
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
					_logger.Error($"Ошибка создания словаря сопоставлений списков пользователей: {e.Message}");
				}
			}
		}

	}
}
