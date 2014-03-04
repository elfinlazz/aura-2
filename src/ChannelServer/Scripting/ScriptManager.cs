// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Aura.Channel.Database;
using Aura.Channel.Scripting.Compilers;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;
using Aura.Channel.World;
using Aura.Channel.World.Quests;
using System.Collections;
using System.Threading.Tasks;
using Aura.Channel.World.Shops;
using Aura.Channel.Network.Sending;

namespace Aura.Channel.Scripting
{
	public class ScriptManager
	{
		private const string SystemIndexRoot = "system/scripts/";
		private const string UserIndexRoot = "user/scripts/";
		private const string IndexPath = SystemIndexRoot + "scripts.txt";

		private Dictionary<string, Compiler> _compilers;

		private Dictionary<int, ItemScript> _itemScripts;
		private Dictionary<string, Type> _aiScripts;
		private Dictionary<string, NpcShop> _shops;
		private Dictionary<int, QuestScript> _questScripts;

		private Dictionary<string, Dictionary<string, List<ScriptHook>>> _hooks;

		private Dictionary<int, CreatureSpawn> _creatureSpawns;

		private List<IDisposable> _scriptsToDispose;

		public ScriptVariables GlobalVars { get; protected set; }

		public ScriptManager()
		{
			_compilers = new Dictionary<string, Compiler>();
			_compilers.Add("cs", new CSharpCompiler());
			_compilers.Add("boo", new BooCompiler());

			_itemScripts = new Dictionary<int, ItemScript>();
			_aiScripts = new Dictionary<string, Type>();
			_shops = new Dictionary<string, NpcShop>();
			_questScripts = new Dictionary<int, QuestScript>();

			_hooks = new Dictionary<string, Dictionary<string, List<ScriptHook>>>();

			_creatureSpawns = new Dictionary<int, CreatureSpawn>();

			_scriptsToDispose = new List<IDisposable>();

			this.GlobalVars = new ScriptVariables();
		}

		public void Init()
		{
			this.GlobalVars.Perm = ChannelDb.Instance.LoadVars("Aura System", 0);
			ChannelServer.Instance.Events.MabiTick += OnMabiTick;
		}

		/// <summary>
		/// Loads all scripts.
		/// </summary>
		public void Load()
		{
			this.LoadAiScripts();
			this.LoadItemScripts();
			this.LoadScripts();

			this.LoadSpawns();
		}

		/// <summary>
		/// Removes all NPCs, props, etc and loads them again.
		/// </summary>
		public void Reload()
		{
			foreach (var script in _scriptsToDispose)
				script.Dispose();
			_scriptsToDispose.Clear();

			ChannelServer.Instance.World.RemoveScriptedEntities();

			this.Load();
		}

