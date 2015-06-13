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
		public bool isDungeonDoor { get; private set; }
		private DungeonBlockType _doorType;
		public bool IsLocked;
		public string InternalName;

		public Door(int id, int regionId, int x, int y, float direction, float scale = 1f, float altitude = 0, 
			string state = "", string name = "", string title = "", string intName = "")
			: base (id, regionId, x, y, direction, scale, altitude, state, name, title)
		{
			this.Behavior = DefaultDoorBehavior;
			this.isDungeonDoor = false;
			this.IsLocked = false;
			this.InternalName = intName;
		}

		private Door(int id, int regionId, int x, int y, float propDirection, DungeonBlockType doorType, string name)
			: this(id, regionId, x, y, propDirection, state: "open", intName: name)
		{
			this.isDungeonDoor = true;
			_doorType = doorType;
			switch (_doorType)
			{
				case DungeonBlockType.Door:
					this.Behavior = DefaultDoorBehavior;
					break;
				case DungeonBlockType.BossDoor:
				case DungeonBlockType.DoorWithLock:
					this.Behavior = LockedDoorBehavior;
					break;
			}
		}

		private void DefaultDoorBehavior(Creature creature, Prop prop)
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
					Send.Notice(creature, NoticeType.MiddleSystem,
						Localization.Get("You have opened the door with the key."));
				}
				else Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
			}
			else
			{
				this.Open();
			}
		}

		private bool OpenWithKey(Creature character)
		{
			foreach (var item in character.Inventory.Items)
				if (item.Info.Id == 70029 || item.Info.Id == 70030)
					if (item.MetaData1.GetString("prop_to_unlock") == this.InternalName)
					{
						character.Inventory.Remove(item);
						return true;
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

		public static Door CreateDoor(int doorPropId, int regionId, int x, int y, float direction, 
			float scale = 1f, float altitude = 0, string state = "", string name = "", string title = "", string intName = "")
		{
			return new Door(doorPropId, regionId, x, y, direction, scale, altitude, state, name, title, intName);
		}

		public static Door CreateDoor(int doorPropId, int x, int y, float direction,
			DungeonBlockType doorType, int regionId = 0, string name = "")
		{
			direction = MabiMath.DegreeToRadian((int)direction);
			if (doorType == DungeonBlockType.BossDoor)
			{
				direction = MabiMath.DirectionToRadian(0, 1);
				y += Dungeon.TileSize + Dungeon.TileSize / 2;
			}
			return new Door(doorPropId, regionId, x, y, direction, doorType, name);
		}

	}
}
