// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Aura.Channel.Scripting.Compilers;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting
{
	public class ScriptManager
	{
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

			var indexPath = "system/scripts/scripts.txt";
			var systemIndexRoot = Path.GetDirectoryName("system/scripts/");
			var userIndexRoot = Path.GetDirectoryName("user/scripts/");

			if (!File.Exists(indexPath))
			{
				Log.Error("Script list not found at '{0}'.", indexPath);
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
						// Get script path for either user or system
						var scriptPath = Path.Combine(userIndexRoot, line);
						if (!File.Exists(scriptPath))
							scriptPath = Path.Combine(systemIndexRoot, line);
						if (!File.Exists(scriptPath))
						{
							Log.Warning("Script not found: {0}", line);
							continue;
						}

						// Easiest way to get a unique, ordered list.
						toLoad[line] = scriptPath;
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
			foreach (string filePath in toLoad.Values)
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
				Log.Error("Script '{0}' not found.", path);
				return false;
			}

			var outPath = this.GetCachePath(path);

			Compiler compiler;
			_compilers.TryGetValue(Path.GetExtension(path).TrimStart('.'), out compiler);
			if (compiler == null)
			{
				Log.Error("No compiler found for script '{0}'.", path);
				return false;
			}

			try
			{
				var scriptAsm = compiler.Compile(path, outPath);
				this.LoadScriptAssembly(scriptAsm);

				return true;
			}
			catch (CompilerErrorsException ex)
			{
				File.Delete(outPath);

				var lines = File.ReadAllLines(path);

				foreach (var err in ex.Errors)
				{
					// Error msg
					Log.Error("In {0} on line {1}, column {2}", err.File, err.Line, err.Column);
					Log.WriteLine(LogLevel.None, "          " + err.Message);

					// Display lines around the error
					int startIdx = Math.Max(1, Math.Min(lines.Length, err.Line));
					for (int i = startIdx - 1; i < startIdx + 2; ++i)
					{
						// Make sure we don't get out of range.
						// (Does ReadAllLines trim the input?)
						string line = "";
						if (i <= lines.Length)
							line = lines[i - 1];

						Log.WriteLine(LogLevel.None, "  {2} {0:0000}: {1}", i, line, (err.Line == i ? '*' : ' '));
					}
				}

				return false;
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "LoadScript: Problem while loading script '{0}'", path);
				//File.Delete(outPath);
				return false;
			}
		}

		/// <summary>
		/// Loads script classes inside assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <returns></returns>
		private void LoadScriptAssembly(Assembly asm)
		{
			foreach (var type in asm.GetTypes().Where(a => a.IsSubclassOf(typeof(BaseScript))))
			{
				// Ignore abstract class and ones starting with underscore,
				// those are used for inheritance.
				if (type.IsAbstract || type.Name.StartsWith("_"))
					continue;

				var scriptObj = Activator.CreateInstance(type);

				// Try to load as NpcScript
				var npcScript = scriptObj as NpcScript;
				if (npcScript != null)
				{
					npcScript.Load();
					npcScript.NPC.State = CreatureStates.Npc | CreatureStates.NamedNpc | CreatureStates.GoodNpc;
					npcScript.NPC.Script = npcScript;

					if (npcScript.NPC.RegionId > 0)
					{
						var region = WorldManager.Instance.GetRegion(npcScript.NPC.RegionId);
						if (region == null)
						{
							Log.Error("Failed to spawn '{0}', region '{1}' not found.", type, npcScript.NPC.RegionId);
							continue;
						}

						region.AddCreature(npcScript.NPC);
					}

					continue;
				}

				// Unknown Script, just load it.
				(scriptObj as BaseScript).Load();
			}
		}

		/// <summary>
		/// Returns path for the compiled version of the script.
		/// Creates directory structure if it doesn't exist.
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
