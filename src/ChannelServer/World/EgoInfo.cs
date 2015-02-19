// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World
{
	public class EgoInfo
	{
		/// <summary>
		/// Ego's race, displayed in stat window.
		/// </summary>
		public EgoRace Race { get; set; }

		/// <summary>
		/// Ego's name, displayed in stat window.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Ego's strength level
		/// </summary>
		public byte StrLevel { get; set; }

		/// <summary>
		/// Ego's strength exp
		/// </summary>
		public int StrExp { get; set; }

		/// <summary>
		/// Ego's intelligence level
		/// </summary>
		public byte IntLevel { get; set; }

		/// <summary>
		/// Ego's intelligence exp
		/// </summary>
		public int IntExp { get; set; }

		/// <summary>
		/// Ego's dexterity level
		/// </summary>
		public byte DexLevel { get; set; }

		/// <summary>
		/// Ego's dexterity exp
		/// </summary>
		public int DexExp { get; set; }

		/// <summary>
		/// Ego's will level
		/// </summary>
		public byte WillLevel { get; set; }

		/// <summary>
		/// Ego's will exp
		/// </summary>
		public int WillExp { get; set; }

		/// <summary>
		/// Ego's luck level
		/// </summary>
		public byte LuckLevel { get; set; }

		/// <summary>
		/// Ego's luck exp
		/// </summary>
		public int LuckExp { get; set; }

		/// <summary>
		/// Ego's social level
		/// </summary>
		public byte SocialLevel { get; set; }

		/// <summary>
		/// Ego's social exp
		/// </summary>
		public int SocialExp { get; set; }

		/// <summary>
		/// Awakening energy counter
		/// </summary>
		public byte AwakeningEnergy { get; set; }

		/// <summary>
		/// Awakening exp
		/// </summary>
		public int AwakeningExp { get; set; }

		/// <summary>
		/// Time the ego was fed last
		/// </summary>
		public DateTime LastFeeding { get; set; }

		/// <summary>
		/// Creates and initializes new EgoInfo.
		/// </summary>
		public EgoInfo()
		{
			this.StrLevel = 1;
			this.IntLevel = 1;
			this.DexLevel = 1;
			this.WillLevel = 1;
			this.LuckLevel = 1;
			this.SocialLevel = 1;
			this.LastFeeding = DateTime.Now;
		}

		/// <summary>
		/// Creates a copy of this object.
		/// </summary>
		/// <returns></returns>
		public EgoInfo Copy()
		{
			var result = new EgoInfo();

			result.Name = this.Name;
			result.Race = this.Race;
			result.StrLevel = this.StrLevel;
			result.StrExp = this.StrExp;
			result.IntLevel = this.IntLevel;
			result.IntExp = this.IntExp;
			result.DexLevel = this.DexLevel;
			result.DexExp = this.DexExp;
			result.WillLevel = this.WillLevel;
			result.WillExp = this.WillExp;
			result.LuckLevel = this.LuckLevel;
			result.LuckExp = this.LuckExp;
			result.SocialLevel = this.SocialLevel;
			result.SocialExp = this.SocialExp;
			result.AwakeningEnergy = this.AwakeningEnergy;
			result.AwakeningExp = this.AwakeningExp;
			result.LastFeeding = this.LastFeeding;

			return result;
		}
	}
}
