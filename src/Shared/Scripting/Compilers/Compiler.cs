// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.IO;
using System.Reflection;
using Aura.Shared.Util;
using System.Collections.Generic;

namespace Aura.Shared.Scripting.Compilers
{
	public abstract class Compiler
	{
		/// <summary>
		/// List of pre-compilers that the scripts go through.
		/// </summary>
		public List<IPreCompiler> PreCompilers { get; protected set; }

		/// <summary>
		/// Compiles script or loads it from outPath, if cache is true and
		/// the scrpt isn't newer.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="outPath"></param>
		/// <param name="cache"></param>
		/// <returns></returns>
		public abstract Assembly Compile(string path, string outPath, bool cache);

		/// <summary>
		/// Creates new compiler
		/// </summary>
		public Compiler()
		{
			this.PreCompilers = new List<IPreCompiler>();
		}

		/// <summary>
		/// Returns true if the out file exists and is newer than the script.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="outPath"></param>
		/// <returns></returns>
		protected bool ExistsAndUpToDate(string path, string outPath)
		{
			// Check existence of compiled assembly
			if (!File.Exists(outPath))
				return false;

			// Check if changes were made to script
			if (File.GetLastWriteTime(path) > File.GetLastWriteTime(outPath))
				return false;

			return true;
		}

		/// <summary>
		/// Saves assembly to outPath, overwrites existing file.
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="outPath"></param>
		protected void SaveAssembly(Assembly asm, string outPath)
		{
			var outRoot = Path.GetDirectoryName(outPath);

			if (File.Exists(outPath))
				File.Delete(outPath);
			else if (!Directory.Exists(outRoot))
				Directory.CreateDirectory(outRoot);

			File.Copy(asm.Location, outPath);
		}

		/// <summary>
		/// Runs script through all pre-compilers.
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>
		protected string PreCompile(string script)
		{
			if (this.PreCompilers.Count == 0)
				return script;

			foreach (var precompiler in this.PreCompilers)
				script = precompiler.PreCompile(script);

			return script;
		}
	}
}
