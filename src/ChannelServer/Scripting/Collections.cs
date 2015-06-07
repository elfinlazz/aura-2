// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting
{
	/// <summary>
	/// Collection of item scripts.
	/// </summary>
	/// <remarks>
	/// Key: Item id
	/// Value: Instance of ItemScript
	/// </remarks>
	public class ItemScriptCollection : Collection<int, ItemScript>
	{
	}

	/// <summary>
	/// Collection of AI scripts.
	/// </summary>
	/// <remarks>
	/// Key: Name of AI
	/// Value: Script class' type, to create a new instance on demand
	/// </remarks>
	public class AiScriptCollection : Collection<string, Type>
	{
		/// <summary>
		/// Creates a new instance of AI with given name and attaches
		/// creature to it. Returns the newly created AI, or null if an AI
		/// with the given name doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="creature"></param>
		/// <returns></returns>
		public AiScript CreateAi(string name, Creature creature)
		{
			var type = this.Get(name);
			if (type == null)
				return null;

			var script = Activator.CreateInstance(type) as AiScript;
			script.Attach(creature);

			return script;
		}
	}

	/// <summary>
	/// Collection of NPC shop scripts.
	/// </summary>
	/// <remarks>
	/// Key: Type name of the script class
	/// Value: Instance of NpcSchopScript
	/// </remarks>
	public class NpcShopScriptCollection : Collection<string, NpcShopScript>
	{
		/// <summary>
		/// Adds shop to collection, uses the shop's type name as index.
		/// Overrides existing entries.
		/// </summary>
		/// <param name="script"></param>
		public void AddOrReplace(NpcShopScript script)
		{
			lock (_entries)
				_entries[script.GetType().Name] = script;
		}
	}

	/// <summary>
	/// Collection of NPC shop scripts.
	/// </summary>
	/// <remarks>
	/// Key: Quest id
	/// Value: Instance of QuestScript
	/// </remarks>
	public class QuestScriptCollection : Collection<int, QuestScript>
	{
	}

	/// <summary>
	/// Collection of dungeon scripts.
	/// </summary>
	/// <remarks>
	/// Key: Dungeon name
	/// Value: Instance of DungeonScript
	/// </remarks>
	public class DungeonScriptCollection : Collection<string, DungeonScript>
	{
	}

	/// <summary>
	/// Collection of puzzle scripts.
	/// </summary>
	/// <remarks>
	/// Key: Puzzle name
	/// Value: Instance of PuzzleScript
	/// </remarks>
	public class PuzzleScriptCollection : Collection<string, PuzzleScript>
	{
	}

	/// <summary>
	/// Collection of hook lists.
	/// </summary>
	public class NpcScriptHookCollection : ListCollection<string, NpcScriptHook>
	{
		/// <summary>
		/// Adds hook.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hookName"></param>
		/// <param name="func"></param>
		public void Add(string npcName, string hookName, NpcScriptHook func)
		{
			this.Add(npcName + "->" + hookName, func);
		}

		/// <summary>
		/// Returns list of hooks, or null if there are none.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hookName"></param>
		/// <returns></returns>
		public List<NpcScriptHook> Get(string npcName, string hookName)
		{
			return this.Get(npcName + "->" + hookName);
		}
	}
}
