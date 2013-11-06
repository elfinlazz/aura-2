// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.IO;

namespace Aura.Shared.Util.Configuration
{
	/// <summary>
	/// Configuration options manager.
	/// </summary>
	/// <remarks>
	/// Uses <see cref="FileReader"/> to read conf files, that are parsed in key:value pairs.
	/// Separating character is a colon ':'.
	/// </remarks>
	public class ConfFile
	{
		private Dictionary<string, string> _options;

		public ConfFile()
		{
			_options = new Dictionary<string, string>();
		}

		/// <summary>
		/// Loads all options in the file, and included files.
		/// Does nothing if file doesn't exist.
		/// </summary>
		/// <param name="filePath"></param>
		public void Include(string filePath)
		{
			if (!File.Exists(filePath))
				return;

			this.LoadFile(filePath);
		}

		/// <summary>
		/// Loads all options in the file, and included files.
		/// Throws FileNotFoundException if file couldn't be found.
		/// </summary>
		/// <param name="filePath"></param>
		public void Require(string filePath)
		{
			this.LoadFile(filePath);
		}

		/// <summary>
		/// Uses filePath as format string, replaces {0} with the folders,
		/// one after the other. Requires the first, includes the others.
		/// </summary>
		/// <example>
		/// This would require the database file from system conf and
		/// include the one from user conf:
		/// filePath: ../../{0}/conf/database.conf
		/// folders:  system, user
		/// </example>
		/// <param name="filePath"></param>
		/// <param name="folders"></param>
		public void RequireAndInclude(string filePath, params string[] folders)
		{
			if (folders.Length < 1)
				return;

			for (int i = 0; i < folders.Length; ++i)
			{
				if (i == 0)
					this.Require(string.Format(filePath, folders[i]));
				else
					this.Include(string.Format(filePath, folders[i]));
			}
		}

		/// <summary>
		/// Loads all options in the file, and included files.
		/// </summary>
		/// <param name="filePath"></param>
		private void LoadFile(string filePath)
		{
			using (var fr = new FileReader(filePath))
			{
				foreach (var line in fr)
				{
					int pos = -1;

					// Check for seperator
					if ((pos = line.IndexOf(':')) < 0)
						return;

					_options[line.Substring(0, pos).Trim()] = line.Substring(pos + 1).Trim();
				}
			}
		}

		/// <summary>
		/// Returns the option as bool, or the default value, if the option
		/// doesn't exist.
		/// </summary>
		/// <param name="option"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public bool GetBool(string option, bool defaultValue = false)
		{
			string value;
			if (!_options.TryGetValue(option, out value))
				return defaultValue;

			return (value == "1" || value == "yes" || value == "true");
		}

		/// <summary>
		/// Returns the option as byte, or the default value, if the option
		/// doesn't exist.
		/// </summary>
		/// <param name="option"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public byte GetByte(string option, byte defaultValue = 0)
		{
			string value;
			if (!_options.TryGetValue(option, out value))
				return defaultValue;

			return byte.Parse(value);
		}

		/// <summary>
		/// Returns the option as short, or the default value, if the option
		/// doesn't exist.
		/// </summary>
		/// <param name="option"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public short GetShort(string option, short defaultValue = 0)
		{
			string value;
			if (!_options.TryGetValue(option, out value))
				return defaultValue;

			return short.Parse(value);
		}

		/// <summary>
		/// Returns the option as int, or the default value, if the option
		/// doesn't exist.
		/// </summary>
		/// <param name="option"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int GetInt(string option, int defaultValue = 0)
		{
			string value;
			if (!_options.TryGetValue(option, out value))
				return defaultValue;

			return int.Parse(value);
		}

		/// <summary>
		/// Returns the option as long, or the default value, if the option
		/// doesn't exist.
		/// </summary>
		/// <param name="option"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public long GetLong(string option, long defaultValue = 0)
		{
			string value;
			if (!_options.TryGetValue(option, out value))
				return defaultValue;

			return long.Parse(value);
		}

		/// <summary>
		/// Returns the option as string, or the default value, if the option
		/// doesn't exist.
		/// </summary>
		/// <param name="option"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string GetString(string option, string defaultValue = "")
		{
			string value;
			if (!_options.TryGetValue(option, out value))
				return defaultValue;

			return value;
		}
	}
}
