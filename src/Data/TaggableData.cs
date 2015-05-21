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
		/// 
		/// Judging by how the tags are used in any place I could find
		/// I don't think devCAT actually parses them, but uses regex.
		/// The only thing in official tags that can't be resolved
		/// using regex is "&", which means you have to split the string,
		/// but that's it. 
		/// 
		/// And: */equip/* & */armor/* & */cloth/*
		/// Or: /equip/|/cloth/
		/// 
		/// I haven't seen a combination of the two, although it would work,
		/// as long as it doesn't get too complex.
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
		/// Input: /animal/
		/// Result: True
		/// 
		/// Input: */bear/*
		/// Result: False
		/// 
		/// Input: /animal/|/blackleopard/
		/// Result: True
		/// 
		/// Input: /animal/&/unable_bubble/
		/// Result: True
		/// </example>
		/// <param name="tag"></param>
		/// <returns></returns>
		public bool HasTag(string tag)
		{
			tag = tag.Replace("*", ".*");

			var tags = tag.Split('&');
			foreach (var t in tags)
			{
				var check = t.Trim();
				if (!Regex.IsMatch(this.Tags, check))
					return false;
			}

			return true;
		}
	}
}
