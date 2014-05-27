// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MsgPack.Serialization;

namespace Aura.Data
{
	public abstract class DatabaseCSVBase<TList, TInfo> : Database<TList, TInfo>
		where TInfo : class, new()
		where TList : ICollection, new()
	{
		private int _min;

		public DatabaseCSVBase()
		{
			var attr = this.GetType().GetMethod("ReadEntry", BindingFlags.NonPublic | BindingFlags.Instance).GetCustomAttributes(typeof(MinFieldCountAttribute), true);
			if (attr.Length > 0)
				_min = (attr[0] as MinFieldCountAttribute).Count;
		}

		public override int Load(string path, bool clear)
		{
			if (clear)
				this.Clear();

			this.Warnings.Clear();

			this.LoadFromFile(path);

			return this.Entries.Count;
		}

		public override int Load(string[] files, string cache, bool clear)
		{
			if (clear)
				this.Clear();

			this.Warnings.Clear();

			var fromFiles = false;
			if (cache == null || !File.Exists(cache))
			{
				fromFiles = true;
			}
			else
			{
				foreach (var file in files)
				{
					if (File.GetLastWriteTime(file) > File.GetLastWriteTime(cache))
					{
						fromFiles = true;
						break;
					}
				}
			}

			if (!fromFiles)
			{
				// deserialize
				//Console.WriteLine("load from cache: " + cache);
				using (var stream = new FileStream(cache, FileMode.OpenOrCreate))
				{
					var serializer = MessagePackSerializer.Create<TList>();
					this.Entries = serializer.Unpack(stream);
				}
			}
			else
			{
				foreach (var path in files.Where(a => File.Exists(a)))
					this.LoadFromFile(path);

				using (var stream = new FileStream(cache, FileMode.OpenOrCreate))
				{
					var serializer = MessagePackSerializer.Create<TList>();
					serializer.Pack(stream, this.Entries);
				}
			}

			return this.Entries.Count;
		}

		protected void LoadFromFile(string path)
		{
			using (var csv = new CSVReader(path))
			{
				foreach (var entry in csv.Next())
				{
					try
					{
						if (entry.Count < _min)
							throw new FieldCountException(_min, entry.Count);

						this.ReadEntry(entry);
					}
					catch (DatabaseWarningException ex)
					{
						ex.Line = entry.Line;
						ex.Source = path.Replace("\\", "/");
						this.Warnings.Add(ex);
						continue;
					}
					catch (OverflowException)
					{
						this.Warnings.Add(new DatabaseWarningException(Path.GetFileName(path), entry.Line, "Variable not fit for number (#{0}).", entry.Pointer));
						continue;
					}
					catch (FormatException)
					{
						this.Warnings.Add(new DatabaseWarningException(Path.GetFileName(path), entry.Line, "Number format exception."));
						continue;
					}
				}
			}
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

		public Type ListType { get { return typeof(List<TInfo>); } }
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
