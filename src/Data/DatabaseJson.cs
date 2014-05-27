// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MsgPack.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aura.Data
{
	/// <summary>
	/// Base class for JSON databases
	/// </summary>
	/// <typeparam name="TList"></typeparam>
	/// <typeparam name="TInfo"></typeparam>
	public abstract class DatabaseJsonBase<TList, TInfo> : DatabaseBase<TList, TInfo>
		where TInfo : class, new()
		where TList : ICollection, new()
	{
		protected override void LoadFromFile(string path)
		{
			using (var fs = new StreamReader(path))
			using (var reader = new JsonTextReader(fs))
			{
				try
				{
					var array = JArray.Load(reader);

					foreach (var entry in array)
					{
						var obj = entry as JObject;
						if (obj == null) continue;

						try
						{
							this.ReadEntry(obj);
						}
						catch (DatabaseWarningException ex)
						{
							this.Warnings.Add(new DatabaseWarningException(ex.Message + ", in " + Environment.NewLine + obj.ToString(), path));
							continue;
						}
						catch (OverflowException)
						{
							this.Warnings.Add(new DatabaseWarningException("Number to big or too small for variable, in " + Environment.NewLine + obj.ToString(), path));
							continue;
						}
					}
				}
				catch (JsonReaderException ex)
				{
					//this.Warnings.Add(new DatabaseWarningException(ex.Message, path));
					//return;
					throw new DatabaseWarningException(ex.Message, path);
				}
			}
		}

		protected abstract void ReadEntry(JObject entry);
	}

	/// <summary>
	/// JSON database holding a data list
	/// </summary>
	/// <typeparam name="TInfo">Data type</typeparam>
	public abstract class DatabaseJson<TInfo> : DatabaseJsonBase<List<TInfo>, TInfo> where TInfo : class, new()
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

	/// <summary>
	/// JSON database holding a data dictionary
	/// </summary>
	/// <typeparam name="TIndex">Type of the dictionary key</typeparam>
	/// <typeparam name="TInfo">Data type</typeparam>
	public abstract class DatabaseJsonIndexed<TIndex, TInfo> : DatabaseJsonBase<Dictionary<TIndex, TInfo>, TInfo> where TInfo : class, new()
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
