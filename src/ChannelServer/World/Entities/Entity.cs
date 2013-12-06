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
		public string EntityIdHex { get { return this.EntityId.ToString("X16"); } }

		public virtual int RegionId { get; set; }
		public Region Region { get; set; }

		public abstract EntityType EntityType { get; }
		public abstract DataType DataType { get; }

		public abstract Position GetPosition();
	}

	public enum EntityType { Undefined, Character, Pet, Item, NPC, Prop }

	/// <summary>
	/// Vague entity data type, used in EntityAppears.
	/// </summary>
	public enum DataType : short { Creature = 16, Item = 80, Prop = 160 }
}
