// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aura.Shared.Util
{
	/// <summary>
	/// Manages localization strings.
	/// </summary>
	public static class Localization
	{
		private static Dictionary<string, string> _storage = new Dictionary<string, string>();

		/// <summary>
		/// Starts parsing on path.
		/// </summary>
		/// <remarks>
		/// If path is a file, it simply reads the file. If path is a directory,
		/// it starts parsing all files recursively.
		/// </remarks>
		/// <param name="path">What to parse</param>
		public static void Parse(string path)
		{
			if (File.Exists(path))
			{
				LoadFile(path);
			}
			else if (Directory.Exists(path))
			{
				var files = Directory.EnumerateFiles(path).Where(a => Path.GetExtension(a).ToLower() == ".po");
				foreach (var file in files)
					LoadFile(file);

				var dirs = Directory.EnumerateDirectories(path);
				foreach (var dir in dirs)
					Parse(dir);
			}
			else
			{
				throw new FileNotFoundException(path + " not found.");
			}
		}

		/// <summary>
		/// Adds strings in file to collection.
		/// </summary>
		/// <param name="path"></param>
		private static void LoadFile(string path)
		{
			if (!File.Exists(path))
				return;

			using (var sr = new StreamReader(path, Encoding.UTF8))
			{
				var buffer = new StringBuilder();
				var id = "";
				var state = 0; // 1=id, 2=str

				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine().Trim();
					var start = -1;

					// Skip empty lines and comments
					if (string.IsNullOrWhiteSpace(line) || line[0] == '#' || (start = line.IndexOf('"')) == -1)
						continue;

					if (line.StartsWith("msgid"))
					{
						// If we were reading a str value, put it into storage
						// if it's not blank.
						if (state == 2)
						{
							var val = buffer.ToString();
							if (!string.IsNullOrWhiteSpace(val))
								_storage[id] = val;
							buffer.Clear();
						}

						state = 1;
					}

					if (line.StartsWith("msgstr"))
					{
						// If we were reading an id, save it
						if (state == 1)
						{
							id = buffer.ToString();
							buffer.Clear();
						}

						state = 2;
					}

					// Read string in between the "s
					start += 1;
					var length = line.LastIndexOf('"') - start;

					buffer.Append(line.Substring(start, length));
				}
			}
		}

		/// <summary>
		/// Returns localization string, or the key, if it doesn't exist.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string Get(string key)
		{
			string val;
			_storage.TryGetValue(key, out val);
			if (val != null)
				return val;
			return key;
		}
	}
}
