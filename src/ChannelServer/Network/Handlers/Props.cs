// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Linq;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when prop is "attacked".
		/// </summary>
		/// <example>
		/// 001 [00A1000100090001] Long   : 45317475545972737
		/// </example>
		[PacketHandler(Op.HitProp)]
		public void HitProp(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			// Check creature and region
			var creature = client.GetCreatureSafe(packet.Id);
			if (creature.Region == Region.Limbo || creature.IsDead)
				return;

			// Check lock
			if (!creature.Can(Locks.Attack))
			{
				Log.Debug("Attack locked for '{0}'.", creature.Name);
				Send.HitPropR(creature, false);
				return;
			}

			// Check prop
			var prop = creature.Region.GetProp(entityId);
			if (prop == null)
			{
				Log.Warning("HitProp: Player '{0}' tried to hit unknown prop '{1}'.", creature.Name, entityId.ToString("X16"));
				Send.ServerMessage(creature, "Unknown target.");
				Send.HitPropR(creature, false);
				return;
			}

			if (creature.GetPosition().InRange(prop.GetPosition(), 1500))
			{
				creature.Stun = 1000;
				Send.HittingProp(creature, prop.EntityId, 1000);

				if (prop.Behavior != null)
				{
					prop.Behavior(creature, prop);
				}
				else
				{
					Log.Unimplemented("HitProp: No prop behavior for '{0}'.", prop.EntityIdHex);
				}
			}
			else
			{
				Send.Notice(creature, NoticeType.MiddleLower, Localization.Get("You're too far away."));
				Log.Warning("HitProp: Player '{0}' tried to hit prop out of range.", creature.Name);
			}

			Send.HitPropR(creature, true);
		}

		/// <summary>
		/// Sent when prop is "touched".
		/// </summary>
		/// <remarks>
		/// Mabinogi, hitting and touching props since 2004.
		/// </remarks>
		/// <example>
		/// 001 [00A000010009042A] Long   : 45036000569263146
		/// </example>
		[PacketHandler(Op.TouchProp)]
		public void TouchProp(ChannelClient client, Packet packet)
		{
			var entityId = packet.GetLong();

			// Check creature and region
			var creature = client.GetCreatureSafe(packet.Id);
			if (creature.Region == Region.Limbo || creature.IsDead)
				return;

			// Check prop
			var prop = creature.Region.GetProp(entityId);
			if (prop == null)
			{
				Log.Warning("TouchProp: Player '{0}' tried to touch unknown prop '{1}'.", creature.Name, entityId.ToString("X16"));
				Send.ServerMessage(creature, "Unknown target.");
				goto L_End;
			}

			// Check behavior
			if (prop.Behavior == null)
			{
				Log.Unimplemented("TouchProp: No prop behavior for '{0}'.", prop.EntityIdHex);
				goto L_End;
			}

			// Check distance to centers
			// Props can be quite big, and the center of it isn't necessarily
			// where the player will touch it, so a simple range check
			// doesn't work properly, e.g. with dungeon's boss doors.
			// The proper way to solve this would probably be checking for a
			// colission with the prop's shape I guess, but we'll just be lazy
			// and assume there are no interactable props which's shapes
			// are so large that a center distance check on all shapes isn't
			// enough.
			// If the prop doesn't have a shape, aka it doesn't have collision,
			// we have no choice but to use the prop's center and hope for the best.
			var creaturePos = creature.GetPosition();
			var inRange = false;

			if (prop.Shapes.Count != 0)
			{
				var shapes = prop.Shapes.ToList();
				foreach (var shape in shapes)
				{
					var centerX = (shape.Min(a => a.X) + shape.Max(a => a.X)) / 2;
					var centerY = (shape.Min(a => a.Y) + shape.Max(a => a.Y)) / 2;
					var pos = new Position(centerX, centerY);

					if (creaturePos.InRange(pos, 1000))
					{
						inRange = true;
						break;
					}
				}
			}
			else
				inRange = creaturePos.InRange(prop.GetPosition(), 1000);

			if (!inRange)
			{
				Log.Warning("TouchProp: Player '{0}' tried to touch prop out of range.", creature.Name);
				goto L_End;
			}

			// Run behavior
			prop.Behavior(creature, prop);

		L_End:
			Send.TouchPropR(creature);
		}
	}
}
