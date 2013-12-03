// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Reflection;
using Aura.Shared.Util;
using CSScriptLibrary;

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

				asm = CSScript.LoadCode(File.ReadAllText(path));

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
	}
}
