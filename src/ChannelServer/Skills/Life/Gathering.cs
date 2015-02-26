// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Inventory;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Gathering skill handler, used for getting wood, eggs, etc.
	/// </summary>
	[Skill(SkillId.Gathering)]
	public class Gathering : IPreparable, ICompletable, ICancelable
	{
		/// <summary>
		/// Range in which gathered items are dropped.
		/// </summary>
		private const int DropRange = 50;

		/// <summary>
		/// Max distance you may be away from the gethering target.
		/// </summary>
		private const int MaxDistance = 400;

		/// <summary>
		/// Max distance the target may move during gathering.
		/// </summary>
		private const int MaxMoveDistance = 50;

		/// <summary>
		/// Prepares skill, skips right to used.
		/// </summary>
		/// <remarks>
		/// Doesn't check anything, like what you can gather with what,
		/// because at this point there's no chance for abuse.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var entityId = packet.GetLong();
			var collectId = packet.GetInt();

			// You shall stop
			creature.StopMove();
			var creaturePosition = creature.GetPosition();

			// Get target (either prop or creature)
			var targetEntity = this.GetTargetEntity(creature.Region, entityId);
			if (targetEntity != null)
				creature.Temp.GatheringTargetPosition = targetEntity.GetPosition();

			// Check distance
			if (!creaturePosition.InRange(creature.Temp.GatheringTargetPosition, MaxDistance))
			{
				Send.Notice(creature, Localization.Get("Your arms are too short to reach that from here."));
				return false;
			}

			// ? (sets creatures position on the client side)
			Send.CollectAnimation(creature, entityId, collectId, creaturePosition);

			// Use
			Send.SkillUse(creature, skill.Info.Id, entityId, collectId);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Completes skill, handling the whole item gathering process.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var entityId = packet.GetLong();
			var collectId = packet.GetInt();

			var rnd = RandomProvider.Get();

			// Check data
			var collectData = AuraData.CollectingDb.Find(collectId);
			if (collectData == null)
			{
				Log.Warning("Gathering.Complete: Unknown collect id '{0}'", collectId);
				this.DoComplete(creature, entityId, collectId, false, 1);
				return;
			}

			// Check tools
			if (!this.CheckHand(collectData.RightHand, creature.RightHand) || !this.CheckHand(collectData.LeftHand, creature.LeftHand))
			{
				Log.Warning("Gathering.Complete: Collecting using invalid tool.", collectId);
				this.DoComplete(creature, entityId, collectId, false, 1);
				return;
			}

			// Reduce tool's durability
			if (creature.RightHand != null && collectData.DurabilityLoss > 0)
			{
				var reduce = collectData.DurabilityLoss;

				// Half dura loss if blessed
				if (creature.RightHand.IsBlessed)
					reduce = Math.Max(1, reduce / 2);

				creature.RightHand.Durability -= reduce;
				Send.ItemExpUpdate(creature, creature.RightHand);
			}

			// Get target (either prop or creature)
			var targetEntity = this.GetTargetEntity(creature.Region, entityId);

			// Check target
			if (targetEntity == null || !targetEntity.HasTag(collectData.Target))
			{
				Log.Warning("Gathering.Complete: Collecting from invalid entity '{0:X16}'", entityId);
				this.DoComplete(creature, entityId, collectId, false, 1);
				return;
			}

			// Check position
			var creaturePosition = creature.GetPosition();
			var targetPosition = targetEntity.GetPosition();

			if (!creaturePosition.InRange(targetPosition, MaxDistance))
			{
				Send.Notice(creature, Localization.Get("Your arms are too short to reach that from here."));
				this.DoComplete(creature, entityId, collectId, false, 1);
				return;
			}

			// Check if moved
			if (creature.Temp.GatheringTargetPosition.GetDistance(targetPosition) > MaxMoveDistance)
			{
				this.DoComplete(creature, entityId, collectId, false, 3);
				return;
			}

			// Determine success
			var successChance = this.GetSuccessChance(creature, collectData);
			var collectSuccess = rnd.NextDouble() * 100 < successChance;

			// Get reduction
			var reduction = collectData.ResourceReduction;
			if (ChannelServer.Instance.Weather.GetWeatherType(creature.RegionId) == WeatherType.Rain)
				reduction += collectData.ResourceReductionRainBonus;

			// Check resource
			if (targetEntity is Prop)
			{
				var targetProp = (Prop)targetEntity;

				targetProp.Resource += (float)((DateTime.Now - targetProp.LastCollect).TotalMinutes * collectData.ResourceRecovering);
				if (targetProp.Resource < collectData.ResourceReduction)
				{
					this.DoComplete(creature, entityId, collectId, false, 2);
					return;
				}

				if (collectSuccess)
				{
					if (!ChannelServer.Instance.Conf.World.InfiniteResources)
						targetProp.Resource -= collectData.ResourceReduction;
					targetProp.LastCollect = DateTime.Now;
				}
			}
			else
			{
				var targetCreature = (Creature)targetEntity;

				if (targetCreature.Mana < collectData.ResourceReduction)
				{
					this.DoComplete(creature, entityId, collectId, false, 2);
					return;
				}

				if (collectSuccess && !ChannelServer.Instance.Conf.World.InfiniteResources)
					targetCreature.Mana -= collectData.ResourceReduction;
			}

			// Drop
			var receiveItemId = 0;
			if (collectSuccess)
			{
				// Product
				var itemId = receiveItemId = collectData.GetRndProduct(rnd);
				if (itemId != 0)
				{
					var item = new Item(itemId);
					if (collectData.Source == 0)
						item.Drop(creature.Region, creaturePosition.GetRandomInRange(DropRange, rnd));
					else
					{
						creature.Inventory.Remove(creature.RightHand);
						creature.Inventory.Add(item, creature.Inventory.RightHandPocket);
					}
				}

				// Product2
				itemId = collectData.GetRndProduct2(rnd);
				if (itemId != 0)
				{
					var item = new Item(itemId);
					item.Drop(creature.Region, creaturePosition.GetRandomInRange(DropRange, rnd));
				}
			}
			// TODO: Figure out how fail products work.
			//else
			//{
			//	// FailProduct
			//	var itemId = receiveItemId = collectData.GetRndFailProduct(rnd);
			//	if (itemId != 0)
			//	{
			//		var item = new Item(itemId);
			//		if (collectData.Source == 0)
			//			item.Drop(creature.Region, creaturePosition.GetRandomInRange(DropRange, rnd));
			//		else
			//		{
			//			creature.Inventory.Remove(creature.RightHand);
			//			creature.Inventory.Add(item, creature.Inventory.RightHandPocket);
			//		}
			//	}

			//	// FailProduct2
			//	itemId = collectData.GetRndFailProduct2(rnd);
			//	if (itemId != 0)
			//	{
			//		var item = new Item(itemId);
			//		item.Drop(creature.Region, creaturePosition.GetRandomInRange(DropRange, rnd));
			//	}
			//}

			// Events
			ChannelServer.Instance.Events.OnCreatureCollected(new CollectEventArgs(creature, collectData, collectSuccess, receiveItemId));

			// Complete
			this.DoComplete(creature, entityId, collectId, collectSuccess, 0);
		}

		/// <summary>
		/// Returns entity by id or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		private Entity GetTargetEntity(Region region, long entityId)
		{
			var isProp = (entityId >= MabiId.ClientProps && entityId < MabiId.AreaEvents);
			var targetEntity = (isProp ? (Entity)region.GetProp(entityId) : (Entity)region.GetCreature(entityId));

			return targetEntity;
		}

		/// <summary>
		/// Returns chance for a successful gathering.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="collectData"></param>
		/// <returns></returns>
		private float GetSuccessChance(Creature creature, CollectingData collectData)
		{
			// Base
			var successChance = collectData.SuccessRate;

			// Add chance for herbs based on Herbalism rank
			if (collectData.Target.Contains("/herb/"))
				successChance += Herbalism.GetChance(creature, collectData);

			// Add Production Mastery bonus
			successChance = ProductionMastery.IncreaseChance(creature, successChance);

			return successChance;
		}

		/// <summary>
		/// Sends use motion and SkillComplete.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="entityId"></param>
		/// <param name="collectId"></param>
		/// <param name="success"></param>
		/// <param name="failCode"></param>
		private void DoComplete(Creature creature, long entityId, int collectId, bool success, short failCode)
		{
			Send.UseMotion(creature, 14, success ? 2 : 3);

			if (success)
				Send.SkillComplete(creature, SkillId.Gathering, entityId, collectId);
			else
				Send.SkillCompleteUnk(creature, SkillId.Gathering, entityId, collectId, failCode);
		}

		/// <summary>
		/// Returns true if item is correct for collect tag.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		private bool CheckHand(string tag, Item item)
		{
			if (string.IsNullOrWhiteSpace(tag) || tag == "/")
				return true;

			if ((tag == "/barehand/" && item != null) || (tag != "/barehand/" && (item == null || !item.Data.HasTag(tag))))
				return false;

			return true;
		}

		/// <summary>
		/// Cancels skill, or rather the animation.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			Send.CollectAnimationCancel(creature);
		}
	}
}
