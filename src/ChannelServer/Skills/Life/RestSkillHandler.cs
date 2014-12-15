// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles the Rest skill. Also called when using a chair.
	/// </summary>
	/// <remarks>
	/// Var1: Life regen multiplicator
	/// Var2: Stamina regen multiplicator
	/// Var3: Injury regen
	/// </remarks>
	[Skill(SkillId.Rest)]
	public class RestSkillHandler : StartStopSkillHandler
	{
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.StopMove();

			var chairItemEntityId = dict.GetLong("ITEMID");

			if (chairItemEntityId != 0)
				this.SetUpChair(creature, chairItemEntityId);

			creature.Activate(CreatureStates.SitDown);
			Send.SitDown(creature);

			creature.Regens.Add("Rest", Stat.Life, (0.12f * ((skill.RankData.Var1 - 100) / 100)), creature.LifeMax);
			creature.Regens.Add("Rest", Stat.Stamina, (0.4f * ((skill.RankData.Var2 - 100) / 100)), creature.StaminaMax);
			creature.Regens.Add("Rest", Stat.LifeInjured, skill.RankData.Var3, creature.LifeMax);

			if (skill.Info.Rank == SkillRank.Novice) skill.Train(1); // Use Rest.

			return StartStopResult.Okay;
		}

		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.Deactivate(CreatureStates.SitDown);
			Send.StandUp(creature);

			creature.Regens.Remove("Rest");

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
			sittingProp.Xml.SetAttributeValue("OWNER", creature.EntityId);
			sittingProp.Xml.SetAttributeValue("SITCHAR", creature.EntityId);

			Send.PropUpdate(sittingProp);

			creature.Temp.CurrentChairData = chairData;
			creature.Temp.SittingProp = sittingProp;
		}

		/// <summary>
		/// Removes current chair prop.
		/// </summary>
		/// <param name="creature"></param>
		private void RemoveChair(Creature creature)
		{
			if (creature.Temp.SittingProp == null || creature.Temp.CurrentChairData == null)
				return;

			// Effect
			if (creature.Temp.CurrentChairData.Effect != 0)
				Send.Effect(creature, Effect.CherryBlossoms, false);

			// Update chair
			creature.Temp.SittingProp.Xml.SetAttributeValue("OWNER", 0);
			creature.Temp.SittingProp.Xml.SetAttributeValue("SITCHAR", 0);

			Send.PropUpdate(creature.Temp.SittingProp);

			Send.AssignSittingProp(creature, 0, 0);

			// Remove chair in 1s
			creature.Temp.SittingProp.DisappearTime = DateTime.Now.AddSeconds(1);

			creature.Temp.SittingProp = null;
		}
	}
}
