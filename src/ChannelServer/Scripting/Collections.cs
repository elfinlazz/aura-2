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
}
