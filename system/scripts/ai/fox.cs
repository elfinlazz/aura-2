//--- Aura Script -----------------------------------------------------------
//  Fox AI
//--- Description -----------------------------------------------------------
//  AI for foxes and fox-like creatures.
//---------------------------------------------------------------------------

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

public class FoxAi : AiScript
{
	public FoxAi()
	{
		SetAggroType(AggroType.Careful);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(5000, 10000));
	}
	
	protected override IEnumerable Alert()
	{
		if(Random() < 50)
			Do(Circle(400, 2000, 8000));
		
		Do(Wait(3000, 6000));
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Say("attack"));
		Do(Wait(3000, 6000));
	}
}
