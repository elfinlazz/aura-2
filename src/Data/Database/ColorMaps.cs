// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;

namespace Aura.Data.Database
{
	public class ColorMapData
	{
		public int Id { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public uint[] ColorMap { get; set; }
	}

	/// <summary>
	/// Holds information about all colors a specific material can have.
	/// Indexed by color map id.
	/// </summary>
	public class ColorMapDb : DatabaseDatIndexed<int, ColorMapData>
	{
		public uint GetRandom(byte id, MTRandom rnd)
		{
			return this.GetAt(id, rnd.GetUInt32(), rnd.GetUInt32());
		}

		public uint GetRandom(byte id, Random rnd)
		{
			return this.GetAt(id, (uint)rnd.Next(int.MaxValue), (uint)rnd.Next(int.MaxValue));
		}

		public uint GetAt(byte id, uint x, uint y)
		{
			var mapInfo = this.Find(id);
			if (mapInfo == null)
				return 0;

			var color = mapInfo.ColorMap[((x % mapInfo.Width) + ((y % mapInfo.Height) * mapInfo.Height))];
			if (color >> 24 == 0)
				color = ((color & 0xFF) << 16) + ((color >> 8 & 0xFF) << 8) + (color >> 16 & 0xFF);
			return color;
		}

		protected override void Read(BinaryReader br)
		{
			var info = new ColorMapData();
			info.Id = br.ReadByte();
			info.Width = br.ReadInt16();
			info.Height = br.ReadInt16();
			info.ColorMap = new uint[info.Width * info.Height];
			for (int i = 0; i < info.ColorMap.Length; ++i)
			{
				info.ColorMap[i] = br.ReadUInt32();
			}
			this.Entries.Add(info.Id, info);
		}
	}
}
