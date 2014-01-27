// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Skills.Base;
using Aura.Shared.Network;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Mabi;
using Aura.Channel.Network.Sending;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles the Rest skill. Also called when using a chair.
	/// </summary>
	[Skill(SkillId.Rest)]
	public class RestSkillHandler : StartStopSkillHandler
	{
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			var chairItemEntityId = dict.Get<long>("ITEMID");

			if (chairItemEntityId != 0)
				this.SetUpChair(creature, chairItemEntityId);

			creature.Activate(CreatureStates.SitDown);
			Send.SitDown(creature);

			creature.Skills.GiveExp(skill, 20);

			return StartStopResult.Okay;
		}

		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Deactivate(CreatureStates.SitDown);
			Send.StandUp(creature);

			if (creature.Temp.SittingProp != null)
				this.RemoveChair(creature);

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Creates sitting prop, fails silently if item or chair
		/// data doesn't exist.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="chairItemEntityId"></param>
		private void SetUpChair(Creature creature, long chairItemEntityId)
		{
			if (chairItemEntityId == 0)
				return;

			// Check item
			var item = creature.Inventory.GetItem(chairItemEntityId);
			if (item == null || item.Data.Type != ItemType.Misc)
				return;

			// Get chair data
			var chairData = AuraData.ChairDb.Find(item.Info.Id);
			if (chairData == null)
				return;

			var pos = creature.GetPosition();

			// Effect
			if (chairData.Effect != 0)
				Send.Effect(creature, chairData.Effect, true);

			// Chair prop
			var sittingProp = new Prop((!creature.IsGiant ? chairData.PropId : chairData.GiantPropId), creature.RegionId, pos.X, pos.Y, MabiMath.ByteToRadian(creature.Direction));
			sittingProp.Info.Color1 = item.Info.Color1;
			sittingProp.Info.Color2 = item.Info.Color2;
			sittingProp.Info.Color3 = item.Info.Color3;
			sittingProp.State = "stand";
			creature.Region.AddProp(sittingProp);

			// Move char
			Send.AssignSittingProp(creature, sittingProp.EntityId, 1);

			// Update chair
			sittingProp.XML = string.Format("<xml OWNER='{0}' SITCHAR='{0}'/>", creature.EntityId);
			Send.PropUpdate(sittingProp);

			creature.Temp.CurrentChairData = chairData;
			creature.Temp.SittingProp = sittingProp;
		}

		private void RemoveChair(Creature creature)
		{
			if (creature.Temp.SittingProp == null || creature.Temp.CurrentChairData == null)
				return;

			// Effect
			if (creature.Temp.CurrentChairData.Effect != 0)
				Send.Effect(creature, Effect.CherryBlossoms, false);

			// Update chair
			creature.Temp.SittingProp.XML = string.Format("<xml OWNER='0' SITCHAR='0'/>");
			Send.PropUpdate(creature.Temp.SittingProp);

			Send.AssignSittingProp(creature, 0, 0);

			// Remove chair in 1s
			creature.Temp.SittingProp.DisappearTime = DateTime.Now.AddSeconds(1);

			creature.Temp.SittingProp = null;
		}
	}
}
