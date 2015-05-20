// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Shared.Util
{
	/// <summary>
	/// Thread-safe wrapper around a generic dictionary.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class Collection<TKey, TValue>
	{
		protected Dictionary<TKey, TValue> _entries;

		/// <summary>
		/// Creates new collection.
		/// </summary>
		public Collection()
		{
			_entries = new Dictionary<TKey, TValue>();
		}

		/// <summary>
		/// Adds value to collection, returns false if key existed already.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Add(TKey key, TValue value)
		{
			lock (_entries)
			{
				if (_entries.ContainsKey(key))
					return false;

				_entries.Add(key, value);
				return true;
			}
		}

		/// <summary>
		/// Adds value to collection, overrides existing keys.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void Set(TKey key, TValue value)
		{
			lock (_entries)
				_entries[key] = value;
		}

		/// <summary>
		/// Removes value with key from collection, returns true if successful.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(TKey key)
		{
			lock (_entries)
				return _entries.Remove(key);
		}

		/// <summary>
		/// Clears collection
		/// </summary>
		public void Clear()
		{
			lock (_entries)
				_entries.Clear();
		}

		/// <summary>
		/// Returns true if collection contains a value for key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(TKey key)
		{
			lock (_entries)
				return _entries.ContainsKey(key);
		}

		/// <summary>
		/// Returns true if collection contains the value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool ContainsValue(TValue value)
		{
			lock (_entries)
				return _entries.ContainsValue(value);
		}

		/// <summary>
		/// Returns value for key or default for value (e.g. null for string).
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue Get(TKey key)
		{
			TValue result;

			lock (_entries)
				_entries.TryGetValue(key, out result);

			return result;
		}

		/// <summary>
		/// Returns list of values that match the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<TKey, TValue>> Get(Func<KeyValuePair<TKey, TValue>, bool> predicate)
		{
			lock (_entries)
				return _entries.Where(predicate);
		}
	}

	/// <summary>
	/// Thread-safe wrapper around a generic dictionary for indexed lists.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue">Type of the list.</typeparam>
	public class ListCollection<TKey, TValue>
	{
		protected Dictionary<TKey, List<TValue>> _entries;

		/// <summary>
		/// Creates new collection.
		/// </summary>
		public ListCollection()
		{
			_entries = new Dictionary<TKey, List<TValue>>();
		}

		/// <summary>
		/// Adds value to key's list.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void Add(TKey key, TValue value)
		{
			lock (_entries)
			{
				List<TValue> list;
				_entries.TryGetValue(key, out list);

				if (list == null)
				{
					list = new List<TValue>();
					_entries.Add(key, list);
				}

				list.Add(value);
			}
		}

		/// <summary>
		/// Clears collection.
		/// </summary>
		public void Clear()
		{
			lock (_entries)
				_entries.Clear();
		}

		/// <summary>
		/// Clears list in collection.
		/// </summary>
		public void Clear(TKey key)
		{
			lock (_entries)
				if (_entries.ContainsKey(key))
					_entries[key].Clear();
		}

		/// <summary>
		/// Returns true if collection contains a value for key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(TKey key)
		{
			lock (_entries)
				return _entries.ContainsKey(key);
		}

		/// <summary>
		/// Returns value for key or default for value (e.g. null for string).
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public List<TValue> Get(TKey key)
		{
			List<TValue> result;

			lock (_entries)
				_entries.TryGetValue(key, out result);

			return result;
		}
	}
}
