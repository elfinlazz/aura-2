// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Aura.Data.Database
{
	[Serializable]
	public class TitleData
	{
		public ushort Id { get; internal set; }
		public string Name { get; internal set; }
		public Dictionary<string, float> Effects { get; internal set; }
	}

	/// <summary>
	/// Indexed by title id.
	/// </summary>
	public class TitleDb : DatabaseCSVIndexed<int, TitleData>
	{
		[MinFieldCount(2)]
		protected override void ReadEntry(CSVEntry entry)
		{
			var data = new TitleData();
			data.Id = entry.ReadUShort();
			data.Name = entry.ReadString();
			data.Effects = new Dictionary<string, float>();

			while (!entry.End)
			{
				var sEffects = entry.ReadString().Trim();
				var splitted = sEffects.Split(':');
				if (splitted.Length != 2)
					throw new DatabaseWarningException("Invalid effect format: " + sEffects);

				float val;
				if (!float.TryParse(splitted[1], NumberStyles.Float, CultureInfo.InvariantCulture, out val))
					throw new DatabaseWarningException("Invalid effect value: " + splitted[1].Trim());

				data.Effects[splitted[0].Trim()] = val;
			}

			this.Entries[data.Id] = data;
		}
	}
}
