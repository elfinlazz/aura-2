// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Mabi.Network;

namespace Aura.Channel.World.Dungeons.Props
{
	/// <summary>
	/// A door, as found in dungeons.
	/// </summary>
	public class Door : Prop
	{
		/// <summary>
		/// Name of the door, used from puzzles.
		/// </summary>
		public string InternalName { get; protected set; }

		/// <summary>
		/// Type of the door.
		/// </summary>
		public DungeonBlockType DoorType { get; protected set; }

		/// <summary>
		/// Specifies whether the door is locked or not.
		/// </summary>
		public bool IsLocked { get; set; }

		/// <summary>
		/// Boss door stays closed when unlocked.
		/// </summary>
		public bool BlockBoss { get; set; }

		/// <summary>
		/// Creates new door prop.
		/// </summary>
		/// <param name="propId"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction">Direction the door faces in, in degree.</param>
		/// <param name="doorType"></param>
		/// <param name="name"></param>
		/// <param name="state"></param>
		public Door(int propId, int regionId, int x, int y, int direction, DungeonBlockType doorType, string name, string state = "open")
			: base(propId, regionId, x, y, direction, 1, 0, state, "", "")
		{
			this.InternalName = name;
			this.DoorType = doorType;
			this.BlockBoss = false;

			switch (doorType)
			{
				default:
				case DungeonBlockType.Door:
					this.Behavior = this.UnlockedBehavior;
					break;

				case DungeonBlockType.BossDoor:
				case DungeonBlockType.DoorWithLock:
					this.Behavior = LockedDoorBehavior;
					break;
			}

			if (doorType == DungeonBlockType.BossDoor)
			{
				this.Info.Direction = MabiMath.DirectionToRadian(0, 1);
				this.Info.Y += Dungeon.TileSize + Dungeon.TileSize / 2;
			}
			else
			{
				this.Info.Direction = MabiMath.DegreeToRadian(direction);
			}
		}

		/// <summary>
		/// Door's behavior when it's not locked.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		private void UnlockedBehavior(Creature creature, Prop prop)
		{
			var creaturePos = creature.GetPosition();
			var propPos = new Position(prop.GetPosition().X, prop.GetPosition().Y - (this.BlockBoss ? Dungeon.TileSize / 2 : 0));

			var cCoord = new Position(creaturePos.X / Dungeon.TileSize, creaturePos.Y / Dungeon.TileSize);
			var pCoord = new Position(propPos.X / Dungeon.TileSize, propPos.Y / Dungeon.TileSize);

			if (cCoord == pCoord)
			{
				Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is a monster still standing.\nYou must defeat all the monsters for the door to open."));
				return;
			}

			var x = propPos.X;
			var y = propPos.Y;

			if (cCoord.X < pCoord.X)
				x -= 1000;
			else if (cCoord.X > pCoord.X)
				x += 1000;
			else if (cCoord.Y < pCoord.Y)
				y -= 1000;
			else if (cCoord.Y > pCoord.Y)
				y += 1000;

			creature.SetPosition(x, y);
			Send.SetLocation(creature, x, y);
		}

		/// <summary>
		/// Door's behavior when it's locked.
		/// </summary>
		/// <remarks>
		/// TODO: Couldn't this be done in one behavior?
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		private void LockedDoorBehavior(Creature creature, Prop prop)
		{
			if (this.IsLocked)
			{
				if (this.OpenWithKey(creature))
				{
					if (this.BlockBoss)
					{
						this.SetState("unlocked");
						this.UnlockedBehavior(creature, prop);
						this.AddConfirmation();
					}
					else
						this.Open();
					this.IsLocked = false;
					Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("You have opened the door with the key."));
				}
				else
					Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
			}
			else
			{
				this.UnlockedBehavior(creature, prop);
			}
		}

		/// <summary>
		/// Returns true if character has the key to unlock this door,
		/// and removes it from his inventory.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool OpenWithKey(Creature character)
		{
			foreach (var item in character.Inventory.Items)
			{
				if (item.Info.Id == 70029 || item.Info.Id == 70030)
				{
					if (item.MetaData1.GetString("prop_to_unlock") == this.InternalName)
					{
						character.Inventory.Remove(item);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Closes door.
		/// </summary>
		public void Close()
		{
			if (this.DoorType == DungeonBlockType.Door)
				this.AddConfirmation();
			this.SetState("closed");
		}

		/// <summary>
		/// Opens door.
		/// </summary>
		public void Open()
		{
			if (!this.IsLocked)
				this.RemoveAllExtensions();
			this.SetState("open");
		}

		/// <summary>
		/// Adds confirmation to get into the room.
		/// </summary>
		public void AddConfirmation()
		{
			var propPos = new Position(this.GetPosition().X, this.GetPosition().Y - (this.BlockBoss ? Dungeon.TileSize / 2 : 0));
			var x = propPos.X / Dungeon.TileSize;
			var y = propPos.Y / Dungeon.TileSize;

			var extname = string.Format("directed_ask({0},{1})", x, y);
			var condition = string.Format("notfrom({0},{1})", x, y);
			var ext = new ConfirmationPropExtension(extname, Localization.Get("Do you wish to go inside the room?"), condition: condition);
			this.AddExtension(ext);
		}

	}
}
