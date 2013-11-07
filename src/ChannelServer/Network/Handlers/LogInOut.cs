// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Database;
using Aura.Channel.Network.Sending;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <example>
		/// 001 [................] String : admin
		/// 002 [................] String : admin
		/// 003 [.79D55246A240C89] Long   : 548688344496999561
		/// 004 [..10000000000002] Long   : 4503599627370498
		/// </example>
		[PacketHandler(Op.ChannelLogin)]
		public void ChannelLogin(ChannelClient client, Packet packet)
		{
			var accountId = packet.GetString();
			// [160XXX] Double account name
			{
				packet.GetString();
			}
			var sessionKey = packet.GetLong();
			var characterId = packet.GetLong();

			// Check state
			if (client.State != ClientState.LoggingIn)
				return;

			// Check account
			var account = ChannelDb.Instance.GetAccount(accountId);
			if (account == null || account.SessionKey != sessionKey)
			{
				Log.Warning("ChannelLogin handler: Invalid account ({0}) or session ({1}).", accountId, sessionKey);
				client.Kill();
				return;
			}

			// Check character
			var character = account.GetCharacter(characterId);
			if (character.EntityId != characterId)
			{
				Log.Warning("ChannelLogin handler: Account ({0}) doesn't contain character ({1}).", accountId, characterId);
				client.Kill();
				return;
			}

			client.Account = account;
			client.Character = character;
			client.Creatures.Add(client.Character.EntityId, client.Character);

			client.State = ClientState.LoggedIn;

			Send.ChannelLoginR(client);

			if (client.Character.Region == 0)
			{
				client.Character.Region = 1;
				client.Character.SetPosition(12800, 38100);
			}

			Send.CharacterLock(client, client.Character, LockType.Unk1);
			Send.EnterRegion(client, client.Character);
		}

		/// <summary>
		/// Sent after EnterRegion.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.EnterRegionRequest)]
		public void EnterRegionRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetPlayerCreature(packet.Id);
			if (creature == null)
				return;

			Send.CharacterUnlock(client, creature, LockType.Unk1);

			Send.EnterRegionRequestR(client, creature);

			//Send.WarpRegion(client, creature);

			//var chrpck = new Packet(0, 0);
			//chrpck.AddCreatureInfo(creature, CreaturePacketType.Public);
			//var chrpckbuilt = chrpck.Build(false);
			//client.Send(new Packet(Op.EntitiesAppear, MabiId.Broadcast).PutShort(1).PutShort((short)EntityType.Character).PutInt(chrpckbuilt.Length).PutBin(chrpckbuilt));
		}

		/// <summary>
		/// Request for character information.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.ChannelCharacterInfoRequest)]
		public void ChannelCharacterInfoRequest(ChannelClient client, Packet packet)
		{
			var creature = client.GetPlayerCreature(packet.Id);
			if (creature == null)
			{
				Send.ChannelCharacterInfoRequestR_Fail(client);
				return;
			}

			// Infamous 5209, aka char info
			Send.ChannelCharacterInfoRequestR(client, creature);
		}

		/// <summary>
		/// Disconnection request.
		/// </summary>
		/// <remarks>
		/// Client doesn't disconnect till we answer.
		/// </remarks>
		/// <example>
		/// ...
		/// </example>
		[PacketHandler(Op.DisconnectRequest)]
		public void HandleDisconnect(ChannelClient client, Packet packet)
		{
			var unk1 = packet.GetByte(); // 1 | 2 (maybe login vs exit?)

			Log.Info("'{0}' is closing the connection. Saving...", client.Account.Id);

			//ChannelDb.Instance.SaveAccount(client.Account);

			//foreach (var pc in client.Creatures.Where(cr => cr is PlayerCreature))
			//    WorldManager.Instance.RemoveCreature(pc);

			client.Creatures.Clear();
			client.Character = null;
			client.Account = null;

			Send.ChannelDisconnectR(client);
		}
	}
}
