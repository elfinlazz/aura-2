// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.IO;
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
				var di = new DirectoryInfo(path);

				var files = Directory.EnumerateFiles(path);
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
			using (var sr = new StreamReader(path, Encoding.UTF8))
			{
				if (!File.Exists(path))
					return;

				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine().Trim();
					if (line.Length < 3 || line.StartsWith("//"))
						continue;

					// Next line if not tab found
					var pos = line.IndexOf('\t');
					if (pos < 0) continue;

					var key = line.Substring(0, pos).Trim();
					var val = line.Substring(pos + 1).Trim();

					// Replace \t and [\r]\n
					val = val.Replace("\\t", "\t");
					val = val.Replace("\\r\\n", "\n");
					val = val.Replace("\\n", "\n");

					_storage[key] = val;
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
