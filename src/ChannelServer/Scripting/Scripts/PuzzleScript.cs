// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Data;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Puzzle script used in dungeons.
	/// </summary>
	/// <example>
	/// Getting a script: ChannelServer.Instance.ScriptManager.PuzzleScripts.Get("entrance_puzzle");
	/// </example>
	public class PuzzleScript : IScript
	{
		/// <summary>
		/// Name of the puzzle
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Called when the script is initially created.
		/// </summary>
		/// <returns></returns>
		public bool Init()
		{
			var attr = this.GetType().GetCustomAttribute<PuzzleScriptAttribute>();
			if (attr == null)
			{
				Log.Error("PuzzleScript.Init: Missing PuzzleScript attribute.");
				return false;
			}

			this.Name = attr.Name;

			ChannelServer.Instance.ScriptManager.PuzzleScripts.Add(this.Name, this);

			return true;
		}

		/// <summary>
		/// Returns random number between 0.0 and 100.0.
		/// </summary>
		/// <returns></returns>
		protected double Random()
		{
			var rnd = RandomProvider.Get();
			return (100 * rnd.NextDouble());
		}

		/// <summary>
		/// Returns random number between 0 and max-1.
		/// </summary>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int max)
		{
			var rnd = RandomProvider.Get();
			return rnd.Next(max);
		}

		/// <summary>
		/// Returns random number between min and max-1.
		/// </summary>
		/// <param name="min">Inclusive lower bound</param>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int min, int max)
		{
			var rnd = RandomProvider.Get();
			return rnd.Next(min, max);
		}

		/// <summary>
		/// Returns the specified amount of random values from the parameters.
		/// The returned values contain every parameter only once.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="amount"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		protected T[] UniqueRnd<T>(int amount, params T[] values)
		{
			if (values == null || values.Length == 0 || values.Length < amount)
				throw new ArgumentException("Values might not be null, empty, or be smaller than amount.");

			var rnd = RandomProvider.Get();
			return values.OrderBy(a => rnd.Next()).Take(amount).ToArray();
		}

		/// <summary>
		/// Returns true if feature is enabled.
		/// </summary>
		/// <remarks>
		/// TODO: Make another more general script base class for this and Random?
		/// </remarks>
		/// <param name="featureName"></param>
		/// <returns></returns>
		protected bool IsEnabled(string featureName)
		{
			return AuraData.FeaturesDb.IsEnabled(featureName);
		}

		public virtual void OnPrepare(Puzzle puzzle)
		{
		}

		public virtual void OnPuzzleCreate(Puzzle puzzle)
		{
		}

		public virtual void OnPropEvent(Puzzle puzzle, Prop prop)
		{
		}

		public virtual void OnMobAllocated(Puzzle puzzle, MonsterGroup group)
		{
		}

		public virtual void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
		{
		}
	}

	public class PuzzleScriptAttribute : Attribute
	{
		public string Name { get; private set; }

		public PuzzleScriptAttribute(string name)
		{
			this.Name = name.ToLower();
		}
	}
}
