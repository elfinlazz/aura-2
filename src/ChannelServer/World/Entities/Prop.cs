// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Structs;

namespace Aura.Channel.World.Entities
{
	public class Prop : Entity
	{
		public override DataType DataType { get { return DataType.Prop; } }

		public PropInfo Info;

		public string Name { get; set; }
		public string Title { get; set; }
		public string State { get; set; }
		public string ExtraData { get; set; }

		public override EntityType EntityType
		{
			get { return Entities.EntityType.Prop; }
		}

		public override Position GetPosition()
		{
			return new Position((int)this.Info.X, (int)this.Info.Y);
		}
	}
}
