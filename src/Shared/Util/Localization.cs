// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence file in the main folder

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
		/// it starts parsing all files recursively, using the folder structure
		/// to make up the prefixes.
		/// </remarks>
		/// <param name="path">What to parse</param>
		/// <param name="root">On root level? (base folder?)</param>
		/// <param name="prefix">Dot prefix for the keys</param>
		public static void Parse(string path, bool root = true, string prefix = "")
		{
			if (File.Exists(path))
			{
				LoadFile(path);
			}
			else if (Directory.Exists(path))
			{
				var di = new DirectoryInfo(path);

				if (!root)
				{
					if (prefix != "")
						prefix += ".";
					prefix += di.Name;
				}

				var files = Directory.EnumerateFiles(path);
				foreach (var file in files)
				{
					LoadFile(file, prefix);
				}

				var dirs = Directory.EnumerateDirectories(path);
				foreach (var dir in dirs)
				{
					Parse(dir, false, prefix);
				}
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
		/// <param name="prefix"></param>
		private static void LoadFile(string path, string prefix = "")
		{
			using (var sr = new StreamReader(path, Encoding.UTF8))
			{
				if (!File.Exists(path))
					return;

				if (prefix != "")
					prefix += ".";
				prefix += Path.GetFileNameWithoutExtension(path);

				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine().Trim();
					if (line.Length < 3 || line.StartsWith("//"))
						continue;

					var pos = line.IndexOf('\t');
					if (pos < 0)
						continue;

					var val = line.Substring(pos + 1).Trim();

					// Replace \t and [\r]\n
					val = val.Replace("\\t", "\t");
					val = val.Replace("\\r\\n", "\n");
					val = val.Replace("\\n", "\n");

					_storage[prefix + "." + line.Substring(0, pos).Trim()] = val;
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
