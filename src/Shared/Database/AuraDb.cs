// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Aura.Shared.Mabi.Const;

namespace Aura.Shared.Database
{
	public class AuraDb
	{
		public static readonly AuraDb Instance = new AuraDb();

		private string _connectionString;

		/// <summary>
		/// Returns new open connection.
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

		private AuraDb()
		{
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
	}

	public static class MySqlDataReaderExtension
	{
		public static bool IsDBNull(this MySqlDataReader reader, string index)
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

	public enum NameCheckResult : byte
	{
		Okay = 0,
		Exists = 1,
		Invalid = 2,
	}
}
