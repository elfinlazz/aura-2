// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Aura.Shared.Mabi
{
	/// <summary>
	/// "Generic" dictionary wrapper that can hold various var types and
	/// serializes to this format: SOMEINT:4:1234;SOMESTR:s:test;
	/// which is primarily used in items and quests.
	/// </summary>
	public class MabiDictionary
	{
		// Stored as object so we can put anything in
		private Dictionary<string, object> _values = new Dictionary<string, object>();
		private string _cache = null;

		public MabiDictionary()
		{
		}

		public MabiDictionary(string toParse)
		{
			this.Parse(toParse);
		}

		public static MabiDictionary Empty { get { return new MabiDictionary(); } }

		private void Set(string key, object val)
		{
			_values[key] = val;
			_cache = null;
		}

		public void SetByte(string key, byte val) { this.Set(key, val); }
		public void SetShort(string key, short val) { this.Set(key, val); }
		public void SetUShort(string key, ushort val) { this.Set(key, (short)val); }
		public void SetInt(string key, int val) { this.Set(key, val); }
		public void SetUInt(string key, uint val) { this.Set(key, (int)val); }
		public void SetLong(string key, long val) { this.Set(key, val); }
		public void SetULong(string key, ulong val) { this.Set(key, (long)val); }
		public void SetFloat(string key, float val) { this.Set(key, val); }
		public void SetString(string key, string val) { this.Set(key, val); }
		public void SetString(string key, string format, params object[] args) { this.Set(key, string.Format(format, args)); }
		public void SetBool(string key, bool val) { this.Set(key, val); }

		/// <summary>
		/// Returns the value with the given key, or null it wasn't found.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object Get(string key)
		{
			object result;
			_values.TryGetValue(key, out result);
			return result;
		}

		/// <summary>
		/// Returns value for key as T, or its default value if the key
		/// doesn't exist, or is of different type.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private T Get<T>(string key)
		{
			object result;
			_values.TryGetValue(key, out result);
			if (result != null && result is T)
				return (T)result;
			return default(T);
		}

		public byte GetByte(string key) { return this.Get<byte>(key); }
		public short GetShort(string key) { return this.Get<short>(key); }
		public ushort GetUShort(string key) { return (ushort)this.Get<short>(key); }
		public int GetInt(string key) { return this.Get<int>(key); }
		public uint GetUInt(string key) { return (uint)this.Get<int>(key); }
		public long GetLong(string key) { return this.Get<long>(key); }
		public ulong GetULong(string key) { return (ulong)this.Get<long>(key); }
		public float GetFloat(string key) { return this.Get<float>(key); }
		public string GetString(string key) { return this.Get<string>(key); }
		public bool GetBool(string key) { return this.Get<bool>(key); }

		/// <summary>
		/// Removes the value with the given key.
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			_values.Remove(key);
			_cache = null;
		}

		/// <summary>
		/// Removes all values.
		/// </summary>
		public void Clear()
		{
			_values.Clear();
			_cache = null;
		}

		/// <summary>
		/// Returns number of values.
		/// </summary>
		public int Count { get { return _values.Count; } }

		/// <summary>
		/// Returns whether a value exists for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Has(string key)
		{
			return _values.ContainsKey(key);
		}

		/// <summary>
		/// Returns string type identifier for the object.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		private string ValToTypeStr(object val)
		{
			if (val is byte || val is sbyte) return "1";
			else if (val is ushort || val is short) return "2";
			else if (val is uint || val is int) return "4";
			else if (val is ulong || val is long) return "8";
			else if (val is float) return "f";
			else if (val is string) return "s";
			else if (val is bool) return "b";
			else
				throw new Exception("Unsupported type '" + val.GetType().ToString() + "'");
		}

		/// <summary>
		/// Returns dictionary in the format "key:varType:value;...".
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (_values.Count < 1)
				return string.Empty;

			if (_cache != null)
				return _cache;

			var sb = new StringBuilder();

			foreach (var tag in _values)
			{
				var sType = this.ValToTypeStr(tag.Value);

				if (sType == "b")
					sb.AppendFormat("{0}:{1}:{2};", tag.Key, sType, (bool)tag.Value ? "1" : "0");
				else if (sType == "s")
					sb.AppendFormat("{0}:{1}:{2};", tag.Key, sType, ((string)tag.Value).Replace(";", "%S").Replace(":", "%C"));
				else
					sb.AppendFormat("{0}:{1}:{2};", tag.Key, sType, tag.Value);
			}

			return (_cache = sb.ToString());
		}

		/// <summary>
		/// Reads a string in the format "key:varType:value;..." and adds
		/// the values to this tag collection.
		/// </summary>
		/// <param name="str"></param>
		public void Parse(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				return;

			foreach (Match match in Regex.Matches(str, "([^:]+):([^:]+):([^;]*);"))
			{
				var key = match.Groups[1].Value;
				var val = match.Groups[3].Value;

				switch (match.Groups[2].Value)
				{
					case "1": this.Set(key, Convert.ToByte(val)); break;
					case "2": this.Set(key, Convert.ToInt16(val)); break;
					case "4": this.Set(key, Convert.ToInt32(val)); break;
					case "8": this.Set(key, Convert.ToInt64(val)); break;
					case "f": this.Set(key, Convert.ToSingle(val, CultureInfo.InvariantCulture)); break;
					case "s": this.Set(key, val.Replace("%S", ";").Replace("%C", ":")); break;
					case "b": this.Set(key, val == "1"); break;
				}
			}
		}

		/// <summary>
		/// Parses string and tries to return the value.
		/// Returns T's default if the key can't be found or the type is incorrect.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="from"></param>
		/// <returns></returns>
		public static T Fetch<T>(string key, string from)
		{
			var dict = new MabiDictionary(from);
			return dict.Get<T>(key);
		}
	}
}
