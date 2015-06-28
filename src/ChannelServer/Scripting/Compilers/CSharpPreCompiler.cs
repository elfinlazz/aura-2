// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Scripting.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting.Compilers
{
	/// <summary>
	/// Pre-Compiler for the channel's C# scripts.
	/// </summary>
	public class CSharpPreCompiler : IPreCompiler
	{
		public string PreCompile(string script)
		{
			var add = new StringBuilder();

			// Default usings
			add.Append("using System;");
			add.Append("using System.Collections.Generic;");
			add.Append("using System.Collections;");
			add.Append("using System.Linq;");
			add.Append("using System.Text;");
			add.Append("using System.Threading.Tasks;");
			add.Append("using System.Timers;");
			add.Append("using Microsoft.CSharp;");
			add.Append("using Aura.Channel.Network;");
			add.Append("using Aura.Channel.Network.Sending;");
			add.Append("using Aura.Channel.Scripting.Scripts;");
			add.Append("using Aura.Channel.Scripting;");
			add.Append("using Aura.Channel.Util;");
			add.Append("using Aura.Channel.World.Dungeons;");
			add.Append("using Aura.Channel.World.Dungeons.Props;");
			add.Append("using Aura.Channel.World.Dungeons.Puzzles;");
			add.Append("using Aura.Channel.World.Entities;");
			add.Append("using Aura.Channel.World;");
			add.Append("using Aura.Channel.World.Quests;");
			add.Append("using Aura.Channel;");
			add.Append("using Aura.Data;");
			add.Append("using Aura.Data.Database;");
			add.Append("using Aura.Mabi.Const;");
			add.Append("using Aura.Mabi;");
			add.Append("using Aura.Shared.Network;");
			add.Append("using Aura.Shared.Util;");
			add.Append("using Aura.Shared.Util.Commands;");
			add.Append("using Aura.Shared.Scripting;");
			script = add + script;

			// Return();
			// --> yield break;
			// Stops Enumerator and the conversation.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?Return\s*\(\s*\)\s*;",
				"$1yield break;"
			);

			// Do(<method_call>);
			// --> foreach(var __callResult in <method_call>) yield return __callResult;
			// Loops through Enumerator returned by the method called and passes
			// the results to the main Enumerator.
			script = Regex.Replace(script,
				@"([\{\}:;\t ])?(Do)\s*\(([^;]*)\)\s*;",
				"$1foreach(var __callResult in $3) yield return __callResult;"
			);

			// duplicate <new_class> : <old_class> { <content_of_load> }
			// --> public class <new_class> : <old_class> { public override void OnLoad() { base.OnLoad(); <content_of_load> } }
			// Makes a new class, based on another one, calls the inherited
			// load first, and the new load afterwards.
			script = Regex.Replace(script,
			   @"duplicate +([^\s:]+) *: *([^\s{]+) *{ *([^}]+) *}",
			   "public class $1 : $2 { public override void Load() { base.Load(); $3 } }"
			);

			return script;
		}
	}
}
