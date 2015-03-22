// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Threading;
using Aura.Mabi.Const;
using Aura.Channel.Scripting.Scripts;
using Aura.Shared.Util;
using System;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World.Entities
{
	public class NPC : Creature
	{
		/// <summary>
		/// Unique entity id increased and used for each NPC.
		/// </summary>
		private static long _npcId = MabiId.Npcs;

		/// <summary>
		/// Type of the NpcScript used by the NPC.
		/// </summary>
		public Type ScriptType { get; set; }

		/// <summary>
		/// AI controlling the NPC
		/// </summary>
		public AiScript AI { get; set; }

		/// <summary>
		/// Creature spawn id, used for respawning.
		/// </summary>
		public int SpawnId { get; set; }

		/// <summary>
		/// List of greetings the NPC uses in conversations.
		/// </summary>
		public SortedList<int, List<string>> Greetings { get; set; }

		/// <summary>
		/// NPCs preferences regarding gifts.
		/// </summary>
		public GiftWeightInfo GiftWeights { get; set; }

		/// <summary>
		/// Location the NPC was spawned at.
		/// </summary>
		public Location SpawnLocation { get; set; }

		/// <summary>
		/// Custom portrait in dialog.
		/// </summary>
		public string DialogPortrait { get; set; }

		/// <summary>
		/// Creates new NPC
		/// </summary>
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
			this.GiftWeights = new GiftWeightInfo();
			this.Greetings = new SortedList<int, List<string>>();
		}

		/// <summary>
		/// Disposes AI.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (this.AI != null)
				this.AI.Dispose();
		}

		/// <summary>
		/// Loads default information from race data.
		/// </summary>
		/// <param name="fullyFunctional">Fully functional creatures have an inv, regens, etc.</param>
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

		/// <summary>
		/// Returns whether the NPC can target the given creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Kills NPC, rewarding the killer.
		/// </summary>
		/// <param name="killer"></param>
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

		/// <summary>
		/// Returns how well the NPC remembers the other creature.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int GetMemory(Creature other)
		{
			// Get NPC memory and last change date
			var memory = other.Vars.Perm["npc_memory_" + this.Name] ?? 0;
			var change = other.Vars.Perm["npc_memory_change_" + this.Name];

			// Reduce memory by 1 each day
			if (change != null && memory > 0)
			{
				TimeSpan diff = DateTime.Now - change;
				memory = Math.Max(0, memory - Math.Floor(diff.TotalDays));
			}

			return (int)memory;
		}

		/// <summary>
		/// Modifies how well the NPC remembers the other creature.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="value"></param>
		/// <returns>New memory value</returns>
		public int SetMemory(Creature other, int value)
		{
			value = Math.Max(0, value);

			other.Vars.Perm["npc_memory_" + this.Name] = value;
			other.Vars.Perm["npc_memory_change_" + this.Name] = DateTime.Now;

			return value;
		}

		/// <summary>
		/// Sets how well the NPC remembers the other creature.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="value"></param>
		/// <returns>New memory value</returns>
		public int ModifyMemory(Creature other, int value)
		{
			return this.SetMemory(other, this.GetMemory(other) + value);
		}

		/// <summary>
		/// Returns favor of the NPC towards the other creature.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int GetFavor(Creature other)
		{
			// Get NPC favor and last change date
			var favor = other.Vars.Perm["npc_favor_" + this.Name] ?? 0;
			var change = other.Vars.Perm["npc_favor_change_" + this.Name];

			// Reduce favor by 1 each hour
			if (change != null && favor > 0)
			{
				TimeSpan diff = DateTime.Now - change;
				favor = Math.Max(0, favor - Math.Floor(diff.TotalHours));
			}

			return (int)favor;
		}

		/// <summary>
		/// Sets favor of the NPC towards the other creature.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="value"></param>
		/// <returns>New favor value</returns>
		public int SetFavor(Creature other, int value)
		{
			other.Vars.Perm["npc_favor_" + this.Name] = value;
			other.Vars.Perm["npc_favor_change_" + this.Name] = DateTime.Now;

			return value;
		}

		/// <summary>
		/// Modifies favor of the NPC towards the other creature.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="value"></param>
		/// <returns>New favor value</returns>
		public int ModifyFavor(Creature other, int value)
		{
			return this.SetFavor(other, this.GetFavor(other) + value);
		}

		/// <summary>
		/// Gets how much the other creature is stressing the NPC.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int GetStress(Creature other)
		{
			// Get NPC stress and last change date
			var stress = other.Vars.Perm["npc_stress_" + this.Name] ?? 0;
			var change = other.Vars.Perm["npc_stress_change_" + this.Name];

			// Reduce stress by 1 each minute
			if (change != null && stress > 0)
			{
				TimeSpan diff = DateTime.Now - change;
				stress = Math.Max(0, stress - Math.Floor(diff.TotalMinutes));
			}

			return (int)stress;
		}

		/// <summary>
		/// Sets how much the other creature is stressing the NPC.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="value"></param>
		/// <returns>New stress value</returns>
		public int SetStress(Creature other, int value)
		{
			value = Math.Max(0, value);

			other.Vars.Perm["npc_stress_" + this.Name] = value;
			other.Vars.Perm["npc_stress_change_" + this.Name] = DateTime.Now;

			return value;
		}

		/// <summary>
		/// Modifies how much the other creature is stressing the NPC.
		/// </summary>
		/// <param name="other"></param>
		/// <param name="value"></param>
		/// <returns>New stress value</returns>
		public int ModifyStress(Creature other, int value)
		{
			return this.SetStress(other, this.GetStress(other) + value);
		}

		/// <summary>
		/// Aggroes target, setting target and putting creature in battle stance.
		/// </summary>
		/// <param name="creature"></param>
		public override void Aggro(Creature target)
		{
			if (this.AI == null)
				return;

			// Aggro attacker if there is not current target,
			// or if there is a target but it's not a player, and the attacker is one,
			// or if the current target is not aggroed yet.
			if (this.Target == null || (this.Target != null && target != null && !this.Target.IsPlayer && target.IsPlayer) || this.AI.State != AiScript.AiState.Aggro)
				this.AI.AggroCreature(target);
		}

		/// <summary>
		/// TODO: Move somewhere? =/
		/// </summary>
		public class GiftWeightInfo
		{
			public float Adult { get; set; }
			public float Anime { get; set; }
			public float Beauty { get; set; }
			public float Individuality { get; set; }
			public float Luxury { get; set; }
			public float Maniac { get; set; }
			public float Meaning { get; set; }
			public float Rarity { get; set; }
			public float Sexy { get; set; }
			public float Toughness { get; set; }
			public float Utility { get; set; }

			public int CalculateScore(Item gift)
			{
				var score = 0f;

				var taste = gift.Data.Taste;

				score += this.Adult * taste.Adult;
				score += this.Anime * taste.Anime;
				score += this.Beauty * taste.Beauty;
				score += this.Individuality * taste.Individuality;
				score += this.Luxury * taste.Luxury;
				score += this.Maniac * taste.Maniac;
				score += this.Meaning * taste.Meaning;
				score += this.Rarity * taste.Rarity;
				score += this.Sexy * taste.Sexy;
				score += this.Toughness * taste.Toughness;
				score += this.Utility * taste.Utility;

				return (int)score;
			}
		}
	}
}
