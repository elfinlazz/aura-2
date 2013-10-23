// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Login.Network.Handlers
{
	public partial class LoginServerHandlers : PacketHandlerManager<LoginClient>
	{
		/// <summary>
		/// First actualy packet from the client, includes client
		/// identification hash, from "data\vf.dat".
		/// </summary>
		/// <example>
		/// NA166
		/// 001 [..............01] Byte   : 1
		/// 002 [................] String : USA_Regular-A2C30B-BDA-F8C
		/// 003 [........00000000] Int    : 0
		/// 004 [................] String : admin
		/// 005 [................] String : admin
		/// </example>
		[PacketHandler(Op.ClientIdent)]
		public void ClientIdent(LoginClient client, MabiPacket packet)
		{
			var unkByte = packet.GetByte();
			var ident = packet.GetString();
			// [180x00] Added some time in G18
			{
				var unkInt = packet.GetInt();
				var accountName1 = packet.GetString(); // sometimes empty?
				var accountName2 = packet.GetString();
			}

			//if (ident != "WHO_Gives-A10211-799-107")
			//    client.Kill();

			Send.CheckIdentR(client, true);
		}

		/// <summary>
		/// Login packet
		/// </summary>
		/// <example>
		/// 001 [..............05] Byte   : 5
		/// 002 [................] String : admin
		/// 003 [................] String : ...
		/// 004 [................] Bin    : ...
		/// 005 [........00000000] Int    : 0
		/// 006 [........00000000] Int    : 0
		/// 007 [................] String : 192.168.178.20
		/// 
		/// 001 [..............14] Byte   : 20
		/// 002 [................] String : admin
		/// 003 [................] String : admin
		/// 004 [0000000000000000] Long   : ...
		/// 005 [................] String : ...
		/// 006 [................] Bin    : ...
		/// 007 [........00000000] Int    : 0
		/// 008 [........00000000] Int    : 0
		/// 009 [................] String : ...
		/// 
		/// </example>
		[PacketHandler(Op.Login)]
		public void Login(LoginClient client, MabiPacket packet)
		{
			Log.Debug(packet);
		}
	}
}
