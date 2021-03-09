using System;

namespace PerformikaLib.Entities
{
	/// <summary>
	/// Значение справочника ИССО
	/// </summary>
	public class IssoValue
	{
		public Guid ObjectUid { get; set; }
		public int? AbdmId { get; set; }
		public string Road { get; set; }
		public Guid? RoadUid { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public Guid? TypeUid { get; set; }
		public string Region { get; set; }
		public Guid? RegionUid { get; set; }
		public string Category { get; set; }

		public override string ToString()
		{
			return $"{nameof(ObjectUid)}: {ObjectUid}, {nameof(AbdmId)}: {AbdmId}, {nameof(Road)}: {Road}, {nameof(RoadUid)}: {RoadUid}, {nameof(Name)}: {Name}, {nameof(Description)}: {Description}, {nameof(Type)}: {Type}, {nameof(TypeUid)}: {TypeUid}, {nameof(Region)}: {Region}, {nameof(RegionUid)}: {RegionUid}, {nameof(Category)}: {Category}, {nameof(Length)}: {Length}, {nameof(Evaluation)}: {Evaluation}, {nameof(RoadSize)}: {RoadSize}, {nameof(BuildReconYear)}: {BuildReconYear}, {nameof(Capacity)}: {Capacity}, {nameof(DiagnosticsYear)}: {DiagnosticsYear}, {nameof(Fku)}: {Fku}, {nameof(FkuUid)}: {FkuUid}, {nameof(ChangeDate)}: {ChangeDate}";
		}

		public int? Length { get; set; }
		public string Evaluation { get; set; }
		public int? RoadSize { get; set; }
		public string BuildReconYear { get; set; }
		public int? Capacity { get; set; }
		public DateTime? DiagnosticsYear { get; set; }
		public string Fku { get; set; }
		public Guid? FkuUid { get; set; }
		public DateTime ChangeDate { get; set; }
	}
}
