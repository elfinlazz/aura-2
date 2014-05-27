// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;

namespace Aura.Data
{
	public interface IDatabase
	{
		/// <summary>
		/// Amount of entries.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Removes all entries.
		/// </summary>
		void Clear();

		/// <summary>
		/// Loads file if it exists, raises exception otherwise.
		/// </summary>
		/// <param name="path">File to load</param>
		/// <param name="clear">Clear database before loading?</param>
		/// <returns></returns>
		int Load(string path, bool clear);

		/// <summary>
		/// Loads multiple files, ignores missing ones.
		/// </summary>
		/// <param name="files">Files to load</param>
		/// <param name="cache">Path to an optional cache file (null for none)</param>
		/// <param name="clear">Clear database before loading?</param>
		/// <returns></returns>
		int Load(string[] files, string cache, bool clear);

		/// <summary>
		/// List of exceptions caught while loading the database.
		/// </summary>
		List<DatabaseWarningException> Warnings { get; }
	}

	public abstract class Database<TList, TInfo> : IDatabase, IEnumerable<TInfo>
		where TInfo : class, new()
		where TList : ICollection, new()
	{
		public TList Entries = new TList();
		protected List<DatabaseWarningException> _warnings = new List<DatabaseWarningException>();

		public List<DatabaseWarningException> Warnings { get { return _warnings; } }

		public int Count { get { return this.Entries.Count; } }

		public abstract void Clear();

		public abstract int Load(string path, bool clear);
		public abstract int Load(string[] files, string cache, bool clear);

		public abstract IEnumerator<TInfo> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}

	public class MinFieldCountAttribute : Attribute
	{
		public int Count { get; protected set; }

		public MinFieldCountAttribute(int count)
		{
			this.Count = count;
		}
	}
}
