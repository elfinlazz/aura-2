// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Aura.Shared.Util;

namespace Aura.Shared.Mabi
{
	public static class Password
	{
		private const int BCryptStrength = 12;

		private static MD5 _md5 = MD5.Create();
		private static SHA256Managed _sha256 = new SHA256Managed();

		/// <summary>
		/// Hashes password bin with MD5.
		/// </summary>
		public static string RawToMD5(byte[] passbin)
		{
			var password = string.Empty;
			foreach (var chr in passbin.TakeWhile(a => a != 0))
				password += (char)chr;

			return RawToMD5(password);
		}

		/// <summary>
		/// Hashes password with MD5.
		/// </summary>
		public static string RawToMD5(string password)
		{
			return BitConverter.ToString(_md5.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
		}

		/// <summary>
		/// Hashes password with SHA256.
		/// </summary>
		public static string MD5ToSHA256(string password)
		{
			return BitConverter.ToString(_sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
		}

		/// <summary>
		/// Hashes password first with MD5, then with SHA256.
		/// </summary>
		public static string RawToMD5SHA256(string password)
		{
			return MD5ToSHA256(RawToMD5(password));
		}

		/// <summary>
		/// Hashes password coming from the client with BCrypt.
		/// </summary>
		public static string Hash(string password)
		{
			return BCrypt.HashPassword(password, BCrypt.GenerateSalt(BCryptStrength));
		}

		/// <summary>
		/// Hashes raw password with MD5, SHA256, and BCrypt.
		/// </summary>
		public static string HashRaw(string password)
		{
			return BCrypt.HashPassword(RawToMD5SHA256(password), BCrypt.GenerateSalt(BCryptStrength));
		}

		/// <summary>
		/// Checks the password with BCrypt.
		/// </summary>
		public static bool Check(string password, string hashedPassword)
		{
			return BCrypt.CheckPassword(password, hashedPassword);
		}
	}
}
