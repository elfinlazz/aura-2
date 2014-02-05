// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi.Const;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.Skills
{
	/// <summary>
	/// Collection of combat actions
	/// </summary>
	/// <remarks>
	/// A combat action (eg a player hitting a monster) consits of multiple
	/// actions, depending on the amount involved creatures. Each action
	/// has information about one creature, to make the client display
	/// the result of the attack. An attack action shows the creature
	/// hitting, a target action shows it receiving a hit.
	/// </remarks>
	public class CombatActionPack
	{
		private static int _actionId = 0;

		public Creature Attacker { get; set; }
		public int CombatActionId { get; set; }
		public int PrevCombatActionId { get; set; }
		public byte Hit { get; set; }
		public byte HitsMax { get; set; }
		public SkillId SkillId { get; set; }

		public List<CombatAction> Actions { get; protected set; }

		private CombatActionPack()
		{
			this.CombatActionId = Interlocked.Increment(ref _actionId);
			this.Hit = 1;
			this.HitsMax = 1;
			this.Actions = new List<CombatAction>();
		}

		public CombatActionPack(Creature attacker, SkillId skillId)
			: this()
		{
			this.Attacker = attacker;
			this.SkillId = skillId;
		}

		/// <summary>
		/// Adds combat actions.
		/// </summary>
		/// <param name="actions"></param>
		public void Add(params CombatAction[] actions)
		{
			this.Actions.AddRange(actions);
		}

		/// <summary>
		/// Handles actions and broadcasts action pack.
		/// </summary>
		public void Handle()
		{
			foreach (var action in this.Actions)
			{
				action.Creature.Stun = action.StunTime;

				Send.StatUpdate(action.Creature, StatUpdateType.Private, Stat.Life, Stat.LifeInjured);
				Send.StatUpdate(action.Creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);

				// Switch to battle stance
				//if (tAction.Creature.BattleState == 0)
				//{
				//    tAction.Creature.BattleState = 1;
				//    Send.ChangesStance(tAction.Creature, 0);
				//}

				// Cancel defense if applicable
				if (action.Is(CombatActionType.Defended))
					action.Creature.Skills.CancelActiveSkill();

				//if (action.Creature.IsDead)
				//{
				//    // Exp, Drops, etc.
				//    WorldManager.Instance.HandleCreatureKill(action.Creature, cap.Attacker, action.OldPosition, action.SkillId);
				//}
			}

			// Start combat action
			Send.CombatAction(this);

			// Skill used
			if (this.SkillId != SkillId.CombatMastery)
				Send.CombatUsedSkill(this.Attacker, this.SkillId);

			// End combat action
			Send.CombatActionEnd(this.Attacker, this.CombatActionId);
		}
	}

	public abstract class CombatAction
	{
		/// <summary>
		/// Creature of this action
		/// </summary>
		public Creature Creature { get; set; }

		// Type of combat action
		public CombatActionType Type { get; set; }

		/// <summary>
		/// Time before creature can move again.
		/// </summary>
		public short StunTime { get; set; }

		/// <summary>
		/// Used skill
		/// </summary>
		public SkillId SkillId { get; set; }

		/// <summary>
		/// Position before the knock back.
		/// </summary>
		//public Position OldPosition { get; set; }

		/// <summary>
		/// Returns true if action is a knock back/down.
		/// </summary>
		public abstract bool IsKnockBack { get; }

		/// <summary>
		/// Attack or Target action
		/// </summary>
		public abstract CombatActionCategory Category { get; }

		/// <summary>
		/// Returns true if the given type equals the combat action's type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool Is(CombatActionType type)
		{
			return (this.Type == type);
		}
	}

	/// <summary>
	/// Contains information about the source action part of the
	/// CombatActionPack. This part is sent first, before the target actions.
	/// </summary>
	public class AttackerAction : CombatAction
	{
		/// <summary>
		/// Attacker options
		/// </summary>
		public AttackerOptions Options { get; set; }

		/// <summary>
		/// Id of the attacked creature/area
		/// </summary>
		public long TargetId { get; set; }

		/// <summary>
		/// Id of a prop created for the skill (eg Fireball)
		/// </summary>
		public long PropId { get; set; }

		/// <summary>
		/// Returns true if KnockBackHit is set.
		/// </summary>
		public override bool IsKnockBack
		{
			get { return this.Has(AttackerOptions.KnockBackHit2) || this.Has(AttackerOptions.KnockBackHit1); }
		}

		public override CombatActionCategory Category { get { return CombatActionCategory.Attack; } }

		public AttackerAction(CombatActionType type, Creature creature, SkillId skillId, long targetId)
		{
			this.Type = type;
			this.Creature = creature;
			this.SkillId = skillId;
			this.TargetId = targetId;
		}

		/// <summary>
		/// Returns true if the specified option is set.
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		public bool Has(AttackerOptions option)
		{
			return ((this.Options & option) != 0);
		}

		/// <summary>
		/// Enables option(s)
		/// </summary>
		/// <param name="option"></param>
		public void Set(AttackerOptions option)
		{
			this.Options |= option;
		}
	}

	/// <summary>
	/// Contains information about the target action part of CombatActionPack.
	/// Multiple target actions are used, depending on the amount of targets.
	/// </summary>
	public class TargetAction : CombatAction
	{
		/// <summary>
		/// Target options
		/// </summary>
		public TargetOptions Options { get; set; }

		/// <summary>
		/// Creature attacking the target
		/// </summary>
		public Creature Attacker { get; set; }

		/// <summary>
		/// Animation delay
		/// </summary>
		public int Delay { get; set; }

		/// <summary>
		/// Normal damage
		/// </summary>
		public float Damage { get; set; }

		/// <summary>
		/// Mana damage (Mana Shield, blue)
		/// </summary>
		public float ManaDamage { get; set; }

		/// <summary>
		/// Returns true if any option involving knocking back/down is
		/// active, including finishers.
		/// </summary>
		public override bool IsKnockBack
		{
			get { return this.Has(TargetOptions.KnockDownFinish) || this.Has(TargetOptions.Smash) || this.Has(TargetOptions.KnockBack) || this.Has(TargetOptions.KnockDown) || this.Has(TargetOptions.Finished); }
		}

		public override CombatActionCategory Category { get { return CombatActionCategory.Target; } }

		public TargetAction(CombatActionType type, Creature creature, Creature attacker, SkillId skillId)
		{
			this.Type = type;
			this.Creature = creature;
			this.Attacker = attacker;
			this.SkillId = skillId;
		}

		/// <summary>
		/// Returns true if the specified option is set.
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		public bool Has(TargetOptions option)
		{
			return ((this.Options & option) != 0);
		}

		/// <summary>
		/// Enables option(s)
		/// </summary>
		/// <param name="option"></param>
		public void Set(TargetOptions option)
		{
			this.Options |= option;
		}
	}

	public enum CombatActionCategory { Attack, Target }
}
