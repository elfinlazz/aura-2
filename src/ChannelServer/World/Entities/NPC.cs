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

		public NpcScript Script { get; set; }
		public AiScript AI { get; set; }
		public int SpawnId { get; set; }

		/// <summary>
		/// Location the NPC was spawned at.
		/// </summary>
		public Location SpawnLocation { get; set; }

		public string DialogPortrait { get; set; }

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

		public override void LoadDefault(bool fullyFunctional = true)
		{
			base.LoadDefault(fullyFunctional);

			var rnd = RandomProvider.Get();

			// Equipment
			foreach (var itemData in this.RaceData.Equip)
			{
				var item = new Item(itemData.GetRandomId(rnd));
				if (itemData.Color1s.Count > 0) item.Info.Color1 = itemData.GetRandomColor1(rnd);
				if (itemData.Color2s.Count > 0) item.Info.Color2 = itemData.GetRandomColor2(rnd);
				if (itemData.Color3s.Count > 0) item.Info.Color3 = itemData.GetRandomColor3(rnd);

				var pocket = (Pocket)itemData.Pocket;
				if (pocket != Pocket.None)
					this.Inventory.Add(item, pocket);
			}

			// Face
			if (this.RaceData.Face.EyeColors.Count > 0) this.EyeColor = (byte)this.RaceData.Face.GetRandomEyeColor(rnd);
			if (this.RaceData.Face.EyeTypes.Count > 0) this.EyeType = (short)this.RaceData.Face.GetRandomEyeType(rnd);
			if (this.RaceData.Face.MouthTypes.Count > 0) this.MouthType = (byte)this.RaceData.Face.GetRandomMouthType(rnd);
			if (this.RaceData.Face.SkinColors.Count > 0) this.SkinColor = (byte)this.RaceData.Face.GetRandomSkinColor(rnd);
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

		/// <summary>
		/// Like <see cref="Warp"/>, except it sends a screen flash
		/// and sound effect to the departing region and arriving region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <remarks>Ideal for NPCs like Tarlach. Be careful not to "double flash"
		/// if you're swapping two NPCs. Only ONE of the NPCs needs to use this method,
		/// the other can use the regular <see cref="Warp"/>.</remarks>
		/// <returns></returns>
		public bool WarpFlash(int regionId, int x, int y)
		{
			// "Departing" effect
			Send.Effect(this, Effect.ScreenFlash, 3000, 0);
			Send.PlaySound(this, "data/sound/Tarlach_change.wav");

			if (!this.Warp(regionId, x, y))
				return false;

			// "Arriving" effect
			Send.Effect(this, Effect.ScreenFlash, 3000, 0);
			Send.PlaySound(this, "data/sound/Tarlach_change.wav");

			return true;
		}

		public override bool CanTarget(Creature creature)
		{
			if (!base.CanTarget(creature))
				return false;

			// Named NPCs (normal dialog ones) can't be targeted.
			// Important because AIs target /pc/ and most NPCs are humans.
			if (creature.Has(CreatureStates.NamedNpc))
				return false;

			return true;
		}

		public override void Kill(Creature killer)
		{
			base.Kill(killer);

			this.DisappearTime = DateTime.Now.AddSeconds(20);

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
			// No surviving once you're in deadly
			if (lifeBefore < 0)
				return false;

			if (!ChannelServer.Instance.Conf.World.DeadlyNpcs)
				return false;

			// Chance = Will/10, capped at 50%
			// (i.e 80 Will = 8%, 500+ Will = 50%)
			// Actual formula unknown
			var chance = Math.Min(50, this.Will / 10);
			return (RandomProvider.Get().Next(101) < chance);
		}

		/// <summary>
		/// Returns random damage based on race data.
		/// </summary>
		/// <param name="weapon"></param>
		/// <param name="balance"></param>
		/// <returns></returns>
		public override float GetRndDamage(Item weapon, float balance = float.NaN)
		{
			float min = this.RaceData.AttackMin, max = this.RaceData.AttackMax;

			if (float.IsNaN(balance))
				balance = this.GetRndBalance(weapon);

			return (min + ((max - min) * balance));
		}
	}
}
