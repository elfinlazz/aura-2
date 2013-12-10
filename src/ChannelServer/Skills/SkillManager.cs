// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Skills
{
	public class SkillManager
	{
	}

	public class SkillAttribute : Attribute
	{
		public SkillId[] Ids { get; protected set; }

		public SkillAttribute(params SkillId[] skillIds)
		{
			this.Ids = skillIds;
		}
	}
}
