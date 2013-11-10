// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Base class for creatures controlled by players.
	/// </summary>
	public abstract class PlayerCreature : Creature
	{
		public override EntityType EntityType { get { return EntityType.Character; } }

		public long CreatureId { get; set; }
		public string Server { get; set; }
		public DateTime DeletionTime { get; set; }

		public DateTime CreationTime { get; set; }
		public DateTime LastRebirth { get; set; }
	}
}
