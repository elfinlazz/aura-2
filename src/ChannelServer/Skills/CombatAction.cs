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
		private static int _actionId = 1;

		public Creature Attacker { get; set; }
		public int CombatActionId { get; set; }
		public int PrevCombatActionId { get; set; }
		public byte Hit { get; set; }
		public byte HitsMax { get; set; }
		public SkillId SkillId { get; set; }

		public List<CombatAction> Actions = new List<CombatAction>();

		private CombatActionPack()
		{
			this.CombatActionId = Interlocked.Increment(ref _actionId);
			this.Hit = 1;
			this.HitsMax = 1;
		}

		public CombatActionPack(Creature attacker, SkillId skillId)
			: this()
		{
			this.Attacker = attacker;
			this.SkillId = skillId;
		}

		public void Add(params CombatAction[] actions)
		{
			this.Actions.AddRange(actions);
		}

		//public MabiPacket GetPacket()
		//{
		//    var p = new MabiPacket(Op.CombatActionBundle, Id.Broadcast);
		//    p.PutInt(this.CombatActionId);
		//    p.PutInt(this.PrevCombatActionId);
		//    p.PutByte(this.Hit);
		//    p.PutByte(this.HitsMax);
		//    p.PutByte(0);

		//    p.PutSInt(this.Actions.Count);
		//    foreach (var action in this.Actions)
		//    {
		//        p.PutIntBin(action.GetPacket(this.CombatActionId).Build(false));
		//    }

		//    return p;
		//}
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
		public Position OldPosition { get; set; }

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

		/// <summary>
		/// Returns a packet with the base information for every action
		/// </summary>
		/// <param name="actionId"></param>
		/// <returns></returns>
		//public virtual MabiPacket GetPacket(uint actionId)
		//{
		//    var result = new MabiPacket(Op.CombatAction, this.Creature.Id);
		//    result.PutInt(actionId);
		//    result.PutLong(this.Creature.Id);
		//    result.PutByte((byte)this.Type);
		//    result.PutShort(this.StunTime);
		//    result.PutShort((ushort)this.SkillId);
		//    result.PutShort(0);
		//    return result;
		//}
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

		//public override MabiPacket GetPacket(uint actionId)
		//{
		//    var pos = this.Creature.GetPosition();

		//    var result = base.GetPacket(actionId);
		//    result.PutLong(this.TargetId);
		//    result.PutInt((uint)this.Options);
		//    result.PutByte(0);
		//    result.PutByte((byte)(!this.Has(AttackerOptions.KnockBackHit2) ? 2 : 1));
		//    result.PutInts(pos.X, pos.Y);
		//    if (this.PropId != 0)
		//        result.PutLong(this.PropId);

		//    return result;
		//}
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

		//public override MabiPacket GetPacket(uint actionId)
		//{
		//    var result = base.GetPacket(actionId);

		//    if (this.Is(CombatActionType.None))
		//        return result;

		//    var pos = this.Creature.GetPosition();
		//    var enemyPos = this.Attacker.GetPosition();

		//    if (this.Is(CombatActionType.Defended) || this.Is(CombatActionType.CounteredHit))
		//    {
		//        result.PutLong(this.Attacker.Id);
		//        result.PutInt(0);
		//        result.PutByte(0);
		//        result.PutByte(1);
		//        result.PutInt(pos.X);
		//        result.PutInt(pos.Y);
		//    }

		//    result.PutInt((uint)this.Options);
		//    result.PutFloat(this.Damage);
		//    result.PutFloat(this.ManaDamage);
		//    result.PutInt(0);

		//    result.PutFloat((float)enemyPos.X - pos.X);
		//    result.PutFloat((float)enemyPos.Y - pos.Y);
		//    if (this.IsKnock)
		//        result.PutFloats(pos.X, pos.Y);

		//    result.PutByte(0); // PDef
		//    result.PutInt(this.Delay);
		//    result.PutLong(this.Attacker.Id);

		//    return result;
		//}
	}

	public enum CombatActionCategory { Attack, Target }
}
