// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System;

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
	//public class UpdatePack : IParameterCollection
	//{
	//    private Dictionary<string, object> _set;

	//    public Dictionary<string, object> Parameters { get { return _set; } }

	//    public UpdatePack()
	//    {
	//        _set = new Dictionary<string, object>();
	//    }

	//    /// <summary>
	//    /// Adds value for column.
	//    /// </summary>
	//    /// <param name="field"></param>
	//    /// <param name="value"></param>
	//    /// <returns></returns>
	//    public UpdatePack Set(string field, object value)
	//    {
	//        _set[field] = value;
	//        return this;
	//    }

	//    /// <summary>
	//    /// Returns query ready list of parameters.
	//    /// </summary>
	//    /// <returns></returns>
	//    public override string ToString()
	//    {
	//        var sb = new StringBuilder();

	//        foreach (var parameter in _set.Keys)
	//            sb.AppendFormat("`{0}` = @{0}, ", parameter);

	//        return sb.ToString().Trim(' ', ',');
	//    }
	//}

	//public interface IParameterCollection
	//{
	//    Dictionary<string, object> Parameters { get; }
	//}

	//public static class MySqlParameterCollectionExt
	//{
	//    public static void AddCollection(this MySqlParameterCollection parameters, IParameterCollection col)
	//    {
	//        foreach (var parameter in col.Parameters)
	//            parameters.AddWithValue("@" + parameter.Key, parameter.Value);
	//    }
	//}

	public class UpdateCommand : IDisposable
	{
		private MySqlCommand _mc;
		private Dictionary<string, object> _set;

		public UpdateCommand(string command, MySqlConnection conn, MySqlTransaction trans = null)
		{
			_mc = new MySqlCommand(command, conn, trans);
			_set = new Dictionary<string, object>();
		}

		/// <summary>
		/// Adds a parameter that's not handled by Set.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void AddParameter(string name, object value)
		{
			_mc.Parameters.AddWithValue(name, value);
		}

		/// <summary>
		/// Sets value for field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		public void Set(string field, object value)
		{
			_set[field] = value;
		}

		/// <summary>
		/// Runs MySqlCommand.ExecuteNonQuery
		/// </summary>
		/// <returns></returns>
		public int Execute()
		{
			// Build setting part
			var sb = new StringBuilder();
			foreach (var parameter in _set.Keys)
				sb.AppendFormat("`{0}` = @{0}, ", parameter);

			// Add setting part
			_mc.CommandText = string.Format(_mc.CommandText, sb.ToString().Trim(' ', ','));

			// Add parameters
			foreach (var parameter in _set)
				_mc.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);

			// Run
			return _mc.ExecuteNonQuery();
		}

		public void Dispose()
		{
			_mc.Dispose();
		}
	}

	public class InsertCommand : IDisposable
	{
		private MySqlCommand _mc;
		private Dictionary<string, object> _set;

		public InsertCommand(string command, MySqlConnection conn, MySqlTransaction transaction = null)
		{
			_mc = new MySqlCommand(command, conn, transaction);
			_set = new Dictionary<string, object>();
		}

		/// <summary>
		/// Adds a parameter that's not handled by Set.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void AddParameter(string name, object value)
		{
			_mc.Parameters.AddWithValue(name, value);
		}

		/// <summary>
		/// Sets value for field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		public void Set(string field, object value)
		{
			_set[field] = value;
		}

		/// <summary>
		/// Runs MySqlCommand.ExecuteNonQuery
		/// </summary>
		/// <returns></returns>
		public int Execute()
		{
			// Build values part
			var sb1 = new StringBuilder();
			var sb2 = new StringBuilder();
			foreach (var parameter in _set.Keys)
			{
				sb1.AppendFormat("`{0}`, ", parameter);
				sb2.AppendFormat("@{0}, ", parameter);
			}

			// Add values part
			var values = "(" + (sb1.ToString().Trim(' ', ',')) + ") VALUES (" + (sb2.ToString().Trim(' ', ',')) + ")";
			_mc.CommandText = string.Format(_mc.CommandText, values);

			// Add parameters
			foreach (var parameter in _set)
				_mc.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);

			// Run
			return _mc.ExecuteNonQuery();
		}

		public void Dispose()
		{
			_mc.Dispose();
		}
	}
}
