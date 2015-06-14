// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Props
{
	public class Door : Prop
	{
		public string InternalName { get; protected set; }
		public bool IsLocked { get; set; }

		public Door(int propId, int regionId, int x, int y, int direction, DungeonBlockType doorType, string name, string state = "open")
			: base(propId, regionId, x, y, direction, 1, 0, state, "", "")
		{
			this.InternalName = name;

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

		private void UnlockedBehavior(Creature creature, Prop prop)
		{
			// TODO: allow to teleport into closed room. Don't open
			this.Open();
		}

		private void LockedDoorBehavior(Creature creature, Prop prop)
		{
			// todo: sometimes we don't want to open a door, only unlock it and teleport in
			if (this.IsLocked)
			{
				if (this.OpenWithKey(creature))
				{
					this.Open();
					this.IsLocked = false;
					Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("You have opened the door with the key."));
				}
				else
					Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
			}
			else
			{
				this.Open();
			}
		}

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

		public void Close()
		{
			this.SetState("closed");
		}

		public void Open()
		{
			this.SetState("open");
		}
	}
}
