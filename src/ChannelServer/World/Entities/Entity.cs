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
		/// <summary>
		/// Unique entity id
		/// </summary>
		public long EntityId { get; set; }

		/// <summary>
		/// Entity id formatted as X16
		/// </summary>
		public string EntityIdHex { get { return this.EntityId.ToString("X16"); } }

		/// <summary>
		/// The region id the entity is in.
		/// </summary>
		public abstract int RegionId { get; set; }

		/// <summary>
		/// Reference to the entity's region, if it is in one.
		/// </summary>
		public Region Region { get; set; }

		/// <summary>
		/// Entity's current position
		/// </summary>
		/// <returns></returns>
		public abstract Position GetPosition();

		/// <summary>
		/// Entity type used in EntityAppears.
		/// </summary>
		public abstract DataType DataType { get; }

		/// <summary>
		/// Time at which the entity is supposed to disappear,
		/// e.g. items and dead monsters.
		/// </summary>
		public DateTime DisappearTime { get; set; }

		/// <summary>
		/// Returns true if entity is of the given data type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool Is(DataType type) { return (this.DataType == type); }

		/// <summary>
		/// Returns true if entity's data has the tag.
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public abstract bool HasTag(string tag);
	}

	/// <summary>
	/// Vague entity data type, used in EntityAppears.
	/// </summary>
	public enum DataType : short { Creature = 16, Item = 80, Prop = 160 }
}
