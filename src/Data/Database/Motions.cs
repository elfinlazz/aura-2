// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class MotionData
	{
		public string Name { get; set; }
		public short Category { get; set; }
		public short Type { get; set; }
		public bool Loop { get; set; }
	}

	/// <summary>
	/// Indexed by motion name.
	/// </summary>
	public class MotionDb : DatabaseJsonIndexed<string, MotionData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "category", "type");

			var info = new MotionData();
			info.Name = entry.ReadString("name");
			info.Category = entry.ReadShort("category");
			info.Type = entry.ReadShort("type");
			info.Loop = entry.ReadBool("loop");

			this.Entries[info.Name] = info;
		}
	}
}
