// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Reflection;
using Aura.Shared.Util;
using CSScriptLibrary;
using System.Text.RegularExpressions;

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
					var newEx = new CompilerError(path, err.Line, err.Column, err.ErrorText);
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
			// Return();
			// --> yield break;
			// Stops Enumerator and the conversation.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?Return\s*\(\s*\)\s*;",
				"$1yield break;",
				RegexOptions.Compiled);

			// Call(<method_call>);
			// --> foreach(var __callResult in <method_call>) yield return __callResult;
			// Loops through Enumerator returned by the method called and passes
			// the results to the main Enumerator.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?Call\s*\(([^;]*)\)\s*;",
				"$1foreach(var __callResult in $2) yield return __callResult;",
				RegexOptions.Compiled);

			// Select(<creature>);
			// --> yield return null;
			// Calls Select and yields, ignoring the result.
			// TODO: Imperfect (;)
			script = Regex.Replace(script,
				@"([^=])([\s]+)Select\s*\(\s*([^)]*)\s*\)\s*;",
				"$1$2Select($3); yield return null;",
				RegexOptions.Compiled);

			// [var] <variable> = Select(<creature>);
			// --> [var] <variable>Object = new Response(); yield return <variable>Object; [var] <variable> = <variable>Object.Value;
			// Calls Select and yields. Afterwards the result from the client
			// is written into the variable.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?(var )?[\t ]*([^\s\)]*)\s*=\s*([^\.]\.)?Select\s*\(\s*([^)]*)\s*\)\s*;",
				"$1$4Select($5); $2$3Object = new Response(); yield return $3Object; $2$3 = $3Object.Value;",
				RegexOptions.Compiled);

			return script;
		}
	}
}