		/// <summary>
		/// Loads scripts from list.
		/// </summary>
		private void LoadScripts()
		{
			Log.Info("Loading scripts, this might take a few minutes...");

			_creatureSpawns.Clear();
			_questScripts.Clear();
			_hooks.Clear();
			_shops.Clear();

			if (!File.Exists(IndexPath))
			{
				Log.Error("Script list not found at '{0}'.", IndexPath);
				return;
			}

			// Read script list
			var toLoad = new OrderedDictionary();
			try
			{
				using (var fr = new FileReader(IndexPath))
				{
					foreach (var line in fr)
					{
						// Get script path for either user or system
						var scriptPath = Path.Combine(UserIndexRoot, line);
						if (!File.Exists(scriptPath))
							scriptPath = Path.Combine(SystemIndexRoot, line);
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
				var asm = this.Compile(filePath);
				if (asm != null)
				{
					this.LoadScriptAssembly(asm, filePath);
					loaded++;
				}

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
		/// Generates script for all items and loads it.
		/// </summary>
		private void LoadItemScripts()
		{
			Log.Info("Loading item scripts...");

			_itemScripts.Clear();

			// Place generated script in the cache folder
			var tmpPath = this.GetCachePath(Path.Combine(SystemIndexRoot, "items", "inline.generated.cs")).Replace(".compiled", "");

			// We go over all items only once, inline scripts are added
			// to the generated script if the inline script needs updating.
			var updateInline =
				(File.GetLastWriteTime(Path.Combine("system", "db", "items.txt")) >= File.GetLastWriteTime(tmpPath)) ||
				(File.GetLastWriteTime(Path.Combine("user", "db", "items.txt")) >= File.GetLastWriteTime(tmpPath));

			var sb = new StringBuilder();

			// Default usings
			sb.AppendLine("// Automatically generated from inline scripts in the item database");
			sb.AppendLine();
			sb.AppendLine("using Aura.Channel.World.Entities;");
			sb.AppendLine("using Aura.Channel.Scripting.Scripts;");
			sb.AppendLine();

			// Go through all items
			foreach (var entry in AuraData.ItemDb.Entries.Values)
			{
				var empty = (string.IsNullOrWhiteSpace(entry.OnUse) && string.IsNullOrWhiteSpace(entry.OnEquip) && string.IsNullOrWhiteSpace(entry.OnUnequip));
				var match = Regex.Match(entry.OnUse, @"^\s*use\s*\(\s*""([^"")]+)""\s*\)\s*;?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				var use = match.Success;

				// Inline script
				if (!empty && !use)
				{
					if (updateInline)
					{
						// Add inline scripts to the collection,
						// wrapped in an ItemScript class.
						sb.AppendFormat("public class ItemScript{0} : ItemScript {{" + Environment.NewLine, entry.Id);
						{
							if (!string.IsNullOrWhiteSpace(entry.OnUse))
								sb.AppendFormat("	public override void OnUse(Creature cr, Item i)     {{ {0} }}" + Environment.NewLine, entry.OnUse.Trim());
							if (!string.IsNullOrWhiteSpace(entry.OnEquip))
								sb.AppendFormat("	public override void OnEquip(Creature cr, Item i)   {{ {0} }}" + Environment.NewLine, entry.OnEquip.Trim());
							if (!string.IsNullOrWhiteSpace(entry.OnUnequip))
								sb.AppendFormat("	public override void OnUnequip(Creature cr, Item i) {{ {0} }}" + Environment.NewLine, entry.OnUnequip.Trim());
						}
						sb.AppendFormat("}}" + Environment.NewLine + Environment.NewLine);
					}

					continue;
				}

				// Not inline or empty

				// Get file name from use command or by id.
				var scriptFile = (use ? (match.Groups[1].Value) : (entry.Id + ".cs"));

				var scriptPath = Path.Combine(UserIndexRoot, "items", scriptFile);
				if (!File.Exists(scriptPath))
					scriptPath = Path.Combine(SystemIndexRoot, "items", scriptFile);
				if (!File.Exists(scriptPath))
				{
					// Only show error if the use was explicit
					if (use)
						Log.Error("Item script not found: {0}", "items/" + scriptFile);
					continue;
				}

				var asm = this.Compile(scriptPath);
				if (asm != null)
					this.LoadItemScriptAssembly(asm, entry.Id);

				Log.Debug(_itemScripts[entry.Id].GetType().Name);
			}

			// Update inline script
			if (updateInline)
			{
				File.WriteAllText(tmpPath, sb.ToString());
			}

			// Compile will update assembly if generated script was updated
			//foreach (string filePath in )
			var inlineAsm = this.Compile(tmpPath);
			if (inlineAsm != null)
				this.LoadItemScriptAssembly(inlineAsm);

			Log.Info("Done loading item scripts.");
		}

		/// <summary>
		/// Loads AI scripts
		/// </summary>
		private void LoadAiScripts()
		{
			Log.Info("Loading AI scripts...");

			_aiScripts.Clear();

			foreach (var folder in new string[] { Path.Combine(SystemIndexRoot, "ai"), Path.Combine(UserIndexRoot, "ai") })
			{
				if (!Directory.Exists(folder))
					continue;

				foreach (var filePath in Directory.GetFiles(folder))
				{
					var fileName = Path.GetFileNameWithoutExtension(filePath);

					var asm = this.Compile(filePath);
					if (asm != null)
					{
						// Get first AiScript class and save the type
						foreach (var type in asm.GetTypes().Where(a => a.IsSubclassOf(typeof(AiScript))))
						{
							_aiScripts[fileName] = type;
							break;
						}
					}
				}
			}

			Log.Info("Done loading AI scripts.");
		}


		/// <summary>
		/// Returns shop or null.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public NpcShop GetShop(string typeName)
		{
			NpcShop result;
			_shops.TryGetValue(typeName, out result);
			return result;
		}

		/// <summary>
		/// Returns new AI script by name for creature, or null.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private AiScript GetAi(string name, Creature creature)
		{
			Type type;
			_aiScripts.TryGetValue(name, out type);
			if (type == null)
				return null;

			var script = Activator.CreateInstance(type) as AiScript;
			script.Creature = creature;

			return script;
		}

		/// <summary>
		///  Compiles script and returns the resulting assembly, or null.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private Assembly Compile(string path)
		{
			if (!File.Exists(path))
			{
				Log.Error("Script '{0}' not found.", path);
				return null;
			}

			var outPath = this.GetCachePath(path);

			Compiler compiler;
			_compilers.TryGetValue(Path.GetExtension(path).TrimStart('.'), out compiler);
			if (compiler == null)
			{
				Log.Error("No compiler found for script '{0}'.", path);
				return null;
			}

			try
			{
				var scriptAsm = compiler.Compile(path, outPath);
				//this.LoadScriptAssembly(scriptAsm);

				return scriptAsm;
			}
			catch (CompilerErrorsException ex)
			{
				File.Delete(outPath);

				var lines = File.ReadAllLines(path);

				foreach (var err in ex.Errors)
				{
					// Error msg
					Log.WriteLine((!err.IsWarning ? LogLevel.Error : LogLevel.Warning), "In {0} on line {1}, column {2}", err.File, err.Line, err.Column);
					Log.WriteLine(LogLevel.None, "          {0}", err.Message);

					// Display lines around the error
					int startLine = Math.Max(1, err.Line - 1);
					int endLine = Math.Min(lines.Length, startLine + 2);
					for (int i = startLine; i <= endLine; ++i)
					{
						// Make sure we don't get out of range.
						// (ReadAllLines "trims" the input)
						var line = (i <= lines.Length) ? lines[i - 1] : "";

						Log.WriteLine(LogLevel.None, "  {2} {0:0000}: {1}", i, line, (err.Line == i ? '*' : ' '));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "LoadScript: Problem while loading script '{0}'", path);
				//File.Delete(outPath);
			}

			return null;
		}

		/// <summary>
		/// Loads script classes inside assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="filePath">Path of the script, for reference</param>
		/// <returns></returns>
		private void LoadScriptAssembly(Assembly asm, string filePath)
		{
			foreach (var type in asm.GetTypes().Where(a => !a.IsAbstract && !a.Name.StartsWith("_")))
			{
				try
				{
					if (type.IsSubclassOf(typeof(BaseScript)))
					{
						var baseScript = Activator.CreateInstance(type) as BaseScript;
						baseScript.ScriptFilePath = filePath;

						// Try to load as NpcScript
						if (baseScript is NpcScript)
						{
							var npcScript = baseScript as NpcScript;

							npcScript.NPC.AI = this.GetAi("npc_normal", npcScript.NPC);

							npcScript.Load();
							npcScript.NPC.State = CreatureStates.Npc | CreatureStates.NamedNpc | CreatureStates.GoodNpc;
							npcScript.NPC.Script = npcScript;
							npcScript.NPC.LoadDefault();

							if (npcScript.NPC.RegionId > 0)
							{
								var region = ChannelServer.Instance.World.GetRegion(npcScript.NPC.RegionId);
								if (region == null)
								{
									Log.Error("Failed to spawn '{0}', region '{1}' not found.", type, npcScript.NPC.RegionId);
									continue;
								}

								region.AddCreature(npcScript.NPC);
							}
						}
						// Try to load as QuestScript
						else if (baseScript is QuestScript)
						{
							var questScript = baseScript as QuestScript;
							questScript.Load();

							if (questScript.Id == 0 || _questScripts.ContainsKey(questScript.Id))
							{
								Log.Error("{1}.Load: Invalid id or already in use ({0}).", questScript.Id, type.Name);
								continue;
							}

							if (questScript.Objectives.Count == 0)
							{
								Log.Error("{1}.Load: Quest '{0}' doesn't have any objectives.", questScript.Id, type.Name);
								continue;
							}

							questScript.Init();

							_questScripts[questScript.Id] = questScript;
						}
						// Try to load as RegionScript
						else if (baseScript is RegionScript)
						{
							var regionScript = baseScript as RegionScript;
							regionScript.Load();
							regionScript.LoadWarps();
							regionScript.LoadSpawns();
						}
						else
						{
							// General Script, just load it.
							baseScript.Load();
						}

						baseScript.AutoLoadEvents();
						_scriptsToDispose.Add(baseScript);
					}
					else if (type.IsSubclassOf(typeof(NpcShop)))
					{
						if (_shops.ContainsKey(type.Name))
						{
							Log.Error("LoadShops: Duplicate '{0}'", type.Name);
							continue;
						}

						_shops[type.Name] = Activator.CreateInstance(type) as NpcShop;
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Error while loading script '{0}' ({1}).", type.Name, ex.Message);
				}
			}
		}

		/// <summary>
		/// Loads item script classes from assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="itemId">Only loads first class for itemId, if this is not 0.</param>
		/// <remarks>
		/// Item scripts have some special needs, like needing an item id.
		/// Hard to handle inside the normal loading.
		/// If itemId is 0 it's retreived from the class name, ItemScript(Id).
		/// </remarks>
		private void LoadItemScriptAssembly(Assembly asm, int itemId = 0)
		{
			foreach (var type in asm.GetTypes().Where(a => a.IsSubclassOf(typeof(ItemScript)) && !a.IsAbstract && !a.Name.StartsWith("_")))
			{
				var itemScript = Activator.CreateInstance(type) as ItemScript;

				// Stop after first type if this loading was for a single item.
				if (itemId != 0)
				{
					_itemScripts[itemId] = itemScript;
					return;
				}

				// Parse item id from name
				if (!Regex.IsMatch(type.Name, "^ItemScript[0-9]+$") || !int.TryParse(type.Name.Substring(10), out itemId))
				{
					Log.Error("Unable to parse item id of item script '{0}'.", type.Name);
					continue;
				}

				_itemScripts[itemId] = itemScript;

				itemId = 0;
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
			var result = (!path.StartsWith("cache") ? Path.Combine("cache", path + ".compiled") : path + ".compiled");
			var dir = Path.GetDirectoryName(result);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			return result;
		}

		/// <summary>
		/// Return the item script by itemId, or null.
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public ItemScript GetItemScript(int itemId)
		{
			ItemScript result;
			_itemScripts.TryGetValue(itemId, out result);
			return result;
		}

		/// <summary>
		/// Adds spawn.
		/// </summary>
		/// <param name="spawn"></param>
		public void AddCreatureSpawn(CreatureSpawn spawn)
		{
			_creatureSpawns[spawn.Id] = spawn;
		}

		/// <summary>
		/// Spawns creatures using creature spawn information.
		/// </summary>
		public void LoadSpawns()
		{
			Log.Info("Spawning creatures...");

			var spawned = 0;

			foreach (var spawn in _creatureSpawns.Values)
			{
				spawned += this.Spawn(spawn);
				continue;
			}

			Log.Info("Done spawning {0} creatures.", spawned);
		}

		/// <summary>
		/// Spawns all creatures for spawn, or amount.
		/// </summary>
		/// <param name="spawnId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public int Spawn(int spawnId, int amount = 0)
		{
			if (!_creatureSpawns.ContainsKey(spawnId))
			{
				Log.Warning("ScriptManager.Spawn: Failed, missing spawn '{0}'.", spawnId);
				return 0;
			}

			return this.Spawn(_creatureSpawns[spawnId], amount);
		}

		/// <summary>
		/// Spawns all creatures for spawn, or amount.
		/// </summary>
		/// <param name="spawn"></param>
		/// <returns></returns>
		public int Spawn(CreatureSpawn spawn, int amount = 0)
		{
			var result = 0;
			if (amount == 0)
				amount = spawn.Amount;

			for (int i = 0; i < amount; ++i)
			{
				var pos = spawn.GetRandomPosition();
				if (this.Spawn(spawn.RaceId, spawn.RegionId, pos.X, pos.Y, spawn.Id, false, false) == null)
					return result;

				result++;
			}

			return result;
		}

		/// <summary>
		/// Spawns a creature.
		/// </summary>
		/// <param name="raceId"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="spawnId"></param>
		/// <returns></returns>
		public Creature Spawn(int raceId, int regionId, int x, int y, int spawnId, bool active, bool effect)
		{
			var creature = new NPC();
			creature.Race = raceId;
			creature.LoadDefault();
			creature.SpawnId = spawnId;
			creature.Name = creature.RaceData.Name;
			creature.Color1 = creature.RaceData.Color1;
			creature.Color2 = creature.RaceData.Color2;
			creature.Color3 = creature.RaceData.Color3;
			creature.Height = creature.RaceData.Size;
			creature.Life = creature.LifeMaxBase = creature.RaceData.Life;
			creature.State = (CreatureStates)creature.RaceData.DefaultState;
			creature.Direction = (byte)RandomProvider.Get().Next(256);

			creature.Drops.GoldMin = creature.RaceData.GoldMin;
			creature.Drops.GoldMax = creature.RaceData.GoldMax;
			creature.Drops.Add(creature.RaceData.Drops);

			if (!string.IsNullOrWhiteSpace(creature.RaceData.AI) && creature.RaceData.AI != "none")
			{
				creature.AI = this.GetAi(creature.RaceData.AI, creature);
				if (creature.AI == null)
					Log.Warning("Spawn: Missing AI '{0}' for '{1}'.", creature.RaceData.AI, raceId);
			}

			if (!creature.Warp(regionId, x, y))
			{
				Log.Error("Failed to spawn '{0}'s.", raceId);
				return null;
			}

			if (creature.AI != null && active)
				creature.AI.Activate(0);

			if (effect)
				Send.SpawnEffect(SpawnEffect.Monster, creature.RegionId, x, y, creature, creature);

			return creature;
		}

		/// <summary>
		/// 5 min tick, global var saving.
		/// </summary>
		/// <param name="time"></param>
		public void OnMabiTick(ErinnTime time)
		{
			ChannelDb.Instance.SaveVars("Aura System", 0, this.GlobalVars.Perm);
			Log.Info("Saved global script variables.");
		}

		/// <summary>
		/// Returs quest data or null.
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public QuestScript GetQuestScript(int questId)
		{
			QuestScript script;
			_questScripts.TryGetValue(questId, out script);
			return script;
		}

		/// <summary>
		/// Calls delegates for npc and hook.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hook"></param>
		/// <returns></returns>
		public IEnumerable<ScriptHook> GetHooks(string npcName, string hook)
		{
			Dictionary<string, List<ScriptHook>> hooks;
			_hooks.TryGetValue(npcName, out hooks);
			if (hooks == null)
				return Enumerable.Empty<ScriptHook>();

			List<ScriptHook> calls;
			hooks.TryGetValue(hook, out calls);
			if (calls == null)
				return Enumerable.Empty<ScriptHook>();

			return calls;
		}

		/// <summary>
		/// Registers hook delegate.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hook"></param>
		/// <param name="func"></param>
		public void AddHook(string npcName, string hook, ScriptHook func)
		{
			if (!_hooks.ContainsKey(npcName))
				_hooks[npcName] = new Dictionary<string, List<ScriptHook>>();

			if (!_hooks[npcName].ContainsKey(hook))
				_hooks[npcName][hook] = new List<ScriptHook>();

			_hooks[npcName][hook].Add(func);
		}
	}

	public delegate Task<HookResult> ScriptHook(NpcScript npc, params object[] args);
}
