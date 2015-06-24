// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	public class DynamicRegion : Region
	{
		/// <summary>
		/// Id of the region this one is based on
		/// </summary>
		public int BaseId { get; protected set; }

		/// <summary>
		/// Name of the region this one is based on
		/// </summary>
		public string BaseName { get; protected set; }

		/// <summary>
		/// Variation file used for this region
		/// </summary>
		public string Variation { get; protected set; }

		/// <summary>
		/// Returns mode of this region, describing when it will be removed.
		/// </summary>
		public RegionMode Mode { get; protected set; }

		/// <summary>
		/// Creates new dynamic region.
		/// </summary>
		/// <param name="baseRegionId"></param>
		/// <param name="variationFile"></param>
		/// <param name="mode"></param>
		public DynamicRegion(int baseRegionId, string variationFile = "", RegionMode mode = RegionMode.RemoveWhenEmpty)
			: base(baseRegionId)
		{
			this.BaseId = baseRegionId;

			var baseRegionInfoData = AuraData.RegionInfoDb.Find(this.BaseId);
			if (baseRegionInfoData == null)
				throw new Exception("DynamicRegion: No region info data found for '" + this.BaseId + "'.");

			this.BaseName = baseRegionInfoData.Name;
			this.Id = ChannelServer.Instance.World.DynamicRegions.GetFreeDynamicRegionId();
			this.Name = "DynamicRegion" + this.Id;
			this.Variation = variationFile;
			this.Mode = mode;

			this.RegionInfoData = CreateVariation(baseRegionInfoData, this.Id, variationFile);

			this.InitializeFromData();

			ChannelServer.Instance.World.DynamicRegions.Add(this);
		}

		/// <summary>
		/// Creates variation of given region info.
		/// </summary>
		/// <param name="baseRegionInfoData"></param>
		/// <param name="newRegionId"></param>
		/// <param name="variationFile"></param>
		/// <returns></returns>
		private static RegionInfoData CreateVariation(RegionInfoData baseRegionInfoData, int newRegionId, string variationFile)
		{
			var result = new RegionInfoData();
			result.Id = newRegionId;
			result.GroupId = baseRegionInfoData.GroupId;
			result.X1 = baseRegionInfoData.X1;
			result.Y1 = baseRegionInfoData.Y1;
			result.X2 = baseRegionInfoData.X2;
			result.Y2 = baseRegionInfoData.Y2;

			// TODO: Filter areas, props, and events to create, based on variation file.

			result.Areas = new List<AreaData>(baseRegionInfoData.Areas.Count);
			var i = 1;
			foreach (var originalArea in baseRegionInfoData.Areas)
			{
				var area = originalArea.Copy(false, false);
				area.Id = i++;

				// Add props
				foreach (var originalProp in originalArea.Props.Values)
				{
					var prop = originalProp.Copy();

					var id = (ulong)prop.EntityId;
					id &= ~0x0000FFFFFFFF0000U;
					id |= ((ulong)result.Id << 32);
					id |= ((ulong)baseRegionInfoData.GetAreaIndex(originalArea.Id) << 16);

					prop.EntityId = (long)id;

					area.Props.Add(prop.EntityId, prop);
				}

				// Add events
				foreach (var originalEvent in originalArea.Events.Values)
				{
					var ev = originalEvent.Copy();
					ev.RegionId = result.Id;

					var id = (ulong)ev.Id;
					id &= ~0x0000FFFFFFFF0000U;
					id |= ((ulong)result.Id << 32);
					id |= ((ulong)baseRegionInfoData.GetAreaIndex(originalArea.Id) << 16);

					ev.Id = (long)id;

					area.Events.Add(ev.Id, ev);
				}

				result.Areas.Add(area);
			}

			return result;
		}

		public override void RemoveCreature(Creature creature)
		{
			base.RemoveCreature(creature);

			// Remove dynamic region from client when it's removed from it on
			// the server, so it's recreated next time it goes to a dynamic
			// region with that id. Otherwise it will load the previous
			// region again.
			Send.RemoveDynamicRegion(creature, this.Id);

			// Remove empty region from world when last *player* was removed
			if (creature.IsPlayer && this.CountPlayers() == 0 && this.Mode == RegionMode.RemoveWhenEmpty)
			{
				ChannelServer.Instance.World.RemoveRegion(this.Id);
				ChannelServer.Instance.World.DynamicRegions.Remove(this.Id);
			}
		}
	}
}
