// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons.Props
{
	public class HerbPatch : DungeonProp
	{
		public HerbPatch(int propId, string name, uint color)
			: base(propId, name)
		{
			this.Info.Color1 = color;
		}
	}
}
