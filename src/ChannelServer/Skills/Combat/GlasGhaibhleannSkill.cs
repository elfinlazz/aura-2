// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Glas Ghaibhleann's laser skill.
	/// </summary>
	/// <remarks>
	/// Since this is a monster skill, the implementation is based on guesses.
	/// It works, but might not be 100% correct.
	/// 
	/// I haven't seen a log of a successful hit of this skill yet.
	/// </remarks>
	[Skill(SkillId.GlasGhaibhleannSkill)]
	public class GlasGhaibhleannSkill : StandardPrepareHandler, IUseable
	{
		/// <summary>
		/// Time in milliseconds the user is being stunned for.
		/// </summary>
		private const int UseStun = 800;

		/// <summary>
		/// Time in milliseconds targets are being stunned for.
		/// </summary>
		private const int TargetStun = 3000; // ?

		/// <summary>
		/// Amount added to the Knockback meter.
		/// </summary>
		private const float KnockbackMeter = 120;

		/// <summary>
		/// Units the enemy is knocked back.
		/// </summary>
		private const int KnockbackDistance = 450;

		/// <summary>
		/// Width of the laser area of effect.
		/// </summary>
		private const int LaserRectWidth = 1000;

		/// <summary>
		/// Height of the laser area of effect.
		/// </summary>
		private const int LaserRectHeight = 400;

		/// <summary>
		/// Preapres the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public override bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			creature.StopMove();

			Send.SkillInitEffect(creature, "");
			Send.UseMotion(creature, 10, 2);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public override bool Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.Effect(creature, 5, (byte)0);
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Handles skill usage.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature attacker, Skill skill, Packet packet)
		{
			var targetAreaEntityId = packet.GetLong();

			Send.Effect(attacker, 5, (byte)1, targetAreaEntityId);

			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.Hit, attacker, skill.Info.Id, targetAreaEntityId);
			aAction.Options |= AttackerOptions.Result;
			aAction.Stun = UseStun;
			cap.Add(aAction);

			var attackerPosition = attacker.GetPosition();

			// Calculate rectangular target area
			var targetAreaPos = new Position(targetAreaEntityId);
			var poe = targetAreaPos.GetRelative(attackerPosition, -800);
			var r = (Math.PI / 2) + Math.Atan2(attackerPosition.Y - targetAreaPos.Y, attackerPosition.X - targetAreaPos.X);
			var pivot = new Point(poe.X, poe.Y);
			var p1 = new Point(pivot.X - LaserRectWidth / 2, pivot.Y - LaserRectHeight / 2);
			var p2 = new Point(pivot.X - LaserRectWidth / 2, pivot.Y + LaserRectHeight / 2);
			var p3 = new Point(pivot.X + LaserRectWidth / 2, pivot.Y + LaserRectHeight / 2);
			var p4 = new Point(pivot.X + LaserRectWidth / 2, pivot.Y - LaserRectHeight / 2);
			p1 = this.RotatePoint(p1, pivot, r);
			p2 = this.RotatePoint(p2, pivot, r);
			p3 = this.RotatePoint(p3, pivot, r);
			p4 = this.RotatePoint(p4, pivot, r);

			// Attack targets
			var targets = attacker.Region.GetCreaturesInPolygon(p1, p2, p3, p4);
			foreach (var target in targets.Where(cr => !cr.IsDead && !cr.Has(CreatureStates.NamedNpc)))
			{
				var targetPosition = target.GetPosition();

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Options = TargetOptions.Result | TargetOptions.KnockDown;
				tAction.Stun = TargetStun;
				tAction.Delay = 1200;
				cap.Add(tAction);

				// Var2: 300/1000, based on rank. Could be damage?
				var damage = skill.RankData.Var2;

				// Increase damage
				CriticalHit.Handle(attacker, attacker.GetCritChanceFor(target), ref damage, tAction);

				// Reduce damage
				SkillHelper.HandleDefenseProtection(target, ref damage);
				ManaShield.Handle(target, ref damage, tAction);

				// Apply damage
				target.TakeDamage(tAction.Damage = 300, attacker);
				target.KnockBack = KnockbackMeter;

				// Check death
				if (target.IsDead)
					tAction.Options |= TargetOptions.FinishingKnockDown;

				// Knock back
				attacker.Shove(target, KnockbackDistance);
			}

			cap.Handle();

			Send.SkillUse(attacker, skill.Info.Id, 0);
		}

		private Point RotatePoint(Point point, Point pivot, double radians)
		{
			var cosTheta = Math.Cos(radians);
			var sinTheta = Math.Sin(radians);

			var x = (int)(cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X);
			var y = (int)(sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y);

			return new Point(x, y);
		}
	}
}
