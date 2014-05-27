// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text.RegularExpressions;

namespace Aura.Data
{
	[Serializable]
	public abstract class TaggableData
	{
		public string Tags { get; set; }

		/// <summary>
		/// Return true if tags contain input.
		/// </summary>
		/// <remarks>
		/// Use asterisks (*) as place-holders.
		/// </remarks>
		/// <example>
		/// Tags: /animal/beast/blackleopard/goodnpc_no_attack_pc/activate_signal/revivebyrp/heal/pc/neutral/unable_tame/unable_bubble/unable_unsummon/no_finish/
		/// 
		/// Input: */*
		/// Result: True
		/// 
		/// Input: */blackleopard/*
		/// Result: True
		/// 
		/// Input: */animal/*
		/// Result: True
		/// 
		/// Input: */bear/*
		/// Result: False
		/// </example>
		/// <param name="tag"></param>
		/// <returns></returns>
		public bool HasTag(string tag)
		{
			tag = tag.Replace("*", ".*");
			return Regex.IsMatch(this.Tags, tag, RegexOptions.Compiled);
		}
	}
}
