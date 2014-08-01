// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// An entity is any being or object that can be sent in Entity(Dis)Appears.
	/// </summary>
	public abstract class Entity
	{
		public long EntityId { get; set; }
		public string EntityIdHex { get { return this.EntityId.ToString("X16"); } }

		public abstract int RegionId { get; set; }
		public Region Region { get; set; }

		public abstract DataType DataType { get; }

		public DateTime DisappearTime { get; set; }

		public abstract Position GetPosition();

		public bool Is(DataType type) { return (this.DataType == type); }
	}

	/// <summary>
	/// Vague entity data type, used in EntityAppears.
	/// </summary>
	public enum DataType : short { Creature = 16, Item = 80, Prop = 160 }
}
