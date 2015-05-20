// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Reflection;
using Aura.Shared.Util;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Pipelines;

namespace Aura.Shared.Scripting.Compilers
{
	/// <summary>
	/// Boo compiler
	/// </summary>
	/// <remarks>
	/// Similar to Python.
	/// http://boo.codehaus.org/
	/// </remarks>
	public class BooCompiler : Compiler
	{
		public override Assembly Compile(string path, string outPath, bool cache)
		{
			Assembly asm = null;
			try
			{
				if (this.ExistsAndUpToDate(path, outPath) && cache)
					return Assembly.LoadFrom(outPath);

				// Precompile script to a temp file
				var precompiled = this.PreCompile(File.ReadAllText(path));
				var tmp = Path.GetTempFileName();
				File.WriteAllText(tmp, precompiled);

				// Compile
				var compiler = new Boo.Lang.Compiler.BooCompiler();
				compiler.Parameters.AddAssembly(typeof(Log).Assembly);
				compiler.Parameters.AddAssembly(Assembly.GetEntryAssembly());
				compiler.Parameters.Input.Add(new FileInput(tmp));
				compiler.Parameters.OutputAssembly = outPath;
				compiler.Parameters.Pipeline = new CompileToFile();

#if DEBUG
				compiler.Parameters.Debug = true;
#else
				compiler.Parameters.Debug = false;
#endif

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
