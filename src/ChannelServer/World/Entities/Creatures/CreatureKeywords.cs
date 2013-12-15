// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections;
using System.Collections.Generic;
using Aura.Channel.Network.Sending;
using Aura.Data;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureKeywords : IEnumerable<ushort>
	{
		private Creature _creature;

		private List<ushort> _list;

		public int Count { get { return _list.Count; } }

		public CreatureKeywords(Creature creature)
		{
			_creature = creature;
			_list = new List<ushort>();
		}

		/// <summary>
		/// Adds keyword.
		/// </summary>
		/// <remarks>
		/// Does not notify client, should be mainly used for loading.
		/// </remarks>
		/// <param name="keywordId"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public bool Add(ushort keywordId)
		{
			if (_list.Contains(keywordId))
				return false;

			_list.Add(keywordId);
			return true;
		}

		/// <summary>
		/// Removes keyword.
		/// </summary>
		/// <param name="keywordId"></param>
		/// <returns></returns>
		public bool Remove(ushort keywordId)
		{
			return _list.Remove(keywordId);
		}

		/// <summary>
		/// Adds keyword and sends NewKeyword.
		/// </summary>
		/// <param name="keywordId"></param>
		private bool Give(ushort keywordId)
		{
			var data = AuraData.KeywordDb.Find(keywordId);
			if (data == null)
			{
				Log.Error("Keywords.Give: Unknown keyword '{0}'.", keywordId);
				return false;
			}

			if (!this.Add(keywordId))
				return false;

			Send.AddKeyword(_creature, keywordId);
			return true;
		}

		/// <summary>
		/// Adds keyword and sends NewKeyword.
		/// </summary>
		/// <param name="keywordId"></param>
		public bool Give(string keyword)
		{
			var data = AuraData.KeywordDb.Find(keyword);
			if (data == null)
			{
				Log.Error("Keywords.Give: Unknown keyword '{0}'.", keyword);
				return false;
			}

			return this.Give(data.Id);
		}

		/// <summary>
		/// Returns true if creature has keyword.
		/// </summary>
		/// <param name="keywordId"></param>
		/// <returns></returns>
		private bool Has(ushort keywordId)
		{
			return (_list.Contains(keywordId));
		}

		/// <summary>
		/// Returns true if creature has keyword.
		/// </summary>
		/// <param name="keywordId"></param>
		/// <returns></returns>
		public bool Has(string keyword)
		{
			var data = AuraData.KeywordDb.Find(keyword);
			if (data == null)
			{
				Log.Warning("Keywords.Has: Unknown keyword '{0}'.", keyword);
				return false;
			}

			return this.Has(data.Id);
		}

		public IEnumerator<ushort> GetEnumerator()
		{
			foreach (var val in _list)
				yield return val;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this.GetEnumerator();
		}
	}
}
