// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aura.Data
{
	internal static class DictionaryExtensions
	{
		internal static TVal GetValueOrDefault<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key) where TVal : class
		{
			TVal result = null;
			dict.TryGetValue(key, out result);
			return result;
		}

		internal static List<TVal> FindAll<TKey, TVal>(this Dictionary<TKey, TVal> dict, Func<KeyValuePair<TKey, TVal>, bool> predicate) where TVal : class
		{
			var result = new List<TVal>();

			foreach (var keyval in dict)
			{
				if (predicate(keyval))
					result.Add(keyval.Value);
			}

			return result;
		}

		internal static byte ReadByte(this JObject obj, string key, byte def = 0) { return (byte)(obj[key] ?? def); }
		internal static sbyte ReadSByte(this JObject obj, string key, sbyte def = 0) { return (sbyte)(obj[key] ?? def); }
		internal static short ReadShort(this JObject obj, string key, short def = 0) { return (short)(obj[key] ?? def); }
		internal static ushort ReadUShort(this JObject obj, string key, ushort def = 0) { return (ushort)(obj[key] ?? def); }
		internal static int ReadInt(this JObject obj, string key, int def = 0) { return (int)(obj[key] ?? def); }
		internal static uint ReadUInt(this JObject obj, string key, uint def = 0) { return (uint)(obj[key] ?? def); }
		internal static float ReadFloat(this JObject obj, string key, float def = 0) { return (float)(obj[key] ?? def); }
		internal static double ReadDouble(this JObject obj, string key, double def = 0) { return (double)(obj[key] ?? def); }
		internal static string ReadString(this JObject obj, string key, string def = "") { return (string)(obj[key] ?? def); }
	}
}
