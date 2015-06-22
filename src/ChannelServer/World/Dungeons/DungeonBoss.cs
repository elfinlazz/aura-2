// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons
{
	public class DungeonBoss
	{
		public int RaceId { get; set; }
		public int Amount { get; set; }

		public DungeonBoss(int raceId, int amount)
		{
			this.RaceId = raceId;
			this.Amount = Math.Max(1, amount);
		}
	}
}
