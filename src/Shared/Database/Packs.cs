// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Aura.Shared.Database
{
	/// <summary>
	/// Helper class to add parameters to update command.
	/// </summary>
	/// <remarks>
	/// The field in the Set method is the name of the column, unlike in the
	/// Parameters.AddWithValue method, where you have to prefix it with "@",
	/// this is done automatically here.
	/// </remarks>
	/// <example><code>
	/// var up = new UpdatePack();
	/// up.Set("race", creature.Race);
	/// up.Set("status", (uint)creature.State);
	/// // ...
	/// 
	/// var mc = new MySqlCommand("UPDATE `creatures` SET " + up.ToString() + " WHERE `creatureId` = @creatureId", conn);
	/// 
	/// mc.Parameters.AddWithValue("@creatureId", creature.CreatureId);
	/// up.AddToCommand(mc);
	/// </code></example>
	public class UpdatePack : IParameterCollection
	{
		private Dictionary<string, object> _set;

		public Dictionary<string, object> Parameters { get { return _set; } }

		public UpdatePack()
		{
			_set = new Dictionary<string, object>();
		}

		/// <summary>
		/// Adds value for column.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public UpdatePack Set(string field, object value)
		{
			_set[field] = value;
			return this;
		}

		/// <summary>
		/// Returns query ready list of parameters.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var sb = new StringBuilder();

			foreach (var parameter in _set.Keys)
				sb.AppendFormat("`{0}` = @{0}, ", parameter);

			return sb.ToString().Trim(' ', ',');
		}
	}

	public interface IParameterCollection
	{
		Dictionary<string, object> Parameters { get; }
	}

	public static class MySqlParameterCollectionExt
	{
		public static void AddCollection(this MySqlParameterCollection parameters, IParameterCollection col)
		{
			foreach (var parameter in col.Parameters)
				parameters.AddWithValue("@" + parameter.Key, parameter.Value);
		}
	}
}
