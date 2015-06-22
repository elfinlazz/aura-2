// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Mabi.Network;
using System.Linq;

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
		public string Name { get; protected set; }

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
			this.Name = name;
			this.DoorType = doorType;
			this.BlockBoss = false;
			this.Behavior = this.DefaultBehavior;

			// Set direction and adjust Y for boss doors
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
		/// Door's behavior.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		private void DefaultBehavior(Creature creature, Prop prop)
		{
			// Open doors can't be interacted with
			if (this.State == "open")
				return;

			// If it's unlocked, warp inside
			if (!this.IsLocked)
			{
				this.WarpInside(creature, prop);
				return;
			}

			// Check if character has the key
			if (!this.RemoveKey(creature))
			{
				Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
				return;
			}

			// Unlock the door, but don't open it if it's supposed to block the bosses
			if (this.BlockBoss)
			{
				if (this.State != "unlocked")
				{
					this.SetState("unlocked");
					this.AddConfirmation();
				}
				this.WarpInside(creature, prop);
			}
			else
				this.Open();

			this.IsLocked = false;
			Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("You have opened the door with the key."));
		}

		/// <summary>
		/// Door's behavior when it's not locked.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="prop"></param>
		private void WarpInside(Creature creature, Prop prop)
		{
			var creaturePos = creature.GetPosition();
			var propPos = prop.GetPosition();
			if (this.DoorType == DungeonBlockType.BossDoor)
				propPos = new Position(propPos.X, propPos.Y - Dungeon.TileSize / 2);

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
		/// Returns true if character has the key to unlock this door,
		/// and removes it from his inventory.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		private bool RemoveKey(Creature character)
		{
			var items = character.Inventory.Items.ToList();

			var key = items.FirstOrDefault(item => (item.Info.Id == 70029 || item.Info.Id == 70030) && item.MetaData1.GetString("prop_to_unlock") == this.Name);
			if (key == null)
				return false;

			character.Inventory.Remove(key);
			return true;
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
			var propPos = this.GetPosition();
			if (this.DoorType == DungeonBlockType.BossDoor)
				propPos = new Position(propPos.X, propPos.Y - Dungeon.TileSize / 2);

			var x = propPos.X / Dungeon.TileSize;
			var y = propPos.Y / Dungeon.TileSize;

			var extname = string.Format("directed_ask({0},{1})", x, y);
			var condition = string.Format("notfrom({0},{1})", x, y);
			var ext = new ConfirmationPropExtension(extname, Localization.Get("Do you wish to go inside the room?"), condition: condition);
			this.AddExtension(ext);
		}
	}
}
