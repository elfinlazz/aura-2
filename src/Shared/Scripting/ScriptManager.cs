// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Data;
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
	public class ScriptManager
	{
		protected Dictionary<string, Compiler> _compilers;

		protected Dictionary<string, Type> _scripts;

		protected List<IDisposable> _scriptsToDispose;

		/// <summary>
		/// Gets or sets whether caching is enabled (enabled by default).
		/// </summary>
		public bool Caching { get; set; }

		/// <summary>
		/// Creates new script manager.
		/// </summary>
		public ScriptManager()
		{
			_compilers = new Dictionary<string, Compiler>();
			_compilers.Add("cs", new CSharpCompiler());
			_compilers.Add("boo", new BooCompiler());
			_compilers.Add("vb", new VisualBasicCompiler());

			_scripts = new Dictionary<string, Type>();

			_scriptsToDispose = new List<IDisposable>();

			this.Caching = true;
		}

		/// <summary>
		/// Disposes all disposable scripts and clears internal script list.
		/// </summary>
		protected void DisposeScripts()
		{
			foreach (var script in _scriptsToDispose)
				script.Dispose();

			_scriptsToDispose.Clear();
			_scripts.Clear();
		}

		/// <summary>
		/// Loads scripts from list file.
		/// </summary>
		/// <param name="scriptListFile">Text file containing paths to script files.</param>
		public void LoadScripts(string scriptListFile)
		{
			if (!File.Exists(scriptListFile))
			{
				Log.Error("Script list not found at '{0}'.", scriptListFile);
				return;
			}

			Log.Info("Loading scripts, this might take a moment...");

			_scripts.Clear();

			// Read script list
			ICollection<string> toLoad = null;
			try
			{
				toLoad = this.ReadScriptList(scriptListFile);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Failed to read script list.");
				return;
			}

			// Load scripts
			var loaded = this.LoadScripts(toLoad);

			// Init scripts
			this.InitializeScripts();

			//if (toLoad.Count > 0)
			//	Log.WriteLine();

			Log.Info("  done loading {0} scripts (of {1}).", loaded, toLoad.Count);
		}

		/// <summary>
		/// Loads scripts from collection.
		/// </summary>
		/// <param name="scriptFileList">Collection of script file names.</param>
		/// <returns></returns>
		private int LoadScripts(ICollection<string> scriptFileList)
		{
			int done = 0, loaded = 0;
			foreach (string scriptPath in scriptFileList)
			{
				try
				{
					if (this.LoadScript(scriptPath))
						loaded++;
				}
				catch (FileNotFoundException)
				{
					Log.Warning("Script not found: {0}", scriptPath);
					continue;
				}

				if (done % 5 == 0)
					Log.Progress(done + 1, scriptFileList.Count);

				done++;
			}
			Log.Progress(100, 100);

			return loaded;
		}

		/// <summary>
		/// Loads script from given path.
		/// </summary>
		/// <param name="scriptPath"></param>
		/// <returns></returns>
		private bool LoadScript(string scriptPath)
		{
			if (!File.Exists(scriptPath))
				throw new FileNotFoundException();

			var asm = this.GetAssembly(scriptPath);
			if (asm == null)
				return false;

			this.LoadScriptAssembly(asm, scriptPath);

			return true;
		}

		/// <summary>
		/// Returns list of script files loaded from scripts.txt.
		/// </summary>
		/// <param name="rootList"></param>
		/// <returns></returns>
		protected virtual List<string> ReadScriptList(string scriptListFile)
		{
			var result = new List<string>();

			using (var fr = new FileReader(scriptListFile))
			{
				foreach (var line in fr)
				{
					var scriptPath = line.Value;

					// Path relative to cwd
					if (scriptPath.StartsWith("/"))
					{
						scriptPath = Path.Combine(Directory.GetCurrentDirectory().Replace("\\", "/"), scriptPath.Replace("\\", "/").TrimStart('/')).Replace("\\", "/");
					}
					// Path relative to list file
					else
					{
						// Get path to the current list's directory
						var listPath = Path.GetFullPath(line.File);
						listPath = Path.GetDirectoryName(listPath);

						// Get full path to script
						scriptPath = Path.Combine(listPath, scriptPath).Replace("\\", "/");
					}

					var paths = new List<string>();
					if (!scriptPath.EndsWith("/*"))
					{
						paths.Add(scriptPath);
					}
					else
					{
						var recursive = scriptPath.EndsWith("/**/*");
						var directoryPath = scriptPath.TrimEnd('/', '*');

						if (Directory.Exists(directoryPath))
						{
							foreach (var file in Directory.EnumerateFiles(directoryPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
							{
								// Ignore "hidden" files
								if (Path.GetFileName(file).StartsWith("."))
									continue;

								paths.Add(file.Replace("\\", "/"));
							}
						}
						else
						{
							Log.Warning("ReadScriptList: Directory not found: {0}", directoryPath);
						}
					}

					foreach (var path in paths)
					{
						if (!result.Contains(path))
							result.Add(path);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Loads assembly for script, compiles it if necessary.
		/// Returns null if there was a problem.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected Assembly GetAssembly(string path)
		{
			if (!File.Exists(path))
			{
				Log.Error("File '{0}' not found.", path);
				return null;
			}

			var ext = Path.GetExtension(path).TrimStart('.');

			// Load dlls directly
			if (ext == "dll")
				return Assembly.LoadFrom(path);

			// Try to compile other files
			var outPath = this.GetCachePath(path);

			Compiler compiler;
			_compilers.TryGetValue(ext, out compiler);
			if (compiler == null)
			{
				Log.Error("No compiler found for script '{0}'.", path);
				return null;
			}

			try
			{
				return compiler.Compile(path, outPath, this.Caching);
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

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// TODO: We might want to stop loading if this is a problem,
		/// scripts might depend on each other, which could lead to more
		/// errors.
		/// </remarks>
		/// <param name="asm"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		protected static IEnumerable<Type> GetJITtedTypes(Assembly asm, string filePath)
		{
			Type[] types;
			try
			{
				types = asm.GetTypes();
				foreach (var method in types.SelectMany(t => t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)).Where(m => !m.IsAbstract && !m.ContainsGenericParameters))
					RuntimeHelpers.PrepareMethod(method.MethodHandle);
			}
			catch (Exception ex)
			{
				// Happens if classes in source or other scripts change,
				// i.e. a class name changes, or a parent class. Only
				// fixable by recaching, so they can "sync" again.

				Log.Exception(ex, "GetJITtedTypes: Loading of one or multiple types in '{0}' failed, classes in this file won't be loaded. " +
					"Restart your server to recompile the offending scripts. Delete your cache folder if this error persists.", filePath);

				// Mark for recomp
				try
				{
					new FileInfo(filePath).LastWriteTime = DateTime.Now;
				}
				catch
				{
					// Fails for DLLs, because they're loaded from where they are.
				}

				return Enumerable.Empty<Type>();
			}

			return types;
		}

		/// <summary>
		/// Reads script classes from assembly and adds them to internal script list.
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="filePath">Path of the script, for reference</param>
		/// <returns></returns>
		private void LoadScriptAssembly(Assembly asm, string filePath)
		{
			var types = GetJITtedTypes(asm, filePath);

			foreach (var type in types.Where(a => a.GetInterfaces().Contains(typeof(IScript)) && !a.IsAbstract))
			{
				try
				{
					// Make sure there's only one copy of each script.
					if (_scripts.ContainsKey(type.Name))
					{
						Log.Error("Script classes must have unique names, duplicate '{0}' found in '{1}'.", type.Name, Path.GetFileName(filePath));
						continue;
					}

					// Remove overridden classes from list
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

					// Remove classes from list that are defined in remove attributes
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

					// Don't load script if feature is not enabled
					var enabled = type.GetCustomAttribute<IfEnabledAttribute>();
					if (enabled != null && !AuraData.FeaturesDb.IsEnabled(enabled.Feature))
						continue;

					// Don't load script if feature is enabled
					var notEnabled = type.GetCustomAttribute<IfNotEnabledAttribute>();
					if (notEnabled != null && AuraData.FeaturesDb.IsEnabled(notEnabled.Feature))
						continue;

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
			foreach (var type in _scripts.Values)
			{
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
		/// Returns path for the compiled version of the script.
		/// Creates directory structure if it doesn't exist.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected virtual string GetCachePath(string path)
		{
			return path + ".compiled";
		}
	}
}
