// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureTitles : IEnumerable<KeyValuePair<ushort, TitleState>>
	{
		private Creature _creature;

		private Dictionary<ushort, TitleState> _list;

		public int Count { get { return _list.Count; } }

		public ushort SelectedTitle { get; set; }
		public ushort SelectedOptionTitle { get; set; }
		public DateTime Applied { get; set; }

		public CreatureTitles(Creature creature)
		{
			_creature = creature;
			_list = new Dictionary<ushort, TitleState>();
		}

		/// <summary>
		/// Adds title, returns true if title was added or state
		/// was changed.
		/// </summary>
		/// <param name="titleId"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public bool Add(ushort titleId, TitleState state)
		{
			if (_list.ContainsKey(titleId) && _list[titleId] == state)
				return false;

			_list[titleId] = state;
			return true;
		}

		/// <summary>
		/// Removes title.
		/// </summary>
		/// <param name="titleId"></param>
		/// <returns></returns>
		public bool Remove(ushort titleId)
		{
			return _list.Remove(titleId);
		}

		/// <summary>
		/// Adds title as "Known" and sends xyz.
		/// </summary>
		/// <param name="titleId"></param>
		public void Show(ushort titleId)
		{
			if (this.Add(titleId, TitleState.Known))
				Send.AddTitle(_creature, titleId, TitleState.Known);
		}

		/// <summary>
		/// Adds title as "Available" and sends xyz.
		/// </summary>
		/// <param name="titleId"></param>
		public void Enable(ushort titleId)
		{
			if (this.Add(titleId, TitleState.Usable))
				Send.AddTitle(_creature, titleId, TitleState.Usable);
		}

		/// <summary>
		/// Returns true if creature knows about title in any way.
		/// </summary>
		/// <param name="titleId"></param>
		/// <returns></returns>
		public bool Knows(ushort titleId)
		{
			return (_list.ContainsKey(titleId));
		}

		/// <summary>
		/// Returns true if creature is able to use title.
		/// </summary>
		/// <param name="titleId"></param>
		/// <returns></returns>
		public bool Usable(ushort titleId)
		{
			return (_list.ContainsKey(titleId) && _list[titleId] == TitleState.Usable);
		}

		public IEnumerator<KeyValuePair<ushort, TitleState>> GetEnumerator()
		{
			foreach (var val in _list)
				yield return val;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this.GetEnumerator();
		}
	}

	public enum TitleState : byte { Known = 0, Usable = 1 }
}
