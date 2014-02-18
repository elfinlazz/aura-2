// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Shared.Mabi.Const
{
	public enum ObjectiveType : byte
	{
		Kill = 1,
		Collect = 2,
		Talk = 3,
		Deliver = 4,
		ReachRank = 9,
		ReachLevel = 15,
	}

	public enum RewardType : byte
	{
		Item = 1,
		Gold = 2,
		Exp = 3,
		ExplExp = 4,
		AP = 5,
		Skill = 8, // ?
	}
}
