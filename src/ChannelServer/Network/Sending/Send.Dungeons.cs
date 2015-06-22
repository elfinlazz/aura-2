// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Dungeons;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends DungeonInfo to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="dungeon"></param>
		public static void DungeonInfo(Creature creature, Dungeon dungeon)
		{
			var packet = new Packet(Op.DungeonInfo, MabiId.Broadcast);

			packet.PutLong(creature.EntityId);
			packet.PutLong(dungeon.InstanceId);
			packet.PutByte(1);
			packet.PutString(dungeon.Name);
			packet.PutInt(dungeon.ItemId);
			packet.PutInt(dungeon.Seed);
			packet.PutInt(dungeon.FloorPlan);

			packet.PutInt(dungeon.Regions.Count);
			foreach (var floor in dungeon.Regions)
				packet.PutInt(floor.Id);

			packet.PutString(dungeon.Options.ToString());

			packet.PutInt(dungeon.Generator.Floors.Count);
			foreach (var floor in dungeon.Generator.Floors)
			{
				var rooms = floor.GetRooms();

				packet.PutInt(rooms.Count);
				foreach (var room in rooms)
				{
					packet.PutByte((byte)room.X);
					packet.PutByte((byte)room.Y);
				}
			}

			packet.PutInt(0); // ? look at ciar info

			packet.PutInt(dungeon.Generator.Floors.Count);
			foreach (var floor in dungeon.Generator.Floors)
			{
				packet.PutUInt(0); // Floor seed or 0 apparently
				packet.PutInt(0); // Somethin.
			}

			creature.Client.Send(packet);
		}
	}
}
