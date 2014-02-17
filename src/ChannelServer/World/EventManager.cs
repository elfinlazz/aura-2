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
		public event Action<PlayerCreature> PlayerLoggedIn;
		public void OnPlayerLoggedIn(PlayerCreature creature) { PlayerLoggedIn.Raise(creature); }

		/// <summary>
		/// Raised when a player disconnects from server.
		/// </summary>
		public event Action<PlayerCreature> PlayerDisconnect;
		public void OnPlayerDisconnect(PlayerCreature creature) { PlayerDisconnect.Raise(creature); }

		/// <summary>
		/// Raised when player enters a region.
		/// </summary>
		public event Action<PlayerCreature> PlayerEntersRegion;
		public void OnPlayerEntersRegion(PlayerCreature creature) { PlayerEntersRegion.Raise(creature); }

		/// <summary>
		/// Raised when player leaves a region.
		/// </summary>
		public event Action<PlayerCreature> PlayerLeavesRegion;
		public void OnPlayerLeavesRegion(PlayerCreature creature) { PlayerLeavesRegion.Raise(creature); }

		/// <summary>
		/// Raised when player drops, destroys, sells,
		/// uses (decrements), etcs an item.
		/// </summary>
		public event Action<PlayerCreature, int, int> PlayerRemovesItem;
		public void OnPlayerRemovesItem(PlayerCreature creature, int itemId, int amount) { PlayerRemovesItem.Raise(creature, itemId, amount); }

		/// <summary>
		/// Raised when player receives an item in any way.
		/// </summary>
		public event Action<PlayerCreature, int, int> PlayerReceivesItem;
		public void OnPlayerReceivesItem(PlayerCreature creature, int itemId, int amount) { PlayerReceivesItem.Raise(creature, itemId, amount); }

		/// <summary>
		/// Raised when player completes a quest.
		/// </summary>
		public event Action<PlayerCreature, int> PlayerCompletesQuest;
		public void OnPlayerCompletesQuest(PlayerCreature creature, int questId) { PlayerCompletesQuest.Raise(creature, questId); }

		// ------------------------------------------------------------------

		/// <summary>
		/// Raised when a creature is killed by something.
		/// </summary>
		public event Action<Creature, Creature> CreatureKilled;
		public void OnCreatureKilled(Creature creature, Creature killer) { CreatureKilled.Raise(creature, killer); }

		/// <summary>
		/// Raised when a creature is killed by something.
		/// </summary>
		public event Action<Creature, PlayerCreature> CreatureKilledByPlayer;
		public void OnCreatureKilledByPlayer(Creature creature, PlayerCreature killer) { CreatureKilledByPlayer.Raise(creature, killer); }
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

		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise<T1, T2>(this Action<T1, T2> handler, T1 args1, T2 args2)
		{
			if (handler != null)
				handler(args1, args2);
		}

		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise<T1, T2, T3>(this Action<T1, T2, T3> handler, T1 args1, T2 args2, T3 args3)
		{
			if (handler != null)
				handler(args1, args2, args3);
		}
	}
}
