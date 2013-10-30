// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System.Collections.Generic;
using System.Collections;

namespace Aura.Data
{
	public interface IDatabase
	{
		int Count { get; }
		void Clear();
		int Load(string path, bool clear);
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

		public abstract IEnumerator<TInfo> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
