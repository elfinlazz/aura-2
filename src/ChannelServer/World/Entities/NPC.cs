// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Threading;
using Aura.Shared.Mabi.Const;
using Aura.Channel.Scripting.Scripts;
using Aura.Shared.Util;
using Aura.Channel.Scripting;
using System;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World.Entities
{
	public class NPC : Creature
	{
		private static long _npcId = MabiId.Npcs;

		public override EntityType EntityType { get { return EntityType.NPC; } }

		public NpcScript Script { get; set; }
		public AiScript AI { get; set; }
		public int SpawnId { get; set; }

		public NPC()
		{
			this.EntityId = Interlocked.Increment(ref _npcId);

			// Some default values to prevent errors
			this.Name = "_undefined";
			this.Race = 190140; // Wood dummy
			this.Height = this.Weight = this.Upper = this.Lower = 1;
			this.RegionId = 0;
			this.Life = this.LifeMaxBase = 1000;
			this.Color1 = this.Color2 = this.Color2 = 0x808080;
		}

		public override void Dispose()
		{
			base.Dispose();

			if (this.AI != null)
				this.AI.Dispose();
		}

		/// <summary>
		/// Moves NPC to target location and adds it to the region.
		/// Returns false if region doesn't exist.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public override bool Warp(int regionId, int x, int y)
		{
			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("NPC.Warp: Region '{0}' doesn't exist.", regionId);
				return false;
			}

			this.SetLocation(regionId, x, y);

			region.AddCreature(this);

			return true;
		}

		public override bool CanTarget(Creature creature)
		{
			if (!base.CanTarget(creature))
				return false;

			return (creature.IsPlayer && !this.Has(CreatureStates.GoodNpc));
		}

		public override void Kill(Creature killer)
		{
			base.Kill(killer);

			var rnd = RandomProvider.Get();
			var pos = this.GetPosition();

			this.DisappearTime = DateTime.Now.AddSeconds(20);

			// Gold
			if (rnd.NextDouble() < ChannelServer.Instance.Conf.World.GoldDropRate)
			{
				var amount = rnd.Next(this.RaceData.GoldMin, this.RaceData.GoldMax + 1);
				if (amount > 0)
				{
					var dropPos = pos.GetRandomInRange(50, rnd);

					var gold = new Item(2000);
					gold.Info.Amount = (ushort)amount;
					gold.Info.Region = this.RegionId;
					gold.Info.X = dropPos.X;
					gold.Info.Y = dropPos.Y;
					gold.DisappearTime = DateTime.Now.AddSeconds(60);

					this.Region.AddItem(gold);
				}
			}

			// Drops
			foreach (var drop in this.RaceData.Drops)
			{
				if (rnd.NextDouble() < drop.Chance * ChannelServer.Instance.Conf.World.DropRate)
				{
					var dropPos = pos.GetRandomInRange(50, rnd);

					var item = new Item(drop.ItemId);
					item.Info.Amount = 1;
					item.Info.Region = this.RegionId;
					item.Info.X = dropPos.X;
					item.Info.Y = dropPos.Y;
					item.DisappearTime = DateTime.Now.AddSeconds(60);

					this.Region.AddItem(item);
				}
			}

			if (killer == null)
				return;

			// Exp
			var exp = (long)(this.RaceData.Exp * ChannelServer.Instance.Conf.World.ExpRate);
			killer.GiveExp(exp);

			Send.CombatMessage(killer, "+{0} EXP", exp);
		}

		/// <summary>
		/// NPCs may survive randomly.
		/// </summary>
		/// <remarks>
		/// http://wiki.mabinogiworld.com/view/Stats#Life
		/// More Will supposedly increases the chance. Unknown if this
		/// applies to players as well. Before certain Gs, NPCs weren't
		/// able to survive attacks under any circumstances.
		/// </remarks>
		/// <param name="damage"></param>
		/// <param name="from"></param>
		/// <param name="lifeBefore"></param>
		/// <returns></returns>
		protected override bool ShouldSurvive(float damage, Creature from, float lifeBefore)
		{
			// Actual formula unknown.
			return ((this.Will * 10) + RandomProvider.Get().Next(1001)) > 999;
		}
	}
}
