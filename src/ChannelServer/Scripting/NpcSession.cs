// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Aura.Channel.Util;
using Aura.Shared.Util;
using Aura.Channel.World.Entities;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;

namespace Aura.Channel.Scripting
{
	/// <summary>
	/// NPC converstation session
	/// </summary>
	public class NpcSession
	{
		public NPC Target { get; private set; }
		public int Id { get; private set; }

		public NpcScript Script { get; set; }
		public Response Response { get; set; }

		public NpcSession()
		{
			// We'll only set this once for every char, for the entire session.
			// In some cases the client doesn't seem to take the new id,
			// which results in a mismatch.
			this.Id = RandomProvider.Get().Next(1, 5000);
		}

		/// <summary>
		/// Starts session
		/// </summary>
		/// <param name="target"></param>
		/// <param name="creature"></param>
		public void Start(NPC target, Creature creature)
		{
			this.Target = target;

			if (target.AI == null)
				return;

			var script = Activator.CreateInstance(target.Script.GetType()) as NpcScript;
			script.NPC = target.Script.NPC;
			script.Player = creature;
			this.Script = script;

			this.Script.TalkAsync();
		}

		/// <summary>
		/// Cancels script and resets session.
		/// </summary>
		public void Clear()
		{
			this.Script.Cancel();
			this.Script = null;
			this.Target = null;
			this.Response = null;
		}

		/// <summary>
		/// Returns true if there is a state and target's id is npcId.
		/// </summary>
		public bool IsValid(long npcId)
		{
			return (this.IsValid() && this.Target.EntityId == npcId);
		}

		/// <summary>
		/// Returns true if there is a state and a target.
		/// </summary>
		public bool IsValid()
		{
			return (this.Target != null && this.Script != null);
		}

		/// <summary>
		/// Checks <see cref="IsValid(long)"/>. If false, throws <see cref="ModerateViolation"/>.
		/// </summary>
		public void EnsureValid(long npcId)
		{
			if (!this.IsValid(npcId))
				throw new ModerateViolation("Invalid NPC session");
		}

		/// <summary>
		/// Checks <see cref="IsValid()"/>. If false, throws <see cref="ModerateViolation"/>.
		/// </summary>
		public void EnsureValid()
		{
			if (!this.IsValid())
				throw new ModerateViolation("Invalid NPC session");
		}
	}

	/// <summary>
	/// Response to a conversation
	/// </summary>
	/// <remarks>
	/// An instance of this class is returned from the NPCs on Select
	/// to give the client something referenceable to write the response to.
	/// (Options, Input, etc.)
	/// </remarks>
	public class Response
	{
		public string Value { get; set; }
	}
}
