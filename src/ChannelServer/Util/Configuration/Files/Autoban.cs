// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
	public class AutobanConfFile : ConfFile
	{
		/// <summary>
		/// Turn autoban on?
		/// </summary>
		public bool Enabled { get; private set; }

		/// <summary>
		/// The ban threshold. When a player's score is greater than
		/// or equal to this number, they're banned.
		/// </summary>
		public int BanAt { get; private set; }

		/// <summary>
		/// The amount to increase a player's score by when a Mild incident occurs
		/// </summary>
		public int MildAmount { get; private set; }

		/// <summary>
		/// The amount to increase a player's score by when a Moderate incident occurs
		/// </summary>
		public int ModerateAmount { get; private set; }

		/// <summary>
		/// The amount to increase a player's score by when a Severe incident occurs
		/// </summary>
		public int SevereAmount { get; private set; }

		/// <summary>
		/// The amount of time it takes to reduce a player's score by one point.
		/// 
		/// Specify zero to disable reduction
		/// </summary>
		public TimeSpan ReductionTime { get; private set; }

		/// <summary>
		/// The "seed" ban length. This is used as the ban time for the first offense
		/// and in calculations for subsequent offenses
		/// </summary>
		public TimeSpan InitialBanTime { get; private set; }

		/// <summary>
		/// The type of subsequent ban length increase.
		/// <remarks>
		/// Options are:
		/// 
		/// None = BanTime
		/// Linear = BanTime * i
		/// Exponential = BanTime * (i ^ 2)
		/// 
		/// Where i is the number of times the player has been autobanned
		/// </remarks>
		/// </summary>
		public AutobanLengthIncrease LengthIncrease { get; private set; }

		/// <summary>
		/// Determines if we should reset the player's Ban Score to 0
		/// when they're autobanned. If false, only the passage of time
		/// (if enabled) will reduce the ban score.
		/// </summary>
		public bool ResetScoreOnBan { get; private set; }

		public void Load()
		{
			this.Require("system/conf/autoban.conf");

			this.Enabled = this.GetBool("enabled", false);
			this.BanAt = this.GetInt("ban_at", 10);
			this.MildAmount = this.GetInt("mild_amount", 1);
			this.ModerateAmount = this.GetInt("moderate_amount", 5);
			this.SevereAmount = this.GetInt("severe_amount", 10);
			this.ReductionTime = this.GetTimeSpan("reduction_time", TimeSpan.FromDays(7));
			this.InitialBanTime = this.GetTimeSpan("initial_ban_time", TimeSpan.FromHours(6));
			this.LengthIncrease = this.GetEnum("length_increase", AutobanLengthIncrease.Exponential);
			this.ResetScoreOnBan = this.GetBool("reset_on_ban", false);
		}
	}
}
