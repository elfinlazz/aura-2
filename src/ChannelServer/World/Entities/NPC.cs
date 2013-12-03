// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Threading;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities
{
	public class NPC : Creature
	{
		private static long _npcId = MabiId.Npcs;

		public override EntityType EntityType { get { return EntityType.NPC; } }

		public NPC()
		{
			this.EntityId = Interlocked.Increment(ref _npcId);

			// Some default values to prevent errors
			this.Name = "_undefined";
			this.Race = 190140; // Wood dummy
			this.Height = this.Weight = this.Upper = this.Lower = 1;
			this.RegionId = 0;
		}
	}
}
