// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using System.Globalization;
using Aura.Channel.World;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		private void EnsureGmcpAuthority(ChannelClient client)
		{
			if (client.Account.Authority < ChannelServer.Instance.Conf.World.GmcpMinAuth)
			{
				throw new ModerateViolation("Not authorized to use GMCP functions");
			}
		}

		/// <summary>
		/// Sent when closing the GMCP.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GmcpClose)]
		public void GmcpClose(ChannelClient client, Packet packet)
		{
			// Log it?

			var creature = client.GetCreatureSafe(packet.Id);

			creature.Vars.Perm.GMCP = null;
		}

		/// <summary>
		/// Summoning a character
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpSummon)]
		public void GmcpSummon(ChannelClient client, Packet packet)
		{
			var targetName = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			var target = ChannelServer.Instance.World.GetPlayer(targetName);
			if (target == null)
			{
				Send.MsgBox(creature, Localization.Get("Character '{0}' couldn't be found."), targetName);
				return;
			}

			var pos = creature.GetPosition();
			target.Warp(creature.RegionId, pos.X, pos.Y);

			Send.ServerMessage(target, Localization.Get("You've been summoned by '{0}'."), creature.Name);
		}

		/// <summary>
		/// Warping to creature
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpMoveToChar)]
		public void GmcpMoveToChar(ChannelClient client, Packet packet)
		{
			var targetName = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			var target = ChannelServer.Instance.World.GetCreature(targetName);
			if (target == null)
			{
				Send.MsgBox(creature, Localization.Get("Character '{0}' couldn't be found."), targetName);
				return;
			}

			var pos = target.GetPosition();
			creature.Warp(target.RegionId, pos.X, pos.Y);
		}

		/// <summary>
		/// Sent when clicking mini-map while GMCP is open.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpWarp)]
		public void GmcpWarp(ChannelClient client, Packet packet)
		{
			var regionId = packet.GetInt();
			var x = packet.GetInt();
			var y = packet.GetInt();

			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			creature.Jump(x, y);
		}

		/// <summary>
		/// Reviving via GMCP
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpRevive)]
		public void GmcpRevive(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);
			if (!creature.IsDead)
				return;

			this.EnsureGmcpAuthority(client);

			creature.FullHeal();
			creature.Revive();
		}

		/// <summary>
		/// The invisible ma- GM.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpInvisibility)]
		public void GmcpInvisibility(ChannelClient client, Packet packet)
		{
			var activate = packet.GetBool();

			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			if (activate)
				creature.Conditions.Activate(ConditionsA.Invisible);
			else
				creature.Conditions.Deactivate(ConditionsA.Invisible);

			Send.GmcpInvisibilityR(creature, true);
		}

		/// <summary>
		/// Kills connection of target.
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpExpel)]
		public void GmcpExpel(ChannelClient client, Packet packet)
		{
			var targetName = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			var target = ChannelServer.Instance.World.GetPlayer(targetName);
			if (target == null)
			{
				Send.MsgBox(creature, Localization.Get("Character '{0}' couldn't be found."), targetName);
				return;
			}

			// Better kill the connection, modders could bypass a dc request.
			target.Client.Kill();

			Send.MsgBox(creature, Localization.Get("'{0}' has been kicked."), targetName);
		}

		/// <summary>
		/// Bans target
		/// </summary>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpBan)]
		public void GmcpBan(ChannelClient client, Packet packet)
		{
			var targetName = packet.GetString();
			var duration = packet.GetInt();
			var reason = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			var target = ChannelServer.Instance.World.GetPlayer(targetName);
			if (target == null)
			{
				Send.MsgBox(creature, Localization.Get("Character '{0}' couldn't be found."), targetName);
				return;
			}

			var end = DateTime.Now.AddMinutes(duration);
			target.Client.Account.BanExpiration = end;
			target.Client.Account.BanReason = reason;

			// Better kill the connection, modders could bypass a dc request.
			target.Client.Kill();

			Send.MsgBox(creature, Localization.Get("'{0}' has been banned till '{1}' UTC."), targetName, end.ToUniversalTime());
		}

		/// <summary>
		/// Displays a list of NPCs?
		/// </summary>
		/// <remarks>
		/// Values and types of the response are guessed,
		/// but they seem to be working. NPCs are only displayed once
		/// in the list, probably grouped if all values are the same.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.GmcpNpcList)]
		public void GmcpNpcList(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			this.EnsureGmcpAuthority(client);

			var npcs = ChannelServer.Instance.World.GetAllGoodNpcs();

			Send.GmcpNpcListR(creature, npcs);
		}

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Given the name and the parameter of this feature we're gonna
		/// assume it's a speeed boost for now.
		/// </remarks>
		/// <example>
		/// 0001 [................] Float  : 1.0
		/// </example>
		[PacketHandler(Op.GmcpBoost)]
		public void GmcpBoost(ChannelClient client, Packet packet)
		{
			var multiplier = packet.GetFloat();

			var creature = client.GetCreatureSafe(packet.Id);

			var speedBonus = (short)(multiplier * 100 - 100);
			speedBonus = (short)Math2.Clamp(0, 1000, speedBonus);

			if (speedBonus == 0)
				creature.Conditions.Deactivate(ConditionsC.Hurry);
			else
				creature.Conditions.Activate(ConditionsC.Hurry, speedBonus);

			Send.ServerMessage(creature, Localization.Get("Speed boost: {0}x"), multiplier.ToString("0.0", CultureInfo.InvariantCulture));
		}
	}
}
