using System;

namespace PerformikaLib.Entities
{
	/// <summary>
	/// Программа работ
	/// </summary>
	public class ProgramInfo
	{
		public Guid Id { get; set; }
		public int ProgramType { get; set; }
		public int Year { get; set; }
		public int Fku { get; set; }

		public string Status { get; set; }
		public DateTime ChangeDate { get; set; }

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(ProgramType)}: {ProgramType}, {nameof(Year)}: {Year}, {nameof(Fku)}: {Fku}, {nameof(Status)}: {Status}, {nameof(ChangeDate)}: {ChangeDate}";
		}
	}
}
