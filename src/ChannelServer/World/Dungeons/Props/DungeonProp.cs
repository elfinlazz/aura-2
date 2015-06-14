// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons.Props
{
	public abstract class DungeonProp : Prop
	{
		public string InternalName { get; protected set; }

		public DungeonProp(int id, string name, string state = "")
			: base(id, 0, 0, 0, 0, 1, 0, state, "", "")
		{
			this.InternalName = name;
		}
	}
}
