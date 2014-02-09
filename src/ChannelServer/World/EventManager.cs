// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi;
using Aura.Channel.World.Entities;
using Aura.Channel.Skills;

namespace Aura.Channel.World
{
	public class EventManager
	{
		/// <summary>
		/// Raised every second in real time.
		/// </summary>
		public event Action<ErinnTime> SecondsTimeTick;
		public void OnSecondsTimeTick(ErinnTime now) { SecondsTimeTick.Raise(now); }

		/// <summary>
		/// Raised every minute in real time.
		/// </summary>
		public event Action<ErinnTime> MinutesTimeTick;
		public void OnMinutesTimeTick(ErinnTime now) { MinutesTimeTick.Raise(now); }

		/// <summary>
		/// Raised every hour in real time.
		/// </summary>
		public event Action<ErinnTime> HoursTimeTick;
		public void OnHoursTimeTick(ErinnTime now) { HoursTimeTick.Raise(now); }

		/// <summary>
		/// Raised every 1.5s (1min Erinn time).
		/// </summary>
		public event Action<ErinnTime> ErinnTimeTick;
		public void OnErinnTimeTick(ErinnTime now) { ErinnTimeTick.Raise(now); }

		/// <summary>
		/// Raised every 18min (1/2 day Erinn time).
		/// </summary>
		public event Action<ErinnTime> ErinnDaytimeTick;
		public void OnErinnDaytimeTick(ErinnTime now) { ErinnDaytimeTick.Raise(now); }

		/// <summary>
		/// Raised at 00:00am Erinn time.
		/// </summary>
		public event Action<ErinnTime> ErinnMidnightTick;
		public void OnErinnMidnightTick(ErinnTime now) { ErinnMidnightTick.Raise(now); }

		/// <summary>
		/// Raised every 5 minutes in real time.
		/// </summary>
		public event Action<ErinnTime> MabiTick;
		public void OnMabiTick(ErinnTime now) { MabiTick.Raise(now); }

		// ------------------------------------------------------------------

		/// <summary>
		/// Raised a few seconds after player logged in.
		/// </summary>
		public event Action<Creature> PlayerLoggedIn;
		public void OnPlayerLoggedIn(Creature creature) { PlayerLoggedIn.Raise(creature); }
	}

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
		public static void Raise<T>(this Action<T> handler, T args)
		{
			if (handler != null)
				handler(args);
		}
	}
}
