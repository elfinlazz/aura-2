// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Aura.Channel.Scripting
{
	public class ScriptVariables
	{
		public dynamic Temp { get; set; }
		public dynamic Perm { get; set; }

		public ScriptVariables()
		{
			this.Temp = new VariableManager();
			this.Perm = new VariableManager();
		}
	}

	/// <summary>
	/// Dynamic variable manager, primarily for scripting.
	/// </summary>
	/// <remarks>
	/// This is a dynamic object, allowing access to fields that may
	/// not exist. When accessing such a field the value for that name
	/// is took from the wrapped dictionary and returned as dynamic.
	/// Being dynamic it can be used without casting, which makes scripting
	/// more comfortable.
	/// If a variable doesn't exist its value is null.
	/// </remarks>
	public class VariableManager : DynamicObject
	{
		private Dictionary<string, object> _variables;

		/// <summary>
		/// Creates new variable manager.
		/// </summary>
		public VariableManager()
		{
			_variables = new Dictionary<string, object>();
		}

		/// <summary>
		/// Creates new variable manager and adds the given values.
		/// </summary>
		public VariableManager(IDictionary<string, object> values)
		{
			_variables = new Dictionary<string, object>(values);
		}

		/// <summary>
		/// Sets given variables.
		/// </summary>
		/// <param name="values"></param>
		public void Load(IDictionary<string, object> values)
		{
			lock (_variables)
			{
				foreach (var value in values)
					_variables[value.Key] = value.Value;
			}
		}

		/// <summary>
		/// Sets the given member to the value.
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			lock (_variables)
				_variables[binder.Name] = value;
			return true;
		}

		/// <summary>
		/// Returns the value for the given member via result, or null if it doesn't exist.
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			lock (_variables)
				if (!_variables.TryGetValue(binder.Name, out result))
					result = null;
			return true;
		}

		/// <summary>
		/// Returns list of all variables as KeyValue collection.
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, object> GetList()
		{
			lock (_variables)
				return new Dictionary<string, object>(_variables);
		}

		/// <summary>
		/// Variable access by string.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public dynamic this[string key]
		{
			get
			{
				object result;
				lock (_variables)
					_variables.TryGetValue(key, out result);
				return result;
			}
			set
			{
				lock (_variables)
					_variables[key] = value;
			}
		}
	}
}
