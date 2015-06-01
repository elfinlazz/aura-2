// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Channel.World.Dungeons
{
	/// <summary>
	/// Manages dynamic regions.
	/// </summary>
	public class DungeonManager
	{
		private static long _instanceId = MabiId.Instances;

		private static readonly HashSet<int> _entryRegionIds = new HashSet<int>()
		{
			13,    // Uladh_Dungeon_Beginners_Hall1 (Alby)
			11,    // Uladh_Dungeon_Black_Wolfs_Hall1 (Ciar)
			32,    // Ula_DgnHall_Bangor_before1 (Bangor)
			33,    // Ula_DgnHall_Bangor_before2
			54,    // Ula_DgnHall_Coill_before (Coil)
			49,    // Ula_DgnHall_Danu_before (Fiodh)
			24,    // Ula_DgnHall_Dunbarton_before1 (Rabbie)
			25,    // Ula_DgnHall_Dunbarton_before2 (Math)
			64,    // Ula_DgnHall_Runda_before (Rundal)
			44,    // Ula_DgnHall_Tirnanog_before1 (Albey)
			78,    // Ula_DgnHall_Tirnanog_G3_before (Baol)
			121,   // Ula_hardmode_DgnHall_Ciar_before (Ciar Hard)
			123,   // Ula_hardmode_DgnHall_Runda_before (Rundal Hard)
			27,    // Ula_hardmode_DgnHall_TirChonaill_before (Alby Hard)
			217,   // Abb_Neagh_keep_DgnHall_before
			205,   // Dugald_Aisle_keep_DgnHall_before
			207,   // Sen_Mag_keep_DgnHall_before
			60206, // Tara_keep_DgnHall_before
			600,   // JP_Nekojima_islet
		};

		private Dictionary<long, Dungeon> _dungeons;
		private HashSet<int> _regionIds;

		private object _sync = new Object();

		/// <summary>
		/// Creates new dungeon manager.
		/// </summary>
		public DungeonManager()
		{
			_dungeons = new Dictionary<long, Dungeon>();
			_regionIds = new HashSet<int>();
		}

		public Dungeon CreateDungeon(string dungeonName, int itemId, Creature creature)
		{
			var instanceId = this.GetInstanceId();
			var dungeon = new Dungeon(instanceId, dungeonName, itemId, creature);

			return dungeon;
		}

		private bool Remove(long instanceId)
		{
			Dungeon dungeon;

			lock (_sync)
			{
				if (!_dungeons.TryGetValue(instanceId, out dungeon))
					return false;

				foreach (var region in dungeon.Regions)
				{
					_regionIds.Remove(region.Id);
					ChannelServer.Instance.World.RemoveRegion(region.Id);
				}

				_dungeons.Remove(instanceId);
			}

			return true;
		}

		/// <summary>
		/// Generates and reserves a new dungeon region id.
		/// </summary>
		/// <returns></returns>
		public int GetRegionId()
		{
			var id = -1;

			lock (_sync)
			{
				for (int i = MabiId.DungeonRegions; i < MabiId.DynamicRegions; ++i)
				{
					if (!_regionIds.Contains(i))
					{
						id = i;
						break;
					}
				}

				_regionIds.Add(id);
			}

			if (id == -1)
				throw new Exception("No dungeon region ids available.");

			return id;
		}

		/// <summary>
		/// Generates and reserves a new dungeon instance id.
		/// </summary>
		/// <returns></returns>
		public long GetInstanceId()
		{
			return Interlocked.Increment(ref _instanceId);
		}

		public bool CheckDrop(Creature creature, Item item)
		{
			var currentRegionId = creature.RegionId;
			if (!_entryRegionIds.Contains(currentRegionId))
				return false;

			var pos = creature.GetPosition();

			var clientEvent = creature.Region.GetClientEvent(a => a.Data.IsAltar);
			if (clientEvent == null)
			{
				Log.Warning("DungeonManager.CheckDrop: No altar found.");
				return false;
			}

			if (!clientEvent.IsInside(pos.X, pos.Y))
			{
				// Tell player to step on altar?
				return false;
			}

			var parameter = clientEvent.Data.Parameters.FirstOrDefault(a => a.EventType == 2110);
			if (parameter == null || parameter.XML == null || parameter.XML.Attribute("dungeonname") == null)
			{
				Log.Warning("DungeonManager.CheckDrop: No dungeon name found in altar event '{0:X16}'.", clientEvent.EntityId);
				return false;
			}

			var dungeonName = parameter.XML.Attribute("dungeonname").Value.ToLower();

			var dungeonScript = ChannelServer.Instance.ScriptManager.DungeonScripts.Get(dungeonName);
			if (dungeonScript == null)
			{
				Send.ServerMessage(creature, "This dungeon hasn't been added yet.");
				Log.Warning("DungeonManager.CheckDrop: No routing dungeon script found for '{0}'.", dungeonName);
				return false;
			}

			if (!dungeonScript.Route(creature, item, ref dungeonName))
			{
				Send.Notice(creature, "Routing fail.");
				return false;
			}

			var dungeon = this.CreateDungeon(dungeonName, item.Info.Id, creature);
			var regionId = dungeon.Regions.First().Id;

			creature.LastLocation = new Location(creature.RegionId, creature.GetPosition());
			creature.SetLocation(regionId, creature.LastLocation.X, creature.LastLocation.Y);
			creature.Warping = true;
			Send.CharacterLock(creature, Locks.Default);

			Send.DungeonInfo(creature, dungeon);

			return true;
		}
	}
}
