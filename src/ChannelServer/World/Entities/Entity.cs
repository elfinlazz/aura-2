// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// An entity is any being or object that can be send in EntityAppears.
	/// </summary>
	public abstract class Entity
	{
		public long EntityId { get; set; }
		public int Region { get; set; }

		public abstract EntityType EntityType { get; }

		public abstract Position GetPosition();
	}

	public enum EntityType { Undefined, Character, Pet, Item, NPC, Prop }
}
