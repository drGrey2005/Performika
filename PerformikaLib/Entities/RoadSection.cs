using System;
using System.Collections.Generic;
using System.Text;

namespace PerformikaLib.Entities
{
	public class RoadSection
	{
		public override string ToString()
		{
			return $"{nameof(Id)}: {Id}, {nameof(Start)}: {Start}, {nameof(StartAdd)}: {StartAdd}, {nameof(Finish)}: {Finish}, {nameof(FinishAdd)}: {FinishAdd}";
		}

		public Guid Id { get; set; }
		public int Start { get; set; }
		public int StartAdd { get; set; }
		public int Finish { get; set; }
		public int FinishAdd { get; set; }
	}
}
