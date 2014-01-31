// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Reflection;
using Aura.Shared.Util;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;

namespace Aura.Channel.Scripting.Compilers
{
	public class BooCompiler : Compiler
	{
		public override Assembly Compile(string path, string outPath)
		{
			Assembly asm = null;
			try
			{
				if (this.ExistsAndUpToDate(path, outPath))
					return Assembly.LoadFrom(outPath);

				var compiler = new Boo.Lang.Compiler.BooCompiler();
				compiler.Parameters.AddAssembly(typeof(Log).Assembly);
				compiler.Parameters.AddAssembly(typeof(ScriptManager).Assembly);
				compiler.Parameters.Input.Add(new FileInput(path));
				compiler.Parameters.OutputAssembly = outPath;
				compiler.Parameters.Pipeline = new CompileToFile();

				var context = compiler.Run();
				if (context.GeneratedAssembly == null)
				{
					var errors = context.Errors;
					var newExs = new CompilerErrorsException();

					foreach (var err in errors)
					{
						var newEx = new CompilerError(path, err.LexicalInfo.Line, err.LexicalInfo.Column, err.Message, false);
						newExs.Errors.Add(newEx);
					}

					throw newExs;
				}

				asm = context.GeneratedAssembly;

				//this.SaveAssembly(asm, outPath);
			}
			catch (CompilerErrorsException)
			{
				throw;
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
