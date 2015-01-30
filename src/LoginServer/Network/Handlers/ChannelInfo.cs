// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Network;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// Request for connection information for the channel.
		/// </summary>
		/// <remarks>
		/// Sent after disconnect info, which makes the client stuck if the
		/// connection to the channel fails.
		/// </remarks>
		/// <example>
		/// Normal
		/// 0001 [................] String : Aura
		/// 0002 [................] String : Ch1
		/// 0003 [..............00] Byte   : 0
		/// 0004 [........00000001] Int    : 1
		/// 0005 [0010000000000002] Long   : 4503599627370498
		/// 
		/// Rebirth
		/// 0001 [................] String : Aura
		/// 0002 [................] String : Ch1
		/// 0003 [..............01] Byte   : 1
		/// 0004 [0010000000000002] Long   : 4503599627370498
		/// 0005 [0000000000000000] Long   : 0
		/// </example>
		[PacketHandler(Op.ChannelInfoRequest)]
		public void ChannelInfoRequest(LoginClient client, Packet packet)
		{
			var serverName = packet.GetString();
			var channelName = packet.GetString();
			var rebirth = packet.GetBool();
			if (!rebirth)
				packet.GetInt(); // unk
			var characterId = packet.GetLong();

			// Check channel and character
			var channelInfo = LoginServer.Instance.ServerList.GetChannel(serverName, channelName);
			var character = client.Account.GetCharacter(characterId) ?? client.Account.GetPet(characterId);

			if (channelInfo == null || character == null)
			{
				Send.ChannelInfoRequestR_Fail(client);
				return;
			}

			// Uninitialize if rebirth requested, so character goes to Nao.
			if (rebirth)
				LoginServer.Instance.Database.UninitializeCreature(character.CreatureId);

			// Success
			Send.ChannelInfoRequestR(client, channelInfo, characterId);
		}
	}
}
