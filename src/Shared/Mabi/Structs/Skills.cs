// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Runtime.InteropServices;
using Aura.Shared.Mabi.Const;

namespace Aura.Shared.Mabi.Structs
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SkillInfo
	{
		public SkillId Id;
		public ushort Version;
		public SkillRank Rank;
		public SkillRank MaxRank;
		private byte __unknown6;
		private byte __unknown7;
		public int Experience;
		public ushort Count;
		public ushort Flag;
		public long LastPromotionTime;
		public ushort PromotionCount;
		private ushort __unknown26;
		public int PromotionExp;
		public ushort ConditionCount1;
		public ushort ConditionCount2;
		public ushort ConditionCount3;
		public ushort ConditionCount4;
		public ushort ConditionCount5;
		public ushort ConditionCount6;
		public ushort ConditionCount7;
		public ushort ConditionCount8;
		public ushort ConditionCount9;
		private ushort __unknown50;
	}
}
