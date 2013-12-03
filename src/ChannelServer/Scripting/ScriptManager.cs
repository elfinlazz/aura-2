// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aura.Channel.Scripting.Compilers;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using System.Collections.Specialized;

namespace Aura.Channel.Scripting
{
	public class ScriptManager
	{
		public const string UserIndex = "user/scripts/scripts.txt";
		public const string SystemIndex = "system/scripts/scripts.txt";

		private Dictionary<string, Compiler> _compilers;

		public ScriptManager()
		{
			_compilers = new Dictionary<string, Compiler>();
			_compilers.Add("cs", new CSharpCompiler());
			_compilers.Add("boo", new BooCompiler());
		}

		/// <summary>
		/// Loads all scripts.
		/// </summary>
		public void Load()
		{
			Log.Info("Loading scripts...");

			var indexPath = File.Exists(UserIndex) ? UserIndex : SystemIndex;
			var indexRoot = Path.GetDirectoryName(indexPath);

			if (!File.Exists(indexPath))
			{
				Log.Error("Script list not found.");
				return;
			}

			// Read script list
			var toLoad = new OrderedDictionary();
			try
			{
				using (var fr = new FileReader(indexPath))
				{
					foreach (var line in fr)
					{
						var scriptPath = Path.Combine(indexRoot, line);
						if (!File.Exists(scriptPath))
						{
							Log.Warning("Script not found: {0}", line);
							continue;
						}

						toLoad[scriptPath] = true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Failed to read script list.");
				return;
			}

			// Load scripts
			int done = 0, loaded = 0;
			foreach (string filePath in toLoad.Keys)
			{
				if (this.LoadScript(filePath))
					loaded++;

				if (done % 5 == 0)
					Log.Progress(done + 1, toLoad.Count);

				done++;
			}
			Log.Progress(100, 100);

			if (toLoad.Count > 0)
				Log.WriteLine();
			Log.Info("Done loading {0} scripts (of {1}).", loaded, toLoad.Count);
		}

		/// <summary>
		/// Retrieves/creates assembly and loads script classes inside.
		/// Returns true if successful.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool LoadScript(string path)
		{
			if (!File.Exists(path))
			{
				Log.Error("Script not found: ", path);
				return false;
			}

			Compiler compiler;
			_compilers.TryGetValue(Path.GetExtension(path).TrimStart('.'), out compiler);
			if (compiler == null)
			{
				Log.Error("No compiler found for script '{0}'.", path);
				return false;
			}

			try
			{
				var scriptAsm = compiler.Compile(path, this.GetCachePath(path));
				this.LoadScript(scriptAsm);

				return true;
			}
			catch (CompilerErrorsException ex)
			{
				var lines = File.ReadAllLines(path);

				foreach (var err in ex.Errors)
				{
					// Error msg
					Log.Error("In {0} on line {1}", err.File, err.Line);
					Log.WriteLine(LogLevel.None, "          " + err.Message);

					// Display lines around the error
					int startIdx = Math.Max(1, Math.Min(lines.Length, err.Line));
					for (int i = startIdx - 1; i < startIdx + 2; ++i)
						Log.WriteLine(LogLevel.None, "  {2} {0:0000}: {1}", i, lines[i - 1], (err.Line == i ? '*' : ' '));
				}

				return false;
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "LoadScript: Problem while loading script '{0}'", path);
				return false;
			}
		}

		/// <summary>
		/// Loads script classes inside assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		private void LoadScript(Assembly asm)
		{
			foreach (var type in asm.GetTypes())
			{
				if (type.IsAbstract)
					continue;

				var scriptObj = Activator.CreateInstance(type);

				// Try to load as NpcScript
				var npcScript = scriptObj as NpcScript;
				if (npcScript != null)
				{
					npcScript.Load();
					npcScript.NPC.State = CreatureStates.GoodNpc | CreatureStates.NamedNpc;

					if (npcScript.NPC.RegionId > 0)
					{
						var region = WorldManager.Instance.GetRegion(npcScript.NPC.RegionId);
						if (region != null)
						{
							region.AddCreature(npcScript.NPC);
						}
						else
						{
							Log.Error("LoadScript: Failed to spawn '{0}', region '{1}' not found.", type, npcScript.NPC.RegionId);
						}
					}

					continue;
				}

				Log.Error("Unknown script type '{0}'.", type);
			}
		}

		/// <summary>
		/// Returns path for the compiled version of the script.
		/// Creates directory if it doesn't exist.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string GetCachePath(string path)
		{
			var result = Path.Combine("cache", path + ".compiled");
			var dir = Path.GetDirectoryName(result);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			return result;
		}
	}
}