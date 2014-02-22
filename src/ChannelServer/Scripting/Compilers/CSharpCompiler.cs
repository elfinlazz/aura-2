// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Reflection;
using Aura.Shared.Util;
using CSScriptLibrary;
using System.Text.RegularExpressions;
using System.Text;

namespace Aura.Channel.Scripting.Compilers
{
	public class CSharpCompiler : Compiler
	{
		public override Assembly Compile(string path, string outPath)
		{
			Assembly asm = null;
			try
			{
				if (this.ExistsAndUpToDate(path, outPath))
					return Assembly.LoadFrom(outPath);

				asm = CSScript.LoadCode(this.PreCompile(File.ReadAllText(path)));

				this.SaveAssembly(asm, outPath);
			}
			catch (csscript.CompilerException ex)
			{
				var errors = ex.Data["Errors"] as System.CodeDom.Compiler.CompilerErrorCollection;
				var newExs = new CompilerErrorsException();

				foreach (System.CodeDom.Compiler.CompilerError err in errors)
				{
					var newEx = new CompilerError(path, err.Line, err.Column, err.ErrorText, err.IsWarning);
					newExs.Errors.Add(newEx);
				}

				throw newExs;
			}
			catch (UnauthorizedAccessException)
			{
				// Thrown if file can't be copied. Happens if script was
				// initially loaded from cache.
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return asm;
		}

		public string PreCompile(string script)
		{
			// Default usings
			var defaultUsings = new StringBuilder();
			defaultUsings.Append("using System;");
			defaultUsings.Append("using System.Collections.Generic;");
			defaultUsings.Append("using System.Collections;");
			defaultUsings.Append("using System.Linq;");
			defaultUsings.Append("using System.Text;");
			defaultUsings.Append("using System.Threading.Tasks;");
			defaultUsings.Append("using System.Timers;");
			defaultUsings.Append("using Microsoft.CSharp;");
			defaultUsings.Append("using Aura.Channel.Network.Sending;");
			defaultUsings.Append("using Aura.Channel.Scripting.Scripts;");
			defaultUsings.Append("using Aura.Channel.Scripting;");
			defaultUsings.Append("using Aura.Channel.World.Entities;");
			defaultUsings.Append("using Aura.Channel.World.Shops;");
			defaultUsings.Append("using Aura.Channel.World;");
			defaultUsings.Append("using Aura.Channel;");
			defaultUsings.Append("using Aura.Shared.Mabi.Const;");
			defaultUsings.Append("using Aura.Shared.Mabi;");
			defaultUsings.Append("using Aura.Shared.Network;");
			defaultUsings.Append("using Aura.Shared.Util;");
			defaultUsings.Append("using Aura.Shared.Util.Commands;");
			script = defaultUsings + script;

			// Return();
			// --> yield break;
			// Stops Enumerator and the conversation.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?Return\s*\(\s*\)\s*;",
				"$1yield break;",
				RegexOptions.Compiled);

			// Do|Call(<method_call>);
			// --> foreach(var __callResult in <method_call>) yield return __callResult;
			// Loops through Enumerator returned by the method called and passes
			// the results to the main Enumerator.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?(Call|Do)\s*\(([^;]*)\)\s*;",
				"$1foreach(var __callResult in $3) yield return __callResult;",
				RegexOptions.Compiled);

			// duplicate <new_class> : <old_class> { <content_of_load> }
			// --> public class <new_class> : <old_class> { public override void OnLoad() { base.OnLoad(); <content_of_load> } }
			// Makes a new class, based on another one, calls the inherited
			// load first, and the new load afterwards.
			script = Regex.Replace(script,
			   @"duplicate +([^\s:]+) *: *([^\s{]+) *{ *([^}]+) *}",
			   "public class $1 : $2 { public override void Load() { base.Load(); $3 } }",
			   RegexOptions.Compiled);

			return script;
		}
	}
}
