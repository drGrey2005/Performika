using System;
using System.Collections.Generic;
using System.Linq;

namespace PerformikaLib.Entities
{
	/// <summary>
	/// Унифицированный класс представляющий информацию полученную из перформики
	/// </summary>
	public class EntityInfo
	{
		public Guid Id { get; set; }

		public override string ToString()
		{
			string vals = string.Join(" ", Values.Select(d => $"{d.Key}: {d.Value.Item1} - {d.Value.Item2}\n").ToArray());

			return $"{nameof(Id)}: {Id}, \n{nameof(Values)}:\n {vals}";
		}

		/// <summary>
		/// Словарь ключами которого являются значения GUID полей, а значениями - значение поля и, если присутствует, то ссылка на справочник
		/// </summary>
		public Dictionary<string, (string, string)> Values { get; set; } = new Dictionary<string, (string, string)>();
	}
}
