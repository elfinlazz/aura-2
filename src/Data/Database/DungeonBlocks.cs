// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class DungeonBlockData
	{
		public int Style { get; set; }
		public int PropId { get; set; }
		public string PropName { get; set; }
		public DungeonBlockType Type { get; set; }
		public int Way { get; set; }
		public int Top { get; set; }
		public int Bottom { get; set; }
		public int Right { get; set; }
		public int Left { get; set; }
		public int Rotation { get; set; }
	}

	public class DungeonBlocksDb : DatabaseJsonIndexed<int, List<DungeonBlockData>>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("style", "blocks");

			var style = entry.ReadInt("style");

			foreach (JObject propEntry in entry["blocks"])
			{
				propEntry.AssertNotMissing("id", "name", "type", "way", "top", "bottom", "right", "left", "rotation");

				var data = new DungeonBlockData();
				data.Style = style;
				data.PropId = propEntry.ReadInt("id");
				data.PropName = propEntry.ReadString("name");
				data.Type = (DungeonBlockType)propEntry.ReadInt("type");
				data.Way = propEntry.ReadInt("way");
				data.Top = propEntry.ReadInt("top");
				data.Bottom = propEntry.ReadInt("bottom");
				data.Right = propEntry.ReadInt("right");
				data.Left = propEntry.ReadInt("left");
				data.Rotation = propEntry.ReadInt("rotation");

				if (!this.Entries.ContainsKey(data.Style))
					this.Entries[data.Style] = new List<DungeonBlockData>();

				this.Entries[data.Style].Add(data);
			}
		}
	}
}
