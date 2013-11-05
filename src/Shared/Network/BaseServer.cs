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
		private List<TClient> _clients;

		public PacketHandlerManager<TClient> Handlers { get; set; }

		public BaseServer()
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_clients = new List<TClient>();
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

				_socket.BeginAccept(new AsyncCallback(OnAccept), _socket);

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
				client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), client);

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
				_socket.BeginAccept(new AsyncCallback(this.OnAccept), _socket);
			}
		}

		/// <summary>
		/// Handles sending packets, obviously.
		/// </summary>
		/// <param name="result"></param>
		private void OnReceive(IAsyncResult result)
		{
			var client = result.AsyncState as TClient;

			try
			{
				int bytesReceived = client.Socket.EndReceive(result);
				int ptr = 0;

				if (bytesReceived == 0)
				{
					this.KillAndRemoveClient(client);
					Log.Info("Connection closed from '{0}.", client.Address);
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

					// handle it
					this.HandleBuffer(client, buffer);
				}

				// Stop if client was killed while handling.
				if (client.State == ClientState.Dead)
				{
					this.RemoveClient(client);
					Log.Info("Killed connection from '{0}'.", client.Address);
					return;
				}

				// Round $round+1, receive!
				client.Socket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), client);
			}
			catch (SocketException)
			{
				this.KillAndRemoveClient(client);
				Log.Info("Connection lost from '{0}'.", client.Address);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				this.KillAndRemoveClient(client);
				Log.Exception(ex, "While receiving data from '{0}'.", client.Address);
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
			lock (_clients)
			{
				_clients.Add(client);
				//Log.Status("Connected clients: {0}", _clients.Count);
			}
		}

		/// <summary>
		/// Removes client from list.
		/// </summary>
		/// <param name="client"></param>
		protected void RemoveClient(TClient client)
		{
			lock (_clients)
			{
				_clients.Remove(client);
				//Log.Status("Connected clients: {0}", _clients.Count);
			}
		}

		/// <summary>
		/// Handler for unhandled exceptions.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Log.Error("Oh no! Ferghus escaped his memory block and infected the rest of the server!");
				Log.Error("Aura has encountered an unexpected and unrecoverable error. We're going to try to save as much as we can.");
			}
			catch { }
			try
			{
				this.Stop();
			}
			catch { }
			try
			{
				this.OnUnhandledException();
			}
			catch { }
			try
			{
				Log.Exception((Exception)e.ExceptionObject);
				Log.Status("Closing the server.");
			}
			catch { }

			CliUtil.Exit(1, false);
		}

		/// <summary>
		/// Returns length of the new incoming packet, so it can be received.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="ptr"></param>
		/// <returns></returns>
		protected abstract int GetPacketLength(byte[] buffer, int ptr);

		protected abstract void HandleBuffer(TClient client, byte[] buffer);

		/// <summary>
		/// Called when a client got accepted.
		/// </summary>
		/// <param name="client"></param>
		protected virtual void OnClientConnected(TClient client)
		{
		}

		/// <summary>
		/// Called when a connection was closed from the client, or lost.
		/// </summary>
		/// <param name="client"></param>
		protected virtual void OnClientDisconnected(TClient client)
		{
		}

		/// <summary>
		/// Called when shit hits the fan.
		/// </summary>
		/// <remarks>
		/// Only called when threads or events aren't secured properly.
		/// </remarks>
		protected virtual void OnUnhandledException()
		{
			//WorldManager.Instance.EmergencyShutdown();
		}
	}
}
