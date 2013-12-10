// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// A skill that is used without preparing it first
	/// </summary>
	public interface IStartable
	{
		void Start(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skills using Start use Stop to end them.
	/// </summary>
	public interface IStoppable
	{
		void Stop(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends prepare when starting to cast it.
	/// </summary>
	public interface IPreparable
	{
		void Prepare(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends ready when done casting.
	/// </summary>
	public interface IReadyable
	{
		void Ready(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends use once the player actually uses it.
	/// </summary>
	public interface IUseable
	{
		void Use(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends complete after it's done using it.
	/// </summary>
	public interface ICompletable
	{
		void Complete(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill is !cancel!able.
	/// </summary>
	public interface ICancelable
	{
		void Cancel(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill using start and stop to activate and deactivate.
	/// </summary>
	public interface IStartStopable : IStartable, IStoppable
	{
	}
}
