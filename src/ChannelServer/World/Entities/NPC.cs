// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Const;
using System.Threading;

namespace Aura.Channel.World.Entities
{
	public class NPC : Creature
	{
		private long _npcId = MabiId.Npcs;

		public override EntityType EntityType { get { return EntityType.NPC; } }

		public NPC()
		{
			this.EntityId = Interlocked.Increment(ref _npcId);
		}
	}
}
