// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Aura.Data
{
	public abstract class DatabaseCSVBase<TList, TInfo> : Database<TList, TInfo>
		where TInfo : class, new()
		where TList : ICollection, new()
	{
		public override int Load(string path, bool clear)
		{
			if (clear)
				this.Clear();

			this.Warnings.Clear();

			var min = 0;
			var attr = this.GetType().GetMethod("ReadEntry", BindingFlags.NonPublic | BindingFlags.Instance).GetCustomAttributes(typeof(MinFieldCountAttribute), true);
			if (attr.Length > 0)
				min = (attr[0] as MinFieldCountAttribute).Count;

			using (var csv = new CSVReader(path))
			{
				foreach (var entry in csv.Next())
				{
					try
					{
						if (entry.Count < min)
							throw new FieldCountException(min);

						this.ReadEntry(entry);
					}
					catch (DatabaseWarningException ex)
					{
						ex.Line = entry.Line;
						ex.Source = Path.GetFileName(path);
						this.Warnings.Add(ex);
						continue;
					}
					catch (OverflowException)
					{
						this.Warnings.Add(new DatabaseWarningException(Path.GetFileName(path), entry.Line, "Variable not fit for number (#{0}).", entry.Pointer));
						continue;
					}
					//catch (FormatException)
					//{
					//    this.Warnings.Add(new DatabaseWarningException(Path.GetFileName(path), entry.Line, "Number format exception."));
					//    continue;
					//}
				}
			}

			return this.Entries.Count;
		}

		protected abstract void ReadEntry(CSVEntry entry);
	}

	public abstract class DatabaseCSV<TInfo> : DatabaseCSVBase<List<TInfo>, TInfo> where TInfo : class, new()
	{
		public override IEnumerator<TInfo> GetEnumerator()
		{
			foreach (var entry in this.Entries)
				yield return entry;
		}

		public override void Clear()
		{
			this.Entries.Clear();
		}
	}

	public abstract class DatabaseCSVIndexed<TIndex, TInfo> : DatabaseCSVBase<Dictionary<TIndex, TInfo>, TInfo> where TInfo : class, new()
	{
		public override IEnumerator<TInfo> GetEnumerator()
		{
			foreach (var entry in this.Entries.Values)
				yield return entry;
		}

		public TInfo Find(TIndex key)
		{
			return this.Entries.GetValueOrDefault(key);
		}

		public bool Exists(TIndex key)
		{
			return this.Entries.ContainsKey(key);
		}

		public override void Clear()
		{
			this.Entries.Clear();
		}
	}
}
