// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util.Configuration;
using System;

namespace Aura.Channel.Util.Configuration.Files
{
	public class WorldConfFile : ConfFile
	{
		public float ExpRate { get; protected set; }
		public float QuestExpRate { get; protected set; }
		public float SkillExpRate { get; protected set; }

		public float DropRate { get; protected set; }
		public float GoldDropChance { get; protected set; }
		public float GoldDropRate { get; protected set; }
		public float LuckyFinishChance { get; protected set; }
		public float BigLuckyFinishChance { get; protected set; }
		public float HugeLuckyFinishChance { get; protected set; }
		public float PropDropChance { get; protected set; }

		public bool EnableContinentWarp { get; protected set; }
		public bool DeadlyNpcs { get; protected set; }
		public bool EnableHunger { get; protected set; }
		public bool YouAreWhatYouEat { get; protected set; }

		public int GmcpMinAuth { get; protected set; }

		public CombatSystem CombatSystem { get; protected set; }
		public bool PerfectPlay { get; protected set; }
		public bool InfiniteResources { get; protected set; }

		public bool Bagception { get; protected set; }
		public bool NoDurabilityLoss { get; protected set; }
		public bool UnlimitedUpgrades { get; protected set; }
		public bool UncapProficiency { get; protected set; }

		public TimeSpan RebirthTime { get; protected set; }

		public int BankGoldPerCharacter { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/world.conf");

			this.ExpRate = this.GetFloat("exp_rate", 100) / 100.0f;
			this.QuestExpRate = this.GetFloat("quest_exp_rate", 100) / 100.0f;
			this.SkillExpRate = this.GetFloat("skill_exp_rate", 100) / 100.0f;

			this.DropRate = this.GetFloat("drop_rate", 100) / 100.0f;
			this.GoldDropChance = this.GetFloat("gold_drop_chance", 30) / 100.0f;
			this.GoldDropRate = this.GetFloat("gold_drop_rate", 100) / 100.0f;
			this.LuckyFinishChance = this.GetFloat("lucky_finish_chance", 0.015f) / 100.0f;
			this.BigLuckyFinishChance = this.GetFloat("big_lucky_finish_chance", 0.005f) / 100.0f;
			this.HugeLuckyFinishChance = this.GetFloat("huge_lucky_finish_chance", 0.001f) / 100.0f;
			this.PropDropChance = this.GetFloat("prop_drop_chance", 30) / 100.0f;

			this.EnableContinentWarp = this.GetBool("enable_continent_warp", true);
			this.DeadlyNpcs = this.GetBool("deadly_npcs", true);
			this.EnableHunger = this.GetBool("enable_hunger", true);
			this.YouAreWhatYouEat = this.GetBool("you_are_what_you_eat", true);

			var gmcpCommand = ChannelServer.Instance.CommandProcessor.GetCommand("gmcp");
			this.GmcpMinAuth = gmcpCommand != null ? gmcpCommand.Auth : this.GetInt("gmcp_min_auth", 50);

			this.PerfectPlay = this.GetBool("perfect_play", false);
			this.CombatSystem = (this.GetString("combat_system", "dynamic") == "classic" ? CombatSystem.Classic : CombatSystem.Dynamic);
			this.InfiniteResources = this.GetBool("infinite_resources", false);

			this.Bagception = this.GetBool("bagception", false);
			this.NoDurabilityLoss = this.GetBool("no_durability_loss", false);
			this.UnlimitedUpgrades = this.GetBool("unlimited_upgrades", false);
			this.UncapProficiency = this.GetBool("uncap_proficiency", false);

			this.RebirthTime = TimeSpan.FromDays(this.GetInt("rebirth_time", 6));

			this.BankGoldPerCharacter = this.GetInt("gold_per_character", 5000000);
		}
	}

	public enum CombatSystem { Dynamic, Classic }
}
