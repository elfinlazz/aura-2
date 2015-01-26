using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	public class EgoInfo
	{
		public byte Id { get; set; }

		public string Name { get; set; }

		public string NpcName { get; set; }
		public string Description { get; set; }

		public byte StrLevel { get; set; }
		public int StrExp { get; set; }

		public byte IntLevel { get; set; }
		public int IntExp { get; set; }

		public byte DexLevel { get; set; }
		public int DexExp { get; set; }

		public byte WillLevel { get; set; }
		public int WillExp { get; set; }

		public byte LuckLevel { get; set; }
		public int LuckExp { get; set; }

		public byte SocialLevel { get; set; }
		public int SocialExp { get; set; }

		public DateTime LastFeeding { get; set; }
	}
}
