// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Runtime.InteropServices;
using Aura.Shared.Mabi.Const;

namespace Aura.Shared.Mabi.Structs
{
	/// <summary>
	/// Information about a skill and its current rank
	/// </summary>
	/// <remarks>
	/// MaxRank: This probably regulates the Advance button, officials set
	///   it to the next valid rank (current one if you can't advance yet).
	/// Experience: This is the progress, in 1/1000. If it reaches 100
	///   (100000), the advance button appears. It will be enabled if the
	///   corrosponding Flag is set, though the button works either way.
	/// ConditionCountX: This is the progress in a specific training method,
	///   but in reverse. The count is set to the max and decrements afterwards.
	///   The condition is done once it reaches 0.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SkillInfo
	{
		public SkillId Id;
		public short Version;
		public SkillRank Rank;
		public SkillRank MaxRank;
		private byte __unknown6;
		private byte __unknown7;
		public int Experience;
		public short Count;
		public SkillFlags Flag;
		public long LastPromotionTime;
		public short PromotionCount;
		public short __unknown26; // -1
		public int PromotionExp;
		public short ConditionCount1;
		public short ConditionCount2;
		public short ConditionCount3;
		public short ConditionCount4;
		public short ConditionCount5;
		public short ConditionCount6;
		public short ConditionCount7;
		public short ConditionCount8;
		public short ConditionCount9;
		public short __unknown50; // 113
	}
}
