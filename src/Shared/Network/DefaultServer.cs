// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Util;

namespace Aura.Shared.Network
{
	/// <summary>
	/// Normal Mabi server (Login, Channel).
	/// </summary>
	/// <typeparam name="TClient"></typeparam>
	public class DefaultServer<TClient> : BaseServer<TClient> where TClient : BaseClient, new()
	{
		protected override int GetPacketLength(byte[] buffer, int ptr)
		{
			// <0x??><int:length><...>
			return
				(buffer[ptr + 1] << 0) +
				(buffer[ptr + 2] << 8) +
				(buffer[ptr + 3] << 16) +
				(buffer[ptr + 4] << 24);
		}

		protected override void HandleBuffer(TClient client, byte[] buffer)
		{
			var length = buffer.Length;

			// Cut 4 bytes at the end (checksum?)
			Array.Resize(ref buffer, length -= 4);

			// Write new length into the buffer.
			BitConverter.GetBytes(length).CopyTo(buffer, 1);

			// Decrypt packet if crypt flag isn't 3.
			if (buffer[5] != 0x03)
				client.DecodeBuffer(buffer);

			//Log.Debug("in:  " + BitConverter.ToString(buffer));

			// Flag 1 is a heartbeat, acknowledge and send back.
			if (buffer[5] == 0x01)
			{
				BitConverter.GetBytes(BitConverter.ToUInt32(buffer, 6) ^ 0x98BADCFE).CopyTo(buffer, 6);
				client.Socket.Send(buffer);
			}
			else
			{
				// First packet, skip challenge and send success msg.
				if (client.State == ClientState.BeingChecked)
				{
					client.Send(new byte[] { 0x88, 0x07, 0x00, 0x00, 0x00, 0x00, 0x07 });

					client.State = ClientState.LoggingIn;
				}
				// Actual packets
				else
				{
					// Start reading after the protocol header
					var packet = new Packet(buffer, 6);

					//Logger.Debug(packet);

					try
					{
						this.Handlers.Handle(client, packet);
					}
					catch (Exception ex)
					{
						Log.Exception(ex, "There has been a problem while handling '{0:X4}'.", packet.Op);
					}
				}
			}
		}

		protected override void OnClientConnected(TClient client)
		{
			// Send seed
			client.Socket.Send(BitConverter.GetBytes(client.Crypto.Seed));

			base.OnClientConnected(client);
		}
	}
}
