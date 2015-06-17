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
	/// Ore deposit, as found in dungeons.
	/// </summary>
	public class OreDeposit : DungeonProp
	{
		/// <summary>
		/// Creates new ore deposit.
		/// </summary>
		/// <param name="propId">Id of the ore prop.</param>
		/// <param name="name">Name of the prop.</param>
		public OreDeposit(int propId, string name)
			: base(propId, name, "normal")
		{
		}
	}
}
