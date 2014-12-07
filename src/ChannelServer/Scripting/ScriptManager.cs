// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aura.Channel.Database;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Compilers;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Skills;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Quests;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Mabi;
using Aura.Shared.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting
{
	public class ScriptManager
	{
		private const string SystemIndexRoot = "system/scripts/";
		private const string UserIndexRoot = "user/scripts/";
		private const string IndexPath = SystemIndexRoot + "scripts.txt";

		private Dictionary<string, Compiler> _compilers;

		private Dictionary<string, Type> _scripts;
		private Dictionary<int, ItemScript> _itemScripts;
		private Dictionary<string, Type> _aiScripts;
		private Dictionary<string, NpcShopScript> _shops;
		private Dictionary<int, QuestScript> _questScripts;
		private Dictionary<long, Dictionary<SignalType, Action<Creature, EventData>>> _clientEventHandlers;

		private Dictionary<string, Dictionary<string, List<ScriptHook>>> _hooks;

		private Dictionary<int, CreatureSpawn> _creatureSpawns;

		private List<IDisposable> _scriptsToDispose;

		public ScriptVariables GlobalVars { get; protected set; }

		public ScriptManager()
		{
			_compilers = new Dictionary<string, Compiler>();
			_compilers.Add("cs", new CSharpCompiler());
			_compilers.Add("boo", new BooCompiler());

			_scripts = new Dictionary<string, Type>();
			_itemScripts = new Dictionary<int, ItemScript>();
			_aiScripts = new Dictionary<string, Type>();
			_shops = new Dictionary<string, NpcShopScript>();
			_questScripts = new Dictionary<int, QuestScript>();
			_clientEventHandlers = new Dictionary<long, Dictionary<SignalType, Action<Creature, EventData>>>();

			_hooks = new Dictionary<string, Dictionary<string, List<ScriptHook>>>();

			_creatureSpawns = new Dictionary<int, CreatureSpawn>();

			_scriptsToDispose = new List<IDisposable>();

			this.GlobalVars = new ScriptVariables();
		}

		public void Init()
		{
			this.GlobalVars.Perm = ChannelServer.Instance.Database.LoadVars("Aura System", 0);
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

			_scripts.Clear();
			_creatureSpawns.Clear();
			_questScripts.Clear();
			_hooks.Clear();
			_shops.Clear();
			_clientEventHandlers.Clear();

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
					foreach (var line in fr)
					{
						try
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
						catch (Exception ex1)
						{
							Log.Exception(ex1, string.Format("Problem in scripts list: '{0}'", line));
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

			// Init scripts
			this.InitializeScripts();

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
				var empty = (string.IsNullOrWhiteSpace(entry.OnUse) && string.IsNullOrWhiteSpace(entry.OnEquip) && string.IsNullOrWhiteSpace(entry.OnUnequip) && string.IsNullOrWhiteSpace(entry.OnCreation));
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
							if (!string.IsNullOrWhiteSpace(entry.OnCreation))
								sb.AppendFormat("	public override void OnCreation(Item i) {{ {0} }}" + Environment.NewLine, entry.OnCreation.Trim());
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
		public NpcShopScript GetShop(string typeName)
		{
			NpcShopScript result;
			_shops.TryGetValue(typeName, out result);
			return result;
		}

		/// <summary>
		/// Returns true if shop of type exists.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public bool ShopExists(string typeName)
		{
			return _shops.ContainsKey(typeName);
		}

		/// <summary>
		/// Adds shop.
		/// </summary>
		/// <param name="shop"></param>
		public void AddShop(NpcShopScript shop)
		{
			_shops[shop.GetType().Name] = shop;
		}

		/// <summary>
		/// Returns new AI script by name for creature, or null.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="creature"></param>
		/// <returns></returns>
		public AiScript GetAi(string name, Creature creature)
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
				try
				{
					File.Delete(outPath);
				}
				catch (UnauthorizedAccessException)
				{
					Log.Warning("Unable to delete '{0}'", outPath);
				}

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

		private static IEnumerable<Type> GetJITtedTypes(Assembly asm, string filePath)
		{
			Type[] types;
			try
			{
				types = asm.GetTypes();
				foreach (var method in types.SelectMany(t => t.GetMethods(BindingFlags.DeclaredOnly |
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)).Where(m => !m.IsAbstract))
				{
					RuntimeHelpers.PrepareMethod(method.MethodHandle);
				}
			}
			catch (Exception)
			{
				// Happens if classes in source or other scripts change,
				// i.e. a class name changes, or a parent class. Only
				// fixable by recaching, so they can "sync" again.

				Log.Error("GetJITtedTypes: Loading of one or multiple types in '{0}' failed, classes in this file won't be loaded. " +
				"Restart your server to recompile the offending scripts. Delete your cache folder if this error persists.", filePath);

				// Mark for recomp
				new FileInfo(filePath).LastWriteTime = DateTime.Now;

				return Enumerable.Empty<Type>();
			}

			return types;
		}

		/// <summary>
		/// Loads script classes inside assembly.
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="filePath">Path of the script, for reference</param>
		/// <returns></returns>
		private void LoadScriptAssembly(Assembly asm, string filePath)
		{
			var types = GetJITtedTypes(asm, filePath);

			foreach (var type in types.Where(a => a.GetInterfaces().Contains(typeof(IScript)) && !a.IsAbstract && !a.Name.StartsWith("_")))
			{
				try
				{
					// Make sure there's only one copy of each script.
					if (_scripts.ContainsKey(type.Name))
					{
						Log.Error("Script classes must have unique names, duplicate '{0}' found in '{1}'.", type.Name, Path.GetFileName(filePath));
						continue;
					}

					// Check overrides
					var overide = type.GetCustomAttribute<OverrideAttribute>();
					if (overide != null)
					{
						if (_scripts.ContainsKey(overide.TypeName))
						{
							_scripts.Remove(overide.TypeName);
						}
						else
							Log.Warning("Override: Script class '{0}' not found ({1} @ {2}).", overide.TypeName, type.Name, Path.GetFileName(filePath));
					}

					// Check removes
					var removes = type.GetCustomAttribute<RemoveAttribute>();
					if (removes != null)
					{
						foreach (var rm in removes.TypeNames)
						{
							if (_scripts.ContainsKey(rm))
							{
								_scripts.Remove(rm);
							}
							else
								Log.Warning("Remove: Script class '{0}' not found ({1} @ {2}).", rm, type.Name, Path.GetFileName(filePath));
						}
					}

					// Add class to load list, even if it's a dummy for remove,
					// we can't be sure it's not supposed to get initialized.
					_scripts[type.Name] = type;
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Error while loading script '{0}' ({1}).", type.Name, ex.Message);
				}
			}
		}

		/// <summary>
		/// Initializes all scripts loaded from assemblies.
		/// </summary>
		private void InitializeScripts()
		{
			foreach (var kvp in _scripts)
			{
				var type = kvp.Value;
				var path = kvp.Key;

				try
				{
					// Initiate script
					var script = Activator.CreateInstance(type) as IScript;
					if (!script.Init())
					{
						Log.Debug("LoadScriptAssembly: Failed to initiate '{0}'.", type.Name);

						continue;
					}

					// Register scripts implementing IDisposable as script to
					// dispose on reload.
					if (type.GetInterfaces().Contains(typeof(IDisposable)))
						_scriptsToDispose.Add(script as IDisposable);

					// Run auto loader. This has to be done after Init,
					// because of how Load is called from GeneralScript.
					if (type.GetInterfaces().Contains(typeof(IAutoLoader)))
						(script as IAutoLoader).AutoLoad();
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
		/// <param name="amount"></param>
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
		/// <param name="active"></param>
		/// <param name="effect"></param>
		/// <returns></returns>
		public Creature Spawn(int raceId, int regionId, int x, int y, int spawnId, bool active, bool effect)
		{
			// Create NPC
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

			// Set drops
			creature.Drops.GoldMin = creature.RaceData.GoldMin;
			creature.Drops.GoldMax = creature.RaceData.GoldMax;
			creature.Drops.Add(creature.RaceData.Drops);

			// Give skills
			foreach (var skill in creature.RaceData.Skills)
				creature.Skills.Add((SkillId)skill.SkillId, (SkillRank)skill.Rank, creature.Race);

			// Set AI
			if (!string.IsNullOrWhiteSpace(creature.RaceData.AI) && creature.RaceData.AI != "none")
			{
				creature.AI = this.GetAi(creature.RaceData.AI, creature);
				if (creature.AI == null)
					Log.Warning("Spawn: Missing AI '{0}' for '{1}'.", creature.RaceData.AI, raceId);
			}

			// Warp to spawn point
			if (!creature.Warp(regionId, x, y))
			{
				Log.Error("Failed to spawn '{0}'s.", raceId);
				return null;
			}

			creature.SpawnLocation = new Location(regionId, x, y);

			// Activate AI at least once
			if (creature.AI != null && active)
				creature.AI.Activate(0);

			// Spawn effect
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
			ChannelServer.Instance.Database.SaveVars("Aura System", 0, this.GlobalVars.Perm);
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
		/// Returns true if quest with the given id exists.
		/// </summary>
		/// <param name="questId"></param>
		/// <returns></returns>
		public bool QuestScriptExists(int questId)
		{
			return _questScripts.ContainsKey(questId);
		}

		/// <summary>
		/// Adds quest script.
		/// </summary>
		/// <param name="script"></param>
		public void AddQuestScript(QuestScript script)
		{
			_questScripts[script.Id] = script;
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

		/// <summary>
		/// Adds handler for client event.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="signal"></param>
		/// <param name="onTriggered"></param>
		public void AddClientEventHandler(long id, SignalType signal, Action<Creature, EventData> onTriggered)
		{
			Dictionary<SignalType, Action<Creature, EventData>> clientEvent;
			if (!_clientEventHandlers.TryGetValue(id, out clientEvent))
				_clientEventHandlers[id] = new Dictionary<SignalType, Action<Creature, EventData>>();

			_clientEventHandlers[id][signal] = onTriggered;
		}

		/// <summary>
		/// Returns handler for client event.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="signal"></param>
		public Action<Creature, EventData> GetClientEventHandler(long id, SignalType signal)
		{
			Dictionary<SignalType, Action<Creature, EventData>> clientEvent;
			if (!_clientEventHandlers.TryGetValue(id, out clientEvent))
				return null;

			Action<Creature, EventData> result;
			if (!clientEvent.TryGetValue(signal, out result))
				return null;

			return result;
		}
	}

	public delegate Task<HookResult> ScriptHook(NpcScript npc, params object[] args);
}
