using System;

namespace PerformikaLib.Entities
{
	public class RoadRepair
	{
		private int? _roadSectionId;

		public Guid ChildObjectUid { get; set; }

		public int? RoadSectionId
		{
			get { return _roadSectionId; }
			set
			{
				if (value == 0)
				{
					_roadSectionId = null;
				}
				else
				{
					_roadSectionId = value;
				}
			}
		}
		public int Start { get; set; }
		public int StartAdd { get; set; }
		public int Finish { get; set; }
		public int FinishAdd { get; set; }
		public int? ObjectId { get; set; } = null;
		public Guid? ObjectUid { get; set; }
		public DateTime ChangeDate { get; set; }

		public override string ToString()
		{
			return $"{nameof(_roadSectionId)}: {_roadSectionId}, {nameof(ChildObjectUid)}: {ChildObjectUid}, {nameof(RoadSectionId)}: {RoadSectionId}, {nameof(Start)}: {Start}, {nameof(StartAdd)}: {StartAdd}, {nameof(Finish)}: {Finish}, {nameof(FinishAdd)}: {FinishAdd}, {nameof(ObjectId)}: {ObjectId}, {nameof(ObjectUid)}: {ObjectUid}, {nameof(ChangeDate)}: {ChangeDate}";
		}


	}
}
