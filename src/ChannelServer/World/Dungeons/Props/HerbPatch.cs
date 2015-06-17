// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons.Props
{
	/// <summary>
	/// Herb patch prop, as found in dungeons.
	/// </summary>
	public class HerbPatch : DungeonProp
	{
		/// <summary>
		/// Creates new herb patch prop.
		/// </summary>
		/// <param name="propId">Id of the herb prop.</param>
		/// <param name="name">Name of the prop.</param>
		/// <param name="color">Color of the leaves.</param>
		public HerbPatch(int propId, string name, uint color)
			: base(propId, name)
		{
			this.Info.Color1 = color;
		}
	}
}
