// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// Skill handler
	/// </summary>
	public interface ISkillHandler
	{
	}

	/// <summary>
	/// A skill that is used without preparing it first
	/// </summary>
	public interface IStartable : ISkillHandler
	{
		void Start(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skills using Start use Stop to end them.
	/// </summary>
	public interface IStoppable : ISkillHandler
	{
		void Stop(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends prepare when starting to cast it.
	/// </summary>
	public interface IPreparable : ISkillHandler
	{
		bool Prepare(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends ready when done casting.
	/// </summary>
	public interface IReadyable : ISkillHandler
	{
		bool Ready(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends use once the player actually uses it.
	/// </summary>
	public interface IUseable : ISkillHandler
	{
		void Use(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill sends complete after it's done using it.
	/// </summary>
	public interface ICompletable : ISkillHandler
	{
		void Complete(Creature creature, Skill skill, Packet packet);
	}

	/// <summary>
	/// Skill is !cancel!able.
	/// </summary>
	public interface ICancelable : ISkillHandler
	{
		void Cancel(Creature creature, Skill skill);
	}

	/// <summary>
	/// Skill using start and stop to activate and deactivate.
	/// </summary>
	public interface IStartStoppable : IStartable, IStoppable
	{
	}

	/// <summary>
	/// Handler with an Init method to subscribe to events.
	/// </summary>
	public interface IInitiableSkillHandler
	{
		void Init();
	}
}
