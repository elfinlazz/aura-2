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

public class BearAi : AiScript
{
	public BearAi()
	{
		//Hates("pc");
		SetAlertDelay(5000);
		SetAggroDelay(5000);
	}

	protected override IEnumerable Idle()
	{
		if (Random() < 50)
		{
			Do(Wander());
		}
		
		Do(Wait(5000, 10000));
	}

	protected override IEnumerable Alert()
	{
		Do(Say("I see you..."));
		Do(Wait(2000));
		Do(Circle(500, 20000));
		Do(Wait(5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(Say("attack!!"));
		Do(Wait(3000));
		//SwitchAction(Idle);
	}
}
