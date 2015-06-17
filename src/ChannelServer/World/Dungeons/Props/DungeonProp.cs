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
	/// <summary>
	/// Base class for dungeon props, like chests and switches.
	/// </summary>
	public abstract class DungeonProp : Prop
	{
		/// <summary>
		/// Name of the prop, used in puzzles.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// Initializes prop.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="state"></param>
		public DungeonProp(int id, string name, string state = "")
			: base(id, 0, 0, 0, 0, 1, 0, state, "", "")
		{
			this.Name = name;
		}
	}
}
