// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting.Scripts
{
	public class DungeonScript : IScript
	{
		/// <summary>
		/// Name of the dungeon
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Called when the script is initially created.
		/// </summary>
		/// <returns></returns>
		public bool Init()
		{
			var attr = this.GetType().GetCustomAttribute<DungeonScriptAttribute>();
			if (attr == null)
			{
				Log.Error("DungeonScript.Init: Missing DungeonScript attribute.");
				return false;
			}

			this.Load();

			return true;
		}

		/// <summary>
		/// Called from Init.
		/// </summary>
		public virtual void Load()
		{
		}

		/// <summary>
		/// Called when the dungeon was just created.
		/// </summary>
		public virtual void OnCreation()
		{
		}

		/// <summary>
		/// Called when the boss was killed.
		/// </summary>
		public virtual void OnCleared()
		{
		}
	}

	public class DungeonScriptAttribute : Attribute
	{
		public string Name { get; private set; }

		public DungeonScriptAttribute(string name)
		{
			this.Name = name;
		}
	}
}
