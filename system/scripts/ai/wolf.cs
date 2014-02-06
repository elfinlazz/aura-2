using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Mabi;
using Aura.Shared.Util;

public class WolfAi : AiScript
{
	protected override IEnumerable Idle()
	{
		if (Random() < 50)
		{
			Call(Wander());
		}
		
		Call(Wait(5000, 10000));
	}
}
