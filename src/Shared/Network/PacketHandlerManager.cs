// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;
using System.Reflection;

namespace Aura.Shared.Network
{
	public abstract class PacketHandlerManager<TClient> where TClient : BaseClient
	{
		public delegate void PacketHandlerFunc(TClient client, MabiPacket packet);

		private Dictionary<int, PacketHandlerFunc> _handlers;

		public PacketHandlerManager()
		{
			_handlers = new Dictionary<int, PacketHandlerFunc>();
		}

		/// <summary>
		/// Adds and/or overwrites handler.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="handler"></param>
		public void Add(int op, PacketHandlerFunc handler)
		{
			if (_handlers.ContainsKey(op))
				Log.Warning("PacketHandlerManager: Overwriting handler for '{0:X4}' with '{1}'.", op, handler.Method.DeclaringType + "." + handler.Method.Name);

			_handlers[op] = handler;
		}

		/// <summary>
		/// Adds all methods with a Handler attribute.
		/// </summary>
		public void AutoLoad()
		{
			foreach (var method in this.GetType().GetMethods())
			{
				foreach (PacketHandler attr in method.GetCustomAttributes(typeof(PacketHandler), false))
				{
					this.Add(attr.Op, (PacketHandlerFunc)Delegate.CreateDelegate(typeof(PacketHandlerFunc), this, method));
				}
			}
		}

		public void Handle(TClient client, MabiPacket packet)
		{
			PacketHandlerFunc handler;
			if (!_handlers.TryGetValue(packet.Op, out handler))
			{
				Log.Unimplemented("PacketHandlerManager: Handler for '{0:X4}'", packet.Op);
				return;
			}

			try
			{
				handler(client, packet);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "There has been a problem while handling '{0:X4}'.", packet.Op);
			}
		}
	}

	public class AsdfHandler : PacketHandlerManager<BaseClient>
	{
		[PacketHandler(123)]
		private void Test(BaseClient client, MabiPacket packet)
		{
			Console.WriteLine("Hi there");
		}

		[PacketHandler(123)]
		private void Test2(BaseClient client, MabiPacket packet)
		{
			Console.WriteLine("Hi there");
		}
	}

	public class PacketHandler : Attribute
	{
		public int Op { get; protected set; }

		public PacketHandler(int op)
		{
			this.Op = op;
		}
	}
}
