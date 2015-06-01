// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.IO;

namespace Aura.Data.Database
{
	public class PropDefaultData
	{
		public int Id { get; set; }
		public int Order { get; set; }
		public float Direction { get; set; }
		public float Scale { get; set; }
		public uint ColorOverride { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
		public uint Color4 { get; set; }
		public uint Color5 { get; set; }
		public uint Color6 { get; set; }
		public uint Color7 { get; set; }
		public uint Color8 { get; set; }
		public uint Color9 { get; set; }
		public string State { get; set; }
		public List<ShapeData> Shapes { get; set; }

		public PropDefaultData()
		{
			this.Shapes = new List<ShapeData>();
		}
	}

	public class PropDefaultsDb : DatabaseDatIndexed<int, List<PropDefaultData>>
	{
		protected override void Read(BinaryReader br)
		{
			var count = br.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				var prop = new PropDefaultData();

				prop.Id = br.ReadInt32();
				prop.Order = br.ReadInt32();
				var shapeCount = br.ReadInt32();
				for (int j = 0; j < shapeCount; ++j)
				{
					var shape = new ShapeData();
					shape.DirX1 = br.ReadSingle();
					shape.DirX2 = br.ReadSingle();
					shape.DirY1 = br.ReadSingle();
					shape.DirY2 = br.ReadSingle();
					shape.LenX = br.ReadSingle();
					shape.LenY = br.ReadSingle();
					shape.PosX = br.ReadSingle();
					shape.PosY = br.ReadSingle();

					prop.Shapes.Add(shape);
				}

				var isCollision = br.ReadByte();
				var isFixedAltitude = br.ReadByte();
				prop.Scale = br.ReadSingle();
				prop.Direction = br.ReadSingle();
				prop.ColorOverride = br.ReadUInt32();
				prop.Color1 = br.ReadUInt32();
				prop.Color2 = br.ReadUInt32();
				prop.Color3 = br.ReadUInt32();
				prop.Color4 = br.ReadUInt32();
				prop.Color5 = br.ReadUInt32();
				prop.Color6 = br.ReadUInt32();
				prop.Color7 = br.ReadUInt32();
				prop.Color8 = br.ReadUInt32();
				prop.Color9 = br.ReadUInt32();
				prop.State = br.ReadString();

				if (!this.Entries.ContainsKey(prop.Id))
					this.Entries[prop.Id] = new List<PropDefaultData>();

				this.Entries[prop.Id].Add(prop);
			}
		}
	}
}
