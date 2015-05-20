// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Scripting.Compilers;
using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Aura.Shared.Scripting
{
	/// <summary>
	/// Base class for Aura's script managers, that work on the user/system
	/// folder-system.
	/// </summary>
	public abstract class UserSystemScriptManager : ScriptManager
	{
		/// <summary>
		/// Relative path to the system script folder
		/// </summary>
		protected const string SystemIndexRoot = "system/scripts/";

		/// <summary>
		/// Relative path to the user script folder
		/// </summary>
		protected const string UserIndexRoot = "user/scripts/";

		/// <summary>
		/// Relative path to the cache folder
		/// </summary>
		protected const string CacheRoot = "cache/";

		/// <summary>
		/// Returns list of script files loaded from scripts.txt.
		/// </summary>
		/// <param name="scriptListFile"></param>
		/// <returns></returns>
		protected override List<string> ReadScriptList(string scriptListFile)
		{
			// Get original list
			var result = base.ReadScriptList(scriptListFile);

			// Fix paths to prioritize files in user over system
			var user = Path.GetFullPath(UserIndexRoot).Replace("\\", "/");
			var system = Path.GetFullPath(SystemIndexRoot).Replace("\\", "/");

			for (int i = 0; i < result.Count; ++i)
			{
				var path = result[i];
				path = path.Replace(user, "").Replace(system, "");

				if (File.Exists(Path.Combine(UserIndexRoot, path)))
					path = Path.Combine(UserIndexRoot, path).Replace("\\", "/");
				else
					path = Path.Combine(SystemIndexRoot, path).Replace("\\", "/");

				result[i] = path;
			}

			return result;
		}

		/// <summary>
		/// Returns path for the compiled version of the script.
		/// Creates directory structure if it doesn't exist.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected override string GetCachePath(string path)
		{
			path = path.Replace(Path.GetFullPath(SystemIndexRoot).Replace("\\", "/"), "");
			path = path.Replace(Path.GetFullPath(UserIndexRoot).Replace("\\", "/"), "");
			path = path.Replace(Path.GetFullPath(CacheRoot).Replace("\\", "/"), "");

			var result = Path.Combine(CacheRoot, base.GetCachePath(path));
			var dir = Path.GetDirectoryName(result);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			return result;
		}
	}
}
