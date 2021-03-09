using System;

namespace PerformikaLib.Entities
{
	public class ProgramObjectInfo
	{
		public Guid Id { get; set; }
		public string ObjectName { get; set; }
		public int RegionId { get; set; }
		public string Status { get; set; }
		public int? RoadSectionId { get; set; }
		public int? NrsId { get; set; }
		public int? Type { get; set; }
		public string IssoName { get; set; }
		public Guid? IssoUid { get; set; }
		public int YearStart { get; set; }
		public int YearEnd { get; set; }
		public DateTime ChangeDate { get; set; }

		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(ObjectName)}: {ObjectName}, {nameof(RegionId)}: {RegionId}, {nameof(Status)}: {Status}, {nameof(RoadSectionId)}: {RoadSectionId}, {nameof(NrsId)}: {NrsId}, {nameof(Type)}: {Type},{nameof(IssoName)}: {IssoName},{nameof(IssoUid)}: {IssoUid}, {nameof(YearStart)}: {YearStart}, {nameof(YearEnd)}: {YearEnd}, {nameof(ChangeDate)}: {ChangeDate}";
		}
	}
}
