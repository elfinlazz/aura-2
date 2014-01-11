// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Mabi;

namespace Aura.Channel.Util
{
	public delegate void TimeEventHandler(ErinnTime time);

	public static class EventHandlerExtensions
	{
		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
		{
			if (handler != null)
				handler(sender, args);
		}

		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise(this TimeEventHandler handler, ErinnTime args)
		{
			if (handler != null)
				handler(args);
		}
	}
}
