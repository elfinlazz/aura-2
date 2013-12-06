// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using System.Text.RegularExpressions;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Request to talk to an NPC.
		/// </summary>
		/// <example>
		/// 0001 [0010F0000000032A] Long   : 4767482418037546
		/// </example>
		[PacketHandler(Op.NpcTalkStart)]
		public void NpcTalkStart(ChannelClient client, Packet packet)
		{
			var npcId = packet.GetLong();

			// Check creature
			var creature = client.GetCreature(packet.Id);
			if (creature == null || creature.Region == null)
				return;

			// Check NPC
			var target = creature.Region.GetNpc(npcId);
			if (target == null)
			{
				Send.NpcTalkStartR_Fail(creature);

				Log.Warning("Creature '{0}' tried to talk to non-existing NPC '{1}'.", creature.Name, npcId);
				return;
			}

			// Check distance
			if (target.GetPosition().GetDistance(creature.GetPosition()) > 1000)
			{
				Send.MsgBox(creature, Localization.Get("world.too_far")); // You're too far away.
				Send.NpcTalkStartR_Fail(creature);

				Log.Warning("Creature '{0}' tried to talk to NPC '{1}' out of range.", creature.Name, npcId);
				return;
			}

			Send.NpcTalkStartR(creature, npcId);

			client.NpcSession.Start(target);

			// Get enumerator and start first run.
			client.NpcSession.State = target.Script.Talk(creature).GetEnumerator();
			client.NpcSession.Continue();
		}

		/// <summary>
		/// Sent when "End Conversation" button is clicked.
		/// </summary>
		/// <example>
		/// 001 [0010F00000000003] Long   : 4767482418036739
		/// 002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.NpcTalkEnd)]
		public void NpcTalkEnd(ChannelClient client, Packet packet)
		{
			var npcId = packet.GetLong();
			var unkByte = packet.GetByte();

			// Check creature
			var creature = client.GetCreature(packet.Id);
			if (creature == null || creature.Region == null)
				return;

			// Check session
			if (!client.NpcSession.IsValid(npcId))
			{
				Log.Warning("Player '{0}' tried ending invalid NPC session.", creature.Name);
				//return;
			}

			client.NpcSession.Clear();

			Send.NpcTalkEndR(creature, npcId);
		}

		/// <summary>
		/// Sent whenever a button, other than "Continue", is pressed
		/// while the client is in "SelectInTalk" mode.
		/// </summary>
		/// <example>
		/// 001 [................] String : <result session='1837'><this type="character">4503599627370498</this><return type="string">@end</return></result>
		/// 002 [........0000072D] Int    : 1837
		/// </example>
		[PacketHandler(Op.NpcTalkSelect)]
		public void NpcTalkSelect(ChannelClient client, Packet packet)
		{
			var result = packet.GetString();
			var sessionid = packet.GetInt();

			var creature = client.GetCreature(packet.Id);
			if (creature == null)
				return;

			// Check session
			if (!client.NpcSession.IsValid())
			{
				Log.Warning("NpcTalkSelect: Player '{0}' is in invalid session.", creature.Name);
				Send.NpcTalkEndR(creature, client.NpcSession.Target.EntityId);
				return;
			}

			// Check result string
			var match = Regex.Match(result, "<return type=\"string\">(?<result>[^<]*)</return>");
			if (!match.Success)
			{
				Log.Warning("NpcTalkSelect: Player '{0}' sent invalid return ({1}).", creature.Name, result);
				Send.NpcTalkEndR(creature, client.NpcSession.Target.EntityId);
				return;
			}

			var response = match.Groups["result"].Value;

			if (response == "@end")
			{
				client.NpcSession.Target.Script.EndConversation(creature);
				return;
			}

			// Cut @input "prefix" added for <input> element.
			if (response.StartsWith("@input"))
				response = response.Substring(7).Trim();

			client.NpcSession.SetResponse(match.Groups["result"].Value);
			client.NpcSession.Continue();
		}

		/// <summary>
		/// Sent when selecting a keyword, to check the validity.
		/// </summary>
		/// <remarks>
		/// Client blocks until the server answers it.
		/// </remarks>
		/// <example>
		/// 001 [................] String : personal_info
		/// </example>
		[PacketHandler(Op.NpcTalkKeyword)]
		public void NpcTalkKeyword(ChannelClient client, Packet packet)
		{
			var keyword = packet.GetString();

			var character = client.GetPlayerCreature(packet.Id);
			if (character == null)
				return;

			// Check session
			if (!client.NpcSession.IsValid())
			{
				Send.NpcTalkKeywordR_Fail(character);
				Log.Warning("Player '{0}' sent a keyword without valid NPC session.", character.Name);
				return;
			}

			// TODO: Actually check keyword
			//if(character doesn't have keyword)
			//	Send.NpcTalkKeywordR_Fail(character);

			Send.NpcTalkKeywordR(character, keyword);
		}
	}
}
