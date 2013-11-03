// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Net.Sockets;
using Aura.Shared.Util;

namespace Aura.Shared.Network
{
	public class BaseClient
	{
		public Socket Socket { get; set; }
		public ClientSocketBuffer Buffer { get; set; }
		public ClientState State { get; set; }
		public MabiCrypto Crypto { get; protected set; }

		private string _address;
		public string Address
		{
			get
			{
				if (_address == null)
				{
					try
					{
						_address = this.Socket.RemoteEndPoint.ToString();
					}
					catch
					{
						_address = "?";
					}
				}

				return _address;
			}
		}

		public BaseClient()
		{
			this.Buffer = new ClientSocketBuffer();
			this.Crypto = new MabiCrypto(0x41757261); // 0xAura
		}

		public bool Is(ClientState state)
		{
			return (this.State == state);
		}

		/// <summary>
		/// Sends buffer (duh).
		/// </summary>
		/// <param name="buffer"></param>
		public void Send(byte[] buffer)
		{
			if (this.State == ClientState.Dead)
				return;

			// Set raw flag
			buffer[5] = 0x03;

			//Log.Debug("out: " + BitConverter.ToString(buffer));

			try
			{
				this.Socket.Send(buffer);
			}
			catch (Exception ex)
			{
				Log.Error("Unable to send packet to '{0}'. ({1})", this.Address, ex.Message);
			}
		}

		/// <summary>
		/// Builds buffer from packet and sends it.
		/// </summary>
		/// <param name="packet"></param>
		public void Send(MabiPacket packet)
		{
			this.Send(packet.Build());
		}

		/// <summary>
		/// Kills client connection.
		/// </summary>
		public void Kill()
		{
			if (this.State != ClientState.Dead)
			{
				try
				{
					this.Socket.Shutdown(SocketShutdown.Both);
					this.Socket.Close();
				}
				catch
				{ }

				// Naturally, we have to clean up after killing somebody.
				this.CleanUp();

				this.State = ClientState.Dead;
			}
			else
			{
				Log.Warning("Client got killed multiple times." + Environment.NewLine + Environment.StackTrace);
			}
		}

		/// <summary>
		/// Takes care of client's remains (saving chars, etc)
		/// </summary>
		protected virtual void CleanUp()
		{

		}
	}

	public class ClientSocketBuffer
	{
		private const int Size = 0x1000;

		public byte[] Front, Back;
		public int Remaining, Ptr;

		public ClientSocketBuffer()
		{
			this.Front = new byte[Size];
			this.Back = new byte[Size];
		}

		public void ResetBack()
		{
			this.Back = new byte[Size];
			this.Ptr = 0;
			this.Remaining = 0;
		}
	}

	public enum ClientState { BeingChecked, LoggingIn, LoggedIn, Dead }
}
