using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace PerformikaLib
{
	/// <summary>
	/// Класс для подключения к Перформике, формирования запросов, получения "сырых" данных
	/// </summary>
	internal class PerformikaApiConnector
	{
		private Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly string _url;
		private readonly string _login;
		private readonly string _password;
		internal AuthData AuthData { get; set; }

		internal PerformikaApiConnector(string url, string login, string password)
		{
			_url = url;
			_login = login;
			_password = password;
		}

		//Подключение к Перформике
		private async Task<bool> Connect()
		{
			if (AuthData != null && AuthData.Expiration > DateTime.Now)
			{
				return true;
			}
			HttpClient client = new HttpClient();

			StringContent content = new StringContent(
				JsonConvert.SerializeObject(
					new { login = _login, password = _password }),
				Encoding.UTF8,
				"application/json");

			_logger.Info($"Попытка получения токена по адресу: '{_url}/auth/login'");
			HttpResponseMessage result = await client.PostAsync($"{_url}/auth/login", content);
			_logger.Info($"Результат получения токена: {result.IsSuccessStatusCode} - {result.StatusCode}");
			StreamReader reader = new StreamReader(await result.Content.ReadAsStreamAsync());

			try
			{
				dynamic auth = JsonConvert.DeserializeObject(reader.ReadToEnd());
				AuthData = new AuthData()
				{
					Token = auth.token,
					RefreshToken = auth.refreshToken,
					Expiration = DateTime.Parse(auth.expiration.ToString()),
					AutorizationMethod = auth.autorizationMethod
				};
			}
			catch (Exception ex)
			{
				_logger.Error($"{ex.Message}: {ex.StackTrace}");
				return false;
			}

			return true;
		}

		//Получение информации от API Перформики
		internal async Task<dynamic> GetJsonResultAsync(string url, HttpMethod httpMethod, StringContent content)
		{
			if (!await Connect())
			{
				throw new AuthenticationException("Ошибка получения токена.");
			}

			HttpClient client = new HttpClient();


			HttpRequestMessage request = new HttpRequestMessage
			{
				Content = content,
				Method = httpMethod,
				RequestUri =
					new Uri(url),
				Headers =
				{
					{"accesstoken", AuthData.Token}
				}
			};


			HttpResponseMessage result = await client.SendAsync(request);

			StreamReader reader = new StreamReader(await result.Content.ReadAsStreamAsync());
			string jsonResult = reader.ReadToEnd();

			dynamic resultObject = JsonConvert.DeserializeObject(jsonResult);
			//File.WriteAllText(@"d:/tester/result.json", resultObject.ToString());
			return resultObject;
		}

		//Подготовка Json контента
		internal StringContent CreateJsonContent(string workspaceId, List<string> viewFields, List<FilterOperator> filters = null, List<FilterOperator> addedFilters = null, int offset = 0, int limit = 30, int sortDirection = 0)
		{

			dynamic d = new ExpandoObject();

			d.limit = limit;
			d.offset = offset;
			d.workspaceId = workspaceId;
			d.viewFields = viewFields;
			d.sort = new { direction = sortDirection };
			d.filter = new
			{
				filterUnits = new List<dynamic>(),
				logicalUnits = new List<dynamic>
				{
					new
					{
						@operator = 2,
						logicalUnits = new List<dynamic>(),
						filterUnits = filters
					},
					new
					{
						@operator = 2,
						logicalUnits = new List<dynamic>(),
						filterUnits = addedFilters
					}
				},
				@operator = 1
			};

			//File.WriteAllText(@"d:/out2.json", JsonConvert.SerializeObject(d));
			return new StringContent(JsonConvert.SerializeObject(d), Encoding.UTF8, "application/json"); ;
		}
	}
}