// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
	public abstract class BaseScript
	{
		public virtual void Load()
		{
			Log.Debug("virtual load");
		}
	}
}