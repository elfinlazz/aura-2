// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Aura.Shared.Util;

namespace Aura.Shared.Network
{
	/// <summary>
	/// Base server, for specialized servers to inherit from.
	/// </summary>
	/// <typeparam name="TClient"></typeparam>
	public abstract class BaseServer<TClient> where TClient : BaseClient, new()
	{
		private Socket _socket;
		public List<TClient> Clients { get; set; }

		public PacketHandlerManager<TClient> Handlers { get; set; }

		/// <summary>
		/// Raised when client successfully connected.
		/// </summary>
		public event ClientConnectionEventHandler ClientConnected;

		/// <summary>
		/// Raised when client disconnected for any reason.
		/// </summary>
		public event ClientConnectionEventHandler ClientDisconnected;

		protected BaseServer()
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_socket.NoDelay = true;
			this.Clients = new List<TClient>();
		}

		/// <summary>
		/// Starts listener.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public void Start(int port)
		{
			if (this.Handlers == null)
			{
				Log.Error("No packet handler manager set, start canceled.");
				return;
			}

			this.Start(new IPEndPoint(IPAddress.Any, port));
		}

		/// <summary>
		/// Starts listener.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		public void Start(string host, int port)
		{
			if (this.Handlers == null)
			{
				Log.Error("No packet handler manager set, start canceled.");
				return;
			}

			this.Start(new IPEndPoint(IPAddress.Parse(host), port));
		}

		/// <summary>
		/// Starts listener.
		/// </summary>
		/// <param name="endPoint"></param>
		private void Start(IPEndPoint endPoint)
		{
			try
			{
				_socket.Bind(endPoint);
				_socket.Listen(10);

				_socket.BeginAccept(this.OnAccept, _socket);

				Log.Status("Server ready, listening on {0}.", _socket.LocalEndPoint);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Unable to set up socket; perhaps you're already running a server?");
				CliUtil.Exit(1);
			}
		}

		/// <summary>
		/// Stops listener.
		/// </summary>
		public void Stop()
		{
			try
			{
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
			}
			catch
			{ }
		}

		/// <summary>
		/// Handles incoming connections.
		/// </summary>
		/// <param name="result"></param>
		private void OnAccept(IAsyncResult result)
		{
			var client = new TClient();

			try
			{
				client.Socket = (result.AsyncState as Socket).EndAccept(result);

				// We don't need this here, since it's inherited from the parent
				// client.Socket.NoDelay = true;

				client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);

				this.AddClient(client);
				Log.Info("Connection established from '{0}.", client.Address);

				this.OnClientConnected(client);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "While accepting connection.");
			}
			finally
			{
				_socket.BeginAccept(this.OnAccept, _socket);
			}
		}

		/// <summary>
		/// Starts receiving for client.
		/// </summary>
		/// <param name="client"></param>
		public void AddReceivingClient(TClient client)
		{
			client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);
		}

		/// <summary>
		/// Handles sending packets, obviously.
		/// </summary>
		/// <param name="result"></param>
		protected void OnReceive(IAsyncResult result)
		{
			var client = result.AsyncState as TClient;

			try
			{
				int bytesReceived = client.Socket.EndReceive(result);
				int ptr = 0;

				if (bytesReceived == 0)
				{
					Log.Info("Connection closed from '{0}.", client.Address);
					this.KillAndRemoveClient(client);
					this.OnClientDisconnected(client);
					return;
				}

				// Handle all received bytes
				while (bytesReceived > 0)
				{
					// Length of new packet
					int length = this.GetPacketLength(client.Buffer, ptr);

					// Shouldn't actually happen...
					if (length > client.Buffer.Length)
						throw new Exception(string.Format("Buffer too small to receive full packet ({0}).", length));

					// Read whole packet and ...
					var buffer = new byte[length];
					Buffer.BlockCopy(client.Buffer, ptr, buffer, 0, length);
					bytesReceived -= length;
					ptr += length;

					// Handle it
					this.HandleBuffer(client, buffer);
				}

				// Stop if client was killed while handling.
				if (client.State == ClientState.Dead)
				{
					Log.Info("Killed connection from '{0}'.", client.Address);
					this.RemoveClient(client);
					this.OnClientDisconnected(client);
					return;
				}

				// Round $round+1, receive!
				client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, this.OnReceive, client);
			}
			catch (SocketException)
			{
				Log.Info("Connection lost from '{0}'.", client.Address);
				this.KillAndRemoveClient(client);
				this.OnClientDisconnected(client);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "While receiving data from '{0}'.", client.Address);
				this.KillAndRemoveClient(client);
				this.OnClientDisconnected(client);
			}
		}

		/// <summary>
		/// Kills and removes client from server.
		/// </summary>
		/// <param name="client"></param>
		protected void KillAndRemoveClient(TClient client)
		{
			client.Kill();
			this.RemoveClient(client);
		}

		/// <summary>
		/// Adds client to list.
		/// </summary>
		/// <param name="client"></param>
		protected void AddClient(TClient client)
		{
			lock (this.Clients)
			{
				this.Clients.Add(client);
				//Log.Status("Connected clients: {0}", _clients.Count);
			}
		}

		/// <summary>
		/// Removes client from list.
		/// </summary>
		/// <param name="client"></param>
		protected void RemoveClient(TClient client)
		{
			lock (this.Clients)
			{
				this.Clients.Remove(client);
				//Log.Status("Connected clients: {0}", _clients.Count);
			}
		}

		/// <summary>
		/// Returns length of the new incoming packet, so it can be received.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="ptr"></param>
		/// <returns></returns>
		protected abstract int GetPacketLength(byte[] buffer, int ptr);

		protected abstract void HandleBuffer(TClient client, byte[] buffer);

		protected virtual void OnClientConnected(TClient client)
		{
			if (this.ClientConnected != null)
				this.ClientConnected(client);
		}

		protected virtual void OnClientDisconnected(TClient client)
		{
			if (this.ClientDisconnected != null)
				this.ClientDisconnected(client);
		}

		public delegate void ClientConnectionEventHandler(TClient client);
	}
}
