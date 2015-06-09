// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	public class DungeonStyleData
	{
		public List<DungeonBlockData> Blocks { get; set; }

		/// <summary>
		/// Returns the first block with the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public DungeonBlockData Get(DungeonBlockType type)
		{
			return this.Blocks.FirstOrDefault(a => a.Type == type);
		}

		/// <summary>
		/// Returns the first block that has only the given direction.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public DungeonBlockData Get(DungeonBlockType type, int direction)
		{
			var top = direction == 0 ? 1 : 0;
			var right = direction == 1 ? 1 : 0;
			var bottom = direction == 2 ? 1 : 0;
			var left = direction == 3 ? 1 : 0;

			return this.Get(type, top, right, bottom, left);
		}

		/// <summary>
		/// Returns the first block with the given type and directions.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public DungeonBlockData Get(DungeonBlockType type, int top, int right, int bottom, int left)
		{
			return this.Blocks.FirstOrDefault(a => a.Type == type && a.Top == top && a.Right == right && a.Bottom == bottom && a.Left == left);
		}

		/// <summary>
		/// Returns the first block with the given type and directions.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public DungeonBlockData Get(DungeonBlockType type, int[] directions)
		{
			if (directions.Length != 4)
				throw new ArgumentException("Expected 4 directions: top, right, bottom, and left, in order.");

			var top = directions[0] != 0 ? 1 : 0;
			var right = directions[1] != 0 ? 1 : 0;
			var bottom = directions[2] != 0 ? 1 : 0;
			var left = directions[3] != 0 ? 1 : 0;

			return this.Get(type, top, right, bottom, left);
		}

		public DungeonStyleData()
		{
			this.Blocks = new List<DungeonBlockData>();
		}
	}

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

	public class DungeonBlocksDb : DatabaseJsonIndexed<int, DungeonStyleData>
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
					this.Entries[data.Style] = new DungeonStyleData();

				this.Entries[data.Style].Blocks.Add(data);
			}
		}
	}
}
