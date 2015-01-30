// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Mabi;
using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;

namespace Aura.Shared.Database
{
	public class AuraDb
	{
		private string _connectionString;

		private Regex _nameCheckRegex = new Regex(@"^[a-zA-Z][a-z0-9]{2,15}$", RegexOptions.Compiled);

		/// <summary>
		/// Returns a valid connection.
		/// </summary>
		public MySqlConnection Connection
		{
			get
			{
				if (_connectionString == null)
					throw new Exception("AuraDb has not been initialized.");

				var result = new MySqlConnection(_connectionString);
				result.Open();
				return result;
			}
		}

		/// <summary>
		/// Sets connection string and calls TestConnection.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="user"></param>
		/// <param name="pass"></param>
		/// <param name="db"></param>
		public void Init(string host, string user, string pass, string db)
		{
			_connectionString = string.Format("server={0}; database={1}; uid={2}; password={3}; pooling=true; min pool size=0; max pool size=100;", host, db, user, pass);
			this.TestConnection();
		}

		/// <summary>
		/// Tests connection, throws on error.
		/// </summary>
		public void TestConnection()
		{
			MySqlConnection conn = null;
			try
			{
				conn = this.Connection;
			}
			finally
			{
				if (conn != null)
					conn.Close();
			}
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Returns whether the account exists.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public bool AccountExists(string accountId)
		{
			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("SELECT `accountId` FROM `accounts` WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Adds new account to the database.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="password"></param>
		public void CreateAccount(string accountId, string password)
		{
			password = Password.Hash(password);

			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `accounts` {0}", conn))
			{
				cmd.Set("accountId", accountId);
				cmd.Set("password", password);
				cmd.Set("creation", DateTime.Now);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Adds card to database and returns it as Card.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="type"></param>
		/// <param name="race"></param>
		/// <returns></returns>
		public Card AddCard(string accountId, int type, int race)
		{
			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("INSERT INTO `cards` (`accountId`, `type`, `race`) VALUES (@accountId, @type, @race)", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@type", type);
				mc.Parameters.AddWithValue("@race", race);

				mc.ExecuteNonQuery();

				return new Card(mc.LastInsertedId, type, race);
			}
		}

		/// <summary>
		/// Returns true if the name is valid and available.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public NameCheckResult NameOkay(string name, string serverName)
		{
			if (!_nameCheckRegex.IsMatch(name))
				return NameCheckResult.Invalid;

			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("SELECT `creatureId` FROM `creatures` WHERE `name` = @name AND `server` = @serverName", conn);
				mc.Parameters.AddWithValue("@name", name);
				mc.Parameters.AddWithValue("@serverName", serverName);

				using (var reader = mc.ExecuteReader())
				{
					if (reader.HasRows)
						return NameCheckResult.Exists;
				}
			}

			return NameCheckResult.Okay;
		}
	}

	/// <summary>
	/// Extensions for the MySqlDataReader.
	/// </summary>
	public static class MySqlDataReaderExtension
	{
		/// <summary>
		/// Returns true if value at index is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static bool IsDBNull(this MySqlDataReader reader, string index)
		{
			return reader.IsDBNull(reader.GetOrdinal(index));
		}

		/// <summary>
		/// Same as GetString, except for a is null check. Returns null if NULL.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string GetStringSafe(this MySqlDataReader reader, string index)
		{
			if (IsDBNull(reader, index))
				return null;
			else
				return reader.GetString(index);
		}

		/// <summary>
		/// Returns DateTime of the index, or DateTime.MinValue, if value is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static DateTime GetDateTimeSafe(this MySqlDataReader reader, string index)
		{
			return reader[index] as DateTime? ?? DateTime.MinValue;
		}
	}

	/// <summary>
	/// Result of NameOkay.
	/// </summary>
	public enum NameCheckResult : byte
	{
		Okay = 0,
		Exists = 1,
		Invalid = 2,
	}
}
