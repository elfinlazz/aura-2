// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Super Icebolt skill handler (GM skill)
	/// </summary>
	/// <remarks>
	/// Var1: Min damage
	/// Var2: Max damage
	/// 
	/// Seems to be just a copy of Icebolt, with only one rank, no casting time,
	/// and very high damage.
	/// </remarks>
	[Skill(SkillId.SuperIcebolt)]
	public class SuperIcebolt : Icebolt
	{
		public override void Init()
		{
		}
	}
}
