// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Shared.Scripting.Compilers
{
	public interface IPreCompiler
	{
		string PreCompile(string script);
	}
}
