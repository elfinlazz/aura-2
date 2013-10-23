// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Database;
using MySql.Data.MySqlClient;
using Aura.Shared.Util;
using Aura.Shared.Mabi;

namespace Aura.Login.Database
{
	public class LoginDb
	{
		public static readonly LoginDb Instance = new LoginDb();

		private LoginDb()
		{
		}

		/// <summary>
		/// Adds new account to the database.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="password"></param>
		public void CreateAccount(string accountId, string password)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				password = Password.Hash(password);

				var mc = new MySqlCommand("INSERT INTO `accounts` (`accountId`, `password`, `creation`) VALUES (@accountId, @password, @creation)", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@password", password);
				mc.Parameters.AddWithValue("@creation", MabiTime.Now.DateTime);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Returns account or null if account doesn't exist.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public Account GetAccount(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("SELECT * FROM `accounts` WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				using (var reader = mc.ExecuteReader())
				{
					if (!reader.Read())
						return null;

					var account = new Account();
					account.Name = reader.GetStringSafe("accountId");
					account.Password = reader.GetStringSafe("password");
					account.SecondaryPassword = reader.GetStringSafe("secondaryPassword");
					account.SessionKey = reader.GetInt64("sessionKey");
					account.BannedExpiration = reader.GetDateTimeSafe("banExpiration");
					account.BannedReason = reader.GetStringSafe("banReason");

					return account;
				}
			}
		}

		/// <summary>
		/// Updates secondary password.
		/// </summary>
		/// <param name="account"></param>
		public void UpdateAccountSecondaryPassword(Account account)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("UPDATE `accounts` SET `secondaryPassword` = @secondaryPassword WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", account.Name);
				mc.Parameters.AddWithValue("@secondaryPassword", account.SecondaryPassword);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Sets new randomized session key for the account and returns it.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public long CreateSession(string accountId)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var sessionKey = RandomProvider.Get().NextInt64();

				var mc = new MySqlCommand("UPDATE `accounts` SET `sessionKey` = @sessionKey WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@sessionKey", sessionKey);

				mc.ExecuteNonQuery();

				return sessionKey;
			}
		}

		/// <summary>
		/// Updates lastLogin and loggedIn from the account.
		/// </summary>
		/// <param name="account"></param>
		public void UpdateAccount(Account account)
		{
			using (var conn = AuraDb.Instance.Connection)
			{
				var mc = new MySqlCommand("UPDATE `accounts` SET `lastLogin` = @lastLogin, `loggedIn` = @loggedIn WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", account.Name);
				mc.Parameters.AddWithValue("@lastLogin", account.LastLogin);
				mc.Parameters.AddWithValue("@loggedIn", account.LoggedIn);

				mc.ExecuteNonQuery();
			}
		}
	}
}
