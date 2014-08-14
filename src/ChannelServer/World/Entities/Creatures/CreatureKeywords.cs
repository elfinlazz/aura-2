// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Channel.Network.Sending;
using Aura.Data;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureKeywords
	{
		private Creature _creature;

		private List<ushort> _list;

		public CreatureKeywords(Creature creature)
		{
			_creature = creature;
			_list = new List<ushort>();
		}

		/// <summary>
		/// Returns new list of all keywords.
		/// </summary>
		/// <returns></returns>
		public ICollection<ushort> GetList()
		{
			lock (_list)
				return _list.ToArray();
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
			lock (_list)
			{
				if (_list.Contains(keywordId))
					return false;

				_list.Add(keywordId);
			}
			return true;
		}

		/// <summary>
		/// Adds keyword.
		/// </summary>
		/// <remarks>
		/// Does not notify client, should be mainly used for loading.
		/// </remarks>
		/// <param name="keyword"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public bool Add(string keyword)
		{
			var data = AuraData.KeywordDb.Find(keyword);
			if (data == null)
			{
				Log.Error("Keywords.Add: Unknown keyword '{0}'.", keyword);
				return false;
			}

			return this.Add(data.Id);
		}

		/// <summary>
		/// Removes keyword and updates client.
		/// </summary>
		/// <param name="keywordId"></param>
		/// <returns></returns>
		public bool Remove(ushort keywordId)
		{
			bool result;

			lock (_list)
				result = _list.Remove(keywordId);

			if (result)
				Send.RemoveKeyword(_creature, keywordId);

			return result;
		}

		/// <summary>
		/// Removes keyword.
		/// </summary>
		/// <param name="keyword"></param>
		/// <returns></returns>
		public bool Remove(string keyword)
		{
			var data = AuraData.KeywordDb.Find(keyword);
			if (data == null)
			{
				Log.Error("Keywords.Remove: Unknown keyword '{0}'.", keyword);
				return false;
			}

			return this.Remove(data.Id);
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
		/// <param name="keyword"></param>
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
			lock (_list)
				return (_list.Contains(keywordId));
		}

		/// <summary>
		/// Returns true if creature has keyword.
		/// </summary>
		/// <param name="keyword"></param>
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
	}
}
