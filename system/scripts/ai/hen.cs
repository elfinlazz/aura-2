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

public class HenAi : AiScript
{
	protected override void Idle()
	{
		if (Random() < 50)
		{
			Say("Cluck, cluck");
		}
		else
		{
			Wander();
		}
		
		Wait(5000, 10000);
	}
}
