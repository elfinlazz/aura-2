// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using Aura.Shared.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Quests
{
	public class PtjTrackRecord
	{
		/// <summary>
		/// The type of PTJ
		/// </summary>
		public PtjType Type { get; set; }

		/// <summary>
		/// How many times the player reported, no matter the outcome
		/// </summary>
		public int Done { get; set; }

		/// <summary>
		/// How many times the PTJ was successful
		/// </summary>
		public int Success { get; set; }

		/// <summary>
		/// When the player reported last.
		/// </summary>
		public DateTime LastChange { get; set; }

		/// <summary>
		/// PTJ success rate
		/// </summary>
		public float SuccessRate { get { return (this.Done == 0 ? 0 : 100f / this.Done * this.Success); } }

		/// <summary>
		/// Creates new track record.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="done"></param>
		/// <param name="success"></param>
		/// <param name="last"></param>
		public PtjTrackRecord(PtjType type, int done, int success, DateTime last)
		{
			this.Type = type;
			this.Done = done;
			this.Success = success;
			this.LastChange = last;
		}

		/// <summary>
		/// Calculates quest level, based on success rate.
		/// </summary>
		/// <returns></returns>
		public QuestLevel GetQuestLevel()
		{
			var successRate = this.SuccessRate;

			if (this.Done > 100 && successRate >= 92)
				return QuestLevel.Adv;

			if (this.Done > 50 && successRate >= 70)
				return QuestLevel.Int;

			return QuestLevel.Basic;
		}
	}
}
