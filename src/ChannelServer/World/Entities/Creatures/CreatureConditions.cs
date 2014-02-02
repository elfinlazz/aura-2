// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Network.Sending;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
	/// <summary>
	/// Holds all conditions of a creature.
	/// </summary>
	/// <remarks>
	/// Conditions can have additional options, apperantly sent as a
	/// MabiDictionary, identified by the id of the condition.
	/// Only known example so far: C.Hurry
	/// Hurry uses a short value to specify the speed increase.
	/// This is the only supported value for the moment, until we have
	/// more info on what exactly we need.
	/// </remarks>
	public class CreatureConditions
	{
		private Creature _creature;

		private Dictionary<int, MabiDictionary> _extra;
		private ICollection<KeyValuePair<int, MabiDictionary>> _extraCache;

		public ConditionsA A { get; private set; }
		public ConditionsB B { get; private set; }
		public ConditionsC C { get; private set; }
		public ConditionsD D { get; private set; }
		public ConditionsE E { get; private set; }

		public CreatureConditions(Creature creature)
		{
			_creature = creature;
			_extra = new Dictionary<int, MabiDictionary>();
		}

		public bool Has(ConditionsA condition) { return ((this.A & condition) != 0); }
		public bool Has(ConditionsB condition) { return ((this.B & condition) != 0); }
		public bool Has(ConditionsC condition) { return ((this.C & condition) != 0); }
		public bool Has(ConditionsD condition) { return ((this.D & condition) != 0); }
		public bool Has(ConditionsE condition) { return ((this.E & condition) != 0); }

		public void Activate(ConditionsA condition) { this.A |= condition; Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsB condition) { this.B |= condition; Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsC condition) { this.C |= condition; Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsD condition) { this.D |= condition; Send.ConditionUpdate(_creature); }
		public void Activate(ConditionsE condition) { this.E |= condition; Send.ConditionUpdate(_creature); }

		public void Activate(ConditionsC condition, short val)
		{
			this.C |= condition;

			var id = (int)Math.Log((double)condition, 2) + 64 * 2;
			lock (_extra)
			{
				if (!_extra.ContainsKey(id))
					_extra[id] = new MabiDictionary();
				_extra[id].SetShort("VAL", val);
			}

			_extraCache = null;

			Send.ConditionUpdate(_creature);
		}

		public void Deactivate(ConditionsA condition) { this.A &= ~condition; Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsB condition) { this.B &= ~condition; Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsC condition)
		{
			this.C &= ~condition;

			var id = (int)Math.Log((double)condition, 2) + 64 * 2;
			lock (_extra)
			{
				if (_extra.ContainsKey(id))
					_extra.Remove(id);
			}

			_extraCache = null;

			Send.ConditionUpdate(_creature);
		}
		public void Deactivate(ConditionsD condition) { this.D &= ~condition; Send.ConditionUpdate(_creature); }
		public void Deactivate(ConditionsE condition) { this.E &= ~condition; Send.ConditionUpdate(_creature); }

		/// <summary>
		/// Resets all conditions and sends update.
		/// </summary>
		public void Clear()
		{
			this.A = 0;
			this.B = 0;
			this.C = 0;
			this.D = 0;
			this.E = 0;
			lock (_extra)
				_extra.Clear();
			_extraCache = null;

			Send.ConditionUpdate(_creature);
		}

		/// <summary>
		/// Returns new list of all extra values.
		/// </summary>
		/// <returns></returns>
		public ICollection<KeyValuePair<int, MabiDictionary>> GetExtraList()
		{
			if (_extraCache != null)
				return _extraCache;

			lock (_extra)
				return (_extraCache = _extra.ToArray());
		}

		/// <summary>
		/// Returns extra val for id, or 0.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public short GetExtraVal(int id)
		{
			lock (_extra)
			{
				if (!_extra.ContainsKey(id))
					return 0;
				return _extra[id].GetShort("VAL");
			}
		}

		public override string ToString()
		{
			return ("(" + this.A + " ; " + this.B + " ; " + this.C + " ; " + this.D + ")");
		}
	}
}
