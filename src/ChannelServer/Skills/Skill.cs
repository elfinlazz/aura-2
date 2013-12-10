// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Structs;
using Aura.Data.Database;

namespace Aura.Channel.Skills
{
	public class Skill
	{
		public SkillInfo Info { get; protected set; }
		public SkillRankData RankInfo { get; protected set; }

		public bool IsRankable { get { return this.Info.Experience >= 100000; } }
	}
}
